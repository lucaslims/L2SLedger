# 6. Variáveis de Ambiente — Referência Completa

> Todas as variáveis de ambiente usadas pelo L2SLedger, organizadas por categoria.

---

## 📋 Índice

- [PostgreSQL](#postgresql)
- [Firebase](#firebase)
- [JWT](#jwt)
- [CORS](#cors)
- [Aplicação](#aplicação)
- [Docker / Deploy](#docker--deploy)
- [Diferenças entre Ambientes](#diferenças-entre-ambientes)
- [Variáveis Sensíveis (Secrets)](#variáveis-sensíveis-secrets)

---

## PostgreSQL

| Variável | Descrição | Obrigatório | Padrão | Contexto | Exemplo |
|----------|-----------|:-----------:|--------|----------|---------|
| `POSTGRES_HOST` | Host do PostgreSQL | Sim | `postgres` | Backend | `postgres` (Docker), `localhost` (local), `10.0.1.5` (prod) |
| `POSTGRES_PORT` | Porta do PostgreSQL | Não | `5432` | Backend | `5432` |
| `POSTGRES_USER` | Usuário do banco | Sim | `l2sledger` | Backend | `l2sledger` |
| `POSTGRES_PASSWORD` | Senha do banco | Sim | `l2sledger` | Backend | `SecureP@ssw0rd` |
| `ConnectionStrings__DefaultConnection` | String completa de conexão | Sim¹ | — | Backend | `Host=postgres;Port=5432;Database=l2sledger;Username=l2sledger;Password=xxx` |

> ¹ No Docker Compose, a connection string é montada a partir das variáveis individuais. Em desenvolvimento local sem Docker, configure diretamente no `appsettings.Development.json`.

---

## Firebase

### Backend

| Variável | Descrição | Obrigatório | Contexto | Exemplo |
|----------|-----------|:-----------:|----------|---------|
| `FIREBASE_CREDENTIAL_PATH` | Caminho para o Service Account JSON | Sim | Backend | `./secrets/firebase-credential.json` |
| `FIREBASE_WEB_API_KEY` | Firebase Web API Key (prod compose) | Sim (prod) | Backend | `AIzaSyXXXXXXXXXXXXXXXXXX` |
| `VITE_FIREBASE_API_KEY` | Firebase Web API Key (local compose) | Sim (local) | Backend | `AIzaSyXXXXXXXXXXXXXXXXXX` |

> **Nota**: Em `docker-compose.yml` (local), o backend usa `VITE_FIREBASE_API_KEY` mapeado para `Firebase__WebAPIKey`. Em `docker-compose.prod.yml`, usa `FIREBASE_WEB_API_KEY`.

### Frontend

| Variável | Descrição | Obrigatório | Contexto | Exemplo |
|----------|-----------|:-----------:|----------|---------|
| `VITE_FIREBASE_API_KEY` | Firebase Web API Key | Sim | Frontend | `AIzaSyXXXXXXXXXXXXXXXXXX` |
| `VITE_FIREBASE_AUTH_DOMAIN` | Firebase Auth Domain | Sim | Frontend | `my-project.firebaseapp.com` |
| `VITE_FIREBASE_PROJECT_ID` | Firebase Project ID | Sim | Frontend | `my-project-id` |
| `VITE_FIREBASE_STORAGE_BUCKET` | Firebase Storage Bucket | Sim | Frontend | `my-project.appspot.com` |
| `VITE_FIREBASE_MESSAGING_SENDER_ID` | Firebase Messaging Sender ID | Sim | Frontend | `123456789` |
| `VITE_FIREBASE_APP_ID` | Firebase App ID | Sim | Frontend | `1:123456789:web:abc123` |

> **Importante**: Todas as variáveis `VITE_*` são injetadas no frontend em runtime via `env.sh` (ver [DevOps Strategy](../devops-strategy.md)).

---

## JWT

| Variável | Descrição | Obrigatório | Padrão | Contexto | Exemplo |
|----------|-----------|:-----------:|--------|----------|---------|
| `JWT_SECRET` | Chave secreta para assinar tokens | Sim | — | Backend | `your-secret-min-32-chars-here!!` |
| `JWT_ISSUER` | Emissor dos tokens JWT | Sim | `https://localhost` | Backend | `https://yourdomain.com` |
| `JWT_AUDIENCE` | Audiência dos tokens JWT | Sim | `l2sledger` | Backend | `l2sledger` |

> **Segurança**: Em produção, o `JWT_SECRET` deve ter no mínimo 64 caracteres, gerado aleatoriamente. Ver [1-prerequisites.md](1-prerequisites.md#gerar-jwt-secret-seguro).

---

## CORS

| Variável | Descrição | Obrigatório | Padrão | Contexto | Exemplo |
|----------|-----------|:-----------:|--------|----------|---------|
| `CORS_ALLOWED_ORIGINS` | Origens permitidas (separadas por vírgula) | Sim | `http://localhost` | Backend | `http://localhost` (dev), `https://yourdomain.com` (prod) |

---

## Aplicação

| Variável | Descrição | Obrigatório | Padrão | Contexto | Exemplo |
|----------|-----------|:-----------:|--------|----------|---------|
| `ASPNETCORE_ENVIRONMENT` | Ambiente do .NET | Sim | `Development` | Backend | `Development`, `Production` |
| `NODE_ENV` | Ambiente do Node.js | Não | `development` | Frontend | `development`, `production` |
| `VITE_API_BASE_URL` | URL base da API para o frontend | Sim | `http://localhost:8080/api` | Frontend | `http://localhost:8080/api` (dev), `https://yourdomain.com/api` (prod) |
| `VITE_ENABLE_DEVTOOLS` | Habilitar devtools no frontend | Não | `true` | Frontend | `true` (dev), `false` (prod) |

---

## Docker / Deploy

| Variável | Descrição | Obrigatório | Padrão | Contexto | Exemplo |
|----------|-----------|:-----------:|--------|----------|---------|
| `GHCR_OWNER` | Owner do repositório no GHCR | Sim (prod) | `l2s-ledger` | Docker Compose | `your-github-org` |
| `IMAGE_TAG` | Tag da imagem Docker a ser deployada | Sim (prod) | `latest` | Docker Compose | `v1.2.3`, `sha-abc1234`, `latest` |

---

## Diferenças entre Ambientes

### Desenvolvimento Local (`.env`)

```env
# Application
ASPNETCORE_ENVIRONMENT=Development
NODE_ENV=development

# PostgreSQL
POSTGRES_HOST=postgres
POSTGRES_PORT=5432
POSTGRES_USER=l2sledger
POSTGRES_PASSWORD=l2sledger

# JWT
JWT_SECRET=your-local-jwt-secret-min-32-chars-here!!
JWT_ISSUER=https://localhost
JWT_AUDIENCE=l2sledger

# Firebase
FIREBASE_CREDENTIAL_PATH=./secrets/firebase-credential.json
VITE_FIREBASE_API_KEY=your-api-key
VITE_FIREBASE_AUTH_DOMAIN=your-project.firebaseapp.com
VITE_FIREBASE_PROJECT_ID=your-project-id
VITE_FIREBASE_STORAGE_BUCKET=your-project.appspot.com
VITE_FIREBASE_MESSAGING_SENDER_ID=123456789
VITE_FIREBASE_APP_ID=1:123456789:web:abc

# CORS & Frontend
CORS_ALLOWED_ORIGINS=http://localhost
VITE_API_BASE_URL=http://localhost:8080/api
VITE_ENABLE_DEVTOOLS=true
```

### Produção (`/opt/l2sledger/.env`)

```env
# Application
ASPNETCORE_ENVIRONMENT=Production
NODE_ENV=production

# PostgreSQL
POSTGRES_HOST=postgres-container-or-ip
POSTGRES_PORT=5432
POSTGRES_USER=l2sledger_prod
POSTGRES_PASSWORD=STRONG_RANDOM_PASSWORD_HERE

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

# CORS & Frontend
CORS_ALLOWED_ORIGINS=https://yourdomain.com
VITE_API_BASE_URL=https://yourdomain.com/api
VITE_ENABLE_DEVTOOLS=false

# Docker / Deploy
GHCR_OWNER=your-github-org
IMAGE_TAG=latest
```

### Diferenças Chave

| Variável | DEV | PROD |
|----------|-----|------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` |
| `JWT_SECRET` | Qualquer string ≥32 chars | Forte, aleatório, ≥64 chars |
| `POSTGRES_PASSWORD` | `l2sledger` | Senha forte e aleatória |
| `CORS_ALLOWED_ORIGINS` | `http://localhost` | `https://yourdomain.com` |
| `VITE_API_BASE_URL` | `http://localhost:8080/api` | `https://yourdomain.com/api` |
| `VITE_ENABLE_DEVTOOLS` | `true` | `false` |

---

## Variáveis Sensíveis (Secrets)

### **NUNCA** commitar no Git

- `JWT_SECRET`
- `POSTGRES_PASSWORD`
- Conteúdo de `firebase-credential.json`
- Tokens de API / GitHub Tokens
- Arquivo `.env` (já no `.gitignore`)

### Armazenamento Seguro

| Contexto | Método |
|----------|--------|
| **Desenvolvimento Local** | Arquivo `.env` (gitignored) + `secrets/` (gitignored) |
| **Produção (VM)** | Arquivo `.env` com permissões restritas (`chmod 600`) |
| **CI/CD** | GitHub Secrets (configurados no repositório) |

### GitHub Secrets Necessários (CI/CD)

| Secret | Descrição |
|--------|-----------|
| `VM_HOST` | IP ou hostname da VM de produção |
| `VM_USER` | Usuário SSH na VM |
| `VM_SSH_KEY` | Chave SSH privada para a VM |
| `VM_SSH_PORT` | Porta SSH (padrão: 22) |
| `VM_DEPLOY_PATH` | Caminho na VM (padrão: `/opt/l2sledger`) |
| `VITE_API_BASE_URL` | URL da API em produção |
| `VITE_FIREBASE_API_KEY` | Firebase API Key |
| `VITE_FIREBASE_AUTH_DOMAIN` | Firebase Auth Domain |
| `VITE_FIREBASE_PROJECT_ID` | Firebase Project ID |
| `VITE_FIREBASE_STORAGE_BUCKET` | Firebase Storage Bucket |
| `VITE_FIREBASE_MESSAGING_SENDER_ID` | Firebase Messaging Sender ID |
| `VITE_FIREBASE_APP_ID` | Firebase App ID |

---

## 📖 Referências

- [`.env.example`](../../.env.example) — Template de variáveis (raiz do projeto)
- [`frontend/.env.example`](../../frontend/.env.example) — Template do frontend
- [DevOps Strategy](../devops-strategy.md) — Decisões de segurança e infraestrutura
- [`docker-compose.yml`](../../docker-compose.yml) — Compose local (mapeamento de variáveis)
- [`docker-compose.prod.yml`](../../docker-compose.prod.yml) — Compose produção (mapeamento de variáveis)
