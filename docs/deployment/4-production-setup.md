# 4. Configuração Inicial do Servidor — L2SLedger

> Procedimento **one-time** para preparar a VM OCI para o primeiro deploy.  
> Execute apenas uma vez ao provisionar um novo servidor.

---

## Pré-requisitos

- VM provisionada (Ubuntu 22.04 LTS ou superior)
- Acesso SSH com sudo
- IP público configurado
- Domínio apontando para o IP da VM (para TLS automático)

---

## 1. Conectar e Atualizar Sistema

```bash
ssh user@VM_IP

# Atualizar pacotes
sudo apt update && sudo apt upgrade -y
```

---

## 2. Instalar Docker

```bash
# Remover versões antigas (se existirem)
sudo apt remove -y docker docker-engine docker.io containerd runc 2>/dev/null

# Instalar dependências
sudo apt install -y ca-certificates curl gnupg lsb-release

# Adicionar GPG key oficial do Docker
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

# Adicionar repositório Docker
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Instalar Docker Engine + Compose Plugin
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

# Adicionar usuário ao grupo docker (evita sudo)
sudo usermod -aG docker $USER

# Aplicar grupo sem logout (ou faça logout/login)
newgrp docker

# Verificar instalação
docker --version
docker compose version
```

---

## 3. Instalar Caddy

```bash
sudo apt install -y debian-keyring debian-archive-keyring apt-transport-https

curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' | \
  sudo gpg --dearmor -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg

curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/debian.deb.txt' | \
  sudo tee /etc/apt/sources.list.d/caddy-stable.list

sudo apt update
sudo apt install -y caddy

# Verificar
caddy version
systemctl status caddy
```

---

## 4. Criar Estrutura de Diretórios

```bash
sudo mkdir -p /opt/l2sledger/secrets
sudo chown -R $USER:$USER /opt/l2sledger
chmod 700 /opt/l2sledger/secrets
```

---

## 5. Criar Redes Docker

```bash
docker network create caddy-network
docker network create shared-db-network
```

---

## 6. Autenticar no GHCR

```bash
# Substitua YOUR_GITHUB_USERNAME e YOUR_TOKEN
echo YOUR_TOKEN | docker login ghcr.io -u YOUR_GITHUB_USERNAME --password-stdin
```

