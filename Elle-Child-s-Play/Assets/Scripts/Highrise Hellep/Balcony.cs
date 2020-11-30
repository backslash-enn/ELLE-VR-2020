using TMPro;
using UnityEngine;

public class Balcony : MonoBehaviour
{
    public HighriseHellepeManager manager;
    private float life = 1.5f;
    public ParticleSystem fire, embers, smoke;
    public Light l;
    private Term currentTerm;
    public TMP_Text t;
    private bool animatingIn;
    private float animateInFactor;

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
        animateInFactor = l.intensity = 0;
        animatingIn = true;
    }

    private void Update()
    {
        float lerpThing = life / 1.5f;

        animateInFactor += 1 * Time.deltaTime;
        if (animateInFactor >= 1) animatingIn = false;
        l.intensity = Mathf.Lerp(0, 4, animatingIn ? animateInFactor : lerpThing);

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
