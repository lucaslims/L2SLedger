---
title: Planejamento Técnico — Fase 7: Saldos e Relatórios
date: 2026-01-18
version: 1.0
dependencies:
  - Fase 1, 2, 3, 4, 5 e 6 concluídas
status: Aprovado
---

# Fase 7: Saldos e Relatórios — L2SLedger

## 📋 Contexto

A **Fase 7** implementa funcionalidades de consulta e visualização de dados financeiros consolidados, permitindo que usuários vejam:

- **Saldos consolidados** por período e categoria
- **Evolução diária** de saldos (day-by-day)
- **Relatórios de fluxo de caixa** com abertura/fechamento

### Status Atual do Projeto

| Fase | Módulo | Testes | Status |
|------|--------|--------|--------|
| 1 | Estrutura Base | 6 | ✅ |
| 2 | Autenticação (Firebase) | 31 | ✅ |
| 3 | Categorias (CRUD + Seed) | 53 | ✅ |
| 4 | Transações (CRUD + Filtros) | 37 | ✅ |
| 5 | Períodos Financeiros | 84 | ✅ |
| 6 | Ajustes Pós-Fechamento | 44 | ✅ |
| **Total** | | **255** | ✅ |

Stack: .NET 9.0, PostgreSQL 17, Firebase Auth, EF Core 9.0, Serilog, AutoMapper, FluentValidation, xUnit

---

## 🎯 Objetivos da Fase 7

### Funcionalidades a Implementar

1. **Saldos Consolidados**
   - Total de receitas, despesas e saldo líquido
   - Agrupamento por categoria
   - Filtros por período

2. **Saldos Diários**
   - Evolução day-by-day dos saldos
   - Saldo de abertura e fechamento por dia
   - Visualização de fluxo diário

3. **Relatório de Fluxo de Caixa**
   - Saldo inicial do período
   - Lista de movimentações ordenadas
   - Saldo final e variação líquida

### Valor de Negócio

- **UX**: Usuários visualizam situação financeira de forma consolidada
- **Decisão**: Dados agregados auxiliam tomada de decisão
- **Performance**: Queries otimizadas para grandes volumes

---

## 📐 ADRs Relacionados

| ADR | Título | Impacto na Fase 7 |
|-----|--------|-------------------|
| **ADR-020** | Clean Architecture e DDD | Organização dos Use Cases de relatórios na Application Layer |
| **ADR-034** | PostgreSQL como Fonte Única | Queries agregadas nativas do PostgreSQL para performance |
| **ADR-006** | Observabilidade e Métricas | Monitoramento de performance das queries de relatórios |
| **ADR-016** | RBAC | Autorização Admin/Financeiro para endpoints de relatórios |
| **ADR-021** | Modelo de Erros Semântico | Validações de datas e tratamento de erros |

---

## 🏗️ Arquitetura da Solução

### Visão Geral

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │ GET /api/v1/balances
       │ GET /api/v1/balances/daily
       │ GET /api/v1/reports/cash-flow
       ▼
┌─────────────────────────────────┐
│      API Layer                   │
│  - BalancesController            │
│  - ReportsController             │
└──────────┬──────────────────────┘
           │
           ▼
┌─────────────────────────────────┐
│   Application Layer              │
│  - GetBalanceUseCase             │
│  - GetDailyBalanceUseCase        │
│  - GetCashFlowReportUseCase      │
│  - DTOs (5 arquivos)             │
└──────────┬──────────────────────┘
           │
           ▼
┌─────────────────────────────────┐
│   Infrastructure Layer           │
│  - ITransactionRepository        │
│  - Queries agregadas SQL         │
│  - Índices otimizados            │
└─────────────────────────────────┘
```

### Fluxo de Dados

1. **Cliente** → Solicita relatório via API
2. **Controller** → Valida autorização e parâmetros
3. **Use Case** → Orquestra lógica de negócio
4. **Repository** → Executa queries agregadas
5. **Use Case** → Mapeia para DTOs
6. **Controller** → Retorna JSON

---

## 📦 Componentes a Implementar

### Application Layer - DTOs (5 arquivos)

#### 1. BalanceSummaryDto.cs

```csharp
namespace L2SLedger.Application.DTOs.Balances;

