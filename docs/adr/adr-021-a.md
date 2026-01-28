# ADR-021-A — Catálogo Completo de Códigos de Erro

## Status

Aprovado (Anexo ao ADR-021)

## Contexto

Este documento é um **anexo** ao ADR-021 (Modelo de Erros Semântico e Fail-Fast). Documenta o catálogo completo e atualizado de todos os códigos de erro implementados no sistema L2SLedger.

A fonte de verdade é o arquivo `L2SLedger.Domain/Constants/ErrorCodes.cs`.

---

## Catálogo de Códigos de Erro

### AUTH_ — Autenticação e Autorização

| Código | HTTP | Descrição |
|--------|------|-----------|
| `AUTH_INVALID_TOKEN` | 401 | Token Firebase inválido ou expirado |
| `AUTH_EMAIL_NOT_VERIFIED` | 400 | Email não verificado no Firebase |
| `AUTH_SESSION_EXPIRED` | 401 | Sessão expirada (cookie inválido) |
| `AUTH_UNAUTHORIZED` | 401 | Usuário não autenticado |
| `AUTH_USER_PENDING` | 403 | Usuário aguardando aprovação do Admin |
| `AUTH_USER_SUSPENDED` | 403 | Usuário suspenso temporariamente |
| `AUTH_USER_REJECTED` | 403 | Cadastro do usuário foi rejeitado |
| `AUTH_USER_INACTIVE` | 403 | Usuário inativo no sistema |
| `AUTH_USER_NOT_FOUND` | 404 | Usuário não encontrado no sistema |
| `AUTH_FIREBASE_ERROR` | 502 | Erro de comunicação com Firebase Auth |

---

### VAL_ — Validação de Dados

| Código | HTTP | Descrição |
|--------|------|-----------|
| `VAL_REQUIRED_FIELD` | 400 | Campo obrigatório não preenchido |
| `VAL_VALIDATION_FAILED` | 400 | Falha geral de validação (FluentValidation) |
| `VAL_INVALID_VALUE` | 400 | Valor inválido para o campo |
| `VAL_INVALID_FORMAT` | 400 | Formato inválido (ex: email, UUID) |
| `VAL_AMOUNT_NEGATIVE` | 400 | Valor monetário não pode ser negativo |
| `VAL_INVALID_DATE` | 400 | Data em formato inválido |
| `VAL_INVALID_RANGE` | 400 | Intervalo de datas inválido (início > fim ou período excede limite) |
| `VAL_DUPLICATE_NAME` | 400 | Nome duplicado (ex: categoria já existe) |
| `VAL_INVALID_REFERENCE` | 400 | Referência inválida entre entidades |
| `VAL_BUSINESS_RULE_VIOLATION` | 400 | Violação genérica de regra de negócio |

---

### FIN_ — Regras Financeiras e Domínio

| Código | HTTP | Descrição |
|--------|------|-----------|
| `FIN_PERIOD_CLOSED` | 422 | Operação bloqueada: período financeiro fechado |
| `FIN_INSUFFICIENT_BALANCE` | 422 | Saldo insuficiente para operação |
| `FIN_DUPLICATE_ENTRY` | 409 | Lançamento duplicado detectado |
| `FIN_INVALID_TRANSACTION` | 400 | Transação inválida |
| `FIN_PERIOD_NOT_FOUND` | 404 | Período financeiro não encontrado |
| `FIN_PERIOD_ALREADY_EXISTS` | 409 | Período financeiro já existe |
| `FIN_PERIOD_ALREADY_CLOSED` | 422 | Período já está fechado |
| `FIN_PERIOD_ALREADY_OPENED` | 422 | Período já está aberto |
| `FIN_PERIOD_INVALID_OPERATION` | 422 | Operação inválida para estado do período |
| `FIN_CATEGORY_NOT_FOUND` | 404 | Categoria financeira não encontrada |
| `FIN_CATEGORY_INVALID_NAME` | 400 | Nome de categoria vazio ou inválido |
| `FIN_CATEGORY_NAME_TOO_LONG` | 400 | Nome de categoria excede 100 caracteres |
| `FIN_TRANSACTION_NOT_FOUND` | 404 | Transação não encontrada |
| `FIN_ADJUSTMENT_NOT_FOUND` | 404 | Ajuste não encontrado |
| `FIN_ADJUSTMENT_PERIOD_CLOSED` | 422 | Período fechado impede ajuste |
| `FIN_ADJUSTMENT_INVALID_ORIGINAL` | 400 | Transação original do ajuste é inválida |
| `FIN_ADJUSTMENT_UNAUTHORIZED` | 403 | Usuário não autorizado para este ajuste |
| `FIN_ADJUSTMENT_ALREADY_DELETED` | 410 | Ajuste já foi excluído |

---

### PERM_ — Permissões

| Código | HTTP | Descrição |
|--------|------|-----------|
| `PERM_ACCESS_DENIED` | 403 | Acesso negado ao recurso |
| `PERM_ROLE_REQUIRED` | 403 | Role específica necessária |
| `PERM_INSUFFICIENT_PRIVILEGES` | 403 | Privilégios insuficientes para operação |

---

### USER_ — Gestão de Usuários

