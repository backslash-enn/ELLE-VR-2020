[System.Serializable]
public class Question
{
	public int questionID;
	public string audioLocation;
	public string imageLocation;
	public string type;
	public string questionText;
	public Term[] answers;
}
