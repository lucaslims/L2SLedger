---
title: Fase 10 — Usuários e Permissões
date: 2026-01-21
version: 1.0
status: Implementado
dependencies:
  - Fases 1-9 concluídas
  - ADR-016 (RBAC)
  - ADR-001 (Firebase Authentication)
  - ADR-005 (Segurança e Autorização)
estimated_tests: ~25 novos testes
priority: 🟢 Baixa
---

# Fase 10 — Usuários e Permissões

## 1. Contexto e Objetivo

### 1.1 Contexto

O **L2SLedger** já possui a entidade `User` implementada em [backend/src/L2SLedger.Domain/Entities/User.cs](backend/src/L2SLedger.Domain/Entities/User.cs) com suporte a:

- Firebase UID e autenticação via Firebase
- Lista de roles (papéis) com métodos `AddRole()`, `RemoveRole()`, `HasRole()`, `IsAdmin()`
- Métodos de atualização como `UpdateDisplayName()`, `VerifyEmail()`

O repositório `IUserRepository` em [backend/src/L2SLedger.Application/Interfaces/IUserRepository.cs](backend/src/L2SLedger.Application/Interfaces/IUserRepository.cs) oferece métodos básicos:

- `GetByIdAsync()`, `GetByFirebaseUidAsync()`, `GetByEmailAsync()`, `AddAsync()`, `UpdateAsync()`

**O que está faltando:**

1. **Endpoints de administração** para listar usuários e gerenciar roles
2. **Use Cases** dedicados para gestão de usuários (Admin-only)
3. **DTOs específicos** para requests e responses de gestão de usuários
4. **Paginação e filtros** para listagem de usuários
5. **Validadores** para requisições de atualização de roles
6. **Testes unitários e de integração** para os novos endpoints

### 1.2 Objetivo

Implementar endpoints de **administração de usuários** que permitam:

- Listar todos os usuários com paginação e filtros
- Consultar detalhes de um usuário específico
- Consultar roles de um usuário
- Atualizar roles de um usuário (Admin-only)

> ⚠️ **Importante**: Conforme ADR-001, o Firebase é o único IdP. Não criamos ou deletamos usuários pela API — apenas consultamos e atualizamos dados de domínio (roles, displayName).

---

## 2. ADRs Relacionados

| ADR | Título | Impacto na Fase 10 |
|-----|--------|-------------------|
| [ADR-016](../../adr/adr-016.md) | RBAC/ABAC | Define os papéis: Admin, Financeiro, Leitura. Autorização sempre no backend. |
| [ADR-001](../../adr/adr-001.md) | Firebase Auth | Firebase é único IdP. Usuários criados no primeiro login. |
| [ADR-005](../../adr/adr-005.md) | Segurança | Backend é security boundary. 401/403 fail-fast. |
| [ADR-020](../../adr/adr-020.md) | Clean Architecture | Organização em camadas: Domain → Application → Infrastructure → API |
| [ADR-014](../../adr/adr-014.md) | Auditoria | Mudanças em roles devem ser auditadas |

---

## 3. Decisões de Design

### 3.1 Papéis Suportados (ADR-016)

```
Admin       → Controle total do sistema
Financeiro  → Lançamentos, ajustes, conciliação
Leitura     → Apenas visualização (role padrão)
```

### 3.2 Permissões dos Endpoints

| Endpoint | Roles Permitidos |
|----------|------------------|
| `GET /api/v1/users` | Admin |
| `GET /api/v1/users/{id}` | Admin |
| `GET /api/v1/users/{id}/roles` | Admin |
| `PUT /api/v1/users/{id}/roles` | Admin |

### 3.3 Restrições

1. **Não é possível criar usuários pela API** — são criados automaticamente no primeiro login via Firebase
2. **Não é possível deletar usuários** — soft delete já implementado na entidade
3. **Um Admin não pode remover seu próprio papel de Admin** — prevenção de lock-out
4. **Pelo menos um Admin deve existir no sistema** — validação na atualização de roles

---

## 4. Componentes a Implementar

### 4.1 Domain Layer

#### 4.1.1 Value Object — `Role.cs` (NOVO)

**Arquivo:** `backend/src/L2SLedger.Domain/ValueObjects/Role.cs`

