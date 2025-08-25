using System.Collections.Concurrent;
using GpsGame.Application.Resources;

namespace GpsGame.Infrastructure.Services;

public sealed class ResourceRules : IResourceRules
{
    private static readonly ConcurrentDictionary<string, int> RespawnByType =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Wood"] = 5,
            ["Iron"] = 10,
            ["Stone"] = 7
        };

    private const int DefaultRespawnMinutes = 10;

    public int GetRespawnMinutes(string resourceType)
    {
        if (string.IsNullOrWhiteSpace(resourceType))
            return DefaultRespawnMinutes;

        return RespawnByType.TryGetValue(resourceType, out var minutes)
            ? minutes
            : DefaultRespawnMinutes;
    }
}