---
title: Pendências — Fase 8.1: Exportação de Relatórios
date: 2026-01-19
version: 1.0
dependencies:
  - Fase 8 (Exportação) base implementada
status: PENDENTE
---

# Fase 8.1: Pendências da Exportação de Relatórios — L2SLedger

## 📋 Contexto

Este documento lista **todas as pendências restantes** da Fase 8 (Exportação de Relatórios) identificadas durante a análise do arquivo `fase-8-exportacao.md`.

### Status Atual da Implementação

**Progresso Geral: ~56% (9.5/17 componentes)**

| Componente | Status | Progresso |
|------------|--------|-----------|
| Domain Layer | ✅ Completo | 100% |
| DTOs | ✅ Completo | 100% |
| Interfaces | ✅ Completo | 100% |
| Use Cases | ⏳ Parcial | 67% (4/6) |
| Validators | ✅ Completo | 100% |
| Services | ⏳ Parcial | 75% (3/4) |
| Configuration | ✅ Completo | 100% |
| Hosted Service | ✅ Completo | 100% |
| Controller | ⏳ Parcial | 67% (4/6 endpoints) |
| DI Configuration | ✅ Completo | 100% |
| Testes Domain | ❌ Pendente | 0% (0/8) |
| Testes Application | ❌ Pendente | 0% (0/15) |
| Testes Contract | ❌ Pendente | 0% (0/7) |
| Validação Manual | ⏳ Parcial | 60% |
| Documentação | ⏳ Parcial | 80% |

---

## 🎯 BLOCO 1: Use Cases Pendentes (PRIORIDADE ALTA)

### 1.1 GetExportsUseCase - Listar Exportações com Paginação

**Arquivo**: `backend/src/L2SLedger.Application/UseCases/Exports/GetExportsUseCase.cs`

**Status Atual**: Stub implementado (retorna lista vazia)

**Requisitos**:
- [ ] Buscar exportações via `IExportRepository.GetByFiltersAsync`
- [ ] Aplicar filtros: Status, Format, UserId
- [ ] Validar ownership (usuário vê apenas suas exportações, Admin vê todas)
- [ ] Implementar paginação (Page, PageSize)
- [ ] Buscar contagem total via `IExportRepository.CountByFiltersAsync`
- [ ] Mapear para `GetExportsResponse`
- [ ] Incluir dados do usuário (RequestedByUserName) via Include

**Critérios de Aceitação**:
- ✅ Retorna lista paginada correta
- ✅ Filtros funcionam individualmente e combinados
- ✅ TotalCount correto para paginação
- ✅ Admin consegue ver todas as exportações
- ✅ Usuário comum vê apenas suas próprias exportações
- ✅ OrderBy RequestedAt DESC (mais recentes primeiro)

**Estimativa**: 1 hora

---

### 1.2 DeleteExportUseCase - Soft Delete de Exportação

**Arquivo**: `backend/src/L2SLedger.Application/UseCases/Exports/DeleteExportUseCase.cs`

**Status Atual**: Stub implementado (retorna 204 sem lógica)

**Requisitos**:
- [ ] Buscar exportação por ID via `IExportRepository.GetByIdAsync`
- [ ] Validar se exportação existe (NotFoundException se não existir)
- [ ] Validar ownership - apenas Admin pode deletar
- [ ] Validar role Admin via `ICurrentUserService` (AuthorizationException se não for Admin)
- [ ] Aplicar soft delete via `Export.MarkAsDeleted()` (se método existir) ou `Export.SetDeleted()`
- [ ] Deletar arquivo físico via `IFileStorageService.DeleteExportFileAsync` (se FilePath não for nulo)
- [ ] Atualizar via `IExportRepository.UpdateAsync`
- [ ] Tratamento de erros adequado

**Critérios de Aceitação**:
- ✅ Soft delete funciona (IsDeleted = true)
- ✅ Arquivo físico é removido do sistema
- ✅ Apenas Admin pode executar
- ✅ NotFoundException se exportação não existir
- ✅ AuthorizationException se não for Admin
- ✅ Não falha se arquivo já foi deletado anteriormente

