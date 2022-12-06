using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayMapController : MonoBehaviour
{
    #region Properties

    [SerializeField]
    private GameObject firstFloorQuad;

    [SerializeField]
    private GameObject secondFloorQuad;

    private Animator animator;
    private float currentFloor = 0.0f;

    [Header("Fingers")]
    public Transform indexPosition;
    public Transform middlePosition;
    public Transform ringPosition;
    public Transform pinkyPosition;
    public Transform thumbPosition;

    #endregion

    #region Public Methods

    public void ShowMap(float floor)
    {
        animator.enabled = true;
        if (floor == -1.0f)
        {
            firstFloorQuad.SetActive(true);
            secondFloorQuad.SetActive(false);
        }
        else if (floor == 1.0f)
        {
            firstFloorQuad.SetActive(false);
            secondFloorQuad.SetActive(true);
        }

        currentFloor = floor;
    }

    public void HideMap()
    {
        animator.enabled = false;
        firstFloorQuad.SetActive(false);
        secondFloorQuad.SetActive(false);
    }

    public void SwitchFloor()
    {
        currentFloor = currentFloor == -1.0f ? 1.0f : -1.0f;
        animator.SetFloat("Floor", currentFloor);
    }

    public void ResetFloor()
    {
        animator.SetFloat("Floor", 0.0f);
    }

    #endregion

    #region Unity Events

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        HideMap();
    }

    #endregion
}
