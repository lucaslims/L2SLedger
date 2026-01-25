# Planejamento Técnico da API — L2SLedger

> **Status:** ✅ Completo - Pendencias de Pós-MVP  
> **Data:** 2026-01-11  
> **Versão:** 1.0  
> **Aprovado em:** 2026-01-11
> **Data de Execução:** 2026-01-11 a 2026-01-23
> **Responsável pela Execução:** @lucaslims

---

## 1. Visão Geral

Este documento apresenta o **planejamento técnico inicial da API do L2SLedger**, seguindo rigorosamente todos os ADRs aprovados. O planejamento serve como base para implementação por agentes de execução.

---

## 2. Arquitetura da API

### 2.1 Estrutura de Camadas (ADR-020)

```plaintext
backend/
├── src/
│   ├── L2SLedger.Domain/              # Camada de Domínio
│   │   ├── Entities/                  # Entidades financeiras
│   │   ├── ValueObjects/              # Value Objects
│   │   ├── Exceptions/                # Exceções de domínio
│   │   ├── Events/                    # Eventos de domínio
│   │   └── Interfaces/                # Contratos do domínio
│   │
│   ├── L2SLedger.Application/         # Camada de Aplicação
│   │   ├── UseCases/                  # Casos de uso
│   │   ├── DTOs/                      # DTOs de entrada/saída
│   │   ├── Interfaces/                # Contratos de infraestrutura
│   │   ├── Validators/                # Validadores (FluentValidation)
│   │   └── Mappers/                   # AutoMapper profiles
│   │
│   ├── L2SLedger.Infrastructure/      # Camada de Infraestrutura
│   │   ├── Persistence/               # Repositórios, DbContext
│   │   │   ├── Configurations/        # Entity configurations
│   │   │   ├── Migrations/            # EF Core migrations
│   │   │   └── Repositories/          # Implementações
│   │   ├── Identity/                  # Firebase integration
│   │   ├── Observability/             # Logs, métricas, tracing
│   │   └── Resilience/                # Polly policies
│   │
│   └── L2SLedger.API/                 # Camada de API
│       ├── Controllers/               # Controllers thin
│       ├── Middleware/                # Middlewares
│       ├── Filters/                   # Action filters
│       └── Contracts/                 # Request/Response contracts
│
├── tests/
│   ├── L2SLedger.Domain.Tests/
│   ├── L2SLedger.Application.Tests/
│   ├── L2SLedger.Infrastructure.Tests/
│   ├── L2SLedger.API.Tests/
│   └── L2SLedger.Contract.Tests/      # Testes de contrato
│
└── L2SLedger.sln
```

---

## 3. Módulos Funcionais da API

### 3.1 Módulo de Autenticação (ADR-001, ADR-002, ADR-003)

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/auth/login` | POST | Validar Firebase ID Token e criar sessão |
| `/api/v1/auth/logout` | POST | Encerrar sessão e invalidar cookie |
| `/api/v1/auth/me` | GET | Retornar usuário autenticado |
| `/api/v1/auth/firebase/login` | POST | 🔧 Login direto no Firebase (apenas DEV/DEMO) |

**Comportamentos:**

- Validação de Firebase ID Token via Firebase Admin SDK
- Verificação obrigatória de `email_verified`
- Criação de cookie HttpOnly + Secure + SameSite=Lax
- Registro em auditoria de login/logout

**Endpoint Auxiliar de Teste:**

O endpoint `/api/v1/auth/firebase/login` é uma ferramenta de desenvolvimento que permite obter um Firebase ID Token diretamente via email/senha, sem necessidade do frontend. 

- ⚠️ Disponível apenas em ambientes DEV/DEMO
- ⚠️ Retorna 404 em produção
- Útil para testes via cURL/Postman/Scripts
- Não deve ser usado em produção (usar Firebase SDK no frontend)

---

### 3.2 Módulo de Lançamentos Financeiros (ADR-014, ADR-015)

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/transactions` | GET | Listar lançamentos (paginado, filtrado) |
| `/api/v1/transactions/{id}` | GET | Obter lançamento por ID |
| `/api/v1/transactions` | POST | Criar novo lançamento |
| `/api/v1/transactions/{id}` | PUT | Atualizar lançamento (período aberto) |
| `/api/v1/transactions/{id}` | DELETE | Exclusão lógica (período aberto) |