/// <summary>
/// DTO com saldos consolidados por período.
/// </summary>
public class BalanceSummaryDto
{
    /// <summary>
    /// Total de receitas no período.
    /// </summary>
    public decimal TotalIncome { get; set; }

    /// <summary>
    /// Total de despesas no período.
    /// </summary>
    public decimal TotalExpense { get; set; }

    /// <summary>
    /// Saldo líquido (receitas - despesas).
    /// </summary>
    public decimal NetBalance { get; set; }

    /// <summary>
    /// Data inicial do período consultado.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Data final do período consultado.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Saldos agrupados por categoria.
    /// </summary>
    public List<CategoryBalanceDto> ByCategory { get; set; } = new();
}
```

#### 2. CategoryBalanceDto.cs

```csharp
namespace L2SLedger.Application.DTOs.Balances;

/// <summary>
/// Saldo de uma categoria específica.
/// </summary>
public class CategoryBalanceDto
{
    /// <summary>
    /// ID da categoria.
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Nome da categoria.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Total de receitas da categoria.
    /// </summary>
    public decimal Income { get; set; }

    /// <summary>
    /// Total de despesas da categoria.
    /// </summary>
    public decimal Expense { get; set; }

    /// <summary>
    /// Saldo líquido da categoria.
    /// </summary>
    public decimal NetBalance { get; set; }
}
```

#### 3. DailyBalanceDto.cs

```csharp
namespace L2SLedger.Application.DTOs.Balances;

/// <summary>
/// Saldo de um dia específico.
/// </summary>
public class DailyBalanceDto
{
    /// <summary>
    /// Data do saldo.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Saldo de abertura do dia.
    /// </summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// Total de receitas do dia.
    /// </summary>
    public decimal Income { get; set; }

    /// <summary>
    /// Total de despesas do dia.
    /// </summary>
    public decimal Expense { get; set; }

    /// <summary>
    /// Saldo de fechamento do dia.
    /// </summary>
    public decimal ClosingBalance { get; set; }
}
```

#### 4. CashFlowReportDto.cs

```csharp
namespace L2SLedger.Application.DTOs.Reports;

/// <summary>
/// Relatório de fluxo de caixa.
/// </summary>
public class CashFlowReportDto
{
    /// <summary>
    /// Data inicial do período.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Data final do período.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Saldo de abertura do período.
    /// </summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// Lista de movimentações ordenadas por data.
    /// </summary>
    public List<MovementDto> Movements { get; set; } = new();

    /// <summary>
    /// Saldo de fechamento do período.
    /// </summary>
    public decimal ClosingBalance { get; set; }

    /// <summary>
    /// Variação líquida (fechamento - abertura).
    /// </summary>
    public decimal NetChange { get; set; }
}
```

#### 5. MovementDto.cs

```csharp
namespace L2SLedger.Application.DTOs.Reports;

