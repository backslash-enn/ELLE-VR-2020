using UnityEngine;

[System.Serializable]
public class Term
{
    public int termID;
    public int questionID;

    public int imageID;
    public string imageLocation;
    public Texture2D image;

    public int audioID;
    public string audioLocation;
    public AudioClip audio;

    public string front;
    public string back;

    public string type;
    public string gender;
    public string language;
    public string[] tags;
}