using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuTerm : MonoBehaviour
{
    private int termIndex = -2;
    private GameMenu menu;

    public void InitializeStuff(string termName, GameMenu m, int i)
    {
        menu = m;
        transform.GetChild(0).GetComponent<TMP_Text>().text = termName;
        termIndex = i;
    }

    public void ToggleTerm()
    {
        menu.ToggleTerm(termIndex);
    }
}
