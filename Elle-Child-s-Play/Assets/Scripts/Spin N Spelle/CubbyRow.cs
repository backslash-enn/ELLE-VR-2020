using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CubbyRow : MonoBehaviour
{
    AudioSource aud;
    private int position;
    private char[] currentWord;
    private int[] currentWordAccents;
    private Transform[] positions;
    private readonly float moveTime = 0.12f;
    private Vector3 ref1;
    private bool move;
    private WaitForSeconds w;
    private SpinNSpelleManager manager;
    private Transform[] frames;

    public Renderer imageFrame;
    public Image timerImage;
    public bool timerOn;
    private const float maxTime = 180; // in seconds
    private float currentTime;

    private void Awake()
    {
        int i;

        currentTime = maxTime;

        currentWord = new char[10];
        currentWordAccents = new int[10];
        for (i = 0; i < 10; i++)
            currentWord[i] = ' ';

        manager = GameObject.Find("Manager").GetComponent<SpinNSpelleManager>();

        Transform posParent = GameObject.Find("Row Positions").transform;
        positions = new Transform[6];
        for (i = 0; i < 6; i++)
            positions[i] = posParent.GetChild(i);

        Transform framesParent = GameObject.Find("Frames").transform;
        frames = new Transform[6];
        for (i = 0; i < 6; i++)
            frames[i] = framesParent.GetChild(0);

        w = new WaitForSeconds(1);

        aud = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (move)
            transform.position = Vector3.SmoothDamp(transform.position, positions[position].position, ref ref1, moveTime);

        timerImage.color = Color.Lerp(Color.red, Color.green, currentTime / maxTime);
        timerImage.fillAmount = Mathf.Lerp(0, 1, currentTime / maxTime);

        if (timerOn)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
                manager.FinishedWord(position, false);
        }
    }

    public void UpdateWord(int cubbySlot, char newCharacter, int newAccent)
    {
        currentWord[cubbySlot] = newCharacter;
        currentWordAccents[cubbySlot] = newAccent;
        manager.CheckIfCorrectWord(position, currentWord, currentWordAccents);
    }

    public void UpdatePosition(int newPosition)
    {
        StopAllCoroutines();
        position = newPosition;
        StartCoroutine(MoveToNewPositionTimer());
    }

    private IEnumerator MoveToNewPositionTimer()
    {
        move = true;
        yield return w;
        move = false;
    }

    public void AddTime()
    {
        currentTime += 25;
        if (currentTime > maxTime) currentTime = maxTime;
    }

    public void PlaySound(AudioClip clip)
    {
        aud.clip = clip;
        aud.Play();
    }
}
