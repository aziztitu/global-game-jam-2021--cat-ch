using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatModel : MonoBehaviour
{
    public GameObject catHiding;
    public GameObject catRunning;

    public Animator currentAnim;

    public void SwitchCurrentModel()
    {
        if (catHiding.activeInHierarchy)
        {
            currentAnim = catHiding.GetComponent<Animator>();
        }
        else
        {
            currentAnim = catRunning.GetComponent<Animator>();
        }

        catHiding.SetActive(!catHiding.activeInHierarchy);
        catRunning.SetActive(!catRunning.activeInHierarchy);
    }

    public void SetHidingAvatar()
    {
        currentAnim = catHiding.GetComponent<Animator>();
        catHiding.SetActive(true);
        catRunning.SetActive(false);
    }

    public void SetRunningAvatar()
    {
        currentAnim = catRunning.GetComponent<Animator>();
        catRunning.SetActive(true);
        catHiding.SetActive(false);
    }
}
