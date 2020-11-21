using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    private AudioSource aud;
    private Vector3 ctmPosition;
    private CanvasGroup chooseTermsMenuCG;
    public GameObject chooseTermsMenu, ctmStartButtom;

    public TMP_Text gameModeText;
    public Image gameModeCover, gameModeUncover;
    private bool gameModeCoverLockedOut;

    public Transform moduleListUIParent;
    public GameObject moduleUIElement;
    public Color[] moduleElementColors;
    public Button checkTermsButton, uncheckTermsButton, startButton;

    private bool ctmIsOpen, openingCTM, closingCTM;
    private List<Module> moduleList;
    private Module currentModule;
    private List<Term> termList;

    private GameMode currentGameMode = GameMode.Quiz;
    public ScrollRect modulesSR, termsSR;

    [HideInInspector]
    public bool inGame = false;
    [HideInInspector]
    public bool finishedGame = false;
    [HideInInspector]
    public bool goodToLeave = true;

    void Start()
    {
        chooseTermsMenuCG = chooseTermsMenu.GetComponent<CanvasGroup>();
        ctmPosition = new Vector3(0.14f, 1f, 1.6f);

        // Start in the main menu, where you choose a module
        moduleList = ELLEAPI.GetModuleList();
        for (int i = 0; i < moduleList.Count; i++)
        {
            GameObject g = Instantiate(moduleUIElement, moduleListUIParent);
            g.transform.GetChild(0).GetComponent<TMP_Text>().text = moduleList[i].name;
            g.transform.GetChild(1).GetComponent<TMP_Text>().text = "LEVEL: " + moduleList[i].complexity;
            g.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = moduleList[i].language.ToUpper();

            var b = g.GetComponent<Button>().colors;
            b.normalColor = moduleElementColors[i % 5];
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
            chooseTermsMenu.transform.position = Vector3.Lerp(chooseTermsMenu.transform.position, ctmPosition, 7 * Time.deltaTime);
            if (chooseTermsMenuCG.alpha > 0.999f)
            {
                chooseTermsMenuCG.alpha = 1;
                chooseTermsMenu.transform.position = ctmPosition;
                openingCTM = false;
            }
        }

        if (closingCTM)
        {
            chooseTermsMenuCG.alpha = Mathf.Lerp(chooseTermsMenuCG.alpha, 0, 7 * Time.deltaTime);
            chooseTermsMenu.transform.position = Vector3.Lerp(chooseTermsMenu.transform.position, ctmPosition + Vector3.left, 7 * Time.deltaTime);
            if (chooseTermsMenuCG.alpha < 0.001f)
            {
                chooseTermsMenuCG.alpha = 0;
                chooseTermsMenu.transform.position = ctmPosition + Vector3.left;
                closingCTM = false;
                chooseTermsMenu.SetActive(false);
            }
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
}

public enum GameMode { Quiz, Endless }