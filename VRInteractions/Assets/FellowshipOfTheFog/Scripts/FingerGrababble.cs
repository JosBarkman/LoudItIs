using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerGrababble : MonoBehaviour
{
    public enum FingerIKFlags : byte
    {
        None = 0,
        Index = 1 << 7,
        Middle = 1 << 6,
        Ring = 1 << 5,
        Pinky = 1 << 4,
        Thumb = 1 << 3
    }

    public Transform indexIKPosition;
    public Transform middleIKPosition;
    public Transform ringIKPosition;
    public Transform pinkyIKPosition;
    public Transform thumbIKPosition;
}
