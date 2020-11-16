using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public class Balcony : MonoBehaviour
{
    private float life = 1.5f;
    public ParticleSystem fire, embers, smoke;
    public Light l;
    public TMP_Text t;

    public void Activate(string word)
    {
        t.transform.parent.gameObject.SetActive(true);
        t.text = word;
        life = 2;

        fire.Play();
        embers.Play();
        smoke.Play();
        l.gameObject.SetActive(true);
        l.intensity = 4;
    }

    private void Update()
    {
        float lerpThing = life / 1.5f;

        l.intensity = Mathf.Lerp(0, 4, lerpThing);

        var e = fire.emission;
        e.rateOverTime = Mathf.Lerp(0, 120, lerpThing);
        e = embers.emission;
        e.rateOverTime = Mathf.Lerp(0, 30, lerpThing);
        e = smoke.emission;
        e.rateOverTime = Mathf.Lerp(0, 3, lerpThing);
    }

    private void Deactivate()
    {
        t.transform.parent.gameObject.SetActive(false);
        fire.Stop();
        embers.Stop();
        smoke.Stop();
        l.gameObject.SetActive(false);
    }

    public void LowerLife()
    {
        life -= Time.deltaTime;
        if (life <= 0) Deactivate();
    }
}
