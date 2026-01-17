---
title: Planejamento Técnico — Fases 6 a 10
date: 2026-01-17
version: 1.1
dependencies:
  - Fase 1, 2, 3, 4 e 5 concluídas
status: Aprovada
---

# Planejamento Técnico — Fases 6-10 do L2SLedger

## Contexto

O **L2SLedger** é um sistema de controle de fluxo de caixa com arquitetura Clean Architecture + DDD. As **Fases 1-5 estão 100% concluídas** com:

| Fase | Módulo | Testes | Status |
| --- | --- | --- | --- |
| 1 | Estrutura Base | 6 | ✅ |
| 2 | Autenticação (Firebase) | 31 | ✅ |
| 3 | Categorias (CRUD + Seed) | 53 | ✅ |
| 4 | Transações (CRUD + Filtros) | 37 | ✅ |
| 5 | Períodos Financeiros (Close/Reopen) | 84 | ✅ |
| Total | | 211 | ✅ |

Stack consolidada: .NET 9.0, PostgreSQL 17, Firebase Auth, EF Core 9.0, Serilog, AutoMapper, FluentValidation.

Módulos pendentes (identificados em api-planning.md):

- 3.5 Ajustes Pós-Fechamento
- 3.6 Saldos e Relatórios
- 3.7 Exportação
- 3.8 Auditoria (endpoints)
- 3.9 Usuários e Permissões
- 3.10 Configurações (opcional)
- 3.13 Health & Observabilidade

## Objetivo

Criar um **plano de implementação** detalhado para os módulos restantes do L2SLedger, respeitando:

- ADRs existentes
- Governança de IA (Planejar → Aprovar → Executar)
- Priorização por valor de negócio e dependências técnicas

## ADRs Relacionados

Fase 6 — Ajustes Pós-Fechamento:

| ADR | Título | Impacto |
| --- | --- | --- |
| ADR-015 | Imutabilidade de Dados Financeiros | CORE - Define fluxo de ajustes - Ajustes criam novos lançamentos, não alteram originais |
| ADR-014 | Auditoria de Operações Críticas / Auditoria Financeira | Registro de quem fez ajustes,quando e por quê |
| ADR-021 | Tratamento de Erros e Exceções | Validação de dados, erros claros para o cliente |

Fase 7 — Saldos e Relatórios:

| ADR | Título | Impacto |
| --- | --- | --- |
| ADR-020 | Clean Architecture | Organização dos Use Cases de relatórios |
| ADR-034 | PostgreSQL com Indexes Otimizados | Performance de queries agregadas para saldos e relatórios |
| ADR-006 | Observabilidade e Métricas | Monitoramento de performance dos relatórios |

Fase 8 — Exportação:

| ADR | Título | Impacto |
| --- | --- | --- |
| ADR-017 | Estratégia de Exportação de Dados | Definição de formatos, background jobs para exportação |
| ADR-016 | RBAC - Controle de Acesso Baseado em Funções | Apenas usuários autorizados podem exportar dados |
| ADR-014 | Auditoria de Operações Críticas / Auditoria Financeira | Registro de exportações realizadas |

Fase 9 — Auditoria (Endpoints):

| ADR | Título | Impacto |
| --- | --- | --- |
| ADR-014 | Auditoria de Operações Críticas / Auditoria Financeira | Definição do modelo de auditoria, endpoints para consulta |
| ADR-019 | Estratégia de Armazenamento de Logs de Auditoria | Tabela imutável para eventos de auditoria |
| ADR-016 | RBAC - Controle de Acesso Baseado em Funções | Apenas Admins podem acessar logs de auditoria |

Fase 10 — Usuários e Permissões:

| ADR | Título | Impacto |
| --- | --- | --- |
| ADR-016 | RBAC - Controle de Acesso Baseado em Funções | Definição de roles e permissões |
| ADR-001 | Autenticação com Firebase | Integração com Firebase Auth para gerenciamento de usuários |
| ADR-005 | Gerenciamento de Usuários e Perfis | Endpoints para consulta e atualização de usuários |

