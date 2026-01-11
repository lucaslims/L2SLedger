# Template de Planejamento de Feature — L2SLedger

> **Uso obrigatório antes de qualquer implementação de feature**
>
> Este template deve ser utilizado em conjunto com o prompt:
> **`L2SLedger – Planner.prompt.md`**

---

## 📌 Identificação

* **Nome da Feature:**
* **Responsável pelo Planejamento:**
* **Data:**
* **Status do Planejamento:** (Em análise / Aprovado / Ajustes solicitados)

---

## 🎯 Contexto

Descreva **o problema ou necessidade** que originou esta feature.

* Qual dor ela resolve?
* Quem é impactado?
* Por que agora?

---

## 🧠 Objetivo da Feature

Explique claramente **o que esta feature deve alcançar**, sem entrar em detalhes de implementação.

---

## 🧩 Escopo Funcional

### Incluído

Liste explicitamente o que **faz parte** da feature:

*
*

### Fora de Escopo

Liste explicitamente o que **não faz parte**:

*
*

---

## 🏗️ Impactos Arquiteturais

Avalie impactos em cada camada:

### Backend

* [ ] Nenhum impacto
* [ ] Novos endpoints
* [ ] Alteração de regras de domínio
* [ ] Impacto em banco de dados

Descrição:

---

### Frontend

* [ ] Nenhum impacto
* [ ] Novas telas
* [ ] Alteração de fluxos existentes
* [ ] Impacto em estado global

Descrição:

---

### Banco de Dados

* [ ] Nenhum impacto
* [ ] Nova tabela
* [ ] Alteração de schema
* [ ] Migrations necessárias

Descrição:

---

### Segurança

* [ ] Nenhum impacto
* [ ] Nova regra de autorização
* [ ] Novo dado sensível
* [ ] Impacto em auditoria

Descrição:

---

### CI/CD & Infraestrutura

* [ ] Nenhum impacto
* [ ] Novas variáveis de ambiente
* [ ] Alteração de pipeline
* [ ] Alteração de Docker

Descrição:

---

## 📚 ADRs Relacionados

Liste os ADRs impactados:

* ADR-XXX —
* ADR-XXX —

### Novo ADR é necessário?

* [ ] Não
* [ ] Sim → Descrever proposta:

---

## ⚠️ Riscos Identificados

Liste riscos técnicos, funcionais ou operacionais:

*
*

Mitigações propostas:

*
*

---

## 🧪 Estratégia de Testes

Defina como a feature será validada:

* [ ] Testes unitários (backend)
* [ ] Testes de integração
* [ ] Testes de contrato
* [ ] Testes de frontend
* [ ] Testes manuais

Descrição:

---

## 📄 Impacto em Documentação

* [ ] README.md
* [ ] Architecture.md
* [ ] ADR
* [ ] Outras documentações

Descrição:

---

## 🤖 Agentes que Serão Acionados

* [ ] L2SLedger – Master
* [ ] L2SLedger – Backend
* [ ] L2SLedger – Frontend
* [ ] L2SLedger – CI/CD

---

## 📋 Checklist de Pré-Aprovação

* [ ] Planejamento completo
* [ ] Escopo claro
* [ ] Impactos avaliados
* [ ] Riscos mapeados
* [ ] ADRs identificados

---

## ✅ Aprovação

* **Aprovado por:**
* **Data:**
* **Observações:**

---

## ➡️ Próximos Passos

Após aprovação:

1. Executar **Checklist Automático de Aprovação**
2. Iniciar execução via `L2SLedger – Master.prompt.md`

> **Sem este template preenchido e aprovado, a execução da feature é proibida.**