**Estimativa**: 1 hora

---

## 🎯 BLOCO 2: Controller - Integrar Use Cases (PRIORIDADE ALTA)

### 2.1 Atualizar ExportsController - GET /api/v1/exports

**Arquivo**: `backend/src/L2SLedger.API/Controllers/ExportsController.cs`

**Status Atual**: Stub retorna lista vazia

**Requisitos**:
- [ ] Injetar `GetExportsUseCase` no construtor
- [ ] Substituir stub por chamada ao Use Case
- [ ] Passar parâmetros: `GetExportsRequest` + `UserId` (via CurrentUserService)
- [ ] Retornar `ActionResult<GetExportsResponse>` com status 200
- [ ] Implementar tratamento de erros (500 para exceções não previstas)

**Critérios de Aceitação**:
- ✅ Endpoint retorna lista paginada real
- ✅ Filtros (Status, Format, Page, PageSize) funcionam corretamente
- ✅ Paginação funcional (TotalCount, Page, PageSize)

**Estimativa**: 30 minutos

---

### 2.2 Atualizar ExportsController - DELETE /api/v1/exports/{id}

**Arquivo**: `backend/src/L2SLedger.API/Controllers/ExportsController.cs`

**Status Atual**: Stub retorna 204 sem lógica

**Requisitos**:
- [ ] Injetar `DeleteExportUseCase` no construtor
- [ ] Substituir stub por chamada ao Use Case
- [ ] Passar parâmetros: `ExportId` + `UserId` (via CurrentUserService)
- [ ] Retornar 204 NoContent em sucesso
- [ ] Implementar tratamento de erros:
  - 404 se exportação não existir
  - 403 se não for Admin
  - 500 para exceções não previstas

**Critérios de Aceitação**:
- ✅ Endpoint deleta exportação corretamente (soft delete)
- ✅ Apenas Admin consegue executar
- ✅ Retorna 403 Forbidden se não for Admin
- ✅ Retorna 404 NotFound se exportação não existir
- ✅ Arquivo físico é removido

**Estimativa**: 30 minutos

---

## 🎯 BLOCO 3: Testes Domain Layer (PRIORIDADE MÉDIA)

### 3.1 ExportTests.cs - 8 Testes

**Arquivo**: `backend/tests/L2SLedger.Domain.Tests/Entities/ExportTests.cs` (CRIAR)

**Testes a Implementar**:

1. **Constructor_WithValidData_CreatesExportWithPendingStatus**
   - Validar que novo Export tem Status = Pending
   - Validar que RequestedAt é preenchido
   - Validar que ProcessingStartedAt e CompletedAt são nulos

2. **MarkAsProcessing_WithPendingStatus_UpdatesStatusAndTimestamp**
   - Validar mudança de status Pending → Processing
   - Validar que ProcessingStartedAt é preenchido
   - Validar que CompletedAt ainda é nulo

3. **MarkAsProcessing_WithNonPendingStatus_ThrowsInvalidOperationException**
   - Tentar marcar como Processing quando já está Processing/Completed/Failed
   - Validar exceção: `InvalidOperationException`

4. **MarkAsCompleted_WithProcessingStatus_UpdatesStatusAndMetadata**
   - Validar mudança de status Processing → Completed
   - Validar que FilePath, FileSizeBytes, RecordCount são preenchidos
   - Validar que CompletedAt é preenchido

5. **MarkAsCompleted_WithNonProcessingStatus_ThrowsInvalidOperationException**
   - Tentar marcar como Completed quando não está Processing
   - Validar exceção: `InvalidOperationException`

6. **MarkAsFailed_WithProcessingStatus_UpdatesStatusAndErrorMessage**
   - Validar mudança de status Processing → Failed
   - Validar que ErrorMessage é preenchido
   - Validar que CompletedAt é preenchido

7. **MarkAsFailed_WithNonProcessingStatus_ThrowsInvalidOperationException**
   - Tentar marcar como Failed quando não está Processing
   - Validar exceção: `InvalidOperationException`

