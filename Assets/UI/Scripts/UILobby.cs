using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cobalt.UI
{
    public class UILobby : UIPanel
    {
        private const string HINT_0 = "Collect all <#ffff00>coins</color> before <#ff0000>enemies</color>";
        private const string HINT_1 = "Tap Play for connect to <#ffff00>{0}</color>";
        private const string HINT_2 = "Tap Play for host new game";

        private const string EVENT_CLICK = "EVENT_CLICK";
        private const string EVENT_UPDATE = "EVENT_UPDATE";

        public TextMeshProUGUI Hint;
        public Button Button;

        private FSM<UILobbyState> fsm = new FSM<UILobbyState>(UILobbyState.None);

        protected override IEnumerator Show()
        {
            App.Lobby.Scan();
            yield break;
        }

        private void Start()
        {
            fsm.On(UILobbyState.None, EVENT_CLICK, () => {

                // fsm.To(UILobbyState.Await);
                // GetComponent<Animator>().Play("Await");
                if (App.Lobby.Spots.Count > 0) App.Lobby.Connect(App.Lobby.Spots[0]);
                else                           App.Lobby.Host(true);
                Close();
                
            });

            fsm.On(UILobbyState.Await, EVENT_CLICK, () => {
                fsm.To(UILobbyState.None);
                GetComponent<Animator>().Play("AwaitToNone");
            });

            // fsm.On(UILobbyState.Scan, EVENT_CLICK, () => {
            //     Button.GetComponentInChildren<TextMeshProUGUI>().text = "Connect";
            //     fsm.To(UILobbyState.None);
            // });
        }

        public void Update()
        {
            Hint.text = App.Lobby.Spots.Count > 0
                ? string.Format(HINT_1, App.Lobby.Spots[0].EndPoint)
                : HINT_2;

            fsm.Do(EVENT_UPDATE);
        }

        public void OnButtonClick()
        {
            fsm.Do(EVENT_CLICK);
        }

        public void OnSettingsClick()
        {
            App.UI<UISettings>().Open();
        }
    }    
}

public enum UILobbyState
{
    None = 0,
    Await,
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