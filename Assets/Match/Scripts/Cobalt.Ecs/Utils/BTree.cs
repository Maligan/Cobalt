using System;
using System.Collections.Generic;
using System.Linq;

public class Btree<T> 
{
    private enum Type
    {
        If,
        Else,
        While,
        Do
    }

    private enum State
    {
        None,
        Process,
        Complete
    }

    private Type type = Type.If;
    private State state;
    private List<Btree<T>> children = new List<Btree<T>>();
    private Btree<T> parent;

    private T context;
    private Func<T, bool> condition;
    private Action<T> method;

    public void Tick()
    {
        switch (type)
        {
            case Type.If:
            case Type.Else:

                if (state == State.None)
                {
                    var success = condition == null || condition(context);
                    if (success)
                    {
                        state = State.Process;

                        if (parent != null)
                        {
                            for (var i = parent.children.IndexOf(this)+1; i < parent.children.Count; i++)
                            {
                                var sibling = parent.children[i];
                                if (sibling.type == Type.Else)
                                    sibling.state = State.Complete;
                                else
                                    break;
                            }
                        }
                    }
                    else state = State.Complete;
                }
                else if (state == State.Process)
                {
                    var first = children.FirstOrDefault(n => n.state != State.Complete);
                    if (first != null) first.Tick();
                    else state = State.Complete;
                }

                break;
            
            case Type.While:

                if (state == State.None || state == State.Process)
                {
                    var success = condition == null || condition(context);
                    if (success)
                    {
                        state = State.Process;

                        var first = children.FirstOrDefault(n => n.state != State.Complete);
                        if (first == null) Reset();
                        else first.Tick();
                    }
                    else
                    {
                        state = State.Complete;
                    }
                }

                break;

            case Type.Do:

                if (state == State.None)
                {
                    state = State.Complete;
                    if (method != null) method(context);
                }

                break;
        }
    }

    public void Reset()
    {
        state = State.None;

        foreach (var child in children)
            child.Reset();
    }

    public Btree(T context)
    {
        this.context = context;
    }

    private Btree(Btree<T> parent, Type type)
    {
        this.type = type;
        this.parent = parent;
        this.context = parent.context;
        this.parent.children.Add(this);
    }

    public Btree<T> Do(Action<T> method) { return new Btree<T>(this, Type.Do) { method = method }.parent; }
    public Btree<T> Do(Btree<T> node)
    {
        node.parent = this;
        children.Add(node);
        return this;
    }

    public Btree<T> If(Func<T, bool> condition = null) { return new Btree<T>(this, Type.If) { condition = condition }; }
    public Btree<T> Else(Func<T, bool> condition = null) { return new Btree<T>(this, Type.Else) { condition = condition }; }
    public Btree<T> While(Func<T, bool> condition = null) { return new Btree<T>(this, Type.While) { condition = condition }; }
    public Btree<T> End() { return parent; }
}