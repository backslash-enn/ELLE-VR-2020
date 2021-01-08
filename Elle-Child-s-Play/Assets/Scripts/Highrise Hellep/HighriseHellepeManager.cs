using Obi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighriseHellepeManager : MonoBehaviour
{
    public Transform pointerController;
    public Transform hosePointer;

    private int currentRound = 0;

    private int sessionID;
    private GameMode currentGameMode = GameMode.Quiz;
    private List<Term> termList;
    private List<Term> termsBag;
    private List<Term> dummyTermList;
    private List<string> correctTerms;
    private int score = 0;
    private int realScore = 0;

    private bool tagCatValid, genderCatValid, posCatValid, moduleNameCatValid;
    private bool hasNouns, hasVerbs, hasAdjectives, hasAdverbs;

    public ObiEmitter oe;

    public Balcony[] balconies;

    public TMP_Text ledSign;

    private AudioSource aud, hoseAud;
    public AudioSource fireAud;

    public Fader blackFader;

    public GameMenu menu;

    private bool movingTruckOut, movingTruckIn;
    public Transform truck;
    public Transform[] truckTires;
    private AudioSource truckAud;
    public AnimationCurve truckMoov;
    public float t = 0;

    public GameObject hoseAndWater, nozzle, remoteLeft, remoteRight;
    public Transform domHand, nondomHand, leftHand, rightHand;
    public Animator leftHandAnim, rightHandAnim;

    public GameObject littlePoof;
    public AudioClip poofSound, roundSound, correctSound, incorrectSound;
    public GameObject yellowScreen, greenScreen, redScreen;
    public Image c, x;
    private bool correctAnimation, incorrectAnimation;
    public Transform fireHYDRANT, fireHYDRANTanchor;

    void Start()
    {
        blackFader.Fade(false, .5f);

        aud = GetComponent<AudioSource>();
        truckAud = truck.GetComponent<AudioSource>();
        hoseAud = nozzle.GetComponent<AudioSource>();

        if (ELLEAPI.rightHanded == false)
        {
            fireHYDRANT.transform.position = Vector3.Scale(fireHYDRANT.transform.position, new Vector3(-1, 1, 1));
            rightHandAnim.SetBool("OffHand", true);
        }
        else
            leftHandAnim.SetBool("OffHand", true);

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

        if (!menu.inGame) return;

        if (Physics.Raycast(pointerController.position, pointerController.forward, out RaycastHit hit))
        {
            if(hit.transform.name == "Building Collider" || hit.transform.name == "Balcony")
            {
                hosePointer.gameObject.SetActive(true);
                hosePointer.position = hit.point;
            }
            if (hit.transform.name == "Balcony" && ((ELLEAPI.rightHanded && VRInput.rightTriggerDigital) || !ELLEAPI.rightHanded && VRInput.leftTriggerDigital))
                hit.transform.GetComponent<Balcony>().LowerLife();
        }

        oe.speed = Mathf.Lerp(0, 15, ELLEAPI.rightHanded ? VRInput.rightTrigger : VRInput.leftTrigger);
        hoseAud.volume = Mathf.Lerp(0, 0.4f, ELLEAPI.rightHanded ? VRInput.rightTrigger : VRInput.leftTrigger);
        
    }

    public void MoveTruck()
    {
        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        int i;

        currentGameMode = menu.currentGameMode;
        termList = menu.termList;

        sessionID = ELLEAPI.StartSession(menu.currentModule.moduleID, currentGameMode == GameMode.Endless);

        // remove
        if (currentGameMode == GameMode.Endless)
        {
            for (i = termsBag.Count - 1; i >= 0; i--)
            {
                if (menu.termEnabled[i] == false)
                    termsBag.RemoveAt(i);
            }
        }

        if (currentGameMode == GameMode.Endless)
        {
            for (i = termList.Count - 1; i >= 0; i--)
            {
                if (menu.termEnabled[i] == false)
                    termList.RemoveAt(i);
            }
        }

        FillTermBag();

        truckAud.Play();
        movingTruckOut = true;

        yield return new WaitForSeconds(3.5f);

        movingTruckOut = false;

        yield return new WaitForSeconds(1.5f);

        hoseAndWater.SetActive(true);
        nozzle.SetActive(true);
        remoteLeft.SetActive(false);
        remoteRight.SetActive(false);
        leftHandAnim.SetBool("InGame", true);
        rightHandAnim.SetBool("InGame", true);

        // For some reason you have to move the hose late. If you move it in start it wont budge
        fireHYDRANTanchor.position = fireHYDRANT.position;

        domHand.parent = ELLEAPI.rightHanded ? rightHand : leftHand;
        domHand.localPosition = new Vector3(0.06f, 0, 0) * (ELLEAPI.rightHanded ? 1 : -0.75f);
        domHand.localRotation = Quaternion.identity;
        nondomHand.parent = ELLEAPI.rightHanded ? leftHand : rightHand;
        nondomHand.localPosition = nondomHand.localEulerAngles = Vector3.zero;

        if(!ELLEAPI.rightHanded)
        {
            Vector3 r = nozzle.transform.localEulerAngles;
            Vector3 p = nozzle.transform.localPosition;
            nozzle.transform.parent = leftHand;
            nozzle.transform.localEulerAngles = r + new Vector3(0, 0, 180);
            nozzle.transform.localPosition = p;
        }

        Instantiate(littlePoof, rightHand.transform.position, Quaternion.identity);
        Instantiate(littlePoof, leftHand.transform.position, Quaternion.identity);
        aud.clip = poofSound;
        aud.Play();

        yield return new WaitForSeconds(3);

        menu.StartInGameMusic();

        // Not all catergory types are valid, so we figure that stuff out here
        int gotATagCount = 0;
        bool hasAMale = false, hasAFemale = false;

        for(i = 0; i < termsBag.Count; i++)
        {
            if(termsBag[i].tags != null && termsBag[i].tags.Length > 0)
                gotATagCount++;
            if (termsBag[i].gender.ToLower() == "m")
                hasAMale = true;
            else if (termsBag[i].gender.ToLower() == "f")
                hasAFemale = true;

            if (termsBag[i].type.ToLower() == "nn")
                hasNouns = true;
            else if (termsBag[i].type.ToLower() == "vr")
                hasVerbs = true;
            else if (termsBag[i].type.ToLower() == "aj")
                hasAdjectives = true;
            else if (termsBag[i].type.ToLower() == "av")
                hasAdverbs = true;
        }

        // We only use the tags catergory if at least 30% of the terms in the deck have at least 1 tag
        tagCatValid = (((float)gotATagCount / termsBag.Count) >= 0.3f);
        // Only use gender catergory if there is at least one male and one female in the deck
        genderCatValid = (hasAMale && hasAFemale);
        // Only use part of speech module if at least 2 different parts of speech are in the module
        int posCount = 0;
        if (hasNouns) posCount++;
        if (hasVerbs) posCount++;
        if (hasAdjectives) posCount++;
        if (hasAdverbs) posCount++;
        posCatValid = (posCount >= 2);
        // Only use module name catergory if the words "chapter", "final", "section","review", "test", or "midterm" are not in the module name
        moduleNameCatValid = false;
        if (!(menu.currentModule.name.ToLower().Contains("chapter")
                                || menu.currentModule.name.ToLower().Contains("final")
                                || menu.currentModule.name.ToLower().Contains("section")
                                || menu.currentModule.name.ToLower().Contains("review")
                                || menu.currentModule.name.ToLower().Contains("test")
                                || menu.currentModule.name.ToLower().Contains("midterm"))) {
            dummyTermList = new List<Term>();
            for (i = 0; i < menu.moduleList.Count; i++)
            {
                if (menu.moduleList[i].moduleID != menu.currentModule.moduleID &&
                    menu.moduleList[i].language != menu.currentModule.language)
                {
                    List<Term> temp = ELLEAPI.GetTermsFromModule(menu.moduleList[i].moduleID);
                    for(int j = 0; j < temp.Count; j++)
                        dummyTermList.Add(temp[j]);                    
                }
            }

            moduleNameCatValid = (dummyTermList.Count >= 4);
        }

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

        if (currentRound <= 8 || currentGameMode == GameMode.Endless)
        {
            int chosenThing = 0;
            while (chosenThing != 5)
            {
                chosenThing = Random.Range(0, 5);
                if (chosenThing == 0)
                {
                    IndividualsCatergory();
                    chosenThing = 5;
                }
                else if (chosenThing == 1)
                {
                    if (tagCatValid)
                    {
                        TagCatergory();
                        chosenThing = 5;
                    }
                }
                else if (chosenThing == 2)
                {
                    if (genderCatValid)
                    {
                        GenderCatergory();
                        chosenThing = 5;
                    }
                }
                else if (chosenThing == 3)
                {
                    if (posCatValid)
                    {
                        PartOfSpeechCatergory();
                        chosenThing = 5;
                    }
                }
                else if (chosenThing == 4)
                {
                    if (moduleNameCatValid)
                    {
                        ModuleNameCatergory();
                        chosenThing = 5;
                    }
                }
            }
        }
        else
        {
            StartCoroutine(FinishGame());
        }

        fireAud.time = 0;
        fireAud.Play();

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
        // Spawn anywhere between 4 and 16 balconies...
        int numBalconies = Random.Range(4, 17);
        // That is, if we even have enough terms
        numBalconies = Mathf.Min(numBalconies, termsBag.Count);

        // We choose our terms to show on the balconies here. To choose, simply choose a
        // random index, and check if it's taken. If it is, just linear probe until you
        // find an open index
        int[] chosenTermIndices = GenerateRandomArray(numBalconies, termsBag.Count);

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

    private void TagCatergory()
    {
        // Spawn anywhere between 4 and 16 balconies...
        int numBalconies = Random.Range(4, 17);
        // That is, if we even have enough terms
        numBalconies = Mathf.Min(numBalconies, termsBag.Count);

        // We choose our terms to show on the balconies here. To choose, simply choose a
        // random index, and check if it's taken. If it is, just linear probe until you
        // find an open index
        int[] chosenTermIndices = new int[0];
        string chosenTag = "";

        // Pick the tag and the terms
        while (chosenTag == "") {
            chosenTermIndices = GenerateRandomArray(numBalconies, termsBag.Count);
            for (int i = 0; i < numBalconies; i++) {
                if (termsBag[chosenTermIndices[i]].tags.Length != 0)
                {
                    int ctIndex = Random.Range(0, termsBag[chosenTermIndices[i]].tags.Length);
                    chosenTag = termsBag[chosenTermIndices[i]].tags[ctIndex].ToLower();
                    break;
                }
            }
        }

        // Know which terms match our chosen tag
        correctTerms.Clear();
        for (int i = 0; i < numBalconies; i++)
        {
            for (int j = 0; j < termsBag[chosenTermIndices[i]].tags.Length; j++)
            {
                if (termsBag[chosenTermIndices[i]].tags[j].ToLower() == chosenTag)
                {
                    correctTerms.Add(termsBag[chosenTermIndices[i]].back);
                    break;
                }
            }
        }

        // Now that we've grabbed a random number of random terms, we now have to apply it to
        // some random balconies - the same number of balconies as the number of terms we have
        ledSign.text = chosenTag;
        int[] chosenBalconies = GenerateRandomArray(numBalconies, 16);
        for (int i = 0; i < numBalconies; i++)
            balconies[chosenBalconies[i]].Activate(termsBag[chosenTermIndices[i]]);
    }

    private void GenderCatergory()
    {
        // Spawn anywhere between 4 and 16 balconies...
        int numBalconies = Random.Range(4, 17);
        // That is, if we even have enough terms
        numBalconies = Mathf.Min(numBalconies, termsBag.Count);

        // We choose our terms to show on the balconies here. To choose, simply choose a
        // random index, and check if it's taken. If it is, just linear probe until you
        // find an open index
        int[] chosenTermIndices = new int[0];
        bool choseGender = false, isMale = false;

        // Pick the tag and the terms
        while (choseGender == false)
        {
            chosenTermIndices = GenerateRandomArray(numBalconies, termsBag.Count);
            for (int i = 0; i < numBalconies; i++)
            {
                if (!string.IsNullOrEmpty(termsBag[chosenTermIndices[i]].gender) && termsBag[chosenTermIndices[i]].gender.ToLower() != "n")
                {
                    isMale = (termsBag[chosenTermIndices[i]].gender.ToLower() == "m");
                    choseGender = true;
                    break;
                }
            }
        }

        // Know which terms match our chosen tag
        correctTerms.Clear();
        for (int i = 0; i < numBalconies; i++)
        {
            if ((termsBag[chosenTermIndices[i]].gender.ToLower() == "m" && isMale) || (termsBag[chosenTermIndices[i]].gender.ToLower() == "f" && !isMale))
                correctTerms.Add(termsBag[chosenTermIndices[i]].back);
        }

        // Now that we've grabbed a random number of random terms, we now have to apply it to
        // some random balconies - the same number of balconies as the number of terms we have
        ledSign.text = isMale ? "Masculine terms" : "Feminine terms";
        int[] chosenBalconies = GenerateRandomArray(numBalconies, 16);
        for (int i = 0; i < numBalconies; i++)
            balconies[chosenBalconies[i]].Activate(termsBag[chosenTermIndices[i]]);
    }

    private void PartOfSpeechCatergory()
    {
        // Spawn anywhere between 4 and 16 balconies...
        int numBalconies = Random.Range(4, 17);
        // That is, if we even have enough terms
        numBalconies = Mathf.Min(numBalconies, termsBag.Count);

        // We choose our terms to show on the balconies here. To choose, simply choose a
        // random index, and check if it's taken. If it is, just linear probe until you
        // find an open index
        int[] chosenTermIndices = new int[0];
        string chosenPOS = "";

        // Pick the tag and the terms
        while (chosenPOS == "")
        {
            chosenTermIndices = GenerateRandomArray(numBalconies, termsBag.Count);
            for (int i = 0; i < numBalconies; i++)
            {
                if (!string.IsNullOrEmpty(termsBag[chosenTermIndices[i]].type))
                {
                    chosenPOS = termsBag[chosenTermIndices[i]].type.ToLower();
                    break;
                }
            }
        }

        // Know which terms match our chosen tag
        correctTerms.Clear();
        for (int i = 0; i < numBalconies; i++)
        {
            if (termsBag[chosenTermIndices[i]].type.ToLower() == chosenPOS)
                correctTerms.Add(termsBag[chosenTermIndices[i]].back);
        }

        // Now that we've grabbed a random number of random terms, we now have to apply it to
        // some random balconies - the same number of balconies as the number of terms we have
        string t = "";
        if      (chosenPOS == "nn") t = "Noun terms";
        else if (chosenPOS == "vr") t = "Verb terms";
        else if (chosenPOS == "av") t = "Adjverb terms";
        else if (chosenPOS == "aj") t = "Adjective terms";
        ledSign.text = t;
        int[] chosenBalconies = GenerateRandomArray(numBalconies, 16);
        for (int i = 0; i < numBalconies; i++)
            balconies[chosenBalconies[i]].Activate(termsBag[chosenTermIndices[i]]);
    }

    private void ModuleNameCatergory()
    {
        // Spawn anywhere between 4 and 8 balconies from this module...
        int numBalconiesThisModule = Random.Range(4, 9);
        // That is, if we even have enough terms
        numBalconiesThisModule = Mathf.Min(numBalconiesThisModule, termsBag.Count);
        // Spawn anywhere between 4 and 8 balconies from this module...
        int numBalconiesDummyModule = Random.Range(4, 9);
        // That is, if we even have enough terms
        numBalconiesDummyModule = Mathf.Min(numBalconiesDummyModule, dummyTermList.Count);

        // We choose our terms to show on the balconies here. To choose, simply choose a
        // random index, and check if it's taken. If it is, just linear probe until you
        // find an open index
        int[] chosenTermIndicesThisModule = GenerateRandomArray(numBalconiesThisModule, termsBag.Count);
        int[] chosenTermIndicesDummyModule = GenerateRandomArray(numBalconiesDummyModule, dummyTermList.Count);

        ledSign.text = menu.currentModule.name;

        // Now that we've grabbed a random number of random terms, we now have to apply it to
        // some random balconies - the same number of balconies as the number of terms we have
        int[] chosenBalconies = GenerateRandomArray(numBalconiesThisModule + numBalconiesDummyModule, 16);
        correctTerms.Clear();
        for (int i = 0; i < numBalconiesThisModule + numBalconiesDummyModule; i++)
        {
            if (i < numBalconiesThisModule)
            {
                correctTerms.Add(termsBag[chosenTermIndicesThisModule[i]].back);
                balconies[chosenBalconies[i]].Activate(termsBag[chosenTermIndicesThisModule[i]]);
            }
            else
                balconies[chosenBalconies[i]].Activate(dummyTermList[chosenTermIndicesDummyModule[i - numBalconiesThisModule]]);
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
                retVal[i] %= numElementsToPickFrom;
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
                for(int j = 0; j < termsBag.Count; j++)
                {
                    if(termsBag[j].back == correctTerms[i])
                    {
                        ELLEAPI.LogAnswer(sessionID, termsBag[j], true, currentGameMode == GameMode.Endless);
                        if(termsBag[j].audio != null)
                            AudioSource.PlayClipAtPoint(termsBag[j].audio, new Vector3(0, 0, 5));
                        break;
                    }
                }
                correctTerms.RemoveAt(i);
                realScore++;
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
            for(int i = 0; i < correctTerms.Count; i++)
            {
                for (int j = 0; j < termsBag.Count; j++)
                {
                    if (termsBag[j].back == correctTerms[i])
                    {
                        ELLEAPI.LogAnswer(sessionID, termsBag[j], false, currentGameMode == GameMode.Endless);
                        break;
                    }
                }
            }
        }

        aud.Play();
        fireAud.Stop();
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
        fireAud.Stop();
        
        Instantiate(littlePoof, leftHand.transform.position, Quaternion.identity);
        Instantiate(littlePoof, rightHand.transform.position, Quaternion.identity);

        hoseAndWater.SetActive(false);
        nozzle.SetActive(false);
        remoteLeft.SetActive(true);
        remoteRight.SetActive(true);
        leftHandAnim.SetBool("InGame", false);
        rightHandAnim.SetBool("InGame", false);

        menu.EndGame(score, 8);
        ELLEAPI.EndSession(sessionID, realScore);

        yield return new WaitForSeconds(2);

        t = 0;
        truckAud.Play();
        movingTruckIn = true;
        menu.StartPostGameMusic();

        yield return new WaitForSeconds(5f);
        movingTruckIn = false;

        yield break;
    }
}