**Por que criar:**
- Encapsular validação de roles válidos
- Evitar strings mágicas espalhadas no código
- Facilitar extensão futura de roles

**Implementação:**

```csharp
namespace L2SLedger.Domain.ValueObjects;

/// <summary>
/// Value Object para representar um papel/role do sistema.
/// Conforme ADR-016: Admin, Financeiro, Leitura.
/// </summary>
public sealed record Role
{
    public static readonly Role Admin = new("Admin");
    public static readonly Role Financeiro = new("Financeiro");
    public static readonly Role Leitura = new("Leitura");

    private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        Admin.Value,
        Financeiro.Value,
        Leitura.Value
    };

    public string Value { get; }

    private Role(string value)
    {
        Value = value;
    }

    public static Role FromString(string role)
    {
        if (!IsValid(role))
            throw new ArgumentException($"Role inválido: {role}. Valores permitidos: {string.Join(", ", ValidRoles)}");

        return new Role(role);
    }

    public static bool IsValid(string role) => ValidRoles.Contains(role);

    public static IReadOnlyCollection<string> GetAllRoles() => ValidRoles;

    public override string ToString() => Value;
}
```

---

### 4.2 Application Layer

#### 4.2.1 DTOs — Pasta `Users` (NOVO)

**Diretório:** `backend/src/L2SLedger.Application/DTOs/Users/`

##### 4.2.1.1 `UserDetailDto.cs`

**Por que criar:** Representar dados completos do usuário para resposta de consulta.

```csharp
namespace L2SLedger.Application.DTOs.Users;

/// <summary>
/// Detalhes completos de um usuário para administração.
/// </summary>
public record UserDetailDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public required bool EmailVerified { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}
```

##### 4.2.1.2 `UserSummaryDto.cs`

**Por que criar:** Representação resumida para listagem (evita dados desnecessários).

```csharp
namespace L2SLedger.Application.DTOs.Users;

/// <summary>
/// Resumo do usuário para listagem.
/// </summary>
public record UserSummaryDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
    public required DateTime CreatedAt { get; init; }
}
```

##### 4.2.1.3 `GetUsersRequest.cs`

**Por que criar:** Encapsular parâmetros de filtro e paginação.

```csharp
namespace L2SLedger.Application.DTOs.Users;

/// <summary>
/// Request para listagem de usuários com paginação e filtros.
/// </summary>
public record GetUsersRequest
{
    /// <summary>Número da página (1-indexed)</summary>
    public int Page { get; init; } = 1;

    /// <summary>Quantidade de itens por página (max 100)</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>Filtrar por email (contém)</summary>
    public string? Email { get; init; }

    /// <summary>Filtrar por role específico</summary>
    public string? Role { get; init; }

    /// <summary>Incluir usuários inativos/deletados</summary>
    public bool IncludeInactive { get; init; } = false;
}
```

##### 4.2.1.4 `GetUsersResponse.cs`

**Por que criar:** Response padronizado com paginação.

```csharp
namespace L2SLedger.Application.DTOs.Users;

/// <summary>
/// Response paginado para listagem de usuários.
/// </summary>
public record GetUsersResponse
{
    public required IReadOnlyList<UserSummaryDto> Users { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
}
```

##### 4.2.1.5 `UpdateUserRolesRequest.cs`

**Por que criar:** Encapsular dados para atualização de roles.

```csharp
namespace L2SLedger.Application.DTOs.Users;

/// <summary>
/// Request para atualização de roles do usuário.
/// </summary>
public record UpdateUserRolesRequest
{
    /// <summary>Lista de roles a atribuir ao usuário</summary>
    public required IReadOnlyList<string> Roles { get; init; }
}
```

##### 4.2.1.6 `UserRolesResponse.cs`

**Por que criar:** Response para consulta de roles.

```csharp
namespace L2SLedger.Application.DTOs.Users;

/// <summary>
/// Response com roles do usuário.
/// </summary>
public record UserRolesResponse
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
    public required IReadOnlyList<string> AvailableRoles { get; init; }
}
```

---

#### 4.2.2 Interface do Repositório — Atualização

**Arquivo:** `backend/src/L2SLedger.Application/Interfaces/IUserRepository.cs`

**Alterações necessárias:**

