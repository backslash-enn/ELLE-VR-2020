using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temp : MonoBehaviour
{
    public Vector3 moveVector;
    public Vector3 rotateVector;

    void Update()
    {
        Vector3 moveV = transform.right * moveVector.x * Time.deltaTime +
            transform.up * moveVector.y * Time.deltaTime +
            transform.forward * moveVector.z * Time.deltaTime;

        transform.position += moveV;
            
        transform.Rotate(rotateVector * 10 * Time.deltaTime, Space.World);
    }
}
