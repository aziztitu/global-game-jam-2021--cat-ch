using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : SingletonMonoBehaviour<LevelManager>
{
    [Serializable]
    public class LevelSettings
    {
        public int numCats = 5;
        public int maxTime = 60;
        public CharacterModel characterPrefab;
        public float difficultyLevel = 0.5f;
    }

    public LevelSettings settings;

    public HUDScreenManager hudScreenManager;

    public List<Transform> playerSpawnPoints;

    public Randomizer<Transform> playerSpawnRandomizer;

    public SimpleTimer timer = new SimpleTimer();

    [ReadOnly]
    public bool isGameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        playerSpawnRandomizer = new Randomizer<Transform>(playerSpawnPoints);
        
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameOver)
        {
            if (CharacterModel.Instance.catsFound >= settings.numCats)
            {
                // Found all cats

                isGameOver = true;
            }

            if (!timer.expired)
            {
                timer.Update();
                if (timer.expired)
                {
                    // Out of timed

                    isGameOver = true;
                }
            }
        }
    }

    void Initialize()
    {
        var spawnPoint = playerSpawnRandomizer.GetRandomItem();
        var character = Instantiate(settings.characterPrefab, spawnPoint.position, spawnPoint.rotation);
        ThirdPersonCamera.Instance.SetTargetObject(character.playerTarget);

        for (int i = 0; i < settings.numCats; i++)
        {
            CatManager.Instance.SpawnCat();
        }

        timer = new SimpleTimer(settings.maxTime);
    }
}
