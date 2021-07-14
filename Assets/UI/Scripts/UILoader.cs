using UnityEngine;
using UnityEngine.UI;

public class UILoader : UIPanel
{
    [Range(0f, 1f)]
    public float ratio = 1;
    public Image ratioImage;

    #if UNITY_EDITOR
    private new void OnValidate() => Refresh();
    #endif

    private void OnEnable() { App.Hook.OnLoadingProgressChange += OnProgress; }
    private void OnDisable() { App.Hook.OnLoadingProgressChange += OnProgress; }

    private void OnProgress(float value)
    {
        ratio = value;
        Refresh();
    }

    private void Refresh()
    {
        ratioImage.fillAmount = ratio;
    }
}