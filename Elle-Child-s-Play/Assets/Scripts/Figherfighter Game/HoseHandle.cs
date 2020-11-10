using UnityEngine;

public class HoseHandle : MonoBehaviour
{
    public Transform target;
    void Start()
    {
        transform.parent = target;
        transform.localPosition = Vector3.zero;
    }
}