Observabilidade e Resiliência:

| ADR | Título | Impacto |
| --- | --- | --- |
| ADR-006 | Observabilidade e Métricas | Health checks, métricas para Prometheus |
| ADR-007 | Estratégia de Resiliência e Retry | Implementação de retry policies com Polly |
| ADR-020 | Clean Architecture | Organização dos serviços de infraestrutura |

Revisar ADRs completos para detalhes.

## Impactos Identificados

Backend:

- Domain: Nova entidade AuditEvent, Value Objects para relatórios
- Application: ~15 novos Use Cases, ~10 novos DTOs, ~5 novos Validators
- Infrastructure: 2-3 novas tabelas (audit_events, exports), novos repositories
- API: ~15 novos endpoints, Health Checks

Banco:

- Nova tabela: audit_events (ADR-014) - ~15 colunas
- Nova tabela: exports (ADR-017) - jobs assíncronos
- Migrations: 3-4 novas migrations

Segurança:

- Autorização granular por endpoint (Admin-only para auditoria)
- Rate limiting para exportações
- Validação de escopo de dados exportados

Performance:

- Queries agregadas para relatórios (índices necessários)
- Exportações assíncronas para grandes volumes
- Cache de saldos consolidados (opcional)

## Riscos

| # | Risco | Probabilidade | Impacto | Mitigação |
| --- | --- | --- | --- | --- |
| 1 | Volume de dados de auditoria | Alta | Médio | Particionamento mensal, retenção definida |
| 2 | Performance de relatórios | Média | Médio | Índices otimizados, cache, paginação |
| 3 | Exportações grandes timeout | Média | Médio | Jobs assíncronos obrigatórios, chunking |
| 4 | Polly incompatível com .NET 9 | Baixa | Alto | Retry manual ou aguardar versão compatível |
| 5 | Escopo crescente de ajustes | Média | Médio | Limitar tipos de ajustes, validar justificativas |

## Opções Consideradas

Opção A — Implementação Sequencial (Recomendada):

Prós:

- Cada fase completa antes da próxima
- Menor risco de regressão
- Validação incremental

Contras:

- Tempo total maior
- Features avançadas demoram mais
- Opção B — Implementação Paralela

Prós:

- Tempo total menor
- Múltiplos agentes simultâneos

Contras:

- Conflitos de merge
- Dependências cruzadas (Ajustes → Auditoria)
- Maior complexidade de orquestração
- Opção C — MVP First (Híbrida)

Prós:

- Entrega rápida das features mais valiosas
- Relatórios e Exportação são high-value

Contras:

- Auditoria pode ficar incompleta temporariamente
- Dívida técnica inicial

Decisão: Opção A (Sequencial) com priorização por dependências.

## Plano Proposto

### 📅 Fase 6 — Ajustes Pós-Fechamento

Prioridade: 🔴 Alta (complementa ADR-015)

Estimativa de Testes: ~40 novos testes

Domain Layer:

| Componente | Descrição |
| --- | --- |
| Adjustment entity | Id, OriginalTransactionId, Amount, Reason, AdjustmentDate, CreatedByUserId |
| AdjustmentType enum | Correction, Reversal, Compensation |
| Validações | Justificativa 10-500 chars, Amount obrigatório |

Application Layer:

| Componente | Descrição |
| --- | --- |
| AdjustmentDto | 10+ propriedades |
| CreateAdjustmentRequest | OriginalTransactionId, Amount, Type, Reason |
| GetAdjustmentsRequest | Filtros por período, transação, tipo |
| CreateAdjustmentUseCase | Valida original existe, cria novo lançamento |
| GetAdjustmentsUseCase | Lista com paginação |
| DeleteAdjustmentUseCase | Apenas Admin pode deletar ajustes |

Infrastructure Layer:

| Componente | Descrição |
| --- | --- |
| adjustments table | FK para transactions, users |
| AdjustmentRepository | CRUD + queries |
| Migration | AddAdjustments |

API Layer:

