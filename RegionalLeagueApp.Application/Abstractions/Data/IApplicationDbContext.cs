using RegionalLeagueApp.Domain.Competitions;

namespace RegionalLeagueApp.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    IQueryable<League> Leagues { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