```csharp
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Repositório para operações com usuários.
/// </summary>
public interface IUserRepository
{
    // === Métodos existentes ===
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    // === NOVOS MÉTODOS (Fase 10) ===
    
    /// <summary>
    /// Lista usuários com paginação e filtros opcionais.
    /// </summary>
    Task<(IReadOnlyList<User> Users, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? emailFilter = null,
        string? roleFilter = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe pelo menos um Admin além do usuário especificado.
    /// </summary>
    Task<bool> ExistsOtherAdminAsync(Guid excludeUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta total de usuários com uma role específica.
    /// </summary>
    Task<int> CountByRoleAsync(string role, CancellationToken cancellationToken = default);
}
```

**Por que alterar:**
- Suportar listagem paginada para o endpoint `GET /api/v1/users`
- Validar regra de negócio: "pelo menos um Admin deve existir"

---

#### 4.2.3 Use Cases — Pasta `Users` (NOVO)

**Diretório:** `backend/src/L2SLedger.Application/UseCases/Users/`

##### 4.2.3.1 `GetUsersUseCase.cs`

**Por que criar:** Listar usuários com paginação e filtros.

```csharp
using AutoMapper;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Application.Interfaces;

namespace L2SLedger.Application.UseCases.Users;

/// <summary>
/// Caso de uso para listar usuários com paginação e filtros.
/// Apenas Admin pode executar (ADR-016).
/// </summary>
public class GetUsersUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUsersUseCase(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<GetUsersResponse> ExecuteAsync(
        GetUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validar limites de paginação
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (users, totalCount) = await _userRepository.GetAllAsync(
            page,
            pageSize,
            request.Email,
            request.Role,
            request.IncludeInactive,
            cancellationToken);

        var userDtos = _mapper.Map<IReadOnlyList<UserSummaryDto>>(users);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new GetUsersResponse
        {
            Users = userDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}
```

##### 4.2.3.2 `GetUserByIdUseCase.cs`

**Por que criar:** Consultar detalhes de um usuário específico.

```csharp
using AutoMapper;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Application.UseCases.Users;

/// <summary>
/// Caso de uso para obter detalhes de um usuário por ID.
/// Apenas Admin pode executar (ADR-016).
/// </summary>
public class GetUserByIdUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUserByIdUseCase(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDetailDto> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new BusinessRuleException(
                "USER_NOT_FOUND",
                $"Usuário com ID {userId} não encontrado.");
        }

        return _mapper.Map<UserDetailDto>(user);
    }
}
```

##### 4.2.3.3 `GetUserRolesUseCase.cs`

**Por que criar:** Consultar roles de um usuário com lista de roles disponíveis.

```csharp
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Exceptions;
using L2SLedger.Domain.ValueObjects;

namespace L2SLedger.Application.UseCases.Users;

/// <summary>
/// Caso de uso para obter roles de um usuário.
/// Apenas Admin pode executar (ADR-016).
/// </summary>
public class GetUserRolesUseCase
{
    private readonly IUserRepository _userRepository;

    public GetUserRolesUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserRolesResponse> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new BusinessRuleException(
                "USER_NOT_FOUND",
                $"Usuário com ID {userId} não encontrado.");
        }

        return new UserRolesResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = user.Roles.ToList(),
            AvailableRoles = Role.GetAllRoles().ToList()
        };
    }
}
```

##### 4.2.3.4 `UpdateUserRolesUseCase.cs`

**Por que criar:** Atualizar roles de um usuário com validações de segurança.

