namespace GpsGame.Application.Security;

public interface ICurrentPlayerAccessor
{
    Guid? PlayerId { get; }
}