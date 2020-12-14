using UnityEngine;

public class SpinNSpelleHand : MonoBehaviour
{
    public Animator anim;
    private bool inGame = false;
    private bool stickerMode = false;
    public GameObject remote, wand;
    public GameObject missBeam, hitBeam, activeBeam, hitMarker;

    public bool isRightHand;
    private Vector2 axis;
    private bool gripping, gripDown;


    public float pushPullFactor = 2.5f;
    private Vector3 lastPos, currentPos;
    public float rotationSpeed = 10;
    public Transform blockRotationTarget;
    public StickerCursor stickerCursor;

    public Transform currentBlock;
    public Transform blockParent;

    public LayerMask lm;
    public BlockCountManager bcm;

    private void Start()
    {
        anim.SetBool("Holding", true);
    }

    public void StartGame()
    {
        anim.SetBool("Holding Wand", true);
        remote.SetActive(false);
        wand.SetActive(true);
        inGame = true;
    }

    public void EndGame()
    {
        anim.SetBool("Holding Wand", false);
        remote.SetActive(true);
        wand.SetActive(false);
        hitBeam.SetActive(false);
        missBeam.SetActive(false);
        hitMarker.SetActive(false);
        inGame = false;
    }

    private void Update()
    {
        if (inGame)
            InGameRoutine();
    }

    private void InGameRoutine()
    {
        axis = !isRightHand ? VRInput.leftStick : VRInput.rightStick;
        gripping = !isRightHand ? VRInput.leftGripDigital : VRInput.rightGripDigital;
        gripDown = !isRightHand ? VRInput.leftGripDown : VRInput.rightGripDown;

        if (currentBlock != null && !gripping)
            DropCurrentBlock();

        if(stickerMode == true)
        {
            if (stickerCursor.holdingSticker == false)
            {
                if (!isRightHand && VRInput.leftGripDown || isRightHand && VRInput.rightGripDown)
                {
                    if (stickerCursor.sticker != null) 
                    {
                        stickerCursor.GrabSticker();
                        anim.SetBool("Holding Sticker", true);
                    }
                }
            }
            else
            {
                if (!isRightHand && VRInput.leftGripUp || isRightHand && VRInput.rightGripUp)
                {
                    if (stickerCursor.sticker != null)
                    {
                        stickerCursor.RemoveSticker(true);
                        ExitStickerMode();
                    }
                }
            }
        }
        else if (Physics.Raycast(wand.transform.position, wand.transform.forward, out RaycastHit hit, 10, lm))
        {
            if (hit.transform.CompareTag("Block"))
            {
                hitBeam.SetActive(currentBlock == null);
                missBeam.SetActive(false);
                if (gripDown && currentBlock != hit.transform && !PauseMenu.paused && !hit.transform.parent.name.Contains("Hand"))
                    ChangeCurrentBlock(hit.transform);
            }
            else
            {
                hitBeam.SetActive(false);
                missBeam.SetActive(currentBlock == null);
            }
            hitMarker.SetActive(true);
            hitMarker.transform.position = hit.point;
            hitMarker.transform.rotation = Quaternion.LookRotation(-hit.normal);
            hitMarker.transform.position -= hitMarker.transform.forward * 0.001f; // To avoid z-fighting
        }
        else
        {
            hitBeam.SetActive(false);
            missBeam.SetActive(currentBlock == null);
            hitMarker.SetActive(false);
        }

        if (currentBlock != null)
        {
            // Rotate block
            currentPos = transform.position;
            blockRotationTarget.rotation *= Quaternion.AngleAxis(-10 * axis.x * rotationSpeed * Time.deltaTime, Vector3.up);
            blockRotationTarget.rotation *= Quaternion.AngleAxis(-10 * axis.y * rotationSpeed * Time.deltaTime, transform.right);
            currentBlock.rotation = Quaternion.Lerp(currentBlock.rotation, blockRotationTarget.rotation, 10 * Time.deltaTime);

            // Take the position delta
            Vector3 posDelta = lastPos - currentPos;

            // Take the part of it that is push pull
            float dot = Vector3.Dot(transform.forward, posDelta);

            // Push/pull the block based on that
            currentBlock.localPosition = new Vector3(currentBlock.localPosition.x, currentBlock.localPosition.y, currentBlock.localPosition.z - dot * pushPullFactor);

            lastPos = transform.position;
        }
    }

    public void DropCurrentBlock()
    {
        if (currentBlock == null) return;

        currentBlock.GetComponent<Block>().StopLightUpFace();

        if (bcm.frozenBlocks == false)
            currentBlock.GetComponent<Rigidbody>().isKinematic = false;
        currentBlock.GetComponent<PhysicsThrow>().ActivatePhysicsThrow();
        currentBlock.parent = blockParent;
        currentBlock.GetComponent<Block>().highlightedPulse.SetActive(false);
        currentBlock = null;

        activeBeam.SetActive(false);

        if (!isRightHand) VRInput.LeftHandContinuousVibration(false, 0);
        else              VRInput.RightHandContinuousVibration(false, 0);
    }

    void ChangeCurrentBlock(Transform newBlock)
    {
        if(currentBlock != null)
            currentBlock.GetComponent<Block>().highlightedPulse.SetActive(false);

        currentBlock = newBlock;
        if (newBlock.parent.CompareTag("Cubby"))
            newBlock.GetComponent<Block>().TakeMeOutOfCubby();

        lastPos = currentPos = transform.position;
        currentBlock.GetComponent<Rigidbody>().isKinematic = true;
        currentBlock.parent = transform;
        currentBlock.GetComponent<PhysicsThrow>().StartPhysicsThrowCapture();
        blockRotationTarget.rotation = Quaternion.identity;
        currentBlock.GetComponent<Block>().highlightedPulse.SetActive(true);

        activeBeam.SetActive(true);

        if (!isRightHand) VRInput.LeftHandContinuousVibration(true, 0.1f);
        else              VRInput.RightHandContinuousVibration(true, 0.1f);
    }

    void EnterStickerMode()
    {
        stickerMode = true;
        wand.SetActive(false);
        anim.SetBool("Sticker Mode", true);
        anim.SetBool("Holding Sticker", false);
    }

    void ExitStickerMode()
    {
        stickerMode = false;
        wand.SetActive(true);
        anim.SetBool("Sticker Mode", false);
        anim.SetBool("Holding Sticker", false);
    }

    public void PutStickerOnBlock()
    {
        stickerCursor.RemoveSticker(false);
        ExitStickerMode();
    }

    private void OnTriggerStay(Collider other)
    {
        if (stickerMode == false && inGame == true && currentBlock == null && other.CompareTag("Sticker Zone"))
            EnterStickerMode();
    }

    private void OnTriggerExit(Collider other)
    {
        if (stickerMode == true && !stickerCursor.holdingSticker && other.CompareTag("Sticker Zone"))
            ExitStickerMode();
    }
}