| Código | HTTP | Descrição |
|--------|------|-----------|
| `USER_NOT_FOUND` | 404 | Usuário não encontrado |
| `USER_INVALID_STATUS_TRANSITION` | 400 | Transição de status inválida |
| `USER_STATUS_REQUIRED` | 400 | Status é obrigatório |
| `USER_STATUS_REASON_REQUIRED` | 400 | Motivo da mudança de status é obrigatório |
| `USER_STATUS_REASON_TOO_LONG` | 400 | Motivo excede tamanho máximo |
| `USER_INVALID_STATUS` | 400 | Status informado é inválido |
| `USER_CANNOT_MODIFY_OWN_STATUS` | 400 | Usuário não pode alterar próprio status |
| `USER_CANNOT_REMOVE_OWN_ADMIN` | 400 | Admin não pode remover própria role de Admin |
| `USER_LAST_ADMIN` | 400 | Não pode remover último Admin do sistema |
| `USER_ROLES_REQUIRED` | 400 | Pelo menos uma role é obrigatória |
| `USER_ROLE_EMPTY` | 400 | Role não pode ser vazia |
| `USER_INVALID_ROLE` | 400 | Role informada é inválida |

---

### SYS_ — Erros de Sistema

| Código | HTTP | Descrição |
|--------|------|-----------|
| `SYS_INTERNAL_ERROR` | 500 | Erro interno não tratado |
| `SYS_UNAVAILABLE` | 503 | Sistema temporariamente indisponível |
| `SYS_CONFIGURATION_ERROR` | 500 | Erro de configuração do sistema |

---

### AUDIT_ — Auditoria

| Código | HTTP | Descrição |
|--------|------|-----------|
| `AUDIT_EVENT_NOT_FOUND` | 404 | Evento de auditoria não encontrado |

---

### INT_ — Integrações Externas

| Código | HTTP | Descrição |
|--------|------|-----------|
| `INT_FIREBASE_UNAVAILABLE` | 502 | Firebase Auth indisponível |
| `INT_DB_CONNECTION` | 503 | Falha de conexão com banco de dados |
| `INT_EXTERNAL_SERVICE_ERROR` | 502 | Erro em serviço externo |

---

### EXPORT_ — Exportações

| Código | HTTP | Descrição |
|--------|------|-----------|
| `EXPORT_NOT_FOUND` | 404 | Exportação não encontrada |
| `EXPORT_DELETE_UNAUTHORIZED` | 403 | Usuário não pode excluir esta exportação |
| `EXPORT_UNAUTHORIZED` | 403 | Acesso negado à exportação |
| `EXPORT_NOT_COMPLETED` | 400 | Exportação ainda não foi concluída |
| `EXPORT_NOT_READY` | 400 | Exportação não está pronta para download |
| `EXPORT_INVALID_STATE` | 400 | Estado inválido para operação de exportação |
| `EXPORT_INVALID_PARAMETERS` | 400 | Parâmetros de exportação inválidos |

---

## Estatísticas

| Categoria | Quantidade |
|-----------|------------|
| AUTH_ | 10 |
| VAL_ | 10 |
| FIN_ | 18 |
| PERM_ | 3 |
| USER_ | 12 |
| SYS_ | 3 |
| AUDIT_ | 1 |
| INT_ | 3 |
| EXPORT_ | 7 |
| **Total** | **67** |

---

## Mapeamento HTTP Status Codes

| Status | Uso |
|--------|-----|
| 400 Bad Request | Validação, regras de negócio, dados inválidos |
| 401 Unauthorized | Autenticação falhou ou ausente |
| 403 Forbidden | Autorização negada (permissões) |
| 404 Not Found | Recurso não encontrado |
| 409 Conflict | Conflito (duplicidade) |
| 410 Gone | Recurso foi removido |
| 422 Unprocessable Entity | Regra de domínio impede operação |
| 500 Internal Server Error | Erro interno do sistema |
| 502 Bad Gateway | Erro em serviço externo |
| 503 Service Unavailable | Sistema indisponível |

---

## Tratamento no Frontend

```typescript
const handleApiError = (error: ErrorResponse) => {
  const { code } = error.error;
  
  switch (code) {
    // Autenticação - redirecionar para login
    case 'AUTH_INVALID_TOKEN':
    case 'AUTH_SESSION_EXPIRED':
    case 'AUTH_UNAUTHORIZED':
      redirectToLogin();
      break;
    
    // Status do usuário - mostrar mensagem específica
    case 'AUTH_USER_PENDING':
      showMessage('Seu cadastro está aguardando aprovação.');
      break;
    case 'AUTH_USER_SUSPENDED':
      showMessage('Sua conta está suspensa. Contate o administrador.');
      break;
    case 'AUTH_USER_REJECTED':
      showMessage('Seu cadastro foi rejeitado.');
      break;
    
    // Validação - mostrar no formulário
    case 'VAL_REQUIRED_FIELD':
    case 'VAL_INVALID_FORMAT':
    case 'VAL_INVALID_DATE':
      showValidationError(error.error.message);
      break;
    
    // Regras financeiras - mostrar alerta
    case 'FIN_PERIOD_CLOSED':
      showAlert('Período fechado. Operação não permitida.');
      break;
    
    // Permissões - mostrar acesso negado
    case 'PERM_ACCESS_DENIED':
    case 'PERM_INSUFFICIENT_PRIVILEGES':
      showAccessDenied();
      break;
    
    // Sistema - mostrar erro genérico com traceId
    case 'SYS_INTERNAL_ERROR':
      showSystemError(error.error.traceId);
      break;
    
    default:
      showGenericError(error.error.message);
  }
};
```

---

## Referências

- [ADR-021](./adr-021.md) — Modelo de Erros Semântico e Fail-Fast
- [ErrorCodes.cs](../../backend/src/L2SLedger.Domain/Constants/ErrorCodes.cs) — Fonte de verdade
- [ErrorResponse.cs](../../backend/src/L2SLedger.API/Contracts/ErrorResponse.cs) — Estrutura de resposta

---

## Histórico

| Data | Versão | Descrição |
|------|--------|-----------|
| 2026-01-27 | 1.0 | Catálogo inicial com 67 códigos de erro |
