using FluentValidation;
using MetroSol.API.DTOs.Assessment;

namespace MetroSol.API.Validators.Assessment;

public class AssessmentCreateDtoValidator : AbstractValidator<AssessmentCreateDto>
{
    public AssessmentCreateDtoValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("ItemId is required.");

        RuleFor(x => x.ReferenceStandardId)
            .NotEmpty().WithMessage("ReferenceStandardId is required.");

        RuleFor(x => x.StandardCertificateId)
            .NotEmpty().WithMessage("StandardCertificateId is required.");

        RuleFor(x => x.MethodId)
            .NotEmpty().WithMessage("MethodId is required.");

        RuleFor(x => x.TechnicianId)
            .NotEmpty().WithMessage("TechnicianId is required.");

        RuleFor(x => x.ExpandedUncertainty)
            .GreaterThanOrEqualTo(0).WithMessage("ExpandedUncertainty must be zero or positive.");

        RuleFor(x => x.CoverageFactor)
            .InclusiveBetween(1, 10).WithMessage("CoverageFactor must be between 1 and 10.");

        RuleFor(x => x.ApplicableStandard)
            .MaximumLength(200).WithMessage("ApplicableStandard must not exceed 200 characters.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .Length(2, 10).WithMessage("Language must be a valid locale code (e.g. 'pt', 'en', 'pt-BR').");

        RuleFor(x => x.PerformedAt)
            .NotEmpty().WithMessage("PerformedAt date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1))
            .WithMessage("PerformedAt cannot be a future date.");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.CompletionDate)
            .When(x => x.StartDate.HasValue && x.CompletionDate.HasValue)
            .WithMessage("StartDate must be before or equal to CompletionDate.");
    }
}
