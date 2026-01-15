# Domínio Comercial — Regras de Planos e Assinaturas

> **Documento normativo para agentes IA e desenvolvedores**
> Este documento define as **regras de domínio do modelo comercial do L2SLedger**.
> Ele deve ser lido **antes de qualquer planejamento, execução ou refatoração** relacionada a planos, assinaturas, limites ou anúncios.

---

## 🎯 Objetivo

Estabelecer regras **claras, invariáveis e auditáveis** para o domínio de **Planos e Assinaturas**, garantindo que:

- Monetização **não contamine** o domínio financeiro
- O backend permaneça a **fonte única da verdade**
- Agentes IA não tomem decisões implícitas ou perigosas
- O sistema permaneça evolutivo (SaaS hoje, cobrança automática amanhã)

---

## 🧱 Princípios Fundamentais (INEGOCIÁVEIS)

1. **Plano NÃO é Assinatura**
   - Plano define *capacidades e limites*
   - Assinatura define *quem usa qual plano e quando*

2. **Domínio Financeiro é cego a planos**
   - Nenhuma regra financeira pode conhecer:
     - Tipo de plano
     - Preço
     - Gratuidade
     - Anúncios

3. **Backend sempre decide**
   - Plano ativo
   - Features habilitadas
   - Limites quantitativos
   - Permissão de anúncios

4. **Frontend é apenas reativo**
   - Nunca decide plano
   - Nunca calcula limites
   - Nunca desbloqueia feature por conta própria

5. **Ausência de pagamento é uma decisão temporária**
   - O modelo **deve estar preparado** para cobrança futura
   - Nenhuma entidade pode impedir essa evolução

---

## 🧩 Entidades do Domínio Comercial

### 🔹 Plan
Representa um **tipo de plano comercial**.

**Responsabilidades**:
- Definir quais features existem
- Definir limites máximos
- Definir se anúncios são permitidos

**Regras**:
- É uma entidade **quase estática**
- Não possui datas
- Não possui pagamento
- Não possui vínculo direto com usuários

---

### 🔹 PlanFeature
Representa uma **capacidade funcional**.

**Exemplos**:
- MULTI_USER
- EXPORT_CSV
- EXPORT_PDF
- AUDIT_LOG
- ADVANCED_REPORTS

**Regras**:
- Feature é binária (habilitada ou não)
- Feature **não define quantidade**
- Feature nunca executa lógica financeira

---

### 🔹 PlanLimit
Representa uma **restrição quantitativa**.

**Exemplos**:
- Máximo de usuários
- Máximo de contas financeiras
- Máximo de exportações
- Máximo de lançamentos por período

**Regras**:
- Limites são verificados **antes** do domínio financeiro
- Limites nunca bloqueiam auditoria
- Limites não alteram dados financeiros existentes

---

### 🔹 Subscription
Representa a **adesão de um tenant a um plano**.

**Responsabilidades**:
- Definir plano ativo
- Controlar status da assinatura
- Controlar vigência

**Regras**:
- Não contém informação de pagamento
- Pode ser criada, suspensa ou expirada
- Toda mudança deve ser auditável

---

### 🔹 SubscriptionHistory (Obrigatória para auditoria)

Registra **toda alteração de plano**.

**Regras**:
- Nenhuma mudança de plano pode ocorrer sem histórico
- Histórico nunca é editado
- Histórico nunca é removido

---

## 📢 Regras sobre Anúncios (Plano Free)

1. Apenas o plano **Free** pode exibir anúncios
2. Anúncios são considerados **feature de UI**
3. É **proibido** exibir anúncios em:
   - Lançamentos financeiros
   - Fechamento de período
   - Auditoria
   - Exportações
4. O backend informa explicitamente se anúncios são permitidos
5. Frontend nunca decide isso sozinho

---

## 🔐 Regras de Segurança e Auditoria

- Toda verificação de plano é feita no backend
- Toda violação de limite deve gerar erro semântico
- Toda mudança de plano deve gerar evento auditável
- Logs de auditoria **não podem ser limitados por plano**

---

## 🚫 Anti-Padrões (PROIBIDOS)

Agentes IA **NÃO PODEM**:

- Criar lógica financeira condicionada a plano
- Bloquear auditoria por tipo de plano
- Implementar anúncios no backend
- Colocar regras de monetização no frontend
- Criar `if (plan == Free)` dentro do domínio financeiro

---

## 🔁 Evolução Futura Esperada

Este modelo **DEVE permitir**, sem quebra arquitetural:

- Cobrança automática
- Integração com gateways de pagamento
- Upgrade/downgrade automático
- Trials temporários

Sem alterar o domínio financeiro.

---

## ✅ Critério de Validação para Agentes IA

Uma execução é considerada **inválida** se:

- Qualquer regra acima for violada
- Monetização vazar para o domínio financeiro
- O frontend tomar decisões comerciais

> **Este documento tem força equivalente a ADR para agentes IA.**
> Deve ser anexado a planejamentos e execuções relacionadas a planos e assinaturas.