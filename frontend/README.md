# L2SLedger Frontend

> SPA em **React 18 + TypeScript** — camada de apresentação do sistema financeiro L2SLedger.

---

## Quick Start

```bash
# Instalar dependências
npm install

# Configurar environment
cp .env.example .env.development

# Rodar em desenvolvimento
npm run dev

# Build para produção
npm run build

# Rodar testes
npm test

# Rodar Storybook
npm run storybook
```

---

## Scripts

| Script | Descrição |
|--------|-----------|
| `npm run dev` | Servidor de desenvolvimento (porta 3000) |
| `npm run build` | Build de produção (`tsc` + `vite build`) |
| `npm run preview` | Preview do build |
| `npm test` | Testes unitários (Vitest) |
| `npm run test:ui` | Rodar testes com UI |
| `npm run test:coverage` | Testes com cobertura |
| `npm run lint` | Lint (ESLint) |
| `npm run lint:fix` | Fix automático de lint |
| `npm run format` | Formatação (Prettier) |
| `npm run format:check` | Verificação de formatação |
| `npm run storybook` | Storybook (porta 6006) |
| `npm run build-storybook` | Build do Storybook |
| `npm run type-check` | Verificação de tipos (TypeScript) |

---

## Stack

- **React 18** + **TypeScript 5** + **Vite**
- **Tailwind CSS** + **Shadcn/ui** (componentes)
- **React Router 6** (navegação)
- **TanStack Query** (server state)
- **React Hook Form** + **Zod** (formulários e validação)
- **Firebase Auth** (autenticação)
- **Vitest** + **Testing Library** (testes)
- **Storybook** (documentação de componentes)

---

## Estrutura

```
frontend/src/
├── app/          # App shell, providers e rotas
├── features/     # Módulos por feature
└── shared/       # Componentes, hooks e utils compartilhados
```

Especificação completa: [SPEC.md](../docs/planning/frontend-planning/SPEC.md)

---

## Princípios Arquiteturais

- **Nenhuma regra financeira** no frontend — apenas apresentação (ADR-027)
- Consome **contratos públicos imutáveis** da API (ADR-022)
- **Nenhum token armazenado** — sessão via cookies HttpOnly (ADR-004)
- Guards de autenticação e autorização (ADR-024, ADR-025)
- Lazy loading de rotas protegidas

---

## Segurança (ADR-001, ADR-004, ADR-005)

- **Cookies HttpOnly + Secure + SameSite=Lax** para sessão
- **Lazy loading** de código protegido
- **Guards** obrigatórios em rotas protegidas
- **Nenhum dado sensível no frontend** — tokens nunca armazenados
- **Logout** via revogação de sessão no backend (ADR-003)
- **Segurança de rede** via OCI Firewall (ADR-008)
- **Criptografia** de dados sensíveis em trânsito (ADR-018)
- **Auditoria de acesso** — tentativas negadas registradas (ADR-019)

---

## Testes (ADR-040)

- Cobertura mínima: **85%**
- Testes unitários com **Vitest**
- Testes de componentes com **Testing Library**
- Pipeline CI bloqueia merge em caso de falha

---

## Integração com Backend

API Base URL: `http://localhost:8080/api/v1` (dev)

Guia: [frontend-api-integration-guide.md](../docs/planning/frontend-api-integration-guide.md)

---

## Build e Deploy

```bash
npm run build    # Gera dist/ com code splitting por rota
```

Gera:

- `dist/` — Arquivos estáticos
- Bundles separados por rota (code splitting)
- PWA manifest e service worker para offline support

---

## 🚢 Deploy

Ver [docs/deployment/](../docs/deployment/README.md).

---

## Referências

- [ADR-040](../docs/adr/adr-040.md) — Estratégia de testes frontend
- [ADR-024](../docs/adr/adr-024.md) — Arquitetura de serviços, guards e UI
- [Architecture.md](../Architecture.md) — Visão arquitetural do sistema
- [Índice de ADRs](../docs/adr/adr-index.md) — 47 decisões registradas
