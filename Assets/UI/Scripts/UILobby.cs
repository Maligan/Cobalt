using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace Cobalt.UI
{
    public class UILobby : UIPanel
    {
        private const string OnClick = "OnClick";
        private const string OnUpdate = "OnUpdate";
        private enum State { None = 0, Scan, AwaitAsClient, AwaitAsServer, Countdown }

        public TextMeshProUGUI Hint;
        public Button Button;

        private FSM<State> fsm = new FSM<State>(State.None);

        private void Start()
        {
            fsm.On(OnClick, State.None, () => {
                // fsm.To(State.Scan);
                Button.GetComponentInChildren<TextMeshProUGUI>().text = "Scan...";
                App.LobbyManager.LocalScan();
            });

            fsm.On(OnClick, State.Scan, () => {
                App.LobbyManager.LocalHost();
                Close();
                // Button.GetComponentInChildren<TextMeshProUGUI>().text = "Connect";
                // fsm.To(State.None);
            });

            fsm.On(OnClick, State.AwaitAsServer, () => {
                // if (NumPlayers > 1) fsm.To(State.Countdown);
                // else Hold On...
            });
        }

        public void OnButtonClick()
        {
            fsm.Do(OnClick);

            /*
            switch (state)
            {
                case State.None:
                    // Scan -> Join or Host
                    break;
                case State.Scan:
                    // Hold On...
                    break;
                case State.AwaitAsClient:
                    // Hold On...
                    break;
                case State.AwaitAsServer:
                    // if (NumPlayers > 1) to Countdown;
                    // else Hold On...
                    break;
                case State.Countdown:
                    // Hold On...
                    break;
            }
            */
        }

        private void Update()
        {
        }

        private void UpdateScan()
        {

        }
    }    
}

public class FSM<T> where T : Enum
{
    private Dictionary<string, Action> table;
    private T state;

    public FSM(T state)
    {
        this.state = state;
        this.table = new Dictionary<string, Action>();
    }

    public void To(T state)
    {
        Do("__exit");
        this.state = state;
        Do("__enter");
    }

    public void Do(string key)
    {
        var k = state + key;
        var h = table.ContainsKey(k) ? table[k] : null;
        if (h != null) h();
    }

    public void On(string key, T state, Action handler) { table[state + key] = handler; }
    public void OnExit(T state, Action handler) { On("__exit", state, handler); }
    public void OnEnter(T state, Action handler) { On("__enter", state, handler); }
}