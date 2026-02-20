# Feature: Busca de Categoria PAI no Cadastro de Categorias — L2SLedger

> **Uso obrigatório antes de qualquer implementação de feature**
>
> Este template deve ser utilizado em conjunto com o prompt:
> **`L2SLedger – Planner.prompt.md`**

---

## 📌 Identificação

* **Nome da Feature:** Busca de Categoria PAI por Nome (Autocomplete)
* **Responsável pelo Planejamento:** L2SLedger – Master Agent
* **Data:** 2026-02-20
* **Status do Planejamento:** Em análise

---

## 🎯 Contexto

Atualmente, o campo `parentCategoryId` no formulário de cadastro/edição de categorias é um `<Input>` de texto livre com placeholder "ID da categoria pai". O usuário precisa conhecer e digitar manualmente um UUID para associar uma categoria PAI.

* **Qual dor ela resolve?** Usabilidade extremamente baixa — impossível para um usuário final saber UUIDs de cabeça.
* **Quem é impactado?** Todos os usuários que criam ou editam categorias com hierarquia.
* **Por que agora?** A funcionalidade de categorias (Fase 3) está implementada, mas esta lacuna de UX torna a hierarquia de categorias praticamente inutilizável em produção.

---

## 🧠 Objetivo da Feature

Substituir o campo de texto livre por um **componente de busca por nome com autocomplete**, permitindo ao usuário:

1. Digitar o nome de uma categoria para buscar
2. Ver sugestões filtradas em um dropdown
3. Selecionar uma categoria como PAI
4. Visualizar apenas o **nome** da categoria selecionada (ID transparente)
5. Limpar a seleção facilmente

---

## 🧩 Escopo Funcional

### Incluído

* Componente `CategorySearch` reutilizável com autocomplete
* Filtro client-side da lista de categorias por nome (com debounce)
* Integração com `react-hook-form` via `Controller`
* Filtro por tipo compatível (Income/Expense) — apenas categorias do mesmo tipo são exibidas
* **Travamento do campo tipo ao selecionar categoria PAI** — o tipo da categoria filho é automaticamente definido pelo tipo da categoria PAI selecionada, e o campo `type` é desabilitado com aviso informativo
* **Aviso visual** — Exibir alerta informando: "Categoria filho deve ser do mesmo tipo que a categoria pai. Para alterar o tipo, remova o vínculo com a categoria pai."
* **Desbloqueio do tipo ao remover PAI** — Ao limpar a seleção de categoria PAI, o campo `type` volta a ser editável
* Prevenção de referência circular (no modo edição, a própria categoria não aparece)
* Exibição do nome da categoria selecionada (transparência do UUID)
* Botão de limpeza da seleção
* Story Storybook para o componente
* Testes unitários do componente e integração com formulário

### Fora de Escopo

* Criação de nova categoria diretamente pelo autocomplete (inline create)
* Busca server-side (API search endpoint) — usa lista já carregada via `useCategories`
* Suporte a múltiplos níveis de hierarquia visual (árvore de categorias)
* Alteração no backend ou contrato da API

---

## 🏗️ Impactos Arquiteturais

### Backend

* [x] Nenhum impacto

Descrição: Nenhuma alteração necessária. O endpoint de categorias já retorna a lista completa, e o campo `parentCategoryId` na request de criação/edição já aceita um UUID opcionalmente.

---

### Frontend

* [x] Novas telas — **Não**
* [x] Alteração de fluxos existentes — **Sim**
* [x] Impacto em estado global — **Não**

Descrição:
- **Novo componente:** `CategorySearch.tsx` em `features/categories/components/`
- **Alteração:** `CategoryForm.tsx` para substituir o `<Input>` por `<CategorySearch>`
- **Reutilização:** Hook `useCategories` já existente para obter lista de categorias
- **Integração:** `react-hook-form` via `Controller` para manter compatibilidade com o formulário

---

### Banco de Dados

* [x] Nenhum impacto

---

### Segurança

* [x] Nenhum impacto

---

### CI/CD & Infraestrutura

* [x] Nenhum impacto

---

## 📚 ADRs Relacionados

* Nenhum ADR diretamente impactado.

### Novo ADR é necessário?

* [x] Não

---

## ⚠️ Riscos Identificados

| Risco | Severidade | Probabilidade |
|-------|-----------|---------------|
| Lista de categorias muito grande (100+) degrada UX do dropdown | Baixa | Baixa |
| Usuário não compreende por que o campo tipo está desabilitado | Média | Baixa |

Mitigações propostas:

* Lista grande: O filtro por nome + debounce reduz itens exibidos. Se necessário no futuro, limitar a 20 sugestões visíveis com scroll.
* Tipo travado: Exibir aviso visual claro (alert/banner) explicando que a categoria filho herda o tipo da PAI e orientando o usuário a remover o vínculo caso deseje alterar o tipo.

---

## 🧪 Estratégia de Testes

* [x] Testes de frontend (unitários + integração)
* [x] Testes manuais

Descrição:

| Teste | Tipo | Descrição |
|-------|------|-----------|
| Renderização do componente | Unit | `CategorySearch` renderiza input e dropdown |
| Busca por nome | Unit | Filtro funciona corretamente com debounce |
| Seleção de categoria | Unit | Ao clicar, exibe nome e armazena ID |
| Limpeza de seleção | Unit | Botão limpar reseta campo para vazio |
| Integração com formulário | Integration | `CategoryForm` envia `parentCategoryId` correto |
| Filtro por tipo | Unit | Apenas categorias do mesmo tipo aparecem |
| Travamento de tipo | Unit | Campo `type` é desabilitado ao selecionar categoria PAI |
| Aviso de tipo travado | Unit | Mensagem informativa é exibida quando tipo está travado |
| Desbloqueio de tipo | Unit | Campo `type` volta a ser editável ao remover categoria PAI |
| Herança de tipo | Unit | Tipo da categoria é atualizado para o tipo da PAI ao selecionar |
| Prevenção circular | Unit | Modo edição não mostra a própria categoria |
| Acessibilidade | Unit | Navegação por teclado (ArrowDown, Enter, Escape) |
| Story visual | Storybook | Validação visual do componente em diferentes estados |

