using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public EventSystem e;
    public GameObject menu, resumeButton;
    public Fader fader;
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
        fader.Fade(true, 1, true);
        Time.timeScale = 0;
        paused = true;
        menu.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1;
        fader.Fade(false);
        paused = false;
        menu.SetActive(false);
    }

    public void Restart ()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Hubworld");
    }
}
