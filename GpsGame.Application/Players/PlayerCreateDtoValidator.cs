using FluentValidation;
using GpsGame.Application.Players;

namespace GpsGame.Application.Players
{
    public class PlayerCreateDtoValidator : AbstractValidator<PlayerCreateDto>
    {
        public PlayerCreateDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .WithMessage("Latitude must be between -90 and 90.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage("Longitude must be between -180 and 180.");
        }
    }
}