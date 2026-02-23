# L2SLedger Backend

> API financeira em **.NET 10** com **Clean Architecture**, **DDD** e governança por ADRs.

---

## Arquitetura

O backend segue **Clean Architecture** (ADR-020) com dependências apontando sempre para o centro:

```
API → Application → Domain ← Infrastructure
```

| Camada             | Projeto                    | Responsabilidade                                  |
| ------------------ | -------------------------- | ------------------------------------------------- |
| **Domain**         | `L2SLedger.Domain`         | Entidades, Value Objects, eventos e regras de negócio |
| **Application**    | `L2SLedger.Application`    | Use Cases, DTOs, validações e interfaces           |
| **Infrastructure** | `L2SLedger.Infrastructure` | Persistência, Firebase, observabilidade, resiliência |
| **API**            | `L2SLedger.API`            | Controllers, middlewares, filtros e configuração    |

> O **backend é a fonte da verdade** para dados e cálculos financeiros.

---

## Princípios

- **Fail-fast** em todas as camadas (ADR-021)
- **Contratos públicos imutáveis** — breaking changes exigem versionamento e ADR (ADR-022, ADR-022-a)
- **Auditoria obrigatória** — todo evento financeiro é registrado de forma imutável (ADR-014)
- **Domínio isolado** — sem dependências externas na camada Domain (ADR-027)
- Erros semânticos com `code`, `message`, `timestamp` e `traceId` (ADR-021-a)

---

## Estrutura de Pastas

```
backend/
├── src/
│   ├── L2SLedger.API/            # Controllers, Middleware, Filters, Configuration
│   ├── L2SLedger.Application/    # UseCases, DTOs, Services, Validators, Mappers
│   ├── L2SLedger.Domain/         # Entities, ValueObjects, Enums, Events, Exceptions
│   └── L2SLedger.Infrastructure/ # Persistence, Repositories, Identity, Observability
├── tests/
│   ├── L2SLedger.API.Tests/
│   ├── L2SLedger.Application.Tests/
│   ├── L2SLedger.Contract.Tests/
│   ├── L2SLedger.Domain.Tests/
│   └── L2SLedger.Infrastructure.Tests/
├── Dockerfile
├── L2SLedger.sln
└── docker-compose.dev.yml
```

---

## Entidades do Domínio

`Transaction`, `Category`, `FinancialPeriod`, `Adjustment`, `AuditEvent`, `Export`, `User`

---

## Endpoints da API

| Controller        | Recurso                 |
| ----------------- | ----------------------- |
| Auth              | Autenticação Firebase   |
| Users             | Gestão de usuários      |
| Transactions      | Lançamentos financeiros |
| Categories        | Categorias              |
| Balances          | Saldos e fluxo de caixa |
| Periods           | Períodos financeiros    |
| Adjustments       | Ajustes financeiros     |
| Reports           | Relatórios              |
| Exports           | Exportação CSV/PDF      |
| Audit             | Trilha de auditoria     |

---

## Segurança

- **Firebase Authentication** como IdP único (ADR-001, ADR-002)
- Sessões via **cookies HttpOnly + Secure + SameSite=Lax** (ADR-004)
- Tokens **nunca** armazenados no frontend
- **RBAC/ABAC** no backend (ADR-016)
- Logout com revogação de sessão (ADR-003)
- Criptografia de dados sensíveis (ADR-018)
- Auditoria de acessos e tentativas negadas (ADR-019)

---

## Persistência

- **PostgreSQL** como fonte única de dados (ADR-034)
- **EF Core** com migrations versionadas (ADR-035)
- Política de retenção e backups (ADR-036)
- Seeds de dados para ambientes DEV/DEMO (ADR-029)

---

## Observabilidade e Resiliência

- **OpenTelemetry** — métricas, logs e tracing (ADR-006)
- **Health Checks** — `/health`, `/health/ready`, `/health/live`
- **Prometheus** — `/metrics`
- **Correlation ID** — rastreamento de requisições
- **Timeout, Retry e Circuit Breaker** (ADR-007)

---

## Testes

| Tipo         | Projeto de Testes              | Referência |
| ------------ | ------------------------------ | ---------- |
| Unitários    | `L2SLedger.Domain.Tests`      | ADR-037    |
| Integração   | `L2SLedger.Infrastructure.Tests` | ADR-037 |
| Contrato     | `L2SLedger.Contract.Tests`    | ADR-039    |
| Regressão    | Testes financeiros dedicados   | ADR-038    |
| API          | `L2SLedger.API.Tests`         | ADR-037    |

Pipeline CI bloqueia merge se testes falharem.

---

## Execução Local

```bash
# Via Docker
docker compose -f docker-compose.dev.yml up -d

# Via CLI
dotnet restore
dotnet build
dotnet run --project src/L2SLedger.API
```

Consulte [RUNNING.md](RUNNING.md) para detalhes de configuração.

---

## CI/CD

- **GitHub Actions** (ADR-031)
- Build multi-plataforma: AMD64 + ARM64 (ADR-033)
- Deploy automático em DEMO, com aprovação em PROD (ADR-043)
- Imagens Docker imutáveis (ADR-032)

---

## Ambientes

| Ambiente | Finalidade           | ADR     |
| -------- | -------------------- | ------- |
| DEV      | Desenvolvimento local | ADR-028 |
| DEMO     | Demonstração segura   | ADR-028 |
| PROD     | Produção              | ADR-028 |

Ambientes são **totalmente isolados** — bancos, configurações e secrets nunca se misturam (ADR-030).

---

## ADRs Relevantes

| ADR | Decisão |
| --- | ------- |
| 001–005 | Segurança e autenticação Firebase |
| 006–007 | Observabilidade e resiliência |
| 009–010 | Configuração e secrets |
| 014–019 | Auditoria e controle de acesso |
| 020–027 | Arquitetura, Clean Architecture e DDD |
| 028–030 | Ambientes e isolamento |
| 031–033 | CI/CD e infraestrutura |
| 034–036 | Banco de dados e persistência |
| 037–039 | Testes automatizados |

Consulte o [índice completo de ADRs](../docs/adr/adr-index.md).

---

## Regras para Contribuições

1. **Respeitar ADRs** — nenhuma mudança pode violar decisões existentes
2. **Testes obrigatórios** — toda mudança deve incluir testes atualizados
3. **Fail-fast** — validar entradas o mais cedo possível
4. **Domínio puro** — sem dependências externas na camada Domain
5. **Contratos imutáveis** — alterações incompatíveis exigem novo ADR e versionamento
6. **Changelog** — atualizar `ai-driven/changelog.md` ao final

Siga o fluxo **Planejar → Aprovar → Executar** definido na [governança](../docs/governance/flow-planejar-provar-executar.md).
