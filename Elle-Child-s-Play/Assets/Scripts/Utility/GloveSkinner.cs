using UnityEngine;

public class GloveSkinner : MonoBehaviour
{
    private Renderer r;
    private Material[] gloveSkins;

    void Start()
    {
        r = GetComponent<Renderer>();
        gloveSkins = Resources.LoadAll<Material>("Glove Skins");
        UpdateSkin();
    }

    public void UpdateSkin()
    {
        foreach (Material m in gloveSkins)
        {
            if (m.name == ELLEAPI.glovesSkin)
                r.material = m;
        }
    }
}