```csharp
using AutoMapper;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Exceptions;
using L2SLedger.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Users;

/// <summary>
/// Caso de uso para atualizar roles de um usuário.
/// Apenas Admin pode executar (ADR-016).
/// Registra auditoria de alterações (ADR-014).
/// </summary>
public class UpdateUserRolesUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateUserRolesUseCase> _logger;

    public UpdateUserRolesUseCase(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<UpdateUserRolesUseCase> logger)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDetailDto> ExecuteAsync(
        Guid userId,
        UpdateUserRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar roles fornecidos
        ValidateRoles(request.Roles);

        // 2. Buscar usuário
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new BusinessRuleException(
                "USER_NOT_FOUND",
                $"Usuário com ID {userId} não encontrado.");

        // 3. Obter usuário atual (Admin executando a ação)
        var currentUserId = _currentUserService.GetCurrentUserId();

        // 4. Validar regras de negócio
        await ValidateBusinessRulesAsync(user, request.Roles, currentUserId, cancellationToken);

        // 5. Atualizar roles
        var oldRoles = user.Roles.ToList();
        UpdateRoles(user, request.Roles);

        // 6. Persistir alterações
        await _userRepository.UpdateAsync(user, cancellationToken);

        // 7. Log de auditoria
        _logger.LogInformation(
            "Roles do usuário {UserId} atualizados de [{OldRoles}] para [{NewRoles}] por Admin {AdminId}",
            userId,
            string.Join(", ", oldRoles),
            string.Join(", ", request.Roles),
            currentUserId);

        return _mapper.Map<UserDetailDto>(user);
    }

    private void ValidateRoles(IReadOnlyList<string> roles)
    {
        if (roles == null || roles.Count == 0)
        {
            throw new ValidationException(
                "ROLES_REQUIRED",
                "Pelo menos uma role deve ser especificada.");
        }

        foreach (var role in roles)
        {
            if (!Role.IsValid(role))
            {
                throw new ValidationException(
                    "INVALID_ROLE",
                    $"Role inválido: {role}. Valores permitidos: {string.Join(", ", Role.GetAllRoles())}");
            }
        }
    }

    private async Task ValidateBusinessRulesAsync(
        Domain.Entities.User user,
        IReadOnlyList<string> newRoles,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var isRemovingAdminFromSelf = user.Id == currentUserId 
            && user.IsAdmin() 
            && !newRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase);

        if (isRemovingAdminFromSelf)
        {
            throw new BusinessRuleException(
                "CANNOT_REMOVE_OWN_ADMIN",
                "Você não pode remover seu próprio papel de Admin.");
        }

        // Verificar se está removendo Admin e se há outro Admin
        if (user.IsAdmin() && !newRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
        {
            var existsOtherAdmin = await _userRepository.ExistsOtherAdminAsync(user.Id, cancellationToken);
            
            if (!existsOtherAdmin)
            {
                throw new BusinessRuleException(
                    "LAST_ADMIN",
                    "Não é possível remover o papel de Admin do último administrador do sistema.");
            }
        }
    }

    private void UpdateRoles(Domain.Entities.User user, IReadOnlyList<string> newRoles)
    {
        // Remover roles atuais que não estão na nova lista
        var rolesToRemove = user.Roles.Where(r => !newRoles.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();
        foreach (var role in rolesToRemove)
        {
            user.RemoveRole(role);
        }

        // Adicionar novas roles que não existem
        foreach (var role in newRoles)
        {
            if (!user.HasRole(role))
            {
                user.AddRole(role);
            }
        }
    }
}
```

---

#### 4.2.4 Validators — Pasta `Users` (NOVO)

**Diretório:** `backend/src/L2SLedger.Application/Validators/Users/`

##### 4.2.4.1 `UpdateUserRolesRequestValidator.cs`

**Por que criar:** Validação de entrada antes de processar a requisição.

```csharp
using FluentValidation;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Domain.ValueObjects;

namespace L2SLedger.Application.Validators.Users;

/// <summary>
/// Validador para UpdateUserRolesRequest.
/// </summary>
public class UpdateUserRolesRequestValidator : AbstractValidator<UpdateUserRolesRequest>
{
    public UpdateUserRolesRequestValidator()
    {
        RuleFor(x => x.Roles)
            .NotNull()
            .WithMessage("A lista de roles é obrigatória.")
            .NotEmpty()
            .WithMessage("Pelo menos uma role deve ser especificada.");

        RuleForEach(x => x.Roles)
            .NotEmpty()
            .WithMessage("Role não pode ser vazio.")
            .Must(Role.IsValid)
            .WithMessage(role => $"Role inválido: '{role}'. Valores permitidos: {string.Join(", ", Role.GetAllRoles())}");
    }
}
```

##### 4.2.4.2 `GetUsersRequestValidator.cs`

**Por que criar:** Validação de parâmetros de paginação e filtros.

