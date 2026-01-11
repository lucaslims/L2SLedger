# Agent Rules — Execução Obrigatória para Agentes IA (L2SLedger)

> **Este arquivo DEVE ser anexado a TODA execução de qualquer agente IA no projeto L2SLedger.**

---

## 🎯 Objetivo

Estabelecer **regras globais e inegociáveis** para qualquer agente de IA que atue no projeto **L2SLedger**, garantindo **consistência, segurança, qualidade e governança contínua**.

---

## 📚 Leitura Obrigatória

Antes de qualquer ação, o agente **DEVE** ler:

* `docs/adr/adr-index.md`
* `architecture.md`
* `README.md`
* Este arquivo (`ai-driven/agent-rules.md`)
* `docs/adr/adr-041.md`

Ignorar qualquer um desses documentos invalida a execução.

---

## 🧱 Respeito aos ADRs

* ADRs são **contratos arquiteturais**
* Nenhuma decisão pode violar ADRs existentes
* Mudanças estruturais exigem novo ADR

---

## 🧪 Testes (Obrigatórios)

Sempre que aplicável, o agente **DEVE**:

* Criar novos testes
* Atualizar testes existentes
* Garantir que testes passem no CI

Tipos de testes esperados:

* Unitários
* Integração
* Contrato
* Regressão financeira (backend)
* Comportamento (frontend)

---

## 📄 Documentação (Obrigatória)

Sempre que aplicável, o agente **DEVE** atualizar:

* `README.md`
* `architecture.md`
* Documentação em `docs/`

Nenhuma alteração relevante pode ficar sem documentação.

---

## 🧾 Changelog (Obrigatório)

Após qualquer execução, o agente **DEVE** atualizar:

```
./ai-driven/changelog.md
```

Incluindo:

* Data
* Arquivos alterados
* Descrição objetiva do que foi feito
* Justificativa técnica

---

## 🚫 Proibições Absolutas

O agente **NÃO PODE**:

* Armazenar tokens no frontend
* Introduzir regra financeira no frontend
* Alterar contratos sem versionamento
* Criar lógica sem testes quando aplicável
* Finalizar execução sem atualizar changelog

---

## 🧠 Postura Esperada do Agente

* Pensar como engenheiro sênior
* Justificar decisões técnicas
* Priorizar clareza e previsibilidade
* Evitar atalhos arquiteturais

---

## ✅ Critério de Validação

Uma execução só é considerada válida se:

* ADRs forem respeitados
* Testes forem atualizados/criados
* Documentação estiver consistente
* Changelog estiver atualizado

> **Este arquivo é a autoridade máxima de execução para agentes IA no L2SLedger.**
> **Qualquer violação destas regras invalida a execução.**