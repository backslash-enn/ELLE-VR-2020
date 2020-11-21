using Obi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FirefighterGameManager : MonoBehaviour
{
    private List<Module> moduleList;
    public Transform moduleListUIParent;
    public GameObject moduleUIElement;
    public Color[] moduleElementColors;

    public Transform pointerController;
    public Transform hosePointer;

    public ObiEmitter oe;

    public Balcony[] balconies;

    public TMP_Text ledSign;

    private AudioSource aud;

    public Fader blackFader;

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
        hosePointer.gameObject.SetActive(false);

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
