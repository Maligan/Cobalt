using TMPro;
using UnityEngine;

namespace Cobalt.UI
{
    public class UIMenu : UIPanel
    {
        private const string HINT_0 = "Collect all <#ffff00>coins</color> before <#ff0000>enemies</color>";
        private const string HINT_1 = "Tap Play for connect to <#ffff00>{0}</color>";
        private const string HINT_2 = "Tap Play for host new game";

        private const string EVENT_CLICK = "EVENT_CLICK";
        private const string EVENT_UPDATE = "EVENT_UPDATE";

        public TextMeshProUGUI Version;

        private void Start()
        {
            Version.text = $"v{Application.version}";
        }

        public void OnPlayClick()
        {
            // UIManager.Get<UIDiscovery>().Show();
            
            Hide();
            App.Lobby.Host(true);
        }
    }    
}