using System.Collections;
using Netouch;
using UnityEngine;
using UnityEngine.UI;

public class UIDiscovery : UIPanel
{
    [Header("Animations")]
    public AnimationClip PopupShow;
    public AnimationClip PopupHide;

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) == true)
        {
            var rectTransform = (RectTransform)transform.GetChild(0);
    
            var rectUnderCursor = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, GetComponentInParent<Canvas>().worldCamera); 
            if (rectUnderCursor == false)
                Hide();
        }
    }

    public void OnImageClick(Image image)
    {   
    }

    private void Refresh()
    {
    }

    private new void OnValidate()
    {
    }

    protected override IEnumerator OnShow() => PlayAndAwait(PopupShow);
    protected override IEnumerator OnHide() => PlayAndAwait(PopupHide);
}
