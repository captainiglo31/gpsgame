using System;
using System.Threading;
using System.Threading.Tasks;
using GpsGame.Application.Resources;

namespace GpsGame.Tests.TestDoubles
{
    public sealed class FakeCollector : IResourceCollector
    {
        public Guid PlayerId { get; set; }
        public string ResourceType { get; set; } = "iron";
        public int Collected { get; set; } = 5;
        public Task<CollectResultDto> CollectAsync(Guid id, CollectRequestDto request, CancellationToken ct)
        {
            return Task.FromResult(new CollectResultDto
            {
                Success = true,
                PlayerId = PlayerId,
                ResourceType = ResourceType,
                Collected = Collected,
                Remaining = 40,
                RespawnAtUtc = null
            });
        }
    }
}