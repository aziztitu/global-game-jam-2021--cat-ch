﻿using BasicTools.ButtonInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatController : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent nav;
    private AudioSource carAudioSource;
    private CatModel catModel;

    private Transform currentHidingSpot = null;
    private Transform potentialHidingSpot = null;

    [Range(0,1)]
    public float hidingDistance = 0;
    
    [Tooltip("Should be greater than hidingDistance.")]
    [Range(1.5f,2)]
    public float inspectingDistance = 0;

    [Range(0, 100)]
    public int carHonkPercent = 0;

    public enum CatState
    {
        Hiding,
        Running,
        Caught
    }

    public CatState catState = CatState.Hiding;

    [Header("Timers")]
    public RangeFloat maxCarTimer = new RangeFloat(0, 0);
    private float currentCarBuffer = 0;
    private bool shouldHonk = false;
    private bool hasCarHonked = false;

    public RangeFloat maxChangeLocationTimer = new RangeFloat(0, 0);
    private float currentChangeLocationBuffer = 0;

    [Header("SFX Names")]
    public string meowSFX;
    public string catPopSFX;
    public string carHonkSFX;

    [Header("Capture Debugging")]
    [ButtonAttribute("Stop", "Stop")]
    [SerializeField]
    private bool _btnStop;

    [ButtonAttribute("Resume", "Resume")]
    [SerializeField]
    private bool _btnResume;

    [ButtonAttribute("Unwrap AI", "UnwrapAI")]
    [SerializeField]
    private bool _btnUnwrapAI;

    public GameObject CagePrefab;

    private void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
        catModel = GetComponent<CatModel>();
        carAudioSource = GetComponent<AudioSource>();

        currentChangeLocationBuffer = maxChangeLocationTimer.GetRandom();
    }

    private void OnEnable()
    {
        catState = CatState.Hiding;
        catModel.SetHidingAvatar();
    }

    public void FindNewHidingSpot()
    {
        if (currentHidingSpot == null)
        {
            potentialHidingSpot = CatManager.Instance.FindOpenLocation();
        }
        else
        {
            potentialHidingSpot = CatManager.Instance.FindOpenLocation(CharacterModel.Instance.gameObject.transform.position, transform.position);
        }
        
        nav.SetDestination(potentialHidingSpot.position);

        transform.rotation = Quaternion.LookRotation(new Vector3(potentialHidingSpot.position.x, 0, potentialHidingSpot.position.z) - new Vector3(transform.position.x, 0, transform.position.z), Vector3.up);
    }

    public void CheckNewHidingSpot()
    {
        if (!CatManager.Instance.IsLocationOpen(potentialHidingSpot))
        {
            FindNewHidingSpot();
            return;
        }

        Vector3 temp1 = transform.position, temp2 = potentialHidingSpot.position;
        temp1.y = 0;
        temp2.y = 0;

        if (Vector3.Distance(temp1, temp2) < hidingDistance)
        {
            ChangeState(CatState.Hiding);
        }
    }

    void ChangeCurrentHidingPlace()
    {
        currentHidingSpot = potentialHidingSpot;
        
        CatManager.Instance.RemoveCurrentHidingSpot(potentialHidingSpot);
        potentialHidingSpot = null;
    }

    public void SetCurrentHidingSpot(Transform newHidingSpot)
    {
        currentHidingSpot = newHidingSpot;
    }

    public void Meow()
    {
        int randNumb = Random.Range(0, 101);

        if (randNumb < 5)
        {
            SoundEffectsManager.Instance.PlayAt(catPopSFX, transform.position);
        }
        else
        {
            SoundEffectsManager.Instance.PlayAt($"{meowSFX}{Random.Range(1, 8)}", transform.position);
        }
    }

    void CountDownCar()
    {
        if (LevelManager.Instance.isGameOver)
        {
            return;
        }

        currentCarBuffer -= Time.deltaTime;

        if (currentCarBuffer <= 0 && !hasCarHonked)
        {
            SoundEffectsManager.Instance.PlayAt(carHonkSFX, transform.position);
            hasCarHonked = true;
            shouldHonk = false;
        }
    }

    void CountDownRoam()
    {
        if (catState != CatState.Hiding)
        {
            return;
        }

        currentChangeLocationBuffer -= Time.deltaTime;

        if(currentChangeLocationBuffer <= 0)
        {
            ChangeState(CatState.Running);
            currentChangeLocationBuffer = maxChangeLocationTimer.GetRandom();
        }
    }


    public void Stop()
    {
        nav.isStopped = true;

        if (carAudioSource.isPlaying)
        {
            carAudioSource.Pause();
        }
    }

    public void Resume()
    {
        if (catState == CatState.Running)
        {
            FindNewHidingSpot();
            carAudioSource.Play();
        }

        nav.isStopped = false;
    }

    public void UnwrapAI()
    {
        ChangeState(CatState.Caught);
        nav.enabled = false;
        
        catModel.SetHidingAvatar();
        catModel.enabled = false;
        
        carAudioSource.enabled = false;

        this.gameObject.GetComponent<Collider>().enabled = false;

        // PutInCage();
    }


    public void ChangeState(CatState state)
    {
        if (catState != state)
        {
            switch (state)
            {
                case CatState.Hiding:
                    catModel.SetHidingAvatar();
                    ChangeCurrentHidingPlace();

                    if (carAudioSource.isPlaying)
                    {
                        carAudioSource.Stop();
                    }

                    if (nav.isStopped == true)
                    {
                        nav.isStopped = false;
                    }
                    break;
                case CatState.Running:
                    catModel.SetRunningAvatar();
                    FindNewHidingSpot();

                    if (currentHidingSpot != null)
                    {
                        CatManager.Instance.AddCurrentHidingSpot(currentHidingSpot);
                    }


                    if (Random.Range(0,101) < carHonkPercent)
                    {
                        shouldHonk = true;
                    }
                    currentCarBuffer = maxCarTimer.GetRandom();
                    hasCarHonked = false;

                    carAudioSource.Play();
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
                if (nav.isStopped == true)
                {
                    nav.isStopped = false;
                }

                break;
            case CatState.Running:
                Vector3 temp1 = transform.position, temp2 = potentialHidingSpot.position;
                temp1.y = 0;
                temp2.y = 0;

                if (Vector3.Distance(temp1, temp2) < inspectingDistance)
                {
                    CheckNewHidingSpot();
                }

                if (shouldHonk)
                {
                    CountDownCar();
                }
                break;
        }
    }

    private void Update()
    {
        CountDownRoam();
        
        CatStateUpdates();

        if (LevelManager.Instance.isGameOver && carAudioSource.isPlaying)
        {
            carAudioSource.Stop();
        }
    }

    public void PutInCage()
    {
        GameObject CageParent = new GameObject($"{ gameObject.name } Caged");
        GameObject obj = Instantiate(CagePrefab);

        Vector3 currentPosition = transform.position;
        currentPosition.y -= transform.localScale.y;

        CageParent.transform.position = currentPosition;

        Vector3 temp1 = CageParent.transform.position, temp2 = ThirdPersonCamera.Instance.gameObject.transform.position;
        temp1.y = 0;
        temp2.y = 0;

        CageParent.transform.rotation = Quaternion.LookRotation((temp2 - temp1).normalized, Vector3.up);

        transform.position = obj.GetComponent<Cage>().cageTransform.position;
        transform.rotation = obj.GetComponent<Cage>().cageTransform.rotation;
        transform.parent = obj.transform;
        obj.transform.parent = CageParent.transform;
        //transform.localScale = obj.GetComponent<Cage>().cageTransform.localScale;

        CatManager.Instance.activeCats.Remove(this);

        if (!CatManager.Instance.hidingSpots.Contains(currentHidingSpot))
        {
            CatManager.Instance.AddCurrentHidingSpot(currentHidingSpot);
        }
    }
}
