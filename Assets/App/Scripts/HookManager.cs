using System;
using Cobalt;

public class HookManager
{
    public Hook<float> OnLoadingProgressChange;
    public Hook OnLobbyChange;
}

#region Hooks

public struct Hook
{
    private Action actions;
    
    public void Invoke()
    {
        if (actions == null) return;

        foreach (Action action in actions.GetInvocationList())
        {
            try { action.Invoke(); }
            catch (Exception e) { Log.Error(this, e); }
        }
    }

    public static Hook operator +(Hook hook, Action action)
    {
        hook.actions += action;
        return hook;
    }

    public static Hook operator -(Hook hook, Action action)
    {
        hook.actions -= action;
        return hook;
    }
}

public struct Hook<T>
{
    private Action<T> actions;
    
    public void Invoke(T args)
    {
        if (actions == null) return;

        foreach (Action<T> action in actions.GetInvocationList())
        {
            try { action.Invoke(args); }
            catch (Exception e) { Log.Error(this, e); }
        }
    }

    public static Hook<T> operator +(Hook<T> hook, Action<T> action)
    {
        hook.actions += action;
        return hook;
    }

    public static Hook<T> operator -(Hook<T> hook, Action<T> action)
    {
        hook.actions -= action;
        return hook;
    }
}

#endregion