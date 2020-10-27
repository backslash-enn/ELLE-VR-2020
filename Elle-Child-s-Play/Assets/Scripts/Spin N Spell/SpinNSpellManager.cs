using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpinNSpellManager : MonoBehaviour
{
    private AudioSource aud;
    public AudioSource projectorAud, musicAud;
    public EventSystem e;

    public AudioClip inGameMusic, endGameMusic, postGameMusic;

    private bool inGame = false;
    private bool finishedGame = false;
    private bool dontLeaveTooEarlyFlag;
    private List<Module> moduleList;
    private Module currentModule;
    private int sessionID;
    private GameMode currentGameMode = GameMode.Quiz;

    public GameObject introCanvas, finishCanvas;
    public TMP_Text scoreFractionText, scorePercentageText;
    public Transform moduleListUIParent;
    public GameObject moduleUIElement;
    public Color[] moduleElementColors;

    private bool raiseProjectorScreen, lowerProjectorScreen;
    public Transform projectorScreen;
    public float t;
    public AnimationCurve projectorMoveCurve;

    public TMP_Text gameModeText;
    public Image gameModeCover, gameModeUncover;
    private bool gameModeCoverLockedOut;

    public SpinNSpellHand leftHand, rightHand;
    public GameObject blocksParent;
    public GameObject littlePoof, bigPoof;
    public Transform cubbyBasePosition;
    public GameObject cubbyBottom;

    private List<Term> termList;
    private List<Term> termsBag;
    private bool[] termEnabled;

    public GameObject cubbyRowPrefab;
    public List<CubbyRow> cubbyRows;
    public Transform newCubbyRowPos, cubbyRowsParent;
    public GameObject successfulRowEffect, unsuccessfulRowEffect;

    public Transform[] frames;

    public ScrollRect modulesSR, termsSR;
    public Button checkTermsButton, uncheckTermsButton, startButton;

    private int attempts = 0;
    private int score = 0;

    public AudioClip correctSound, menuMoveSound, switchModeSound, poofSound;

    public GameObject chooseTermsMenu, ctmStartButtom;
    private CanvasGroup chooseTermsMenuCG;
    private bool ctmIsOpen, openingCTM, closingCTM;
    private Vector3 ctmPosition;
    public Transform termListUIParent;
    public GameObject termUIElement;

    private WaitForSeconds w;

    public Fader blackFader;

    void Start()
    {
        blackFader.Fade(false, .5f);

        aud = GetComponent<AudioSource>();
        termsBag = new List<Term>();
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

            if (i == 0) e.SetSelectedGameObject(g);
        }

        w = new WaitForSeconds(0.08f);
    }

    public void PickModule(int moduleIndex)
    {
        currentModule = moduleList[moduleIndex];
        termList = ELLEAPI.GetTermsFromModule(currentModule.moduleID);

        if (currentGameMode == GameMode.Quiz)
            RaiseProjector();
        else
            EndlessMenu();
    }

    public void RaiseProjector() {
        if (inGame) return;

        raiseProjectorScreen = true;
        t = 0;
        projectorAud.Play();
        inGame = true;
        PauseMenu.canPause = true;

        closingCTM = true;
        openingCTM = false;
        ctmIsOpen = false;

        for (int i = 0; i < moduleListUIParent.childCount; i++)
            moduleListUIParent.GetChild(i).GetComponent<MenuModule>().enabled = false;
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
            term.InitializeStuff(termList[i].front, this, i);
            if (i == 0) firstToggle = term.transform.GetComponent<Toggle>();
        } 

        if(firstToggle != null)
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
            if(t.isOn != enabled)
            t.isOn = enabled;
        }
    }

    public void ToggleTerm(int termIndex)
    {
        termEnabled[termIndex] = !termEnabled[termIndex];
    }

    void Update()
    {
        if (!inGame && !ctmIsOpen && !raiseProjectorScreen && !lowerProjectorScreen && !dontLeaveTooEarlyFlag && (VRInput.bDown || VRInput.yDown))
            SceneManager.LoadScene("Hubworld");

        if (finishedGame && !raiseProjectorScreen && !lowerProjectorScreen && !dontLeaveTooEarlyFlag && (VRInput.aDown || VRInput.xDown))
            SceneManager.LoadScene("SpinNSpell");

        if (!inGame)
        {
            GameObject currentActive = e.currentSelectedGameObject;

            // If somehow no option is highlighted in the menu, reset it to the first option
            if (currentActive == null)
            {
                if (ctmIsOpen)
                    e.SetSelectedGameObject(startButton.gameObject);
                else if (moduleListUIParent.childCount > 0)
                    e.SetSelectedGameObject(moduleListUIParent.GetChild(0).gameObject);
            }
            else if(ctmIsOpen)
            {
                if (e.currentSelectedGameObject.transform.parent == moduleListUIParent)
                    e.SetSelectedGameObject(startButton.gameObject);

                if(termList.Count > 4)
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

                        aud.clip = switchModeSound;
                        aud.Play();
                    }
                }
                else if (VRInput.rightTrigger + VRInput.leftTrigger < 0.05f)
                    gameModeCoverLockedOut = false;
            }

            if(VRInput.bDown || VRInput.yDown)
            {
                if(ctmIsOpen)
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
                        e.SetSelectedGameObject(moduleListUIParent.GetChild(0).gameObject);
                }
            }
        }

        if(openingCTM)
        {
            chooseTermsMenuCG.alpha = Mathf.Lerp(chooseTermsMenuCG.alpha, 1, 7 * Time.deltaTime);
            chooseTermsMenu.transform.position = Vector3.Lerp(chooseTermsMenu.transform.position, ctmPosition, 7 * Time.deltaTime);
            if(chooseTermsMenuCG.alpha > 0.999f)
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

        if (raiseProjectorScreen)
        {
            t += 0.3f * Time.deltaTime;
            projectorScreen.position = new Vector3(projectorScreen.position.x, projectorMoveCurve.Evaluate(t), projectorScreen.position.z);
            musicAud.volume -= 0.2f * Time.deltaTime; 
            if(t >= 1)
            {
                raiseProjectorScreen = false;
                introCanvas.SetActive(false);
                StartCoroutine(GetItStarted());
            }
        }

        if (lowerProjectorScreen)
        {
            t -= 0.3f * Time.deltaTime;
            projectorScreen.position = new Vector3(projectorScreen.position.x, projectorMoveCurve.Evaluate(t), projectorScreen.position.z);
            musicAud.volume -= 0.2f * Time.deltaTime;
            if (t <= 0)
            {
                lowerProjectorScreen = false;
                musicAud.clip = postGameMusic;
                musicAud.volume = 0.3f;
                musicAud.Play();
            }
        }
    }

    private (float, float) ValidScrollRange(Transform currentActive, int totalElementCount, int visibleCount)
    {
        float minScroll, maxScroll;
        float stepSize = 1 / (float)(totalElementCount - visibleCount);
        int moduleIndex = -1;
        for(int i = 0; i < currentActive.parent.childCount; i++)
        {
            if (currentActive.parent.GetChild(i) == currentActive)
                moduleIndex = i;
        }

        maxScroll = 1 - (moduleIndex + 1 - visibleCount) * stepSize;
        minScroll = maxScroll - stepSize * (visibleCount - 1);

        return (minScroll, maxScroll);
    }

    private IEnumerator GetItStarted()
    {
        sessionID = ELLEAPI.StartSession(currentModule.moduleID, currentGameMode == GameMode.Endless);

        if (currentGameMode == GameMode.Endless)
        {
            for (int i = termList.Count - 1; i >= 0; i--)
            {
                if (termEnabled[i] == false)
                    termList.RemoveAt(i);
            }
        }

        Instantiate(littlePoof, rightHand.transform.position, Quaternion.identity);
        Instantiate(littlePoof, leftHand.transform.position, Quaternion.identity);
        Instantiate(bigPoof, cubbyBasePosition);
        cubbyBottom.SetActive(true);
        leftHand.StartGame();
        rightHand.StartGame();
        aud.clip = poofSound;
        aud.Play();

        
        FillTermBag();

        yield return new WaitForSeconds(2);

        for (int i = 0; i < 6; i++)
        {
            if (i >= termsBag.Count) continue;
            cubbyRows.Add(Instantiate(cubbyRowPrefab, newCubbyRowPos.position, Quaternion.identity, cubbyRowsParent).GetComponent<CubbyRow>());
            cubbyRows[i].UpdatePosition(i);
            UpdateFrame(i, false, true);
            yield return new WaitForSeconds(.7f);

            // If this term happens to have an audio clip, play the clip and wait for it to finish before continuing
            if(termsBag[i].audio != null)
            {
                cubbyRows[i].PlaySound(termsBag[i].audio);
                yield return new WaitForSeconds(termsBag[i].audio.length + 0.4f);
            }
        }

        yield return new WaitForSeconds(1.5f);

        blocksParent.SetActive(true);
        musicAud.clip = inGameMusic;
        musicAud.volume = 0.3f;
        musicAud.Play();
        for (int i = 0; i < 6; i++)
        {
            if (i >= termsBag.Count) break;
            cubbyRows[i].timerOn = true;
        }    

        if(termList.Count == 0)
            StartCoroutine(FinishIt());
    }

    private void FillTermBag()
    {
        List<int> indicies = new List<int>();
        int currentIndex, i;

        for (i = 0; i < termList.Count; i++) 
            indicies.Add(i);

        while(indicies.Count > 0) 
        {
            currentIndex = Random.Range(0, indicies.Count);
            termsBag.Add(termList[indicies[currentIndex]]);
            indicies.RemoveAt(currentIndex);
        }

        if (currentGameMode == GameMode.Endless && termsBag.Count < 6 && termList.Count > 0)
            FillTermBag();
    }

    public void CheckIfCorrectWord(int position, char[] newWord, int[] accents)
    {
        for(int i = 0; i < accents.Length; i++)
        {
            switch(accents[i])
            {
                case 1:
                    if (newWord[i] == 'a')
                        newWord[i] = 'à';
                    if (newWord[i] == 'e')
                        newWord[i] = 'è';
                    if (newWord[i] == 'i')
                        newWord[i] = 'ì';
                    if (newWord[i] == 'o')
                        newWord[i] = 'ò';
                    if (newWord[i] == 'u')
                        newWord[i] = 'ù';
                    break;
                case 2:
                    if (newWord[i] == 'a')
                        newWord[i] = 'ǎ';
                    if (newWord[i] == 'c')
                        newWord[i] = 'č';
                    if (newWord[i] == 'e')
                        newWord[i] = 'ě';
                    if (newWord[i] == 'i')
                        newWord[i] = 'ǐ';
                    if (newWord[i] == 'o')
                        newWord[i] = 'ǒ';
                    if (newWord[i] == 's')
                        newWord[i] = 'š';
                    if (newWord[i] == 'u')
                        newWord[i] = 'ǔ';
                    if (newWord[i] == 'z')
                        newWord[i] = 'ž';
                    break;
                case 3:
                    if (newWord[i] == 'c')
                        newWord[i] = 'ç';
                    if (newWord[i] == 'e')
                        newWord[i] = 'ȩ';
                    if (newWord[i] == 's')
                        newWord[i] = 'ş';
                    if (newWord[i] == 't')
                        newWord[i] = 'ţ';
                    break;
                case 4:
                    if (newWord[i] == 'a')
                        newWord[i] = 'ã';
                    if (newWord[i] == 'e')
                        newWord[i] = 'ẽ';
                    if (newWord[i] == 'e')
                        newWord[i] = 'ẽ';
                    if (newWord[i] == 'n')
                        newWord[i] = 'ñ';
                    if (newWord[i] == 'o')
                        newWord[i] = 'õ'; 
                    if(newWord[i] == 'u')
                        newWord[i] = 'ũ';
                    break;
                case 5:
                    if (newWord[i] == 'a')
                        newWord[i] = 'â';
                    if (newWord[i] == 'e')
                        newWord[i] = 'ê';
                    if (newWord[i] == 'i')
                        newWord[i] = 'î';
                    if (newWord[i] == 'o')
                        newWord[i] = 'ô';
                    if (newWord[i] == 'u')
                        newWord[i] = 'û';
                    break;
                case 6:
                    if (newWord[i] == 'a')
                        newWord[i] = 'ä';
                    if (newWord[i] == 'e')
                        newWord[i] = 'ë';
                    if (newWord[i] == 'i')
                        newWord[i] = 'ï';
                    if (newWord[i] == 'o')
                        newWord[i] = 'ö';
                    if (newWord[i] == 'u')
                        newWord[i] = 'ü';
                    break;
                case 7:
                    if (newWord[i] == 'a')
                        newWord[i] = 'á';
                    if (newWord[i] == 'e')
                        newWord[i] = 'é';
                    if (newWord[i] == 'i')
                        newWord[i] = 'í';
                    if (newWord[i] == 'o')
                        newWord[i] = 'ó';
                    if (newWord[i] == 'u')
                        newWord[i] = 'ú';
                    if (newWord[i] == 'y')
                        newWord[i] = 'ý';
                    break;
                default:
                    continue;
            }
        }

        if (termsBag[position].front.ToLower() == new string(newWord).Trim())
            FinishedWord(position, true);
    }

    public void FinishedWord(int finishedPosition, bool correct)
    {
        Term term = termsBag[finishedPosition];

        Instantiate(correct ? successfulRowEffect : unsuccessfulRowEffect, cubbyRows[finishedPosition].transform.position, Quaternion.identity);
        Destroy(cubbyRows[finishedPosition].gameObject);
        cubbyRows.RemoveAt(finishedPosition);

        attempts++;


        if (correct) score++;
        ELLEAPI.LogAnswer(sessionID, term, correct, currentGameMode == GameMode.Endless);

        termsBag.RemoveAt(finishedPosition);

        if (termsBag.Count == 0)
        {
            UpdateFrame(0, true, false);
            StartCoroutine(FinishIt());
            return;
        }

        if (correct)
        {
            for (int i = 0; i < 5; i++)
            {
                if (i < termsBag.Count)
                    cubbyRows[i].AddTime();
            }
        }

        if (currentGameMode == GameMode.Endless && termsBag.Count < 6)
            FillTermBag();

        if (termsBag.Count >= 6)
        {
            cubbyRows.Add(Instantiate(cubbyRowPrefab, newCubbyRowPos.position, newCubbyRowPos.rotation, cubbyRowsParent).GetComponent<CubbyRow>());
            cubbyRows[cubbyRows.Count - 1].timerOn = true;
            int newRowIndex = cubbyRows.Count - 1;
            if (termsBag[newRowIndex].audio != null)
                cubbyRows[newRowIndex].PlaySound(termsBag[newRowIndex].audio);
        }

        for (int i = 0; i < 6; i++)
        {
            if (i >= termsBag.Count)
                UpdateFrame(i, true, false);
            else
            {
                cubbyRows[i].UpdatePosition(i);
                UpdateFrame(i, false, false);
            }
        }

        aud.clip = correctSound;
        aud.Play();
    }

    private void UpdateFrame(int index, bool isBlank, bool fancyAnimation)
    {
        Renderer imageFrame = frames[index].GetChild(0).GetComponent<Renderer>();
        imageFrame.material.SetTexture("_BaseMap", isBlank ? null : termsBag[index].image);

        TMP_Text nameFrame = frames[index].GetChild(1).GetChild(0).GetComponent<TMP_Text>();
        if (isBlank)
            nameFrame.text = "";
        else if (fancyAnimation)
            StartCoroutine(AnimateWordOnWall(termsBag[index].back, nameFrame));
        else
            nameFrame.text = termsBag[index].back;
    }


    private IEnumerator FinishIt()
    {
        dontLeaveTooEarlyFlag = true;
        inGame = false;
        finishedGame = true;
        PauseMenu.canPause = false;
        Instantiate(littlePoof, leftHand.transform.position, Quaternion.identity);
        Instantiate(littlePoof, rightHand.transform.position, Quaternion.identity);
        Instantiate(bigPoof, cubbyBasePosition);
        cubbyBottom.SetActive(false);

        leftHand.DropCurrentBlock();
        rightHand.DropCurrentBlock();
        leftHand.EndGame();
        rightHand.EndGame();

        aud.clip = switchModeSound;
        aud.Play();

        finishCanvas.SetActive(true);
        scoreFractionText.text = score + "/" + termList.Count;
        scorePercentageText.text = termList.Count == 0 ? "-%" : Mathf.RoundToInt(100 * score / (float)attempts) + "%";
        ELLEAPI.EndSession(sessionID, score);

        yield return new WaitForSeconds(2);

        lowerProjectorScreen = true;
        dontLeaveTooEarlyFlag = false;
        t = 1;
        projectorAud.Play();

        if (blocksParent.GetComponent<BlockCountManager>().frozenBlocks)
            blocksParent.GetComponent<BlockCountManager>().ToggleFrozenBlocks();
        blocksParent.SetActive(false);

        yield break;
    }

    private IEnumerator AnimateWordOnWall(string word, TMP_Text textField)
    {
        List<char> current = new List<char>();

        for(int i = 0; i < word.Length; i++)
        {
            current.Add(' ');
            // Show 3 random letters
            for (int j = 0; j < 3; j++)
            {
                int r = Random.Range(0, 52);
                if (r >= 26)  r += 6;
                current[i] = (char)(r + 'A');
                textField.text = new string(current.ToArray());
                yield return w;
            }

            //Show the real deal
            current[i] = word[i];
            textField.text = new string(current.ToArray());
            yield return w;
        }
    }
}

public enum GameMode { Quiz, Endless }