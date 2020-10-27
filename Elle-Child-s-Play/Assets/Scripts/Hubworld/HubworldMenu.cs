using UnityEngine;
using UnityEngine.UI;

public class HubworldMenu : MonoBehaviour
{
    public Text usernameText;
    public Image leftHandedButton, rightHandedButton;
    public Transform leftHandPointer, rightHandPointer;
    public GameObject leftHandBeam, rightHandBeam, leftHandDot, rightHandDot;

    void Start()
    {
        usernameText.text = ELLEAPI.username;
        leftHandedButton.color = Color.white * (ELLEAPI.rightHanded ? .6f : .9f);
        rightHandedButton.color = Color.white * (ELLEAPI.rightHanded ? .9f : .6f);
    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(leftHandPointer.position, leftHandPointer.forward, out hit))
        {
            if (hit.transform.CompareTag("Menu"))
            {
                leftHandBeam.SetActive(true);
                leftHandDot.SetActive(true);
                leftHandDot.transform.eulerAngles = hit.normal;
                leftHandDot.transform.eulerAngles += new Vector3(0, 180, 0);
                leftHandDot.transform.position = hit.point + leftHandDot.transform.forward * -0.001f;

                if(hit.transform.name == "Left Hand Button")
                    leftHandedButton.color = Color.white;
                else
                    leftHandedButton.color = Color.white * (ELLEAPI.rightHanded ? .6f : .9f);
                if (hit.transform.name == "Right Hand Button")
                    rightHandedButton.color = Color.white;
                else
                    rightHandedButton.color = Color.white * (ELLEAPI.rightHanded ? .9f : .6f);
            }
            else
            {
                leftHandBeam.SetActive(false);
                leftHandDot.SetActive(false);
                leftHandedButton.color = Color.white * (ELLEAPI.rightHanded ? .6f : .9f);
                rightHandedButton.color = Color.white * (ELLEAPI.rightHanded ? .9f : .6f);
            }
        }
        else
        {
            leftHandBeam.SetActive(false);
            leftHandDot.SetActive(false);
            leftHandedButton.color = Color.white * (ELLEAPI.rightHanded ? .6f : .9f);
            rightHandedButton.color = Color.white * (ELLEAPI.rightHanded ? .9f : .6f);
        }
    }
}
