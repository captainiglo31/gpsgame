using FluentValidation;
using GpsGame.Api.DTOs.Players;

namespace GpsGame.Api.Validation;

public class PlayerCreateValidator : AbstractValidator<PlayerCreateDto>
{
    public PlayerCreateValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 32).WithMessage("Username length must be between 3 and 32 characters.");
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");
        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
    }
}