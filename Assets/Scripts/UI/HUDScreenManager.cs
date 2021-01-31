using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class HUDScreenManager : SingletonMonoBehaviour<HUDScreenManager>
{
    public RectTransform menuPanel;
    public RectTransform optionPanel;

    public GameObject endS;
    public Text text;
    public TextMeshProUGUI betterText;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI catsText;
    public TextMeshProUGUI scoreText;
    public GameObject popcat;

    public int offset = 300;
    public int optionOffset = 400;
    public float smooth = 0.25f;
    public float delay = 6;

    public float endscreenDelay = 6;

    public static bool gameIsPaused = false;
    public static bool optionsOn = false;

    private Vector2 startMenu;
    private Vector2 startMenuOffset;
    private Vector2 startOption;
    private Vector2 startOptionOffset;

    // Start is called before the first frame update
    void Start()
    {
        startMenu = new Vector2(menuPanel.anchoredPosition.x, menuPanel.anchoredPosition.y);
        startMenuOffset = new Vector2(menuPanel.anchoredPosition.x + offset, 0);
        startOption = new Vector2(optionPanel.anchoredPosition.x, optionPanel.anchoredPosition.y);
        startOptionOffset = new Vector2(optionPanel.anchoredPosition.x, optionPanel.anchoredPosition.y + optionOffset);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused || !CharacterModel.Instance.characterAnimEventHandler.isInDivingState)
            {
                gameIsPaused = !gameIsPaused;
                PauseGame();
            }
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
            menuPanel.DOAnchorPos(startMenuOffset, smooth).SetUpdate(true);
            Time.timeScale = 0f;
            HelperUtilities.UpdateCursorLock(false);
        }
        else
        {
            Debug.Log("moveleft");

            if (DOTween.TotalPlayingTweens() == 0)
            {
                menuPanel.DOAnchorPos(startMenu, smooth).SetUpdate(true);
            }

            Time.timeScale = 1;
            HelperUtilities.UpdateCursorLock(true);
        }

        if (optionsOn)
        {
            optionsButtonTweenBack();
        }
    }

    public void resumeButton()
    {
        Debug.Log("moveleft");
        gameIsPaused = !gameIsPaused;

        PauseGame();

        /*if (DOTween.TotalPlayingTweens() == 0)
        {
            menuPanel.DOAnchorPos(startMenu, smooth).SetUpdate(true);
            optionsButtonTweenBack();
        }*/
    }

    public void optionsButtonTween()
    {
        if (!optionsOn)
        {
            optionPanel.DOAnchorPos(startOptionOffset, smooth).SetUpdate(true);
            optionsOn = !optionsOn;
        }
    }

    public void optionsButtonTweenBack()
    {
        optionPanel.DOAnchorPos(startOption, smooth).SetUpdate(true);
        optionsOn = !optionsOn;
    }

    public void restart()
    {
        GameManager.Instance.RestartCurrentScene();
    }

    public void toMain()
    {
        GameManager.Instance.GoToMainMenu();
    }

    public void quitButton()
    {
        GameManager.Instance.QuitGame();
    }

    //hook up the values here
    public void enableEndscreen(string title)
    {
        popcat.SetActive(true);
        titleText.text = title;
    }

    public void showEndStats()
    {
        Time.timeScale = 0f;
        HelperUtilities.UpdateCursorLock(false);

        endS.SetActive(true);

        var timeSpent = LevelManager.Instance.timer.elapsedTimeClamped;

        var timeLeft = LevelManager.Instance.timer.durationRange.selected - timeSpent;

        timerText.text = $"{(timeSpent / 60):#0}:{(timeSpent % 60):00}";
        catsText.text = $"{CharacterModel.Instance.catsFound}/{LevelManager.Instance.settings.numCats}";

        int finalScore = (int)(CharacterModel.Instance.catsFound * LevelManager.Instance.pointsPerCatFound +
                               timeLeft * LevelManager.Instance.pointsPerSecondLeft);
        text.DOText($"{finalScore}", delay, true, ScrambleMode.All).SetUpdate(true).Play();
    }
}