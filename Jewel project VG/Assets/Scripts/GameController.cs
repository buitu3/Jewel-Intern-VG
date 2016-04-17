using UnityEngine;
using UnityEngine.UI;
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

    [HideInInspector]
    public GameState currentState;

    public enum GameState
    {
        idle = 0,
        focusUnit,
        scanningUnit,
        destroyingUnit,
        regenUnit,
        _statesCount
    }

    private int Score;

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        Instance = this;
    }

    void start()
    {
        currentState = GameState.idle;
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
        DOTween.To(() => Score, x => Score = x, Score += bonusScore, 1f).SetUpdate(UpdateType.Fixed).OnUpdate(updateScoreText);
        //scoreText.text = Score.ToString();
    }

    void updateScoreText()
    {
        scoreText.text = Score.ToString();
    }

    public void pauseGame()
    {
        print("paused");
    }

    
}
