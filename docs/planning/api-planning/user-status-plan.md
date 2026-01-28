# Plano de Backend: Inclusão de Status do Usuário

> **Data:** 2026-01-25  
> **Status:** Aprovado  
> **Pré-requisito para:** Frontend Fase 1 (Autenticação)

---

## 🎯 Objetivo

Adicionar campo `Status` na entidade `User` para controlar o fluxo de aprovação de cadastros, permitindo que administradores aprovem, suspendam ou rejeitem usuários antes que possam acessar o sistema.

---

## 📋 Resumo Executivo

| Aspecto | Descrição |
|---------|-----------|
| **Escopo** | Adicionar status de aprovação para usuários |
| **Impacto** | Domain, Infrastructure, Application, API |
| **Breaking Changes** | ❌ Nenhum (alterações aditivas) |
| **Estimativa** | 10 horas |
| **Dependências** | Nenhuma |

---

## 📚 ADRs Impactados

| ADR | Impacto |
|-----|---------|
| ADR-001 | ✅ Compatível — Firebase continua como IdP, status é controle interno |
| ADR-002 | ✅ Compatível — Fluxo de login adiciona verificação de status |
| ADR-014 | ⚠️ Necessário — Mudanças de status devem gerar evento de auditoria |
| ADR-016 | ✅ Compatível — Admin gerencia status de usuários |
| ADR-020 | ✅ Compatível — Alteração segue Clean Architecture |
| ADR-021 | ⚠️ Necessário — Novos códigos de erro semânticos |
| ADR-022 | ⚠️ Atenção — Contratos de API serão alterados (adicionar campo) |
| ADR-035 | ⚠️ Necessário — Nova migration para banco de dados |

---

## 🔴 Análise de Breaking Changes

| Contrato | Mudança | Breaking? | Mitigação |
|----------|---------|-----------|-----------|
| `GET /api/v1/auth/me` | Adiciona campo `status` | ❌ Não | Campo adicional é backward-compatible |
| `POST /api/v1/auth/login` | Retorna `status` no user | ❌ Não | Campo adicional |
| `GET /api/v1/users` | Adiciona campo `status` | ❌ Não | Campo adicional |
| `GET /api/v1/users/{id}` | Adiciona campo `status` | ❌ Não | Campo adicional |
| `PUT /api/v1/users/{id}/status` | **Novo endpoint** | ❌ Não | Endpoint novo |

**Conclusão:** Nenhuma breaking change. Alterações são aditivas.

---

## 🧱 Alterações por Camada

### 1. Domain Layer

**Arquivo:** `backend/src/L2SLedger.Domain/Entities/User.cs`

| Alteração | Descrição |
|-----------|-----------|
| Criar `UserStatus` enum | `Pending`, `Active`, `Suspended`, `Rejected` |
| Adicionar propriedade `Status` | Tipo `UserStatus`, default `Pending` |
| Criar método `Approve()` | Muda status para `Active` |
| Criar método `Suspend()` | Muda status para `Suspended` |
| Criar método `Reject()` | Muda status para `Rejected` |
| Criar método `Reactivate()` | Muda de `Suspended` para `Active` |
| Adicionar validação | Transições de status válidas |

**Novo Arquivo:** `UserStatus.cs`

```csharp
namespace L2SLedger.Domain.Entities;

/// <summary>
/// Status de aprovação do usuário no sistema.
/// Conforme planejamento de integração Frontend-Backend.
/// </summary>
public enum UserStatus
{
    /// <summary>Aguardando aprovação do Admin.</summary>
    Pending = 0,
    
    /// <summary>Aprovado e pode acessar o sistema.</summary>
    Active = 1,
    
    /// <summary>Suspenso temporariamente.</summary>
    Suspended = 2,
    
    /// <summary>Cadastro rejeitado.</summary>
    Rejected = 3
}
```

**Transições de Status Válidas:**

```
┌─────────┐
│ Pending │
└────┬────┘
     │
     ├──────────────────┐
     ▼                  ▼
┌─────────┐       ┌──────────┐
│ Active  │       │ Rejected │
└────┬────┘       └──────────┘
     │                  ▲
     ▼                  │ (sem retorno)
┌───────────┐           │
│ Suspended │───────────┘
└─────┬─────┘
      │
      ▼
┌─────────┐
│ Active  │ (reativação)
└─────────┘
```

