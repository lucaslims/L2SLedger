# AI‑Driven — Governança do Uso de Agentes IA (L2SLedger)

> **Este diretório é o núcleo de governança do uso de IA no projeto L2SLedger.**
>
> Tudo o que está aqui define **as regras, limites e responsabilidades** para que agentes de IA possam **planejar e executar mudanças** de forma segura, auditável e alinhada à arquitetura.

---

## 🎯 Objetivo da Pasta `ai-driven`

A pasta `/ai-driven` **NÃO contém prompts executáveis**.

Ela centraliza exclusivamente **governança e controle** do uso de IA:

* Regras obrigatórias e imutáveis para agentes IA
* Definição do fluxo oficial Planejar → Aprovar → Executar
* Responsabilidades de cada tipo de agente
* Registro auditável das execuções realizadas com apoio de IA

Ela existe para garantir que:

* Nenhuma execução de IA viole ADRs
* Planejamento, aprovação e execução estejam claramente separados
* Toda alteração seja rastreável
* O uso de IA seja **disciplinado, previsível e seguro**

---

## 📂 Estrutura da Pasta

```text
ai-driven/
├── README.md        # Este documento (governança e uso de IA)
├── agent-rules.md   # Regras globais e obrigatórias para agentes IA
├── changelog.md     # Registro auditável das execuções de IA
```

📌 **Importante:**

> Nenhum prompt executável deve existir dentro desta pasta.

---

## 🧠 Conceito Central

### Planejar → Aprovar → Executar

O L2SLedger segue **estritamente** o fluxo abaixo:

1. **Planejar** — Criar plano técnico detalhado (sem escrever código)
2. **Aprovar** — Validar checklist e obter aprovação humana
3. **Executar** — Executar mudanças via agentes especializados

⚠️ **Execução sem planejamento aprovado é proibida.**

---

## 🤖 Papéis dos Agentes de IA

### 1️⃣ Agente de Planejamento

*Responsabilidade:* **Planejamento apenas**

Responsável por:

* Analisar a solicitação
* Criar plano técnico detalhado
* Identificar impactos em arquitetura, testes e documentação

📌 Não escreve código
📌 Não executa comandos
📌 Não atualiza changelog

---

### 2️⃣ Agente Orquestrador (Prompt Master)

*Responsabilidade:* **Execução coordenada**

Responsável por:

* Validar que planejamento e aprovação existem
* Orquestrar agentes especializados
* Garantir aderência total aos ADRs
* Exigir atualização de testes e documentação
* Registrar execução no changelog

📌 Atua **somente na fase de execução**.

---

### 3️⃣ Agentes Especializados

Executam tarefas **somente após autorização do Orquestrador**.

#### Backend

* Domínio financeiro
* APIs
* Persistência
* Segurança

#### Frontend

* SPA
* UX
* Integração com backend
* Autenticação

#### CI/CD

* Pipelines
* Docker
* Deploy por ambiente

Cada agente possui **escopo fechado**.
Decisões fora do escopo são proibidas.

---

## 📍 Onde Estão os Prompts Executáveis

Os prompts **NÃO ficam nesta pasta**.

Eles estão localizados em:

```text
.github/prompts/
```

Essa decisão é **intencional** e faz parte da governança do projeto:

* Melhor integração com **VS Code (Custom Agents)**
* Compatibilidade nativa com **GitHub Copilot / Prompt Files**
* Separação clara entre **governança** e **execução**

### Regra fundamental

* `/ai-driven` → **regras, limites e auditoria**
* `/.github/prompts` → **prompts executáveis usados pelas ferramentas**


Misturar esses papéis é proibido.

---

## 📜 Regras Obrigatórias (`agent-rules.md`)

Antes de qualquer execução, **todo agente deve carregar e respeitar**:

* `ai-driven/agent-rules.md`
* `docs/adr/adr-index.md`
* `docs/adr/adr-041.md`
* `architecture.md`
* Documentos em `docs/governance/`

Essas regras são **imutáveis**.

---

## 🧾 Changelog de IA (`changelog.md`)

Após **toda execução**, é obrigatório atualizar:

```text
ai-driven/changelog.md
```

O registro deve conter:

* Data
* Agentes envolvidos
* O que foi alterado
* Motivo da alteração
* Impacto técnico

📌 Planejamento **NÃO** gera changelog.
📌 Apenas execução gera registro.

---

## ⚙️ Uso Correto no Dia a Dia

Fluxo recomendado:

1. Executar o **Agente de Planejamento**
2. Criar plano técnico detalhado
3. Validar checklist de aprovação
4. Obter aprovação humana
5. Executar via **Prompt Master**
6. Criar Pull Request seguindo a governança

---

## 🚫 Proibições Absolutas

Agentes IA **NUNCA** podem:

* Executar código sem planejamento aprovado
* Violar ADRs
* Introduzir lógica financeira no frontend
* Alterar contratos públicos sem versionamento
* Atualizar changelog fora da fase de execução
* Finalizar execução sem atualizar testes e documentação (quando aplicável)

---

## ✅ Objetivo Final

O uso de IA no L2SLedger deve resultar em:

* Evolução segura do sistema
* Alta previsibilidade
* Auditoria completa
* Arquitetura consistente
* Confiança no uso de agentes

---

> **IA é uma força multiplicadora — nunca uma autoridade autônoma.**
>
> Este diretório existe para garantir isso.
