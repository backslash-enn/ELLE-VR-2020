using UnityEngine;

public class Fader : MonoBehaviour
{
    private CanvasGroup cg;
    private bool fadingIn, fadingOut;
    private float fadeSpeed;

    void Start()
    {
        cg = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (fadingIn)
        {
            cg.alpha += fadeSpeed * Time.deltaTime;
            if (cg.alpha >= 1)
                fadingIn = false;
        }

        if (fadingOut)
        {
            cg.alpha -= fadeSpeed * Time.deltaTime;
            if (cg.alpha <= 0)
                fadingOut = false;
        }
    }

    public void Fade(bool fadeIn = true, float speed = 1, bool instant = false)
    {
        if(instant)
        {
            cg.alpha = fadeIn ? 1 : 0;
            return;
        }
        fadeSpeed = speed;
        fadingIn = fadeIn;
        fadingOut = !fadeIn;
    }
}
