# 10. Rollback e Recuperação — L2SLedger

> Procedimentos de rollback para versões anteriores e recuperação de desastres.

---

## 1. Rollback para Versão Anterior

**Cenário**: Deploy da versão nova causou problema crítico.

**Pré-requisito**: Saber a tag da versão anterior estável (ex: `v1.2.2` ou `sha-abc1234`).

### Rollback Completo (Backend + Frontend)

```bash
# 1. Conectar na VM
ssh user@VM_IP
cd /opt/l2sledger

# 2. Definir tag da versão anterior
export IMAGE_TAG=v1.2.2  # substituir pela última versão estável
export GHCR_OWNER=your-github-org

# 3. Pull da versão anterior
docker compose -f docker-compose.prod.yml pull

# 4. Deploy da versão anterior
docker compose -f docker-compose.prod.yml up -d

# 5. Verificar saúde
sleep 15
docker compose -f docker-compose.prod.yml ps
curl -f http://localhost:8080/api/v1/health
curl -f http://localhost:3000/

# 6. Verificar logs
docker logs l2sledger-backend --tail 50
docker logs l2sledger-frontend --tail 50
```

### Rollback Apenas Backend

```bash
export IMAGE_TAG=v1.2.2
export GHCR_OWNER=your-github-org

docker compose -f docker-compose.prod.yml pull backend
docker compose -f docker-compose.prod.yml up -d --no-deps backend

# Verificar
sleep 10
curl -f http://localhost:8080/api/v1/health
```

### Rollback Apenas Frontend

```bash
export IMAGE_TAG=v1.2.2
export GHCR_OWNER=your-github-org

docker compose -f docker-compose.prod.yml pull frontend
docker compose -f docker-compose.prod.yml up -d --no-deps frontend

# Verificar
sleep 5
curl -f http://localhost:3000/
```

> **Tempo estimado**: < 5 minutos para rollback completo.

---

## 2. Recuperação de Container Crashed

**Cenário**: Container parou inesperadamente.

### Diagnóstico

```bash
# Ver estado do container
docker ps -a | grep l2sledger

# Ver logs do crash
docker logs l2sledger-backend --tail 200

# Ver eventos Docker recentes
docker events --since '10m' --filter 'container=l2sledger-backend'
```

### Recuperação

```bash
# 1. Tentar restart simples
docker compose -f /opt/l2sledger/docker-compose.prod.yml restart backend

# 2. Se não funcionar, forçar recreação
docker compose -f /opt/l2sledger/docker-compose.prod.yml up -d --force-recreate backend

# 3. Se ainda falhar, verificar recursos
docker stats --no-stream
df -h
free -m

# 4. Cleanup e restart completo
docker system prune -f
docker compose -f /opt/l2sledger/docker-compose.prod.yml down backend
docker compose -f /opt/l2sledger/docker-compose.prod.yml up -d backend
```

---

## 3. Recuperação de Ambiente Completo

**Cenário**: Múltiplos problemas, ambiente instável, necessita reset completo.

> **ATENÇÃO**: Isso recria TODOS os containers da stack L2SLedger (exceto database/redis se forem externos).

```bash
cd /opt/l2sledger

# 1. Backup de configuração atual
cp .env .env.backup.$(date +%Y%m%d_%H%M%S)

# 2. Parar e remover containers
docker compose -f docker-compose.prod.yml down

# 3. Limpar imagens antigas (opcional)
docker image prune -f

# 4. Re-pull das imagens
export IMAGE_TAG=latest  # ou versão específica estável
export GHCR_OWNER=your-github-org
docker compose -f docker-compose.prod.yml pull

# 5. Recriar ambiente
docker compose -f docker-compose.prod.yml up -d

# 6. Monitorar startup
docker compose -f docker-compose.prod.yml logs -f &
sleep 30

# 7. Verificar saúde
docker compose -f docker-compose.prod.yml ps
curl -f http://localhost:8080/api/v1/health
curl -f http://localhost:3000/
```

---

## 4. Recuperação de Rede Docker

**Cenário**: Problemas de conectividade entre containers ou Caddy.

### Diagnóstico

```bash
# Listar redes
docker network ls

# Inspecionar redes
docker network inspect caddy-network
docker network inspect shared-db-network

# Ver conectividade dos containers
docker inspect l2sledger-backend | jq '.[0].NetworkSettings.Networks'
```

### Recuperação

```bash
# 1. Parar containers
cd /opt/l2sledger
docker compose -f docker-compose.prod.yml down

# 2. Recriar redes (se necessário)
docker network rm caddy-network shared-db-network 2>/dev/null
docker network create caddy-network
docker network create shared-db-network

# 3. Reconectar Caddy à rede (se for container)
docker network connect caddy-network caddy-container-name 2>/dev/null

# 4. Recriar containers
docker compose -f docker-compose.prod.yml up -d

# 5. Verificar conectividade
docker network inspect caddy-network | jq '.[0].Containers'
docker exec l2sledger-backend wget -qO- http://localhost:8080/api/v1/health
```

---

## 5. Recuperação de Database (Referência)

