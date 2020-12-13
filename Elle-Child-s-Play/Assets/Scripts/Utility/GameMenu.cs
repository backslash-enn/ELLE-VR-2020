using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameMenu : MonoBehaviour
{
    public VideoClip backgroundVideo;
    public Texture backgroundImage;
    public bool useVideoForBackground;
    public GameObject background;
    public AudioClip preGameMusic, inGameMusic, postGameMusic;

    private AudioSource aud;
    public AudioSource musicAud;
    private CanvasGroup chooseTermsMenuCG;
    public GameObject chooseTermsMenu, ctmStartButtom;

    public TMP_Text gameModeText;
    public Image gameModeCover, gameModeUncover;
    private bool gameModeCoverLockedOut;

    public Transform moduleListUIParent;
    public GameObject moduleUIElement;
    public Color[] moduleElementColors;
    public Button checkTermsButton, uncheckTermsButton, startButton;
    public TMP_Text scoreFractionText, scorePercentageText;

    private bool ctmIsOpen, openingCTM, closingCTM;
    public Transform termListUIParent;
    public GameObject termUIElement;
    private Vector3 openCTMVector, closeCTMVector;

    public ScrollRect modulesSR, termsSR;

    [HideInInspector]
    public GameMode currentGameMode = GameMode.Quiz;
    [HideInInspector]
    public List<Module> moduleList;
    [HideInInspector]
    public List<Term> termList;
    [HideInInspector]
    public bool[] termEnabled;
    [HideInInspector]
    public Module currentModule;
    [HideInInspector]
    public bool inGame = false;
    [HideInInspector]
    public bool finishedGame = false;
    [HideInInspector]
    public bool goodToLeave;

    public delegate void BeginMethod();
    public BeginMethod onStartGame;

    public GameObject startMenu, endMenu;

    private bool fadingOutMusic;

    void Start()
    {
        aud = GetComponent<AudioSource>();
        chooseTermsMenuCG = chooseTermsMenu.GetComponent<CanvasGroup>();
        openCTMVector = new Vector3(0, chooseTermsMenu.transform.localPosition.y, chooseTermsMenu.transform.localPosition.z);
        closeCTMVector = new Vector3(-1, chooseTermsMenu.transform.localPosition.y, chooseTermsMenu.transform.localPosition.z);

        goodToLeave = true;

        if (useVideoForBackground)
        {
            var vp = background.GetComponent<VideoPlayer>();
            vp.clip = backgroundVideo;
            vp.enabled = true;
        }
        else
        {
            background.GetComponent<Renderer>().material.mainTexture = backgroundImage;
        }

        musicAud.clip = preGameMusic;
        musicAud.volume = 1;
        musicAud.Play();

        // Start in the main menu, where you choose a module
        moduleList = ELLEAPI.GetModuleList();
        for (int i = 0; i < moduleList.Count; i++)
        {
            GameObject g = Instantiate(moduleUIElement, moduleListUIParent);
            g.transform.GetChild(0).GetComponent<TMP_Text>().text = moduleList[i].name;
            g.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = moduleList[i].language.ToUpper();
            g.GetComponent<MenuModule>().menu = this;

            var b = g.GetComponent<Button>().colors;
            b.normalColor = moduleElementColors[i % moduleElementColors.Length];
            g.GetComponent<Button>().colors = b;

            if (i == 0) EventSystem.current.SetSelectedGameObject(g);
        }
    }

    void Update()
    {
        if (!inGame && !ctmIsOpen && goodToLeave && (VRInput.bDown || VRInput.yDown))
            SceneManager.LoadScene("Hubworld");

        if (finishedGame && goodToLeave && (VRInput.aDown || VRInput.xDown))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (!inGame)
        {
            GameObject currentActive = EventSystem.current.currentSelectedGameObject;

            // If somehow no option is highlighted in the menu, reset it to the first option
            if (currentActive == null)
            {
                if (ctmIsOpen)
                    EventSystem.current.SetSelectedGameObject(startButton.gameObject);
                else if (moduleListUIParent.childCount > 0)
                    EventSystem.current.SetSelectedGameObject(moduleListUIParent.GetChild(0).gameObject);
            }
            else if (ctmIsOpen)
            {
                if (EventSystem.current.currentSelectedGameObject.transform.parent == moduleListUIParent)
                    EventSystem.current.SetSelectedGameObject(startButton.gameObject);

                if (termList.Count > 4)
                {
                    // If the currentMenuOption isn't visible, fix that
                    (float minScroll, float maxScroll) = ValidScrollRange(currentActive.transform, termList.Count, 4);
                    float currentScroll = termsSR.verticalScrollbar.value;
                    if (currentScroll < minScroll || currentScroll > maxScroll)
                    {
                        float targetScroll = (currentScroll < minScroll) ? minScroll : maxScroll;
                        termsSR.verticalScrollbar.value = Mathf.Lerp(currentScroll, targetScroll, 10 * Time.deltaTime);
                    }
                }
            }

            // Else if is important. If not there it errors when fixing null
            else if (moduleList?.Count > 5)
            {
                // If the currentMenuOption isn't visible, fix that
                (float minScroll, float maxScroll) = ValidScrollRange(currentActive.transform, moduleList.Count, 5);
                float currentScroll = modulesSR.verticalScrollbar.value;
                if (currentScroll < minScroll || currentScroll > maxScroll)
                {
                    float targetScroll = (currentScroll < minScroll) ? minScroll : maxScroll;
                    modulesSR.verticalScrollbar.value = Mathf.Lerp(currentScroll, targetScroll, 10 * Time.deltaTime);
                }
            }

            if (gameModeUncover.fillAmount > 0)
            {
                gameModeUncover.fillAmount = Mathf.Lerp(gameModeUncover.fillAmount, 0, 5 * Time.deltaTime);
                if (gameModeUncover.fillAmount < 0.01f)
                    gameModeUncover.fillAmount = 0;
            }

            if (!finishedGame)
            {
                if (gameModeCoverLockedOut == false)
                {
                    gameModeCover.fillAmount = VRInput.rightTrigger + VRInput.leftTrigger;
                    if (gameModeCover.fillAmount <= 0.05f)
                        gameModeCover.fillAmount = 0;
                    if (VRInput.rightTrigger + VRInput.leftTrigger > 0.95f)
                    {
                        currentGameMode = currentGameMode == GameMode.Quiz ? GameMode.Endless : GameMode.Quiz;
                        gameModeText.text = currentGameMode == GameMode.Quiz ? "Quiz" : "Endless";
                        gameModeCoverLockedOut = true;
                        gameModeCover.fillAmount = 0;
                        gameModeUncover.fillAmount = 1;

                        aud.Play();
                    }
                }
                else if (VRInput.rightTrigger + VRInput.leftTrigger < 0.05f)
                    gameModeCoverLockedOut = false;
            }

            if (VRInput.bDown || VRInput.yDown)
            {
                if (ctmIsOpen)
                {
                    closingCTM = true;
                    openingCTM = false;
                    ctmIsOpen = false;

                    for (int i = 0; i < moduleListUIParent.childCount; i++)
                    {
                        moduleListUIParent.GetChild(i).GetComponent<MenuModule>().enabled = true;
                        moduleListUIParent.GetChild(i).GetComponent<Button>().enabled = true;
                    }

                    if (moduleListUIParent.childCount > 0)
                        EventSystem.current.SetSelectedGameObject(moduleListUIParent.GetChild(0).gameObject);
                }
            }
        }

        if (openingCTM)
        {
            chooseTermsMenuCG.alpha = Mathf.Lerp(chooseTermsMenuCG.alpha, 1, 7 * Time.deltaTime);
            chooseTermsMenu.transform.localPosition = Vector3.Lerp(chooseTermsMenu.transform.localPosition, openCTMVector, 7 * Time.deltaTime);
            if (chooseTermsMenuCG.alpha > 0.999f)
            {
                chooseTermsMenuCG.alpha = 1;
                chooseTermsMenu.transform.localPosition = openCTMVector;
                openingCTM = false;
            }
        }

        if (closingCTM)
        {
            chooseTermsMenuCG.alpha = Mathf.Lerp(chooseTermsMenuCG.alpha, 0, 7 * Time.deltaTime);
            chooseTermsMenu.transform.localPosition = Vector3.Lerp(chooseTermsMenu.transform.localPosition, closeCTMVector, 7 * Time.deltaTime);
            if (chooseTermsMenuCG.alpha < 0.001f)
            {
                chooseTermsMenuCG.alpha = 0;
                chooseTermsMenu.transform.localPosition = closeCTMVector;
                closingCTM = false;
                chooseTermsMenu.SetActive(false);
            }
        }

        if(fadingOutMusic)
        {
            musicAud.volume -= 0.3f * Time.deltaTime;
            if (musicAud.volume <= 0)
                fadingOutMusic = false;
        }
    }

    private (float, float) ValidScrollRange(Transform currentActive, int totalElementCount, int visibleCount)
    {
        float minScroll, maxScroll;
        float stepSize = 1 / (float)(totalElementCount - visibleCount);
        int moduleIndex = -1;
        for (int i = 0; i < currentActive.parent.childCount; i++)
        {
            if (currentActive.parent.GetChild(i) == currentActive)
                moduleIndex = i;
        }

        maxScroll = 1 - (moduleIndex + 1 - visibleCount) * stepSize;
        minScroll = maxScroll - stepSize * (visibleCount - 1);

        return (minScroll, maxScroll);
    }

    public void PickModule(int moduleIndex)
    {
        currentModule = moduleList[moduleIndex];
        termList = ELLEAPI.GetTermsFromModule(currentModule.moduleID);

        if (currentGameMode == GameMode.Quiz)
            StartGame();
        else
            EndlessMenu();
    }

    public void EndlessMenu()
    {
        int i;

        termEnabled = new bool[termList.Count];
        for (i = 0; i < termList.Count; i++)
            termEnabled[i] = true;

        for (i = termListUIParent.childCount - 1; i >= 0; i--)
            Destroy(termListUIParent.GetChild(i).gameObject);

        Toggle firstToggle = null;
        for (i = 0; i < termList.Count; i++)
        {
            MenuTerm term = Instantiate(termUIElement, termListUIParent).GetComponent<MenuTerm>();
            term.InitializeStuff(termList[i].back, this, i);
            if (i == 0) firstToggle = term.transform.GetComponent<Toggle>();
        }

        if (firstToggle != null)
        {
            Navigation n;

            n = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnRight = uncheckTermsButton,
                selectOnDown = firstToggle
            };
            checkTermsButton.navigation = n;

            n = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = checkTermsButton,
                selectOnRight = startButton,
                selectOnDown = firstToggle
            };
            uncheckTermsButton.navigation = n;

            n = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = uncheckTermsButton,
                selectOnDown = firstToggle
            };
            startButton.navigation = n;
        }

        chooseTermsMenu.SetActive(true);
        openingCTM = true;
        closingCTM = false;
        ctmIsOpen = true;

        for (i = 0; i < moduleListUIParent.childCount; i++)
        {
            moduleListUIParent.GetChild(i).GetComponent<MenuModule>().enabled = false;
            moduleListUIParent.GetChild(i).GetComponent<Button>().enabled = false;
        }
    }

    public void SetAllTerms(bool enabled)
    {
        for (int i = 0; i < termEnabled.Length; i++)
        {
            Toggle t = termListUIParent.GetChild(i).GetComponent<Toggle>();
            t.isOn = enabled;
        }
    }

    public void ToggleTerm(int termIndex)
    {
        termEnabled[termIndex] = !termEnabled[termIndex];
    }

    public void DisableStartMenu() 
    { 
        startMenu.SetActive(false); 
    }
    public void EnableEndMenu(int points, int attempts)
    { 
        endMenu.SetActive(true);
        scoreFractionText.text = points + "/" + attempts;
        scorePercentageText.text = termList.Count == 0 ? "-%" : Mathf.RoundToInt(100 * points / (float)attempts) + "%";
    }

    public void StartInGameMusic()
    {
        fadingOutMusic = false;
        musicAud.clip = inGameMusic;
        musicAud.volume = 1;
        musicAud.Play();
    }

    public void StartPostGameMusic()
    {
        fadingOutMusic = false;
        musicAud.clip = postGameMusic;
        musicAud.volume = 1;
        musicAud.Play();
    }
    public void FadeOutMusic()
    {
        fadingOutMusic = true;
    }

    public void StartGame()
    {
        if (inGame) return;

        closingCTM = true;
        openingCTM = false;
        ctmIsOpen = false;

        for (int i = 0; i < moduleListUIParent.childCount; i++)
            moduleListUIParent.GetChild(i).GetComponent<MenuModule>().enabled = false;

        try
        {
            onStartGame();
        }
        catch
        {
            Debug.LogError("There is no callback for the menu. Assign a function to the " +
                "onStartGame delegate to kick off the game!");
        }
    }
}

public enum GameMode { Quiz, Endless }