```csharp
using FluentValidation;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Domain.ValueObjects;

namespace L2SLedger.Application.Validators.Users;

/// <summary>
/// Validador para GetUsersRequest.
/// </summary>
public class GetUsersRequestValidator : AbstractValidator<GetUsersRequest>
{
    public GetUsersRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Página deve ser maior ou igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Tamanho da página deve estar entre 1 e 100.");

        RuleFor(x => x.Role)
            .Must(role => role == null || Role.IsValid(role))
            .WithMessage(x => $"Role inválido: '{x.Role}'. Valores permitidos: {string.Join(", ", Role.GetAllRoles())}");
    }
}
```

---

#### 4.2.5 Mapper — `UserMappingProfile.cs` (NOVO)

**Arquivo:** `backend/src/L2SLedger.Application/Mappers/UserMappingProfile.cs`

**Por que criar:** Mapeamento AutoMapper entre entidade User e DTOs.

```csharp
using AutoMapper;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Mappers;

/// <summary>
/// Profile de mapeamento para User.
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserSummaryDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.ToList()));

        CreateMap<User, UserDetailDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.ToList()))
            .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore()); // TODO: Adicionar campo na entidade se necessário
    }
}
```

---

### 4.3 Infrastructure Layer

#### 4.3.1 Repositório — Atualização do `UserRepository.cs`

**Arquivo:** `backend/src/L2SLedger.Infrastructure/Persistence/Repositories/UserRepository.cs`

**Alterações necessárias:** Adicionar os novos métodos da interface.

```csharp
// === ADICIONAR ao final da classe UserRepository ===

public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetAllAsync(
    int page,
    int pageSize,
    string? emailFilter = null,
    string? roleFilter = null,
    bool includeInactive = false,
    CancellationToken cancellationToken = default)
{
    var query = _context.Users.AsQueryable();

    // Filtro de inativos (soft delete)
    if (!includeInactive)
    {
        query = query.Where(u => !u.IsDeleted);
    }

    // Filtro por email (contains, case-insensitive)
    if (!string.IsNullOrWhiteSpace(emailFilter))
    {
        query = query.Where(u => u.Email.ToLower().Contains(emailFilter.ToLower()));
    }

    // Filtro por role
    if (!string.IsNullOrWhiteSpace(roleFilter))
    {
        // EF Core suporta Contains em coleções JSON
        query = query.Where(u => u.Roles.Contains(roleFilter));
    }

    // Contar total antes de paginar
    var totalCount = await query.CountAsync(cancellationToken);

    // Aplicar paginação e ordenação
    var users = await query
        .OrderBy(u => u.Email)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    return (users, totalCount);
}

public async Task<bool> ExistsOtherAdminAsync(Guid excludeUserId, CancellationToken cancellationToken = default)
{
    return await _context.Users
        .Where(u => u.Id != excludeUserId && !u.IsDeleted)
        .AnyAsync(u => u.Roles.Contains("Admin"), cancellationToken);
}

public async Task<int> CountByRoleAsync(string role, CancellationToken cancellationToken = default)
{
    return await _context.Users
        .Where(u => !u.IsDeleted)
        .CountAsync(u => u.Roles.Contains(role), cancellationToken);
}
```

---

### 4.4 API Layer

#### 4.4.1 Controller — `UsersController.cs` (NOVO)

**Arquivo:** `backend/src/L2SLedger.API/Controllers/UsersController.cs`

**Por que criar:** Expor endpoints REST para gestão de usuários.

