using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PauseScreenManager : MonoBehaviour
{
    public RectTransform menuPanel;
    public RectTransform optionPanel;

    public int offset = 100;
    public int optionOffset = 400;
    public float smooth = 0.25f;

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

    public void quitButton()
    {
        Application.Quit();
    }
}