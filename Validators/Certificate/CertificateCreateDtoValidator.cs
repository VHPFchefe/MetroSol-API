using FluentValidation;
using MetroSol.API.DTOs.Certificate;

namespace MetroSol.API.Validators.Certificate;

public class CertificateCreateDtoValidator : AbstractValidator<CertificateCreateDto>
{
    public CertificateCreateDtoValidator()
    {
        RuleFor(x => x.AssessmentId)
            .NotEmpty().WithMessage("AssessmentId is required.");

        RuleFor(x => x.CertificateNumber)
            .NotEmpty().WithMessage("CertificateNumber is required.")
            .MaximumLength(100).WithMessage("CertificateNumber must not exceed 100 characters.");

        RuleFor(x => x.Standard)
            .NotEmpty().WithMessage("Standard is required.")
            .MaximumLength(200).WithMessage("Standard must not exceed 200 characters.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .Length(2, 10).WithMessage("Language must be a valid locale code (e.g. 'pt', 'en', 'pt-BR').");

        RuleFor(x => x.QrCodeUrl)
            .MaximumLength(500).WithMessage("QrCodeUrl must not exceed 500 characters.")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("QrCodeUrl must be a valid absolute URL.");

        RuleFor(x => x.IssuedAt)
            .NotEmpty().WithMessage("IssuedAt date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1))
            .WithMessage("IssuedAt cannot be set to a future date.");
    }
}
