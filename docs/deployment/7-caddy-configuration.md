# 7. Configuração do Caddy — L2SLedger

> Caddy funciona como reverse proxy na VM de produção, gerenciando TLS, roteamento e headers de segurança.

---

## 1. Conceito

O Caddy desempenha três funções principais:

| Função | Descrição |
|--------|-----------|
| **Reverse Proxy** | Roteia requests externos para os containers internos |
| **TLS Termination** | Gerencia certificados SSL/TLS automaticamente via Let's Encrypt |
| **Security Headers** | Adiciona headers de segurança a todas as respostas |

---

## 2. Arquitetura de Rede

```
Internet
    │
    ▼
Caddy :80/:443   ← TLS termination + security headers
    │
    ▼
Docker Network (caddy-network)
    ├──► l2sledger-backend:8080    ← /api/*
    └──► l2sledger-frontend:3000   ← /* (catch-all)
```

**Importante**:
- Containers **NÃO** expõem portas diretamente (`expose:` em vez de `ports:`)
- Caddy acessa os containers via DNS interno do Docker na `caddy-network`
- Todo tráfego externo passa pelo Caddy

---

## 3. Caddyfile

**Localização**: `/etc/caddy/Caddyfile`

### Configuração Completa

```caddyfile
# ══════════════════════════════════════════════
# L2SLedger — Production Reverse Proxy
# ══════════════════════════════════════════════

yourdomain.com {
    # ── Logs ──────────────────────────────────
    log {
        output file /var/log/caddy/access.log {
            roll_size 100mb
            roll_keep 5
        }
        level INFO
    }

    # ── Backend API (/api/*) ──────────────────
    handle /api/* {
        reverse_proxy l2sledger-backend:8080 {
            header_up X-Real-IP {remote_host}
            header_up X-Forwarded-For {remote_host}
            header_up X-Forwarded-Proto {scheme}
        }
    }

    # ── Frontend SPA (catch-all) ──────────────
    handle {
        reverse_proxy l2sledger-frontend:3000
    }

    # ── Security Headers ─────────────────────
    header {
        # XSS Protection
        X-Content-Type-Options "nosniff"
        X-Frame-Options "DENY"
        X-XSS-Protection "1; mode=block"

        # Referrer Policy
        Referrer-Policy "strict-origin-when-cross-origin"

        # HSTS — habilitar após confirmar que SSL está funcionando
        # Strict-Transport-Security "max-age=31536000; includeSubDomains; preload"

        # CSP — ajustar conforme necessário
        # Content-Security-Policy "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:;"
    }
}
```

---

## 4. Ordem dos Handles

A **ordem** dos blocos `handle` é crítica. Blocos mais específicos devem vir **antes** de blocos genéricos.

```caddyfile
# ✅ CORRETO — específico antes de genérico
handle /api/* {
    reverse_proxy l2sledger-backend:8080
}
handle {
    reverse_proxy l2sledger-frontend:3000
}
```

```caddyfile
# ❌ INCORRETO — catch-all captura tudo primeiro
handle {
    reverse_proxy l2sledger-frontend:3000
}
handle /api/* {
    reverse_proxy l2sledger-backend:8080  # nunca será alcançado!
}
```

---

## 5. TLS Automático

Caddy provisiona certificados automaticamente via Let's Encrypt.

### Requisitos

- Domínio válido com DNS apontando para o IP da VM
- Portas 80 e 443 abertas no firewall
- Caddy rodando com permissão de bind em portas privilegiadas

### Verificar Certificado

```bash
# Verificar certificados gerenciados pelo Caddy
sudo caddy list-certificates

# Verificar validade do certificado
echo | openssl s_client -showcerts -servername yourdomain.com \
  -connect yourdomain.com:443 2>/dev/null | openssl x509 -noout -dates
```

### Habilitar HSTS (Após SSL Confirmado)

Após confirmar que o TLS está funcionando corretamente, descomente a linha HSTS no Caddyfile:

```caddyfile
header {
    # ... outros headers ...
    Strict-Transport-Security "max-age=31536000; includeSubDomains; preload"
}
```

---

## 6. Comandos Úteis

### Validar Configuração

```bash
sudo caddy validate --config /etc/caddy/Caddyfile
```

### Reload (Sem Downtime)

```bash
sudo caddy reload --config /etc/caddy/Caddyfile
# ou
sudo systemctl reload caddy
```

### Restart

```bash
sudo systemctl restart caddy
```

