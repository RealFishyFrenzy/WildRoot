using System;

// Explicit gameplay messages, independent of Console logging and presentation.
public static class GameplayNotifications
{
    public static event Action<string, string> Posted;

    public static void Show(string message, string repeatKey = null)
    {
        if (!string.IsNullOrWhiteSpace(message))
            Posted?.Invoke(message, repeatKey);
    }
}