**Regras de Negócio:**

- Período fechado bloqueia edição/exclusão (ADR-015)
- Toda operação é auditada (ADR-014)
- Exclusão é lógica, nunca física

---

### 3.3 Módulo de Categorias

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/categories` | GET | Listar categorias |
| `/api/v1/categories/{id}` | GET | Obter categoria por ID |
| `/api/v1/categories` | POST | Criar categoria |
| `/api/v1/categories/{id}` | PUT | Atualizar categoria |
| `/api/v1/categories/{id}` | DELETE | Desativar categoria |

---

### 3.4 Módulo de Períodos Financeiros (ADR-015)

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/periods` | GET | Listar períodos |
| `/api/v1/periods/{id}` | GET | Obter período por ID |
| `/api/v1/periods/{id}/close` | POST | Fechar período |
| `/api/v1/periods/{id}/reopen` | POST | Reabrir período (permissão Admin) |

**Regras:**

- Fechamento audita todos os saldos
- Reabertura requer permissão elevada e registro em auditoria

---

### 3.5 Módulo de Ajustes Pós-Fechamento (ADR-015)

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/adjustments` | GET | Listar ajustes |
| `/api/v1/adjustments` | POST | Criar ajuste (com referência ao original) |

**Regras:**

- Ajustes são novos lançamentos
- Devem referenciar lançamento original
- Devem conter justificativa obrigatória

---

### 3.6 Módulo de Saldos e Relatórios

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/balances` | GET | Obter saldos consolidados |
| `/api/v1/balances/daily` | GET | Saldos diários por período |
| `/api/v1/reports/cash-flow` | GET | Fluxo de caixa por período |

---

### 3.7 Módulo de Exportação (ADR-017)

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/exports/transactions` | POST | Solicitar exportação CSV/PDF |
| `/api/v1/exports/{id}/status` | GET | Consultar status da exportação |
| `/api/v1/exports/{id}/download` | GET | Download do arquivo |

**Regras:**

- Exportações grandes são assíncronas
- Permissão explícita requerida
- Auditoria obrigatória

---

### 3.8 Módulo de Auditoria (ADR-014, ADR-019)

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/audit/events` | GET | Listar eventos de auditoria |
| `/api/v1/audit/events/{id}` | GET | Detalhe de evento |
| `/api/v1/audit/access-logs` | GET | Logs de acesso e tentativas negadas |

**Regras:**

- Endpoints somente leitura
- Permissão Admin requerida
- Dados imutáveis

---

### 3.9 Módulo de Usuários e Permissões (ADR-016)

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/users` | GET | Listar usuários internos |
| `/api/v1/users/{id}` | GET | Obter usuário |
| `/api/v1/users/{id}/roles` | GET | Obter papéis do usuário |
| `/api/v1/users/{id}/roles` | PUT | Atualizar papéis (Admin) |

---

### 3.10 Módulo de Configurações

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/configurations` | GET | Obter configurações do sistema |
| `/api/v1/configurations` | PUT | Atualizar configurações (Admin) |
| `/api/v1/configurations/notifications` | GET | Obter configurações de notificações |
| `/api/v1/configurations/notifications` | PUT | Atualizar notificações (Admin) |

---

