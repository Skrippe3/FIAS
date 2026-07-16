using Fias.Application.DTO;

namespace Fias.Application.Interfaces;

public interface IFiasUpdatePlannerService
{
    Task<FiasUpdatePlanDto> GetUpdatePlanAsync(
        CancellationToken cancellationToken = default);

    Task<int> QueuePendingUpdatesAsync(
        CancellationToken cancellationToken = default);
}
