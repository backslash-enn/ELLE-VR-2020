using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuTerm : MonoBehaviour
{
    private int termIndex = -2;
    private SpinNSpellManager manager;

    public void InitializeStuff(string termName, SpinNSpellManager m, int i)
    {
        manager = m;
        transform.GetChild(0).GetComponent<TMP_Text>().text = termName;
        termIndex = i;
    }

    public void ToggleTerm()
    {
        print($"I, {gameObject.name}, index {termIndex}, am now toggling");
        manager.ToggleTerm(termIndex);
    }
}
