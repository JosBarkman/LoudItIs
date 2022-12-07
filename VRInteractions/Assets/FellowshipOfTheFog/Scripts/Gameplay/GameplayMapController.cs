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
    private bool switchingMap = false;

    [Header("Fingers")]
    public Transform indexPosition;
    public Transform middlePosition;
    public Transform ringPosition;
    public Transform pinkyPosition;
    public Transform thumbPosition;

    #endregion

    #region Public Methods

    public void ShowMap()
    {
        animator.enabled = true;
        currentFloor = 0.0f;
        switchingMap = false;
    }

    public void HideMap()
    {
        animator.enabled = false;
        firstFloorQuad.SetActive(false);
        secondFloorQuad.SetActive(false);
        switchingMap = false;
    }

    public void SwitchFloor()
    {
        if (switchingMap)
        {
            return;
        }

        currentFloor = currentFloor == -1.0f ? 1.0f : -1.0f;
        animator.SetFloat("Floor", currentFloor);
        switchingMap = true;
    }

    public void ResetFloor()
    {
        switchingMap = false;
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
