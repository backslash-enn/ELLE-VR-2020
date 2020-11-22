using Obi;
using System.Collections;
using TMPro;
using UnityEngine;

public class FirefighterGameManager : MonoBehaviour
{
    public Transform pointerController;
    public Transform hosePointer;

    public ObiEmitter oe;

    public Balcony[] balconies;

    public TMP_Text ledSign;

    private AudioSource aud;

    public Fader blackFader;

    public GameMenu menu;

    private bool movingTruck;
    public Transform truck;
    public Transform[] truckTires;
    private AudioSource truckAud;
    public AnimationCurve truckMoov;
    public float t = 0;

    public GameObject hoseAndWater, nozzle;
    public Transform domHand, nondomHand, leftHand, rightHand;

    public GameObject littlePoof;
    public AudioClip poofSound;

    public string[] words =
    {
        "Horse",
        "Giraffe",
        "Monkey",
        "Rhino",
        "Pencil",
        "Stapler",
    };
    
    void Start()
    {
        blackFader.Fade(false, .5f);

        aud = GetComponent<AudioSource>();
        truckAud = truck.GetComponent<AudioSource>();

        menu.onStartGame = MoveTruck;
    }

    void Update()
    {
        hosePointer.gameObject.SetActive(false);

        if(movingTruck)
        {
            truck.position += -Vector3.right * truckMoov.Evaluate(t*.4f) * 12 * Time.deltaTime;
            t += Time.deltaTime;
            foreach (Transform tire in truckTires)
                tire.Rotate(Vector3.right * 4000 * Time.deltaTime * truckMoov.Evaluate(t * .4f), Space.Self);
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
        print($"trig: {VRInput.rightTrigger} | speed: {oe.speed}");
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
        truckAud.Play();
        movingTruck = true;

        yield return new WaitForSeconds(2.5f);

        movingTruck = false;

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
    }

    private void StartIndividualsCatergory()
    {
        for(int i = 0; i < 6; i++)
        {
            int r = Random.Range(0, 16);
            balconies[r].Activate(words[i]);
        }
        ledSign.text = "";
        for (int i = 0; i < 3; i++)
        {
            int r = Random.Range(0, 6);
            ledSign.text += words[r] + "\n";
        }

    }
}
