using FluentValidation;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Domain.ValueObjects;

namespace L2SLedger.Application.Validators.Users;

/// <summary>
/// Validador para GetUsersRequest.
/// Valida parâmetros de paginação e filtros.
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
            .Must(role => role is null || Role.IsValid(role))
            .WithMessage(x => $"Role inválido: '{x.Role}'. Valores permitidos: {string.Join(", ", Role.GetAllRoles())}");

        RuleFor(x => x.Email)
            .MaximumLength(256)
            .WithMessage("Filtro de email não pode exceder 256 caracteres.");
    }
}
