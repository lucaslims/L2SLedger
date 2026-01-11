# Playbook de Uso de IA — L2SLedger

Este playbook define **como o time deve utilizar IA no projeto L2SLedger**, garantindo **segurança, previsibilidade, qualidade e aderência arquitetural**.

Ele é **obrigatório** para qualquer pessoa (humana ou IA) que atue no repositório.

---

## 🎯 Objetivo

* Padronizar o uso de IA no dia a dia
* Evitar decisões improvisadas
* Garantir respeito aos ADRs
* Maximizar produtividade sem comprometer qualidade

---

## 🧠 Princípios Fundamentais

1. **IA não decide sozinha**
2. **Planejar sempre precede executar**
3. **ADRs são contratos**
4. **Testes e documentação não são opcionais**
5. **Toda ação deve ser rastreável**

---

## 🤖 Tipos de Uso de IA no Projeto

### 1️⃣ Planejamento (Planner)

**Quando usar:**

* Nova feature
* Refactor relevante
* Mudança arquitetural
* Avaliação de risco

**Prompt:**

* `L2SLedger–Planner.prompt.md`

**Permissões:**

* ✅ Analisar
* ✅ Propor planos
* ❌ Executar código

---

### 2️⃣ Orquestração (Master)

**Quando usar:**

* Execuções que envolvem mais de uma área
* Coordenação backend + frontend + CI/CD

**Prompt:**

* `L2SLedger–Master.prompt.md`

**Permissões:**

* ✅ Decidir agentes
* ✅ Validar execução
* ❌ Codar diretamente

---

### 3️⃣ Execução Especializada

**Quando usar:**

* Implementação direta

**Prompts:**

* `L2SLedger–Backend.prompt.md`
* `L2SLedger–Frontend.prompt.md`
* `L2SLedger–CI-CD.prompt.md`

**Permissões:**

* ✅ Criar código
* ✅ Atualizar testes
* ✅ Atualizar documentação

---

## 🔁 Fluxo Oficial de Uso de IA

```
Ideia
 ↓
Planner
 ↓
Template de Feature
 ↓
Checklist de Aprovação
 ↓
Aprovação Humana
 ↓
Master
 ↓
Agentes de Execução
```

Nenhuma etapa pode ser pulada.

---

## 📚 Documentos Obrigatórios em Toda Execução

Sempre anexar:

* `ai-driven/agent-rules.md`
* ADRs relevantes
* Planejamento aprovado (quando aplicável)

---

## 🧪 Testes e Qualidade

Toda execução deve:

* Criar ou atualizar testes
* Garantir CI verde
* Não introduzir regressões

IA **nunca** pode justificar ausência de testes.

---

## 📄 Documentação

IA deve atualizar documentação sempre que:

* Criar feature
* Alterar comportamento
* Alterar arquitetura

---

## 🧾 Changelog

Após qualquer execução:

* Atualizar `ai-driven/changelog.md`
* Registrar o que foi feito e por quê

---

## 🚫 Anti‑Padrões (Proibido)

* Executar sem planejamento
* Alterar contratos sem versionamento
* Introduzir lógica financeira no frontend
* Ignorar ADRs
* Executar sem `agent-rules.md`

---

## 👥 Responsabilidade do Time

* Humanos aprovam
* IA executa dentro de limites
* Arquitetura é preservada

---

## 📂 Referências de Governança

* GitHub PR Governance ([`docs/governance/github-pr-governance.md`](./docs/governance/github-pr-governance.md))
* Flow Planejar → Aprovar → Executar ([`docs/governance/flow-planejar-provar-executar.md`](./docs/governance/flow-planejar-provar-executar.md))
* Checklist de Aprovação ([`docs/governance/approval-checklist.md`](./docs/governance/approval-checklist.md))

---

## ✅ Critério de Sucesso

Uso de IA é considerado correto se:

* Fluxo foi seguido
* Código é previsível
* ADRs foram respeitados
* Testes e docs estão atualizados

> **IA acelera. Governança protege. Disciplina garante evolução segura.**
