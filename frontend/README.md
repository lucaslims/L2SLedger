# L2SLedger Frontend

> **Sistema de Controle Financeiro**  
> Frontend SPA em React + TypeScript

---

## 🚀 Quick Start

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

## 📋 Scripts Disponíveis

| Script | Descrição |
|--------|-----------|
| `npm run dev` | Servidor de desenvolvimento (porta 3000) |
| `npm run build` | Build de produção |
| `npm run preview` | Preview do build |
| `npm test` | Rodar testes unitários |
| `npm run test:ui` | Rodar testes com UI |
| `npm run test:coverage` | Rodar testes com cobertura |
| `npm run lint` | Lint do código |
| `npm run lint:fix` | Fix automático de lint |
| `npm run format` | Formatar código |
| `npm run storybook` | Rodar Storybook |
| `npm run build-storybook` | Build do Storybook |

---

## 🏗️ Stack Tecnológica

- **React 18** + **TypeScript 5**
- **Vite** (bundler)
- **Tailwind CSS** (styling)
- **Shadcn/ui** (componentes)
- **React Router 6** (navegação)
- **TanStack Query** (server state)
- **React Hook Form** + **Zod** (formulários)
- **Firebase Auth** (autenticação)
- **Vitest** (testes)
- **Storybook** (documentação)

---

## 📁 Estrutura do Projeto

Ver [docs/planning/frontend-planning/SPEC.md](../docs/planning/frontend-planning/SPEC.md)

---

## 🔐 Segurança

- **Cookies HttpOnly** para sessão
- **Lazy loading** de código protegido
- **Guards** de autenticação e autorização
- **Nenhum token no frontend**

---

## 🧪 Testes

- Cobertura mínima: **85%**
- Testes unitários com **Vitest**
- Testes de componentes com **Testing Library**

---

## 📚 Documentação

- [SPEC.md](../docs/planning/frontend-planning/SPEC.md) — Especificação completa
- [ADR-040](../docs/adr/adr-040.md) — Estratégia de testes frontend
- [Architecture.md](../Architecture.md) — Visão geral do sistema

---

## 🔗 Integração com Backend

API Base URL: `http://localhost:5000/api/v1` (dev)

Ver: [frontend-api-integration-guide.md](../docs/planning/frontend-api-integration-guide.md)

---

## 📦 Build

```bash
npm run build
```

Gera:
- `dist/` — Arquivos estáticos
- Bundles separados por rota (code splitting)
- PWA manifest e service worker

---

## 🚢 Deploy

Ver [.github/workflows/](../.github/workflows/) para pipelines de CI/CD.

---

**Desenvolvido com ❤️ para o L2SLedger**
