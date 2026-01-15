  # Guia de Teste Manual - Autenticação L2SLedger

Este guia fornece instruções passo a passo para testar manualmente o módulo de autenticação.

---

## Pré-requisitos

1. **.NET 9.0 SDK** instalado
2. **Docker Desktop** instalado e rodando
3. **Conta Firebase** configurada
4. **Postman** ou **curl** para testes de API

---

## Passo 1: Configurar PostgreSQL com Docker

### Iniciar PostgreSQL

```bash
cd c:\projects\projeto-financeiro\cash-flow\backend
docker-compose -f docker-compose.dev.yml up -d
```

### Verificar se está rodando

```powershell
docker ps
# Deve mostrar: l2sledger-postgres-dev

docker logs l2sledger-postgres-dev
# Deve mostrar: "database system is ready to accept connections"
```

### Testar conexão

```powershell
docker exec -it l2sledger-postgres-dev psql -U l2sledger -d l2sledger
```

Dentro do psql:
```sql
\dt  -- Listar tabelas (vazio antes da primeira migration)
\q   -- Sair
```

---

## Passo 2: Configurar Firebase

### 1. Criar/Acessar Projeto Firebase

1. Acesse https://console.firebase.google.com/
2. Crie um novo projeto ou use um existente
3. Nome sugerido: `l2sledger-dev`

### 2. Ativar Authentication

1. No menu lateral: **Authentication**
2. Clique em **Get Started**
3. Em **Sign-in method**, ative **Email/Password**
4. Marque **Email link (passwordless sign-in)** como DESABILITADO

### 3. Gerar Service Account Key

1. No ícone de engrenagem → **Project Settings**
2. Aba **Service Accounts**
3. Clique em **Generate new private key**
4. Salve o arquivo JSON em local seguro
5. **IMPORTANTE**: Anote o caminho completo do arquivo

### 4. Criar Usuário de Teste

1. Authentication → Users → **Add user**
2. Email: `admin@l2sledger.com`
3. Password: `Test@123456`
4. ✅ Marque **Email verified** (importante!)
5. Clique em **Add user**

---

## Passo 3: Configurar API

### Criar appsettings.Development.json

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

**⚠️ ATENÇÃO**: Substitua o `CredentialPath` pelo caminho real do arquivo Firebase!

### Aplicar Migrations

```powershell
cd c:\projects\projeto-financeiro\cash-flow\backend
dotnet ef database update --project src/L2SLedger.Infrastructure --startup-project src/L2SLedger.API
```

### Verificar tabelas criadas

```powershell
docker exec -it l2sledger-postgres-dev psql -U l2sledger -d l2sledger -c "\dt"
```

Deve mostrar:
```
               Lista de relações
 Esquema |        Nome        | Tipo  |   Dono    
---------+--------------------+-------+-----------
 public  | __EFMigrationsHistory | tabela | l2sledger
 public  | users              | tabela | l2sledger
```

---

## Passo 4: Iniciar a API

```powershell
cd c:\projects\projeto-financeiro\cash-flow\backend\src\L2SLedger.API
dotnet run
```

Aguarde até ver:
```
[INF] L2SLedger API iniciada com sucesso
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```

### Verificar Swagger

Abra no navegador: https://localhost:5001/swagger

Você deve ver os endpoints:
- `POST /api/v1/auth/login`
- `GET /api/v1/auth/me`
- `POST /api/v1/auth/logout`

---

## Passo 5: Obter Firebase ID Token

### Opção A: Usando Firebase REST API (Recomendado)

Substitua `SEU_FIREBASE_API_KEY` pela Web API Key do seu projeto:
- Firebase Console → Project Settings → General
- Seção "Your apps" → Web API Key

```powershell
$body = @{
    email = "admin@l2sledger.com"
    password = "Test@123456"
    returnSecureToken = $true
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=SEU_FIREBASE_API_KEY" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body

$idToken = $response.idToken
Write-Host "Firebase ID Token: $idToken"
```

**Guarde o idToken** retornado!

### Opção B: Usando curl

```bash
curl -X POST "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=SEU_FIREBASE_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@l2sledger.com",
    "password": "Test@123456",
    "returnSecureToken": true
  }'
```

---

## Passo 6: Testar Login

### Usando PowerShell

