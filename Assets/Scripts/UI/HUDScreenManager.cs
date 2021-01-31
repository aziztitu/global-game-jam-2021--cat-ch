using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class HUDScreenManager : MonoBehaviour
{
    public RectTransform menuPanel;
    public RectTransform optionPanel;

    public GameObject endS;
    public Text text;
    public TextMeshProUGUI betterText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI catsText;

    public int offset = 100;
    public int optionOffset = 400;
    public float smooth = 0.25f;
    public float delay = 6;

    public static bool gameIsPaused = false;
    public static bool optionsOn = false;

    // Start is called before the first frame update
    void Start()
    {
        DOTween.KillAll();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameIsPaused = !gameIsPaused;
            PauseGame();
        }

        
        betterText.text = text.text;
    }

    void PauseGame()
    {
        Debug.Log("pause");
        menuPanel.DOKill(true);

        if (gameIsPaused)
        {
            Debug.Log("moveright");
            menuPanel.DOAnchorPos(new Vector2(menuPanel.localPosition.x + offset, 0), smooth).SetUpdate(true);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.Log("moveleft");
            
            if(DOTween.TotalPlayingTweens() == 0)
            {
                menuPanel.DOAnchorPos(new Vector2(menuPanel.localPosition.x - offset, 0), smooth).SetUpdate(true);
            }
            Time.timeScale = 1;
        }

        if (optionsOn)
        {
            optionsButtonTweenBack();
        }
    }

    public void resumeButton()
    {
        Debug.Log("moveleft");
        menuPanel.DOKill(true);
        if (DOTween.TotalPlayingTweens() == 0)
        {
            menuPanel.DOAnchorPos(new Vector2(menuPanel.localPosition.x - offset, 0), smooth).SetUpdate(true);
            optionsButtonTweenBack();
            gameIsPaused = !gameIsPaused;
        }
    }

    public void optionsButtonTween()
    {
        if(!optionsOn)
        {
            optionPanel.DOAnchorPos(new Vector2(optionPanel.localPosition.x, optionPanel.localPosition.y + optionOffset), smooth).SetUpdate(true);
            optionsOn = !optionsOn;
        }
    }

    public void optionsButtonTweenBack()
    {
        optionPanel.DOAnchorPos(new Vector2(optionPanel.localPosition.x, optionPanel.localPosition.y - optionOffset), smooth).SetUpdate(true);
        optionsOn = !optionsOn;
    }

    public void toMain()
    {

    }

    public void quitButton()
    {
        Application.Quit();
    }

    //hook up the values here
    public void enableEndscreen()
    {
        endS.SetActive(true);
        text.DOText("A", delay, true, ScrambleMode.All);
        catsText.text = "";
        timerText.text = "";

    }
}