> **NOTA**: Este procedimento assume backup prévio existente. Backups devem ser configurados externamente.

### Backup (Executar Periodicamente)

```bash
# Backup completo
docker exec postgres pg_dump -U l2sledger -d l2sledger > backup_$(date +%Y%m%d_%H%M%S).sql

# Backup comprimido
docker exec postgres pg_dump -U l2sledger -d l2sledger | gzip > backup_$(date +%Y%m%d_%H%M%S).sql.gz
```

### Restore

```bash
# Restore de backup
cat backup_20260216.sql | docker exec -i postgres psql -U l2sledger -d l2sledger

# Restore de backup comprimido
gunzip -c backup_20260216.sql.gz | docker exec -i postgres psql -U l2sledger -d l2sledger
```

> **Cuidado**: Antes de restaurar, considere se precisa dropar e recriar o banco para evitar conflitos.

---

## 6. Recuperação de Secrets Perdidos

### Firebase Credential

```bash
# 1. Re-download do Firebase Console
#    Firebase Console → Project Settings → Service Accounts → Generate New Key

# 2. Upload para VM
scp firebase-credential.json user@VM_IP:/opt/l2sledger/secrets/

# 3. Permissões corretas
chmod 600 /opt/l2sledger/secrets/firebase-credential.json

# 4. Restart backend
docker compose -f /opt/l2sledger/docker-compose.prod.yml restart backend
```

### .env Perdido

```bash
# 1. Recriar a partir do template
#    (usar .env.example como referência e preencher com valores de produção)

# 2. Restaurar de backup (se existir)
ls /opt/l2sledger/.env.backup.*
cp /opt/l2sledger/.env.backup.YYYYMMDD_HHMMSS /opt/l2sledger/.env

# 3. Verificar e completar valores
nano /opt/l2sledger/.env
```

---

## 7. Disaster Recovery: VM Perdida

**Cenário**: VM foi destruída ou inacessível. Necessita recriar ambiente do zero.

### Pré-requisitos para DR

Para que o DR seja possível, mantenha **externamente** (fora da VM):

- Backups de database (agendados)
- `.env` com configurações (GitHub Secrets ou armazenamento seguro)
- `firebase-credential.json` (backup seguro)
- Código no GitHub (sempre atualizado)

### Procedimento

1. **Provisionar nova VM** → Seguir [4-production-setup.md](4-production-setup.md)
2. **Restaurar database** → Aplicar backup SQL mais recente
3. **Configurar `.env` e secrets** → A partir do backup ou GitHub Secrets
4. **Deploy da última versão estável** → Seguir [5-production-deploy.md](5-production-deploy.md)
5. **Verificar tudo** → Health checks, autenticação, funcionalidades
6. **Atualizar DNS** → Se o IP da VM mudou

### Tempo Estimado de Recuperação

| Etapa | Tempo |
|-------|-------|
| Provisionar VM | 10-15 min |
| Setup (Docker, Caddy, etc.) | 20-30 min |
| Configurar .env e secrets | 10-15 min |
| Deploy | 5-10 min |
| Restore database | 5-30 min (depende do tamanho) |
| Verificação | 10-15 min |
| **Total** | **~60-120 min** |

---

## 8. Checklist Pós-Recuperação

Após qualquer procedimento de recuperação, verifique:

```markdown
- [ ] Todos os containers healthy (`docker ps`)
- [ ] Health endpoints respondem OK
- [ ] Logs não mostram erros críticos
- [ ] Frontend acessível externamente (HTTPS)
- [ ] Backend API acessível via frontend
- [ ] Autenticação Firebase funcionando (login/logout)
- [ ] Database conectado e funcional
- [ ] Caddy roteando corretamente
- [ ] HTTPS funcionando (certificado válido)
- [ ] Equipe notificada sobre a recuperação
- [ ] Post-mortem agendado (se aplicável)
```

---

## 9. Escalation Path

| Nível | Situação | Ação |
|-------|----------|------|
| 1 | Deploy com problemas | Rollback (Seção 1) — qualquer dev/ops |
| 2 | Container instável | Recuperação de container (Seção 2) — ops |
| 3 | Ambiente degradado | Recuperação completa (Seção 3) — ops senior |
| 4 | Rede Docker quebrada | Recuperação de rede (Seção 4) — ops |
| 5 | Dados corrompidos | Restore de database (Seção 5) — DBA/ops senior |
| 6 | VM perdida | Disaster Recovery (Seção 7) — ops senior + lead |

### Regra de Escalação

- **Primeiro Responder**: Tentar rollback (< 5 min)
- **Se persistir (> 15 min)**: Escalar para ops senior
- **Se crítico (> 30 min)**: Escalar para tech lead
- **Desastre total**: DR completo + comunicação executiva

---

## 📖 Referências

- [5-production-deploy.md](5-production-deploy.md) — Deploy procedures
- [9-troubleshooting.md](9-troubleshooting.md) — Resolução de problemas
- [8-monitoring-health.md](8-monitoring-health.md) — Monitoramento e verificação
- [4-production-setup.md](4-production-setup.md) — Setup de servidor novo (para DR)
