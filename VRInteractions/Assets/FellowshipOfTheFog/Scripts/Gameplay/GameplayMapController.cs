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

    #endregion

    #region Public Methods

    public void ShowMap(int floor = 0)
    {
        if (floor == 0)
        {
            firstFloorQuad.SetActive(true);
            secondFloorQuad.SetActive(false);
        }
        else if (floor == 1)
        {
            firstFloorQuad.SetActive(false);
            secondFloorQuad.SetActive(true);
        }
    }

    public void HideMap()
    {
        firstFloorQuad.SetActive(false);
        secondFloorQuad.SetActive(false);
    }

    public void SwitchFloor()
    {
        animator.SetFloat("Floor", 0);
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