```csharp
using L2SLedger.API.Contracts;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Application.UseCases.Users;
using L2SLedger.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L2SLedger.API.Controllers;

/// <summary>
/// Endpoints para administração de usuários.
/// Conforme ADR-016: Apenas Admin pode gerenciar usuários.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly GetUsersUseCase _getUsersUseCase;
    private readonly GetUserByIdUseCase _getUserByIdUseCase;
    private readonly GetUserRolesUseCase _getUserRolesUseCase;
    private readonly UpdateUserRolesUseCase _updateUserRolesUseCase;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        GetUsersUseCase getUsersUseCase,
        GetUserByIdUseCase getUserByIdUseCase,
        GetUserRolesUseCase getUserRolesUseCase,
        UpdateUserRolesUseCase updateUserRolesUseCase,
        ILogger<UsersController> logger)
    {
        _getUsersUseCase = getUsersUseCase;
        _getUserByIdUseCase = getUserByIdUseCase;
        _getUserRolesUseCase = getUserRolesUseCase;
        _updateUserRolesUseCase = updateUserRolesUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os usuários com paginação e filtros.
    /// </summary>
    /// <param name="page">Número da página (1-indexed)</param>
    /// <param name="pageSize">Quantidade de itens por página (max 100)</param>
    /// <param name="email">Filtrar por email (contém)</param>
    /// <param name="role">Filtrar por role</param>
    /// <param name="includeInactive">Incluir usuários inativos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de usuários</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetUsersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetUsersResponse>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? email = null,
        [FromQuery] string? role = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var request = new GetUsersRequest
        {
            Page = page,
            PageSize = pageSize,
            Email = email,
            Role = role,
            IncludeInactive = includeInactive
        };

        var response = await _getUsersUseCase.ExecuteAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Obtém detalhes de um usuário por ID.
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Detalhes do usuário</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDetailDto>> GetUserById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _getUserByIdUseCase.ExecuteAsync(id, cancellationToken);
            return Ok(user);
        }
        catch (BusinessRuleException ex) when (ex.Code == "USER_NOT_FOUND")
        {
            return NotFound(ErrorResponse.Create(ex.Code, ex.Message));
        }
    }

    /// <summary>
    /// Obtém roles de um usuário.
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Roles do usuário e roles disponíveis</returns>
    [HttpGet("{id:guid}/roles")]
    [ProducesResponseType(typeof(UserRolesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserRolesResponse>> GetUserRoles(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _getUserRolesUseCase.ExecuteAsync(id, cancellationToken);
            return Ok(response);
        }
        catch (BusinessRuleException ex) when (ex.Code == "USER_NOT_FOUND")
        {
            return NotFound(ErrorResponse.Create(ex.Code, ex.Message));
        }
    }

    /// <summary>
    /// Atualiza roles de um usuário.
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="request">Novas roles</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Usuário atualizado</returns>
    [HttpPut("{id:guid}/roles")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDetailDto>> UpdateUserRoles(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _updateUserRolesUseCase.ExecuteAsync(id, request, cancellationToken);
            
            _logger.LogInformation("Roles do usuário {UserId} atualizados com sucesso", id);
            
            return Ok(user);
        }
        catch (BusinessRuleException ex) when (ex.Code == "USER_NOT_FOUND")
        {
            return NotFound(ErrorResponse.Create(ex.Code, ex.Message));
        }
        catch (BusinessRuleException ex) when (ex.Code is "CANNOT_REMOVE_OWN_ADMIN" or "LAST_ADMIN")
        {
            return BadRequest(ErrorResponse.Create(ex.Code, ex.Message));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ErrorResponse.Create(ex.Code, ex.Message));
        }
    }
}
```

---

#### 4.4.2 Configuração de DI — Atualização

**Arquivo:** `backend/src/L2SLedger.API/Configuration/DependencyInjectionExtensions.cs`

**Alterações necessárias:** Adicionar método para registrar Use Cases de usuários.

```csharp
// === ADICIONAR novo método ===

/// <summary>
/// Registra use cases de usuários.
/// ADR-016: Gestão de roles e permissões.
/// </summary>
public static IServiceCollection AddUserUseCases(this IServiceCollection services)
{
    services.AddScoped<GetUsersUseCase>();
    services.AddScoped<GetUserByIdUseCase>();
    services.AddScoped<GetUserRolesUseCase>();
    services.AddScoped<UpdateUserRolesUseCase>();

    return services;
}
```

**Arquivo:** `backend/src/L2SLedger.API/Program.cs`

**Alteração necessária:** Adicionar chamada ao novo método de DI.

```csharp
// === Adicionar após AddExportUseCases() ===
builder.Services.AddUserUseCases();
```

---

## 5. Testes a Implementar

### 5.1 Testes Unitários

**Diretório:** `backend/tests/L2SLedger.Application.Tests/UseCases/Users/`

