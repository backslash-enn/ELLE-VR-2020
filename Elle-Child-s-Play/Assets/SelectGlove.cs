using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SelectGlove : MonoBehaviour
{
    private int selectedGloveIdx = -1;
    private int lastSelected = -1;
    private bool defaultView = true;
    private bool rightHanded = false;
   

    public Glove[] gloveList;
    public string[] gloveColors;
    public GameObject header;
    public GameObject subHeader1;
    public GameObject subHeader2;
    public GameObject colorHeader;
    private Text color;
    private Text curHeader;
    private Text curSubHeader1;
    private Text curSubHeader2;
    public GameObject confirmBtn;
    public GameObject cancelBtn;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject leftHandS;
    public GameObject rightHandS;


    [System.Serializable]
    public class Glove
    {
        public GameObject glove;
        public GameObject glovePin;
    }

    // Start is called before the first frame update
    void Start()
    {
        curHeader = header.GetComponent<Text>();
        curSubHeader1 = subHeader1.GetComponent<Text>();
        curSubHeader2 = subHeader2.GetComponent<Text>();
        color = colorHeader.GetComponent<Text>();
        leftHand.GetComponent<UnityEngine.UI.Toggle>().isOn = false;
        leftHand.GetComponent<UnityEngine.UI.Toggle>().isOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        int len = gloveList.Length;


        if (!defaultView)
        {
            for (int i = 0; i < len; i++)
            {
                if (i != selectedGloveIdx && Vector3.Distance(gloveList[i].glove.transform.position, gloveList[selectedGloveIdx].glove.transform.position) <= (100 * System.Math.Abs(selectedGloveIdx - i)) + 150)
                {
                    
                    if (i < selectedGloveIdx)
                    {
                        gloveList[i].glove.transform.position += 400 * Time.deltaTime * Vector3.left;
                        gloveList[i].glove.transform.rotation = Quaternion.Lerp(gloveList[i].glove.transform.rotation, Quaternion.Euler(0, 0, 10), Time.deltaTime * 5);
                        gloveList[i].glovePin.transform.position += 400 * Time.deltaTime * Vector3.left;
                    }
                    else
                    {
                        gloveList[i].glove.transform.position += 400 * Time.deltaTime * Vector3.right;
                        gloveList[i].glove.transform.rotation = Quaternion.Slerp(gloveList[i].glove.transform.rotation, Quaternion.Euler(0, 0, -10), Time.deltaTime * 5);
                        gloveList[i].glovePin.transform.position += 400 * Time.deltaTime * Vector3.right;
                    }

                }
                    
                else
                    gloveList[i].glove.transform.rotation = Quaternion.Slerp(gloveList[i].glove.transform.rotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 5);
            }

            color.gameObject.SetActive(true);
            color.text = gloveColors[selectedGloveIdx];

            if (selectedGloveIdx == 0)
                color.color = new Color(1, 0.4f, 0.7f);
            else if (selectedGloveIdx == 1)
                color.color = new Color(0.48f, 0, 0.48f);
            else if (selectedGloveIdx == 2)
                color.color = Color.blue;
            else if (selectedGloveIdx == 3)
                color.color = new Color(0, .18f, 0);
            else if (selectedGloveIdx == 4)
                color.color = Color.yellow;
            else if (selectedGloveIdx == 5)
                color.color = new Color(1, 0.55f, 0);
            else if (selectedGloveIdx == 6)
                color.color = Color.white;
            else if (selectedGloveIdx == 7)
                color.color = Color.black;

            curHeader.enabled = false;
            curSubHeader1.enabled = false;
            curSubHeader2.enabled = false;

            confirmBtn.SetActive(true);
            cancelBtn.SetActive(true);
            leftHand.SetActive(true);
            rightHand.SetActive(true);
        }

        if (defaultView && lastSelected != -1)
        {
            for (int i = 0; i < len; i++)
            {
                if (i != lastSelected && Vector3.Distance(gloveList[i].glove.transform.position, gloveList[lastSelected].glove.transform.position) >= (100 * System.Math.Abs(lastSelected - i)))
                {
                    if (i < lastSelected)
                    {
                        gloveList[i].glove.transform.position += 400 * Time.deltaTime * Vector3.right;
                        gloveList[i].glove.transform.rotation = Quaternion.Slerp(gloveList[i].glove.transform.rotation, Quaternion.Euler(0, 0, -10), Time.deltaTime * 5);
                        gloveList[i].glovePin.transform.position += 400 * Time.deltaTime * Vector3.right;
                    }
                    else
                    {
                        gloveList[i].glove.transform.position += 400 * Time.deltaTime * Vector3.left;
                        gloveList[i].glove.transform.rotation = Quaternion.Slerp(gloveList[i].glove.transform.rotation, Quaternion.Euler(0, 0, 10), Time.deltaTime * 5);
                        gloveList[i].glovePin.transform.position += 400 * Time.deltaTime * Vector3.left;
                    }

                }
          
                else
                    gloveList[i].glove.transform.rotation = Quaternion.Lerp(gloveList[i].glove.transform.rotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 5);
            }

            
            curHeader.enabled = true;
            curSubHeader1.enabled = true;
            curSubHeader2.enabled = true;

            color.gameObject.SetActive(false);
            confirmBtn.SetActive(false);
            cancelBtn.SetActive(false);
        }
        

    }

    public void GloveSelected(int idx)
    {
        selectedGloveIdx = idx;
        int gloveNumber = gloveList.Length;

        defaultView = false;

        for (int i = 0; i < gloveNumber; i++)
        {
            gloveList[i].glove.gameObject.GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
    }

    public void CancelSelection()
    {
        lastSelected = selectedGloveIdx;
        selectedGloveIdx = lastSelected;

        defaultView = true;

        int gloveNumber = gloveList.Length;

        for (int i = 0; i < gloveNumber; i++)
        {
            gloveList[i].glove.gameObject.GetComponent<UnityEngine.UI.Button>().interactable = true;
        }
    }

    public void ConfirmSelection()
    {
        lastSelected = selectedGloveIdx;
        int gloveNumber = gloveList.Length;

        defaultView = true;

        for (int i = 0; i < gloveNumber; i++)
        {
            gloveList[i].glove.gameObject.GetComponent<UnityEngine.UI.Button>().interactable = true;
        }
    }

    public void LeftHandSelected()
    {
        rightHanded = false;

        leftHandS.GetComponent<UnityEngine.UI.Image>().enabled = true;
        leftHand.GetComponent<UnityEngine.UI.Image>().enabled = false;
        rightHandS.GetComponent<UnityEngine.UI.Image>().enabled = false;
        rightHand.GetComponent<UnityEngine.UI.Image>().enabled = true;

    }

    public void RightHandSelected()
    {
        rightHanded = true;

        rightHandS.GetComponent<UnityEngine.UI.Image>().enabled = true;
        rightHand.GetComponent<UnityEngine.UI.Image>().enabled = false;
        leftHandS.GetComponent<UnityEngine.UI.Image>().enabled = false;
        leftHand.GetComponent<UnityEngine.UI.Image>().enabled = true;

    }
}