```powershell
$token = "COLE_SEU_FIREBASE_ID_TOKEN_AQUI"

$loginBody = @{
    firebaseIdToken = $token
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "https://localhost:5001/api/v1/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body $loginBody `
    -SessionVariable session `
    -SkipCertificateCheck

Write-Host "Status Code: $($response.StatusCode)"
Write-Host "Response Body:"
$response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10

# Cookies recebidos
Write-Host "`nCookies:"
$session.Cookies.GetCookies("https://localhost:5001")
```

**Resposta esperada** (200 OK):
```json
{
  "user": {
    "id": "guid-do-usuario",
    "email": "admin@l2sledger.com",
    "displayName": "admin@l2sledger.com",
    "roles": ["Leitura"],
    "createdAt": "2026-01-13T..."
  }
}
```

### Verificar no Banco de Dados

```powershell
docker exec -it l2sledger-postgres-dev psql -U l2sledger -d l2sledger -c "SELECT id, email, display_name, roles FROM users;"
```

Deve mostrar o usuário criado!

---

## Passo 7: Testar GET /auth/me

```powershell
$meResponse = Invoke-WebRequest -Uri "https://localhost:5001/api/v1/auth/me" `
    -Method GET `
    -WebSession $session `
    -SkipCertificateCheck

Write-Host "Status Code: $($meResponse.StatusCode)"
$meResponse.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
```

**Resposta esperada** (200 OK): Mesma estrutura do login

---

## Passo 8: Testar Logout

```powershell
$logoutResponse = Invoke-WebRequest -Uri "https://localhost:5001/api/v1/auth/logout" `
    -Method POST `
    -WebSession $session `
    -SkipCertificateCheck

Write-Host "Status Code: $($logoutResponse.StatusCode)"  # Esperado: 204 No Content
```

### Verificar que cookie foi removido

```powershell
# Tentar acessar /me novamente (deve falhar)
$meResponse2 = Invoke-WebRequest -Uri "https://localhost:5001/api/v1/auth/me" `
    -Method GET `
    -WebSession $session `
    -SkipCertificateCheck `
    -ErrorAction SilentlyContinue

# Se retornou 401 Unauthorized, o logout funcionou!
```

---

## Passo 9: Testar Cenários de Erro

### 1. Email não verificado

No Firebase Console, crie um novo usuário:
- Email: `unverified@test.com`
- Password: `Test@123`
- ❌ NÃO marque "Email verified"

Tente fazer login com este usuário. **Esperado: 400 Bad Request**

```json
{
  "error": {
    "code": "AUTH_EMAIL_NOT_VERIFIED",
    "message": "Email não verificado. Verifique seu email antes de fazer login.",
    "timestamp": "...",
    "traceId": "..."
  }
}
```

### 2. Token inválido

```powershell
$invalidLoginBody = @{
    firebaseIdToken = "token-invalido-fake"
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "https://localhost:5001/api/v1/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body $invalidLoginBody `
    -SkipCertificateCheck `
    -ErrorAction SilentlyContinue

# Esperado: 401 Unauthorized
```

### 3. Acesso sem autenticação

```powershell
$response = Invoke-WebRequest -Uri "https://localhost:5001/api/v1/auth/me" `
    -Method GET `
    -SkipCertificateCheck `
    -ErrorAction SilentlyContinue

# Esperado: 401 Unauthorized
```

---

## Passo 10: Testar Roles

### Adicionar role Admin ao usuário

```powershell
docker exec -it l2sledger-postgres-dev psql -U l2sledger -d l2sledger -c "UPDATE users SET roles = '[\"Leitura\", \"Admin\"]'::jsonb WHERE email = 'admin@l2sledger.com';"
```

### Fazer login novamente e verificar roles

```powershell
# Repetir Passo 6 e verificar que "roles" agora contém ["Leitura", "Admin"]
```

---

## Verificação de Sucesso

✅ **Todos os cenários devem passar:**

1. Login com usuário válido e email verificado → 200 OK + cookie
2. GET /me com cookie válido → 200 OK + dados do usuário
3. Logout → 204 No Content + cookie removido
4. GET /me após logout → 401 Unauthorized
5. Login com email não verificado → 400 Bad Request
6. Login com token inválido → 401 Unauthorized
7. Roles atribuídos corretamente → Verificado no banco e no response

---

## Limpeza

### Parar PostgreSQL

```powershell
docker-compose -f docker-compose.dev.yml down
```

### Limpar dados (CUIDADO!)

```powershell
docker-compose -f docker-compose.dev.yml down -v  # Remove volumes também
```

---

## Troubleshooting

### Erro: "Firebase credential não configurado"
- Verifique se o `CredentialPath` está correto no appsettings.Development.json
- Certifique-se que o arquivo JSON existe no caminho especificado

### Erro: "Could not connect to PostgreSQL"
- Verifique se o container está rodando: `docker ps`
- Tente reiniciar: `docker-compose -f docker-compose.dev.yml restart`

### Erro: "AUTH_EMAIL_NOT_VERIFIED"
- Vá no Firebase Console → Authentication → Users
- Clique no usuário e marque "Email verified"

### Erro 500 Internal Server Error
- Verifique os logs da API
- Verifique os logs do PostgreSQL: `docker logs l2sledger-postgres-dev`

---

## Próximos Testes

Após validar a autenticação, você pode:
1. Implementar testes de integração com Testcontainers
2. Criar testes de carga com k6 ou JMeter
3. Implementar módulo de categorias (Fase 3)
