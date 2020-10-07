using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ELLEAPI : MonoBehaviour
{
    private static readonly string serverLocation = "http://54.158.210.144:3000/api";
    private static readonly string jwt = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpYXQiOjE2MDE5NTg0MzIsIm5iZiI6MTYwMTk1ODQzMiwianRpIjoiNmRkYTg0YTYtOGI1NC00ODI3LTkyMzYtZWJkZGJlYjFjN2NhIiwiZXhwIjoxNjAzMTY4MDMyLCJpZGVudGl0eSI6OSwiZnJlc2giOmZhbHNlLCJ0eXBlIjoiYWNjZXNzIiwidXNlcl9jbGFpbXMiOiJzdSJ9.8DiuwxvITXnmEBG92UJJaNKBcJXuHLP7e3yDOYvpBsM";

    public static List<Module> GetModuleList()
    {
        string modulesJson = MakeRequest("modules");
        /*
        modulesJson = 
@"[
    {
        ""moduleID"": 1,
        ""groupID"": 1,
        ""name"": ""TestModule"",
        ""language"": ""EN"",
        ""complexity"": null
    }
]";
        */
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
                var test = json[closeIndex];
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

    private static string MakeRequest(string route, bool isPost = false, Dictionary<string, string> form = null)
    {
        string jsonResponse;

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{serverLocation}/{route}");
        request.Method = isPost ? "POST" : "GET";
        request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {jwt}");

        if (isPost && form != null) {
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

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        {
            jsonResponse = reader.ReadToEnd();
        }

        return jsonResponse;
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
            {
                Debug.Log(www.error);
                print("failed aud :(");
            }
            else
            {
                term.audio = DownloadHandlerAudioClip.GetContent(www);
                print("did aud succ!");
            }
        }
    }
}
