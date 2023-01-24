using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public enum FingerIKFlags : byte
{
    None = 0,
    Index = 1 << 7,
    Middle = 1 << 6,
    Ring = 1 << 5,
    Pinky = 1 << 4,
    Thumb = 1 << 3
}

[System.Serializable]
public class FingerTargetPositions
{
    public Transform attachPoint = null;
    public Transform indexIKPosition = null;
    public Transform middleIKPosition = null;
    public Transform ringIKPosition = null;
    public Transform pinkyIKPosition = null;
    public Transform thumbIKPosition = null;

    public void Copy(FingerTargetPositions positions, Vector3 axis, Vector3 rotationAxis)
    {
        if (positions.attachPoint != null && attachPoint == null)
        {
            attachPoint = new GameObject().transform;
            attachPoint.parent = positions.attachPoint.parent;
            attachPoint.localRotation = Quaternion.Euler(positions.attachPoint.localRotation * rotationAxis);
            attachPoint.localPosition = Vector3.Scale(positions.attachPoint.localPosition, axis);
            attachPoint.localScale = positions.attachPoint.localScale;
        }

        if (positions.indexIKPosition != null && indexIKPosition == null)
        {
            indexIKPosition = new GameObject().transform;
            indexIKPosition.parent = positions.indexIKPosition.parent;
            indexIKPosition.localPosition = Vector3.Scale(positions.indexIKPosition.localPosition, axis);
            indexIKPosition.localRotation = positions.indexIKPosition.localRotation;
            indexIKPosition.localScale = positions.indexIKPosition.localScale;
        }

        if (positions.middleIKPosition != null && middleIKPosition == null)
        {
            middleIKPosition = new GameObject().transform;
            middleIKPosition.parent = positions.middleIKPosition.parent;
            middleIKPosition.localPosition = Vector3.Scale(positions.middleIKPosition.localPosition, axis);
            middleIKPosition.localRotation = positions.middleIKPosition.localRotation;
            middleIKPosition.localScale = positions.middleIKPosition.localScale;
        }

        if (positions.ringIKPosition != null && ringIKPosition == null)
        {
            ringIKPosition = new GameObject().transform;
            ringIKPosition.parent = positions.ringIKPosition.parent;
            ringIKPosition.localPosition = Vector3.Scale(positions.ringIKPosition.localPosition, axis);
            ringIKPosition.localRotation = positions.ringIKPosition.localRotation;
            ringIKPosition.localScale = positions.ringIKPosition.localScale;
        }

        if (positions.pinkyIKPosition != null && pinkyIKPosition == null)
        {
            pinkyIKPosition = new GameObject().transform;
            pinkyIKPosition.parent = positions.pinkyIKPosition.parent;
            pinkyIKPosition.localPosition = Vector3.Scale(positions.pinkyIKPosition.localPosition, axis);
            pinkyIKPosition.localRotation = positions.pinkyIKPosition.localRotation;
            pinkyIKPosition.localScale = positions.pinkyIKPosition.localScale;
        }

        if (positions.thumbIKPosition != null && thumbIKPosition == null)
        {
            thumbIKPosition = new GameObject().transform;
            thumbIKPosition.parent = positions.thumbIKPosition.parent;
            thumbIKPosition.localPosition = Vector3.Scale(positions.thumbIKPosition.localPosition, axis);
            thumbIKPosition.localRotation = positions.thumbIKPosition.localRotation;
            thumbIKPosition.localScale = positions.thumbIKPosition.localScale;
        }
    }
}

public class HandGrabable : XRGrabInteractable
{
    #region Properties

    [Header("Settings")]
    [SerializeField] private bool useAttachPoint = true;
    public bool doorHandling = false;
    [SerializeField] private bool leftHand = false;

    [Header("Hand Settings")]
    [SerializeField] private Vector3 axis;
    [SerializeField] private Vector3 rotationAxis;

    [SerializeField]
    private FingerTargetPositions rightHandFignersPosition = new FingerTargetPositions();
    
    [SerializeField]
    private FingerTargetPositions leftHandFignersPosition = new FingerTargetPositions();

    [SerializeField]
    private FingerTargetPositions secondaryRightHandFignersPosition = new FingerTargetPositions();

    [SerializeField]
    private FingerTargetPositions secondaryLeftHandFignersPosition = new FingerTargetPositions();

