using System;

public static class Util
{
    private static Delegate[] _cachedHandlers;
    private static Func<bool> _lastEvent;

    public static bool CheckEventFunc(this Func<bool> events)
    {
        if (events == null) return false;

        // 캐시 활용
        if (_lastEvent != events)
        {
            _lastEvent = events;
            _cachedHandlers = events.GetInvocationList();
        }

        foreach (Func<bool> handler in _cachedHandlers)
        {
            if (handler())
            {
                return true;
            }
        }
        return false;
    }
}