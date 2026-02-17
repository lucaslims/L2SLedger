# 5. Deploy em Produção e DEMO — L2SLedger

> Guia de deploy automático (DEMO) e manual com aprovação (PROD) via GitHub Actions, ou SSH manual (emergência).

---

## Pré-requisitos

- Servidor configurado (ver [4-production-setup.md](4-production-setup.md))
- Imagens buildadas e disponíveis no GHCR
- Tag da versão a ser deployada (ex: `v1.2.3`, `sha-abc1234`)

---

## Estratégia de Deploy

O L2SLedger utiliza **dois workflows distintos** para deploy, conforme [ADR-043](../adr/adr-043.md):

### 🔄 Deploy Automático (DEMO)

- **Workflow:** `.github/workflows/deploy-demo.yml`
- **Trigger:** Push para `main`, `release/**`, `hotfix/**`
- **Aprovação:** ❌ Não requer (automático)
- **Tagging:** SHA-based (`sha-abc1234`)
- **Uso:** Validação interna, testes de integração, demonstrações
- **Ambiente GitHub:** `demo`

### ✅ Deploy Manual com Aprovação (PROD)

- **Workflow:** `.github/workflows/deploy.yml`
- **Trigger:** Manual (workflow_dispatch)
- **Aprovação:** ✅ Requer aprovação humana (GitHub Environments)
- **Tagging:** Semantic versioning (`v1.2.3`)
- **Uso:** Releases oficiais para clientes
- **Ambiente GitHub:** `production`

> **Governança:** Esta separação garante que deployments para produção passem por aprovação explícita, enquanto o ambiente DEMO pode ser atualizado automaticamente para agilizar validações internas.

---

## Método 1: Deploy Automático para DEMO (GitHub Actions)

### Como Funciona

Push para branch principal dispara automaticamente:

1. **Detecção de Mudanças:** Workflow verifica se backend ou frontend foram alterados
2. **Tag SHA:** Gera tag baseada no commit SHA (`sha-abc1234`)
3. **Deploy:** Implanta automaticamente no ambiente DEMO
4. **Health Checks:** Valida que serviços iniciaram corretamente

### Branches que disparam deploy DEMO

```yaml
main
release/**
hotfix/**
```

### Verificação Pós-Deploy Automático

Após o push, aguarde 5-10 minutos e verifique:

```bash
# Health check do DEMO
curl -f https://demo.yourdomain.com/api/v1/health

# Verificar versão deployada no GitHub Actions
# Actions → Deploy to DEMO → último run
```

---

## Método 2: Deploy Manual com Aprovação para PROD (GitHub Actions)

### 1. Preparação

- Garantir que as imagens foram buildadas pelo pipeline CI e pushadas no GHCR
- Verificar tag disponível no GitHub Packages
- **Tag deve seguir semantic versioning:** `v1.2.3`

### 2. Trigger do Workflow

Via GitHub UI:

1. Ir para **Actions** → **Deploy to PROD (Manual + Approval)**
2. Clicar em **Run workflow**
3. Preencher:
   - `image_tag`: `v1.2.3` (semantic versioning obrigatório)
   - `service`: `all`, `backend`, ou `frontend`
4. Confirmar

### 3. Aprovação Humana

- O workflow **pausará** aguardando aprovação
- Revisores configurados no GitHub Environment `production` receberão notificação
- Revisor deve:
  - ✅ Aprovar para continuar o deploy
  - ❌ Rejeitar para cancelar

### 4. Acompanhar Execução

- Verificar logs em tempo real no GitHub Actions
- Aguardar health checks passarem
- Confirmar sucesso no workflow

### 5. Verificação Pós-Deploy

```bash
# Via SSH na VM
ssh user@VM_IP

docker compose -f /opt/l2sledger/docker-compose.prod.yml ps
docker logs l2sledger-backend --tail 50
docker logs l2sledger-frontend --tail 50
```

---

## Método 3: Deploy Manual SSH (Emergência)

> Use este método apenas quando o GitHub Actions não estiver disponível.

### 1. Conectar na VM

```bash
ssh user@VM_IP
cd /opt/l2sledger
```

### 2. Autenticar no GHCR (Se Necessário)

```bash
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin
```