    [Header("Door Handling")]
    [SerializeField] private bool inverseHandHandling = true; 
    [SerializeField] private bool doubleSide = false;
    [SerializeField] private Transform temporalHandAttachPoint;
    [SerializeField] private Transform upAxisAttackTransform;
    [SerializeField] private float angleOfAttackDegrees = 95.0f;
    [SerializeField] private float distanceThreshold = .05f;
    [SerializeField] private GameObject primaryCollider;
    [SerializeField] private GameObject secondaryCollider;

    private Vector3 oldHandAttachPointLocalPosition = Vector3.zero;
    private float angleOfAttackRadians = 0.0f;

    private SelectEnterEventArgs selectArgs = null;
    private float startingDistance = 0.0f;
    private bool secondaryCollision;

    [HideInInspector]
    public FingerTargetPositions currentPositions = null;

    #endregion

    #region Unity Events

    private void Start()
    {
        angleOfAttackRadians = angleOfAttackDegrees * Mathf.Deg2Rad;

        if (leftHand)
        {
            rightHandFignersPosition.Copy(leftHandFignersPosition, axis, rotationAxis);
            if (doubleSide)
            {
                secondaryRightHandFignersPosition.Copy(secondaryLeftHandFignersPosition, axis, rotationAxis);
            }
        }
        else
        {
            leftHandFignersPosition.Copy(rightHandFignersPosition, axis, rotationAxis);
            if (doubleSide)
            {
                secondaryLeftHandFignersPosition.Copy(secondaryRightHandFignersPosition, axis, rotationAxis);
            }
        }
    }

    public void Update()
    {
        if (selectArgs == null)
        {
            return;
        }      

        float currentDistance = Vector3.Distance(selectArgs.interactorObject.transform.position, upAxisAttackTransform.position);

        if (currentDistance >= startingDistance + distanceThreshold)
        {
            selectArgs.manager.SelectCancel(selectArgs.interactorObject, selectArgs.interactableObject);
            selectArgs = null;
            return;
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        bool leftController = args.interactorObject.transform.CompareTag("LeftController");        

        currentPositions = leftController ? leftHandFignersPosition : rightHandFignersPosition;        

        if (useAttachPoint)
        {
            attachTransform = currentPositions.attachPoint;
        }

        if (!doorHandling)
        {
            return;
        }

        if (doubleSide)
        {
            secondaryCollision = (args.interactorObject.transform.position - secondaryCollider.transform.position).sqrMagnitude <
            (args.interactorObject.transform.position - primaryCollider.transform.position).sqrMagnitude;

            if (secondaryCollision)
            {
                currentPositions = leftController ? secondaryLeftHandFignersPosition : secondaryRightHandFignersPosition;
            }
        }

        float radians = 1.0f;
        float hand = -1.0f;

        if (leftController)
        {
            hand = 1.0f;
        }

        // Left controller interacts facing the green axis
        // Right cotroller interacts facing the back of green axis
        if (inverseHandHandling)
        {
            radians = hand;
        }

        // Left hand x axis points down while right hand x axis points up, that's why we have to inverse it
        radians = Mathf.Acos(Vector3.Dot(args.interactorObject.transform.right * hand, 
            (doubleSide && secondaryCollision ? upAxisAttackTransform.up : upAxisAttackTransform.up * -1.0f) * radians));

        // Check conditions for selecting
        // We use right becvause hand is rotated and red axis is facing down
        if (radians > angleOfAttackRadians && angleOfAttackDegrees != 0.0f)
        {
            args.manager.SelectCancel(args.interactorObject, args.interactableObject);
            return;
        }

        Transform handAttachTransform = args.interactorObject.GetAttachTransform(args.interactableObject);

        oldHandAttachPointLocalPosition = handAttachTransform.localPosition;
        handAttachTransform.position = temporalHandAttachPoint.position;

        startingDistance = Vector3.Distance(args.interactorObject.transform.position, upAxisAttackTransform.position);

        selectArgs = args;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (doorHandling)
        {
            if (oldHandAttachPointLocalPosition != Vector3.zero)
            {
                args.interactorObject.GetAttachTransform(args.interactableObject).localPosition = oldHandAttachPointLocalPosition;
            }

            selectArgs = null;
        }
    }

    #endregion
}
