# 9. Troubleshooting — L2SLedger

> Guia de resolução de problemas comuns, organizado por sintoma → diagnóstico → solução.

---

## Problema 1: Container Não Inicia

### Sintomas

- `docker ps` não mostra o container
- Container aparece e desaparece imediatamente
- Status `Exited (1)` ou similar

### Causas Possíveis

1. Erro na aplicação durante startup
2. Variável de ambiente faltando ou inválida
3. Arquivo de configuração ausente (ex: `firebase-credential.json`)
4. Porta já em uso

### Diagnóstico

```bash
# Ver logs do container (mesmo se parado)
docker logs l2sledger-backend

# Inspecionar configuração
docker inspect l2sledger-backend | jq '.[0].State'

# Verificar portas em uso
sudo lsof -i :8080
sudo lsof -i :3000
```

### Solução

```bash
# 1. Verificar e corrigir variáveis no .env
nano /opt/l2sledger/.env

# 2. Verificar se firebase-credential.json existe
ls -l /opt/l2sledger/secrets/firebase-credential.json

# 3. Liberar porta se ocupada
docker stop <container-using-port>

# 4. Tentar iniciar novamente
docker compose -f docker-compose.prod.yml up -d backend

# 5. Monitorar logs durante startup
docker logs -f l2sledger-backend
```

---

## Problema 2: Health Check Falhando

### Sintomas

- Docker mostra `(unhealthy)` no status
- Deploy falha na verificação de saúde
- Aplicação parece rodar mas health endpoint não responde

### Causas Possíveis

1. Aplicação não terminou de inicializar
2. Health endpoint retornando erro
3. Timeout muito curto
4. Problema de conectividade interna

### Diagnóstico

```bash
# Tentar acessar health endpoint manualmente
docker exec l2sledger-backend wget -qO- http://localhost:8080/api/v1/health

# Ver detalhes e histórico do health check
docker inspect l2sledger-backend | jq '.[0].State.Health'

# Verificar logs da aplicação
docker logs l2sledger-backend | grep -i "health\|error\|exception"
```

### Solução

```bash
# 1. Dar mais tempo para startup (aguardar 30s-1min)
sleep 60
docker ps

# 2. Se persistir, verificar logs de erro
docker logs l2sledger-backend --tail 50

# 3. Verificar conectividade com database
docker exec l2sledger-backend wget -qO- http://localhost:8080/api/v1/health

# 4. Restart forçado
docker compose -f docker-compose.prod.yml restart backend

# 5. Se não resolver, recriar container
docker compose -f docker-compose.prod.yml up -d --force-recreate backend
```

---

## Problema 3: Erro de Autenticação Firebase

### Sintomas

- Login retorna erro 500 ou 401
- Logs mostram "Firebase authentication failed"
- "Could not load Firebase credential"

### Causas Possíveis

1. `firebase-credential.json` não existe ou caminho incorreto
2. Permissões do arquivo incorretas
3. JSON malformado ou corrompido
4. Service Account desabilitado no Firebase Console

### Diagnóstico

```bash
# Verificar se arquivo existe
ls -l /opt/l2sledger/secrets/firebase-credential.json

# Verificar permissões
stat /opt/l2sledger/secrets/firebase-credential.json

# Verificar se é JSON válido
cat /opt/l2sledger/secrets/firebase-credential.json | jq .

# Ver variável de ambiente no container
docker exec l2sledger-backend env | grep FIREBASE

# Verificar logs de auth
docker logs l2sledger-backend 2>&1 | grep -i "firebase\|auth\|credential"
```

### Solução

```bash
# 1. Re-download do Service Account
#    Firebase Console → Project Settings → Service Accounts → Generate New Key

# 2. Re-upload para o servidor
scp firebase-credential.json user@VM_IP:/opt/l2sledger/secrets/

# 3. Corrigir permissões
chmod 600 /opt/l2sledger/secrets/firebase-credential.json
chown $USER:$USER /opt/l2sledger/secrets/firebase-credential.json

# 4. Verificar path no docker-compose.prod.yml / .env
grep FIREBASE_CREDENTIAL_PATH /opt/l2sledger/.env

# 5. Restart do backend
docker compose -f docker-compose.prod.yml restart backend
```

---

## Problema 4: Frontend Não Conecta ao Backend

### Sintomas

- Frontend carrega mas chamadas à API falham
- Console do navegador mostra erros de CORS
- Network tab mostra 404, timeout ou `ERR_CONNECTION_REFUSED`

### Causas Possíveis

1. `VITE_API_BASE_URL` incorreto
2. CORS não configurado corretamente no backend
3. Backend não está acessível
4. Caddy não está roteando corretamente

### Diagnóstico

```bash
# 1. Verificar variável no frontend (runtime injection)
docker exec l2sledger-frontend cat /app/dist/env-config.js

# 2. Testar backend diretamente
curl http://localhost:8080/api/v1/health

# 3. Testar via Caddy
curl https://yourdomain.com/api/v1/health

# 4. Ver logs do Caddy
sudo journalctl -u caddy -f

# 5. Verificar CORS no backend
docker logs l2sledger-backend 2>&1 | grep -i cors
docker exec l2sledger-backend env | grep CORS
```

### Solução

```bash
# 1. Corrigir VITE_API_BASE_URL no .env
# Produção: https://yourdomain.com/api  (sem /v1)
# Local: http://localhost:8080/api
nano /opt/l2sledger/.env

# 2. Corrigir CORS_ALLOWED_ORIGINS
# Deve incluir o domínio do frontend
CORS_ALLOWED_ORIGINS=https://yourdomain.com

# 3. Restart frontend e backend
docker compose -f docker-compose.prod.yml restart

# 4. Verificar se env-config.js foi regenerado
docker exec l2sledger-frontend cat /app/dist/env-config.js
```

