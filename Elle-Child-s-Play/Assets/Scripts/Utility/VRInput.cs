using UnityEngine.XR;
using UnityEngine;

// Handles all VR Input for the entire game. Any other class that wants to poll VR input gets it from here.

public class VRInput : MonoBehaviour
{
    private static InputDevice leftHand, rightHand;

    public static float leftTrigger, rightTrigger;
    public static bool leftTriggerDigital, rightTriggerDigital;
    public static float leftGrip, rightGrip;
    public static bool leftGripDigital, rightGripDigital;
    public static bool a, b, x, y;
    public static Vector2 leftStick, rightStick;
    public static bool leftStickClick, rightStickClick;
    public static bool startButton;

    public float digitalThreshold = 0.3f;

    public static bool leftTriggerDigitalDown, leftTriggerDigitalUp, rightTriggerDigitalDown, rightTriggerDigitalUp;
    public static bool leftGripDown, leftGripUp, rightGripDown, rightGripUp;
    public static bool aDown, aUp, bDown, bUp, xDown, xUp, yDown, yUp;
    public static bool leftStickClickDown, leftStickClickUp, rightStickClickDown, rightStickClickUp;
    public static bool startButtonDown, startButtonUp;

    private static bool leftHandContinuousVib, rightHandContinuousVib;
    private static float leftHandContinuousVibStrength, rightHandContinuousVibStrength;

    void Start()
    {
        leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    void Update()
    {
        bool temp;

        leftHand.TryGetFeatureValue(CommonUsages.trigger, out leftTrigger);
        temp = leftTriggerDigital;
        leftTriggerDigital = leftTrigger >= digitalThreshold;
        leftTriggerDigitalDown = (!temp && leftTriggerDigital);
        leftTriggerDigitalUp = (temp && !leftTriggerDigital);
        rightHand.TryGetFeatureValue(CommonUsages.trigger, out rightTrigger);
        temp = rightTriggerDigital;
        rightTriggerDigital = rightTrigger >= digitalThreshold;
        rightTriggerDigitalDown = (!temp && rightTriggerDigital);
        rightTriggerDigitalUp = (temp && !rightTriggerDigital);

        leftHand.TryGetFeatureValue(CommonUsages.grip, out leftGrip);
        temp = leftGripDigital;
        leftGripDigital = leftGrip >= digitalThreshold;
        leftGripDown = (!temp && leftGripDigital);
        leftGripUp = (temp && !leftGripDigital);
        rightHand.TryGetFeatureValue(CommonUsages.grip, out rightGrip);
        temp = rightGripDigital;
        rightGripDigital = rightGrip >= digitalThreshold;
        rightGripDown = (!temp && rightGripDigital);
        rightGripUp = (temp && !rightGripDigital);

        temp = a;
        rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out a);
        aDown = (!temp && a);
        aUp = (temp && !a);
        temp = b;
        rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out b);
        bDown = (!temp && b);
        bUp = (temp && !b);
        temp = x;
        leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out x);
        xDown = (!temp && x);
        xUp = (temp && !x);
        temp = y;
        leftHand.TryGetFeatureValue(CommonUsages.secondaryButton, out y);
        yDown = (!temp && y);
        yUp = (temp && !y);


        leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftStick);
        rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightStick);

        temp = leftStickClick;
        leftHand.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out leftStickClick);
        leftStickClickDown = (!temp && leftStickClick);
        leftStickClickUp = (temp && !leftStickClick);
        temp = rightStickClick;
        rightHand.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out rightStickClick);
        rightStickClickDown = (!temp && rightStickClick);
        rightStickClickUp = (temp && !rightStickClick);

        temp = startButton;
        leftHand.TryGetFeatureValue(CommonUsages.menuButton, out startButton);
        startButtonDown = (!temp && startButton);
        startButtonUp = (temp && !startButton);

        if (leftHandContinuousVib)
        {
            if (leftHand.TryGetHapticCapabilities(out HapticCapabilities capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    leftHand.SendHapticImpulse(channel, leftHandContinuousVibStrength, Time.deltaTime + 0.0001f);
                }
            }
        }

        if (rightHandContinuousVib)
        {
            if (rightHand.TryGetHapticCapabilities(out HapticCapabilities capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    rightHand.SendHapticImpulse(channel, rightHandContinuousVibStrength, Time.deltaTime + 0.0001f);
                }
            }
        }
    }

    public static void LeftHandVibrationEvent(float strength, float duration)
    {
        if (leftHand.TryGetHapticCapabilities(out HapticCapabilities capabilities))
        {
            if (capabilities.supportsImpulse)
            {
                uint channel = 0;
                leftHand.SendHapticImpulse(channel, strength, duration);
            }
        }
    }

    public static void RightHandVibrationEvent(float strength, float duration)
    {
        if (rightHand.TryGetHapticCapabilities(out HapticCapabilities capabilities))
        {
            if (capabilities.supportsImpulse)
            {
                uint channel = 0;
                rightHand.SendHapticImpulse(channel, strength, duration);
            }
        }
    }

    public static void LeftHandContinuousVibration(bool on, float strength)
    {
        leftHandContinuousVib = on;
        leftHandContinuousVibStrength = strength;
    }

    public static void RightHandContinuousVibration(bool on, float strength)
    {
        rightHandContinuousVib = on;
        rightHandContinuousVibStrength = strength;
    }
}