8. **IsDownloadable_WithCompletedStatusAndFilePath_ReturnsTrue**
   - Validar que IsDownloadable() retorna true quando Status = Completed e FilePath != null
   - Validar que IsDownloadable() retorna false quando Status != Completed ou FilePath == null

**Critérios de Aceitação**: 8/8 testes passando ✅

**Estimativa**: 1.5 horas

---

## 🎯 BLOCO 4: Testes Application Layer (PRIORIDADE MÉDIA)

### 4.1 RequestExportUseCaseTests.cs - 4 Testes

**Arquivo**: `backend/tests/L2SLedger.Application.Tests/UseCases/Exports/RequestExportUseCaseTests.cs` (CRIAR)

**Mocks Necessários**: IExportRepository, ICurrentUserService, IValidator<RequestExportRequest>

**Testes a Implementar**:

1. **RequestExport_WithValidRequest_CreatesExportWithPendingStatus**
   - Setup: Validator retorna válido
   - Setup: CurrentUserService retorna userId mockado
   - Setup: Repository.AddAsync retorna export criado
   - Assert: Export criado tem Status = Pending
   - Assert: ParametersJson é serializado corretamente
   - Assert: Repository.AddAsync foi chamado uma vez

2. **RequestExport_WithInvalidFormat_ThrowsValidationException**
   - Request: Format = 3 (inválido)
   - Setup: Validator retorna inválido
   - Assert: Lança `ValidationException`

3. **RequestExport_WithPeriodTooLong_ThrowsValidationException**
   - Request: StartDate e EndDate com diferença > 365 dias
   - Setup: Validator retorna inválido
   - Assert: Lança `ValidationException`

4. **RequestExport_WithStartDateAfterEndDate_ThrowsValidationException**
   - Request: StartDate > EndDate
   - Setup: Validator retorna inválido
   - Assert: Lança `ValidationException`

**Critérios de Aceitação**: 4/4 testes passando ✅

**Estimativa**: 45 minutos

---

### 4.2 GetExportStatusUseCaseTests.cs - 3 Testes

**Arquivo**: `backend/tests/L2SLedger.Application.Tests/UseCases/Exports/GetExportStatusUseCaseTests.cs` (CRIAR)

**Mocks Necessários**: IExportRepository, ICurrentUserService

**Testes a Implementar**:

1. **GetExportStatus_WithValidId_ReturnsStatus**
   - Setup: Repository retorna export existente
   - Setup: CurrentUserService retorna userId = export.RequestedByUserId
   - Assert: Retorna ExportStatusResponse correto
   - Assert: ProgressPercentage calculado corretamente (Pending=0%, Processing=50%, Completed/Failed=100%)

2. **GetExportStatus_WithInvalidId_ThrowsNotFoundException**
   - Setup: Repository retorna null
   - Assert: Lança `NotFoundException`

3. **GetExportStatus_WithAnotherUserId_ThrowsUnauthorizedException**
   - Setup: Repository retorna export de outro usuário
   - Setup: CurrentUserService retorna userId diferente (e não é Admin)
   - Assert: Lança `AuthorizationException`

**Critérios de Aceitação**: 3/3 testes passando ✅

**Estimativa**: 30 minutos

---

### 4.3 DownloadExportUseCaseTests.cs - 4 Testes

**Arquivo**: `backend/tests/L2SLedger.Application.Tests/UseCases/Exports/DownloadExportUseCaseTests.cs` (CRIAR)

**Mocks Necessários**: IExportRepository, IFileStorageService, ICurrentUserService

**Testes a Implementar**:

1. **DownloadExport_WithCompletedExport_ReturnsFileBytes**
   - Setup: Repository retorna export com Status = Completed e FilePath preenchido
   - Setup: FileStorage retorna bytes mockados
   - Assert: Retorna bytes corretos
   - Assert: ContentType correto (text/csv ou application/pdf)
   - Assert: FileName correto

2. **DownloadExport_WithPendingExport_ThrowsBusinessRuleException**
   - Setup: Repository retorna export com Status = Pending
   - Assert: Lança `BusinessRuleException` (exportação não disponível para download)

