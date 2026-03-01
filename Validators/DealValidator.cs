using FluentValidation;
using DealManagementSystem.Entities;

public class DealValidator : AbstractValidator<Deal>
{
    public DealValidator()
    {
        RuleFor(d => d.Name)
            .NotEmpty();

        RuleFor(d => d.Slug)
            .NotEmpty();

        RuleFor(d => d.Hotels)
            .Must(h => h != null && h.Count > 0)
            .WithMessage("At least one hotel is required.");
    }
}