using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class CharacterManager : MonoBehaviour
{
    public List<Button> characters = new List<Button>();
    public Button char1;
    public Button char2;
    public Button char3;
    public Button char4;

    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void buttonID(int id)
    {
        switch(id)
        {
            case 0:
                text.text = "agg";
                break;
            case 1:
                text.text = "";
                break;
            default:
                text.text = "player not found";
                break;
        }    
    }
    
    public void changeSceneButton(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
