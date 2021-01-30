﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatController : MonoBehaviour
{
    private NavMeshAgent nav;

    private Transform currentHidingSpot = null;
    private Transform potentialHidingSpot = null;

    [Range(0,1)]
    public float hidingDistance = 0;
    private bool isHiding = false;

    public enum CatState
    {
        Hiding,
        Running
    }

    public CatState catState = CatState.Running;

    public RangeFloat maxMeowTimer = new RangeFloat(0, 0);
    private float currentMeowBuffer = 0;
    public List<AudioClip> meowClips = new List<AudioClip>();
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        nav = GetComponent<NavMeshAgent>();

        FindNewHidingSpot();
    }

    public void FindNewHidingSpot()
    {
        potentialHidingSpot = CatManager.Instance.FindOpenLocation();
        nav.SetDestination(potentialHidingSpot.position);
        
        transform.rotation = Quaternion.LookRotation(new Vector3(potentialHidingSpot.position.x, 0, potentialHidingSpot.position.z) - new Vector3(transform.position.x, 0, transform.position.z), Vector3.up);
    }

    public void CheckNewHidingSpot()
    {
        if (Vector3.Distance(transform.position, potentialHidingSpot.position) > hidingDistance)
        {
            if (!CatManager.Instance.IsLocationOpen(potentialHidingSpot))
            {
                FindNewHidingSpot();
            }
            else
            {
                return;
            }
        }
        else
        {
            ChangeState(CatState.Hiding);
        }
    }

    void ChangeCurrentHidingPlace()
    {
        if (currentHidingSpot != null)
        {
            CatManager.Instance.AddCurrentHidingSpot(currentHidingSpot);
        }
        currentHidingSpot = potentialHidingSpot;
        
        CatManager.Instance.RemoveCurrentHidingSpot(potentialHidingSpot);
        potentialHidingSpot = null;
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

    public void ChangeState(CatState state)
    {
        if (catState != state)
        {
            switch (state)
            {
                case CatState.Hiding:
                    ChangeCurrentHidingPlace();
                    
                    break;
                case CatState.Running:
                    FindNewHidingSpot();

                    break;
            }

            catState = state;
        }
        
    }

    void CatStateUpdates()
    {
        switch (catState)
        {
            case CatState.Hiding:

                break;
            case CatState.Running:
                CheckNewHidingSpot();
                break;
        }
    }

    private void Update()
    {
        CountDownMeow();

        CatStateUpdates();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeState(CatState.Running);
        }
    }
}
