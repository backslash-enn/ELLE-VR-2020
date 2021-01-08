using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginButton : MonoBehaviour
{
    private Image i;
    private Color startColor;
    private TMP_Text numText;
    private Image iconImage;
    public LoginManager lm;
    public bool backspaceButton, submitButton;
    public int num;

    void Start()
    {
        i = GetComponent<Image>();
        startColor = i.color;
        if (!backspaceButton && !submitButton)
            numText = transform.GetChild(0).GetComponent<TMP_Text>();
        else
            iconImage = transform.GetChild(0).GetComponent<Image>();
    }

    private void OnTriggerEnter(Collider other)
    {
        i.color = startColor * 0.6f;
        if(!backspaceButton && !submitButton)
            numText.color = Color.gray;
        else
            iconImage.color = Color.gray;
    }

    private void OnTriggerExit(Collider other)
    {
        i.color = startColor;
        if (!backspaceButton && !submitButton)
            numText.color = Color.white;
        else
            iconImage.color = Color.white;

        if (backspaceButton) lm.RemoveNumFromCode();
        else if (submitButton) lm.SubmitCode();
        else lm.AddNumToCode(num);
    }
}