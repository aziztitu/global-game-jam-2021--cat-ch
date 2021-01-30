using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController : MonoBehaviour
{
    private Transform currentHidingSpot = null;
    private Transform potentialHidingSpot = null;

    public RangeFloat maxMeowTimer = new RangeFloat(0, 0);
    private float currentMeowBuffer = 0;
    public List<AudioClip> meowClips = new List<AudioClip>();
    private AudioSource audioSource;

    public enum CatState
    {
        Hiding, 
        Running
    }

    public CatState catState = CatState.Running;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void FindNewHidingSpot()
    {
        potentialHidingSpot = CatManager.Instance.FindOpenLocation();
    }

    public void TryMove()
    {
        if (!CatManager.Instance.IsLocationOpen(potentialHidingSpot))
        {
            potentialHidingSpot = CatManager.Instance.FindOpenLocation();
        }
        else
        {
            ChangeCurrentHidingPlace();
        }
    }

    void ChangeCurrentHidingPlace()
    {
        currentHidingSpot = potentialHidingSpot;
        CatManager.Instance.RemoveCurrentHidingSpot(currentHidingSpot);

        transform.position = currentHidingSpot.position;
    }

    void CountDownMeow()
    {
        currentMeowBuffer -= Time.deltaTime;

        if (currentMeowBuffer <= 0)
        {
            Meow();
            currentMeowBuffer = maxMeowTimer.GetRandom();
        }
    }

    public void Meow()
    {
        audioSource.clip = meowClips[Random.Range(0, meowClips.Count)];
        audioSource.Play();
    }

    private void Update()
    {
        CountDownMeow();


    }
}