/// <summary>
/// Movimentação individual no fluxo de caixa.
/// </summary>
public class MovementDto
{
    /// <summary>
    /// Data da movimentação.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Descrição da transação.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Nome da categoria.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Valor da movimentação (positivo para receita, negativo para despesa).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Tipo da transação (Income ou Expense).
    /// </summary>
    public string Type { get; set; } = string.Empty;
}
```

---

### Application Layer - Use Cases (3 arquivos)

#### 1. GetBalanceUseCase.cs

**Responsabilidade**: Calcular saldos consolidados por período e categoria

**Input**:
- UserId (obrigatório)
- StartDate (opcional, default: início do mês atual)
- EndDate (opcional, default: hoje)
- CategoryId (opcional, para filtrar por categoria)

**Output**: `BalanceSummaryDto`

**Lógica**:
1. Validar datas (StartDate ≤ EndDate)
2. Query agregada:
   ```sql
   SELECT 
     category_id, 
     type, 
     SUM(amount) as total
   FROM transactions
   WHERE user_id = @userId 
     AND transaction_date BETWEEN @start AND @end
     AND is_deleted = false
     AND (@categoryId IS NULL OR category_id = @categoryId)
   GROUP BY category_id, type
   ```
3. Processar resultados e agrupar por categoria
4. Calcular totais gerais (TotalIncome, TotalExpense, NetBalance)

**Regras de Negócio**:
- Apenas transações não deletadas
- Apenas transações do usuário autenticado
- StartDate e EndDate são inclusivos
- Se CategoryId informado, filtra apenas essa categoria

---

#### 2. GetDailyBalanceUseCase.cs

**Responsabilidade**: Retornar evolução diária de saldos

**Input**:
- UserId (obrigatório)
- StartDate (obrigatório)
- EndDate (obrigatório)

**Output**: `List<DailyBalanceDto>`

**Lógica**:
1. Validar período (máximo 365 dias)
2. Calcular saldo de abertura (transações antes de StartDate)
3. Query agregada por dia:
   ```sql
   SELECT 
     DATE(transaction_date) as date,
     type,
     SUM(amount) as total
   FROM transactions
   WHERE user_id = @userId
     AND transaction_date BETWEEN @start AND @end
     AND is_deleted = false
   GROUP BY date, type
   ORDER BY date
   ```
4. Calcular saldos acumulados dia a dia
5. Preencher dias sem movimentação (opcional)

**Regras de Negócio**:
- OpeningBalance = saldo acumulado até StartDate - 1 dia
- ClosingBalance = OpeningBalance + Income - Expense
- Cada dia subsequente usa ClosingBalance do dia anterior como OpeningBalance

---

#### 3. GetCashFlowReportUseCase.cs

**Responsabilidade**: Gerar relatório de fluxo de caixa com movimentações

**Input**:
- UserId (obrigatório)
- StartDate (obrigatório)
- EndDate (obrigatório)

**Output**: `CashFlowReportDto`

**Lógica**:
1. Validar período (máximo 90 dias para performance)
2. Calcular OpeningBalance (saldo antes do período)
3. Query de movimentações:
   ```sql
   SELECT 
     t.transaction_date,
     t.description,
     c.name as category_name,
     t.amount,
     t.type
   FROM transactions t
   INNER JOIN categories c ON t.category_id = c.id
   WHERE t.user_id = @userId
     AND t.transaction_date BETWEEN @start AND @end
     AND t.is_deleted = false
   ORDER BY t.transaction_date, t.created_at
   ```
4. Mapear para MovementDto (amount negativo se Expense)
5. Calcular ClosingBalance e NetChange

**Regras de Negócio**:
- Movimentos ordenados por data e hora de criação
- Amount positivo para Income, negativo para Expense
- NetChange = ClosingBalance - OpeningBalance

---

### Infrastructure Layer - Queries

**Nota**: Usar repositórios existentes (`ITransactionRepository`) com novos métodos para queries agregadas.

**Novos métodos sugeridos**:

```csharp
Task<Dictionary<(Guid CategoryId, TransactionType Type), decimal>> GetBalanceByCategoryAsync(
    Guid userId, 
    DateTime startDate, 
    DateTime endDate, 
    Guid? categoryId = null
);

Task<decimal> GetBalanceBeforeDateAsync(
    Guid userId, 
    DateTime beforeDate
);

Task<Dictionary<DateTime, (decimal Income, decimal Expense)>> GetDailyBalancesAsync(
    Guid userId, 
    DateTime startDate, 
    DateTime endDate
);
```

**Índices Existentes** (validar se são suficientes):
- `IX_transactions_user_date` (user_id, transaction_date)
- `IX_transactions_category` (category_id)

---

### API Layer - Controllers (2 arquivos)

#### 1. BalancesController.cs

```csharp
[ApiController]
[Route("api/v1/balances")]
[Authorize]
public class BalancesController : ControllerBase
{
    /// <summary>
    /// GET /api/v1/balances
    /// Retorna saldos consolidados do período.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Financeiro")]
    public async Task<ActionResult<BalanceSummaryDto>> GetBalance(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? categoryId
    )

