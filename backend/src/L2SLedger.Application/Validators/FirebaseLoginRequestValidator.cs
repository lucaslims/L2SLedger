using FluentValidation;
using L2SLedger.Application.DTOs.Auth;

namespace L2SLedger.Application.Validators;

/// <summary>
/// Validador para FirebaseLoginRequest.
/// </summary>
public class FirebaseLoginRequestValidator : AbstractValidator<FirebaseLoginRequest>
{
    public FirebaseLoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(255).WithMessage("Email não pode exceder 255 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres");
    }
}
