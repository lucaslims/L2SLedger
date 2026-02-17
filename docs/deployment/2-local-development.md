# 2. Desenvolvimento Local (Sem Docker) — L2SLedger

> Guia para rodar backend e frontend diretamente no sistema operacional, ideal para desenvolvimento e debug ativo.

---

## Pré-requisitos

Certifique-se de ter completado o [1-prerequisites.md](1-prerequisites.md) antes de prosseguir.

**Ferramentas obrigatórias**:
- .NET 9 SDK
- Node.js 20+
- PostgreSQL 17 (local ou via container)
- Firebase projeto configurado

---

## Backend (.NET)

### 1. Iniciar PostgreSQL

#### Opção A: Via Container (Recomendado)

```bash
cd backend
docker compose -f docker-compose.dev.yml up -d
```

Isso inicia um PostgreSQL 17 local na porta 5432 com:
- Database: `l2sledger`
- User: `l2sledger`
- Password: `l2sledger`

#### Opção B: PostgreSQL Local

Se você tem PostgreSQL instalado diretamente, crie o banco:

```sql
CREATE DATABASE l2sledger;
CREATE USER l2sledger WITH ENCRYPTED PASSWORD 'l2sledger';
GRANT ALL PRIVILEGES ON DATABASE l2sledger TO l2sledger;
```

### 2. Preparar Firebase Credential

Coloque o arquivo Service Account JSON em um local seguro:

```bash
# Na raiz do projeto
mkdir -p secrets
# Copie o arquivo baixado do Firebase Console
cp ~/Downloads/firebase-service-account.json secrets/firebase-credential.json
```

### 3. Configurar `appsettings.Development.json`

Crie o arquivo `backend/src/L2SLedger.API/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=l2sledger;Username=l2sledger;Password=l2sledger"
  },
  "Firebase": {
    "CredentialPath": "C:\\caminho\\para\\secrets\\firebase-credential.json",
    "WebApiKey": "SUA_FIREBASE_WEB_API_KEY_AQUI"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000"
    ]
  }
}
```

> **Importante**: Ajuste `CredentialPath` para o caminho absoluto real do arquivo Firebase no seu sistema. Este arquivo é gitignored — não o commite.

### 4. Restaurar Pacotes

```bash
cd backend
dotnet restore
```

### 5. Aplicar Migrations

```bash
cd backend
dotnet ef database update --project src/L2SLedger.Infrastructure --startup-project src/L2SLedger.API
```

> **Nota**: Em modo Development, as migrations também são aplicadas automaticamente ao iniciar a API.

### 6. Executar o Backend

```bash
cd backend/src/L2SLedger.API
dotnet run
```

### 7. Verificar

| Endpoint | URL |
|----------|-----|
| Swagger | http://localhost:8080/swagger |
| Health Check | http://localhost:8080/api/v1/health |

```bash
# Teste rápido
curl http://localhost:8080/api/v1/health
```

---

## Frontend (React)

### 1. Configurar Variáveis de Ambiente

Crie o arquivo `frontend/.env.development`:

```env
VITE_API_BASE_URL=http://localhost:8080/api
VITE_FIREBASE_API_KEY=your-firebase-api-key
VITE_FIREBASE_AUTH_DOMAIN=your-project.firebaseapp.com
VITE_FIREBASE_PROJECT_ID=your-project-id
VITE_FIREBASE_STORAGE_BUCKET=your-project.appspot.com
VITE_FIREBASE_MESSAGING_SENDER_ID=123456789
VITE_FIREBASE_APP_ID=1:123456789:web:abc
VITE_ENABLE_DEVTOOLS=true
```

> Obtenha esses valores do Firebase Console (ver [1-prerequisites.md](1-prerequisites.md#obter-configuração-do-firebase-frontend)).

### 2. Instalar Dependências

```bash
cd frontend
npm install
```

### 3. Executar o Frontend

```bash
npm run dev
```

### 4. Verificar

| Recurso | URL |
|---------|-----|
| Aplicação | http://localhost:3000 |

---

## Integração Backend + Frontend

### Ordem de Inicialização

1. **Primeiro**: PostgreSQL (container ou local)
2. **Segundo**: Backend (`dotnet run`)
3. **Terceiro**: Frontend (`npm run dev`)

### Configuração CORS

O backend aceita requests do frontend via CORS. No `appsettings.Development.json`, a origem `http://localhost:3000` já está configurada.

Se o frontend rodar em outra porta, ajuste `Cors.AllowedOrigins` no backend.

### Verificar Integração

1. Abra http://localhost:3000 no navegador
2. O frontend deve se comunicar com o backend em http://localhost:8080/api
3. Tente fazer login com um usuário de teste do Firebase

---

## Testar Autenticação

### 1. Criar Usuário de Teste no Firebase

1. Acesse o [Firebase Console](https://console.firebase.google.com/)
2. Vá para **Authentication** → **Users** → **Add user**
3. Email: `test@example.com`
4. Password: `Test@123`

### 2. Obter Firebase ID Token (via REST API)

```bash
curl -X POST "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=YOUR_FIREBASE_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@123",
    "returnSecureToken": true
  }'
```

Copie o valor de `idToken` da resposta.

### 3. Login na API

```bash
curl -X POST "http://localhost:8080/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"firebaseIdToken": "SEU_ID_TOKEN_AQUI"}' \
  -c cookies.txt
```

### 4. Verificar Usuário Autenticado

```bash
curl -X GET "http://localhost:8080/api/v1/auth/me" \
  -b cookies.txt
```

### 5. Logout

```bash
curl -X POST "http://localhost:8080/api/v1/auth/logout" \
  -b cookies.txt
```

---

## Executar Testes

### Backend

```bash
cd backend
dotnet test
```

### Frontend

```bash
cd frontend
npm test
```

---

## Logs

### Backend

- Console: Logs JSON estruturados no terminal
- Arquivo: `backend/logs/l2sledger-YYYYMMDD.log`

### Frontend

- Console do navegador (DevTools → Console)

---

## Problemas Comuns

| Problema | Solução |
|----------|---------|
| Porta 5432 já em uso | Parar PostgreSQL local ou ajustar porta no `docker-compose.dev.yml` |
| Porta 8080 já em uso | Verificar se outro serviço usa a porta: `netstat -an \| findstr 8080` |
| Firebase credential not found | Verificar caminho absoluto em `appsettings.Development.json` |
| CORS error no navegador | Verificar `Cors.AllowedOrigins` inclui `http://localhost:3000` |
| Migration falha | Verificar se PostgreSQL está rodando e connection string está correta |

---

## ADRs Relevantes

- **ADR-001**: Firebase Authentication como IdP
- **ADR-002**: Fluxo de autenticação
- **ADR-004**: Cookies HttpOnly
- **ADR-006**: PostgreSQL como banco de dados
- **ADR-016**: RBAC (Role-Based Access Control)

---

## ➡️ Próximos Passos

- Quer rodar a stack completa via Docker? → [3-docker-local.md](3-docker-local.md)
- Precisa verificar variáveis de ambiente? → [6-environment-variables.md](6-environment-variables.md)