---

### 2. Infrastructure Layer

**Arquivo:** `backend/src/L2SLedger.Infrastructure/Persistence/Configurations/UserConfiguration.cs`

| Alteração | Descrição |
|-----------|-----------|
| Mapear `Status` | Coluna `status` como `integer` |
| Adicionar índice | `ix_users_status` para queries por status |

**Nova Migration:** `AddUserStatus.cs`

```sql
-- Adicionar coluna
ALTER TABLE users ADD COLUMN status integer NOT NULL DEFAULT 0;

-- Usuários existentes são automaticamente Active
UPDATE users SET status = 1 WHERE status = 0;

-- Criar índice
CREATE INDEX ix_users_status ON users (status);
```

**Atualização Repository:**

| Método | Alteração |
|--------|-----------|
| `GetAllAsync` | Adicionar parâmetro `statusFilter` |
| `GetPendingUsersCountAsync` | Novo método para contagem de pendentes |

---

### 3. Application Layer

**DTOs a Atualizar:**

| Arquivo | Alteração |
|---------|-----------|
| `UserDto.cs` | Adicionar `Status` como string |
| `CurrentUserResponse.cs` | Status já virá no UserDto |
| `UserDetailDto.cs` | Adicionar `Status` |
| `UserSummaryDto.cs` | Adicionar `Status` |

**Novos DTOs:**

```csharp
// UpdateUserStatusRequest.cs
public record UpdateUserStatusRequest
{
    /// <summary>Novo status do usuário.</summary>
    public required string Status { get; init; }
    
    /// <summary>Motivo obrigatório para a mudança.</summary>
    public required string Reason { get; init; }
}
```

**Use Cases a Criar:**

| Use Case | Responsabilidade |
|----------|------------------|
| `UpdateUserStatusUseCase` | Alterar status do usuário |

**Use Cases a Atualizar:**

| Use Case | Alteração |
|----------|-----------|
| `AuthenticationService.LoginAsync` | Bloquear login se status ≠ `Active` |
| `GetUsersUseCase` | Adicionar filtro por status |

**Lógica de Bloqueio no Login:**

```csharp
// Após validar Firebase token e buscar/criar usuário
if (user.Status != UserStatus.Active)
{
    var errorCode = user.Status switch
    {
        UserStatus.Pending => "AUTH_USER_PENDING",
        UserStatus.Suspended => "AUTH_USER_SUSPENDED",
        UserStatus.Rejected => "AUTH_USER_REJECTED",
        _ => "AUTH_USER_INACTIVE"
    };
    
    throw new AuthenticationException(errorCode, GetStatusMessage(user.Status));
}
```

---

### 4. API Layer

**Arquivo:** `AuthController.cs`

| Alteração | Descrição |
|-----------|-----------|
| `POST /auth/login` | Retornar 403 com código específico se status ≠ `Active` |

**Arquivo:** `UsersController.cs`

| Alteração | Descrição |
|-----------|-----------|
| `GET /users` | Adicionar parâmetro `?status=` |
| `PUT /users/{id}/status` | **Novo endpoint** para alterar status |

---

## 📡 Novos Endpoints

### `PUT /api/v1/users/{id}/status`

**Descrição:** Altera o status de um usuário.

**Autorização:** Apenas `Admin`

**Request:**

```json
{
  "status": "Active",
  "reason": "Cadastro aprovado após verificação de documentos"
}
```

**Valores válidos para `status`:**
- `Active` — Aprovar usuário
- `Suspended` — Suspender usuário
- `Rejected` — Rejeitar cadastro

**Response (200 OK):**

```json
{
  "id": "uuid",
  "email": "user@example.com",
  "displayName": "User Name",
  "status": "Active",
  "emailVerified": true,
  "roles": ["Leitura"],
  "createdAt": "2026-01-25T10:00:00Z",
  "updatedAt": "2026-01-25T12:00:00Z"
}
```

**Erros Possíveis:**

