using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using RegionalLeagueApp.Web.Hubs;

namespace RegionalLeagueApp.Web.Services;

public sealed class MatchUpdatesClient : IAsyncDisposable
{
    private readonly NavigationManager navigationManager;
    private readonly ILogger<MatchUpdatesClient> logger;
    private readonly List<Func<Task>> handlers = [];
    private HubConnection? hubConnection;

    public MatchUpdatesClient(NavigationManager navigationManager, ILogger<MatchUpdatesClient> logger)
    {
        this.navigationManager = navigationManager;
        this.logger = logger;
    }

    public async Task SubscribeAsync(Func<Task> handler)
    {
        handlers.Add(handler);
        await EnsureConnectionAsync();
    }

    public void Unsubscribe(Func<Task> handler)
    {
        handlers.Remove(handler);
    }

    private async Task EnsureConnectionAsync()
    {
        if (hubConnection is null)
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri(MatchUpdatesHub.Route))
                .WithAutomaticReconnect()
                .Build();

            hubConnection.On(MatchUpdatesHub.MatchDataChanged, NotifyHandlersAsync);
        }

        if (hubConnection.State == HubConnectionState.Disconnected)
        {
            await hubConnection.StartAsync();
        }
    }

    private async Task NotifyHandlersAsync()
    {
        foreach (var handler in handlers.ToArray())
        {
            try
            {
                await handler();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo refrescar un componente despues de {EventName}.", MatchUpdatesHub.MatchDataChanged);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }
}
