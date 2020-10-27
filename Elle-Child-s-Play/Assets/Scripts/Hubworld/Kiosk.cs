﻿using UnityEngine;

public class Kiosk : MonoBehaviour
{
    public Transform text;
    private bool open;
    private float tranSpeed = 10;

    void Update()
    {
        if(open)
            text.Rotate(Vector3.up * 6 * Time.deltaTime);
        text.localScale = Vector3.Lerp(text.localScale, Vector3.one * (open ? 0.33f : 0), tranSpeed * Time.deltaTime);
        text.position = new Vector3(text.position.x, Mathf.Lerp(text.position.y, open ? 1.2f : 0, tranSpeed * Time.deltaTime), text.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        other.transform.GetComponent<HubworldPlayer>().inSpinNSpell = true;
        open = true;
    }

    private void OnTriggerExit(Collider other)
    {
        other.transform.GetComponent<HubworldPlayer>().inSpinNSpell = false;
        open = false;
    }
}