    /// <summary>
    /// GET /api/v1/balances/daily
    /// Retorna saldos diários do período.
    /// </summary>
    [HttpGet("daily")]
    [Authorize(Roles = "Admin,Financeiro")]
    public async Task<ActionResult<List<DailyBalanceDto>>> GetDailyBalance(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate
    )
}
```

#### 2. ReportsController.cs

```csharp
[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    /// <summary>
    /// GET /api/v1/reports/cash-flow
    /// Retorna relatório de fluxo de caixa.
    /// </summary>
    [HttpGet("cash-flow")]
    [Authorize(Roles = "Admin,Financeiro")]
    public async Task<ActionResult<CashFlowReportDto>> GetCashFlowReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate
    )
}
```

---

## 🧪 Estratégia de Testes

### Distribuição de Testes (~35 testes)

| Tipo | Quantidade | Descrição |
|------|------------|-----------|
| **Application Tests** | 20 | Use Cases com mocks |
| **Contract Tests** | 15 | Estrutura de DTOs |
| **Total** | **35** | |

### Application Tests - GetBalanceUseCaseTests (7 testes)

1. `GetBalance_WithValidPeriod_ReturnsCorrectSummary()`
   - Cenário: 3 receitas, 2 despesas em 2 categorias
   - Assert: TotalIncome, TotalExpense, NetBalance, ByCategory corretos

2. `GetBalance_WithCategoryFilter_ReturnsOnlyThatCategory()`
   - Cenário: Filtrar por categoria específica
   - Assert: Retorna apenas saldos da categoria filtrada

3. `GetBalance_WithNullDates_UsesCurrentMonth()`
   - Cenário: StartDate e EndDate null
   - Assert: Usa primeiro dia do mês atual até hoje

4. `GetBalance_WithNoTransactions_ReturnsZeroBalances()`
   - Cenário: Usuário sem transações no período
   - Assert: TotalIncome = 0, TotalExpense = 0, ByCategory vazio

5. `GetBalance_WithInvalidDates_ThrowsValidationException()`
   - Cenário: StartDate > EndDate
   - Assert: Lança ValidationException

6. `GetBalance_ExcludesDeletedTransactions()`
   - Cenário: 2 transações ativas, 1 deletada
   - Assert: Considera apenas as 2 ativas

7. `GetBalance_OnlyIncludesUserTransactions()`
   - Cenário: Transações de múltiplos usuários
   - Assert: Retorna apenas do usuário autenticado

### Application Tests - GetDailyBalanceUseCaseTests (6 testes)

1. `GetDailyBalance_WithValidPeriod_ReturnsCorrectDailyBalances()`
   - Cenário: 3 dias com transações
   - Assert: OpeningBalance, Income, Expense, ClosingBalance corretos para cada dia

2. `GetDailyBalance_WithNoPreviousTransactions_StartsWithZero()`
   - Cenário: Primeira transação do usuário
   - Assert: Primeiro dia tem OpeningBalance = 0

3. `GetDailyBalance_OrdersByDate_Ascending()`
   - Cenário: Transações em ordem aleatória
   - Assert: Resultado ordenado por Date crescente

4. `GetDailyBalance_WithPeriodTooLong_ThrowsValidationException()`
   - Cenário: Período > 365 dias
   - Assert: Lança ValidationException

5. `GetDailyBalance_CalculatesAccumulatedBalances()`
   - Cenário: 3 dias consecutivos
   - Assert: ClosingBalance do dia N = OpeningBalance do dia N+1

6. `GetDailyBalance_ExcludesDeletedTransactions()`
   - Cenário: Transações deletadas no período
   - Assert: Não considera transações deletadas

### Application Tests - GetCashFlowReportUseCaseTests (7 testes)

1. `GetCashFlowReport_WithValidPeriod_ReturnsCompleteReport()`
   - Cenário: Período com várias transações
   - Assert: OpeningBalance, Movements, ClosingBalance, NetChange corretos

2. `GetCashFlowReport_OrdersMovementsByDate()`
   - Cenário: Transações em ordem aleatória
   - Assert: Movements ordenadas por Date

3. `GetCashFlowReport_CalculatesOpeningBalance()`
   - Cenário: Transações antes do período
   - Assert: OpeningBalance = saldo acumulado antes de StartDate

4. `GetCashFlowReport_CalculatesNetChange()`
   - Cenário: Receitas e despesas no período
   - Assert: NetChange = ClosingBalance - OpeningBalance

5. `GetCashFlowReport_FormatsAmountsByType()`
   - Cenário: Receitas e despesas
   - Assert: Income com amount positivo, Expense com amount negativo

6. `GetCashFlowReport_WithPeriodTooLong_ThrowsValidationException()`
   - Cenário: Período > 90 dias
   - Assert: Lança ValidationException

7. `GetCashFlowReport_IncludesCategoryNames()`
   - Cenário: Transações em múltiplas categorias
   - Assert: Movements incluem Category name correto

### Contract Tests - BalanceContractTests (15 testes)

1. `BalanceSummaryDto_ShouldHaveRequiredStructure()` - 6 propriedades
2. `BalanceSummaryDto_ShouldSerializeCorrectly()` - JSON camelCase
3. `CategoryBalanceDto_ShouldHaveRequiredStructure()` - 5 propriedades
4. `CategoryBalanceDto_ShouldSerializeCorrectly()` - JSON camelCase
5. `DailyBalanceDto_ShouldHaveRequiredStructure()` - 5 propriedades
6. `DailyBalanceDto_ShouldSerializeCorrectly()` - JSON camelCase
7. `DailyBalanceDto_CalculatesClosingBalance()` - OpeningBalance + Income - Expense
8. `CashFlowReportDto_ShouldHaveRequiredStructure()` - 6 propriedades
9. `CashFlowReportDto_ShouldSerializeCorrectly()` - JSON camelCase
10. `CashFlowReportDto_CalculatesNetChange()` - ClosingBalance - OpeningBalance
11. `MovementDto_ShouldHaveRequiredStructure()` - 5 propriedades
12. `MovementDto_ShouldSerializeCorrectly()` - JSON camelCase
13. `MovementDto_IncomeAmount_ShouldBePositive()` - Validação de sinal
14. `MovementDto_ExpenseAmount_ShouldBeNegative()` - Validação de sinal
15. `MovementDto_ShouldIncludeCategoryName()` - Propriedade obrigatória

---

## ⚡ Otimizações de Performance

### Índices Existentes

Validar se os índices atuais são suficientes:

```sql
-- Já existe (confirmar)
CREATE INDEX IX_transactions_user_date 
ON transactions(user_id, transaction_date);

