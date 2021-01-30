using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController : MonoBehaviour
{
    public List<Transform> hidingSpots = new List<Transform>();
    private Transform currentHidingSpot = null;

    public List<AudioClip> meowClips = new List<AudioClip>();
    private AudioSource audioSource;

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

    public void Meow()
    {
        audioSource.clip = meowClips[Random.Range(0, meowClips.Count)];
        audioSource.Play();
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Meow();
        }

        Debug.Log(hidingSpots.Count);
    }
}
