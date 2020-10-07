using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackFader : MonoBehaviour
{
    private CanvasGroup cg;
    private bool fadingBlack, fadingColor;
    private float fadeSpeed = .09f;
    public GameObject pauseMenu;

    void Start()
    {
        cg = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (fadingBlack)
        {
            cg.alpha += fadeSpeed;
            if (cg.alpha >= 1)
            {
                cg.alpha = 1;
                fadingBlack = false;
            }
        }

        if (fadingColor)
        {
            cg.alpha -= fadeSpeed;
            if (cg.alpha <= 1)
            {
                cg.alpha = 0;
                fadingColor = false;
            }
        }
    }

    public void FadeToBlack()
    {
        fadingBlack = true;
        pauseMenu.SetActive(true);
    }

    public void FadeToColor()
    {
        fadingColor = true;
        pauseMenu.SetActive(false);
    }
}
