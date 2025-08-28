using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GpsGame.Application.Resources;

namespace GpsGame.Tests.TestDoubles
{
    /// <summary>
    /// IResourceQuery-Testdouble, das die zuletzt übergebenen BBox-Parameter speichert.
    /// </summary>
    public sealed class RecordingResourceQuery : IResourceQuery
    {
        public double? LastMinLat { get; private set; }
        public double? LastMinLng { get; private set; }
        public double? LastMaxLat { get; private set; }
        public double? LastMaxLng { get; private set; }

        public Task<IReadOnlyList<ResourceDto>> GetByBoundingBoxAsync(
            double minLat, double minLng, double maxLat, double maxLng, CancellationToken ct)
        {
            LastMinLat = minLat;
            LastMinLng = minLng;
            LastMaxLat = maxLat;
            LastMaxLng = maxLng;
            // Wir geben absichtlich eine leere Liste zurück – für den Endpoint-Test reicht das
            return Task.FromResult((IReadOnlyList<ResourceDto>)new List<ResourceDto>());
        }
    }
}