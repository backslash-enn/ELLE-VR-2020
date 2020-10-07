using System.Collections.Generic;
using UnityEngine;

public class PhysicsThrow : MonoBehaviour
{
    public int weightFrames;
    public int ignoreFrames;
    public float throwStrength;
    private bool isCapturingPositions;
    private List<Vector3> positionsDeltas;
    private Vector3 lastPosition;
    private Rigidbody rb;

    void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isCapturingPositions)
        {
            positionsDeltas.Add(transform.position - lastPosition);
            lastPosition = transform.position;
            if (positionsDeltas.Count > weightFrames)
                positionsDeltas.RemoveAt(0);
        }
    }

    public void StartPhysicsThrowCapture()
    {
        isCapturingPositions = true;
        lastPosition = transform.position;
        positionsDeltas = new List<Vector3>(weightFrames);
    }

    public void ActivatePhysicsThrow()
    {
        isCapturingPositions = false;
        Vector3 throwVector = Vector3.zero;

        for (int i = 0; i < positionsDeltas.Count; i++)
        {
            if (i >= positionsDeltas.Count - ignoreFrames) break;
            throwVector += positionsDeltas[i];
        }
        throwVector /= (positionsDeltas.Count - ignoreFrames);

        rb.velocity = throwVector * throwStrength;
    }
}
