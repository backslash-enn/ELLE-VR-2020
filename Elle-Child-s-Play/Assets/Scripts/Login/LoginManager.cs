using System.Timers;
using TMPro;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public TMP_Text codeText;
    private string code = "";

    public void AddNumToCode(int num)
    {
        if (code.Length >= 6) return;

        code += num.ToString();
        codeText.text = code;
    }

    public void RemoveNumFromCode()
    {
        if(code.Length == 0) return;

        code = code.Substring(0, code.Length - 1);
        codeText.text = code;
    }

    public void SubmitCode()
    {
        ELLEAPI.OTCLogin(code);
    }
}
