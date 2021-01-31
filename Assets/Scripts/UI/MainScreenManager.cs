using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MainScreenManager : MonoBehaviour
{
    public RectTransform mainMenu;
    public string swapToLevel = "";

    public int xOffset = 200;
    public float xSmooth = 0.25f;
    public float delay = 0.6f;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;
        HelperUtilities.UpdateCursorLock(false);
        mainMenu.DOAnchorPos(new Vector2(mainMenu.localPosition.x - xOffset, mainMenu.localPosition.y), xSmooth).SetDelay(0.25f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void tweenLeft()
    {
        mainMenu.DOAnchorPos(new Vector2(mainMenu.localPosition.x - xOffset, mainMenu.localPosition.y), xSmooth);
    }

    public void tweenLeftDelay()
    {
        mainMenu.DOAnchorPos(new Vector2(mainMenu.localPosition.x - xOffset, mainMenu.localPosition.y), xSmooth).SetDelay(1);
    }

    public void tweenRight()
    {
        mainMenu.DOAnchorPos(new Vector2(mainMenu.localPosition.x + xOffset, mainMenu.localPosition.y), xSmooth);
    }

    public void changeLevel()
    {
        SceneManager.LoadScene(swapToLevel);
    }

    public void quit()
    {
        Application.Quit();
    }

}
