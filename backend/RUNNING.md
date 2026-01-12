# L2SLedger Backend - Guia de Execução

## Pré-requisitos

- **.NET 9.0 SDK** instalado
- **PostgreSQL 17** em execução
- **Firebase Project** configurado (para autenticação)
- **Visual Studio 2022** ou **VS Code** com extensões C#

---

## Configuração Inicial

### 1. Configurar PostgreSQL

Crie o banco de dados e usuário:

```sql
CREATE DATABASE l2sledger;
CREATE USER l2sledger WITH ENCRYPTED PASSWORD 'l2sledger';
GRANT ALL PRIVILEGES ON DATABASE l2sledger TO l2sledger;
```

### 2. Configurar Firebase

1. Acesse o [Firebase Console](https://console.firebase.google.com/)
2. Crie um novo projeto ou use um existente
3. Ative **Authentication** com provedor **Email/Password**
4. Gere uma **Service Account Key**:
   - Project Settings → Service Accounts
   - Generate New Private Key
   - Salve o arquivo JSON

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
    "CredentialPath": "C:\\caminho\\para\\firebase-service-account.json"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000"
    ]
  }
}
```

**Importante:**
- Ajuste o `CredentialPath` para o caminho real do arquivo Firebase
- Não commite este arquivo (já está no .gitignore)

---

## Executar o Backend

### 1. Restaurar Pacotes

```bash
cd backend
dotnet restore
```

### 2. Aplicar Migrations

```bash
dotnet ef database update --project src/L2SLedger.Infrastructure --startup-project src/L2SLedger.API
```

Ou rode a API em Development (auto-migration está habilitado):

```bash
cd src/L2SLedger.API
dotnet run
```

### 3. Verificar API

Acesse o Swagger: [https://localhost:5001/swagger](https://localhost:5001/swagger)

Endpoints disponíveis:
- `POST /api/v1/auth/login` - Login com Firebase ID Token
- `GET /api/v1/auth/me` - Dados do usuário autenticado
- `POST /api/v1/auth/logout` - Logout

---

## Testar Autenticação

### Passo 1: Criar Usuário no Firebase

Use o [Firebase Console](https://console.firebase.google.com/) para criar um usuário de teste:

1. Authentication → Users → Add user
2. Email: `test@example.com`
3. Password: `Test@123`
4. ✅ Marque "Email verified"

### Passo 2: Obter Firebase ID Token

#### Opção A: Via Frontend

Se você tem um frontend integrado:
1. Faça login pelo frontend
2. O frontend enviará o ID Token para `POST /api/v1/auth/login`

#### Opção B: Via Firebase REST API

```bash
curl -X POST "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=YOUR_FIREBASE_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@123",
    "returnSecureToken": true
  }'
```

Pegue o valor de `idToken` na resposta.

### Passo 3: Login na API

```bash
curl -X POST "https://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "firebaseIdToken": "SEU_FIREBASE_ID_TOKEN_AQUI"
  }' \
  -c cookies.txt
```

Isso criará um cookie de sessão em `cookies.txt`.

### Passo 4: Verificar Usuário Autenticado

```bash
curl -X GET "https://localhost:5001/api/v1/auth/me" \
  -b cookies.txt
```

### Passo 5: Logout

```bash
curl -X POST "https://localhost:5001/api/v1/auth/logout" \
  -b cookies.txt
```

---

## Estrutura de Roles

Ao criar um usuário, ele recebe automaticamente o role **"Leitura"**.

### Roles Disponíveis
- **Leitura**: Pode visualizar dados
- **Financeiro**: Pode criar/editar transações
- **Admin**: Acesso total ao sistema

### Adicionar Roles Manualmente (via banco)

```sql
UPDATE users
SET roles = '["Admin", "Financeiro", "Leitura"]'::jsonb
WHERE email = 'test@example.com';
```

---

## Logs

Logs são gravados em:
- **Console**: Logs estruturados JSON
- **Arquivo**: `backend/src/L2SLedger.API/logs/l2sledger-YYYYMMDD.log`

---

## Próximos Passos

- [ ] Implementar testes unitários e de integração
- [ ] Configurar Docker Compose para desenvolvimento local
- [ ] Implementar módulo de categorias
- [ ] Implementar módulo de transações

---

## Problemas Comuns

### Erro: "Firebase credential não configurado"
- Certifique-se que o `Firebase:CredentialPath` está correto no `appsettings.Development.json`
- Verifique se o arquivo Firebase Service Account existe

### Erro: "Could not connect to PostgreSQL"
- Verifique se o PostgreSQL está rodando: `pg_isready`
- Confirme as credenciais na connection string

### Erro: "AUTH_EMAIL_NOT_VERIFIED"
- O usuário Firebase precisa ter o email verificado
- Marque "Email verified" no Firebase Console

---

## ADRs Relevantes

- **ADR-001**: Firebase como IdP único
- **ADR-002**: Fluxo de autenticação completo
- **ADR-004**: Cookies HttpOnly + Secure + SameSite=Lax
- **ADR-006**: PostgreSQL como banco de dados
- **ADR-016**: RBAC com 3 níveis (Admin, Financeiro, Leitura)
