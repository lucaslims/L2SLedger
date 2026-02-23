# L2SLedger

Sistema de controle financeiro e fluxo de caixa com foco em **segurança, auditabilidade e evolução controlada**. Estruturado com 47 ADRs formais, adequado para ambientes corporativos e regulatórios.

---

## Objetivos

- Controle de fluxo de caixa com rastreabilidade completa
- Auditoria financeira imutável
- Separação rigorosa entre domínio e infraestrutura
- Evolução segura via governança por ADRs

---

## Arquitetura

| Componente   | Tecnologia                  |
| ------------ | --------------------------- |
| Frontend     | React 18 + TypeScript + Vite |
| Backend      | .NET 10 (Clean Architecture) |
| Banco        | PostgreSQL                   |
| Autenticação | Firebase Authentication      |
| Infra        | Docker + OCI (ARM64)         |
| CI/CD        | GitHub Actions               |

Detalhes em [Architecture.md](Architecture.md).

---

## Princípios

- **Backend é a fonte da verdade** para dados e cálculos financeiros
- **Fail-fast** obrigatório em todas as camadas (ADR-021)
- **Contratos públicos imutáveis** — breaking changes exigem versionamento (ADR-022)
- **Auditoria financeira** obrigatória e imutável (ADR-014)
- **Ambientes isolados** — DEV / DEMO / PROD nunca compartilham dados (ADR-030)

---

## Estrutura do Repositório

```
├── backend/        # API .NET 10 — Clean Architecture + DDD
├── frontend/       # SPA React + TypeScript
├── docs/           # ADRs, governança, deploy e planejamentos
├── ai-driven/      # Governança de uso de IA
└── Architecture.md # Visão arquitetural consolidada
```

---

## Documentação

| Documento                                                          | Descrição                          |
| ------------------------------------------------------------------ | ---------------------------------- |
| [Architecture.md](Architecture.md)                                 | Visão detalhada da arquitetura     |
| [docs/adr/adr-index.md](docs/adr/adr-index.md)                    | Índice dos 47 ADRs                 |
| [docs/README.md](docs/README.md)                                  | Organização da documentação        |
| [backend/README.md](backend/README.md)                            | Documentação do backend            |
| [frontend/README.md](frontend/README.md)                          | Documentação do frontend           |
| [docs/deployment/README.md](docs/deployment/README.md)            | Guias de deploy e operação         |

---

## Testes

- Unitários, integração e contrato (ADR-037, ADR-039)
- Regressão financeira (ADR-038)
- Frontend focado em comportamento (ADR-040)
- Pipeline CI bloqueia merge em caso de falha

---

## Deploy

- GitHub Actions com deploy automático em DEMO e aprovação em PROD (ADR-043)
- Containers Docker imutáveis, multi-plataforma AMD64 + ARM64 (ADR-032, ADR-033)
- Configuração por variáveis de ambiente (ADR-010)
- Backups e DR definidos por ADR (ADR-012, ADR-036)

---

## Governança

Todas as decisões técnicas são ADRs imutáveis. Mudanças estruturais exigem novo ADR.

| Documento                                                                          | Descrição                            |
| ---------------------------------------------------------------------------------- | ------------------------------------ |
| [flow-planejar-provar-executar.md](docs/governance/flow-planejar-provar-executar.md) | Fluxo Planejar → Aprovar → Executar |
| [approval-checklist.md](docs/governance/approval-checklist.md)                      | Checklist obrigatório de aprovação   |
| [ai-playbook.md](docs/governance/ai-playbook.md)                                   | Regras para uso de IA                |
| [github-pr-governance.md](docs/governance/github-pr-governance.md)                  | Governança de Pull Requests          |
| [ai-driven/agent-rules.md](ai-driven/agent-rules.md)                               | Regras obrigatórias para agentes IA  |

---

## Status

> Arquitetura e governança finalizadas. Pronto para desenvolvimento incremental e operação controlada.
