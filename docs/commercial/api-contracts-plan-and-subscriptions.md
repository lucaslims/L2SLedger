# Contratos de API — Planos e Assinaturas

> **Documento técnico normativo para agentes IA**
> Este documento define os **contratos de API relacionados a Planos e Assinaturas**.
> Ele deve ser utilizado por agentes de planejamento, backend e frontend.

---

## 🎯 Objetivo

Garantir que:

- O backend seja a **fonte única da verdade** sobre planos
- O frontend consuma contratos **imutáveis e previsíveis**
- Agentes IA não inventem endpoints ou decisões implícitas

---

## 🧱 Princípios de Contrato

1. **Contratos são públicos e imutáveis**
2. **Frontend nunca infere plano ou limite**
3. **Backend retorna tudo que o frontend precisa**
4. **Nenhuma regra financeira depende desses contratos**

---

## 🔐 Escopo dos Contratos

Os contratos descritos aqui:

- São somente de **leitura**
- Não criam ou alteram assinaturas
- Não lidam com pagamento

---

## 📡 Endpoint: Obter contexto comercial do tenant

`GET /me/commercial-context`

### Responsabilidade

Retornar **todo o contexto comercial necessário** para o frontend operar corretamente.

---

### Response (DTO conceitual)

- `plan`
  - `code`
  - `name`
  - `isPaid`

- `features[]`
  - `code`
  - `enabled`

- `limits[]`
  - `type`
  - `max`
  - `period`
  - `used`

- `ads`
  - `allowed`

---

## 📏 Regras Obrigatórias

- Todos os dados retornados são calculados no backend
- `used` sempre reflete uso atual
- Ausência de feature = feature desabilitada
- Ausência de limite = ilimitado

---

## 🚫 Anti‑Padrões (Proibidos)

Agentes IA **NÃO PODEM**:

- Criar endpoints separados para cada feature
- Criar lógica de plano no frontend
- Expor valores financeiros junto com dados comerciais
- Expor pagamento ou preço neste contrato

---

## 🔁 Evolução Futura Permitida

Este contrato poderá evoluir para:

- Trials
- Datas de renovação
- Status de cobrança

Desde que:
- Seja versionado
- Não quebre consumidores existentes

---

## ✅ Critério de Validação para Agentes IA

Uma execução é inválida se:

- O frontend precisar inferir algo não retornado
- O backend delegar decisão comercial ao frontend

---

> **Este documento tem caráter obrigatório para agentes IA.**

