using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BTree
{
    private enum Type
    {
        If,
        While,
        Do,
    }

    private enum State
    {
        None,
        Process,
        Complete,
    }

    private Type type = Type.If;
    private Func<bool> condition;
    private List<BTree> children = new List<BTree>();
    private BTree parent;

    private State state;

    public void Tick()
    {
        switch (type)
        {
            case Type.If:

                if (state == State.None)
                {
                    var success = condition == null || condition();
                    if (success) state = State.Process;
                    else state = State.Complete;
                }

                if (state == State.Process)
                {
                    var first = children.FirstOrDefault(n => n.state != State.Complete);
                    if (first != null) first.Tick();
                    else state = State.Complete;
                }

                break;
            
            case Type.While:

                if (state == State.None || state == State.Process)
                {
                    var success = condition == null || condition();
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
                    Debug.Log($"{string.Format("{0:0.00}", Time.unscaledDeltaTime)} Do#{arg}");
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


    public BTree()
    {
    }

    private BTree(BTree parent, Type type)
    {
        this.type = type;
        this.parent = parent;
        this.parent.children.Add(this);
    }

    public BTree Do() { return new BTree(this, Type.Do) { }.parent; }

    public BTree If(Func<bool> condition) { return new BTree(this, Type.If) { condition = condition }; }
    public BTree While(Func<bool> condition) { return new BTree(this, Type.While) { condition = condition }; }
    public BTree End() { return parent; }
}