# Fase 6: Correção de Bugs, Melhorias de Segurança, Usabilidade, QA e Otimização — Frontend L2SLedger

> **Data:** 2026-02-20  
> **Versão:** 1.0  
> **Status:** Em análise  
> **Dependência:** Fase 5 (Admin - Gestão de Usuários) completa  
> **Estimativa Total:** ~40 horas

---

## 📋 Índice

1. [Visão Geral](#1-visão-geral)
2. [Bugs — Dashboard](#2-bugs--dashboard)
3. [Bugs — Layout Mobile](#3-bugs--layout-mobile)
4. [Bugs — Layout Desktop](#4-bugs--layout-desktop)
5. [Melhorias de Segurança](#5-melhorias-de-segurança)
6. [Melhorias de Usabilidade](#6-melhorias-de-usabilidade)
7. [Melhorias de QA](#7-melhorias-de-qa)
8. [Otimização de Aplicação](#8-otimização-de-aplicação)
9. [Critérios de Aceite Globais](#9-critérios-de-aceite-globais)
10. [ADRs e Governança](#10-adrs-e-governança)

---

## 1. Visão Geral

### 1.1 Propósito

Esta fase consolida **todos os bugs identificados**, **melhorias de segurança**, **usabilidade**, **QA** e **otimização** a serem executados após a conclusão da Fase 5 (Admin - Gestão de Usuários). O objetivo é estabilizar a aplicação antes de avançar para funcionalidades posteriores ao MVP.

### 1.2 Priorização

| Prioridade | Categoria | Justificativa |
|------------|-----------|---------------|
| **P0 — Crítica** | Bugs de Dashboard (dados financeiros não exibidos) | Funcionalidade core quebrada |
| **P0 — Crítica** | Segurança (cookie/token, env vars) | Risco de exposição |
| **P1 — Alta** | Bugs de layout mobile | UX degradada em dispositivos móveis |
| **P1 — Alta** | Bug de layout desktop (header/logo) | Identidade visual comprometida |
| **P2 — Média** | Melhorias de usabilidade (busca categoria) | Melhoria de UX |
| **P2 — Média** | Melhorias de QA | Qualidade e confiabilidade |
| **P3 — Baixa** | Otimização de bundle | Performance |

---

## 2. Bugs — Dashboard

**Estimativa:** 8 horas  
**Prioridade:** P0 — Crítica

### 2.1 Transações Recentes Não Listadas

**Arquivo afetado:** `features/dashboard/components/RecentTransactions.tsx`, `features/dashboard/services/dashboardService.ts`

**Causa raiz identificada:** O serviço `getRecentTransactions()` chama o endpoint genérico de transações com `pageSize: 5` e extrai `response.data`. Se a API retorna um envelope diferente (ex: `{ items: [...] }` ou `{ transactions: [...] }`), `response.data` retorna `undefined`, e o componente exibe "Nenhuma transação encontrada" ao invés dos dados reais.

**Ação corretiva:**

| # | Task | Descrição |
|---|------|-----------|
| 2.1.1 | Investigar contrato da API | Verificar o formato de resposta real do endpoint `GET /api/v1/transactions?pageSize=5&page=1` e documentar o envelope |
| 2.1.2 | Ajustar `dashboardService.ts` | Corrigir o mapeamento da resposta para extrair corretamente a lista de transações do envelope da API |
| 2.1.3 | Tratar estado de erro | Verificar se `useRecentTransactions` propaga erros corretamente e exibir feedback visual ao usuário |
| 2.1.4 | Testes unitários | Criar/atualizar testes para `useRecentTransactions` com mock do formato correto da API |
| 2.1.5 | Testar integração | Validar com backend real que as transações são listadas |

---

### 2.2 Saldo Atual Não Contabilizado

**Arquivo afetado:** `features/dashboard/pages/DashboardPage.tsx`, `features/dashboard/hooks/useBalances.ts`, `features/dashboard/services/dashboardService.ts`

**Causa raiz identificada:** O hook `useBalances()` chama `dashboardService.getBalances()` que acessa `API_ENDPOINTS.BALANCES`. Se o endpoint não existe, retorna erro ou retorna um formato diferente, `balances` fica `undefined` e todos os cards exibem `0`. Adicionalmente, o estado `isError` retornado por `useBalances` **não é consumido** no `DashboardPage`, ou seja, erros são silenciados.

**Ação corretiva:**

| # | Task | Descrição |
|---|------|-----------|
| 2.2.1 | Validar endpoint de saldos | Confirmar que `GET /api/v1/balances` existe e retorna `{ totalIncome, totalExpense, currentBalance }` |
| 2.2.2 | Ajustar mapeamento | Corrigir `dashboardService.getBalances()` para mapear corretamente a resposta da API |
| 2.2.3 | Consumir `isError` no Dashboard | Exibir estado de erro nos `BalanceCard` quando `isError` for `true` |
| 2.2.4 | Implementar fallback visual | Exibir skeleton ou mensagem "Erro ao carregar saldos" ao invés de `R$ 0,00` silencioso |
| 2.2.5 | Testes unitários | Testar cenários de sucesso, erro e loading do `useBalances` |

---

### 2.3 Gráfico BalanceChart em Branco

**Arquivo afetado:** `features/dashboard/components/BalanceChart.tsx`, `features/dashboard/hooks/useDailyBalances.ts`

**Causa raiz identificada:** O hook `useDailyBalances()` é chamado **sem parâmetros** de data (`startDate` e `endDate` são `undefined`). Dependendo da implementação do `apiClient`, esses valores podem ser enviados como a string literal `"undefined"` nos query params, causando erro no backend. Além disso, se o backend exige esses parâmetros, retorna erro ou conjunto vazio.

**Ação corretiva:**

| # | Task | Descrição |
|---|------|-----------|
| 2.3.1 | Definir período padrão | Implementar cálculo automático de período padrão (ex: últimos 30 dias) quando nenhum período é informado |
| 2.3.2 | Sanitizar parâmetros | Garantir que `undefined` não seja enviado como string nos query params do `apiClient` |
| 2.3.3 | Validar formato de dados | Confirmar que o mapeamento `date → data` (renomeação da propriedade para PT-BR) está correto |
| 2.3.4 | Tratar erros no chart | Exibir mensagem de erro significativa ao invés de tela branca quando o carregamento falha |
| 2.3.5 | Testes unitários | Testar `useDailyBalances` com e sem parâmetros de data |

---

## 3. Bugs — Layout Mobile

**Estimativa:** 10 horas  
**Prioridade:** P1 — Alta

### 3.1 MobileNav Sobrepondo Conteúdo

**Arquivo afetado:** `shared/components/layout/AppLayout.tsx`, `shared/components/layout/MobileNav.tsx`

**Causa raiz identificada:** O `MobileNav` usa `fixed bottom-0` com altura `h-16` (64px), porém o `<main>` não possui `padding-bottom` suficiente para compensar essa barra fixa. O último item do Dashboard (Transações Recentes) fica parcialmente coberto pela navegação inferior.

**Ação corretiva:**

| # | Task | Descrição |
|---|------|-----------|
| 3.1.1 | Adicionar `pb-20` ao `<main>` | No `AppLayout.tsx`, quando `isMobile` for `true`, adicionar `pb-20` (80px para folga extra) ao container principal `<main>` |
| 3.1.2 | Testar em múltiplas resoluções | Validar em 320px, 375px, 414px, 768px que nenhum conteúdo fica oculto |
| 3.1.3 | Testar com scroll | Garantir que o último item é totalmente visível ao realizar scroll até final da página |

---

### 3.2 Opções Admin Ausentes no Mobile

**Arquivo afetado:** `shared/components/layout/MobileNav.tsx`

**Causa raiz identificada:** O `MobileNav` possui `mobileNavItems` **hardcoded** com apenas 3 itens (Dashboard, Transações, Categorias). Diferente do `Sidebar.tsx` que verifica `isAdmin` e exibe condicionalmente o link para "Usuários", o `MobileNav` não importa `useAuth`, não verifica roles, e não inclui `ROUTES.ADMIN_USERS`.

**Ação corretiva:**

| # | Task | Descrição |
|---|------|-----------|
| 3.2.1 | Importar `useAuth` no `MobileNav` | Obter `currentUser` e verificar role Admin |
| 3.2.2 | Adicionar item Admin condicional | Quando o usuário tiver role `Admin`, exibir ícone/link para `ROUTES.ADMIN_USERS` na barra inferior |
| 3.2.3 | Adaptar layout da nav | Ajustar o grid/flex da barra inferior para acomodar 4 itens quando Admin (3 itens para usuários normais) |
| 3.2.4 | Manter consistência visual | Garantir que o item Admin segue o mesmo padrão visual dos demais itens |
| 3.2.5 | Testes | Testar que usuário Admin vê 4 itens e usuário regular vê 3 itens |

---

### 3.3 Layouts Quebrados em Transações e Categorias (Mobile)

**Arquivo afetado:** `shared/components/layout/AppLayout.tsx`, `shared/components/layout/Header.tsx`, `shared/components/layout/MobileNav.tsx`

**Causa raiz identificada:** Ao navegar para páginas de Transações ou Categorias no mobile, tanto a Nav inferior (MobileNav) quanto a Nav superior (Header) apresentam layouts quebrados. Possíveis causas:
- Header não se adapta corretamente ao conteúdo das páginas internas
- Conflito de z-index entre Header (`z-40`) e MobileNav (`z-50`)
- Overflow do conteúdo afetando o posicionamento fixo das navs
- Componentes internos (tabelas, formulários) quebrando o flex container

**Ação corretiva:**

| # | Task | Descrição |
|---|------|-----------|
| 3.3.1 | Diagnosticar breakpoints | Usar DevTools mobile para identificar exatamente quais CSS properties estão quebrando em cada página |
| 3.3.2 | Revisar Header responsivo | Garantir que `Header.tsx` usa `w-full`, `overflow-hidden` e se adapta ao viewport mobile |
| 3.3.3 | Revisar MobileNav responsivo | Garantir que `MobileNav.tsx` mantém `fixed bottom-0 left-0 right-0` independentemente do conteúdo da página |
| 3.3.4 | Isolar scroll do `<main>` | Garantir que `overflow-y-auto` está apenas no `<main>` e não propaga para os elementos fixos |
| 3.3.5 | Revisar tabelas responsivas | Garantir que `TransactionList` e `CategoryList` usam `overflow-x-auto` em containers mobile |
| 3.3.6 | Testes visuais | Testar todas as páginas (Dashboard, Transações, Categorias, Admin) em viewports de 320px a 768px |

---

## 4. Bugs — Layout Desktop

**Estimativa:** 3 horas  
**Prioridade:** P1 — Alta

### 4.1 Logo L2SLEDGER Desaparece ao Rolar

**Arquivo afetado:** `shared/components/layout/Sidebar.tsx`, `shared/components/layout/AppLayout.tsx`

**Causa raiz identificada:** O `Sidebar` contém o logo L2SLEDGER mas não possui posicionamento `sticky` ou `fixed`. Ele é um elemento `flex w-64 flex-col border-r` dentro do flex container da página. Quando o conteúdo da página excede o viewport e o container externo (`min-h-screen`) realiza scroll, o Sidebar (incluindo o logo) rola junto. O `Header` possui `sticky top-0 z-40`, mas como funciona dentro de um flex column no container que rola, perde efetividade.

**Ação corretiva:**

| # | Task | Descrição |
|---|------|-----------|
| 4.1.1 | Tornar Sidebar fixo/sticky | Aplicar `sticky top-0 h-screen overflow-y-auto` ao `Sidebar.tsx` para que fique fixo durante scroll |
| 4.1.2 | Revisar estrutura flex | Garantir que o container externo do `AppLayout` não rola — apenas o `<main>` deve ter `overflow-y-auto` |
| 4.1.3 | Garantir logo visível | Validar que o logo permanece visível em qualquer posição de scroll do conteúdo principal |
| 4.1.4 | Testar com conteúdo longo | Testar com lista extensa de transações/categorias que force scroll vertical |

---

## 5. Melhorias de Segurança

**Estimativa:** 8 horas  
**Prioridade:** P0 — Crítica

### 5.1 Ciclo de Vida do Cookie / Validação de Token Firebase

**Arquivos afetados:** Backend (Cookie emitter), `app/providers/AuthProvider.tsx`, `shared/lib/api/client.ts`

**Contexto:** Atualmente o cookie emitido pelo backend pode ter um tempo de vida excessivo, permitindo sessões ativas mesmo após o token Firebase expirar. Devemos garantir que a sessão backend reflita a validade real do token Firebase.

**Opções de implementação (requer análise e ADR):**

| Opção | Descrição | Prós | Contras |
|-------|-----------|------|---------|
| **A — Diminuir TTL do cookie** | Reduzir o `Max-Age` do cookie para valor mais curto (ex: 15-30 min) | Simples, reduz janela de exposição | Usuário precisa re-autenticar com mais frequência |
| **B — Validar + renovar cookie** | Backend valida token Firebase em cada requisição e renova cookie se válido | Sessão contínua enquanto Firebase token for válido | Overhead de validação por request |
| **C — Híbrido** | Cookie curto (1h) + endpoint de refresh que valida Firebase e emite novo cookie | Balanço entre UX e segurança | Complexidade adicional |

**Ação corretiva:**

| # | Task | Descrição |
|---|------|-----------|
| 5.1.1 | Analisar estratégia | Avaliar opções A, B e C com base em ADRs existentes e requisitos de UX |
| 5.1.2 | Criar ADR (se necessário) | Documentar decisão arquitetural sobre ciclo de vida do cookie |
| 5.1.3 | Implementar backend | Ajustar emissão/validação de cookie no backend .NET |
| 5.1.4 | Implementar frontend | Ajustar `AuthProvider` para lidar com renovação/expiração (interceptor de 401) |
| 5.1.5 | Implementar refresh silencioso | Se opção B ou C, implementar renovação automática de cookie sem interromper UX |
| 5.1.6 | Testes de sessão | Testar cenários: cookie expirado, token Firebase expirado, renovação bem-sucedida, logout forçado |

---

### 5.2 Exposição de Variáveis de Ambiente via `/env-config.js`

**Arquivos afetados:** `frontend/docker/env.sh`, `frontend/index.html`, `frontend/src/shared/lib/env.ts`

**Contexto:** O arquivo `/env-config.js` é gerado em runtime pelo `env.sh` e expõe **todas** as variáveis `VITE_*` em `window.__ENV__`. Embora a estrutura atual (geração em runtime, sem cache) seja correta, qualquer usuário pode acessar `https://dominio.com/env-config.js` e visualizar as variáveis de ambiente, incluindo potencialmente chaves de API do Firebase e URLs internas.

**Análise de risco:**

| Variável | Risco | Observação |
|----------|-------|------------|
| `VITE_FIREBASE_API_KEY` | Baixo | Chave pública do Firebase, protegida por regras de domínio |
| `VITE_FIREBASE_AUTH_DOMAIN` | Baixo | Informação pública necessária para auth |
| `VITE_API_BASE_URL` | Médio | Expõe URL interna do backend |
| Outras `VITE_*` | Variável | Depende do que for adicionado no futuro |

**Opções de mitigação:**

| Opção | Descrição | Ambiente |
|-------|-----------|----------|
| **A — Whitelist de variáveis** | `env.sh` itera apenas variáveis explicitamente listadas ao invés de todas `VITE_*` | Local + Deploy |
| **B — Variáveis inline no HTML** | Injetar variáveis diretamente no `index.html` via template no container, sem arquivo JS separado | Deploy |
| **C — Endpoint de configuração autenticado** | Criar endpoint `/api/v1/config/public` no backend que retorna configs públicas após autenticação | Local + Deploy |
| **D — Build-time injection** | Usar `VITE_*` apenas em build-time (`import.meta.env`), eliminando `env-config.js` por completo | Local (requer rebuild por ambiente para deploy) |

**Ação corretiva:**

| # | Task | Descrição |
|---|------|-----------|
| 5.2.1 | Auditar variáveis expostas | Listar todas `VITE_*` atualmente usadas e classificar nível de sensibilidade |
| 5.2.2 | Implementar whitelist | Modificar `env.sh` para exportar apenas variáveis explicitamente permitidas |
| 5.2.3 | Avaliar necessidade de endpoint | Se há variáveis sensíveis, implementar endpoint protegido ou injeção inline |
| 5.2.4 | Documentar decisão | Registrar estratégia escolhida e justificativa |
| 5.2.5 | Testar em ambos ambientes | Validar que a solução funciona tanto em `npm run dev` quanto em container Docker |

---

## 6. Melhorias de Usabilidade

**Estimativa:** 6 horas  
**Prioridade:** P2 — Média

### 6.1 Busca de Categoria PAI no Cadastro de Categorias

**Arquivo afetado:** `features/categories/components/CategoryForm.tsx`

**Contexto:** Atualmente o campo `parentCategoryId` é um `<Input>` de texto livre com placeholder "ID da categoria pai", exigindo que o usuário conheça e digite manualmente um UUID. Isso é inusável para o usuário final.

> ⚠️ **Esta melhoria possui planejamento detalhado separado.**
> Ver: [feature-busca-categoria-pai.md](./feature-busca-categoria-pai.md)

**Resumo da solução:**

| Componente | Descrição |
|------------|-----------|
| **Campo de busca** | Input com autocomplete que busca categorias pelo nome |
| **Lista de sugestões** | Dropdown com categorias filtradas, exibindo nome + tipo |
| **Seleção** | Ao selecionar, exibe apenas o nome da categoria ao usuário |
| **Envio** | O `parentCategoryId` (UUID) é enviado na requisição de forma transparente |
| **Limpeza** | Botão para limpar a categoria PAI selecionada |

**Ação corretiva:**

| # | Task | Descrição |
|---|------|-----------|
| 6.1.1 | Criar componente `CategorySearch` | Componente de busca com debounce e dropdown de sugestões |
| 6.1.2 | Integrar com `useCategories` | Reutilizar hook existente para obter lista de categorias |
| 6.1.3 | Implementar filtro client-side | Filtrar categorias por nome conforme digitação (com debounce de 300ms) |
| 6.1.4 | Substituir Input no `CategoryForm` | Substituir o campo texto pelo componente `CategorySearch` |
| 6.1.5 | Garantir compatibilidade com react-hook-form | Integrar via `Controller` ou `useController` do react-hook-form |
| 6.1.6 | Filtrar categorias compatíveis | Exibir apenas categorias do mesmo type (Income/Expense) como opções de PAI |
| 6.1.7 | Prevenir referência circular | Não exibir a própria categoria como opção (no modo edição) |
| 6.1.8 | Testes unitários | Testar busca, seleção, limpeza e envio do formulário |
| 6.1.9 | Stories Storybook | Criar story para o componente `CategorySearch` |

---

## 7. Melhorias de QA

**Estimativa:** 6 horas  
**Prioridade:** P2 — Média

### 7.1 Testes E2E com Backend

**Arquivos afetados:** `tests/e2e/*.spec.ts`

**Contexto:** Após integração com backend, testes E2E autenticados marcados com `.skip` devem ser habilitados.

| # | Task | Descrição |
|---|------|-----------|
| 7.1.1 | Inventariar testes `.skip` | Listar todos os testes E2E com `.skip` e classificar quais dependem de backend |
| 7.1.2 | Remover `.skip` gradualmente | Habilitar testes por módulo: auth → dashboard → categorias → transações → admin |
| 7.1.3 | Configurar ambiente de teste | Garantir que CI possui backend de teste disponível para E2E |
| 7.1.4 | Validar estabilidade | Executar suíte completa 3x para confirmar que não há flaky tests |

---

### 7.2 Visual QA via Storybook

**Arquivos afetados:** `.storybook/*`, `src/**/*.stories.tsx`

**Contexto:** O Storybook está falhando ao iniciar (`npm run storybook` exit code 1). Além disso, deve ser feito deploy no GitHub Pages para revisão visual dos componentes.

| # | Task | Descrição |
|---|------|-----------|
| 7.2.1 | Diagnosticar falha do Storybook | Analisar log de erro do `npm run storybook` e corrigir dependências ou configuração |
| 7.2.2 | Corrigir build do Storybook | Garantir que `npm run build-storybook` executa sem erros |
| 7.2.3 | Revisar stories existentes | Executar Storybook local e revisar cada componente visualmente |
| 7.2.4 | Adicionar stories faltantes | Identificar componentes sem stories e criar (priorizar componentes de layout e admin) |
| 7.2.5 | Configurar deploy GitHub Pages | Garantir que workflow `.github/workflows/deploy-storybook.yml` está funcional |
| 7.2.6 | Validar deploy | Confirmar que Storybook é acessível via GitHub Pages após push |

---

## 8. Otimização de Aplicação

**Estimativa:** 4 horas  
**Prioridade:** P3 — Baixa

### 8.1 Code Splitting do Tremor

**Arquivo afetado:** `vite.config.ts`

**Contexto:** A biblioteca `@tremor/react` (~906KB) está sendo incluída no chunk do Dashboard, inflando significativamente o bundle de páginas protegidas. Como o Tremor só é usado no `BalanceChart`, pode ser separado em seu próprio chunk.

| # | Task | Descrição |
|---|------|-----------|
| 8.1.1 | Analisar bundle atual | Executar `npx vite-bundle-visualizer` para mapa detalhado dos chunks |
| 8.1.2 | Criar chunk separado para Tremor | Adicionar `@tremor/react` ao `manualChunks` no `vite.config.ts` como chunk `charts` |
| 8.1.3 | Lazy load do BalanceChart | Garantir que `BalanceChart` usa `React.lazy()` para que o chunk `charts` só carregue quando necessário |
| 8.1.4 | Revisar double-bundling | Verificar se páginas de auth listadas em `manualChunks.public` e também lazy-loaded não estão duplicadas |
| 8.1.5 | Validar tamanhos | Comparar tamanhos de bundle antes e depois da otimização |
| 8.1.6 | Testar carregamento | Verificar via DevTools que chunks são carregados sob demanda |

---

## 9. Critérios de Aceite Globais

### Dashboard (P0)
- [ ] Transações recentes são listadas corretamente no Dashboard
- [ ] Saldo atual é calculado e exibido corretamente
- [ ] Gráfico `BalanceChart` carrega dados e exibe visualização corretamente
- [ ] Estados de erro são visíveis ao usuário (não silenciados)

### Mobile (P1)
- [ ] MobileNav não sobrepõe conteúdo — último item do Dashboard é totalmente visível
- [ ] Opções de Admin são visíveis na navegação mobile para usuários Admin
- [ ] Navegação e Header funcionam corretamente em todas as páginas no mobile (Transações, Categorias, Admin)
- [ ] Layouts testados em viewports de 320px a 768px

### Desktop (P1)
- [ ] Logo L2SLEDGER permanece visível independentemente da posição de scroll

### Segurança (P0)
- [ ] Estratégia de cookie/token definida e documentada
- [ ] Variáveis de ambiente não são expostas desnecessariamente
- [ ] Solução de env funciona tanto em ambiente local quanto em deploy

### Usabilidade (P2)
- [ ] Campo de busca de categoria pai funcional com autocomplete
- [ ] ID da categoria é transparente para o usuário

### QA (P2)
- [ ] Testes E2E `.skip` removidos e passando com backend
- [ ] Storybook funcional localmente
- [ ] Storybook com deploy correto no GitHub Pages

### Otimização (P3)
- [ ] Chunk do Tremor separado e carregado sob demanda
- [ ] Tamanho total do bundle protegido reduzido

---

## 10. ADRs e Governança

### ADRs Potencialmente Impactados

| ADR | Relação |
|-----|---------|
| ADR-005 (Autenticação Firebase) | Ciclo de vida do cookie/token |
| ADR-021-A (Códigos de Erro) | Tratamento de erros no Dashboard |
| ADR-016 (RBAC) | Admin nav no mobile |

### Novo ADR Necessário?

| Tópico | Necessidade |
|--------|-------------|
| Ciclo de vida do cookie | **Sim** — Se a estratégia de renovação mudar significativamente |
| Exposição de env vars | **Avaliar** — Depende da solução escolhida |
| Code splitting strategy | Não — Otimização interna sem impacto arquitetural |

### Regras de Segurança Aplicáveis
- Nenhuma lógica financeira no frontend
- Cookies HttpOnly + Secure + SameSite=Lax
- Tokens nunca armazenados no frontend
- Código protegido não carrega sem autenticação

---

## 📋 Resumo de Estimativas

| Categoria | Estimativa | Prioridade |
|-----------|------------|------------|
| Bugs Dashboard | 8h | P0 |
| Segurança | 8h | P0 |
| Bugs Mobile | 10h | P1 |
| Bugs Desktop | 3h | P1 |
| Usabilidade | 6h | P2 |
| QA | 6h | P2 |
| Otimização | 4h | P3 |
| **Total** | **~45h** | - |

---

## ➡️ Ordem de Execução Recomendada

```
1. [P0] Bugs Dashboard (2.1, 2.2, 2.3)
2. [P0] Segurança — Cookie/Token (5.1)
3. [P0] Segurança — Env vars (5.2)
4. [P1] Bugs Desktop — Logo (4.1)
5. [P1] Bugs Mobile — Nav sobrepondo (3.1)
6. [P1] Bugs Mobile — Admin nav (3.2)
7. [P1] Bugs Mobile — Layouts quebrados (3.3)
8. [P2] QA — Storybook (7.2)
9. [P2] QA — Testes E2E (7.1)
10. [P2] Usabilidade — Busca categoria (6.1)
11. [P3] Otimização — Bundle (8.1)
```

---

**Status:** Em análise  
**Próximo passo:** Aprovação do plano → Iniciar execução via `L2SLedger – Master.prompt.md`
