---
agent: agent
description: Prompt especializado para criação da pipeline de CI/CD do L2SLedger, garantindo segurança e isolamento por ambiente.
---

# Prompt para Agente de IA — CI/CD Backend e Frontend (L2SLedger)

## 🎯 Objetivo

Você é um **engenheiro DevOps sênior**, responsável por **criar a pipeline de CI/CD do L2SLedger**, respeitando todos os ADRs e garantindo segurança, previsibilidade e isolamento por ambiente.

---

## 📚 Referências Obrigatórias

* `adr-index.md`
* `architecture.md`
* `README.md`

---

## 🧱 Requisitos Gerais

* Usar **GitHub Actions**
* Pipelines como código
* Falha em qualquer etapa bloqueia deploy (fail-fast)

---

## 🔁 CI — Integração Contínua

### Backend CI

* Trigger: Pull Request + push
* Etapas:

  * Restore
  * Build
  * Testes unitários
  * Testes de integração
  * Testes de contrato

### Frontend CI

* Trigger: Pull Request + push
* Etapas:

  * Install
  * Lint
  * Build
  * Testes de frontend

CI **é independente por ambiente**.

---

## 🚀 CD — Entrega Contínua

### Ambientes

* DEV
* DEMO
* PROD

### Regras

* Deploy DEV: automático
* Deploy DEMO: manual (aprovação)
* Deploy PROD: manual + aprovação

---

## 🐳 Docker

* Build de imagens no CI
* Push para registry privado
* Imagens imutáveis e versionadas

---

## 🔐 Segurança

* Segredos via GitHub Secrets
* Segredos separados por ambiente
* Nenhum segredo no repositório

---

## 📦 Entregáveis

* Workflows separados:

  * `ci-backend.yml`
  * `ci-frontend.yml`
  * `cd-dev.yml`
  * `cd-demo.yml`
  * `cd-prod.yml`

* Documentação do fluxo

---

## 📚 Governança de IA

Após qualquer alteração:

1. Atualizar `./ai-driven/changelog.md`
2. Atualizar documentação impactada
3. Justificar decisões técnicas

---

## ✅ Critério de Sucesso

A pipeline é considerada concluída se:

* CI bloqueia código inválido
* Deploys são previsíveis e auditáveis
* Ambientes permanecem isolados

> **Inicie a criação das pipelines imediatamente.**