| Código | HTTP | Descrição |
|--------|------|-----------|
| `USER_NOT_FOUND` | 404 | Usuário não encontrado |
| `USER_INVALID_STATUS_TRANSITION` | 400 | Transição de status inválida |
| `USER_STATUS_REASON_REQUIRED` | 400 | Motivo obrigatório |
| `PERM_ACCESS_DENIED` | 403 | Apenas Admin pode alterar status |

---

### `GET /api/v1/users` (Atualização)

**Novo Query Parameter:**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `status` | string | Filtrar por status (`Pending`, `Active`, `Suspended`, `Rejected`) |

**Exemplo:**

```
GET /api/v1/users?status=Pending&page=1&pageSize=20
```

---

## 🚨 Novos Códigos de Erro (ADR-021)

| Código | HTTP | Descrição | Mensagem |
|--------|------|-----------|----------|
| `AUTH_USER_PENDING` | 403 | Usuário aguardando aprovação | "Seu cadastro está aguardando aprovação do administrador." |
| `AUTH_USER_SUSPENDED` | 403 | Usuário suspenso | "Sua conta foi suspensa. Entre em contato com o administrador." |
| `AUTH_USER_REJECTED` | 403 | Cadastro rejeitado | "Seu cadastro foi rejeitado. Entre em contato com o administrador." |
| `USER_INVALID_STATUS_TRANSITION` | 400 | Transição de status inválida | "Não é possível alterar o status de {oldStatus} para {newStatus}." |
| `USER_STATUS_REASON_REQUIRED` | 400 | Motivo obrigatório | "É obrigatório informar o motivo da alteração de status." |

---

## 📝 Auditoria (ADR-014)

Novos eventos a registrar:

| Evento | EntityType | Dados Extras |
|--------|------------|--------------|
| `UserStatusChanged` | `User` | `oldStatus`, `newStatus`, `reason` |

**Exemplo de AuditEvent:**

```json
{
  "eventType": "Update",
  "entityType": "User",
  "entityId": "user-uuid",
  "userId": "admin-uuid",
  "changes": {
    "status": {
      "old": "Pending",
      "new": "Active"
    }
  },
  "metadata": {
    "reason": "Cadastro aprovado após verificação"
  },
  "timestamp": "2026-01-25T12:00:00Z"
}
```

---

## 🧪 Testes Necessários

### Domain Tests

| Teste | Descrição |
|-------|-----------|
| `User_ShouldHaveDefaultStatus_Pending` | Novo usuário tem status Pending |
| `User_Approve_ShouldChangeStatusToActive` | Método Approve funciona |
| `User_Approve_FromSuspended_ShouldFail` | Não pode aprovar suspenso |
| `User_Suspend_ShouldChangeStatusToSuspended` | Método Suspend funciona |
| `User_Suspend_FromPending_ShouldFail` | Não pode suspender pendente |
| `User_Reject_ShouldChangeStatusToRejected` | Método Reject funciona |
| `User_Reject_FromActive_ShouldFail` | Não pode rejeitar ativo |
| `User_Reactivate_FromSuspended_ShouldWork` | Reativação funciona |
| `User_Reactivate_FromRejected_ShouldFail` | Não pode reativar rejeitado |
| `User_Reactivate_FromPending_ShouldFail` | Não pode reativar pendente |

### Application Tests

| Teste | Descrição |
|-------|-----------|
| `Login_WithPendingUser_ShouldReturn403` | Login bloqueado com código correto |
| `Login_WithSuspendedUser_ShouldReturn403` | Login bloqueado com código correto |
| `Login_WithRejectedUser_ShouldReturn403` | Login bloqueado com código correto |
| `Login_WithActiveUser_ShouldSucceed` | Login permitido |
| `Login_NewUser_ShouldHaveStatusPending` | Novo usuário criado com Pending |
| `UpdateUserStatus_ShouldCreateAuditEvent` | Auditoria registrada |
| `UpdateUserStatus_AsNonAdmin_ShouldFail` | Apenas admin |
| `UpdateUserStatus_InvalidTransition_ShouldFail` | Validação funciona |
| `GetUsers_FilterByStatus_ShouldWork` | Filtro funciona |

