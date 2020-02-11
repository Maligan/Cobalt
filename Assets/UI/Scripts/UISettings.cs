using System.Collections;
using System.Collections.Generic;
using Cobalt.UI;
using Netouch;
using UnityEngine;
using UnityEngine.UI;

public class UISettings : UIPanel
{
    [Range(1, 4)]
    public int NumPlayers = 2;

    public List<Image> Images;

    private string CloseState;

    private void Start()
    {
        var swipe = new SwipeGesture(gameObject);
        swipe.Recognized += OnSwipe;
    }

    private void OnSwipe(Gesture gesture)
    {
        var swipe = (SwipeGesture)gesture;
        if (swipe.Direction == SwipeGestureDirection.Up)
        {
            CloseState = "HideUp";
            Close();
        }
        else if (swipe.Direction == SwipeGestureDirection.Down)
        {
            CloseState = "HideDown";
            Close();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) == true)
        {
            var rectTransform = (RectTransform)transform.GetChild(0);
    
            var rectUnderCursor = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, GetComponentInParent<Canvas>().worldCamera); 
            if (rectUnderCursor == false)
            {
                CloseState = "HideUp";
                Close();
            }
        }
    }

    public void OnImageClick(Image image)
    {   
        NumPlayers = Images.IndexOf(image) + 1;
        Refresh();
    }

    private void Refresh()
    {
        for (var i = 0; i < Images.Count; i++)
            Images[i].color = (i+1 == NumPlayers)
                ? new Color(1, 1, 1, 0.1f)
                : new Color(0, 0, 0, 0.1f);
    }

    private new void OnValidate()
    {
        NumPlayers = Mathf.Clamp(NumPlayers, 1, 4);
        Refresh();
    }

    protected override IEnumerator Show() => PlayAndAwait("Show");
    protected override IEnumerator Hide() => PlayAndAwait(CloseState);
}
