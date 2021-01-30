using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController : MonoBehaviour
{
    public List<Transform> hidingSpots = new List<Transform>();
    private Transform currentHidingSpot = null;

    public RangeFloat maxMeowTimer = new RangeFloat(0, 0);
    private float currentMeowBuffer = 0;
    public List<AudioClip> meowClips = new List<AudioClip>();
    private AudioSource audioSource;

    public GameObject playerTest;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void ChooseNewLocation()
    {
        Transform oldHidingSpot = null;
        if (currentHidingSpot != null)
        {
            oldHidingSpot = currentHidingSpot;
        }

        int randNumb = Random.Range(0, hidingSpots.Count);
        
        transform.position = hidingSpots[randNumb].position;
        currentHidingSpot = hidingSpots[randNumb];
        hidingSpots.Remove(hidingSpots[randNumb]);

        if (oldHidingSpot != null)
        {
            hidingSpots.Add(oldHidingSpot);
        }
    }

    public void ChooseNewLocation(Vector3 playerPos)
    {
        Transform oldHidingSpot = null;
        if (currentHidingSpot != null)
        {
            oldHidingSpot = currentHidingSpot;
        }

        float max = Mathf.Abs(Vector3.Distance(hidingSpots[0].position, playerPos));
        int maxIndex = 0;
        for (int i = 1; i < hidingSpots.Count; i++)
        {
             if (Mathf.Abs(Vector3.Distance(hidingSpots[i].position, playerPos)) > max)
             {
                max = Mathf.Abs(Vector3.Distance(hidingSpots[i].position, playerPos));
                maxIndex = i;
             }
        }

        transform.position = hidingSpots[maxIndex].position;
        currentHidingSpot = hidingSpots[maxIndex];
        hidingSpots.Remove(hidingSpots[maxIndex]);

        if (oldHidingSpot != null)
        {
            hidingSpots.Add(oldHidingSpot);
        }
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChooseNewLocation(playerTest.transform.position);
        }

        
    }
}
