using System;
using System.Threading;
using System.Threading.Tasks;
using GpsGame.Application.Resources;

namespace GpsGame.Tests.TestDoubles
{
    /// <summary>Collector, der immer fehlschlägt (Reason konfigurierbar).</summary>
    public sealed class AlwaysFailCollector : IResourceCollector
    {
        public string Reason { get; set; } = "respawning";

        public Task<CollectResultDto> CollectAsync(Guid id, CollectRequestDto request, CancellationToken ct)
            => Task.FromResult(new CollectResultDto { Success = false, Reason = Reason });
    }
}