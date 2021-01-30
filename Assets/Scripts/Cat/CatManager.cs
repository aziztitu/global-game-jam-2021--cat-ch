using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatManager : SingletonMonoBehaviour<CatManager>
{
    public List<Transform> hidingSpots = new List<Transform>();
    
    [Range(0,360)]
    public float angleOfPlayerDistance = 0;

    new void Awake()
    {
        base.Awake();
    }

    public Transform FindOpenLocation()
    {
        int randNumb = Random.Range(0, hidingSpots.Count);
        return hidingSpots[randNumb];
    }

    public Transform FindOpenLocation(Vector3 playerPos, Vector3 catPos)
    {
        List<Transform> tempHideSpots = new List<Transform>();
        for (int i = 0; i < hidingSpots.Count; i++)
        {
            Vector3 tempHideSpot = hidingSpots[i].position, tempPlayerPos = playerPos, tempCatPos = catPos;
            tempHideSpot.y = 0;
            tempPlayerPos.y = 0;
            tempCatPos.y = 0;

            if (Mathf.Abs(Vector3.Angle((tempHideSpot - tempCatPos).normalized, (tempPlayerPos - tempCatPos).normalized)) > angleOfPlayerDistance)
            {
                Debug.Log(Mathf.Abs(Vector3.Angle((tempHideSpot - tempCatPos).normalized, (tempPlayerPos - tempCatPos).normalized)));
                tempHideSpots.Add(hidingSpots[i]);
            }
        }

        if (tempHideSpots.Count > 0)
        {
            int randNumb = Random.Range(0, tempHideSpots.Count);
            return tempHideSpots[randNumb];
        }
        else
        {
            int randNumb = Random.Range(0, hidingSpots.Count);
            return hidingSpots[randNumb];
        }
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