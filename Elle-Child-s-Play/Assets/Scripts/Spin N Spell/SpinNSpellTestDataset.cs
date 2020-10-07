using UnityEngine;

public class SpinNSpellTestDataset : MonoBehaviour
{
    private string texturepath = @"Spin N Spell/";
    private string[] termNames =
    {
        "House", "Football", "Book", "Horse", "Shoe", "Flamingo", "Truck", "Waffle", "Tree", "Apple"
    };

    public static Term[] terms;

    void Awake()
    {
        terms = new Term[termNames.Length];
        for (int i = 0; i < termNames.Length; i++) {
            terms[i] = new Term
            {
                front = termNames[i],
                image = (Texture2D)Resources.Load(texturepath + termNames[i])
            };
        }
    }
}
