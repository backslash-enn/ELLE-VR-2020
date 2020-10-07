using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelZone : MonoBehaviour
{
    public Transform sign;
    public bool open;
    public float tranSpeed = 10;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        sign.Rotate(Vector3.up * 6 * Time.deltaTime);
        sign.localScale = Vector3.Lerp(sign.localScale, Vector3.one * (open ? 0.33f : 0), tranSpeed * Time.deltaTime);
        sign.position = new Vector3(sign.position.x, Mathf.Lerp(sign.position.y, open ? 1.2f : 0, tranSpeed * Time.deltaTime), sign.position.z);
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