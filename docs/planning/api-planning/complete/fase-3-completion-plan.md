# Plano Técnico - Conclusão da Fase 3: Módulo de Categorias

> **Status:** Implementação Completa  
> **Data:** 2026-01-15  
> **Versão:** 1.0  
> **Objetivo:** Completar Fase 3 com testes + seed (100%)

---

## 📊 Contexto

A Fase 3 está **95% implementada**:
- ✅ Domain Layer completo
- ✅ Application Layer completo (5 use cases)
- ✅ Infrastructure Layer completo (repository + migration)
- ✅ API Layer completo (5 endpoints)
- ✅ Build: SUCCESS (9 projetos compilando)
- ❌ **Faltam**: Testes (55+ testes) + Seed de categorias

---

## 🎯 Objetivos

1. Implementar **55+ testes** seguindo padrão existente
2. Implementar **seed de categorias padrão** (ADR-029)
3. Garantir **100% coverage** da Fase 3
4. Manter **ADRs aplicados** (020, 021, 022, 029, 034, 037)

---

## 📋 Plano de Implementação

### **1. Domain.Tests - CategoryTests.cs**

**Localização:** `backend/tests/L2SLedger.Domain.Tests/Entities/CategoryTests.cs`

**Padrão:** Seguir `UserTests.cs` (xUnit + FluentAssertions)

**Testes (12 testes):**

#### Constructor e Validações (4 testes)
1. `Constructor_ShouldCreateCategoryWithDefaultValues`
   - Validar: Id, Name, IsActive=true, CreatedAt
2. `Constructor_WithEmptyName_ShouldThrowBusinessRuleException`
   - Código: CAT_INVALID_NAME
3. `Constructor_WithNameTooLong_ShouldThrowBusinessRuleException`
   - Nome > 100 caracteres
   - Código: CAT_NAME_TOO_LONG
4. `Constructor_WithParentCategoryId_ShouldSetParentCorrectly`

#### UpdateName (3 testes)
5. `UpdateName_WithValidName_ShouldUpdateNameAndTimestamp`
6. `UpdateName_WithEmptyName_ShouldThrowBusinessRuleException`
7. `UpdateName_WithNameTooLong_ShouldThrowBusinessRuleException`

#### UpdateDescription (2 testes)
8. `UpdateDescription_WithValidDescription_ShouldUpdateDescriptionAndTimestamp`
9. `UpdateDescription_WithNull_ShouldSetDescriptionToNull`

#### Activate/Deactivate (2 testes)
10. `Deactivate_ShouldSetIsActiveToFalse`
11. `Activate_ShouldSetIsActiveToTrue`

#### Hierarquia (1 teste)
12. `CanHaveSubCategories_ShouldReturnTrueForRootCategory`
13. `CanHaveSubCategories_ShouldReturnFalseForSubCategory`

---

### **2. Application.Tests - Categories Use Cases**

**Localização:** `backend/tests/L2SLedger.Application.Tests/UseCases/Categories/`

**Padrão:** Seguir `AuthenticationServiceTests.cs` (Moq + AutoMapper + FluentAssertions)

#### **2.1 CreateCategoryUseCaseTests.cs (8 testes)**

**Setup:** Mock ICategoryRepository, IMapper, IValidator

1. `ExecuteAsync_WithValidRequest_ShouldCreateCategory`
2. `ExecuteAsync_WithInvalidRequest_ShouldThrowFluentValidationException`
3. `ExecuteAsync_WithDuplicateName_ShouldThrowBusinessRuleException`
4. `ExecuteAsync_WithInvalidParentId_ShouldThrowBusinessRuleException`
   - Parent não existe
5. `ExecuteAsync_WithParentAsSubCategory_ShouldThrowBusinessRuleException`
   - Parent já é subcategoria (max 2 níveis)
6. `ExecuteAsync_WithValidParent_ShouldCreateSubCategory`
7. `ExecuteAsync_ShouldCallRepositoryAddAsync`
8. `ExecuteAsync_ShouldReturnMappedCategoryDto`

#### **2.2 UpdateCategoryUseCaseTests.cs (8 testes)**

