using FluentValidation;

namespace GpsGame.Application.Resources;

public sealed class CollectRequestValidator : AbstractValidator<CollectRequestDto>
{
    public CollectRequestValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.PlayerLatitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.PlayerLongitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.Amount).InclusiveBetween(1, 50);
    }
}