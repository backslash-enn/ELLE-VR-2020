using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    // Endpoint for the api. Note that the "/api" is removed when downloading images and audio
    private static readonly string serverLocation = "https://endlesslearner.com/api";

    private static string jwt = "";
    public static string username;
    public static int userID;
    public static bool rightHanded;
    public static string glovesSkin;
    private static string GOName;

    public void Start()
    {
        GOName = gameObject.name;
    }

    public static string GetJWTFromOTC(string otcCode)
    {
        string response = MakeRequest("otclogin", true, new Dictionary<string, string> { { "otc", otcCode } });
        if (response == null) return null;

        var jo = JObject.Parse(response);

        return jo.Value<string>("access_token");
    }

    public static bool LoginWithJWT(string currentJWT)
    {
        string response;
        jwt = currentJWT;

        response = MakeRequest("user");
        if (response == null) return false;

        var jo = JObject.Parse(response);

        username = jo.Value<string>("username");

        response = MakeRequest("userpreferences");
        if (response == null) return false;

        jo = JObject.Parse(response);

        rightHanded = jo.Value<string>("preferredHand") == "R";
        glovesSkin = jo.Value<string>("vrGloveColor");

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
            modules.Add(JsonConvert.DeserializeObject<Module>(modulesListJson[i]));
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

    public static List<Question> GetQuestionsFromModule(int moduleID)
    {
        string questionsJson = MakeRequest("modulequestions", true, new Dictionary<string, string> { { "moduleID", moduleID.ToString() } });

        var temp = SplitJsonArray(questionsJson);
        List<Question> questions = GetQuestionsFromJsonArray(temp);

        return questions;
    }

    private static List<Question> GetQuestionsFromJsonArray(List<string> questionsJson)
    {
        List<Question> questions = new List<Question>();
        for (int i = 0; i < questionsJson.Count; i++)
            questions.Add(JsonConvert.DeserializeObject<Question>(questionsJson[i]));
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

        var jo = JObject.Parse(response);

        return jo.Value<int>("sessionID");
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
        catch (Exception e)
        {
            print("EXCEPTION: " + e.Message);
            return null;
        }
    }

    public static void GetTexture(Term term)
    {
        print("starting get texture");
        // Coroutines cannot be static. This is my hacky workaround
        var instance = GameObject.Find(GOName).GetComponent<ELLEAPI>();
        instance.StartCoroutine(instance.GetTextureCoroutine(term));
    }

    public IEnumerator GetTextureCoroutine(Term term)
    {
        if (string.IsNullOrEmpty(term.imageLocation)) yield break;

        string url = serverLocation.Remove(serverLocation.LastIndexOf('/'));
        url += term.imageLocation;

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
                Debug.Log("IMAGE FILE ERROR: " + www.error);
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
        var instance = GameObject.Find(GOName).GetComponent<ELLEAPI>();
        instance.StartCoroutine(instance.GetAudioClipCoroutine(term));
    }

    public IEnumerator GetAudioClipCoroutine(Term term)
    {
        if (string.IsNullOrEmpty(term.audioLocation)) yield break;

        string url = serverLocation.Remove(serverLocation.LastIndexOf('/'));
        url += term.audioLocation;

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
                Debug.Log("AUDIO FILE ERROR: " + www.error);
            else
                term.audio = DownloadHandlerAudioClip.GetContent(www);
        }
    }
}