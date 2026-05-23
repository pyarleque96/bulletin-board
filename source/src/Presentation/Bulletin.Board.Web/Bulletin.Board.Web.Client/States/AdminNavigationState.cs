namespace Bulletin.Board.Web.Client.States;

public class AdminNavigationState
{
    public bool IsInitialLoad { get; private set; } = true;
    public bool IsNavigating { get; private set; }

    public event Action? OnChange;

    public void MarkNavigating()
    {
        if (IsNavigating) return;
        IsNavigating = true;
        OnChange?.Invoke();
    }

    public void MarkNavigationDone()
    {
        if (!IsNavigating) return;
        IsNavigating = false;
        OnChange?.Invoke();
    }

    public void MarkInitialLoadDone()
    {
        if (!IsInitialLoad) return;
        IsInitialLoad = false;
        OnChange?.Invoke();
    }
}