### 3. Definir Tag e Owner

```bash
export IMAGE_TAG=v1.2.3
export GHCR_OWNER=your-github-org
```

### 4. Pull de Imagens

**Todos os serviços:**

```bash
docker compose -f docker-compose.prod.yml pull
```

**Apenas backend:**

```bash
docker compose -f docker-compose.prod.yml pull backend
```

**Apenas frontend:**

```bash
docker compose -f docker-compose.prod.yml pull frontend
```

### 5. Deploy

**Todos os serviços:**

```bash
docker compose -f docker-compose.prod.yml up -d
```

**Apenas backend (sem afetar outros serviços):**

```bash
docker compose -f docker-compose.prod.yml up -d --no-deps backend
```

**Apenas frontend (sem afetar outros serviços):**

```bash
docker compose -f docker-compose.prod.yml up -d --no-deps frontend
```

### 6. Verificar Status

```bash
docker compose -f docker-compose.prod.yml ps
```

### 7. Limpar Imagens Antigas

```bash
docker image prune -f --filter "until=168h"
```

---

## Verificação Pós-Deploy

### Health Checks

**Backend:**

```bash
# Dentro da VM
curl -f http://localhost:8080/api/v1/health

# Externo (via Caddy)
curl -f https://yourdomain.com/api/v1/health
```

**Frontend:**

```bash
# Dentro da VM
curl -f http://localhost:3000/

# Externo (via Caddy)
curl -f https://yourdomain.com/
```

### Status dos Containers

```bash
# Status rápido
docker ps -a | grep l2sledger

# Formato detalhado
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep l2sledger
```

### Logs Recentes

```bash
docker logs l2sledger-backend --tail 50 --timestamps
docker logs l2sledger-frontend --tail 50 --timestamps
```

### Resource Usage

```bash
docker stats l2sledger-backend l2sledger-frontend --no-stream
```

---

## Deploy de Hotfix

Para deploys urgentes com mudanças mínimas:

```bash
ssh user@VM_IP
cd /opt/l2sledger

# 1. Usar a tag do hotfix
export IMAGE_TAG=v1.2.4-hotfix
export GHCR_OWNER=your-github-org

# 2. Pull apenas do serviço afetado
docker compose -f docker-compose.prod.yml pull backend

# 3. Deploy sem downtime do frontend
docker compose -f docker-compose.prod.yml up -d --no-deps backend

# 4. Verificar
sleep 10
curl -f http://localhost:8080/api/v1/health
docker logs l2sledger-backend --tail 20
```

---

## Troubleshooting de Deploy

### Container Não Inicia

```bash
# Ver logs detalhados
docker logs l2sledger-backend

# Inspecionar configuração
docker inspect l2sledger-backend
```

### Health Check Falhando

```bash
# Verificar manualmente
docker exec l2sledger-backend wget -qO- http://localhost:8080/api/v1/health

# Verificar variáveis de ambiente
docker exec l2sledger-backend env | grep -E 'ASPNETCORE|Firebase|ConnectionStrings'
```

### Image Pull Falhando

```bash
# Verificar autenticação
docker login ghcr.io

# Verificar se tag existe
docker pull ghcr.io/your-org/l2sledger-backend:v1.2.3
```

> Para problemas mais detalhados, consulte [9-troubleshooting.md](9-troubleshooting.md).  
> Para rollback, consulte [10-rollback-recovery.md](10-rollback-recovery.md).

---

## Checklist de Deploy

```markdown
### Pré-Deploy
- [ ] Imagens buildadas e disponíveis no GHCR
- [ ] Tag da versão identificada
- [ ] Changelog revisado
- [ ] Testes passando no CI

### Durante Deploy
- [ ] Pull das imagens com sucesso
- [ ] Containers iniciados com sucesso
- [ ] Health checks passando

### Pós-Deploy
- [ ] Frontend acessível externamente
- [ ] Backend API respondendo
- [ ] Autenticação funcionando
- [ ] Logs sem erros críticos
- [ ] Equipe notificada
```

---

## ➡️ Próximos Passos

- Monitorar saúde → [8-monitoring-health.md](8-monitoring-health.md)
- Precisa reverter? → [10-rollback-recovery.md](10-rollback-recovery.md)
