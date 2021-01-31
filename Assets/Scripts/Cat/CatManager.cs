using BasicTools.ButtonInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatManager : SingletonMonoBehaviour<CatManager>
{
    [Header("Cat Spawning")]
    [ButtonAttribute("Spawn Cat", "SpawnCat")]
    [SerializeField]
    private bool _btnSpawnShield;

    public List<GameObject> catPrefabsToSpawn = new List<GameObject>();

    [HideInInspector] public List<CatController> activeCats = new List<CatController>();

    [Header("Hiding Spots")]
    public List<Transform> hidingSpots = new List<Transform>();
    private int initialHidingSpotCount = 0;
    
    [Header("Cat Avoidance")]
    [Range(0,360)]
    public float angleOfPlayerDistance = 0;
    public float playerAvoidanceRadius = 0;

    [Header("Cat Caller")]
    public RangeFloat maxMeowTimer = new RangeFloat(0, 0);
    private float currentMeowBuffer = 0;

    new void Awake()
    {
        base.Awake();

        currentMeowBuffer = maxMeowTimer.GetRandom();
    }

    private void Start()
    {
        initialHidingSpotCount = hidingSpots.Count;
    }

    public void SpawnCat()
    {
        if (activeCats.Count >= initialHidingSpotCount - 1)
        {
            return;
        }

        if (activeCats.Count < initialHidingSpotCount - 1)
        {
            int randNumb = Random.Range(0, catPrefabsToSpawn.Count);

            GameObject cat = Instantiate(catPrefabsToSpawn[randNumb], this.transform);
            Transform spawnedLocation = FindOpenLocation();

            cat.GetComponent<CatController>().nav.enabled = false;
            cat.transform.position = spawnedLocation.position;
            cat.GetComponent<CatController>().nav.enabled = true;

            activeCats.Add(cat.GetComponent<CatController>());

            cat.GetComponent<CatController>().SetCurrentHidingSpot(spawnedLocation);
            hidingSpots.Remove(spawnedLocation);
        }
    }

    public Transform FindOpenLocation()
    {
        int randNumb = Random.Range(0, hidingSpots.Count);
        return hidingSpots[randNumb];
    }

    public Transform FindOpenLocation(Vector3 playerPos, Vector3 catPos)
    {
        List<Transform> tempHideSpots = new List<Transform>();
        List<Transform> finalTempHideSpots = new List<Transform>();
        for (int i = 0; i < hidingSpots.Count; i++)
        {
            Vector3 tempHideSpot = hidingSpots[i].position, tempPlayerPos = playerPos, tempCatPos = catPos;
            tempHideSpot.y = 0;
            tempPlayerPos.y = 0;
            tempCatPos.y = 0;

            if (Mathf.Abs(Vector3.Angle((tempHideSpot - tempCatPos).normalized, (tempPlayerPos - tempCatPos).normalized)) > angleOfPlayerDistance)
            {
                tempHideSpots.Add(hidingSpots[i]);
            }
        }

        for (int i = 0; i < tempHideSpots.Count; i++)
        {
            Vector3 tempHideSpot = hidingSpots[i].position, tempPlayerPos = playerPos, tempCatPos = catPos;
            tempHideSpot.y = 0;
            tempPlayerPos.y = 0;
            tempCatPos.y = 0;

            if (Vector3.Distance(tempHideSpots[i].position, playerPos) > angleOfPlayerDistance)
            {
                finalTempHideSpots.Add(tempHideSpots[i]);
            }
        }

        if (finalTempHideSpots.Count > 0)
        {
            int randNumb = Random.Range(0, finalTempHideSpots.Count);
            return finalTempHideSpots[randNumb];
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

    void CountDownMeow()
    {
        currentMeowBuffer -= Time.deltaTime;

        if (currentMeowBuffer <= 0)
        {
            FindClosestCats();
            currentMeowBuffer = maxMeowTimer.GetRandom();
        }
    }

    public void FindClosestCats()
    {
        if (activeCats.Count == 0)
        {
            return;
        }
        else if (activeCats.Count == 1)
        {
            activeCats[0].Meow();
            return;
        }

        List<CatController> tempCats = new List<CatController>();
        for (int i = 0; i < activeCats.Count; i++)
        {
            if (activeCats[i].catState == CatController.CatState.Hiding)
            {
                tempCats.Add(activeCats[i]);
            }
        }

        if (tempCats.Count == 0)
        {
            return;
        }
        else if (tempCats.Count == 1)
        {
            tempCats[0].Meow();
            return;
        }

        List<CatController> closestCats = new List<CatController>();
        for (int i = 0; i < 2; i++)
        {
            Vector3 tempPlayer = CharacterModel.Instance.gameObject.transform.position, tempCat = tempCats[0].gameObject.transform.position;
            tempPlayer.y = 0;
            tempCat.y = 0;
            float minDist = Vector3.Distance(tempPlayer, tempCat);
            CatController closestCurrentCat = tempCats[0];
            for (int j = 1; j < tempCats.Count; j++)
            {
                tempCat = tempCats[j].gameObject.transform.position;
                tempCat.y = 0;

                if (Vector3.Distance(tempPlayer, tempCat) < minDist)
                {
                    minDist = Vector3.Distance(tempPlayer, tempCat);
                    closestCurrentCat = tempCats[j];
                }
            }

            closestCats.Add(closestCurrentCat);
            tempCats.Remove(closestCurrentCat);
        }

        int randNumb = Random.Range(0, closestCats.Count);
        closestCats[randNumb].Meow();
    }

    private void Update()
    {
        CountDownMeow();
    }
}