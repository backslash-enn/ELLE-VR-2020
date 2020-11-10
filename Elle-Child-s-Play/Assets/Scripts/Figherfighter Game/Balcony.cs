using TMPro;
using UnityEngine;

public class Balcony : MonoBehaviour
{
    private float life = 1.5f;
    public ParticleSystem fire;
    public TMP_Text t;

    public void Activate(string word)
    {
        t.text = word;
        fire.Play();
        life = 2;
    }

    private void Update()
    {
        var e = fire.emission;
        e.rateOverTime = Mathf.Lerp(0, 120, life / 1.5f);
    }

    private void Deactivate()
    {
        t.text = "";
        fire.Stop();
    }

    public void LowerLife()
    {
        life -= Time.deltaTime;
        if (life <= 0) Deactivate();
    }
}