| Componente | Método | Autorização |
| --- | --- | --- |
| /api/v1/adjustments | GET | Admin, Financeiro |
| /api/v1/adjustments | POST | Admin, Financeiro |
| /api/v1/adjustments/{id} | GET | Admin, Financeiro |
| /api/v1/adjustments/{id} | DELETE | Admin |

### 📅 Fase 7 — Saldos e Relatórios

Prioridade: 🟡 Média (UX/valor de negócio)

Estimativa: ~35 testes

Application Layer:

| Componente | Descrição |
| --- | --- |
| BalanceSummaryDto | TotalIncome, TotalExpense, NetBalance, ByCategory[] |
| DailyBalanceDto | Date, Income, Expense, Balance |
| CashFlowReportDto | Period, Opening, Movements[], Closing |
| GetBalanceUseCase | Calcula saldos consolidados |
| GetDailyBalanceUseCase | Saldos por dia no período |
| GetCashFlowReportUseCase | Relatório de fluxo de caixa |

Infrastructure Layer:

| Componente | Descrição |
| --- | --- |
| Queries agregadas | SUM por categoria, GROUP BY date |
| Índices | (user_id, transaction_date) para performance |

API Layer:

| Componente | Método | Autorização | Descrição |
| --- | --- | --- | --- |
| /api/v1/balances | GET | Admin, Financeiro | Saldos consolidados |
| /api/v1/balances/daily | GET | Admin, Financeiro | Saldos diários |
| /api/v1/reports/cash-flow | GET | Admin, Financeiro | Relatório de fluxo de caixa |

### 📅 Fase 8 — Exportação

Prioridade: 🟡 Média (ADR-017)

Estimativa: ~30 testes

Domain Layer:

| Componente | Descrição |
| --- | --- |
| Export entity | Id, Type, Format, Status, FilePath, RequestedByUserId |
| ExportStatus enum | Pending, Processing, Completed, Failed |
| ExportFormat enum | CSV, PDF |

Application Layer:

| Componente | Descrição |
| --- | --- |
| RequestExportUseCase | Cria job de exportação |
| GetExportStatusUseCase | Retorna status |
| ProcessExportUseCase | Background job (Hosted Service) |

Infrastructure Layer:

| Componente | Descrição |
| --- | --- |
| exports table | Status, timestamps, file path |
| ExportRepository | CRUD + GetPendingAsync |
| CsvExportService | Gera CSV |
| PdfExportService | Gera PDF (QuestPDF ou similar) |

API Layer:

| Componente | Método | Autorização | Descrição |
| --- | --- | --- | --- |
| /api/v1/exports/transactions | POST | Admin, Financeiro, Usuário | Solicita exportação de transações |
| /api/v1/exports/{id}/status | GET | Admin, Financeiro, Usuário | Consulta status da exportação |
| /api/v1/exports/{id}/download | GET | Admin, Financeiro, Usuário | Download do arquivo exportado |

### 📅 Fase 9 — Auditoria (Endpoints)

Prioridade: 🟢 Baixa (Admin-only)

Estimativa: ~30 testes

Domain Layer:

| Componente | Descrição |
| --- | --- |
| `AuditEvent` entity | Id, EventType, EntityType, EntityId, Before, After, UserId, Timestamp, Source |
| `AuditEventType` enum | Create, Update, Delete, Login, Logout, Export, Close, Reopen |

Application Layer:

| Componente | Descrição |
| --- | --- |
| `AuditEventDto` | Mapeamento da entidade para DTO - 12+ propriedades |
| `GetAuditEventsUseCase` | Lista eventos com filtros (data, tipo, usuário) |
| `GetAuditEventByIdUseCase` | Detalhes de um evento específico |
| `IAuditService` | Interface para registrar eventos |

Infrastructure Layer:

| Componente | Descrição |
| --- | --- |
| `audit_events` table | Tabela imutável para eventos de auditoria |
| `AuditEventRepository` | Apenas Insert + Read |
| `AuditService` | Registra eventos automaticamente |

API Layer:

