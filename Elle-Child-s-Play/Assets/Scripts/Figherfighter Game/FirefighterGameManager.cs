using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FirefighterGameManager : MonoBehaviour
{
    public Transform pointerController;
    public Transform hosePointer;

    public ObiRigidbody hoseEnd;
    public Transform hoseTipPointer;

    public ParticleSystem waterPS;

    public ObiEmitter oe;

    public Balcony[] balconies;
    
    void Start()
    {
        balconies[0].Activate("Horse");
        balconies[1].Activate("Giraffe");
        balconies[2].Activate("Monkey");
        balconies[3].Activate("Rhino");
        balconies[4].Activate("Pencil");
        balconies[5].Activate("Stapler");
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
}