### 3.11 Módulo de Controle de Planos (ADR-042, ADR-042-a)

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/me/commercial-context` | GET | Obter contexto comercial do tenant |
| `/api/v1/me/plan` | GET | Obter plano ativo e features habilitadas |
| `/api/v1/me/usage` | GET | Obter limites e uso atual |
| `/api/v1/me/ads-permission` | GET | Verificar permissão para exibir anúncios |
| `/api/v1/me/upgrade` | POST | Solicitar upgrade de plano |
| `/api/v1/me/downgrade` | POST | Solicitar downgrade de plano |
| `/api/v1/me/cancel` | POST | Cancelar assinatura |

---

### 3.12 Módulo de controle de Pagamentos (futuro)

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/payments/charge` | POST | Cobrar assinatura (futuro) |
| `/api/v1/payments/me/billing-info` | GET | Obter informações de cobrança |
| `/api/v1/payments/me/billing-info` | PUT | Atualizar informações de cobrança |
| `/api/v1/payments/me/invoices` | GET | Listar faturas |
| `/api/v1/payments/me/invoices/{id}` | GET | Obter fatura por ID |
| `/api/v1/payments/me/payment-methods` | GET | Listar métodos de pagamento |
| `/api/v1/payments/me/payment-methods` | POST | Adicionar método de pagamento |
| `/api/v1/payments/me/payment-methods/{id}` | DELETE | Remover método de pagamento |

---

### 3.13 Health & Observabilidade (ADR-006)

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/health` | GET | Health check |
| `/health/ready` | GET | Readiness check |
| `/health/live` | GET | Liveness check |
| `/metrics` | GET | Métricas Prometheus (interno) |

---

## 4. Modelo de Erros (ADR-021)

### 4.1 Estrutura Padrão

```json
{
  "error": {
    "code": "FIN_PERIOD_CLOSED",
    "message": "Período financeiro está fechado",
    "details": "Período 2026-01 foi fechado em 2026-02-01",
    "timestamp": "2026-01-11T10:30:00Z",
    "traceId": "abc123-def456"
  }
}
```

### 4.2 Catálogo de Códigos

| Prefixo | Categoria | Exemplos |
|---------|-----------|----------|
| `AUTH_` | Autenticação | `AUTH_INVALID_TOKEN`, `AUTH_EMAIL_NOT_VERIFIED`, `AUTH_SESSION_EXPIRED` |
| `VAL_` | Validação | `VAL_REQUIRED_FIELD`, `VAL_INVALID_FORMAT`, `VAL_AMOUNT_NEGATIVE` |
| `FIN_` | Regras Financeiras | `FIN_PERIOD_CLOSED`, `FIN_INSUFFICIENT_BALANCE`, `FIN_DUPLICATE_ENTRY` |
| `PERM_` | Permissões | `PERM_ACCESS_DENIED`, `PERM_ROLE_REQUIRED` |
| `SYS_` | Sistema | `SYS_INTERNAL_ERROR`, `SYS_UNAVAILABLE` |
| `INT_` | Integrações | `INT_FIREBASE_UNAVAILABLE`, `INT_DB_CONNECTION` |

### 4.3 Mapeamento HTTP

| Código | HTTP Status |
|--------|-------------|
| `AUTH_*` | 401 / 403 |
| `VAL_*` | 400 |
| `FIN_*` | 422 |
| `PERM_*` | 403 |
| `SYS_*` | 500 |
| `INT_*` | 502 / 503 |

---

## 5. Segurança (ADR-001 a ADR-005, ADR-016)

### 5.1 Autenticação

- Firebase Authentication como único IdP
- Cookie de sessão: HttpOnly + Secure + SameSite=Lax
- Validação de token via Firebase Admin SDK
- Verificação obrigatória de `email_verified`

### 5.2 Autorização (RBAC/ABAC)

```
Papéis:
├── Admin      → Controle total
├── Financeiro → Lançamentos, ajustes, conciliação
└── Leitura    → Apenas visualização
```

### 5.3 Middleware de Segurança

```csharp
// Ordem de execução
app.UseAuthentication();           // Validar cookie
app.UseCorrelationId();            // Gerar/propagar TraceId
app.UseAuditLogging();             // Registrar acesso
app.UseAuthorization();            // Verificar permissões
app.UseExceptionHandler();         // Tratamento de erros
```

---

## 6. Observabilidade (ADR-006)

### 6.1 Logs Estruturados

```json
{
  "timestamp": "2026-01-11T10:30:00Z",
  "level": "Information",
  "traceId": "abc123",
  "userId": "user-456",
  "endpoint": "/api/v1/transactions",
  "method": "POST",
  "statusCode": 201,
  "durationMs": 45,
  "message": "Transaction created"
}
```

### 6.2 Métricas

- `http_requests_total{endpoint, method, status}`
- `http_request_duration_seconds{endpoint, method}`
- `db_connections_active`
- `auth_failures_total`

### 6.3 Tracing

- Propagação de `X-Correlation-Id` / `TraceId`
- Spans obrigatórios: HTTP, Database, Firebase

---

## 7. Resiliência (ADR-007)

### 7.1 Políticas Polly

```csharp
// Firebase calls
services.AddHttpClient<IFirebaseService>()
    .AddPolicyHandler(GetRetryPolicy())      // 3 retries, exponential backoff
    .AddPolicyHandler(GetCircuitBreaker())   // 5 failures → open 30s
    .AddPolicyHandler(GetTimeout(5));        // 5 seconds

