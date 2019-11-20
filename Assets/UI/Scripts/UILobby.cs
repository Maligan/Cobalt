using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace Cobalt.UI
{
    public class UILobby : UIPanel
    {
        private const string EVENT_CLICK = "EVENT_CLICK";
        private const string EVENT_UPDATE = "EVENT_UPDATE";

        public TextMeshProUGUI Hint;
        public Button Button;

        private FSM<UILobbyState> fsm = new FSM<UILobbyState>(UILobbyState.None);

        private void Start()
        {
            fsm.On(UILobbyState.None, EVENT_CLICK, () => {
                // fsm.To(UILobbyState.Scan);
                // Button.GetComponentInChildren<TextMeshProUGUI>().text = "Scan...";
                // App.LobbyManager.LocalScan();

                App.LobbyManager.LocalHost(true);
                Close();
            });

            fsm.On(UILobbyState.Scan, EVENT_CLICK, () => {
                Button.GetComponentInChildren<TextMeshProUGUI>().text = "Connect";
                fsm.To(UILobbyState.None);
            });

            fsm.On(UILobbyState.AwaitAsServer, EVENT_CLICK, () => {
                // if (NumPlayers > 1) fsm.To(State.Countdown);
                // else Hold On...
            });
        }

        public void Update()
        {
            fsm.Do(EVENT_UPDATE);
        }

        public void OnButtonClick()
        {
            fsm.Do(EVENT_CLICK);
        }
    }    
}

public enum UILobbyState
{
    None = 0,
    Scan,
    AwaitAsClient,
    AwaitAsServer,
    Countdown
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

    //
    // Configuraion
    //

    public void On(T state, string key, Action handler) { table[state + key] = handler; }
    public void OnExit(T state, Action handler) { On(state, "__exit", handler); }
    public void OnEnter(T state, Action handler) { On(state, "__enter", handler); }

    //
    // Action
    //

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
}