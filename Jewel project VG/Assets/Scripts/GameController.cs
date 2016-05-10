using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class GameController : MonoBehaviour {

    //==============================================
    // Constants
    //==============================================

    public static GameController Instance;

    //==============================================
    // Fields
    //==============================================

    public Slider scoreSlider;
    public Text scoreText;

    public GameObject pausePanel;

    [HideInInspector]
    public GameState currentState;

    public enum GameState
    {
        idle = 0,
        focusUnit,
        scanningUnit,
        destroyingUnit,
        regenUnit,
        shufflePuzzle,
        _statesCount
    }

    private Tweener scoreTween;

    private int Score;

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        Instance = this;

        DOTween.SetTweensCapacity(100, 10);
    }

    void start()
    {
        currentState = GameState.idle;

        //scoreTween = DOTween.To(() => Score, x => Score = x, 0, 0.3f).OnUpdate(updateScoreText);
    }

    //void Update()
    //{
    //    scoreText.text = Score.ToString();
    //}

    //==============================================
    // Methods
    //==============================================

    public void updateScore(int bonusScore)
    {
        //Score += bonusScore;
        scoreSlider.DOValue(Score += bonusScore, 1f, true);
        //DOTween.To(() => Score, x => Score = x, Score += bonusScore, 0.3f).OnUpdate(updateScoreText);
        scoreTween = DOTween.To(() => Score, x => Score = x, Score += bonusScore, 0.3f).OnUpdate(updateScoreText);
        //scoreTween.ChangeEndValue(Score += bonusScore);
        //scoreText.text = Score.ToString();
    }

    void updateScoreText()
    {
        scoreText.text = Score.ToString();
    }

    public void pauseGame()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    public void resumeGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    public void restartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Game Scene");
    }
}
