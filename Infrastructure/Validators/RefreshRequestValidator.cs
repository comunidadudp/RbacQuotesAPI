
using FluentValidation;
using RbacApi.DTOs;

namespace RbacApi.Infrastructure.Validators;

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Debe proporcionar el token");
    }
}
