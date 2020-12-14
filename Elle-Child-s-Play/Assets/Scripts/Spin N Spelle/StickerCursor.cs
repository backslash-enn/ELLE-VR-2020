using System.Collections;
using UnityEngine;

public class StickerCursor : MonoBehaviour
{
    public Transform stickerT;
    private Color defaultColor, hoveredColor;
    public GameObject sticker;
    public bool holdingSticker;
    public Transform[] stickerParents;
    public GameObject[] stickerPrefabs;

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
        int stickerNum = int.Parse(sticker.name[8].ToString());
        StartCoroutine(ReplaceSticker(stickerNum - 1));
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

    private IEnumerator ReplaceSticker(int index)
    {
        yield return new WaitForSeconds(2);
        GameObject g = Instantiate(stickerPrefabs[index], stickerParents[index]);
        g.transform.localPosition = Vector3.zero;
    }
}
