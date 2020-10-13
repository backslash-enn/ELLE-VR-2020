using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public EventSystem e;
    public GameObject menu, resumeButton;
    public BackFader fader;
    public static bool paused, canPause;

    void Update()
    {
        if (VRInput.startButtonDown)
        {
            if (!paused && canPause) Pause();
            else Resume();
        }

        if (paused && e.currentSelectedGameObject == null)
            e.SetSelectedGameObject(resumeButton);
    }

    public void Pause()
    {
        Time.timeScale = 0.001f;
        fader.FadeToBlack();
        paused = true;
        menu.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1;
        fader.FadeToColor();
        paused = false;
        menu.SetActive(false);
    }

    public void Restart ()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        SceneManager.LoadScene("Hubworld");
    }
}
