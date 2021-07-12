using System.Collections;
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

            // StartCoroutine(ScanAndJoin());
        }

        private IEnumerator ScanAndJoin()
        {
            Log.Info(this, "Start ScanAndJoin()...");
            
            App.Lobby.Scan(2500);

            while (App.Lobby.IsScanning && App.Lobby.Spots.Count == 0)
                yield return null;

            if (App.Lobby.Spots.Count == 0)
            {
                Log.Warning(this, "There is no spots");
                yield break;
            }

            // Join
            var spot = App.Lobby.Spots[0];
            Log.Info(this, $"Connect to {spot.EndPoint}");
            Hide();
            App.Lobby.Join(App.Lobby.Spots[0]);
        }
    }    
}