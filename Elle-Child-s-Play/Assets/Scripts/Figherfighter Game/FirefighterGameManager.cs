using Obi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class FirefighterGameManager : MonoBehaviour
{
    public Transform pointerController;
    public Transform hosePointer;

    public ObiEmitter oe;

    public Balcony[] balconies;

    public TMP_Text ledSign;

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
        StartIndividualsCatergory();
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