| Componente | Método | Autorização | Descrição |
| --- | --- | --- | --- |
| `GET /api/v1/audit/events` | GET | Admin | Lista eventos de auditoria |
| `GET /api/v1/audit/events/{id}` | GET | Admin | Detalhes de um evento específico |
| `GET /api/v1/audit/access-logs` | GET | Admin | Logs de acesso |

### Fase 10 — Usuários e Permissões

Prioridade: 🟢 Baixa (pode usar seed inicial)

Estimativa: ~25 testes

Application Layer:

| Componente | Descrição |
| --- | --- |
| UpdateUserRolesUseCase | Admin atualiza roles |
| GetUsersUseCase | Lista usuários |
| GetUserByIdUseCase | Detalhes + roles |

API Layer:

| Componente | Método | Autorização | Descrição |
| --- | --- | --- | --- |
| GET /api/v1/users | GET | Admin | Lista usuários |
| GET /api/v1/users/{id} | GET | Admin | Detalhes do usuário |
| GET /api/v1/users/{id}/roles | GET | Admin | Roles do usuário |
| PUT /api/v1/users/{id}/roles | PUT | Admin | Atualiza roles do usuário |

### 📅 Fase Técnica — Health & Observabilidade (Transversal)

Prioridade: 🟡 Média (operacional)

Estimativa: ~15 testes  

Componentes:

| Componente | Descrição |
| --- | --- |
| Health Checks | PostgreSQL, Firebase reachability |
| /health | Basic health |
| /health/ready | Readiness (DB + Firebase) |
| /health/live | Liveness |
| Métricas | Endpoint /metrics para Prometheus |

Detalhes da Implementação:

| Componente | Método | Autorização | Descrição |
| --- | --- | --- | --- |
| /health | GET | Público | Health check básico |
| /health/ready | GET | Público | Readiness check |
| /health/live | GET | Público | Liveness check |
| /metrics | GET | Público | Métricas para Prometheus |

## Resumo de Estimativas

| Fase | Módulo | Endpoints | Testes Est. | Prioridade |
| --- | --- | --- | --- | --- |
| 6 | Ajustes Pós-Fechamento | 3 | ~40 | 🔴 Alta |
| 7 | Saldos e Relatórios | 3 | ~35 | 🟡 Média |
| 8 | Exportação | 3 | ~30 | 🟡 Média |
| 9 | Auditoria | 3 | ~30 | 🟢 Baixa |
| 10 | Usuários | 4 | ~25 | 🟢 Baixa |
| Tech | Health & Observabilidade | 3 | ~15 | 🟡 Média |
| **Total** | | 19 | **~175** | |

Total estimado de testes novos: ~175 (total pós-implementação ~386)

## Ordem Recomendada de Implementação

```text
Fase 6 (Ajustes) → Fase 9 (Auditoria) → Fase 7 (Saldos) → Fase 8 (Exportação) → Fase 10 (Usuários) → Tech (Health)
```

Justificativa:

- Fase 6 primeiro — Completa ADR-015, é pré-requisito para integridade financeira
- Fase 9 em seguida — Ajustes devem ser auditados (ADR-014)
- Fase 7 depois — Saldos dependem de ajustes consolidados
- Fase 8 em sequência — Exportação usa dados consolidados
- Fase 10 por último — Seed atual funciona, baixa urgência
- Tech quando possível — Health Checks podem entrar a qualquer momento

## Agentes a Serem Acionados

| Agente | Responsabilidade |
| --- | --- |
| Backend Agent | Implementação Domain, Application, Infrastructure, API e Health Checks |
| QA Agent | Testes unitários, contract tests, integration tests |
| CI/CD Agent | Pipeline updates (se necessário) |

## Checklist de Aprovação (por fase)

- [ ] ADRs relevantes revisados
- [ ] Plano de implementação aprovado
- [ ] Estimativa de testes definida
- [ ] Dependências verificadas

---

## Próxima Ação Imediata

Aguardar aprovação deste planejamento para iniciar Fase 6 — Ajustes Pós-Fechamento via orquestração do Master Prompt.
