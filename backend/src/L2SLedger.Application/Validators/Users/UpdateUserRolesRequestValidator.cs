using FluentValidation;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Domain.ValueObjects;

namespace L2SLedger.Application.Validators.Users;

/// <summary>
/// Validador para UpdateUserRolesRequest.
/// Conforme ADR-016: Validação de roles válidos.
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
