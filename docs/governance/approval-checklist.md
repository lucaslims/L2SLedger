# Checklist Automático de Aprovação — L2SLedger

Este checklist define os **critérios obrigatórios de aprovação** para qualquer mudança no projeto **L2SLedger**, seja ela funcional, técnica ou arquitetural.

Ele é usado em conjunto com o fluxo oficial:

> **Planejar → Aprovar → Executar**

---

## 🎯 Objetivo

Garantir que **nenhuma mudança seja executada ou integrada** sem:

* Planejamento adequado
* Validação arquitetural
* Conformidade com ADRs
* Testes e documentação atualizados

Este checklist é **automático, auditável e obrigatório**.

---

## 🧠 Etapa 1 — Checklist de Planejamento (PLANNER)

> Deve ser validado **antes** de qualquer execução.

* [ ] Existe um plano documentado usando `L2SLedger – Planner.prompt.md`
* [ ] O plano descreve claramente o **objetivo da mudança**
* [ ] Impactos em **backend / frontend / dados / segurança / CI/CD** foram analisados
* [ ] ADRs afetados foram identificados
* [ ] Foi avaliado se é necessário criar um **novo ADR**
* [ ] Riscos técnicos foram explicitados
* [ ] O plano define **quais agentes serão acionados**

❌ Sem todos os itens acima, a execução é proibida.

---

## 🧠 Etapa 2 — Checklist de Aprovação (HUMANA)

> Deve ser validado pelo **owner / arquiteto / tech lead**.

* [ ] O plano respeita todos os ADRs existentes
* [ ] Não há violação de princípios arquiteturais
* [ ] Escopo está claro e delimitado
* [ ] Impactos são aceitáveis
* [ ] Riscos possuem mitigação
* [ ] A execução foi explicitamente autorizada

Resultado:

* [ ] ✅ ou V - Aprovado
* [ ] 🔁 ou A - Ajustes solicitados
* [ ] ❌ ou X - Rejeitado

---

## ⚙️ Etapa 3 — Checklist de Execução (AGENTES)

> Validado **durante e após** a execução.

* [ ] Execução seguiu exatamente o plano aprovado
* [ ] Apenas os agentes necessários foram utilizados
* [ ] `ai-driven/agent-rules.md` foi anexado
* [ ] Nenhuma decisão improvisada foi tomada

---

## 🧪 Etapa 4 — Testes (OBRIGATÓRIO)

* [ ] Testes unitários criados ou atualizados
* [ ] Testes de integração criados ou atualizados
* [ ] Testes de contrato validados (se aplicável)
* [ ] Testes de frontend atualizados (se aplicável)
* [ ] Pipeline de CI passou sem falhas

❌ Mudanças sem testes atualizados são inválidas.

---

## 📄 Etapa 5 — Documentação (OBRIGATÓRIA)

* [ ] `README.md` atualizado (se aplicável)
* [ ] `Architecture.md` atualizado (se aplicável)
* [ ] Documentação em `docs/` atualizada (se aplicável)
* [ ] ADR criado ou atualizado (se necessário)

---

## 🧾 Etapa 6 — Governança & Auditoria

* [ ] `ai-driven/changelog.md` atualizado
* [ ] Alterações estão rastreáveis
* [ ] Nenhuma regra de segurança foi violada

---

## ✅ Critério Final de Aprovação

Uma mudança **só pode ser considerada concluída** se:

* Todas as etapas acima estiverem marcadas
* A aprovação humana estiver registrada
* O changelog estiver atualizado

> **Checklist incompleto = mudança inválida.**

---

## 🤖 Uso com IA

Este checklist deve ser:

* Referenciado pelo `prompt-master.md`
* Utilizado como critério de validação por agentes
* Integrado futuramente a PRs no GitHub

---

## 📌 Observação Final

Este checklist é parte da **governança oficial do L2SLedger** e não pode ser ignorado ou reduzido sem novo ADR.