3. **DownloadExport_WithNonExistentFile_ThrowsFileNotFoundException**
   - Setup: Repository retorna export Completed mas FileStorage lança FileNotFoundException
   - Assert: Lança `FileNotFoundException`

4. **DownloadExport_WithAnotherUserId_ThrowsUnauthorizedException**
   - Setup: Repository retorna export de outro usuário
   - Setup: CurrentUserService retorna userId diferente (e não é Admin)
   - Assert: Lança `AuthorizationException`

**Critérios de Aceitação**: 4/4 testes passando ✅

**Estimativa**: 45 minutos

---

### 4.4 GetExportByIdUseCaseTests.cs - 2 Testes

**Arquivo**: `backend/tests/L2SLedger.Application.Tests/UseCases/Exports/GetExportByIdUseCaseTests.cs` (CRIAR)

**Mocks Necessários**: IExportRepository, ICurrentUserService

**Testes a Implementar**:

1. **GetExportById_WithValidId_ReturnsExportDto**
   - Setup: Repository retorna export existente com Include de RequestedByUser
   - Setup: CurrentUserService retorna userId = export.RequestedByUserId
   - Assert: Retorna ExportDto correto
   - Assert: RequestedByUserName preenchido

2. **GetExportById_WithInvalidId_ThrowsNotFoundException**
   - Setup: Repository retorna null
   - Assert: Lança `NotFoundException`

**Critérios de Aceitação**: 2/2 testes passando ✅

**Estimativa**: 30 minutos

---

### 4.5 GetExportsUseCaseTests.cs - 2 Testes (NOVO)

**Arquivo**: `backend/tests/L2SLedger.Application.Tests/UseCases/Exports/GetExportsUseCaseTests.cs` (CRIAR)

**Mocks Necessários**: IExportRepository, ICurrentUserService

**Testes a Implementar**:

1. **GetExports_WithFilters_ReturnsPaginatedList**
   - Setup: Repository retorna lista de exports filtrada
   - Setup: Repository.CountByFiltersAsync retorna TotalCount correto
   - Request: Filtros Status = Completed, Format = Csv, Page = 1, PageSize = 10
   - Assert: Retorna GetExportsResponse com lista correta
   - Assert: TotalCount, Page, PageSize corretos

2. **GetExports_WithoutFilters_ReturnsAllUserExports**
   - Setup: Repository retorna todos os exports do usuário
   - Request: Sem filtros (apenas paginação padrão)
   - Assert: Retorna todos os exports do usuário (respeitando ownership)

**Critérios de Aceitação**: 2/2 testes passando ✅

**Estimativa**: 30 minutos

---

## 🎯 BLOCO 5: Testes Contract Layer (PRIORIDADE MÉDIA)

### 5.1 ExportContractTests.cs - 7 Testes

**Arquivo**: `backend/tests/L2SLedger.Contract.Tests/Exports/ExportContractTests.cs` (CRIAR)

**Testes a Implementar**:

1. **ExportDto_ShouldHaveRequiredStructure**
   - Validar que ExportDto tem 15 propriedades obrigatórias
   - Validar tipos corretos (Guid, string, DateTime, long?, int?)

2. **ExportDto_ShouldSerializeCorrectly**
   - Serializar ExportDto para JSON
   - Validar que JSON usa camelCase
   - Validar que propriedades opcionais (FilePath, ErrorMessage) podem ser null

3. **RequestExportRequest_ShouldHaveRequiredStructure**
   - Validar que RequestExportRequest tem 5 propriedades
   - Validar tipos corretos (int, DateTime?, Guid?)

4. **ExportStatusResponse_ShouldHaveRequiredStructure**
   - Validar que ExportStatusResponse tem 6 propriedades
   - Validar tipos corretos

5. **ExportStatusResponse_ProgressPercentage_ShouldBeBetween0And100**
   - Criar ExportStatusResponse com ProgressPercentage fora do range
   - Validar que valores válidos são 0-100

6. **GetExportsResponse_ShouldHaveRequiredStructure**
   - Validar que GetExportsResponse tem 4 propriedades
   - Validar tipos corretos (List<ExportDto>, int)

