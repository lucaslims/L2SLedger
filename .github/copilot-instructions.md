# L2SLedger — AI Agent Instructions

> ⚠️ **Context Only**
> This document provides **technical context** for AI assistants.
> It does **NOT replace** agent prompts, ADRs, or governance rules.
> All executions must follow the official **Planejar → Aprovar → Executar** flow.

---

## 🎯 Purpose

L2SLedger is a financial cash-flow control system built with **Clean Architecture**, **DDD**, and **strict architectural governance** through ADRs.

This file exists to:

* Provide **high-level architectural context**
* Reduce incorrect assumptions by AI assistants
* Point to the **authoritative sources of truth**

---

## 📚 Authoritative References (MUST READ)

Before making **any change**, always consult:

* `docs/adr/adr-index.md` — Master index of architectural decisions
* `architecture.md` — System architecture overview
* `ai-driven/agent-rules.md` — Mandatory execution rules
* `docs/governance/flow-planejar-aprovar-executar.md`

ADRs are **immutable contracts**.

---

## 🧱 Architecture Overview

### Backend

* Clean Architecture layers:

  * **Domain**: Financial entities and business rules (no external dependencies)
  * **Application**: Use cases and orchestration
  * **Infrastructure**: Persistence and integrations
  * **API**: Thin controllers only

> Dependencies always point inward.

### Frontend

* React + TypeScript SPA
* No financial logic in frontend
* Consumes immutable public contracts

---

## 🔐 Security Summary

* Firebase Authentication is the **only IdP**
* Backend validates Firebase ID Tokens
* Sessions via **HttpOnly + Secure + SameSite=Lax cookies**
* Tokens must **never** be stored in the frontend

---

## 📋 API & Error Contracts

* Public API contracts are **immutable**
* Breaking changes require versioning and ADR
* Errors are semantic and fail-fast

Error example:

```json
{
  "error": {
    "code": "AUTH_INVALID_TOKEN",
    "message": "Token expired",
    "timestamp": "2026-01-10T10:30:00Z",
    "traceId": "abc123"
  }
}
```

---

## 🧪 Tests & Documentation

* Tests are **mandatory when applicable**
* Documentation must be updated when impacted

> Follow the rules defined in `ai-driven/agent-rules.md`.

---

## 🌍 Environments

* DEV / DEMO / PROD are fully isolated
* Databases and configs never mix between environments

---

## 🚀 Infrastructure Summary

* Docker-based services
* CI/CD via GitHub Actions
* Hosted on OCI
* Frontend and Backend run in same VM/network

---

## 🚫 Absolute Constraints

AI assistants must **never**:

* Violate ADRs
* Introduce financial logic in frontend
* Change public contracts without versioning
* Skip tests or documentation updates

---

## 🧠 Final Note

When in doubt:

1. Check ADRs
2. Check governance documents
3. Check `agent-rules.md`
4. Always update `\ai-driven/changelog.md` with a summary of changes made

If a change requires architectural deviation, **a new ADR is mandatory**.

---