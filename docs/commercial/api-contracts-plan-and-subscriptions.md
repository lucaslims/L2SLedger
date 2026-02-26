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

```
GET /api/v1/me/commercial-context
Authorization: Cookie (session — ADR-001, ADR-004)
```

### Responsabilidade

Retornar **todo o contexto comercial necessário** para o frontend operar corretamente.
Consumido **no bootstrap da aplicação**, conforme ADR-042-A.

---

### Response — Tipos (TypeScript)

```typescript
type PlanCode = 'FREE' | 'PRO' | 'BUSINESS';
type FeatureCode =
  | 'AUDIT_TRAIL'
  | 'EXPORT_CSV'
  | 'EXPORT_PDF'
  | 'FINANCIAL_PERIODS'
  | 'MULTI_USER'
  | 'ADS_FREE'
  | 'ADVANCED_REPORTS';
type LimitType = 'TRANSACTIONS_PER_MONTH' | 'CATEGORIES' | 'USERS';
type LimitPeriod = 'MONTHLY' | 'TOTAL';

interface CommercialContextResponse {
  plan: {
    code: PlanCode;        // 'FREE' | 'PRO' | 'BUSINESS'
    name: string;          // 'Gratuito' | 'Pro' | 'Business'
    isPaid: boolean;       // false para FREE
  };
  features: Array<{
    code: FeatureCode;
    enabled: boolean;
  }>;
  limits: Array<{
    type: LimitType;
    max: number | null;    // null = ilimitado
    period: LimitPeriod;
    used: number;          // sempre reflete uso atual
  }>;
  ads: {
    allowed: boolean;      // true apenas no plano FREE
  };
}
```

---

### Exemplo de Response — Plano FREE

```json
{
  "plan": {
    "code": "FREE",
    "name": "Gratuito",
    "isPaid": false
  },
  "features": [
    { "code": "AUDIT_TRAIL",        "enabled": false },
    { "code": "EXPORT_CSV",         "enabled": false },
    { "code": "EXPORT_PDF",         "enabled": false },
    { "code": "FINANCIAL_PERIODS",  "enabled": false },
    { "code": "MULTI_USER",         "enabled": false },
    { "code": "ADS_FREE",           "enabled": false },
    { "code": "ADVANCED_REPORTS",   "enabled": false }
  ],
  "limits": [
    { "type": "TRANSACTIONS_PER_MONTH", "max": 100,  "period": "MONTHLY", "used": 42 },
    { "type": "CATEGORIES",            "max": 10,   "period": "TOTAL",   "used": 7  },
    { "type": "USERS",                 "max": 1,    "period": "TOTAL",   "used": 1  }
  ],
  "ads": { "allowed": true }
}
```

### Exemplo de Response — Plano PRO

```json
{
  "plan": {
    "code": "PRO",
    "name": "Pro",
    "isPaid": true
  },
  "features": [
    { "code": "AUDIT_TRAIL",        "enabled": true  },
    { "code": "EXPORT_CSV",         "enabled": true  },
    { "code": "EXPORT_PDF",         "enabled": true  },
    { "code": "FINANCIAL_PERIODS",  "enabled": true  },
    { "code": "MULTI_USER",         "enabled": false },
    { "code": "ADS_FREE",           "enabled": true  },
    { "code": "ADVANCED_REPORTS",   "enabled": false }
  ],
  "limits": [
    { "type": "TRANSACTIONS_PER_MONTH", "max": null, "period": "MONTHLY", "used": 128 },
    { "type": "CATEGORIES",            "max": null, "period": "TOTAL",   "used": 23  },
    { "type": "USERS",                 "max": 3,    "period": "TOTAL",   "used": 2   }
  ],
  "ads": { "allowed": false }
}
```

---

### Códigos de Resposta HTTP

| Status | Descrição |
|--------|-----------|
| `200 OK` | Contexto retornado com sucesso |
| `401 Unauthorized` | Sessão inválida ou expirada |
| `500 Internal Server Error` | Falha inesperada no backend |

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