7. **Export_EnumsShouldSerializeAsIntegers**
   - Serializar ExportFormat e ExportStatus
   - Validar que são serializados como integers (1, 2, etc)
   - Validar que deserialização funciona corretamente

**Critérios de Aceitação**: 7/7 testes passando ✅

**Estimativa**: 1 hora

---

## 🎯 BLOCO 6: Refinamentos Opcionais (PRIORIDADE BAIXA)

### 6.1 PdfExportService - Geração Real de PDF

**Arquivo**: `backend/src/L2SLedger.Infrastructure/Services/PdfExportService.cs`

**Status Atual**: Gera HTML formatado (funcional para PoC)

**Requisitos**:
- [ ] Instalar biblioteca QuestPDF, iTextSharp ou PdfSharpCore
- [ ] Implementar geração de PDF real (não HTML)
- [ ] Layout profissional:
  - Logo/cabeçalho
  - Título e período
  - Tabela formatada (Date, Description, Category, Amount, Type)
  - Totais (Total Income, Total Expense, Net Balance)
  - Rodapé (data de geração, número de registros)
- [ ] Formatação de valores monetários (R$ 1.234,56)
- [ ] Formatação de datas (dd/MM/yyyy)
- [ ] Cores e estilos profissionais

**Recomendação**: 
- Manter HTML como fallback funcional
- Implementar PDF real em iteração futura se não for crítico
- HTML funciona para validação de conceito

**Critérios de Aceitação**:
- ✅ PDF gerado é válido e pode ser aberto
- ✅ Layout profissional e legível
- ✅ Valores formatados corretamente
- ✅ Tamanho de arquivo razoável (< 5MB para 1000 transações)

**Estimativa**: 1-2 horas (dependendo da biblioteca)

---

## 📊 Resumo de Pendências

### Por Prioridade

| Prioridade | Componente | Tarefas | Estimativa |
|------------|------------|---------|------------|
| **ALTA** | Use Cases | 2 | 2h |
| **ALTA** | Controller | 2 endpoints | 1h |
| **MÉDIA** | Testes Domain | 8 testes | 1.5h |
| **MÉDIA** | Testes Application | 15 testes | 3h |
| **MÉDIA** | Testes Contract | 7 testes | 1h |
| **BAIXA** | PdfExportService | Geração real PDF | 1-2h |

**Total Estimado**: 8-10 horas (7-8h crítico + 1-2h opcional)

---

### Por Status

| Status | Quantidade | Descrição |
|--------|------------|-----------|
| ❌ **Não Iniciado** | 2 Use Cases | GetExportsUseCase, DeleteExportUseCase |
| ❌ **Não Iniciado** | 2 Endpoints | GET /exports, DELETE /exports/{id} |
| ❌ **Não Iniciado** | 30 Testes | Domain (8) + Application (15) + Contract (7) |
| ⏳ **Parcial** | 1 Service | PdfExportService (gera HTML) |
| ⏳ **Parcial** | Validação | Testes manuais incompletos |

---

## 📋 Ordem de Execução Recomendada

```
🎯 FASE 1: Use Cases + Controller (CRÍTICO) — 3h
├── 1. Implementar GetExportsUseCase (1h)
├── 2. Implementar DeleteExportUseCase (1h)
├── 3. Atualizar ExportsController GET /exports (30min)
├── 4. Atualizar ExportsController DELETE /exports/{id} (30min)
└── 5. Validação Manual com Postman (30min)

🎯 FASE 2: Testes Domain (IMPORTANTE) — 1.5h
└── 6. Implementar ExportTests.cs (8 testes) (1.5h)

🎯 FASE 3: Testes Application (IMPORTANTE) — 3h
├── 7. Implementar RequestExportUseCaseTests (45min)
├── 8. Implementar GetExportStatusUseCaseTests (30min)
├── 9. Implementar DownloadExportUseCaseTests (45min)
├── 10. Implementar GetExportByIdUseCaseTests (30min)
└── 11. Implementar GetExportsUseCaseTests (30min)

🎯 FASE 4: Testes Contract (IMPORTANTE) — 1h
└── 12. Implementar ExportContractTests (7 testes) (1h)

🎯 FASE 5: Validação Final (OBRIGATÓRIO) — 1h
├── 13. Rodar dotnet build (verificar compilação)
├── 14. Rodar dotnet test (verificar ~320 testes passando)
├── 15. Validação manual de todos os 6 endpoints
├── 16. Validar background processing (logs Hosted Service)
└── 17. Atualizar documentação (changelog, STATUS.md)

🎯 FASE 6: Refinamento PDF (OPCIONAL) — 1-2h
└── 18. Implementar PdfExportService com biblioteca real
```

