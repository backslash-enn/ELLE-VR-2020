using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    private int cubbyFace;

    public SpinNSpelleHand leftHand, rightHand;
    private BlockCountManager countManager;
    private Transform blockParent;
    private Rigidbody r;
    private AudioSource a;
    private Transform[] lightUps;

    private Vector3 ref1;
    private WaitForSeconds w;
    private Vector3[] faceVectors, faceRotations;
    private char[] faceLetters;
    private int[] faceAccents;
    public Transform[] accentTransforms;

    private bool movingToCubby, cubbyLockout, onBlockBase;
    private float maxDeletionTimer = 5, currentDeletionTimer = 5;

    private Vector3 lastPos;
    public AudioClip[] collisionSounds;

    public GameObject highlightedPulse;
    
    void Start()
    {
        if (gameObject.name.Length > 9)
            gameObject.name = gameObject.name.Substring(0, 9);

        leftHand = GameObject.Find("Left Hand").GetComponent<SpinNSpelleHand>();
        rightHand = GameObject.Find("Right Hand").GetComponent<SpinNSpelleHand>();
        countManager = GameObject.Find("Blocks").GetComponent<BlockCountManager>();
            
        blockParent = transform.parent;
        r = GetComponent<Rigidbody>();
        a = GetComponent<AudioSource>();

        w = new WaitForSeconds(.5f);

        lightUps = new Transform[6];
        for (int i = 0; i < 6; i++)
            lightUps[i] = transform.GetChild(i);

        InitializeConstants();
    }

    void Update()
    {
        if (movingToCubby)
        {
            transform.position = Vector3.SmoothDamp(transform.position, transform.parent.position, ref ref1, 0.12f);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(faceRotations[cubbyFace]), 10f * Time.deltaTime);
        }
        if (transform.parent.CompareTag("Hand") == true)
            LightUpFace();

        // If you are not in a cubby, not being held by a hand (or are frozen), and are not in the block base, start the deletion timer
        if (!r.isKinematic && !onBlockBase)
        {
            currentDeletionTimer -= Time.deltaTime;
            if (currentDeletionTimer <= 0)
            {
                countManager.ReplaceBlock(gameObject);
                Destroy(gameObject);
            }
        }
        else
            currentDeletionTimer = maxDeletionTimer;
    }

    void LateUpdate()
    {
        lastPos = transform.position;
    }

    private void UpdateFaceVectors()
    {
        faceVectors[0] = -transform.right;
        faceVectors[1] = transform.forward;
        faceVectors[2] = -transform.forward;
        faceVectors[3] = -transform.up;
        faceVectors[4] = transform.up;
        faceVectors[5] = transform.right;
    }

    private void LightUpFace()
    {
        int i;
        float matchingFaceDot = -2;
        UpdateFaceVectors();

        for (i = 0; i < 6; i++)
        {
            float d = Vector3.Dot(-Vector3.forward, faceVectors[i]);
            if (d > matchingFaceDot)
            {
                cubbyFace = i;
                matchingFaceDot = d;
            }
        }
        for (i = 0; i < 6; i++)
            lightUps[i].gameObject.SetActive(i == cubbyFace);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (transform.parent.CompareTag("Cubby") == false && other.CompareTag("Cubby") == true)
            PutMeInsideCubby(other.transform);
        if (other.transform.name == "Block Base")
            onBlockBase = true;

        if (other.CompareTag("Sticker"))
        {
            Transform sticker = other.transform;
            int stickerFaceIndex = GetStickerFaceIndex(sticker);

            if (faceAccents[stickerFaceIndex] != 0) return;

            SpinNSpelleHand p = sticker.transform.parent.parent.GetComponent<SpinNSpelleHand>();
            if (p == null) return;
            p.PutStickerOnBlock();
            sticker.parent = accentTransforms[stickerFaceIndex];
            sticker.localPosition = sticker.localEulerAngles = Vector3.zero;

            int accentNumber = sticker.name[8] - '0';
            faceAccents[stickerFaceIndex] = accentNumber;

            // Hard code support for accents that sit below the letter
            if (accentNumber == 3)
                sticker.localPosition -= new Vector3(0, 0.78f, 0);
        }
    }

    private int GetStickerFaceIndex(Transform sticker)
    {
        Vector3 stickerVector = sticker.position - transform.position;
        stickerVector = stickerVector.normalized;
        UpdateFaceVectors();

        int faceIndex = -1;
        float bestDot = -1;
        for(int i = 0; i < 6; i++)
        {
            float currentDot = Vector3.Dot(stickerVector, faceVectors[i]);
            if(currentDot > bestDot)
            {
                faceIndex = i;
                bestDot = currentDot;
            }
        }

        return faceIndex;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.name == "Block Base")
            onBlockBase = false;
    }

    private void PutMeInsideCubby(Transform t)
    {
        if (cubbyLockout) return;
        if (t.childCount > 0) return;

        if (leftHand.currentBlock == transform)
            leftHand.DropCurrentBlock();
        if (rightHand.currentBlock == transform)
            rightHand.DropCurrentBlock();

        r.isKinematic = true;
        StopLightUpFace();
        
        StartCoroutine(MoveToCubby());
        countManager.ReplaceBlock(gameObject);

        transform.parent = t;
        t.parent.parent.GetComponent<CubbyRow>().UpdateWord(t.name[0] - '0', faceLetters[cubbyFace], faceAccents[cubbyFace]);

        a.Play();
    }

    public void TakeMeOutOfCubby()
    {
        transform.parent.parent.parent.GetComponent<CubbyRow>().UpdateWord(transform.parent.name[0] - '0', ' ', 0);
        transform.parent = blockParent;
        a.Play();
        StartCoroutine(LockoutCubby());
        StopCoroutine(MoveToCubby());
        movingToCubby = false;
    }

    private IEnumerator MoveToCubby()
    {
        movingToCubby = true;
        yield return w;
        movingToCubby = false;
    }

    private IEnumerator LockoutCubby()
    {
        // When you first remove a block from a cubby, it is very easy to accidentily immdiately place
        // the block in an adjacent cubby. The cubby lockout window is a brief period of time right
        // after you remove a block from a cubby where the block will not snap inside of any cubby. 
        // This should prevent a lot of unintentional block placements and lead to a better UX
        cubbyLockout = true;
        yield return w;
        cubbyLockout = false;
    }

    public void StopLightUpFace()
    {
        lightUps[cubbyFace].gameObject.SetActive(false);
    }

    private void InitializeConstants()
    {
        faceLetters = new char[6];
        faceAccents = new int[6];
        if (gameObject.name == "A-F Block")
        {
            faceLetters[0] = 'a';
            faceLetters[1] = 'b';
            faceLetters[2] = 'c';
            faceLetters[3] = 'd';
            faceLetters[4] = 'e';
            faceLetters[5] = 'f';
        }
        if (gameObject.name == "G-L Block")
        {
            faceLetters[0] = 'g';
            faceLetters[1] = 'h';
            faceLetters[2] = 'i';
            faceLetters[3] = 'j';
            faceLetters[4] = 'k';
            faceLetters[5] = 'l';
        }
        if (gameObject.name == "M-R Block")
        {
            faceLetters[0] = 'm';
            faceLetters[1] = 'o';
            faceLetters[2] = 'n';
            faceLetters[3] = 'p';
            faceLetters[4] = 'q';
            faceLetters[5] = 'r';
        }
        if (gameObject.name == "S-X Block")
        {
            faceLetters[0] = 's';
            faceLetters[1] = 't';
            faceLetters[2] = 'u';
            faceLetters[3] = 'v';
            faceLetters[4] = 'w';
            faceLetters[5] = 'x';
        }
        if (gameObject.name == "Y-Z Block")
        {
            faceLetters[0] = 'y';
            faceLetters[1] = 'y';
            faceLetters[2] = 'y';
            faceLetters[3] = 'z';
            faceLetters[4] = 'z';
            faceLetters[5] = 'z';
        }

        faceVectors = new Vector3[6];
        faceRotations = new Vector3[] {
            new Vector3(0, 90, -180),
            new Vector3(0, 180, -180),
            new Vector3(0, 0, -180),
            new Vector3(0, 90, 90),
            new Vector3(0, 90, -90),
            new Vector3(0, 90, 0)
        };
    }

    private void OnCollisionEnter(Collision collision)
    {
        float soundIntensity = Mathf.InverseLerp(0, 1, (transform.position - lastPos).magnitude);

        float thingLeft = 1.0f;
        float currentThing;
        while (thingLeft > 0)
        {
            currentThing = Random.Range(0.0f, 1.0f);
            if (currentThing > thingLeft)
                currentThing = thingLeft + 0.000001f;
            a.PlayOneShot(collisionSounds[Random.Range(0, collisionSounds.Length)], currentThing * soundIntensity);
            thingLeft -= currentThing;
        }
    }
}