---

## Problema 5: Caddy Não Roteia Tráfego

### Sintomas

- Site inacessível externamente
- HTTPS não funciona
- 502 Bad Gateway
- Containers estão healthy mas site inacessível

### Causas Possíveis

1. Caddy não está rodando
2. Containers não estão na rede `caddy-network`
3. Caddyfile mal configurado
4. Firewall bloqueando portas
5. DNS não aponta para o IP da VM

### Diagnóstico

```bash
# 1. Verificar Caddy
systemctl status caddy

# 2. Validar configuração
sudo caddy validate --config /etc/caddy/Caddyfile

# 3. Verificar portas
sudo lsof -i :80
sudo lsof -i :443

# 4. Verificar redes Docker
docker network inspect caddy-network | jq '.[0].Containers'

# 5. Verificar logs
sudo journalctl -u caddy -n 50

# 6. Verificar DNS
dig yourdomain.com
```

### Solução

```bash
# 1. Restart Caddy
sudo systemctl restart caddy

# 2. Conectar containers à rede (se não estiverem)
docker network connect caddy-network l2sledger-backend
docker network connect caddy-network l2sledger-frontend

# 3. Validar e recarregar Caddyfile
sudo caddy validate --config /etc/caddy/Caddyfile
sudo caddy reload --config /etc/caddy/Caddyfile

# 4. Verificar firewall
sudo ufw status
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
```

> Mais detalhes: [7-caddy-configuration.md](7-caddy-configuration.md#9-troubleshooting)

---

## Problema 6: Database Connection Failed

### Sintomas

- Backend não inicia ou fica em crash loop
- Logs mostram "Could not connect to database"
- Migrations falham

### Causas Possíveis

1. PostgreSQL não está rodando
2. Connection string incorreta
3. Rede `shared-db-network` não configurada
4. Credenciais inválidas

### Diagnóstico

```bash
# 1. Verificar PostgreSQL
docker ps | grep postgres

# 2. Testar conexão
docker exec -it postgres psql -U l2sledger -d l2sledger -c "SELECT 1;"

# 3. Verificar connection string no backend
docker exec l2sledger-backend env | grep ConnectionStrings

# 4. Verificar rede
docker network inspect shared-db-network
```

### Solução

```bash
# 1. Iniciar PostgreSQL (se não estiver rodando)
docker start postgres-container-name

# 2. Corrigir connection string no .env
nano /opt/l2sledger/.env
# ConnectionStrings__DefaultConnection=Host=CORRECT_HOST;Port=5432;Database=l2sledger;Username=USER;Password=PASS

# 3. Garantir que backend está na rede correta
docker network connect shared-db-network l2sledger-backend

# 4. Restart backend
docker compose -f docker-compose.prod.yml restart backend
```

---

## Problema 7: Image Pull Failed

### Sintomas

- Deploy falha com "image not found"
- "authentication required"
- "manifest unknown"

### Causas Possíveis

1. Não autenticado no GHCR
2. Tag não existe
3. Repository privado sem permissão

### Diagnóstico

```bash
# 1. Verificar autenticação
cat ~/.docker/config.json | jq '.auths'

# 2. Testar pull manual
docker pull ghcr.io/your-org/l2sledger-backend:v1.2.3

# 3. Verificar tags disponíveis (via GitHub UI ou API)
```

### Solução

```bash
# 1. Re-autenticar no GHCR
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin

# 2. Verificar tag correto
# GitHub → Packages → l2sledger-backend → ver tags disponíveis

# 3. Usar tag correto
export IMAGE_TAG=correct-tag
docker compose -f docker-compose.prod.yml pull
```

---

## Problema 8: Disco Cheio

### Sintomas

- Containers falham ao iniciar
- Logs param de ser escritos
- Erros "no space left on device"

### Diagnóstico

```bash
# Uso de disco
df -h

# Espaço usado pelo Docker
docker system df

# Imagens grandes
docker images --format "{{.Repository}}:{{.Tag}} {{.Size}}" | sort -k2 -h
```

### Solução

```bash
# 1. Limpar imagens não utilizadas
docker image prune -f

# 2. Limpar build cache
docker builder prune -f

# 3. Limpar containers parados
docker container prune -f

# 4. Limpeza agressiva (cuidado!)
docker system prune -f

# 5. Verificar logs grandes
du -sh /var/lib/docker/containers/*/
```

---

## Comandos Úteis de Diagnóstico

### Status Geral

```bash
# Containers
docker ps -a
docker compose -f docker-compose.prod.yml ps

# Caddy
systemctl status caddy

# Sistema
df -h
free -m
uptime
```

### Logs Importantes

```bash
# Aplicação
docker logs l2sledger-backend --tail 100
docker logs l2sledger-frontend --tail 100

# Caddy
sudo journalctl -u caddy -n 100

# Sistema
dmesg | tail -20
```

### Network

```bash
# Redes Docker
docker network ls
docker network inspect caddy-network
docker network inspect shared-db-network

# Portas em uso
ss -tulpn | grep -E ':80|:443|:8080|:3000'
```

### Resources

```bash
# Docker resources
docker stats --no-stream

# Sistema
df -h
free -m
top -bn1 | head -10
```

### Processos

```bash
ps aux | grep -E 'caddy|docker'
```

---

## 📖 Referências

- [8-monitoring-health.md](8-monitoring-health.md) — Monitoramento contínuo
- [10-rollback-recovery.md](10-rollback-recovery.md) — Rollback e recuperação
- [7-caddy-configuration.md](7-caddy-configuration.md) — Troubleshooting do Caddy
- [6-environment-variables.md](6-environment-variables.md) — Referência de variáveis
