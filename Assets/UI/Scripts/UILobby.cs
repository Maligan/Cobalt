using System;
using System.Collections;
using System.Collections.Generic;
using Netouch;
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
        public List<GameObject> Slots;

        private FSM<UILobbyState> fsm = new FSM<UILobbyState>(UILobbyState.None);

        protected override IEnumerator Show()
        {
            App.Lobby.Scan();
            yield break;
        }

        private void Start()
        {
            var tap = new TapGesture(Button.gameObject); tap.Recognized += _ => OnPlayTap();
            var longPress = new LongPressGesture(Button.gameObject); longPress.Recognized += _ => OnPlayLongPress();

            fsm.On(UILobbyState.None, EVENT_CLICK, () => {

                var numPlayers = App.UI<UISettings>().NumPlayers;
                for (var i = 0; i < Slots.Count; i++)
                    Slots[i].SetActive(i < numPlayers);

                fsm.To(UILobbyState.Await);
                // GetComponent<Animator>().Play("Await");
                StartCoroutine(ToAwait());
                longPress.IsActive = false;

                // if (App.Lobby.Spots.Count > 0) App.Lobby.Join(App.Lobby.Spots[0]);
                // else                           App.Lobby.Host(true);
                // Close();
            });

            fsm.On(UILobbyState.Await, EVENT_CLICK, () => {
                fsm.To(UILobbyState.None);
                Button.GetComponent<Animator>().SetTrigger("Stop");
                GetComponent<Animator>().Play("AwaitToNone");
                longPress.IsActive = true;
            });

            // fsm.On(UILobbyState.Scan, EVENT_CLICK, () => {
            //     Button.GetComponentInChildren<TextMeshProUGUI>().text = "Connect";
            //     fsm.To(UILobbyState.None);
            // });
        }

        private IEnumerator ToAwait()
        {
            // foreach (var slot in Slots) slot.GetComponent<Animator>().Play("Idle", 0, 1);

            Button.GetComponent<Animator>().SetTrigger("Play");
            yield return PlayAndAwait(GetComponent<Animator>(), "Await");

            // Slots[0].GetComponent<Animator>().Play("Charge");
            // Slots[1].GetComponent<Animator>().Play("Charge");
            // yield return new WaitForSeconds(10f);
            // Slots[2].GetComponent<Animator>().Play("Charge");
        }

        public void Update()
        {
            Hint.text = App.Lobby.Spots.Count > 0
                ? string.Format(HINT_1, App.Lobby.Spots[0].EndPoint)
                : HINT_2;

            fsm.Do(EVENT_UPDATE);
        }

        private void OnPlayTap()
        {
            fsm.Do(EVENT_CLICK);
        }

        private void OnPlayLongPress()
        {
            App.UI<UISettings>().Open();
        }

        private IEnumerator PlayAndAwait(Animator animator, string state)
        {
            if (animator != null && animator.enabled)
            {
                animator.Play(state);
                yield return null;

                var layer = animator.GetCurrentAnimatorStateInfo(0);
                var layerTime = layer.length;
                yield return new WaitForSeconds(layerTime);
            }
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