1. `ExecuteAsync_WithValidRequest_ShouldUpdateCategory`
2. `ExecuteAsync_WithInvalidRequest_ShouldThrowFluentValidationException`
3. `ExecuteAsync_WithNonExistentCategory_ShouldThrowBusinessRuleException`
4. `ExecuteAsync_WithDuplicateName_ShouldThrowBusinessRuleException`
5. `ExecuteAsync_ChangingToInvalidParent_ShouldThrowBusinessRuleException`
6. `ExecuteAsync_ShouldUpdateNameAndDescription`
7. `ExecuteAsync_ShouldCallRepositoryUpdateAsync`
8. `ExecuteAsync_ShouldReturnMappedCategoryDto`

#### **2.3 GetCategoriesUseCaseTests.cs (6 testes)**

1. `ExecuteAsync_WithoutFilters_ShouldReturnAllActiveCategories`
2. `ExecuteAsync_WithIncludeInactive_ShouldReturnAllCategories`
3. `ExecuteAsync_WithParentCategoryId_ShouldReturnSubCategories`
4. `ExecuteAsync_WithEmptyResult_ShouldReturnEmptyList`
5. `ExecuteAsync_ShouldCallRepositoryWithCorrectParameters`
6. `ExecuteAsync_ShouldReturnMappedCategoryDtos`

#### **2.4 GetCategoryByIdUseCaseTests.cs (4 testes)**

1. `ExecuteAsync_WithValidId_ShouldReturnCategory`
2. `ExecuteAsync_WithNonExistentId_ShouldThrowBusinessRuleException`
   - Código: FIN_CATEGORY_NOT_FOUND
3. `ExecuteAsync_ShouldCallRepositoryGetByIdAsync`
4. `ExecuteAsync_ShouldReturnMappedCategoryDto`

#### **2.5 DeactivateCategoryUseCaseTests.cs (6 testes)**

1. `ExecuteAsync_WithValidId_ShouldDeactivateCategory`
2. `ExecuteAsync_WithNonExistentId_ShouldThrowBusinessRuleException`
3. `ExecuteAsync_WithAlreadyInactiveCategory_ShouldNotThrow`
4. `ExecuteAsync_WithActiveSubCategories_ShouldDeactivateAll`
   - Desativar pai desativa filhos
5. `ExecuteAsync_ShouldCallRepositoryDeleteAsync`
6. `ExecuteAsync_ShouldReturnSuccess`

**Total Application.Tests: 32 testes**

---

### **3. Contract.Tests - CategoryDtoTests.cs**

**Localização:** `backend/tests/L2SLedger.Contract.Tests/DTOs/CategoryDtoTests.cs`

**Padrão:** Seguir `AuthDtoContractTests.cs`

**Testes (8 testes):**

#### CategoryDto (3 testes)
1. `CategoryDto_ShouldHaveAllRequiredProperties`
   - Id, Name, Description, IsActive, ParentCategoryId, ParentCategoryName, CreatedAt, UpdatedAt
2. `CategoryDto_ShouldSerializeCorrectly`
   - JSON camelCase
3. `CategoryDto_ShouldDeserializeCorrectly`

#### CreateCategoryRequest (2 testes)
4. `CreateCategoryRequest_ShouldHaveRequiredProperties`
   - Name, Description, ParentCategoryId
5. `CreateCategoryRequest_ShouldSerializeCorrectly`

#### UpdateCategoryRequest (2 testes)
6. `UpdateCategoryRequest_ShouldHaveRequiredProperties`
   - Name, Description
7. `UpdateCategoryRequest_ShouldSerializeCorrectly`

#### GetCategoriesResponse (1 teste)
8. `GetCategoriesResponse_ShouldHaveRequiredProperties`
   - Categories, TotalCount

**Total Contract.Tests: 8 testes**

---

### **4. Seed de Categorias Padrão (ADR-029)**

**Objetivo:** Criar categorias padrão para DEV/DEMO

#### **4.1 Implementação**

**Arquivo:** `backend/src/L2SLedger.Infrastructure/Persistence/Seeds/CategorySeeder.cs`

