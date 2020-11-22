using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public class Balcony : MonoBehaviour
{
    public FirefighterGameManager manager;
    private float life = 1.5f;
    public ParticleSystem fire, embers, smoke;
    public Light l;
    private Term currentTerm;
    public TMP_Text t;

    public void Activate(Term term)
    {
        t.transform.parent.gameObject.SetActive(true);
        currentTerm = term;
        t.text = currentTerm.front;
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

    public void Deactivate(bool putOut)
    {
        t.transform.parent.gameObject.SetActive(false);
        fire.Stop();
        embers.Stop();
        smoke.Stop();
        l.gameObject.SetActive(false);
        life = 2;

        if (putOut) manager.CheckIfCorrect(currentTerm.back);
    }

    public void LowerLife()
    {
        life -= Time.deltaTime;
        if (life <= 0) Deactivate(true);
    }
}
