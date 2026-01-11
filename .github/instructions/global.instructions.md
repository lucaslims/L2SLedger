---
applyTo: '**'
---
# Global Instructions — L2SLedger

> ⚠️ **Contexto Global para Ferramentas de IA**
>
> Este arquivo fornece **contexto global** para ferramentas de IA integradas ao repositório (VS Code, Copilot, MCP).
> Ele **NÃO substitui** prompts, ADRs ou regras de governança.

---

## 🎯 Propósito

Estas instruções existem para:

* Evitar interpretações incorretas do projeto
* Garantir consistência arquitetural
* Direcionar agentes para as **fontes corretas de verdade**

---

## 📚 Fontes Oficiais de Verdade

Antes de qualquer ação, ferramentas de IA devem considerar:

1. `docs/adr/adr-index.md` — Decisões arquiteturais (imutáveis)
2. `docs/governance/flow-planejar-aprovar-executar.md` — Fluxo oficial
3. `ai-driven/agent-rules.md` — Regras obrigatórias de execução
4. `architecture.md` — Visão arquitetural
5. `README.md` — Visão geral do projeto

---

## 🧠 Organização de IA no Projeto

### Governança vs Execução

O projeto separa claramente:

* **Governança de IA** → `/ai-driven`
* **Execução de IA (prompts)** → `/.github/prompts`

Ferramentas de IA:

* DEVEM respeitar as regras em `/ai-driven`
* DEVEM usar prompts apenas de `/.github/prompts`
* NÃO DEVEM criar prompts fora dessas pastas

---

## 🔁 Fluxo Obrigatório

Toda interação relevante com IA deve seguir:

```
Planejar → Aprovar → Executar
```

* Planejamento: Agente Planner
* Aprovação: Checklist + humano
* Execução: Prompt Master + agentes especializados

Qualquer violação desse fluxo é proibida.

---

## 🚫 Proibições Globais

Ferramentas de IA **NUNCA** podem:

* Executar código sem planejamento aprovado
* Ignorar ADRs
* Alterar contratos públicos sem versionamento
* Introduzir lógica financeira no frontend
* Atualizar changelog fora da fase de execução

---

## 🧾 Auditoria

Toda execução válida de IA deve resultar em:

* Código consistente
* Testes atualizados (quando aplicável)
* Documentação atualizada (quando aplicável)
* Registro em `ai-driven/changelog.md`

---

## 🧠 Nota Final

> IA no L2SLedger é uma **ferramenta governada**, não um agente autônomo.
>
> Em caso de dúvida, **não executar** — solicitar planejamento ou novo ADR.
