using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class CharacterManager : MonoBehaviour
{
    public List<LevelManager.LevelSettings> settings;

    public List<Button> characters = new List<Button>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void Select(int charId)
    {
        GameManager.Instance.selectedLevelSettings = settings[charId];
        GameManager.Instance.GoToMainLevel();
    }
}
