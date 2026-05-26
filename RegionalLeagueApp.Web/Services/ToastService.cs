namespace RegionalLeagueApp.Web.Services;

public sealed class ToastService
{
    public event Action<ToastMessage>? OnShow;

    public void Success(string message, string? title = null) => Show(ToastType.Success, message, title);

    public void Warning(string message, string? title = null) => Show(ToastType.Warning, message, title);

    public void Error(string message, string? title = null) => Show(ToastType.Error, message, title);

    public void Info(string message, string? title = null) => Show(ToastType.Info, message, title);

    private void Show(ToastType type, string message, string? title)
    {
        OnShow?.Invoke(new ToastMessage(Guid.NewGuid(), type, title, message));
    }
}

public sealed record ToastMessage(Guid Id, ToastType Type, string? Title, string Message);

public enum ToastType
{
    Success,
    Warning,
    Error,
    Info
}
