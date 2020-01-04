using UnityEngine;

public class Rotation : MonoBehaviour
{
    [Tooltip("Angles per second")]
    [SerializeField]
    public Vector3 angularVelocity;

    private void Update()
    {
        var angle = transform.localRotation.eulerAngles;
        var delta = angularVelocity * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(angle + delta);
    }
}