### Ver Status

```bash
systemctl status caddy
```

### Ver Logs

```bash
# Logs do systemd
sudo journalctl -u caddy -f

# Logs de acesso (se configurado)
tail -f /var/log/caddy/access.log
```

### Testar Roteamento

```bash
# Backend (via Caddy)
curl -H "Host: yourdomain.com" http://localhost/api/v1/health

# Frontend (via Caddy)
curl -H "Host: yourdomain.com" http://localhost/

# HTTPS (externo)
curl https://yourdomain.com/api/v1/health
curl https://yourdomain.com/
```

---

## 7. Conectividade com Docker

### Caddy como Serviço Systemd (Padrão)

Quando Caddy roda como serviço systemd (instalação via apt), ele utiliza o DNS do Docker para resolver nomes de containers. Para isso funcionar:

1. Os containers devem estar na rede `caddy-network`
2. A rede deve ser `external: true` no compose (já está)
3. O Caddy precisa conseguir acessar a interface Docker

### Caddy como Container (Alternativa)

Se Caddy rodar como container:

```bash
# Conectar caddy à rede
docker network connect caddy-network caddy-container-name
```

### Verificar Conectividade

```bash
# Verificar se containers estão na rede caddy-network
docker network inspect caddy-network | grep -A 5 "Containers"

# Testar acesso interno (de dentro de um container na mesma rede)
docker run --rm --network caddy-network alpine wget -qO- http://l2sledger-backend:8080/api/v1/health
```

---

## 8. Segurança Adicional

### Content Security Policy (CSP)

Ajuste conforme as necessidades do frontend:

```caddyfile
header {
    Content-Security-Policy "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self' https://*.googleapis.com https://*.firebaseio.com"
}
```

> **NOTA**: O frontend usa Firebase SDK, que requer acesso a domínios do Google. Ajuste `connect-src` conforme necessário.

### Rate Limiting (Futuro)

```caddyfile
# Requer módulo rate_limit
rate_limit {
    zone dynamic {
        key {remote_host}
        events 100
        window 1m
    }
}
```

### IP Whitelist para Rotas Admin (Futuro)

```caddyfile
@admin {
    path /api/admin/*
}
handle @admin {
    @blocked not remote_ip 10.0.0.0/8 192.168.0.0/16
    abort @blocked
    reverse_proxy l2sledger-backend:8080
}
```

---

## 9. Troubleshooting

### Erro: Cannot bind to port 80/443

```bash
# Verificar se outra coisa usa a porta
sudo lsof -i :80
sudo lsof -i :443

# Parar serviço conflitante (ex: apache2, nginx)
sudo systemctl stop apache2 2>/dev/null
sudo systemctl stop nginx 2>/dev/null

# Garantir que Caddy tem permissão
sudo setcap cap_net_bind_service=+ep $(which caddy)
```

### Erro: 502 Bad Gateway

```bash
# Container não está rodando ou não está na rede
docker ps | grep l2sledger
docker network inspect caddy-network

# Testar acesso direto ao container
docker exec l2sledger-backend wget -qO- http://localhost:8080/api/v1/health
```

### Erro: Certificate obtaining failed

```bash
# Verificar DNS resolve para o IP da VM
dig yourdomain.com

# Verificar portas abertas
sudo ufw status
curl http://yourdomain.com  # deve responder (redirect para HTTPS)

# Ver logs detalhados
sudo journalctl -u caddy -n 100
```

### Erro: Caddy não resolve nomes dos containers

```bash
# Verificar se containers estão na rede
docker network inspect caddy-network

# Reiniciar Caddy após containers estarem up
sudo systemctl restart caddy
```

---

## 10. Manutenção

### Atualizar Caddy

```bash
sudo apt update
sudo apt upgrade caddy
caddy version
```

### Rotação de Logs

Os logs do Caddy são rotacionados automaticamente conforme configurado no Caddyfile (`roll_size 100mb`, `roll_keep 5`).

Para verificar tamanho dos logs:

```bash
du -sh /var/log/caddy/
```

### Backup do Caddyfile

```bash
sudo cp /etc/caddy/Caddyfile /etc/caddy/Caddyfile.backup.$(date +%Y%m%d)
```

---

## 📖 Referências

- [Caddy Documentation](https://caddyserver.com/docs/)
- [DevOps Strategy](../devops-strategy.md) — Decisões sobre Caddy e integração
- [4-production-setup.md](4-production-setup.md) — Instalação do Caddy
