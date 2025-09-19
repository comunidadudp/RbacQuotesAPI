using FluentValidation;
using RbacApi.DTOs;

namespace RbacApi.Infrastructure.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Ingrese el nombre de usuario");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Ingrese la contraseña");
    }
}
