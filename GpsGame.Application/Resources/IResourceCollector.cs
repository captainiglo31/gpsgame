using System.Threading;
using System.Threading.Tasks;

namespace GpsGame.Application.Resources;

public interface IResourceCollector
{
    Task<CollectResultDto> CollectAsync(Guid nodeId, CollectRequestDto request, CancellationToken ct = default);
}