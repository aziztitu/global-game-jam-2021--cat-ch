using System.Collections;
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
    
    [Tooltip("Should be greater than hidingDistance.")]
    [Range(1.5f,2)]
    public float inspectingDistance = 0;
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

    public GameObject tempPlayer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        nav = GetComponent<NavMeshAgent>();

        currentMeowBuffer = maxMeowTimer.GetRandom();
    }

    private void Start()
    {
        FindNewHidingSpot();
    }

    public void FindNewHidingSpot()
    {
        if (currentHidingSpot == null)
        {
            potentialHidingSpot = CatManager.Instance.FindOpenLocation();
        }
        else
        {
            potentialHidingSpot = CatManager.Instance.FindOpenLocation(tempPlayer.transform.position, transform.position);
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

        CatStateUpdates();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeState(CatState.Running);
        }
    }
}
