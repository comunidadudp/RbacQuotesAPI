using FluentValidation;
using RbacApi.DTOs;

namespace RbacApi.Infrastructure.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Fullname)
            .NotEmpty().WithMessage("Debe proporcionar el nombre completo")
            .Length(5, 30).WithMessage("Nombre debe contener entre 5 y 30 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Debe proporcionar el correo electrónico")
            .EmailAddress().WithMessage("Correo electrónico tiene un formato inválido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Debe proporcionar la contraseña")
            .Matches(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,16}$").WithMessage(
                "La contraseña debe tener entre 8 y 16 caracteres, incluyendo al menos una mayúscula, " +
                "una minúscula, un número y un carácter especial");

        RuleFor(x => x.PasswordConf)
            .NotEmpty().WithMessage("Debe confirmar la contraseña")
            .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden");
    }
}
