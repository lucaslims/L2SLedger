# Integração do Fluxo Planejar → Aprovar → Executar com Pull Requests (GitHub)

Este documento define **como o fluxo oficial do L2SLedger é integrado aos Pull Requests no GitHub**, transformando governança em **regra técnica e auditável**.

---

## 🎯 Objetivo

Garantir que **nenhuma mudança seja mergeada** sem:

* Planejamento documentado
* Aprovação explícita
* Execução conforme ADRs
* Testes e documentação atualizados

O PR passa a ser o **ponto de controle final**.

---

## 🧠 Princípio Central

> **Todo Pull Request deve provar que seguiu o fluxo Planejar → Aprovar → Executar.**

Se não provar, não é aprovado.

---

## 1️⃣ Estrutura Obrigatória do Pull Request

Todo PR **DEVE** conter:

### 📌 Título do PR

Formato obrigatório:

```
[type] descrição curta
```

Exemplos:

* `feat: importação de lançamentos financeiros`
* `fix: correção de cálculo de saldo`
* `refactor: isolamento do domínio financeiro`

---

### 📄 Template de Pull Request (Obrigatório)

Criar o arquivo:

```
.github/pull_request_template.md
```

Conteúdo:

```md
## 📌 Contexto
Descreva brevemente a mudança.

## 🧠 Planejamento
- [ ] Feature planejada usando template
- [ ] Planejamento anexado ou linkado

Link para planejamento:

## 📋 Checklist de Aprovação
- [ ] Checklist automático preenchido

## 📚 ADRs Impactados
- ADR-XXX

## 🧪 Testes
- [ ] Unitários
- [ ] Integração
- [ ] Frontend

## 📄 Documentação
- [ ] README.md
- [ ] Architecture.md
- [ ] Docs atualizadas

## 🧾 Changelog
- [ ] `ai-driven/changelog.md` atualizado
```

---

## 2️⃣ Labels Obrigatórias

Configurar labels no repositório:

| Label             | Uso                         |
| ----------------- | --------------------------- |
| `planned`         | Planejamento anexado        |
| `approved`        | Aprovação humana registrada |
| `executed-by-ai`  | Executado por agente IA     |
| `breaking-change` | Mudança incompatível        |

PR sem `planned` **não pode ser mergeado**.

---

## 3️⃣ Branch Protection Rules

Configurar regras no GitHub:

* Bloquear merge direto na `main`
* Exigir PR aprovado
* Exigir CI verde
* Exigir pelo menos 1 reviewer
* Proibir force push

---

## 4️⃣ Integração com CI (Gate Técnico)

### Checks obrigatórios

* CI Backend
* CI Frontend
* Lint
* Testes

Se qualquer check falhar → merge bloqueado.

---

## 5️⃣ Aprovação Humana (Gate de Governança)

A aprovação humana significa que:

* Planejamento foi revisado
* Checklist foi validado
* ADRs foram respeitados

Sem aprovação → merge proibido.

---

## 6️⃣ Execuções com IA

PRs gerados por IA **DEVEM**:

* Conter label `executed-by-ai`
* Referenciar prompt utilizado
* Referenciar planejamento

---

## 7️⃣ Fluxo Completo no GitHub

```
Planejamento (docs)
   ↓
PR criado
   ↓
Template preenchido
   ↓
CI executa
   ↓
Checklist validado
   ↓
Aprovação humana
   ↓
Merge permitido
```

---

## 8️⃣ Auditoria e Histórico

Cada PR passa a ser:

* Registro de decisão
* Evidência de governança
* Histórico técnico

---

## ✅ Critério de Conclusão

Um PR só pode ser mergeado se:

* Template estiver completo
* Labels corretas estiverem aplicadas
* CI estiver verde
* Aprovação humana registrada

> **PR não é apenas código. PR é governança executável.**
