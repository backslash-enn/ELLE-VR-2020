using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HubworldPlayer : MonoBehaviour
{
    public float moveSpeed;
    public Transform cam;
    private Rigidbody rb;
    public bool inSpinNSpell;
    public Fader blackFader;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        blackFader.Fade(false, .5f);
    }

    void Update()
    {
        Vector3 moveDir;
        float moveVel; float moveAng;

        moveVel = VRInput.leftStick.magnitude * moveSpeed;

        moveAng = Mathf.Atan2(VRInput.leftStick.y, -VRInput.leftStick.x) * Mathf.Rad2Deg - 90;
        Vector3 lookDir = new Vector3(cam.forward.x, 0, cam.forward.z);
        lookDir = lookDir.normalized;
        moveDir = Quaternion.AngleAxis(moveAng, Vector3.up) * lookDir;

        rb.velocity = moveDir * moveVel;

        if (VRInput.a && inSpinNSpell)
            StartCoroutine(LoadScene("SpinNSpell"));

    }

    private IEnumerator LoadScene(string scene)
    {
        blackFader.Fade(true, .5f);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(scene);
    }
}