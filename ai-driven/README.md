# AI-Driven — Governança de IA (L2SLedger)

> Núcleo de governança do uso de agentes IA no projeto. Define regras, limites e auditoria.

---

## Objetivo

Centralizar **governança e controle** do uso de IA — não contém prompts executáveis.

Garante que:

- Nenhuma execução viole ADRs
- Planejamento, aprovação e execução estejam separados
- Toda alteração seja rastreável e auditável

---

## Estrutura

```
ai-driven/
├── README.md        # Este documento
├── agent-rules.md   # Regras globais obrigatórias para agentes IA
└── changelog.md     # Registro auditável das execuções de IA
```

> Prompts executáveis ficam em `.github/prompts/` — separação intencional entre governança e execução.

---

## Fluxo: Planejar → Aprovar → Executar

1. **Planejar** — Criar plano técnico sem escrever código
2. **Aprovar** — Validar checklist + aprovação humana
3. **Executar** — Implementar via agentes especializados

⚠️ **Execução sem planejamento aprovado é proibida.**

---

## Papéis dos Agentes

| Agente | Responsabilidade | Permissões |
| ------ | ---------------- | ---------- |
| **Planner** | Análise e plano técnico | Analisar, propor — **não executa** |
| **Master** | Orquestração e validação | Coordenar agentes — **não coda** |
| **Backend** | Domínio, APIs, persistência | Criar código + docs |
| **Frontend** | SPA, UX, integração | Criar código + docs |
| **CI/CD** | Pipelines, Docker, deploy | Criar código + testes + docs |
| **QA** | Testes automatizados | Criar testes + validar execução |

Cada agente possui **escopo fechado**. Decisões fora do escopo são proibidas.

---

## Regras Obrigatórias

Antes de executar, todo agente deve carregar:

- `ai-driven/agent-rules.md`
- `docs/adr/adr-index.md`
- `Architecture.md`
- Documentos em `docs/governance/`

Detalhes completos em [agent-rules.md](agent-rules.md).

---

## Changelog

Após **toda execução**, atualizar `ai-driven/changelog.md` com: data, arquivos alterados, descrição e justificativa.

Planejamento **não** gera changelog — apenas execução.

---

## Uso Correto no Dia a Dia

Fluxo recomendado:

1. Executar o **Agente de Planejamento**
2. Criar plano técnico detalhado
3. Validar checklist de aprovação
4. Obter aprovação humana
5. Executar via **Prompt Master**
6. Criar Pull Request seguindo a governança

---

## Proibições

Agentes IA **nunca** podem:

- Executar sem planejamento aprovado
- Violar ADRs
- Introduzir lógica financeira no frontend
- Alterar contratos sem versionamento
- Finalizar sem atualizar testes/docs (quando aplicável)
- Atualizar changelog fora da fase de execução

---

## ✅ Objetivo Final

O uso de IA no L2SLedger deve resultar em:

- Evolução segura do sistema
- Alta previsibilidade
- Auditoria completa
- Arquitetura consistente
- Confiança no uso de agentes

---

> **IA é uma força multiplicadora — nunca uma autoridade autônoma.**
