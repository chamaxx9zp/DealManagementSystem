using FluentValidation;
using DealManagementSystem.Entities;

namespace DealManagementSystem.Validators
{
    public class HotelValidator : AbstractValidator<Hotel>
    {
        public HotelValidator()
        {
            RuleFor(h => h.Name)
                .NotEmpty();

            RuleFor(h => h.Rate)
                .InclusiveBetween(1.0m, 5.0m)
                .WithMessage("Rate must be between 1.0 and 5.0.");
        }
    }
}
