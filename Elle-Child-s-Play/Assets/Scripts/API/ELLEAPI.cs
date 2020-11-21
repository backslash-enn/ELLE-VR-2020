using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ELLEAPI : MonoBehaviour
{
    private static readonly string serverLocation = "http://54.158.210.144:3000/api";

    private static string jwt = "";//"eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpYXQiOjE2MDE5NTg0MzIsIm5iZiI6MTYwMTk1ODQzMiwianRpIjoiNmRkYTg0YTYtOGI1NC00ODI3LTkyMzYtZWJkZGJlYjFjN2NhIiwiZXhwIjoxNjAzMTY4MDMyLCJpZGVudGl0eSI6OSwiZnJlc2giOmZhbHNlLCJ0eXBlIjoiYWNjZXNzIiwidXNlcl9jbGFpbXMiOiJzdSJ9.8DiuwxvITXnmEBG92UJJaNKBcJXuHLP7e3yDOYvpBsM";
    public static string username;
    public static int userID;
    public static bool rightHanded;
    public static string glovesSkin;

    public static string GetJWTFromOTC(string otcCode)
    {
        string response = MakeRequest("otclogin", true, new Dictionary<string, string> { { "otc", otcCode } });
        if (response == null) return null;

        dynamic d = JsonToDynamic(response);

        return d.access_token;
    }

    public static bool LoginWithJWT(string currentJWT)
    {
        string response;
        dynamic d;

        jwt = currentJWT;

        response = MakeRequest("user");
        if (response == null) return false;

        d = JsonToDynamic(response);

        username = d.username;

        response = MakeRequest("userpreferences");
        if (response == null) return false;

        d = JsonToDynamic(response);

        userID = (int)d.userID;
        rightHanded = d.preferredHand == "R";
        glovesSkin = d.vrGloveColor;

        return true;
    }

    public static void UpdatePreferences()
    {
        var form = new Dictionary<string, string>
        {
            { "preferredHand", ELLEAPI.rightHanded ? "R" : "L" },
            { "vrGloveColor",  ELLEAPI.glovesSkin }
        };

        MakeRequest("userpreferences", false, form, true);
    }

    public static List<Module> GetModuleList()
    {
        string modulesJson = MakeRequest("modules");

        if (string.IsNullOrEmpty(modulesJson))
        {
            print("Module request failed...");
            return null;
        }

        List<string> modulesListJson = SplitJsonArray(modulesJson);
        return GetModulesFromJsonArray(modulesListJson);
    }

    private static List<Module> GetModulesFromJsonArray(List<string> modulesListJson)
    {
        List<Module> modules = new List<Module>();
        for (int i = 0; i < modulesListJson.Count; i++)
            modules.Add(JsonUtility.FromJson<Module>(modulesListJson[i]));
        return modules;
    }

    public static List<Term> GetTermsFromModule(int moduleID)
    {
        string questionsJson = MakeRequest("modulequestions", true, new Dictionary<string, string> { { "moduleID", moduleID.ToString() } });

        var temp = SplitJsonArray(questionsJson);
        List<Question> questions = GetQuestionsFromJsonArray(temp);

        List<Term> terms = new List<Term>();
        for (int i = 0; i < questions.Count; i++)
        {
            if(questions[i].type == "MATCH" && questions[i].answers != null)
            {
                for (int j = 0; j < questions[i].answers.Length; j++)
                {
                    terms.Add(questions[i].answers[j]);
                    terms[terms.Count - 1].questionID = questions[i].questionID;
                    ELLEAPI.GetTexture(terms[terms.Count - 1]);
                    ELLEAPI.GetAudioClip(terms[terms.Count - 1]);
                }
            }
        }
        return terms;
    }

    private static List<Question> GetQuestionsFromJsonArray(List<string> questionsJson)
    {
        List<Question> questions = new List<Question>();
        for (int i = 0; i < questionsJson.Count; i++)
            questions.Add(JsonUtility.FromJson<Question>(questionsJson[i]));
        return questions;
    }

    private static List<string> SplitJsonArray(string json)
    {
        int openIndex, closeIndex;
        List<string> elements = new List<string>();

        openIndex = json.IndexOf('{');
        while (openIndex != -1)
        {
            closeIndex = openIndex + 1;
            int openCount = 0;
            while(json[closeIndex] != '}' || openCount > 0)
            {
                if (json[closeIndex] == '{') openCount++;
                if (json[closeIndex] == '}') openCount--;
                closeIndex++;
            }
            elements.Add(json.Substring(openIndex, closeIndex - openIndex + 1));
            openIndex = json.IndexOf('{', closeIndex + 1);
        }

        return elements;
    }

    public static int StartSession(int moduleID, bool isEndless)
    {
        string date = DateTime.Now.ToString("MM/dd/yy");
        string time = DateTime.Now.ToString("HH:mm");
        var form = new Dictionary<string, string> {
            { "moduleID", moduleID.ToString() },
            { "sessionDate", date },
            { "startTime", time },
            { "platform", "vr" }
        };

        if(isEndless)
            form.Add("mode", "endless");

        string response = MakeRequest("session", true, form);
        int start = response.IndexOf(':');
        response = response.Substring(start+1, response.IndexOf('\n', start) - start);
        return int.Parse(response);
    }

    public static void LogAnswer(int sessionID, Term term, bool correct, bool isEndless)
    {
        var form = new Dictionary<string, string> {
            { "questionID", term.questionID.ToString()},
            { "termID", term.termID.ToString()},
            { "sessionID", sessionID.ToString() },
            { "correct", correct ? "1" : "0" }
        };

        if (isEndless)
            form.Add("mode", "endless");

        MakeRequest("loggedanswer", true, form);
    }

    public static void EndSession(int sessionID, int playerScore)
    {
        string time = DateTime.Now.ToString("HH:mm");
        var form = new Dictionary<string, string> {
            { "sessionID", sessionID.ToString()},
            { "endTime", time},
            { "playerScore", playerScore.ToString() }
        };
        MakeRequest("endsession", true, form);
    }

    private static string MakeRequest(string route, bool isPost = false, Dictionary<string, string> form = null, bool isPut = false)
    {
        string jsonResponse;

        if (form != null && !isPost && !isPut)
        {
            bool didFirst = false;
            foreach (KeyValuePair<string, string> formEntry in form)
            {
                if (didFirst)
                    route += "&";
                else
                {
                    route += "?";
                    didFirst = true;
                }
                route += $"{formEntry.Key}={formEntry.Value}";
            }
        }

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{serverLocation}/{route}");
        if (isPost) request.Method = "POST";
        else if (isPut) request.Method = "PUT";
        else request.Method = "GET";

        if (route != "otclogin")
            request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {jwt}");

        if(form != null && (isPost || isPut))
        {
            request.ContentType = "application/json";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                StringBuilder formString = new StringBuilder();
                formString.Append('{');
                foreach (KeyValuePair<string, string> formEntry in form)
                {
                    formString.Append($"\"{formEntry.Key}\":\"{formEntry.Value}\",");
                }
                if (formString.Length > 1)
                    formString.Remove(formString.Length - 1, 1);
                formString.Append('}');

                streamWriter.Write(formString.ToString());
            }
        }

        try
        {
          HttpWebResponse response = (HttpWebResponse)request.GetResponse();

          using (StreamReader reader = new StreamReader(response.GetResponseStream()))
          {
              jsonResponse = reader.ReadToEnd();
          }

          return jsonResponse;
        }
        catch
        {
            return null;
        }
    }

    public static void GetTexture(Term term)
    {
        print("starting get texture");
        // Coroutines cannot be static. This is my hacky workaround
        var instance = GameObject.Find("Manager").GetComponent<ELLEAPI>();
        instance.StartCoroutine(instance.GetTextureCoroutine(term));
    }

    public IEnumerator GetTextureCoroutine(Term term)
    {
        if (string.IsNullOrEmpty(term.imageLocation)) { print(term.imageLocation); yield break; }
        // For some reason when grabbing images you dont include the port number
        string url = serverLocation.Remove(serverLocation.LastIndexOf(':'));
        url += term.imageLocation;

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
                term.image = tex;
            }
        }
    }

    public static void GetAudioClip(Term term)
    {
        // Coroutines cannot be static. This is my hacky workaround
        var instance = GameObject.Find("Manager").GetComponent<ELLEAPI>();
        instance.StartCoroutine(instance.GetAudioClipCoroutine(term));
    }

    public IEnumerator GetAudioClipCoroutine(Term term)
    {
        if (string.IsNullOrEmpty(term.audioLocation)) yield break;
        // For some reason when grabbing images you dont include the port number
        string url = serverLocation.Remove(serverLocation.LastIndexOf(':'));
        url += term.audioLocation;

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
                Debug.Log(www.error);
            else
                term.audio = DownloadHandlerAudioClip.GetContent(www);
        }
    }

    public static dynamic JsonToDynamic(string json)
    {
        return ParseJsonObject(json, 0).Item1;
    }

    private static (dynamic, int) ParseJsonObject(string json, int index)
    {
        int i = index;
        dynamic d = new ExpandoObject();

        string key;

        while(i < json.Length && json[i] != '}')
        {
            // GET THE KEY. IF WHILE TRYING TO GET THE KEY WE HIT A 
            // CLOSING CURLY BRACE, WE'VE HIT THE END OF THE OBJECT
            int charIndex = i;

            while (json[charIndex] != '\"' && json[charIndex] != '}')
                charIndex++;

            if (json[charIndex] == '}') return (d, charIndex);

            key = json.Substring(charIndex + 1, json.IndexOf('\"', charIndex + 1) - charIndex - 1);

            i = json.IndexOf(':', json.IndexOf('\"', charIndex + 1));
            i++;

            while (json[i] == ' ') i++;


            // GET THE VALUE. IT CAN BE A STRING, FLOAT, BOOL, OR OBJECT.
            // IF AN OBJECT, WE WILL HAVE TO MAKE A RECURSIVE CALL.

            // Value is a another object
            if (json[i] == '{')
            {
                var ret = ParseJsonObject(json, i);
                ((IDictionary<string, object>)d).Add(key, ret.Item1);
                i = ret.Item2 + 1;
            }
            // Value is a string
            else if (json[i] == '\"')
            {
                int j = i + 1;

                do { j = json.IndexOf('\"', j); }
                while (json[j - 1] == '\\');

                ((IDictionary<string, object>)d).Add(key, json.Substring(i + 1, j - i - 1));
                i = j + 1;
            }
            // Value is the bool true
            else if(json.Length >= i + 4 &&
            json[i] == 't' && json[i + 1] == 'r' && json[i + 2] == 'u' && json[i + 3] == 'e')
            {
                ((IDictionary<string, object>)d).Add(key, true);
                i = i + 4;
            }
            // Value is the bool false
            else if (json.Length >= i + 5 &&
            json[i] == 'f' && json[i + 1] == 'a' && json[i + 2] == 'l' && json[i + 3] == 's' && json[i + 4] == 'e')
            {
                ((IDictionary<string, object>)d).Add(key, false);
                i = i + 5;
            }
            // Value is a number
            else
            {
                int j = i;
                while (json[j] != ' ' && 
                    json[j] != ',' && 
                    json[j] != '}' && 
                    json[j] != '\r' && 
                    json[j] != '\n') 
                    j++;

                string numString = json.Substring(i, j - i);
                ((IDictionary<string, object>)d).Add(key, float.Parse(numString));
                i = j;
            }
        }

        return (d, i);
    }
}
