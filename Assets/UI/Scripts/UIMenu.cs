using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cobalt.UI
{
    public class UIMenu : UIPopup
    {
        public TextMeshProUGUI Hint;
        public Button Button;

        private enum State { None = 0, AwaitAsClient, AwaitAsServer, Countdown }
        private State state;

        public void OnButtonClick()
        {
            switch (state)
            {
                case State.None:
                    // Scan -> Join or Host
                    break;
                case State.AwaitAsClient:
                    // Hold On...
                    break;
                case State.AwaitAsServer:
                    // if (NumPlayers > 1) to Countdown;
                    // else Hold On...
                    break;
                case State.Countdown:
                    // Nop
                    break;
            }

            Close();
        }
    }    
}