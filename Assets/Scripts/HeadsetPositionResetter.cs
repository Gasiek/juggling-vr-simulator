using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

public class HeadsetPositionResetter : MonoBehaviour
{
    private XROrigin xrOrigin;
    private bool hmdIsValid;
    private bool positionResetted = false;
    private void Awake()
    {
        xrOrigin = GetComponent<XROrigin>();
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        hmdIsValid = device.isValid;
    }

    private void LateUpdate()
    {
        if (hmdIsValid && !positionResetted)
        {
            ResetHeadsetPosition();
        }
    }


    public void ResetHeadsetPosition()
    {
        Debug.Log("Resetting head position");
        positionResetted = true;
        StartCoroutine(PositionResetCoroutine());
    }

    private IEnumerator PositionResetCoroutine()
    {
        xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
        yield return new WaitForEndOfFrame();
        xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
    }
}