| Teste | Arquivo | Descrição |
|-------|---------|-----------|
| GetUsersUseCase_ReturnsEmptyList_WhenNoUsers | `GetUsersUseCaseTests.cs` | Lista vazia retorna corretamente |
| GetUsersUseCase_ReturnsPaginatedList | `GetUsersUseCaseTests.cs` | Paginação funciona corretamente |
| GetUsersUseCase_FiltersByEmail | `GetUsersUseCaseTests.cs` | Filtro por email funciona |
| GetUsersUseCase_FiltersByRole | `GetUsersUseCaseTests.cs` | Filtro por role funciona |
| GetUserByIdUseCase_ReturnsUser_WhenExists | `GetUserByIdUseCaseTests.cs` | Retorna usuário existente |
| GetUserByIdUseCase_ThrowsNotFound_WhenNotExists | `GetUserByIdUseCaseTests.cs` | Lança exceção para usuário inexistente |
| GetUserRolesUseCase_ReturnsRolesAndAvailable | `GetUserRolesUseCaseTests.cs` | Retorna roles e lista de disponíveis |
| UpdateUserRolesUseCase_UpdatesRoles | `UpdateUserRolesUseCaseTests.cs` | Atualiza roles com sucesso |
| UpdateUserRolesUseCase_ThrowsWhenInvalidRole | `UpdateUserRolesUseCaseTests.cs` | Rejeita role inválido |
| UpdateUserRolesUseCase_ThrowsWhenRemovingOwnAdmin | `UpdateUserRolesUseCaseTests.cs` | Impede remover próprio Admin |
| UpdateUserRolesUseCase_ThrowsWhenLastAdmin | `UpdateUserRolesUseCaseTests.cs` | Impede remover último Admin |

### 5.2 Testes de Integração

**Diretório:** `backend/tests/L2SLedger.API.Tests/Controllers/`

| Teste | Arquivo | Descrição |
|-------|---------|-----------|
| GetUsers_Returns200_WhenAdmin | `UsersControllerTests.cs` | Admin pode listar usuários |
| GetUsers_Returns403_WhenNotAdmin | `UsersControllerTests.cs` | Não-Admin recebe 403 |
| GetUserById_Returns200_WhenExists | `UsersControllerTests.cs` | Retorna usuário existente |
| GetUserById_Returns404_WhenNotExists | `UsersControllerTests.cs` | Retorna 404 para inexistente |
| GetUserRoles_Returns200_WithAvailableRoles | `UsersControllerTests.cs` | Retorna roles e disponíveis |
| UpdateUserRoles_Returns200_WhenValid | `UsersControllerTests.cs` | Atualiza roles com sucesso |
| UpdateUserRoles_Returns400_WhenInvalidRole | `UsersControllerTests.cs` | Rejeita role inválido |
| UpdateUserRoles_Returns400_WhenRemovingSelfAdmin | `UsersControllerTests.cs` | Impede remover próprio Admin |
| UpdateUserRoles_Returns400_WhenLastAdmin | `UsersControllerTests.cs` | Impede remover último Admin |

### 5.3 Testes do Value Object

**Diretório:** `backend/tests/L2SLedger.Domain.Tests/ValueObjects/`

| Teste | Arquivo | Descrição |
|-------|---------|-----------|
| Role_IsValid_ReturnsTrue_ForValidRoles | `RoleTests.cs` | Valida roles conhecidos |
| Role_IsValid_ReturnsFalse_ForInvalidRoles | `RoleTests.cs` | Rejeita roles inválidos |
| Role_FromString_ThrowsForInvalid | `RoleTests.cs` | Lança exceção para inválido |
| Role_GetAllRoles_ReturnsAllThreeRoles | `RoleTests.cs` | Retorna Admin, Financeiro, Leitura |

---

## 6. Resumo de Arquivos

### 6.1 Arquivos a Criar