> O token precisa do escopo `read:packages`. Ver [1-prerequisites.md](1-prerequisites.md#autenticação).

---

## 7. Configurar `.env` de Produção

```bash
cd /opt/l2sledger
nano .env
```

Conteúdo (substituir com valores reais):

```env
# Application
ASPNETCORE_ENVIRONMENT=Production
NODE_ENV=production

# PostgreSQL
POSTGRES_HOST=postgres-container-or-ip
POSTGRES_PORT=5432
POSTGRES_USER=l2sledger_prod
POSTGRES_PASSWORD=STRONG_RANDOM_PASSWORD

# JWT
JWT_SECRET=STRONG_RANDOM_SECRET_MIN_64_CHARS_GENERATED_BY_OPENSSL
JWT_ISSUER=https://yourdomain.com
JWT_AUDIENCE=l2sledger

# Firebase
FIREBASE_CREDENTIAL_PATH=/opt/l2sledger/secrets/firebase-credential.json
FIREBASE_WEB_API_KEY=actual-prod-api-key
VITE_FIREBASE_API_KEY=actual-prod-api-key
VITE_FIREBASE_AUTH_DOMAIN=prod-project.firebaseapp.com
VITE_FIREBASE_PROJECT_ID=prod-project-id
VITE_FIREBASE_STORAGE_BUCKET=prod-project.appspot.com
VITE_FIREBASE_MESSAGING_SENDER_ID=123456789
VITE_FIREBASE_APP_ID=1:123456789:web:abc

# CORS
CORS_ALLOWED_ORIGINS=https://yourdomain.com

# Frontend
VITE_API_BASE_URL=https://yourdomain.com/api
VITE_ENABLE_DEVTOOLS=false

# Docker / Deploy
GHCR_OWNER=your-github-org
IMAGE_TAG=latest
```

Proteger o arquivo:

```bash
chmod 600 /opt/l2sledger/.env
```

> Referência completa das variáveis: [6-environment-variables.md](6-environment-variables.md)

---

## 8. Upload do Firebase Credential

A partir da sua máquina local:

```bash
scp firebase-credential.json user@VM_IP:/opt/l2sledger/secrets/
```

Na VM, ajustar permissões:

```bash
chmod 600 /opt/l2sledger/secrets/firebase-credential.json
```

---

## 9. Copiar `docker-compose.prod.yml`

A partir da sua máquina local:

```bash
scp docker-compose.prod.yml user@VM_IP:/opt/l2sledger/
```

---

## 10. Configurar Caddy

Edite o Caddyfile:

```bash
sudo nano /etc/caddy/Caddyfile
```

Conteúdo:

```caddyfile
yourdomain.com {
    # Logs
    log {
        output file /var/log/caddy/access.log {
            roll_size 100mb
            roll_keep 5
        }
        level INFO
    }

    # Backend API
    handle /api/* {
        reverse_proxy l2sledger-backend:8080 {
            header_up X-Real-IP {remote_host}
            header_up X-Forwarded-For {remote_host}
            header_up X-Forwarded-Proto {scheme}
        }
    }

    # Frontend SPA
    handle {
        reverse_proxy l2sledger-frontend:3000
    }

    # Security Headers
    header {
        X-Content-Type-Options "nosniff"
        X-Frame-Options "DENY"
        X-XSS-Protection "1; mode=block"
        Referrer-Policy "strict-origin-when-cross-origin"
    }
}
```

Validar e reload:

```bash
sudo caddy validate --config /etc/caddy/Caddyfile
sudo systemctl reload caddy
```

> Configuração detalhada do Caddy: [7-caddy-configuration.md](7-caddy-configuration.md)

---

## 11. Configurar Segurança

### Firewall (UFW)

```bash
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
sudo ufw status
```

### SSH Hardening

```bash
sudo nano /etc/ssh/sshd_config
```

Configurações recomendadas:

```
PermitRootLogin no
PasswordAuthentication no
PubkeyAuthentication yes
```

```bash
sudo systemctl restart sshd
```

### Fail2Ban

```bash
sudo apt install -y fail2ban
sudo systemctl enable fail2ban
sudo systemctl start fail2ban
```

---

## 12. Conectar Caddy à Rede Docker

Se Caddy roda como **serviço systemd** (padrão), ele precisa resolver nomes dos containers Docker. O Caddy acessa os containers via a rede `caddy-network`, que deve estar configurada como `external: true` no compose.

Para Caddy resolver nomes de containers, existem duas opções:

### Opção A: Caddy como Systemd + Docker DNS (Padrão)

O Caddy usa o resolvedor DNS do Docker. Após os containers estarem rodando na `caddy-network`, o Caddy consegue acessá-los pelos nomes definidos em `container_name`.

> **Nota**: Se Caddy não roda como container, ele utiliza o IP do host Docker para acessar os containers. Nesse caso, pode ser necessário usar `host.docker.internal` ou os IPs internos da rede Docker.

### Opção B: Caddy como Container

Se preferir rodar Caddy também como container:

```bash
docker network connect caddy-network caddy-container-name
```

---

## 13. Verificação Final

Execute todos os checks:

```bash
# Docker rodando
docker ps

# Redes criadas
docker network ls | grep -E 'caddy-network|shared-db-network'

# Caddy rodando
systemctl status caddy

# Diretórios existem
ls -la /opt/l2sledger/
ls -la /opt/l2sledger/secrets/

# .env configurado
test -f /opt/l2sledger/.env && echo "OK: .env exists" || echo "FAIL: .env missing"

# Firebase credential presente
test -f /opt/l2sledger/secrets/firebase-credential.json && echo "OK: credential exists" || echo "FAIL: credential missing"

# Firewall ativo
sudo ufw status
```

### Checklist

```markdown
- [ ] Ubuntu atualizado
- [ ] Docker Engine instalado e rodando
- [ ] Docker Compose Plugin instalado
- [ ] Caddy instalado e rodando
- [ ] Diretórios criados (/opt/l2sledger, /opt/l2sledger/secrets)
- [ ] Redes Docker criadas (caddy-network, shared-db-network)
- [ ] GHCR autenticado
- [ ] .env configurado com valores de produção
- [ ] Firebase credential uploaded
- [ ] docker-compose.prod.yml copiado
- [ ] Caddyfile configurado e validado
- [ ] Firewall UFW ativado (22, 80, 443)
- [ ] SSH hardened (no root, no password)
- [ ] Fail2Ban instalado e rodando
```

---

## ➡️ Próximos Passos

- Pronto para o primeiro deploy → [5-production-deploy.md](5-production-deploy.md)
- Configuração avançada do Caddy → [7-caddy-configuration.md](7-caddy-configuration.md)