---

## 📄 Impacto em Documentação

* [ ] README.md — Não
* [ ] Architecture.md — Não
* [ ] ADR — Não
* [x] Outras documentações — `ai-driven/changelog.md` (registro da mudança)

---

## 🤖 Agentes que Serão Acionados

* [x] L2SLedger – Master
* [ ] L2SLedger – Backend
* [x] L2SLedger – Frontend
* [ ] L2SLedger – CI/CD

---

## 📋 Checklist de Pré-Aprovação

* [x] Planejamento completo
* [x] Escopo claro
* [x] Impactos avaliados
* [x] Riscos mapeados
* [x] ADRs identificados

---

## ✅ Aprovação

* **Aprovado por:**
* **Data:**
* **Observações:**

---

## 🧩 Especificação Técnica Detalhada

### Novo Componente: `CategorySearch`

**Localização:** `features/categories/components/CategorySearch.tsx`

```
┌─────────────────────────────────────────────┐
│  Categoria PAI (opcional)                   │
│ ┌─────────────────────────────────────────┐ │
│ │ 🔍 Buscar categoria...            [x]  │ │
│ └─────────────────────────────────────────┘ │
│ ┌─────────────────────────────────────────┐ │
│ │  Alimentação          (Despesa)     ▸   │ │
│ │  Aluguel              (Despesa)     ▸   │ │
│ │  Automóvel            (Despesa)     ▸   │ │
│ └─────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
```

**Após seleção:**

```
┌─────────────────────────────────────────────┐
│  Categoria PAI (opcional)                   │
│ ┌─────────────────────────────────────────┐ │
│ │  ✓ Alimentação                     [x]  │ │
│ └─────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
```

### Props do Componente

```typescript
interface CategorySearchProps {
  /** Valor atual (categoryId) */
  value?: string;
  /** Callback quando categoria é selecionada — retorna ID e tipo da PAI */
  onChange: (categoryId: string | undefined, parentType?: 'Income' | 'Expense') => void;
  /** Tipo da categoria (Income/Expense) para filtrar compatíveis */
  categoryType?: 'Income' | 'Expense';
  /** ID da categoria sendo editada (para excluir de opções) */
  excludeCategoryId?: string;
  /** Placeholder customizado */
  placeholder?: string;
  /** Desabilitado */
  disabled?: boolean;
  /** Mensagem de erro do formulário */
  error?: string;
}
```

### Comportamento

| Ação | Resultado |
|------|-----------|
| Foco no input | Exibe dropdown com todas as categorias compatíveis |
| Digitar texto | Filtra categorias por nome (debounce 300ms) |
| Clicar em sugestão | Armazena `categoryId`, exibe nome da categoria, fecha dropdown |
| Tecla `Escape` | Fecha dropdown sem selecionar |
| Tecla `ArrowDown/Up` | Navega entre sugestões |
| Tecla `Enter` | Seleciona sugestão destacada |
| Clicar `[x]` | Limpa seleção, retorna campo para estado de busca, **desbloqueia campo `type`** |
| Clicar fora | Fecha dropdown |
| Selecionar categoria PAI | **Trava o campo `type`** com o tipo da PAI selecionada, exibe aviso informativo |
| Tentar alterar tipo com PAI selecionada | Campo `type` está desabilitado — usuário é orientado a remover o vínculo com a PAI primeiro |

### Integração com `CategoryForm.tsx`

**Antes:**
```typescript
<Input placeholder="ID da categoria pai" {...field} />
```

**Depois:**
```typescript
// Ao selecionar PAI, travar tipo e exibir aviso
const handleParentChange = (categoryId: string | undefined, parentType?: 'Income' | 'Expense') => {
  setValue('parentCategoryId', categoryId);
  if (categoryId && parentType) {
    setValue('type', parentType); // herda tipo da PAI
    setIsTypeLocked(true);
  } else {
    setIsTypeLocked(false); // desbloqueia tipo
  }
};

// Campo de busca de categoria PAI
<Controller
  name="parentCategoryId"
  control={control}
  render={({ field }) => (
    <CategorySearch
      value={field.value}
      onChange={handleParentChange}
      categoryType={watchedType}
      excludeCategoryId={editingCategoryId}
      error={errors.parentCategoryId?.message}
    />
  )}
/>

// Campo type com travamento condicional
<Select disabled={isTypeLocked} ...>
  ...
</Select>
{isTypeLocked && (
  <p className="text-sm text-amber-600">
    Categoria filho deve ser do mesmo tipo que a categoria pai.
    Para alterar o tipo, remova o vínculo com a categoria pai.
  </p>
)}
```

---

## ➡️ Próximos Passos

Após aprovação:

1. Executar **Checklist Automático de Aprovação**
2. Iniciar execução via `L2SLedger – Master.prompt.md`
3. Implementar `CategorySearch` componente
4. Integrar com `CategoryForm`
5. Criar testes e stories
6. Registrar em `ai-driven/changelog.md`

> **Sem este template preenchido e aprovado, a execução da feature é proibida.**
