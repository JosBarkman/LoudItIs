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
    public Transform attachPoint;
    public Transform indexIKPosition;
    public Transform middleIKPosition;
    public Transform ringIKPosition;
    public Transform pinkyIKPosition;
    public Transform thumbIKPosition;

    public void Copy(FingerTargetPositions positions, Vector3 axis, Vector3 rotationAxis)
    {
        if (positions.attachPoint != null)
        {
            attachPoint = new GameObject().transform;
            attachPoint.parent = positions.attachPoint.parent;
            attachPoint.localRotation = Quaternion.Euler(positions.attachPoint.localRotation * rotationAxis);
            attachPoint.localPosition = Vector3.Scale(positions.attachPoint.localPosition, axis);
        }

        if (positions.indexIKPosition != null)
        {
            indexIKPosition = new GameObject().transform;
            indexIKPosition.parent = positions.indexIKPosition.parent;
            indexIKPosition.localPosition = Vector3.Scale(positions.indexIKPosition.localPosition, axis);
        }

        if (positions.middleIKPosition != null)
        {
            middleIKPosition = new GameObject().transform;
            middleIKPosition.parent = positions.middleIKPosition.parent;
            middleIKPosition.localPosition = Vector3.Scale(positions.middleIKPosition.localPosition, axis);
        }

        if (positions.ringIKPosition != null)
        {
            ringIKPosition = new GameObject().transform;
            ringIKPosition.parent = positions.ringIKPosition.parent;
            ringIKPosition.localPosition = Vector3.Scale(positions.ringIKPosition.localPosition, axis);
        }

        if (positions.pinkyIKPosition != null)
        {
            pinkyIKPosition = new GameObject().transform;
            pinkyIKPosition.parent = positions.pinkyIKPosition.parent;
            pinkyIKPosition.localPosition = Vector3.Scale(positions.pinkyIKPosition.localPosition, axis);
        }

        if (positions.thumbIKPosition != null)
        {
            thumbIKPosition = new GameObject().transform;
            thumbIKPosition.parent = positions.thumbIKPosition.parent;
            thumbIKPosition.localPosition = Vector3.Scale(positions.thumbIKPosition.localPosition, axis);
        }
    }
}

public class HandGrabable : XRGrabInteractable
{
    [Header("Hand Settings")]
    public bool left = true;
    public bool useAttachPoint = true;
    public Vector3 axis;
    public Vector3 rotationAxis;

    public FingerTargetPositions rightHandFignersPosition = new FingerTargetPositions();
    public FingerTargetPositions leftHandFignersPosition = new FingerTargetPositions();

    private void Start()
    {
        if (left)
        {
            rightHandFignersPosition.Copy(leftHandFignersPosition, axis, rotationAxis);
        }
        else
        {
            leftHandFignersPosition.Copy(rightHandFignersPosition, axis, rotationAxis);
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (args.interactorObject.transform.CompareTag("LeftController") && useAttachPoint)
        {
            attachTransform = leftHandFignersPosition.attachPoint;
        }
        else if (args.interactorObject.transform.CompareTag("RightController") && useAttachPoint)
        {
            attachTransform = rightHandFignersPosition.attachPoint;
        }
    }
}
