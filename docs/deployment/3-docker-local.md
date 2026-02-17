# 3. Desenvolvimento com Docker Compose — L2SLedger

> Rodar a stack completa localmente via containers. Ambiente mais próximo da produção.

---

## Pré-requisitos

- Docker Desktop rodando (ver [1-prerequisites.md](1-prerequisites.md))
- Firebase projeto configurado com Service Account Key
- Redes Docker criadas:

```bash
docker network create caddy-network 2>/dev/null || true
docker network create shared-db-network 2>/dev/null || true
```

---

## 1. Preparação

### Copiar e Configurar `.env`

```bash
# Na raiz do projeto
cp .env.example .env
```

Edite o `.env` com os valores reais do seu Firebase:

```env
# Application
ASPNETCORE_ENVIRONMENT=Development
NODE_ENV=development

# PostgreSQL (usado pelo container postgres do docker-compose.dev.yml)
POSTGRES_HOST=postgres
POSTGRES_PORT=5432
POSTGRES_USER=l2sledger
POSTGRES_PASSWORD=l2sledger

# JWT
JWT_SECRET=your-local-jwt-secret-min-32-chars-here!!
JWT_ISSUER=https://localhost
JWT_AUDIENCE=l2sledger

# Firebase — preencher com valores reais
FIREBASE_CREDENTIAL_PATH=./secrets/firebase-credential.json
VITE_FIREBASE_API_KEY=your-actual-api-key
VITE_FIREBASE_AUTH_DOMAIN=your-project.firebaseapp.com
VITE_FIREBASE_PROJECT_ID=your-project-id
VITE_FIREBASE_STORAGE_BUCKET=your-project.appspot.com
VITE_FIREBASE_MESSAGING_SENDER_ID=123456789
VITE_FIREBASE_APP_ID=1:123456789:web:abc

# CORS
CORS_ALLOWED_ORIGINS=http://localhost

# Frontend
VITE_API_BASE_URL=http://localhost:8080/api
VITE_ENABLE_DEVTOOLS=true
```

### Colocar Firebase Credential

```bash
mkdir -p secrets/
cp ~/Downloads/firebase-service-account.json secrets/firebase-credential.json
```

> O diretório `secrets/` já está no `.gitignore`.

---

## 2. Iniciar o PostgreSQL

O `docker-compose.yml` principal **não inclui** PostgreSQL. Use o compose de dev do backend:

```bash
# Iniciar PostgreSQL separadamente
cd backend
docker compose -f docker-compose.dev.yml up -d
cd ..
```

Verifique se está rodando:

```bash
docker ps | findstr postgres
```

---

## 3. Executar a Stack

### Build e Start

```bash
# Na raiz do projeto
docker compose up -d
```

Isso inicia:
- **l2sledger-backend** (porta 8080)
- **l2sledger-frontend** (porta 3000)

### Verificar Status

```bash
docker compose ps
```

Esperado:

```
NAME                  STATUS          PORTS
l2sledger-backend     Up (healthy)    0.0.0.0:8080->8080/tcp
l2sledger-frontend    Up (healthy)    0.0.0.0:3000->3000/tcp
```

---

## 4. Acessar Serviços

| Serviço | URL | Container |
|---------|-----|-----------|
| Backend API | http://localhost:8080 | `l2sledger-backend` |
| Swagger | http://localhost:8080/swagger | `l2sledger-backend` |
| Health Check | http://localhost:8080/api/v1/health | `l2sledger-backend` |
| Frontend | http://localhost:3000 | `l2sledger-frontend` |
| PostgreSQL | `localhost:5432` | `l2sledger-postgres-dev` |

```bash
# Testes rápidos
curl http://localhost:8080/api/v1/health
curl http://localhost:3000/
```

---

## 5. Logs e Debug

### Ver Logs em Tempo Real

```bash
# Todos os serviços
docker compose logs -f

# Apenas backend
docker compose logs -f backend

# Apenas frontend
docker compose logs -f frontend
```

### Ver Logs com Timestamp

```bash
docker compose logs --timestamps backend
```

### Últimas N Linhas

```bash
docker compose logs --tail 50 backend
```

### Executar Comandos dentro do Container

```bash
# Shell no backend
docker exec -it l2sledger-backend sh

# Shell no frontend
docker exec -it l2sledger-frontend sh

# Verificar variáveis de ambiente do backend
docker exec l2sledger-backend env | sort
```

---

## 6. Rebuild Após Mudanças

### Mudanças no Backend

```bash
docker compose build backend
docker compose up -d backend
```

### Mudanças no Frontend

```bash
docker compose build frontend
docker compose up -d frontend
```

### Rebuild Completo (Sem Cache)

```bash
docker compose build --no-cache
docker compose up -d
```

---

## 7. Parar e Limpar

### Parar Containers

```bash
# Parar a stack principal
docker compose down

# Parar PostgreSQL
cd backend
docker compose -f docker-compose.dev.yml down
```

### Parar e Remover Volumes (Reset Completo)

```bash
# Remove containers E dados do banco
cd backend
docker compose -f docker-compose.dev.yml down -v
cd ..
docker compose down -v
```

> **Cuidado**: `-v` remove os volumes, apagando todos os dados do banco de dados local.

---

## 8. Dicas

### Acompanhar Startup

Para ver o progresso do startup (especialmente útil na primeira vez):

```bash
docker compose up -d
docker compose logs -f
```

### Verificar Resource Usage

```bash
docker stats l2sledger-backend l2sledger-frontend --no-stream
```

### Conectar ao PostgreSQL

```bash
# Via psql no container
docker exec -it l2sledger-postgres-dev psql -U l2sledger -d l2sledger

# Listar tabelas
docker exec -it l2sledger-postgres-dev psql -U l2sledger -d l2sledger -c "\dt"
```

### Limpar Imagens Antigas

```bash
docker image prune -f
```

---

## Arquitetura dos Containers (Local)

```
┌──────────────────────────────────────────────────────────┐
│ Docker Compose (Local)                                    │
│                                                           │
│  ┌─────────────────┐     ┌─────────────────┐             │
│  │ l2sledger-       │     │ l2sledger-       │             │
│  │ backend          │     │ frontend         │             │
│  │ :8080            │     │ :3000 (serve)    │             │
│  └────────┬─────────┘     └──────────────────┘             │
│           │                                                │
│  ┌────────▼─────────┐                                     │
│  │ l2sledger-        │                                     │
│  │ postgres-dev      │                                     │
│  │ :5432             │                                     │
│  └───────────────────┘                                     │
└──────────────────────────────────────────────────────────┘
```

> **Nota**: Em local, ambos backend e frontend usam `ports:` e são acessíveis diretamente. Em produção, usam `expose:` e Caddy roteia via Docker DNS.

---

## Problemas Comuns

| Problema | Solução |
|----------|---------|
| `network caddy-network not found` | Criar rede: `docker network create caddy-network` |
| `network shared-db-network not found` | Criar rede: `docker network create shared-db-network` |
| Container sai imediatamente | Ver logs: `docker compose logs backend` |
| Build falha | Verificar se Docker Desktop está rodando |
| PostgreSQL não acessível pelo backend | Verificar se postgres-dev está na rede `shared-db-network` |

Para problemas mais detalhados, consulte [9-troubleshooting.md](9-troubleshooting.md).

---

## ➡️ Próximos Passos

- Quer configurar servidor de produção? → [4-production-setup.md](4-production-setup.md)
- Precisa verificar variáveis? → [6-environment-variables.md](6-environment-variables.md)
