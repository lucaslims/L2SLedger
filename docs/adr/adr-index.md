# Índice de ADRs — L2SLedger

> **Documento de Referência Arquitetural**
> Este arquivo consolida todas as decisões arquiteturais (ADRs) do projeto **L2SLedger**.
> Ele é a principal fonte de consulta para desenvolvedores, revisores, agentes de IA e auditorias técnicas.

---

## Estatísticas

* **Total de ADRs**: 47
* **Última atualização**: 2026-02-18

---

## Categorias

| Categoria                         | ADRs                     |
| --------------------------------- | ------------------------ |
| **Segurança & Autenticação**      | 001–005, 008, 018        |
| **Observabilidade & Resiliência** | 006–007                  |
| **Configuração & Secrets**        | 009–010                  |
| **FinOps & Disaster Recovery**    | 011–012                  |
| **Compliance & Proteção de Dados**| 013                      |
| **Auditoria & Controle de Acesso**| 014–017, 019             |
| **Arquitetura & Padrões**         | 020–027, 022-a           |
| **Ambientes & Dados**             | 028–030, 044             |
| **CI/CD & Infraestrutura**        | 031–033, 043             |
| **Banco de Dados & Persistência** | 034–036                  |
| **Qualidade & Testes**            | 037–040                  |
| **IA & Modelos de Agentes**       | 041                      |
| **Comercialização & Monetização** | 042, 042-a               |


---

## Lista Completa de ADRs

