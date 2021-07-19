using System.Collections;
using System.Collections.Generic;
using Cobalt;
using Mopsicus.InfiniteScroll;
using Netouch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDiscovery : UIPanel
{
    [Header("Animations")]
    public AnimationClip PopupShow;
    public AnimationClip PopupHide;

    [Header("Parts")]
    public InfiniteScroll _list;
    public GameObject _listEmpty;
    
    public List<Rotation> _gears;
    [Range(1, 20)]
    public float _gearsSpeed = 1;

    private void Start()
    {
        _list.OnHeight += i => (int)_list.Prefab.GetComponent<RectTransform>().rect.height;
        _list.OnWidth += i => (int)_list.Prefab.GetComponent<RectTransform>().rect.width;
        _list.OnFill += (i, view) => view.GetComponentInChildren<TextMeshProUGUI>().text = "Label " + i;
        _list.InitData(15);
        _listEmpty.SetActive(false);
    }

    public void OnValidate()
    {
        SetGearsSpeed(_gearsSpeed);
    }

    public void OnButtonHostClick()
    {
        Hide();
        Get<UIMenu>().HostAndJoin();
    }

    private void SetGearsSpeed(float speed)
    {
        const float value = 16f;
        for (var i = 0; i < _gears.Count; i++)
            _gears[i].angularVelocity = new Vector3(0, 0, (i%2-1/2f) * (i+1) * value * speed);
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

    protected override IEnumerator OnShow() => PlayAndAwait(PopupShow);
    protected override IEnumerator OnHide() => PlayAndAwait(PopupHide);
}
