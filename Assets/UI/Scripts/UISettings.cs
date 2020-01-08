using System.Collections;
using System.Collections.Generic;
using Cobalt.UI;
using UnityEngine;

public class UISettings : UIPanel
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) == true)
            Close();
    }

    protected override IEnumerator Show() => PlayAndAwait("Show");
    // protected override IEnumerator Hide() => PlayAndAwait("Hide");
}
