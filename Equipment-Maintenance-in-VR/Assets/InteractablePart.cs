﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class InteractablePart : MonoBehaviour {

    // Interactable related
    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);
    private Interactable interactable;


    public Transform endPointTransform;
   
    public float acceptableDegreesFromEndPoint = 10f;
    public float acceptableMetersFromEndPoint = 0.1f;
    public bool snapAndDetach = true;
    public UnityEvent onAcceptablePlacement;

    private Transform transform;
    private Rigidbody rigidbody;
    private bool wasAcceptable = false;
    // Use this for initialization
    void Start () {
        interactable = GetComponent<Interactable>();
        transform = GetComponent<Transform>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void OnAcceptablePlacement()
    {
        onAcceptablePlacement.Invoke();
    }

    public void VibrateController(Hand hand, float durationSec, float frequency, float amplitude)
    {
        StartCoroutine(VibrateControllerContinuous(hand, durationSec, frequency, amplitude));
    }


    IEnumerator VibrateControllerContinuous(Hand hand, float durationSec, float frequency, float amplitude)
    {
        // if true the pulse will happen in a sawtooth pattern like this /|/|/|/|
        // else it will happen opposite like this |\|\|\|\
        hand.TriggerHapticPulse(durationSec, frequency, amplitude);
        yield break;

    }

    private bool IsWithinRangeOfCenter(Transform otherTransform, float limit)
    {
        return Vector3.Distance(gameObject.transform.position, otherTransform.position) <= limit ? true : false;
    }

    private bool IsWithinRangeOfRotation(Quaternion rot1, Quaternion rot2, float limit)
    {
        // Debug.Log("Rotation between objects: " + Quaternion.Angle(rot1, rot2));
        return Quaternion.Angle(rot1, rot2) <= limit ? true : false;
    }

    //-------------------------------------------------
    // Called when a Hand starts hovering over this object
    //-------------------------------------------------
    private void OnHandHoverBegin(Hand hand)
    {
        VibrateController(hand, 0.15f, 5f, 1f);
    }


    //-------------------------------------------------
    // Called when a Hand stops hovering over this object
    //-------------------------------------------------
    private void OnHandHoverEnd(Hand hand)
    {
    }


    //-------------------------------------------------
    // Called every Update() while a Hand is hovering over this object
    //-------------------------------------------------
    private void HandHoverUpdate(Hand hand)
    {
        GrabTypes startingGrabType = hand.GetGrabStarting();
        bool isGrabEnding = hand.IsGrabEnding(this.gameObject);

        if (interactable.attachedToHand == null && startingGrabType != GrabTypes.None)
        {
            // Call this to continue receiving HandHoverUpdate messages,
            // and prevent the hand from hovering over anything else
            hand.HoverLock(interactable);
            // Attach this object to the hand
            hand.AttachObject(gameObject, startingGrabType, attachmentFlags);

            // If it was picked up from the end point location apply gravity and kinematic again
            if(transform.position == endPointTransform.position)
            {
                rigidbody.useGravity = true;
                rigidbody.isKinematic = true;
            }
        }
        else if (isGrabEnding)
        {
            // Detach this object from the hand
            hand.DetachObject(gameObject);
            // Call this to undo HoverLock
            hand.HoverUnlock(interactable);
            // For snapping
            if (IsWithinRangeOfCenter(endPointTransform, acceptableMetersFromEndPoint)
                && IsWithinRangeOfRotation(gameObject.transform.rotation, endPointTransform.rotation, acceptableDegreesFromEndPoint))
            {
                OnAcceptablePlacement();
                wasAcceptable = true;
                // stay in place when placed correctly
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;

                // Move to End point transform
                transform.position = endPointTransform.position;
                transform.rotation = endPointTransform.rotation;
                endPointTransform.gameObject.SetActive(false);
                
            }
            else
            {
                wasAcceptable = false;
            }
        }

        if (interactable.attachedToHand != null && !isGrabEnding && endPointTransform != null)
        {
           if(IsWithinRangeOfCenter(endPointTransform, acceptableMetersFromEndPoint)
                && IsWithinRangeOfRotation(gameObject.transform.rotation, endPointTransform.rotation, acceptableDegreesFromEndPoint))
            {
                if (!wasAcceptable)
                {
                    OnAcceptablePlacement();
                    wasAcceptable = true;
                    if (snapAndDetach)
                    {
                        // Detach this object from the hand
                        hand.DetachObject(gameObject);
                        // Call this to undo HoverLock
                        hand.HoverUnlock(interactable);
                        // Move to End point transform
                        transform.position = endPointTransform.position;
                        transform.rotation = endPointTransform.rotation;
                        endPointTransform.gameObject.SetActive(false);
                    }
                }
                else
                {
                    endPointTransform.gameObject.SetActive(true);
                }
            }
            else
            {
                wasAcceptable = false;
            }

        }

    }

    //-------------------------------------------------
    // Called when this GameObject becomes attached to the hand
    //-------------------------------------------------
    private void OnAttachedToHand(Hand hand)
    {
    }


    //-------------------------------------------------
    // Called when this GameObject is detached from the hand
    //-------------------------------------------------
    private void OnDetachedFromHand(Hand hand)
    {
    }


    //-------------------------------------------------
    // Called every Update() while this GameObject is attached to the hand
    //-------------------------------------------------
    private void HandAttachedUpdate(Hand hand)
    {
    }


    //-------------------------------------------------
    // Called when this attached GameObject becomes the primary attached object
    //-------------------------------------------------
    private void OnHandFocusAcquired(Hand hand)
    {
    }


    //-------------------------------------------------
    // Called when another attached GameObject becomes the primary attached object
    //-------------------------------------------------
    private void OnHandFocusLost(Hand hand)
    {
    }
}
