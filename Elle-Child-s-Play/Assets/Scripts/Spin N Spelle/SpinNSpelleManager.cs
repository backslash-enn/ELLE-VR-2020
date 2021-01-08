using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpinNSpelleManager : MonoBehaviour
{
    private AudioSource aud;
    public AudioSource projectorAud;

    public Transform[] leftyFlippers;

    private bool dontLeaveTooEarlyFlag;

    private bool raiseProjectorScreen, lowerProjectorScreen;
    public Transform projectorScreen;
    public float t;
    public AnimationCurve projectorMoveCurve;

    public SpinNSpelleHand leftHand, rightHand;
    public GameObject blocksParent;
    public GameObject littlePoof, bigPoof;
    public Transform cubbyBasePosition;
    public GameObject cubbyBottom;

    private int sessionID;
    private GameMode currentGameMode = GameMode.Quiz;
    private List<Term> termList;
    private List<Term> termsBag;

    public GameObject cubbyRowPrefab;
    public List<CubbyRow> cubbyRows;
    public Transform newCubbyRowPos, cubbyRowsParent;
    public GameObject successfulRowEffect, unsuccessfulRowEffect;

    public Transform[] frames;

    private int attempts = 0;
    private int score = 0;

    public AudioClip correctSound, switchModeSound, poofSound;

    private WaitForSeconds w;

    public Fader blackFader;

    public GameMenu menu;

    private void Awake()
    {
        if (ELLEAPI.rightHanded == false)
        {
            for (int i = 0; i < leftyFlippers.Length; i++)
                leftyFlippers[i].eulerAngles = new Vector3(0, 180, 0);
        }

        
        blackFader.Fade(false, .5f);

        menu.onStartGame = RaiseProjector;
    }

    void Start()
    {
        aud = GetComponent<AudioSource>();
        termsBag = new List<Term>();

        w = new WaitForSeconds(0.08f);
    }

    public void RaiseProjector()
    {
        raiseProjectorScreen = true;
        t = 0;
        projectorAud.Play();
    }

    void Update()
    {
        menu.goodToLeave = !raiseProjectorScreen && !lowerProjectorScreen && !dontLeaveTooEarlyFlag;

        if (raiseProjectorScreen)
        {
            t += 0.3f * Time.deltaTime;
            projectorScreen.position = new Vector3(projectorScreen.position.x, projectorMoveCurve.Evaluate(t), projectorScreen.position.z);
            if (t >= 1)
            {
                raiseProjectorScreen = false;
                StartCoroutine(GetItStarted());
            }
        }

        if (lowerProjectorScreen)
        {
            t -= 0.3f * Time.deltaTime;
            projectorScreen.position = new Vector3(projectorScreen.position.x, projectorMoveCurve.Evaluate(t), projectorScreen.position.z);
            if (t <= 0)
            {
                lowerProjectorScreen = false;
                menu.StartPostGameMusic();
            }
        }
    }

    private IEnumerator GetItStarted()
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
            if (termsBag[i].audio != null)
            {
                cubbyRows[i].PlaySound(termsBag[i].audio);
                yield return new WaitForSeconds(termsBag[i].audio.length + 0.4f);
            }
        }

        yield return new WaitForSeconds(1.5f);

        blocksParent.SetActive(true);
        menu.StartInGameMusic();
        for (int i = 0; i < 6; i++)
        {
            if (i >= termsBag.Count) break;
            cubbyRows[i].timerOn = true;
        }

        if (termList.Count == 0)
            StartCoroutine(FinishIt());
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

    public void CheckIfCorrectWord(int position, char[] newWord, int[] accents)
    {
        for (int i = 0; i < accents.Length; i++)
        {
            switch (accents[i])
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
                    if (newWord[i] == 'u')
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

        menu.EndGame(score, attempts);
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

        for (int i = 0; i < word.Length; i++)
        {
            current.Add(' ');
            // Show 3 random letters
            for (int j = 0; j < 3; j++)
            {
                int r = Random.Range(0, 52);
                if (r >= 26) r += 6;
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