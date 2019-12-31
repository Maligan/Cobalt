using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cobalt.UI
{
    public class UILoader : UIPanel
    {
        [Range(0f, 1f)]
        public float ratio = 1;
        public Image ratioImage;

        #if UNITY_EDITOR
        private new void OnValidate() => Refresh();
        #endif

        private void OnEnable() { App.Hook.OnProgressChange += OnProgress; }
        private void OnDisable() { App.Hook.OnProgressChange += OnProgress; }

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
}