// Database calls
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.CommandTimeout(3)));          // 3 seconds
```

---

## 8. Contratos e Versionamento (ADR-022, ADR-022-a)

### 8.1 Estratégia

- Versionamento via URL: `/api/v1/`, `/api/v2/`
- Contratos são imutáveis após publicados
- Quebras exigem nova versão + ADR

### 8.2 Compatibilidade

Considera-se **quebra de contrato**:

- Remoção de campos
- Alteração de tipo
- Mudança de significado semântico
- Alteração de códigos de erro

---

## 9. Banco de Dados (ADR-034, ADR-035)

### 9.1 Tabelas Principais

```sql
-- Core
Users
Categories
FinancialPeriods
Transactions

-- Audit
AuditEvents
AccessLogs

-- Export
ExportRequests
```

### 9.2 Migrations

- EF Core Migrations versionadas
- Execução via pipeline CI/CD
- Sem alterações manuais em produção

---

## 10. Testes (ADR-037, ADR-039)

### 10.1 Cobertura Obrigatória

| Camada | Tipo de Teste | Cobertura Mínima |
|--------|---------------|------------------|
| Domain | Unitário | 90% |
| Application | Unitário | 80% |
| Infrastructure | Integração | 70% |
| API | Contrato | 100% endpoints |

### 10.2 Testes de Contrato

```csharp
[Fact]
public async Task PostTransaction_ReturnsCreated_WithValidContract()
{
    // Valida estrutura de response
    // Valida códigos de erro
    // Valida versionamento
}
```

---

## 11. Checklist de Implementação

### Fase 1 — Estrutura Base

- [x] Criar solution e projetos (Clean Architecture)
- [x] Configurar EF Core + PostgreSQL
- [x] Configurar Firebase Admin SDK
- [x] Implementar middleware de autenticação
- [x] Implementar modelo de erros semântico
- [x] Configurar Serilog (logs estruturados)
- [ ] Configurar Polly (resiliência)


### Fase 2 — Módulo de Autenticação

- [x] Implementar `/api/v1/auth/login`
- [x] Implementar `/api/v1/auth/logout`
- [x] Implementar `/api/v1/auth/me`
- [x] Criar testes unitários e de integração
- [x] Criar testes de contrato

### Fase 3 — Módulo de Categorias

- [x] Criar entidade Category no Domain
- [x] Implementar CRUD de categorias
- [x] Criar testes

### Fase 3.1 — Endpoint Auxiliar de Teste (Firebase Login)

- [x] Implementar `/api/v1/auth/firebase/login` (apenas DEV/DEMO)
- [x] Criar IFirebaseAuthenticationService
- [x] Integrar com Firebase Authentication REST API
- [x] Adicionar validação de ambiente (IsProduction → 404)
- [x] Criar testes (12 testes)
- [ ] Atualizar documentação Swagger com grupo "dev"

### Fase 4 — Módulo de Lançamentos

- [x] Criar entidade Transaction no Domain
- [x] Implementar regras de período (aberto/fechado)
- [x] Implementar auditoria automática
- [x] Criar testes incluindo regressão financeira

### Fase 5 — Módulo de Períodos

- [x] Criar entidade FinancialPeriod
- [x] Implementar fechamento/reabertura
- [x] Criar testes

### Fase 6 — Módulos Complementares

- [x] Saldos e relatórios
- [~] Exportação (CSV/PDF)
- [x] Auditoria (consulta)
- [x] Usuários e permissões

### Fase 7 — Observabilidade e DevOps

- [x] Health checks
- [x] Métricas Prometheus
- [x] Tracing distribuído
- [ ] Dockerfile e docker-compose

### Fase 8 — Controle de Planos e Comercial (Pós-MVP)

- [ ] Implementar `/api/v1/commercial/me/context`
- [ ] Implementar endpoints de plano, uso, anúncios
- [ ] Criar testes de contrato

### Fase 9 — Testes Finais e Documentação (Pós-MVP)

- [ ] Cobertura mínima de testes
- [ ] Testes de contrato completos
- [ ] Documentação Swagger atualizada
- [ ] Revisão de código e segurança
- [ ] Preparar release notes
- [ ] Atualizar documentação técnica
- [ ] Criar guia de implantação
- [ ] Criar documentação para Frontend visando integração com API usando IA

---

## 12. Tecnologias e Dependências

| Categoria | Tecnologia |
| --- | --- |
| Runtime | .NET 10 |
| Framework | ASP.NET Core Minimal APIs ou Controllers |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL 17 |
| Auth | Firebase Admin SDK |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Resilience | Polly |
| Logging | Serilog |
| Metrics | Prometheus-net |
| Testing | xUnit, FluentAssertions, Testcontainers |

---

## 13. ADRs Aplicados

| ADR | Aplicação |
|-----|-----------|
| ADR-001/002/003 | Autenticação Firebase + Cookies |
| ADR-004/005 | Segurança de cookies e autorização |
| ADR-006 | Observabilidade obrigatória |
| ADR-007 | Resiliência (timeout, retry, circuit breaker) |
| ADR-014 | Auditoria financeira obrigatória |
| ADR-015 | Imutabilidade e fechamento de períodos |
| ADR-016 | RBAC/ABAC |
| ADR-017 | Exportação controlada |
| ADR-019 | Auditoria de acessos negados |
| ADR-020 | Clean Architecture + DDD |
| ADR-021 | Modelo de erros semântico fail-fast |
| ADR-022/022-a | Contratos imutáveis e versionamento |
| ADR-034 | PostgreSQL como fonte única |
| ADR-035 | Migrations versionadas |
| ADR-037/039 | Testes automatizados e de contrato |

---

## 14. Próximos Passos

Este planejamento deve ser **aprovado** antes da execução, seguindo o fluxo:

```
Planejar → Aprovar → Executar
```

Após aprovação, os agentes de execução devem:

1. Seguir o checklist de implementação
2. Atualizar testes conforme cada módulo
3. Atualizar documentação quando aplicável
4. Registrar execuções em `ai-driven/changelog.md`

---

## 15. Histórico de Revisões

| Data | Versão | Descrição |
|------|--------|-----------|
| 2026-01-11 | 1.0 | Versão inicial do planejamento || 2026-01-11 | 1.0 | ✅ Planejamento aprovado - Iniciando execução |
---

> **Nota:** Este planejamento foi elaborado em conformidade com todos os ADRs do L2SLedger. Qualquer desvio requer novo ADR ou aprovação explícita.