| Camada | Arquivo | Tipo |
|--------|---------|------|
| Domain | `ValueObjects/Role.cs` | Value Object |
| Application | `DTOs/Users/UserDetailDto.cs` | DTO |
| Application | `DTOs/Users/UserSummaryDto.cs` | DTO |
| Application | `DTOs/Users/GetUsersRequest.cs` | DTO |
| Application | `DTOs/Users/GetUsersResponse.cs` | DTO |
| Application | `DTOs/Users/UpdateUserRolesRequest.cs` | DTO |
| Application | `DTOs/Users/UserRolesResponse.cs` | DTO |
| Application | `UseCases/Users/GetUsersUseCase.cs` | Use Case |
| Application | `UseCases/Users/GetUserByIdUseCase.cs` | Use Case |
| Application | `UseCases/Users/GetUserRolesUseCase.cs` | Use Case |
| Application | `UseCases/Users/UpdateUserRolesUseCase.cs` | Use Case |
| Application | `Validators/Users/UpdateUserRolesRequestValidator.cs` | Validator |
| Application | `Validators/Users/GetUsersRequestValidator.cs` | Validator |
| Application | `Mappers/UserMappingProfile.cs` | Mapper |
| API | `Controllers/UsersController.cs` | Controller |
| Tests | `UseCases/Users/GetUsersUseCaseTests.cs` | Teste |
| Tests | `UseCases/Users/GetUserByIdUseCaseTests.cs` | Teste |
| Tests | `UseCases/Users/GetUserRolesUseCaseTests.cs` | Teste |
| Tests | `UseCases/Users/UpdateUserRolesUseCaseTests.cs` | Teste |
| Tests | `Controllers/UsersControllerTests.cs` | Teste |
| Tests | `ValueObjects/RoleTests.cs` | Teste |

### 6.2 Arquivos a Alterar

| Camada | Arquivo | Alteração |
|--------|---------|-----------|
| Application | `Interfaces/IUserRepository.cs` | Adicionar 3 novos métodos |
| Infrastructure | `Persistence/Repositories/UserRepository.cs` | Implementar 3 novos métodos |
| API | `Configuration/DependencyInjectionExtensions.cs` | Adicionar `AddUserUseCases()` |
| API | `Program.cs` | Chamar `AddUserUseCases()` |

---

## 7. Ordem de Implementação Recomendada

```
1. Domain: Role Value Object (com testes)
2. Application: DTOs (todos os 6 arquivos)
3. Application: Interface IUserRepository (alteração)
4. Infrastructure: UserRepository (implementação dos novos métodos)
5. Application: Validators (com testes)
6. Application: Mapper UserMappingProfile
7. Application: Use Cases (4 arquivos, com testes)
8. API: UsersController
9. API: DI Configuration
10. Integration Tests
```

---

## 8. Critérios de Aceitação

- [ ] Endpoint `GET /api/v1/users` retorna lista paginada (Admin-only)
- [ ] Endpoint `GET /api/v1/users/{id}` retorna detalhes do usuário (Admin-only)
- [ ] Endpoint `GET /api/v1/users/{id}/roles` retorna roles e lista de disponíveis (Admin-only)
- [ ] Endpoint `PUT /api/v1/users/{id}/roles` atualiza roles (Admin-only)
- [ ] Usuários não-Admin recebem 403 Forbidden
- [ ] Admin não pode remover seu próprio papel de Admin
- [ ] Sistema impede remoção do último Admin
- [ ] Roles inválidos são rejeitados com erro claro
- [ ] Paginação funciona corretamente
- [ ] Filtros por email e role funcionam
- [ ] Todos os ~25 testes passando
- [ ] Sem regressões nos 211 testes existentes

---

## 9. Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Consulta de roles em JSON lenta | Baixa | Médio | Índice GIN no PostgreSQL se necessário |
| Admin lock-out | Baixa | Alto | Validações robustas implementadas |
| Conflito com Auth existente | Baixa | Médio | Reutilizar DTOs de Auth quando possível |

---

## 10. Agentes Responsáveis

| Agente | Responsabilidade |
|--------|------------------|
| Backend Agent | Implementação completa (Domain, Application, Infrastructure, API) |
| QA Agent | Testes unitários e de integração |

---

## 11. Próxima Ação

Aguardar aprovação deste planejamento para iniciar implementação via Backend Agent.

---

## Referências

- [ADR-016 — RBAC/ABAC](../../adr/adr-016.md)
- [ADR-001 — Firebase Auth](../../adr/adr-001.md)
- [ADR-005 — Segurança](../../adr/adr-005.md)
- [ASP.NET Core Authorization Roles](https://learn.microsoft.com/aspnet/core/security/authorization/roles)
- [Clean Architecture — Use Cases](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
