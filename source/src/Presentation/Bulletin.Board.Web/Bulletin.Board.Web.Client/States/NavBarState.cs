namespace Bulletin.Board.Web.Client.States;

public class NavBarState
{
    private bool _isDark;

    public bool IsDark
    {
        get => _isDark;
        set
        {
            if (_isDark == value) return;
            _isDark = value;
            OnChange?.Invoke();
        }
    }

    public event Action? OnChange;
}
