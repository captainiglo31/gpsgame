using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GpsGame.Application.Resources;

namespace GpsGame.Tests.TestDoubles
{
    public sealed class FakeResourceQuery : IResourceQuery
    {
        public Task<IReadOnlyList<ResourceDto>> GetByBoundingBoxAsync(
            double minLat, double minLng, double maxLat, double maxLng, CancellationToken ct)
            => Task.FromResult((IReadOnlyList<ResourceDto>)new List<ResourceDto>());
    }
}