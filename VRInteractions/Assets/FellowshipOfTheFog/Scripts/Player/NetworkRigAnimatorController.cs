using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRigAnimatorController : MonoBehaviour
{
    #region Properties

    [SerializeField]
    private float fixFootOffset = .25f;

    private Animator animator;

    #endregion

    #region Unity Events

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Nice tutorial
    private void OnAnimatorIK(int layerIndex)
    {
        Vector3 footPosition = animator.GetIKPosition(AvatarIKGoal.RightFoot);
        Quaternion footRotation = animator.GetIKRotation(AvatarIKGoal.RightFoot);

        RaycastHit hit;
        if (Physics.Raycast(footPosition + Vector3.up, Vector3.down, out hit))
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1.0f);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, hit.point + fixFootOffset * Vector3.up);

            footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(footRotation * Vector3.forward, hit.normal), hit.normal);

            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1.0f);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, footRotation);
        }
        else
            {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0.0f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0.0f);
        }

        footPosition = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        footRotation = animator.GetIKRotation(AvatarIKGoal.LeftFoot);

        if (Physics.Raycast(footPosition + Vector3.up, Vector3.down, out hit))
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1.0f);
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point + fixFootOffset * Vector3.up);

            footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(footRotation * Vector3.forward, hit.normal), hit.normal);

            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1.0f);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, footRotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0.0f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0.0f);
        }
    }

    #endregion
}
