## ADR-042-A — Contratos Comerciais Consumidos pelo Frontend

### Status
Aprovado

---

### Contexto

O ADR-042 definiu o modelo de comercialização SaaS do L2SLedger.

É necessário complementar essa decisão definindo **quais contratos comerciais** são consumidos pelo frontend e **como essas informações devem ser utilizadas**, garantindo:

- Backend como fonte da verdade
- Frontend reativo
- Contratos previsíveis e auditáveis

---

### Decisão

O frontend do L2SLedger consumirá **um único contrato comercial consolidado**, responsável por informar:

- Plano ativo do tenant
- Features habilitadas
- Limites e uso atual
- Permissão de anúncios

Esse contrato será consumido **no bootstrap da aplicação**.

---

### Contrato Oficial

`GET /api/v1/me/commercial-context`

Este endpoint é o **único contrato autorizado** para decisões comerciais no frontend.

---

### Regras de Uso no Frontend

1. O frontend **NÃO PODE**:
   - Inferir plano
   - Calcular limites
   - Decidir habilitação de feature

2. O frontend **PODE**:
   - Ocultar ou exibir funcionalidades
   - Exibir avisos de limite
   - Exibir anúncios se permitido

3. Toda lógica condicional deve ser baseada **exclusivamente** nesse contrato

---

### Regras de Segurança

- Nenhuma informação financeira é exposta
- Nenhuma lógica crítica depende do frontend
- Toda violação de limite deve ser bloqueada no backend

---

### Consequências

#### Positivas

- Frontend simples e previsível
- Redução de bugs de permissão
- Facilidade para marketing e vendas

#### Negativas

- Maior responsabilidade no backend
- Necessidade de testes de contrato

---

### ADRs Relacionados

- ADR-022 — Contratos Públicos Imutáveis
- ADR-024 — Arquitetura de Serviços, Guards e UI
- ADR-025 — Normalização de Serviços e Guards
- ADR-042 — Modelo de Comercialização SaaS

---

### Critério de Validação

Qualquer implementação que:

- Ignore este contrato
- Crie decisões comerciais fora dele

É considerada **violação arquitetural**.

---

> **Este ADR estende formalmente o ADR-042.**

---

