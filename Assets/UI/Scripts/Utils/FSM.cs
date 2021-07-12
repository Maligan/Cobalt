using System;
using System.Collections.Generic;

public class FSM<T> where T : Enum
{
    private Dictionary<string, Action> table;
    private T state;

    public FSM(T state)
    {
        this.state = state;
        this.table = new Dictionary<string, Action>();
    }

    //
    // Configuraion
    //

    public void On(T state, string key, Action handler) { table[state + key] = handler; }
    public void OnExit(T state, Action handler) { On(state, "_exit", handler); }
    public void OnEnter(T state, Action handler) { On(state, "_enter", handler); }

    //
    // Action
    //

    public void To(T state)
    {
        Do("_exit");
        this.state = state;
        Do("_enter");
    }

    public void Do(string key)
    {
        var k = state + key;
        var h = table.ContainsKey(k) ? table[k] : null;
        if (h != null) h();
    }
}