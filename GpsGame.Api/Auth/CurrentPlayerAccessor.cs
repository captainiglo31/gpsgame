using System.Security.Claims;
using GpsGame.Application.Security;

namespace GpsGame.Api.Auth;

public sealed class CurrentPlayerAccessor : ICurrentPlayerAccessor
{
    private readonly IHttpContextAccessor _http;
    public CurrentPlayerAccessor(IHttpContextAccessor http) => _http = http;

    public Guid? PlayerId
    {
        get
        {
            var id = _http.HttpContext?.User.FindFirstValue("player_id")
                     ?? _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var g) ? g : null;
        }
    }
}