using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatManager : SingletonMonoBehaviour<CatManager>
{
    public List<Transform> hidingSpots = new List<Transform>();

    new void Awake()
    {
        base.Awake();
    }

    public Transform FindOpenLocation()
    {
        int randNumb = Random.Range(0, hidingSpots.Count);

        return hidingSpots[randNumb];
    }

    public Transform FindOpenLocation(Vector3 playerPos)
    {
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

        return hidingSpots[maxIndex];
    }

    public bool IsLocationOpen(Transform hidingSpot)
    {
        return hidingSpots.Contains(hidingSpot) ?  true :  false;
    }

    public void RemoveCurrentHidingSpot(Transform catHidingSpot)
    {
        hidingSpots.Remove(catHidingSpot);
    }

    public void AddCurrentHidingSpot(Transform catHidingSpot)
    {
        hidingSpots.Add(catHidingSpot);
    }
}