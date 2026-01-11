# L2SLedger

**L2SLedger** é um sistema de controle financeiro e fluxo de caixa projetado com foco em **segurança, auditabilidade, previsibilidade e evolução controlada**.

O projeto foi estruturado desde o início com decisões arquiteturais formais (ADRs), tornando-o adequado para ambientes corporativos, financeiros e regulatórios.

---

## 🎯 Objetivos do Sistema

* Controle de fluxo de caixa
* Rastreabilidade completa de operações financeiras
* Auditoria imutável
* Separação clara entre domínio e infraestrutura
* Evolução segura e previsível

---

## 🧱 Arquitetura (Visão Geral)

* Frontend SPA (React + TypeScript)
* Backend (.NET 10)
* PostgreSQL como banco relacional
* Firebase Authentication como IdP
* Infraestrutura baseada em OCI
* Containers Docker
* CI/CD com GitHub Actions

---

## 🔐 Princípios Fundamentais

* Backend é a fonte da verdade
* Fail-fast obrigatório
* Contratos públicos imutáveis
* Auditoria financeira obrigatória
* Separação de ambientes (DEV / DEMO / PROD)

---

## 📂 Documentação

| Documento         | Descrição                                 |
| ----------------- | ----------------------------------------- |
| `docs/adr-index.md`    | Índice completo de decisões arquiteturais |
| `architecture.md` | Visão detalhada da arquitetura            |
| `docs/`           | Documentação técnica complementar         |

---

## 🧪 Qualidade & Testes

* Testes unitários, integração e contrato
* Testes de regressão financeira
* Testes de frontend focados em comportamento
* Pipeline CI bloqueia regressões

---

## 🚀 Deploy & Operação

* Deploy automatizado via GitHub Actions
* Containers Docker imutáveis
* Configuração por variáveis de ambiente
* Backups e DR definidos por ADR

---

## 📜 Governança

Todas as decisões técnicas relevantes estão documentadas como ADRs.
Nenhuma mudança estrutural deve ser feita sem criação ou atualização de um ADR.
Consulte `docs/adr-index.md` para o índice completo.

Esse projeto segue estritamente o modelo de governança definido em `docs/governance/`:

* [`flow-planejar-provar-executar.md`](./docs/governance/flow-planejar-provar-executar.md): Fluxo oficial de Planejar → Aprovar → Executar
* [`approval-checklist.md`](./docs/governance/approval-checklist.md): Checklist obrigatório para aprovação de mudanças
* [`ai-playbook.md`](./docs/governance/ai-playbook.md): Regras para uso de IA na geração de 
* [`github-pr-governance.md`](./docs/governance/github-pr-governance.md): Fluxo completo de PRs no GitHub

---

## 📌 Status do Projeto

> **Arquitetura e governança finalizadas**
> Pronto para desenvolvimento incremental e operação controlada.
