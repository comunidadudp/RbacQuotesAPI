using FluentValidation;
using RbacApi.DTOs;

namespace RbacApi.Infrastructure.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("El SKU es obligatorio.")
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100);

        RuleFor(x => x.Slug)
            .MaximumLength(100).WithMessage("El slug puede contener máximo 100 caracteres.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("La categoría es obligatoria.");

        RuleFor(x => x.BasePrice)
            .NotNull().WithMessage("El precio base es obligatorio.")
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Image)
            .NotNull().WithMessage("La imagen principal es obligatoria.");

        RuleForEach(x => x.Images)
            .SetValidator(new ProductImageDTOValidator());

        RuleForEach(x => x.Features)
            .SetValidator(new FeatureDTOValidator());

        RuleForEach(x => x.DefaultConfiguration)
            .SetValidator(new FeatureDTOValidator());

        When(x => x.Dimensions != null, () =>
        {
            RuleFor(x => x.Dimensions)
                .SetValidator(new DimensionDTOValidator());
        });
    }
}

public class ProductImageDTOValidator : AbstractValidator<ProductImageDTO>
{
    public ProductImageDTOValidator()
    {
        RuleFor(x => x.Alt)
            .MaximumLength(100);

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Order.HasValue);

        RuleFor(x => x.File)
            .NotNull().When(x => x.File != null);
    }
}

public class FeatureDTOValidator : AbstractValidator<FeatureDTO>
{
    public FeatureDTOValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("La clave de la característica es obligatoria.");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("La etiqueta de la característica es obligatoria.");

        RuleFor(x => x.PriceImpact)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PriceImpact.HasValue);
    }
}

public class DimensionDTOValidator : AbstractValidator<DimensionsDTO>
{
    public DimensionDTOValidator()
    {
        RuleFor(x => x.Length)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Length.HasValue);

        RuleFor(x => x.Width)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Width.HasValue);

        RuleFor(x => x.Height)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Height.HasValue);

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("La unidad de medida es obligatoria.");
    }
}