using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MainScreenManager : MonoBehaviour
{
    public RectTransform mainMenu;
    public RectTransform logoTrans;
    public Image logoImage;
    public string swapToLevel = "";

    public int xOffset = 200;
    public int yOffset = 300;
    public float xSmooth = 0.25f;
    public float delay = 1.2f;

    private Vector2 startMenu;
    private Vector2 startMenuOffset;
    private Vector2 logo;
    private Vector2 logoOffset;


    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;
        HelperUtilities.UpdateCursorLock(false);
        fadeIn();
        startMenu = new Vector2(mainMenu.anchoredPosition.x, mainMenu.anchoredPosition.y);
        startMenuOffset = new Vector2(mainMenu.anchoredPosition.x - xOffset, mainMenu.anchoredPosition.y);
        logo = new Vector2(logoTrans.anchoredPosition.x, logoTrans.anchoredPosition.y);
        logoOffset = new Vector2(logoTrans.anchoredPosition.x, logoTrans.anchoredPosition.y + yOffset);
        tweenLeft();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void fadeIn()
    {
        logoImage.DOFade(255, 2);
    }

    public void tweenLeft()
    {
        mainMenu.DOAnchorPos(startMenuOffset, xSmooth).SetDelay(0.25f);
    }

    public void tweenLeftDelay()
    {
        mainMenu.DOAnchorPos(startMenuOffset, xSmooth).SetDelay(delay);
    }

    public void tweenRight()
    {
        mainMenu.DOAnchorPos(startMenu, xSmooth);
    }

    public void tweenUp()
    {
        logoTrans.DOAnchorPos(logoOffset, xSmooth);
    }

    public void tweenDownDelay()
    {
        logoTrans.DOAnchorPos(logo, xSmooth).SetDelay(0.8f);
    }

    public void changeLevel()
    {
        GameManager.Instance.GoToScene(swapToLevel);
    }

    public void quit()
    {
        GameManager.Instance.QuitGame();
    }

}
