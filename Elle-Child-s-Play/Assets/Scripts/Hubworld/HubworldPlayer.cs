using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HubworldPlayer : MonoBehaviour
{
    public float moveSpeed, rotationSpeed;
    public Transform cam;
    private Rigidbody rb;
    public bool inSpinNSpell, inFF;
    public Fader blackFader;
    public Transform groundT;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        blackFader.Fade(false, .5f);
    }

    void Update()
    {
        Vector3 upVector = Vector3.up;
        if (Physics.Raycast(groundT.position, Vector3.down, out RaycastHit hit))
            upVector = hit.normal;

        Debug.DrawRay(groundT.position, upVector, Color.red);

        Vector3 lookDir = new Vector3(cam.forward.x, 0, cam.forward.z);
        lookDir = lookDir.normalized;

        Vector3 biTan = Vector3.Cross(upVector, lookDir);

        Debug.DrawRay(groundT.position, biTan, Color.green);

        lookDir = Vector3.Cross(upVector, biTan);

        Debug.DrawRay(groundT.position, lookDir, Color.blue);

        float moveAng = Mathf.Atan2(-VRInput.leftStick.y, VRInput.leftStick.x) * Mathf.Rad2Deg - 90;
        
        float moveVel = VRInput.leftStick.magnitude * moveSpeed;
        if (moveVel > moveSpeed) moveVel = moveSpeed;
        
        Vector3 moveDir = Quaternion.AngleAxis(moveAng, upVector) * lookDir;

        Debug.DrawRay(groundT.position, moveDir, Color.white);

        rb.velocity = moveDir * moveVel;

        transform.Rotate(Vector3.up * VRInput.rightStick.x * rotationSpeed * 10 * Time.deltaTime);

        if (VRInput.a && inSpinNSpell)
            StartCoroutine(LoadScene("SpinNSpell"));
        if (VRInput.a && inFF)
            StartCoroutine(LoadScene("FirefighterGame"));
    }

    private IEnumerator LoadScene(string scene)
    {
        blackFader.Fade(true, .5f);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(scene);
    }
}