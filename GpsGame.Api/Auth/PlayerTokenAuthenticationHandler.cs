using System.Security.Claims;
using System.Text.Encodings.Web;
using GpsGame.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GpsGame.Api.Auth;

public sealed class PlayerTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Scheme = "PlayerToken";
    public const string HeaderName = "X-Player-Token";
    private readonly AppDbContext _db;

    public PlayerTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        AppDbContext db) : base(options, logger, encoder, clock)
    {
        _db = db;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var tokenValues))
            return AuthenticateResult.NoResult();

        var token = tokenValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(token))
            return AuthenticateResult.NoResult();

        var player = await _db.Players
            .AsNoTracking()
            .Where(p => p.ApiToken != null && p.ApiToken == token)
            .Select(p => new { p.Id, p.Username })
            .FirstOrDefaultAsync(Context.RequestAborted);

        if (player is null)
            return AuthenticateResult.Fail("Invalid player token.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
            new Claim(ClaimTypes.Name, player.Username),
            new Claim("player_id", player.Id.ToString())
        };
        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);

        return AuthenticateResult.Success(ticket);
    }
}