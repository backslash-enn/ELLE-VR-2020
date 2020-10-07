using UnityEngine;
using UnityEngine.UI;

public class StickerCursor : MonoBehaviour
{
    public Transform stickerT;
    private Color defaultColor, hoveredColor;
    public GameObject sticker;
    public bool holdingSticker;

    void Start()
    {
        defaultColor = new Color(1, 1, 1, 1);
        hoveredColor = new Color(0.35f, 0.5f, 0.5f, 0.5f);
    }

    public void GrabSticker()
    {
        sticker.GetComponent<Renderer>().material.color = defaultColor;
        holdingSticker = true;
        sticker.transform.parent = stickerT;
        sticker.transform.localPosition = Vector3.zero;
    }

    public void RemoveSticker(bool destroy)
    {
        if(destroy) Destroy(sticker);
        sticker = null;
        holdingSticker = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!holdingSticker && other.CompareTag("Sticker"))
        {
            sticker = other.gameObject;
            sticker.GetComponent<Renderer>().material.color = hoveredColor;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!holdingSticker && sticker != null && other.CompareTag("Sticker"))
        {
            sticker.GetComponent<Renderer>().material.color = defaultColor;
            sticker = null;
        }
    }
}