**Tempo Total**: 8-10 horas  
**Tempo Crítico (Fases 1-5)**: 7-8 horas

---

## ✅ Critérios de Conclusão

### Obrigatórios (MUST HAVE)
- [ ] Todos os 6 endpoints implementados e funcionais
- [ ] 6 Use Cases completos (Request, GetStatus, GetById, Download, GetExports, Delete)
- [ ] 30 testes implementados e passando (8 Domain + 15 Application + 7 Contract)
- [ ] Total de testes do projeto: ~320 (290 + 30)
- [ ] Zero regressões em testes existentes
- [ ] Validação manual de todos os endpoints bem-sucedida
- [ ] Documentação atualizada (changelog.md, STATUS.md)
- [ ] Background processing validado (logs do Hosted Service)

### Desejáveis (NICE TO HAVE)
- [ ] PdfExportService com biblioteca PDF real (QuestPDF recomendado)
- [ ] Testes de integração E2E
- [ ] Métricas de performance documentadas
- [ ] Code coverage > 80% para módulo de Exports

---

## 🚨 Riscos e Mitigações

| Risco | Impacto | Probabilidade | Mitigação |
|-------|---------|---------------|-----------|
| Testes com mocks complexos | Médio | Alta | Seguir padrões existentes (Phases 5-7), usar Moq |
| PdfExportService real demorar | Baixo | Média | Manter HTML como fallback, implementar em iteração futura |
| Regressões em testes existentes | Alto | Baixa | Rodar `dotnet test` após cada mudança significativa |
| Validação de ownership complexa | Médio | Média | Reutilizar lógica de FinancialPeriod e Adjustment (Admin vs User) |
| Integração DI falhar | Médio | Baixa | Validar registros de Use Cases em Program.cs |

---

## 📝 Notas para Implementação

1. **Always Run**: `dotnet build` após cada arquivo criado
2. **Always Run**: `dotnet test` após cada bloco de testes
3. **Follow Patterns**: Usar padrões estabelecidos nas Fases 5-7 (FinancialPeriod, Adjustment, Balance)
4. **Mocks**: Usar Moq (já presente no projeto)
5. **Ownership**: Todos os Use Cases devem injetar `ICurrentUserService` para validar userId/role
6. **Admin Role**: Validar que apenas Admin pode executar DeleteExport
7. **Soft Delete**: Verificar se `Export.MarkAsDeleted()` existe ou usar `SetDeleted()`
8. **File Cleanup**: DeleteExport deve remover arquivo físico (não falhar se já deletado)
9. **Paginação**: GetExports deve usar Page (1-based), PageSize (default 20, max 100)
10. **Serialization**: Todos os DTOs devem usar camelCase em JSON

---

## 📚 Referências

- **Fase 8 Original**: `fase-8-exportacao.md`
- **ADRs Relacionados**: ADR-017 (Exportação), ADR-016 (RBAC), ADR-014 (Auditoria), ADR-029 (Soft Delete)
- **Padrões de Teste**: Ver `tests/L2SLedger.Application.Tests/UseCases/FinancialPeriods/` e `Adjustments/`
- **Padrões de Use Case**: Ver `src/L2SLedger.Application/UseCases/FinancialPeriods/` e `Adjustments/`

---

**Data de Criação**: 2026-01-19  
**Autor**: Master Agent (via análise de fase-8-exportacao.md)  
**Próxima Ação**: Executar FASE 1 (Use Cases + Controller) com agentes especializados  
**Aprovação**: ✅ DOCUMENTO DE PENDÊNCIAS APROVADO