```csharp
public class CategorySeeder
{
    public static async Task SeedAsync(L2SLedgerDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return; // Já existem categorias

        var categories = new List<Category>
        {
            // Receitas (categorias raiz)
            new Category("Salário", "Rendimentos de trabalho formal"),
            new Category("Freelance", "Trabalhos autônomos e projetos"),
            new Category("Investimentos", "Rendimentos de aplicações financeiras"),
            
            // Despesas (categorias raiz)
            new Category("Alimentação", "Gastos com alimentação"),
            new Category("Transporte", "Gastos com deslocamento"),
            new Category("Moradia", "Aluguel, condomínio, IPTU"),
            new Category("Saúde", "Planos de saúde, medicamentos, consultas"),
            new Category("Lazer", "Entretenimento e diversão")
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }
}
```

#### **4.2 Integração no Program.cs**

```csharp
// Aplicar migrations e seed em Development
if (app.Environment.IsDevelopment())
{
    await app.ApplyMigrationsAsync();
    
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<L2SLedgerDbContext>();
    await CategorySeeder.SeedAsync(dbContext);
}
```

---

## 📊 Resumo de Testes

| Camada | Arquivo | Testes |
|--------|---------|--------|
| Domain | CategoryTests.cs | 13 |
| Application | CreateCategoryUseCaseTests.cs | 8 |
| Application | UpdateCategoryUseCaseTests.cs | 8 |
| Application | GetCategoriesUseCaseTests.cs | 6 |
| Application | GetCategoryByIdUseCaseTests.cs | 4 |
| Application | DeactivateCategoryUseCaseTests.cs | 6 |
| Contract | CategoryDtoTests.cs | 8 |
| **TOTAL** | | **53 testes** |

---

## ✅ Critérios de Aceitação

### **Build**
- ✅ Todos os 9 projetos compilando sem erros
- ✅ Sem warnings críticos

### **Testes**
- ✅ 53+ novos testes implementados
- ✅ Todos os testes passando (90 total: 37 existentes + 53 novos)
- ✅ Coverage: 100% das classes de Categories

### **Seed**
- ✅ CategorySeeder implementado
- ✅ Seed executando apenas em DEV
- ✅ 8 categorias padrão criadas

### **ADRs**
- ✅ ADR-020: Clean Architecture respeitada
- ✅ ADR-021: Erros semânticos validados
- ✅ ADR-022: Contratos validados
- ✅ ADR-029: Seed implementado
- ✅ ADR-037: Estratégia de testes aplicada

---

## 🚀 Próximos Passos Após Aprovação

1. **Implementar testes Domain** (13 testes)
2. **Implementar testes Application** (32 testes)
3. **Implementar testes Contract** (8 testes)
4. **Implementar CategorySeeder** + integração
5. **Executar `dotnet test`** - validar 90 testes passando
6. **Atualizar STATUS.md** - marcar Fase 3 como 100%
7. **Atualizar changelog.md** - registrar conclusão

---

## 📝 Observações

- Padrão de código: Seguir testes existentes (User, Auth)
- Nomenclatura: PascalCase, sufixo `Tests`
- Assertions: FluentAssertions
- Mocks: Moq
- Framework: xUnit

---

## 🔗 Referências

- [UserTests.cs](c:\projects\projeto-financeiro\cash-flow\backend\tests\L2SLedger.Domain.Tests\Entities\UserTests.cs)
- [AuthenticationServiceTests.cs](c:\projects\projeto-financeiro\cash-flow\backend\tests\L2SLedger.Application.Tests\UseCases\Auth\AuthenticationServiceTests.cs)
- [AuthDtoContractTests.cs](c:\projects\projeto-financeiro\cash-flow\backend\tests\L2SLedger.Contract.Tests\DTOs\AuthDtoContractTests.cs)
- [ADR-029 - Seed de Dados](c:\projects\projeto-financeiro\cash-flow\docs\adr\adr-029.md)
- [ADR-037 - Estratégia de Testes](c:\projects\projeto-financeiro\cash-flow\docs\adr\adr-037.md)

---

**Aprovação necessária para prosseguir com a execução.**
