using System.Collections;
using Cobalt.Core.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MenuSpotItem : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image image;
    public RectTransform circles;

    private Spot spot;
    public Spot Spot
    {
        get { return spot; }
        set {
            if (spot != value)
            {
                spot = value;
                Rebuild();
            }
        }
    }

    private void Rebuild()
    {
        text.text = Spot.EndPoint.Address.ToString();
        OnToggle();
    }

    public void Select()
    {
        GetComponent<Toggle>().isOn = true;
    }

    public void OnToggle()
    {
        var isOn = GetComponent<Toggle>().isOn;
        GetComponent<Animator>().Play(isOn ? "Connected" : "Disconnected");

        if (isOn)
            StartCoroutine(Connect());
    }

    private IEnumerator Connect()
    {
        yield break;


        

        var url = string.Format("http://{0}/join", Spot.EndPoint);
        var www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isDone && !www.isHttpError && !www.isNetworkError)
            App.DoConnect(www.downloadHandler.data);
    }
}