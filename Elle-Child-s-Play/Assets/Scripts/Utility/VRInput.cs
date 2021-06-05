using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.EventSystems;

/// Handles all VR Input for the entire game. Any other class that wants to poll VR input gets it from here.
public class VRInput : MonoBehaviour
{
    public HandControls leftHand;
    public HandControls rightHand;
    public InputActionReference start;

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

    public BaseInputModule leftHandEventSystem, rightHandEventSystem;

    [SerializeField]
    InputActionAsset m_ActionAsset;
    public InputActionAsset actionAsset
    {
        get => m_ActionAsset;
        set => m_ActionAsset = value;
    }

    private void OnEnable()
    {
        if (m_ActionAsset != null)
        {
            m_ActionAsset.Enable();
        }

        if (ELLEAPI.rightHanded)
            leftHandEventSystem.enabled = false;
        else
            rightHandEventSystem.enabled = false;
    }


    void Update()
    {
        bool temp;

        leftTrigger = leftHand.trigger.action.ReadValue<float>();
        temp = leftTriggerDigital;
        leftTriggerDigital = leftTrigger >= digitalThreshold;
        leftTriggerDigitalDown = (!temp && leftTriggerDigital);
        leftTriggerDigitalUp = (temp && !leftTriggerDigital);
        rightTrigger = rightHand.trigger.action.ReadValue<float>();
        temp = rightTriggerDigital;
        rightTriggerDigital = rightTrigger >= digitalThreshold;
        rightTriggerDigitalDown = (!temp && rightTriggerDigital);
        rightTriggerDigitalUp = (temp && !rightTriggerDigital);

        leftGrip = leftHand.grip.action.ReadValue<float>();
        temp = leftGripDigital;
        leftGripDigital = leftGrip >= digitalThreshold;
        leftGripDown = (!temp && leftGripDigital);
        leftGripUp = (temp && !leftGripDigital);
        rightGrip = rightHand.grip.action.ReadValue<float>();
        temp = rightGripDigital;
        rightGripDigital = rightGrip >= digitalThreshold;
        rightGripDown = (!temp && rightGripDigital);
        rightGripUp = (temp && !rightGripDigital);

        temp = a;
        a = (rightHand.primary.action.ReadValue<float>() == 1);
        aDown = (!temp && a);
        aUp = (temp && !a);
        temp = b;
        b = (rightHand.secondary.action.ReadValue<float>() == 1);
        bDown = (!temp && b);
        bUp = (temp && !b);
        temp = x;
        x = (leftHand.primary.action.ReadValue<float>() == 1);
        xDown = (!temp && x);
        xUp = (temp && !x);
        temp = y;
        y = (leftHand.secondary.action.ReadValue<float>() == 1);
        yDown = (!temp && y);
        yUp = (temp && !y);


        leftStick = leftHand.stick.action.ReadValue<Vector2>();
        rightStick = rightHand.stick.action.ReadValue<Vector2>();

        temp = leftStickClick;
        leftStickClick = (leftHand.stickClick.action.ReadValue<float>() == 1);
        leftStickClickDown = (!temp && leftStickClick);
        leftStickClickUp = (temp && !leftStickClick);
        temp = rightStickClick;
        rightStickClick = (rightHand.stickClick.action.ReadValue<float>() == 1);
        rightStickClickDown = (!temp && rightStickClick);
        rightStickClickUp = (temp && !rightStickClick);

        temp = startButton;
        startButton = (start.action.ReadValue<float>() == 1);
        startButtonDown = (!temp && startButton);
        startButtonUp = (temp && !startButton);


        // !!!!!!!!!!!!!Vibes Only!!!!!!!!!!!!!
        if (leftHandContinuousVib)
        {
            /*
            if (leftHand.TryGetHapticCapabilities(out HapticCapabilities capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    leftHand.SendHapticImpulse(channel, leftHandContinuousVibStrength, Time.deltaTime + 0.0001f);
                }
            }
            */
        }

        if (rightHandContinuousVib)
        {
            /*
            if (rightHand.TryGetHapticCapabilities(out HapticCapabilities capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    rightHand.SendHapticImpulse(channel, rightHandContinuousVibStrength, Time.deltaTime + 0.0001f);
                }
            }
            */
        }
    }

    public static void LeftHandVibrationEvent(float strength, float duration)
    {
        //if (leftHand.TryGetHapticCapabilities(out HapticCapabilities capabilities))
        //{
        //    if (capabilities.supportsImpulse)
        //    {
        //        uint channel = 0;
        //        leftHand.SendHapticImpulse(channel, strength, duration);
        //    }
        //}
    }

    public static void RightHandVibrationEvent(float strength, float duration)
    {
        //if (rightHand.TryGetHapticCapabilities(out HapticCapabilities capabilities))
        //{
        //    if (capabilities.supportsImpulse)
        //    {
        //        uint channel = 0;
        //        rightHand.SendHapticImpulse(channel, strength, duration);
        //    }
        //}
    }

    public static void LeftHandContinuousVibration(bool on, float strength)
    {
        //leftHandContinuousVib = on;
        //leftHandContinuousVibStrength = strength;
    }

    public static void RightHandContinuousVibration(bool on, float strength)
    {
        //rightHandContinuousVib = on;
        //rightHandContinuousVibStrength = strength;
    }

    [Serializable]
    public class HandControls
    {
        public InputActionReference primary, secondary, stickClick;
        public InputActionReference trigger, grip;
        public InputActionReference stick;
    }
}