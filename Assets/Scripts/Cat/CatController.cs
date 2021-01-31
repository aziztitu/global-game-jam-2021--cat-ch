using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatController : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent nav;
    private CatModel catModel;

    private Transform currentHidingSpot = null;
    private Transform potentialHidingSpot = null;

    [Range(0,1)]
    public float hidingDistance = 0;
    
    [Tooltip("Should be greater than hidingDistance.")]
    [Range(1.5f,2)]
    public float inspectingDistance = 0;

    public enum CatState
    {
        Hiding,
        Running
    }

    public CatState catState = CatState.Hiding;

    [Header("Timers")]
    public RangeFloat maxMeowTimer = new RangeFloat(0, 0);
    private float currentMeowBuffer = 0;

    public RangeFloat maxChangeLocationTimer = new RangeFloat(0, 0);
    private float currentChangeLocationBuffer = 0;

    [Header("Meows")]
    public List<AudioClip> meowClips = new List<AudioClip>();
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        nav = GetComponent<NavMeshAgent>();
        catModel = GetComponent<CatModel>();

        currentMeowBuffer = maxMeowTimer.GetRandom();
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
            //potentialHidingSpot = CatManager.Instance.FindOpenLocation(tempPlayer.transform.position, transform.position);
            potentialHidingSpot = CatManager.Instance.FindOpenLocation();
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

        if (Vector3.Distance(transform.position, potentialHidingSpot.position) < hidingDistance)
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

    public void ChangeState(CatState state)
    {
        if (catState != state)
        {
            switch (state)
            {
                case CatState.Hiding:
                    catModel.SetHidingAvatar();
                    ChangeCurrentHidingPlace();
                    
                    break;
                case CatState.Running:
                    catModel.SetRunningAvatar();
                    FindNewHidingSpot();

                    if (currentHidingSpot != null)
                    {
                        CatManager.Instance.AddCurrentHidingSpot(currentHidingSpot);
                    }
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
                if (Vector3.Distance(transform.position, potentialHidingSpot.position) < inspectingDistance)
                {
                    CheckNewHidingSpot();
                }
                break;
        }
    }

    private void Update()
    {
        CountDownMeow();
        CountDownRoam();
        
        CatStateUpdates();

        if (Input.GetKeyDown(KeyCode.D))
        {
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        CatManager.Instance.activeCats.Remove(this);

        if (!CatManager.Instance.hidingSpots.Contains(currentHidingSpot))
        {
            CatManager.Instance.AddCurrentHidingSpot(currentHidingSpot);
        }
    }
}
