# Fluxo Oficial — Planejar → Aprovar → Executar (L2SLedger)

Este documento define o **fluxo oficial e obrigatório** para qualquer evolução do projeto **L2SLedger**, seja funcional, técnica ou arquitetural.

---

## 🧠 Visão Geral do Fluxo

```
┌────────────┐      ┌────────────┐      ┌────────────┐
│  PLANEJAR  │ ───▶ │  APROVAR   │ ───▶ │  EXECUTAR  │
└────────────┘      └────────────┘      └────────────┘
```

Nenhuma etapa pode ser pulada.

---

## 1️⃣ PLANEJAR

### Objetivo

Entender o problema, avaliar impactos e definir um plano seguro **sem alterar o sistema**.

### Prompt Utilizado

* `prompt-planning.md`

### Quem Executa

* Arquiteto
* Tech Lead
* Agente IA em modo `planner`

### Entradas

* Pedido do usuário
* ADRs existentes
* Arquitetura atual

### Saídas Esperadas

* Plano estruturado
* Impactos identificados
* Riscos mapeados
* Lista de agentes a acionar

❌ Nenhum código é alterado
❌ Nenhum arquivo é criado

---

## 2️⃣ APROVAR

### Objetivo

Validar se o plano:

* Respeita ADRs
* Não introduz riscos indevidos
* Está alinhado ao roadmap

### Quem Aprova

* Você (owner do projeto)
* Arquiteto responsável

### Possíveis Decisões

* ✅ Aprovar integralmente
* 🔁 Solicitar ajustes no planejamento
* ❌ Rejeitar

### Saída

* Autorização explícita para execução

Sem aprovação, **execução é proibida**.

---

## 3️⃣ EXECUTAR

### Objetivo

Implementar exatamente o que foi planejado e aprovado.

### Prompts Utilizados

Dependendo do escopo:

* `prompt-master.md` (orquestração)
* `prompt-backend.md`
* `prompt-frontend.md`
* `prompt-ci-cd.md`

Sempre anexando:

* `ai-driven/agent-rules.md`

### Obrigações Durante Execução

* Atualizar testes (se aplicável)
* Atualizar documentação (se aplicável)
* Atualizar `ai-driven/changelog.md`

---

## 🔐 Regras de Governança

* Planejamento **não executa**
* Execução **não improvisa**
* Mudança fora do plano exige novo ciclo

---

## ✅ Critério de Conclusão

Uma entrega só é considerada válida se:

* Seguiu o fluxo completo
* Respeitou ADRs
* Possui testes atualizados
* Possui documentação atualizada
* Possui changelog atualizado

> **Planejar protege o sistema. Aprovar protege o time. Executar com disciplina garante evolução segura.**