CREATE INDEX IX_transactions_category 
ON transactions(category_id);
```

### Queries Otimizadas

**1. Balance Summary**
```sql
SELECT 
    t.category_id,
    c.name as category_name,
    t.type,
    SUM(t.amount) as total
FROM transactions t
INNER JOIN categories c ON t.category_id = c.id
WHERE t.user_id = @userId 
  AND t.transaction_date BETWEEN @startDate AND @endDate
  AND t.is_deleted = false
GROUP BY t.category_id, c.name, t.type;
```

**2. Daily Balances**
```sql
SELECT 
    DATE(transaction_date) as date,
    type,
    SUM(amount) as total
FROM transactions
WHERE user_id = @userId
  AND transaction_date BETWEEN @startDate AND @endDate
  AND is_deleted = false
GROUP BY date, type
ORDER BY date;
```

**3. Opening Balance**
```sql
SELECT 
    COALESCE(
        SUM(CASE WHEN type = 1 THEN amount ELSE 0 END) -
        SUM(CASE WHEN type = 2 THEN amount ELSE 0 END),
        0
    ) as opening_balance
FROM transactions
WHERE user_id = @userId
  AND transaction_date < @startDate
  AND is_deleted = false;
```

### Limites de Performance

- **Balance Summary**: Sem limite de período (query simples GROUP BY)
- **Daily Balance**: Máximo 365 dias (para evitar payload muito grande)
- **Cash Flow Report**: Máximo 90 dias (lista todas as transações, sem agregação)

---

## 📝 Checklist de Implementação

### Phase 1: DTOs (5 arquivos)
- [ ] BalanceSummaryDto.cs
- [ ] CategoryBalanceDto.cs
- [ ] DailyBalanceDto.cs
- [ ] CashFlowReportDto.cs
- [ ] MovementDto.cs

### Phase 2: Use Cases (3 arquivos)
- [ ] GetBalanceUseCase.cs
- [ ] GetDailyBalanceUseCase.cs
- [ ] GetCashFlowReportUseCase.cs

### Phase 3: Infrastructure (1 arquivo)
- [ ] Adicionar métodos em ITransactionRepository (ou criar IBalanceRepository)

### Phase 4: API (2 arquivos)
- [ ] BalancesController.cs
- [ ] ReportsController.cs
- [ ] Registrar Use Cases no DI

### Phase 5: Testes (4 arquivos)
- [ ] GetBalanceUseCaseTests.cs (7 testes)
- [ ] GetDailyBalanceUseCaseTests.cs (6 testes)
- [ ] GetCashFlowReportUseCaseTests.cs (7 testes)
- [ ] BalanceContractTests.cs (15 testes)

### Phase 6: Validação
- [ ] Rodar suite completa de testes (~290 testes)
- [ ] Validar performance das queries (< 100ms para datasets típicos)
- [ ] Testar endpoints via Postman/curl
- [ ] Verificar logs de SQL gerado pelo EF Core

### Phase 7: Documentação
- [ ] Atualizar ai-driven/changelog.md
- [ ] Atualizar backend/STATUS.md
- [ ] Adicionar exemplos de uso no README (opcional)

---

## 🎯 Critérios de Aceitação

### Funcional

- ✅ 3 endpoints REST implementados e funcionais
- ✅ Saldos calculados corretamente (validado por testes)
- ✅ Filtros de período e categoria funcionando
- ✅ Autorização correta (Admin, Financeiro)
- ✅ Erros semânticos com códigos apropriados

### Qualidade

- ✅ ~35 testes implementados e passando
- ✅ Total de testes do projeto: ~290
- ✅ Cobertura de cenários críticos (zero balances, deleted transactions)
- ✅ Code review aprovado (Clean Architecture respeitada)

### Performance

- ✅ Queries agregadas usando índices existentes
- ✅ Tempo de resposta < 100ms para datasets típicos (< 10k transações)
- ✅ Limites de período definidos e validados

### Documentação

- ✅ Changelog atualizado com detalhes da Fase 7
- ✅ STATUS.md refletindo progresso
- ✅ Comentários XML em todos os DTOs e Use Cases

---

## 🚀 Ordem de Execução

1. **Validar pré-condições** ✅
   - Fase 6 completa
   - 255 testes passando
   - Índices verificados

2. **Implementar DTOs** (30 min)
   - 5 arquivos simples de estrutura de dados

3. **Implementar Use Cases** (1.5h)
   - 3 arquivos com lógica de negócio
   - Mockar repositórios

4. **Implementar Controllers** (30 min)
   - 2 arquivos com endpoints REST

5. **Implementar Testes** (2h)
   - 4 arquivos de teste (35 testes)

6. **Validação e Ajustes** (30 min)
   - Rodar testes
   - Validar performance
   - Ajustar queries se necessário

7. **Documentação** (15 min)
   - Atualizar changelog e STATUS.md

**Tempo estimado total**: ~5 horas

---

## 📊 Métricas Esperadas

| Métrica | Baseline (Fase 6) | Meta (Fase 7) |
|---------|-------------------|---------------|
| Total de testes | 255 | ~290 |
| Endpoints REST | 18 | 21 |
| Use Cases | 20 | 23 |
| DTOs | 30 | 35 |
| Controllers | 6 | 8 |

---

## 🔄 Dependências

### Pré-requisitos (COMPLETOS)

- ✅ Fase 4: Transações (tabela, repository, CRUD)
- ✅ Fase 5: Períodos Financeiros (fechamento)
- ✅ Fase 6: Ajustes Pós-Fechamento (ajustes considerados nos saldos)

### Impacto em Fases Futuras

- **Fase 8 (Exportação)**: Usará os mesmos Use Cases para gerar relatórios exportáveis
- **Frontend**: Telas de dashboard consumirão os endpoints de balances

---

## 📋 TODO - Fase 7: Saldos e Relatórios

### 1. Validar Pré-condições
- [ ] Confirmar Fase 6 completa (255 testes passando)
- [ ] Revisar ADR-020, ADR-034, ADR-006
- [ ] Validar índices existentes em transactions table
- [ ] Verificar ITransactionRepository atual

### 2. Domain Layer - Value Objects (Opcional)
- [ ] Avaliar se precisa Value Objects ou apenas DTOs
- [ ] Se necessário: BalanceSummary, DailyBalance, CashFlowReport

### 3. Application Layer - DTOs
- [ ] Criar DTOs/Balances/BalanceSummaryDto.cs
- [ ] Criar DTOs/Balances/CategoryBalanceDto.cs
- [ ] Criar DTOs/Balances/DailyBalanceDto.cs
- [ ] Criar DTOs/Reports/CashFlowReportDto.cs
- [ ] Criar DTOs/Reports/MovementDto.cs

### 4. Application Layer - Use Cases
- [ ] Criar UseCases/Balances/GetBalanceUseCase.cs
  - [ ] Implementar validação de datas
  - [ ] Implementar query agregada por categoria
  - [ ] Implementar cálculo de totais
- [ ] Criar UseCases/Balances/GetDailyBalanceUseCase.cs
  - [ ] Implementar validação de período (max 365 dias)
  - [ ] Implementar cálculo de saldo de abertura
  - [ ] Implementar agregação diária
- [ ] Criar UseCases/Reports/GetCashFlowReportUseCase.cs
  - [ ] Implementar validação de período (max 90 dias)
  - [ ] Implementar query de movimentações
  - [ ] Implementar cálculo de saldos

### 5. Infrastructure Layer - Queries
- [ ] Adicionar métodos em ITransactionRepository ou criar IBalanceRepository
  - [ ] GetBalanceByCategoryAsync
  - [ ] GetBalanceBeforeDateAsync
  - [ ] GetDailyBalancesAsync
- [ ] Implementar queries otimizadas no repository
- [ ] Validar uso dos índices existentes

### 6. API Layer - Controllers
- [ ] Criar Controllers/BalancesController.cs
  - [ ] Endpoint GET /api/v1/balances
  - [ ] Endpoint GET /api/v1/balances/daily
- [ ] Criar Controllers/ReportsController.cs
  - [ ] Endpoint GET /api/v1/reports/cash-flow
- [ ] Adicionar autorização [Authorize(Roles = "Admin,Financeiro")]
- [ ] Registrar Use Cases no DI

### 7. Testes - Application
- [ ] Criar GetBalanceUseCaseTests.cs (7 testes)
  - [ ] GetBalance_WithValidPeriod_ReturnsCorrectSummary
  - [ ] GetBalance_WithCategoryFilter_ReturnsOnlyThatCategory
  - [ ] GetBalance_WithNullDates_UsesCurrentMonth
  - [ ] GetBalance_WithNoTransactions_ReturnsZeroBalances
  - [ ] GetBalance_WithInvalidDates_ThrowsValidationException
  - [ ] GetBalance_ExcludesDeletedTransactions
  - [ ] GetBalance_OnlyIncludesUserTransactions
- [ ] Criar GetDailyBalanceUseCaseTests.cs (6 testes)
  - [ ] GetDailyBalance_WithValidPeriod_ReturnsCorrectDailyBalances
  - [ ] GetDailyBalance_WithNoPreviousTransactions_StartsWithZero
  - [ ] GetDailyBalance_OrdersByDate_Ascending
  - [ ] GetDailyBalance_WithPeriodTooLong_ThrowsValidationException
  - [ ] GetDailyBalance_CalculatesAccumulatedBalances
  - [ ] GetDailyBalance_ExcludesDeletedTransactions
- [ ] Criar GetCashFlowReportUseCaseTests.cs (7 testes)
  - [ ] GetCashFlowReport_WithValidPeriod_ReturnsCompleteReport
  - [ ] GetCashFlowReport_OrdersMovementsByDate
  - [ ] GetCashFlowReport_CalculatesOpeningBalance
  - [ ] GetCashFlowReport_CalculatesNetChange
  - [ ] GetCashFlowReport_FormatsAmountsByType
  - [ ] GetCashFlowReport_WithPeriodTooLong_ThrowsValidationException
  - [ ] GetCashFlowReport_IncludesCategoryNames

### 8. Testes - Contract
- [ ] Criar BalanceContractTests.cs (15 testes)
  - [ ] BalanceSummaryDto_ShouldHaveRequiredStructure
  - [ ] BalanceSummaryDto_ShouldSerializeCorrectly
  - [ ] CategoryBalanceDto_ShouldHaveRequiredStructure
  - [ ] CategoryBalanceDto_ShouldSerializeCorrectly
  - [ ] DailyBalanceDto_ShouldHaveRequiredStructure
  - [ ] DailyBalanceDto_ShouldSerializeCorrectly
  - [ ] DailyBalanceDto_CalculatesClosingBalance
  - [ ] CashFlowReportDto_ShouldHaveRequiredStructure
  - [ ] CashFlowReportDto_ShouldSerializeCorrectly
  - [ ] CashFlowReportDto_CalculatesNetChange
  - [ ] MovementDto_ShouldHaveRequiredStructure
  - [ ] MovementDto_ShouldSerializeCorrectly
  - [ ] MovementDto_IncomeAmount_ShouldBePositive
  - [ ] MovementDto_ExpenseAmount_ShouldBeNegative
  - [ ] MovementDto_ShouldIncludeCategoryName

### 9. Validação Final
- [ ] Compilar projeto (dotnet build)
- [ ] Rodar todos os testes (dotnet test) - Meta: ~290 testes ✅
- [ ] Validar performance de queries (< 100ms)
- [ ] Testar endpoints via Postman/HTTP
- [ ] Revisar logs SQL do EF Core
- [ ] Code review de Clean Architecture

### 10. Documentação
- [ ] Atualizar ai-driven/changelog.md com Fase 7
- [ ] Atualizar backend/STATUS.md com progresso
- [ ] Adicionar métricas finais (testes, endpoints, etc)
- [ ] Marcar Fase 7 como CONCLUÍDA

---

## 📈 Progresso Esperado

```
Fase 7: Saldos e Relatórios
├── DTOs (5 arquivos) ..................... [ ] 0%
├── Use Cases (3 arquivos) ................ [ ] 0%
├── Infrastructure Queries ................ [ ] 0%
├── Controllers (2 arquivos) .............. [ ] 0%
├── Testes Application (20 testes) ........ [ ] 0%
├── Testes Contract (15 testes) ........... [ ] 0%
├── Validação e Performance ............... [ ] 0%
└── Documentação .......................... [ ] 0%

Total: 0/8 (0%)
```

---

**Data de criação**: 2026-01-18  
**Próxima ação**: Aguardar aprovação para iniciar implementação