### API Tests

| Teste | Descrição |
|-------|-----------|
| `PUT_UserStatus_ShouldReturnUpdatedUser` | Endpoint funcional |
| `PUT_UserStatus_InvalidTransition_ShouldReturn400` | Validação funciona |
| `PUT_UserStatus_WithoutReason_ShouldReturn400` | Reason obrigatório |
| `PUT_UserStatus_AsNonAdmin_ShouldReturn403` | Autorização funciona |
| `GET_Users_FilterByStatus_ShouldWork` | Filtro funciona |
| `POST_Login_PendingUser_ShouldReturn403` | Bloqueio funciona |

### Contract Tests

| Teste | Descrição |
|-------|-----------|
| `AuthMe_Response_ShouldContainStatus` | Contrato atualizado |
| `LoginResponse_ShouldContainStatus` | Contrato atualizado |
| `UserDetail_Response_ShouldContainStatus` | Contrato atualizado |
| `UserSummary_Response_ShouldContainStatus` | Contrato atualizado |

---

## 📋 Lista de Tasks

### Fase 1: Domain (Estimativa: 2h)

| # | Task | Descrição | Dependência |
|---|------|-----------|-------------|
| 1.1 | Criar `UserStatus.cs` | Enum com Pending, Active, Suspended, Rejected | - |
| 1.2 | Atualizar `User.cs` | Adicionar propriedade Status | 1.1 |
| 1.3 | Criar métodos de transição | `Approve()`, `Suspend()`, `Reject()`, `Reactivate()` | 1.2 |
| 1.4 | Adicionar validações | Validar transições permitidas | 1.3 |
| 1.5 | Criar exceção `InvalidStatusTransitionException` | Exceção de domínio | 1.4 |
| 1.6 | Criar testes unitários | Testar entidade User | 1.5 |

### Fase 2: Infrastructure (Estimativa: 1h)

| # | Task | Descrição | Dependência |
|---|------|-----------|-------------|
| 2.1 | Atualizar `UserConfiguration.cs` | Mapear Status | 1.2 |
| 2.2 | Criar migration `AddUserStatus` | Adicionar coluna e índice | 2.1 |
| 2.3 | Atualizar `IUserRepository` | Adicionar filtro por status | 1.2 |
| 2.4 | Atualizar `UserRepository` | Implementar filtro | 2.3 |
| 2.5 | Testar migration | Executar em ambiente local | 2.2 |

### Fase 3: Application (Estimativa: 3h)

| # | Task | Descrição | Dependência |
|---|------|-----------|-------------|
| 3.1 | Atualizar `UserDto.cs` | Adicionar Status | 2.1 |
| 3.2 | Atualizar `UserDetailDto.cs` | Adicionar Status | 2.1 |
| 3.3 | Atualizar `UserSummaryDto.cs` | Adicionar Status | 2.1 |
| 3.4 | Criar `UpdateUserStatusRequest.cs` | Request DTO | 3.1 |
| 3.5 | Criar `UpdateUserStatusUseCase.cs` | Use Case | 3.4 |
| 3.6 | Atualizar `AuthenticationService.cs` | Bloquear login por status | 3.1 |
| 3.7 | Atualizar `GetUsersUseCase.cs` | Adicionar filtro status | 3.3 |
| 3.8 | Atualizar `GetUsersRequest.cs` | Adicionar parâmetro status | 3.7 |
| 3.9 | Adicionar eventos de auditoria | Em UpdateUserStatusUseCase | 3.5 |
| 3.10 | Atualizar AutoMapper profiles | Mapear Status | 3.3 |
| 3.11 | Criar testes de Use Cases | Testar lógica | 3.10 |

### Fase 4: API (Estimativa: 2h)

| # | Task | Descrição | Dependência |
|---|------|-----------|-------------|
| 4.1 | Adicionar códigos de erro | Em `ErrorCodes.cs` | 3.6 |
| 4.2 | Atualizar `AuthController.cs` | Tratar erros de status | 4.1 |
| 4.3 | Criar endpoint `PUT /users/{id}/status` | Em UsersController | 3.5 |
| 4.4 | Atualizar `GET /users` | Adicionar parâmetro status | 3.7 |
| 4.5 | Criar testes de API | Testar endpoints | 4.4 |

