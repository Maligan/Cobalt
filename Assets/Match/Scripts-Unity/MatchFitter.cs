using UnityEngine;

[ExecuteAlways]
public class MatchFitter : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.localScale = 0.5f * Vector3.one * Mathf.Min(Screen.width, Screen.height) / Screen.height;
        transform.localRotation = Screen.width > Screen.height ? Quaternion.identity : Quaternion.Euler(0, 0, -90);
    }
}
