using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private GameObject menu, resumeButton;
    public Fader fader;
    public static bool paused, canPause;

    void Start()
    {
        menu = transform.GetChild(0).gameObject;
        resumeButton = menu.transform.GetChild(1).gameObject;
    }
    void Update()
    {
        if (VRInput.startButtonDown)
        {
            if (!paused && canPause) Pause();
            else Resume();
        }

        if (paused && EventSystem.current.currentSelectedGameObject == null)
            EventSystem.current.SetSelectedGameObject(resumeButton);
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
        paused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Time.timeScale = 1;
        paused = false;
        SceneManager.LoadScene("Hubworld");
    }
}
