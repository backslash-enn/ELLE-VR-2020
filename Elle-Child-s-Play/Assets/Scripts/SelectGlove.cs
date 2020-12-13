using TMPro;
using UnityEngine;

public class SelectGlove : MonoBehaviour
{
    private int selectedGloveIdx = -1;
    private int lastSelected = -1;
    private bool defaultView = true;
   

    public Glove[] gloveList;
    public string[] gloveColors;
    public GameObject header;
    public GameObject subHeader1;
    public GameObject subHeader2;
    public GameObject colorHeader;
    private TMP_Text color;
    private TMP_Text curHeader;
    private TMP_Text curSubHeader1;
    private TMP_Text curSubHeader2;
    public GameObject confirmBtn;
    public GameObject cancelBtn;


    [System.Serializable]
    public class Glove
    {
        public GameObject glove;
        public GameObject glovePin;
    }

    void Start()
    {
        curHeader = header.GetComponent<TMP_Text>();
        curSubHeader1 = subHeader1.GetComponent<TMP_Text>();
        curSubHeader2 = subHeader2.GetComponent<TMP_Text>();
        color = colorHeader.GetComponent<TMP_Text>();
    }

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

            print(color);
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
        //int gloveNumber = gloveList.Length;

        defaultView = false;

        //for (int i = 0; i < gloveNumber; i++)
        //{
        //    gloveList[i].glove.gameObject.GetComponent<Button>().interactable = false;
        //}
    }

    public void CancelSelection()
    {
        lastSelected = selectedGloveIdx;
        selectedGloveIdx = lastSelected;

        defaultView = true;

        //int gloveNumber = gloveList.Length;

        //for (int i = 0; i < gloveNumber; i++)
        //{
        //    gloveList[i].glove.gameObject.GetComponent<UnityEngine.UI.Button>().interactable = true;
        //}
    }

    public void ConfirmSelection()
    {
        lastSelected = selectedGloveIdx;
        //int gloveNumber = gloveList.Length;

        defaultView = true;

        //for (int i = 0; i < gloveNumber; i++)
        //{
        //    gloveList[i].glove.gameObject.GetComponent<UnityEngine.UI.Button>().interactable = true;
        //}
    }
}
