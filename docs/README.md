# Documentação do L2SLedger

Repositório central de documentação técnica, decisões arquiteturais e governança.

---

## Estrutura

```
docs/
├── adr/            # 47 ADRs — decisões arquiteturais imutáveis
├── commercial/     # Modelo SaaS, planos e contratos comerciais
├── deployment/     # Guias de deploy, infra e troubleshooting
├── governance/     # Fluxos, checklists e regras de governança
├── planning/       # Planejamentos de API, frontend e CI/CD
└── devops-strategy.md  # Estratégia DevOps e segurança
```

---

## ADRs (Architecture Decision Records)

ADRs são **contratos imutáveis**. Índice completo: [adr/adr-index.md](adr/adr-index.md).

| Categoria                      | ADRs                |
| ------------------------------ | ------------------- |
| Segurança & Autenticação       | 001–005, 008, 018   |
| Observabilidade & Resiliência  | 006–007             |
| Configuração & Secrets         | 009–010             |
| FinOps & Disaster Recovery     | 011–012             |
| Compliance & LGPD              | 013                 |
| Auditoria & Controle de Acesso | 014–017, 019        |
| Arquitetura & Padrões          | 020–027, 022-a      |
| Ambientes & Dados              | 028–030, 044        |
| CI/CD & Infraestrutura         | 031–033, 043        |
| Banco de Dados & Persistência  | 034–036             |
| Qualidade & Testes             | 037–040             |
| IA & Agentes                   | 041                 |
| Comercialização                | 042, 042-a          |

### Propor Novo ADR

1. Criar `adr-XXX.md` seguindo o template existente
2. Registrar no `adr-index.md`
3. Submeter via PR com label `planned`
4. Obter aprovação humana antes de implementar

> ADRs existentes **não podem ser alterados** — apenas complementados por ADRs adicionais (ex.: ADR-021-a, ADR-022-a).

---

## Governança

Fluxo obrigatório: **Planejar → Aprovar → Executar**. Nenhuma etapa pode ser pulada.

| Documento | Descrição |
| --------- | --------- |
| [flow-planejar-provar-executar.md](governance/flow-planejar-provar-executar.md) | Fluxo oficial de mudanças |
| [approval-checklist.md](governance/approval-checklist.md) | Checklist obrigatório de aprovação |
| [ai-playbook.md](governance/ai-playbook.md) | Regras para uso de IA |
| [github-pr-governance.md](governance/github-pr-governance.md) | Governança de Pull Requests |

### Fluxo Resumido

```
Planejar (análise + plano documentado)
   ↓
Aprovar (checklist + aprovação humana)
   ↓
Executar (implementação conforme plano)
```

Nenhuma etapa pode ser pulada. Execuções sem planejamento são proibidas.

---

## Uso de IA

IA é uma **ferramenta governada**, não um agente autônomo.

- Regras obrigatórias: [ai-driven/agent-rules.md](../ai-driven/agent-rules.md)
- Playbook: [governance/ai-playbook.md](governance/ai-playbook.md)
- Seleção de modelos: [ADR-041](adr/adr-041.md)
- Changelog: [ai-driven/changelog.md](../ai-driven/changelog.md)

Princípios: IA não decide sozinha, planejamento precede execução, ADRs são invioláveis, testes e docs são obrigatórios, Toda ação é registrada no [changelog](../ai-driven/changelog.md).

---

## Navegação Rápida

| Pasta | Conteúdo |
| ----- | -------- |
| [commercial/](commercial/) | Modelo SaaS e regras de planos |
| [deployment/](deployment/) | Deploy, configuração e troubleshooting |
| [planning/](planning/) | Planejamentos de API, frontend e CI/CD |
| [PRs/](PRs/) | Histórico de Pull Requests |

---

## Regras Documentais

- Documentação é **obrigatória** para qualquer mudança relevante
- `README.md`, `Architecture.md` e `docs/` devem ser atualizados quando impactados
- Nenhum documento pode contradizer ADRs
- Alterações documentais seguem o mesmo fluxo de aprovação do código
