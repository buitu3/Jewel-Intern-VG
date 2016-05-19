using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;
using System.Text;

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
    public Text hiScoreText;
    public Text movesCountText;
    public Text unitBGCountText;

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

    private int star1Score;
    private int star2Score;
    private int star3Score;
    private int hiScore;
    private int moves;
    private int bgCount = 0;

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        Instance = this;

        DOTween.SetTweensCapacity(100, 10);

        star1Score = (int)LevelsManager.Instance.selectedLevelInfoJSON.GetField("1 Star Score").i;
        star2Score = (int)LevelsManager.Instance.selectedLevelInfoJSON.GetField("2 Star Score").i;
        star3Score = (int)LevelsManager.Instance.selectedLevelInfoJSON.GetField("3 Star Score").i;
        moves = (int)LevelsManager.Instance.selectedLevelInfoJSON.GetField("Moves").i;

        string levelHiScoreKey = new StringBuilder("Level " + LevelsManager.Instance.selectedLevel + " High Score").ToString();
        if (PlayerPrefs.HasKey(levelHiScoreKey))
        {
            hiScore = PlayerPrefs.GetInt(levelHiScoreKey);
        }
        else
        {
            hiScore = 0;
        }

    }

    void Start()
    {
        currentState = GameState.idle;

        //scoreTween = DOTween.To(() => Score, x => Score = x, 0, 0.3f).OnUpdate(updateScoreText);
        hiScoreText.text = hiScore.ToString();
        movesCountText.text = moves.ToString();
        bgCount = UnitBGGenerator.Instance.BlueBGCount;
        unitBGCountText.text = bgCount.ToString();
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

    public void updateUnitBGCountText(int unitBGCount)
    {
        unitBGCountText.text = unitBGCount.ToString();
    }

    public void reduceMovesCount()
    {
        if(moves > 0)
        {
            moves--;
            movesCountText.text = moves.ToString();
        }
        else
        {
            print("out of moves");
        }
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

    public void selectLevel()
    {
        Time.timeScale = 1f;
        gameOver();
        SceneManager.LoadScene("Select Level Scene");
    }

    public void gameOver()
    {
        saveGame();        
    }

    void saveGame()
    {
        print("save");
        // Save current level hi-score and star rating
        string levelHiScoreKey = new StringBuilder("Level " + LevelsManager.Instance.selectedLevel + " High Score").ToString();

        // Check if selected has been played before
        if (PlayerPrefs.HasKey(levelHiScoreKey))
        {
            // Save new hi-score if current score is higher than previous hi-score
            if (Score > PlayerPrefs.GetInt(levelHiScoreKey))
            {
                PlayerPrefs.SetInt(levelHiScoreKey, Score);
                print(star1Score);

                rateLevelStar(Score);
            }
        }
        // Save this score as hi-score if the selected level is played for the first time
        else
        {
            PlayerPrefs.SetInt(levelHiScoreKey, Score);

            rateLevelStar(Score);
        }
    }

    void rateLevelStar(int score)
    {
        string levelStarKey = new StringBuilder("Level " + LevelsManager.Instance.selectedLevel + " Star").ToString();
        if (Score > star1Score)
        {
            // Unlock the next level
            if (!PlayerPrefs.HasKey("Unlocked Level"))
            {                
                PlayerPrefs.SetInt("Unlocked Level", LevelsManager.Instance.selectedLevel + 1);
            }
            else
            {
                if (LevelsManager.Instance.selectedLevel == PlayerPrefs.GetInt("Unlocked Level") && 
                    LevelsManager.Instance.selectedLevel != LevelsManager.Instance.maxLevel)
                {
                    PlayerPrefs.SetInt("Unlocked Level", LevelsManager.Instance.selectedLevel + 1);
                }
            }

            // Rate star
            PlayerPrefs.SetInt(levelStarKey, 1);
            //print(LevelsManager.Instance.selectedLevel);
            print("1 star unlocked");
        }
        if (Score > star2Score)
        {
            // Rate star
            PlayerPrefs.SetInt(levelStarKey, 2);
            print("2 star unlocked");
        }
        if (Score > star3Score)
        {
            // Rate star
            PlayerPrefs.SetInt(levelStarKey, 3);
            print("3 star unlocked");
        }
    }
}
