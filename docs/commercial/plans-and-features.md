# Tabela de Features por Plano — Fonte Única de Verdade

> **Documento canônico de features comerciais do L2SLedger**
> Este arquivo é a **referência única** para:
>
> * Marketing
> * Backend (feature flags / guards)
> * Frontend (exibição condicional)
> * Agentes IA (planejamento e execução)

---

## 🎯 Objetivo

Definir, de forma **explícita e auditável**, quais features e limites estão disponíveis em cada plano do L2SLedger, evitando:

* Divergência entre frontend e backend
* Decisões implícitas por agentes IA
* Regras comerciais espalhadas no código

---

## 🧱 Planos Definidos

| Código   | Nome     | Tipo          |
| -------- | -------- | ------------- |
| FREE     | Free     | Gratuito      |
| PRO      | Pro      | Pago (manual) |
| BUSINESS | Business | Pago (manual) |

---

## 🧩 Features Funcionais (Binárias)

| Feature Code     | Descrição                    | Free | Pro | Business |
| ---------------- | ---------------------------- | ---- | --- | -------- |
| MULTI_USER       | Múltiplos usuários           | ❌    | ✅   | ✅        |
| MULTI_ACCOUNT    | Múltiplas contas financeiras | ❌    | ✅   | ✅        |
| AUDIT_LOG        | Trilha de auditoria          | ✅    | ✅   | ✅        |
| EXPORT_CSV       | Exportação CSV               | ❌    | ✅   | ✅        |
| EXPORT_PDF       | Exportação PDF               | ❌    | ❌   | ✅        |
| ADVANCED_REPORTS | Relatórios avançados         | ❌    | ❌   | ✅        |
| PERIOD_CLOSE     | Fechamento de período        | ❌    | ✅   | ✅        |
| API_ACCESS       | Acesso via API               | ❌    | ❌   | ✅        |

---

## 📏 Limites Quantitativos

| Limite             | Tipo      | Free | Pro   | Business  |
| ------------------ | --------- | ---- | ----- | --------- |
| Usuários           | MONTHLY   | 1    | 5     | Ilimitado |
| Contas financeiras | UNLIMITED | 1    | 5     | Ilimitado |
| Lançamentos / mês  | MONTHLY   | 100  | 1.000 | Ilimitado |
| Exportações / mês  | MONTHLY   | 0    | 10    | Ilimitado |

---

## 📢 Anúncios

| Regra                | Free | Pro | Business |
| -------------------- | ---- | --- | -------- |
| Exibição de anúncios | ✅    | ❌   | ❌        |

### Restrições obrigatórias

* Anúncios **nunca** aparecem em:

  * Lançamentos financeiros
  * Auditoria
  * Fechamento de período
  * Exportações

---

## 🧠 Regras de Interpretação (OBRIGATÓRIAS)

1. Feature ❌ + limite > 0 = **feature desabilitada**
2. Feature ✅ + limite = 0 = **bloqueio total**
3. Ausência de limite = ilimitado
4. Auditoria nunca é limitada

---

## 🗂️ Onde este documento deve existir no código

### 📁 Local recomendado

```
/docs/commercial/
 └── plans-and-features.md
```

### Por quê?

* Fica versionado junto com ADRs
* Pode ser referenciado por agentes IA
* Serve como insumo para:

  * Seeds de banco
  * Testes de contrato
  * Documentação pública

---

## 🔁 Uso pelo Backend

* Seeds de `Plan`, `PlanFeature` e `PlanLimit`
* Guards de casos de uso
* Respostas do endpoint `/api/v1/me/commercial-context`

---

## 🎨 Uso pelo Frontend

* Exibição condicional de menus
* Mensagens de upgrade
* Bloqueio visual de ações

---

## 🤖 Uso por Agentes IA

Agentes IA **DEVEM**:

* Ler este documento antes de alterar qualquer regra comercial
* Nunca criar features fora desta tabela sem novo ADR
* Validar consistência com ADR-042 e ADR-042-A

---

## 🚫 Anti-Padrões

* Duplicar esta tabela em código sem referência
* Hardcode de regras comerciais
* Divergência entre marketing e backend

---

## ✅ Critério de Validação

Qualquer mudança em planos ou features exige:

* Atualização deste documento
* Atualização de seeds
* Atualização de testes
* (Se estrutural) novo ADR

---

> **Este documento é a fonte única de verdade para features por plano no L2SLedger.**
> **Qualquer divergência deve ser corrigida imediatamente.**