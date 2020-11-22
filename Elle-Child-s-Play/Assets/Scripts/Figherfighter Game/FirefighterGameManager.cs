using Obi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirefighterGameManager : MonoBehaviour
{
    public Transform pointerController;
    public Transform hosePointer;

    private int currentRound = 0;

    private int sessionID;
    private GameMode currentGameMode = GameMode.Quiz;
    private List<Term> termList;
    private List<Term> termsBag;
    private List<string> correctTerms;
    private int score = 0;

    public ObiEmitter oe;

    public Balcony[] balconies;

    public TMP_Text ledSign;

    private AudioSource aud;

    public Fader blackFader;

    public GameMenu menu;

    private bool movingTruckOut, movingTruckIn;
    public Transform truck;
    public Transform[] truckTires;
    private AudioSource truckAud;
    public AnimationCurve truckMoov;
    public float t = 0;

    public GameObject hoseAndWater, nozzle;
    public Transform domHand, nondomHand, leftHand, rightHand;

    public GameObject littlePoof;
    public AudioClip poofSound, roundSound, correctSound, incorrectSound;
    public GameObject yellowScreen, greenScreen, redScreen;
    public Image c, x;
    private bool correctAnimation, incorrectAnimation;
    

    void Start()
    {
        blackFader.Fade(false, .5f);

        aud = GetComponent<AudioSource>();
        truckAud = truck.GetComponent<AudioSource>();

        menu.onStartGame = MoveTruck;

        termsBag = new List<Term>();
        correctTerms = new List<string>();
    }

    void Update()
    {
        hosePointer.gameObject.SetActive(false);

        if(movingTruckOut)
        {
            truck.position += -Vector3.right * truckMoov.Evaluate(t*.25f) * 7.5f * Time.deltaTime;
            t += Time.deltaTime;
            foreach (Transform tire in truckTires)
                tire.Rotate(Vector3.right * 4000 * Time.deltaTime * truckMoov.Evaluate(t * .4f), Space.Self);
        }
        if (movingTruckIn)
        {
            truck.position += Vector3.right * truckMoov.Evaluate(t * .25f) * 7.5f * Time.deltaTime;
            t += Time.deltaTime;
            foreach (Transform tire in truckTires)
                tire.Rotate(Vector3.right * -4000 * Time.deltaTime * truckMoov.Evaluate(t * .4f), Space.Self);
        }
        if(correctAnimation)
        {
            c.transform.localScale += Vector3.one * .2f * Time.deltaTime;
            c.fillAmount += 1.5f * Time.deltaTime;
        }
        if (incorrectAnimation)
        {
            x.transform.localScale += Vector3.one * .2f * Time.deltaTime;
            x.fillAmount += 1.5f * Time.deltaTime;
        }

        if (Physics.Raycast(pointerController.position, pointerController.forward, out RaycastHit hit))
        {
            if(hit.transform.name == "Building Collider" || hit.transform.name == "Balcony")
            {
                hosePointer.gameObject.SetActive(true);
                hosePointer.position = hit.point;
            }
            if (hit.transform.name == "Balcony" && VRInput.rightTriggerDigital)
                hit.transform.GetComponent<Balcony>().LowerLife();
        }

        oe.speed = Mathf.Lerp(0, 15, VRInput.rightTrigger);
    }

    public void MoveTruck()
    {
        menu.inGame = true;
        PauseMenu.canPause = true;
        menu.DisableStartMenu();
        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        currentGameMode = menu.currentGameMode;
        termList = menu.termList;

        sessionID = ELLEAPI.StartSession(menu.currentModule.moduleID, currentGameMode == GameMode.Endless);

        if (currentGameMode == GameMode.Endless)
        {
            for (int i = termList.Count - 1; i >= 0; i--)
            {
                if (menu.termEnabled[i] == false)
                    termList.RemoveAt(i);
            }
        }

        FillTermBag();

        truckAud.Play();
        menu.FadeOutMusic();
        movingTruckOut = true;

        yield return new WaitForSeconds(3.5f);

        movingTruckOut = false;

        yield return new WaitForSeconds(1.5f);

        hoseAndWater.SetActive(true);
        nozzle.SetActive(true);
        domHand.parent = rightHand;
        nondomHand.parent = leftHand;
        domHand.localPosition = domHand.localEulerAngles = Vector3.zero;
        nondomHand.localPosition = nondomHand.localEulerAngles = Vector3.zero;

        Instantiate(littlePoof, rightHand.transform.position, Quaternion.identity);
        Instantiate(littlePoof, leftHand.transform.position, Quaternion.identity);
        aud.clip = poofSound;
        aud.Play();

        yield return new WaitForSeconds(3);

        menu.StartInGameMusic();

        if (termList.Count < 4)
            StartCoroutine(FinishGame());
        else
            DoRound();
    }

    private void FillTermBag()
    {
        List<int> indicies = new List<int>();
        int currentIndex, i;

        for (i = 0; i < termList.Count; i++)
            indicies.Add(i);

        while (indicies.Count > 0)
        {
            currentIndex = Random.Range(0, indicies.Count);
            termsBag.Add(termList[indicies[currentIndex]]);
            indicies.RemoveAt(currentIndex);
        }

        if (currentGameMode == GameMode.Endless && termsBag.Count < 6 && termList.Count > 0)
            FillTermBag();
    }

    private void DoRound()
    {
        StartCoroutine(DoRoundCoroutine());
    }

    private IEnumerator DoRoundCoroutine()
    {
        currentRound++;
        ledSign.text = "";
        

        yield return new WaitForSeconds(1.5f);

        correctAnimation = incorrectAnimation = false;
        greenScreen.SetActive(false);
        redScreen.SetActive(false);

        aud.clip = roundSound;
        aud.Play();

        if (currentRound <= 8)
        {
            IndividualsCatergory();
        }
        else
        {
            StartCoroutine(FinishGame());
        }

        yellowScreen.SetActive(true);
        yield return new WaitForSeconds(.05f);
        yellowScreen.SetActive(false);
        yield return new WaitForSeconds(.05f);
        yellowScreen.SetActive(true);
        yield return new WaitForSeconds(.05f);
        yellowScreen.SetActive(false);
    }

    private void IndividualsCatergory()
    {
        // Spawn anywhere between 6 and 16 balconies...
        int numBalconies = Random.Range(6, 17);
        // That is, if we even have enough terms
        numBalconies = Mathf.Min(numBalconies, termsBag.Count);

        int[] chosenTermIndices = GenerateRandomArray(numBalconies, termsBag.Count);
        // We choose our terms to show on the balconies here. To choose, simply choose a
        // random index, and check if it's taken. If it is, just linear probe until you
        // find an open index
        

        // Now that we've grabbed a random number of random terms, we now have to apply it to
        // some random balconies - the same number of balconies as the number of terms we have
        int[] chosenBalconies = GenerateRandomArray(numBalconies, 16);
        correctTerms.Clear();
        for (int i = 0; i < numBalconies; i++)
        {
            if (i < 3)
            {
                ledSign.text += termsBag[chosenTermIndices[i]].back + "\n";
                correctTerms.Add(termsBag[chosenTermIndices[i]].back);
            }
            balconies[chosenBalconies[i]].Activate(termsBag[chosenTermIndices[i]]);
        }
    }

    public int[] GenerateRandomArray(int numElementsInResult, int numElementsToPickFrom)
    {
        int[] retVal = new int[numElementsInResult];
        for (int i = 0; i < numElementsInResult; i++)
        {
            retVal[i] = Random.Range(0, numElementsToPickFrom);
            bool open = true;
            for (int j = 0; j < i; j++)
            {
                if (retVal[j] == retVal[i])
                    open = false;
            }
            while (!open)
            {
                retVal[i]++;
                retVal[i] %= termsBag.Count;
                open = true;
                for (int j = 0; j < i; j++)
                {
                    if (retVal[j] == retVal[i])
                        open = false;
                }
            }
        }

        return retVal;
    }

    public void CheckIfCorrect(string termPutOut)
    {
        bool putOutCorrectTerm = false;

        for (int i = 0; i < correctTerms.Count; i++)
        {
            if (correctTerms[i] == termPutOut)
            {
                putOutCorrectTerm = true;
                correctTerms.RemoveAt(i);
                break;
            }
        }

        if (putOutCorrectTerm && correctTerms.Count > 0)
            return;

        if (putOutCorrectTerm)
        {
            score++;
            aud.clip = correctSound;
            greenScreen.SetActive(true);
            c.transform.localScale = Vector3.one;
            c.fillAmount = 0;
            correctAnimation = true;
        }
        else
        {
            aud.clip = incorrectSound;
            redScreen.SetActive(true);
            x.transform.localScale = Vector3.one;
            x.fillAmount = 0;
            incorrectAnimation = true;
        }

        aud.Play();
        DeactivateAllBalconies();
        DoRound();
    }

    private void DeactivateAllBalconies()
    {
        for (int i = 0; i < 16; i++)
            balconies[i].Deactivate(false);
    }

    private IEnumerator FinishGame()
    {
        //dontLeaveTooEarlyFlag = true;
        menu.inGame = false;
        menu.finishedGame = true;
        PauseMenu.canPause = false;
        Instantiate(littlePoof, leftHand.transform.position, Quaternion.identity);
        Instantiate(littlePoof, rightHand.transform.position, Quaternion.identity);

        hoseAndWater.SetActive(false);
        nozzle.SetActive(false);

        //aud.clip = switchModeSound;
        //aud.Play();

        menu.FadeOutMusic();
        ELLEAPI.EndSession(sessionID, score);

        yield return new WaitForSeconds(2);

        t = 0;
        truckAud.Play();
        movingTruckIn = true;
        //dontLeaveTooEarlyFlag = false;
        //projectorAud.Play();
        menu.StartPostGameMusic();
        menu.EnableEndMenu(score, 8);

        yield return new WaitForSeconds(5f);
        movingTruckIn = false;

        yield break;
    }
}