### Fase 5: Contract Tests (Estimativa: 1h)

| # | Task | Descrição | Dependência |
|---|------|-----------|-------------|
| 5.1 | Atualizar contract tests `/auth/me` | Verificar campo status | 4.2 |
| 5.2 | Atualizar contract tests `/auth/login` | Verificar campo status | 4.2 |
| 5.3 | Atualizar contract tests `/users` | Verificar campo status | 4.4 |
| 5.4 | Criar contract tests `/users/{id}/status` | Novo endpoint | 4.3 |

### Fase 6: Documentação (Estimativa: 1h)

| # | Task | Descrição | Dependência |
|---|------|-----------|-------------|
| 6.1 | Atualizar `frontend-api-integration-guide.md` | Documentar novo campo e endpoint | 5.4 |
| 6.2 | Atualizar `ai-driven/changelog.md` | Registrar alterações | 6.1 |

---

## ⏱️ Estimativa Total

| Fase | Tempo |
|------|-------|
| Domain | 2h |
| Infrastructure | 1h |
| Application | 3h |
| API | 2h |
| Contract Tests | 1h |
| Documentação | 1h |
| **Total** | **10h** |

---

## 🔄 Fluxo de Login Atualizado

```
┌─────────────────┐
│  POST /auth/login│
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│ 1. Validar Firebase Token               │
│ 2. Verificar email_verified             │
└────────┬────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│ 3. Buscar/Criar usuário interno         │
│    - Se novo: Status = Pending          │
│    - Se existente: manter status        │
└────────┬────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│ 4. Verificar Status                     │
└────────┬────────────────────────────────┘
         │
    ┌────┴────┬────────────┬────────────┐
    │         │            │            │
    ▼         ▼            ▼            ▼
┌───────┐ ┌────────┐ ┌──────────┐ ┌────────┐
│Active │ │Pending │ │Suspended │ │Rejected│
└───┬───┘ └────┬───┘ └────┬─────┘ └────┬───┘
    │          │          │            │
    ▼          ▼          ▼            ▼
┌────────┐ ┌──────────────────────────────┐
│ 200 OK │ │ 403 Forbidden                │
│ Cookie │ │ Código específico por status │
│ +User  │ │ AUTH_USER_PENDING            │
│        │ │ AUTH_USER_SUSPENDED          │
│        │ │ AUTH_USER_REJECTED           │
└────────┘ └──────────────────────────────┘
```

---

## ✅ Critérios de Aceite

| Critério | Validação |
|----------|-----------|
| Novo usuário tem status `Pending` | Teste unitário + integração |
| Login bloqueado para status ≠ `Active` | Teste de API |
| Erro retornado tem código específico por status | Teste de API |
| Admin pode alterar status | Teste de API com autenticação |
| Transições inválidas retornam erro 400 | Teste de validação |
| Mudanças de status são auditadas | Verificar tabela de auditoria |
| Contract tests passam | CI/CD |
| Cobertura ≥ 85% | CI/CD |
| Usuários existentes mantêm acesso (status=Active) | Verificar migration |

---

## 🚨 Riscos e Mitigações

| Risco | Impacto | Probabilidade | Mitigação |
|-------|---------|---------------|-----------|
| Usuários existentes sem status | Alto | Baixa | Migration define status=Active para existentes |
| Quebra de testes existentes | Médio | Média | Atualizar mocks e fixtures |
| Performance de queries | Baixo | Baixa | Índice em status |
| Admin não consegue aprovar primeiro usuário | Alto | Média | Seed cria admin com status Active |

---

## 📌 Observações Finais

1. **Pré-requisito:** Este plano deve ser executado **antes** da Fase 1 do frontend
2. **Migration:** Usuários existentes serão automaticamente marcados como `Active`
3. **Seed:** O usuário admin do seed deve ter status `Active`
4. **Rollback:** Se necessário, a migration pode ser revertida removendo a coluna

---

> **Este plano foi aprovado em 2026-01-25.**
> **Próximo passo:** Execução conforme fluxo Planejar → Aprovar → Executar.
