using UnityEngine;

public class ScrollTexture : MonoBehaviour
{
    public Vector2 scrollAmount;
    private Vector2 currentOffset;
    private Material mat;
    void Start()
    {
        mat = GetComponent<Renderer>().material;
        currentOffset = Vector2.zero;
    }

    void Update()
    {
        currentOffset += scrollAmount * Time.deltaTime;
        mat.SetTextureOffset("_BaseMap", currentOffset);
    }
}