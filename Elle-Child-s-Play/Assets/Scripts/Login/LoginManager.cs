using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public TMP_Text codeText;
    private string code = "";
    public Fader errorMessageFader;

    void Start()
    {
        string jwt = PlayerPrefs.GetString("jwt", "");
        if (!string.IsNullOrEmpty(jwt))
            TryToGetIn(jwt);
    }

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
        string jwt = ELLEAPI.GetJWTFromOTC(code);
        if (jwt == null)
            StartCoroutine(ShowLoginError());
        else
            TryToGetIn(jwt);
    }

    private IEnumerator ShowLoginError()
    {
        errorMessageFader.Fade(true);
        yield return new WaitForSeconds(4);
        errorMessageFader.Fade(false);
    }

    private void TryToGetIn(string jwt)
    {
        if (ELLEAPI.LoginWithJWT(jwt))
        {
            PlayerPrefs.SetString("jwt", jwt);
            SceneManager.LoadScene("Hubworld");
        }
        else
            PlayerPrefs.DeleteKey("jwt");
    }
}
