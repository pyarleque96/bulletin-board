namespace Bulletin.Board.Web.Client.States;

public class FooterState
{
    private bool _isCompact = true;
    private bool _isDark = true;

    public bool IsCompact
    {
        get => _isCompact;
        set
        {
            if (_isCompact == value) return;
            _isCompact = value;
            OnChange?.Invoke();
        }
    }

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