| ADR         | Título                                     |  Link                                  |
| ----------- | ------------------------------------------ | -------------------------------------- |
| ADR-001     | Autenticação Centralizada via Firebase com Cookies no L2SLedger     | [Ver ADR-001](./adr-001.md) |
| ADR-002     | Autenticação com Firebase (Fluxo Completo) no L2SLedger             | [Ver ADR-002](./adr-002.md) |
| ADR-003     | Logout e Revogação de Sessão via Firebase no L2SLedger               | [Ver ADR-003](./adr-003.md) |
| ADR-004     | Segurança de Cookies, CSRF e SameSite no L2SLedger                | [Ver ADR-004](./adr-004.md) |
| ADR-005     | Segurança e Autorização (Firebase-only) no L2SLedger    | [Ver ADR-005](./adr-005.md) |
| ADR-006     | Observabilidade (Logs, Métricas e Tracing) no L2SLedger                            | [Ver ADR-006](./adr-006.md) |
| ADR-007     | Resiliência (Timeout, Retry e Circuit Breaker) no L2SLedger                                | [Ver ADR-007](./adr-007.md) |
| ADR-008     | Segurança de Rede (OCI) no L2SLedger       | [Ver ADR-008](./adr-008.md) |
| ADR-009     | Gerenciamento de Configuração e Segredos no L2SLedger | [Ver ADR-009](./adr-009.md) |
| ADR-010     | Política de Variáveis de Ambiente no L2SLedger | [Ver ADR-010](./adr-010.md) |
| ADR-011     | FinOps Avançado no L2SLedger               | [Ver ADR-011](./adr-011.md) |
| ADR-012     | Backups e Disaster Recovery no L2SLedger   | [Ver ADR-012](./adr-012.md) |
| ADR-013     | LGPD e Proteção de Dados Financeiros no L2SLedger | [Ver ADR-013](./adr-013.md) |
| ADR-014     | Auditoria e Trilha de Auditoria Financeira no L2SLedger        | [Ver ADR-014](./adr-014.md) |
| ADR-015     | Imutabilidade e Fechamento de Períodos Financeiros no L2SLedger | [Ver ADR-015](./adr-015.md) |
| ADR-016     | Controle de Acesso e Permissões Financeiras (RBAC/ABAC) no L2SLedger  | [Ver ADR-016](./adr-016.md) |
| ADR-017     | Exportação de Dados Financeiros (CSV / PDF / API) no L2SLedger | [Ver ADR-017](./adr-017.md) |
| ADR-018     | Criptografia e Proteção de Dados Sensíveis no L2SLedger       | [Ver ADR-018](./adr-018.md) |
| ADR-019     | Auditoria de Acessos e Tentativas Negadas no L2SLedger | [Ver ADR-019](./adr-019.md) |
| ADR-020     | Clean Architecture e Domain-Driven Design no L2SLedger                   | [Ver ADR-020](./adr-020.md) |
| ADR-021     | Modelo de Erros Semântico e Fail-Fast no L2SLedger                  | [Ver ADR-021](./adr-021.md) |
| ADR-021-a   | Catálogo Completo de Códigos de Erro no L2SLedger                   | [Ver ADR-021-a](./adr-021-a.md) |
| ADR-022     | Contratos Públicos Imutáveis da API no L2SLedger               | [Ver ADR-022](./adr-022.md) |
| ADR-022-a   | Estratégia de Versionamento de Contratos da API no L2SLedger                 | [Ver ADR-022-a](./adr-022-a.md) |
| ADR-023     | Não Adoção do Padrão Backend for Frontend (BFF) no L2SLedger                          | [Ver ADR-023](./adr-023.md) |
| ADR-024     | Arquitetura de Serviços, Guards e UI       | [Ver ADR-024](./adr-024.md) |
| ADR-025     | Normalização de Serviços, Guards Obrigatórios e Fail-Fast no L2SLedger                   | [Ver ADR-025](./adr-025.md) |
| ADR-026     | Integração entre Clean Architecture e Auditoria no L2SLedger             | [Ver ADR-026](./adr-026.md) |
| ADR-027     | Separação entre Domínio Financeiro e Infraestrutura no L2SLedger       | [Ver ADR-027](./adr-027.md) |
| ADR-028     | Ambientes Controlados (DEV / DEMO / PROD) no L2SLedger                      | [Ver ADR-028](./adr-028.md) |
| ADR-029     | Estratégia de Seed de Dados Financeiros no L2SLedger                  | [Ver ADR-029](./adr-029.md) |
| ADR-030     | Isolamento de Ambientes e Dados no L2SLedger                    | [Ver ADR-030](./adr-030.md) |
| ADR-031     | GitHub Actions como Pipeline de CI/CD no L2SLedger                       | [Ver ADR-031](./adr-031.md) |
| ADR-032     | Docker como Padrão de Containerização no L2SLedger                                     | [Ver ADR-032](./adr-032.md) |
| ADR-033     | Uso da Oracle Cloud Infrastructure (OCI) como Infraestrutura Base do L2SLedger                | [Ver ADR-033](./adr-033.md) |
| ADR-034     | Banco Relacional (PostgreSQL) como Fonte Única de Persistência no L2SLedger           | [Ver ADR-034](./adr-034.md) |
| ADR-035     | Estratégia de Migrations e Versionamento de Banco de Dados no L2SLedger                        | [Ver ADR-035](./adr-035.md) |
| ADR-036     | Política de Retenção, Arquivamento e Backups de Dados Financeiros no L2SLedger           | [Ver ADR-036](./adr-036.md) |
| ADR-037     | Estratégia de Testes Automatizados         | [Ver ADR-037](./adr-037.md) |
| ADR-038     | Testes de Regressão Financeira             | [Ver ADR-038](./adr-038.md) |
| ADR-039     | Testes de Contrato e Garantia de Integração no L2SLedger                         | [Ver ADR-039](./adr-039.md) |
| ADR-040     | Estratégia de Testes de Frontend no L2SLedger                         | [Ver ADR-040](./adr-040.md) |
| ADR-041     | Seleção de Modelos de IA por Papel de Agente no L2SLedger               | [Ver ADR-041](./adr-041.md) |
| ADR-042     | Modelo de Comercialização SaaS e Planos de Assinatura no L2SLedger      | [Ver ADR-042](./adr-042.md) |
| ADR-042-a   | Contratos Comerciais Consumidos pelo Frontend no L2SLedger              | [Ver ADR-042-a](./adr-042-a.md) |
| ADR-043     | Estratégia de Deploy Automático (DEMO) e com Aprovação (PROD)           | [Ver ADR-043](./adr-043.md) |
| ADR-044     | Adição de Tipo (CategoryType) à Entidade de Categoria no L2SLedger      | [Ver ADR-044](./adr-044.md) |

---

## Observações para IA

* **Backend é a fonte da verdade** para dados e cálculos financeiros
* **Fail-fast é transversal** a todo o sistema
* **Contratos públicos são imutáveis** e versionados
* **Auditoria é obrigatória e imutável**

---

*Este índice deve ser atualizado sempre que um novo ADR for criado ou revisado.*
