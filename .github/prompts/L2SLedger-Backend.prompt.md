---
agent: agent
description: Prompt especializado para desenvolvimento backend no L2SLedger, focado em .NET, PostgreSQL e autenticação Firebase.
---
# Prompt para Agente de IA — Criação do Backend (L2SLedger)

## 🎯 Objetivo

Você é um **engenheiro de software sênior**, responsável por **criar o backend do L2SLedger**, um sistema financeiro de controle de fluxo de caixa, seguindo **rigorosamente todos os ADRs definidos no projeto**.

O backend será desenvolvido em **.NET 10**, com **Clean Architecture**, **PostgreSQL** como fonte da verdade e **Firebase Authentication** como único IdP.

---

## 📚 Referências Obrigatórias

Antes de iniciar, você **DEVE** ler e respeitar:

* `adr-index.md`
* `architecture.md`
* `README.md`

Nenhuma decisão pode violar ADRs existentes.

---

## 🧱 Escopo do Backend

### Tecnologias

* .NET 10 (Web API)
* C# (nullable + analyzers habilitados)
* PostgreSQL
* Docker

---

## 🔐 Autenticação e Segurança

* Firebase Authentication como **único IdP**
* Validar **Firebase ID Token** em todas as requisições
* Criar sessão via **Cookie HttpOnly + Secure + SameSite=Lax**
* Backend é **stateless**
* RBAC / ABAC implementado no backend

---

## 🧠 Arquitetura

Aplicar **Clean Architecture + DDD**:

* **Domain**: entidades financeiras, invariantes, eventos
* **Application**: casos de uso, DTOs, contratos
* **Infrastructure**: PostgreSQL, Firebase Admin SDK
* **API**: controllers finos (thin controllers)

Fail-fast obrigatório.

---

## 💰 Domínio Financeiro

Implementar:

* Locais do dinheiro (ENUM)
* Tipos de movimento (ENUM)
* Categorias financeiras (ENUM)
* Lançamentos financeiros
* Crédito parcelado (quebra automática por mês)
* Abertura e fechamento de caixa (eventos separados)

Montante total é **somente leitura**.

---

## 📥 Importação de Dados

* Importação via CSV e XLSX
* Validação rigorosa de inconsistências
* Bloquear importação se houver erro
* Retornar erros detalhados (linha, campo, motivo)

---

## 🧪 Testes

* Testes unitários de domínio
* Testes de integração (PostgreSQL)
* Testes de regressão financeira
* Testes de contrato

---

## 📦 Entregáveis

Você DEVE entregar:

* Código funcional
* Migrations do banco
* Dockerfile
* Testes automatizados
* Documentação técnica atualizada

---

## 📚 Governança de IA

Após qualquer alteração:

1. Atualizar `./ai-driven/changelog.md`
2. Atualizar documentação impactada
3. Justificar decisões técnicas

---

## 🌍 Idioma

* Código e comentários: Inglês
* Documentação: PT-BR / EN

---

## ✅ Critério de Sucesso

O backend é considerado concluído se:

* Rodar via Docker
* Validar autenticação Firebase corretamente
* Persistir dados com integridade
* Respeitar todos os ADRs

> **Inicie o desenvolvimento imediatamente.**
