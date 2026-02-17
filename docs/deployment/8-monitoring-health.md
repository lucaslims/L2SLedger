# 8. Monitoramento e Health Checks — L2SLedger

> Verificar saúde, analisar logs e monitorar recursos do sistema em produção.

---

## 1. Health Endpoints

### Backend

**URL**: `GET /api/v1/health`

**Resposta Saudável** (HTTP 200):

```json
{
  "status": "Healthy",
  "timestamp": "2026-02-16T10:00:00Z"
}
```

**Verificação**:

```bash
# Interno (na VM)
curl http://localhost:8080/api/v1/health

# Externo (via Caddy)
curl https://yourdomain.com/api/v1/health
```

### Frontend

**URL**: `GET /`

**Resposta**: HTML da SPA (HTTP 200)

**Verificação**:

```bash
# Interno (na VM)
curl -I http://localhost:3000/

# Externo (via Caddy)
curl -I https://yourdomain.com/
```

---

## 2. Docker Health Checks

Os Dockerfiles já incluem health checks automáticos:

### Backend

```dockerfile
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/api/v1/health || exit 1
```

### Frontend

```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:3000/ || exit 1
```

### Verificar Status

```bash
# A coluna STATUS mostra o estado do health check
docker ps
# Exemplo: "Up 5 minutes (healthy)" ou "Up 2 minutes (unhealthy)"
```

### Detalhes do Health Check

```bash
# Ver histórico de health checks
docker inspect l2sledger-backend | jq '.[0].State.Health'
docker inspect l2sledger-frontend | jq '.[0].State.Health'
```

---

## 3. Logs

### Ver Logs em Tempo Real

```bash
# Backend
docker logs -f l2sledger-backend

# Frontend
docker logs -f l2sledger-frontend

# Ambos (em terminais separados)
docker logs -f l2sledger-backend &
docker logs -f l2sledger-frontend
```

### Logs com Timestamp

```bash
docker logs --timestamps l2sledger-backend
```

### Últimas N Linhas

```bash
docker logs --tail 100 l2sledger-backend
docker logs --tail 50 l2sledger-frontend
```

### Filtrar por Nível

```bash
# Erros
docker logs l2sledger-backend 2>&1 | grep "ERROR"
docker logs l2sledger-backend 2>&1 | grep "Exception"

# Warnings
docker logs l2sledger-backend 2>&1 | grep "WARN"

# Health-related
docker logs l2sledger-backend 2>&1 | grep -i health
```

### Logs do Caddy

```bash
# Via systemd
sudo journalctl -u caddy -f

# Via arquivo (se configurado)
tail -f /var/log/caddy/access.log
```

### Via Docker Compose

```bash
cd /opt/l2sledger
docker compose -f docker-compose.prod.yml logs -f
docker compose -f docker-compose.prod.yml logs --tail 50
```

---

## 4. Status de Containers

### Lista Rápida

```bash
docker ps -a | grep l2sledger
```

### Formato Detalhado

```bash
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep l2sledger
```

### Via Docker Compose

```bash
cd /opt/l2sledger
docker compose -f docker-compose.prod.yml ps
```

### Inspecionar Container

```bash
docker inspect l2sledger-backend | jq '.[0].State'
```

---

## 5. Resource Usage

### Stats em Tempo Real

```bash
docker stats l2sledger-backend l2sledger-frontend
```

### Snapshot (Sem Streaming)

```bash
docker stats --no-stream l2sledger-backend l2sledger-frontend
```

### Verificar Limites Configurados

```bash
# Memória
docker inspect l2sledger-backend | jq '.[0].HostConfig.Memory'
# Backend: 512MB (536870912)

docker inspect l2sledger-frontend | jq '.[0].HostConfig.Memory'
# Frontend: 128MB (134217728)

# CPU
docker inspect l2sledger-backend | jq '.[0].HostConfig.NanoCpus'
# Backend: 1.0 CPU (1000000000)

docker inspect l2sledger-frontend | jq '.[0].HostConfig.NanoCpus'
# Frontend: 0.5 CPU (500000000)
```

### Sistema (VM)

```bash
# Disco
df -h

# Memória
free -m

# CPU
top -bn1 | head -5

# Processos Docker
ps aux | grep -E 'docker|caddy'
```

---

## 6. Network Connectivity

### Verificar Conectividade Interna

```bash
# Backend pode acessar database?
docker exec l2sledger-backend ping -c 3 postgres 2>/dev/null || echo "ping not available, try wget"

# Testar conexão do backend com database via health
docker exec l2sledger-backend wget -qO- http://localhost:8080/api/v1/health
```

### Verificar Redes Docker

```bash
# Listar redes
docker network ls

# Containers na caddy-network
docker network inspect caddy-network | jq '.[0].Containers'

# Containers na shared-db-network
docker network inspect shared-db-network | jq '.[0].Containers'

# Redes do backend
docker inspect l2sledger-backend | jq '.[0].NetworkSettings.Networks'
```

### Verificar Portas

```bash
# Portas em uso
ss -tulpn | grep -E ':80|:443|:8080|:3000'
```

---

## 7. Database Connection

### Teste de Conexão

```bash
# Via container PostgreSQL
docker exec -it postgres psql -U l2sledger -d l2sledger -c "SELECT 1;"

# Verificar tabelas
docker exec -it postgres psql -U l2sledger -d l2sledger -c "\dt"

# Ver conexões ativas
docker exec -it postgres psql -U l2sledger -d l2sledger -c "SELECT count(*) FROM pg_stat_activity;"
```

---

## 8. Checklist de Monitoramento

### Verificação Diária (Sugerida)

```bash
# Script de verificação rápida
echo "=== Container Status ==="
docker ps --format "table {{.Names}}\t{{.Status}}" | grep l2sledger

echo "=== Health Checks ==="
curl -s http://localhost:8080/api/v1/health
curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/

echo "=== Resource Usage ==="
docker stats --no-stream l2sledger-backend l2sledger-frontend

echo "=== Disk Space ==="
df -h /

echo "=== Recent Errors ==="
docker logs l2sledger-backend --since 24h 2>&1 | grep -c "ERROR" || echo "0 errors"
```

### Alertas

| Condição | Severidade | Ação |
|----------|------------|------|
| Container `unhealthy` | Alta | Verificar logs, restart se necessário |
| Health endpoint down | Crítica | Verificar app logs, database connection |
| CPU > 90% sustentado | Média | Investigar leak ou load, considerar scaling |
| Memory > 80% | Média | Verificar leaks, restart preventivo |
| Disk > 85% | Alta | Limpar images antigas, verificar logs |
| 5xx errors frequentes | Alta | Analisar logs de exceção, verificar dependencies |
| Certificado expirando | Média | Verificar Caddy, forçar renovação |

---

## 9. Monitoramento Automatizado (Futuro)

### Prometheus + Grafana

- Expor métricas do backend: `/api/v1/metrics` (a implementar)
- Dashboard de performance e requests
- Alertas automáticos por Slack/Email

### Loki + Promtail

- Agregação centralizada de logs
- Queries e filtros avançados
- Correlação de eventos

### UptimeRobot / Similar

- Monitoramento externo de disponibilidade
- Notificação de downtime
- Histórico de uptime

---

## 📖 Referências

- [DevOps Strategy](../devops-strategy.md) — Decisões de containerização e health checks
- [5-production-deploy.md](5-production-deploy.md) — Verificação pós-deploy
- [9-troubleshooting.md](9-troubleshooting.md) — Resolução de problemas
