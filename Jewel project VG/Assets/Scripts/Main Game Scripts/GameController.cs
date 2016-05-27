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

    public Text gameOverScoreText;
    public Text gameOverHiScoreText;
    public Text gameCompleteScoreText;
    public Text gameCompleteHiScoreText;

    public GameObject gameCompleteStarOne;
    public GameObject gameCompleteStarTwo;
    public GameObject gameCompleteStarThree;

    public GameObject greyPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject gameCompletedPanel;

    public GameObject levelText;
    public Text levelNumber;

    public AudioClip clickSound;

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
        bgCount = UnitBGGenerator.Instance.UnitBGCount;
        unitBGCountText.text = bgCount.ToString();

        scoreSlider.maxValue = star3Score;
    }

    //void Update()
    //{
    //    scoreText.text = Score.ToString();
    //}

    //==============================================
    // Methods
    //==============================================

    #region Update UI Text methods

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
    }

    public void checkIfGameOver()
    {
        if (moves == 0)
        {
            gameOver();
        }
    }

    public bool isGameCompleted()
    {
        if (UnitBGGenerator.Instance.UnitBGCount <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    public void pauseGame()
    {
        SoundController.Instance.playOneShotClip(clickSound);

        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        greyPanel.SetActive(true);

        pausePanel.transform.DOScale(new Vector2(0, 0), 0.5f).From().SetUpdate(UpdateType.Normal, true).SetEase(Ease.OutBack);       
    }

    public void resumeGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        greyPanel.SetActive(false);

        SoundController.Instance.playOneShotClip(clickSound);
    }

    public void restartGame()
    {
        SoundController.Instance.playOneShotClip(clickSound);

        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Game Scene");
    }

    public void loadNextLevel()
    {
        SoundController.Instance.playOneShotClip(clickSound);

        if (LevelsManager.Instance.selectedLevel < LevelsManager.Instance.maxLevel)
        {
            LevelsManager.Instance.selectedLevel++;
            LevelsManager.Instance.selectedLevelInfoJSON = LevelsManager.Instance.levelsInfoJSON.GetField("Level " + LevelsManager.Instance.selectedLevel);

            SceneManager.LoadScene("Main Game Scene");
        }
        else
        {
            SceneManager.LoadScene("Main Game Scene");
        }
    }

    public void selectLevel()
    {
        SoundController.Instance.playOneShotClip(clickSound);

        Time.timeScale = 1f;
        SceneManager.LoadScene("Select Level Scene");
    }

    public void gameOver()
    {
        saveGame();

        gameOverPanel.SetActive(true);
        greyPanel.SetActive(true);

        gameOverPanel.transform.DOScale(new Vector2(0, 0), 0.5f).From().SetUpdate(UpdateType.Normal, true).SetEase(Ease.OutBack);

        gameOverScoreText.text = Score.ToString();
        gameOverHiScoreText.text = hiScore.ToString();

        Time.timeScale = 0f;
    }

    public void gameCompleted()
    {
        saveGame();

        gameCompletedPanel.SetActive(true);
        greyPanel.SetActive(true);

        gameCompletedPanel.transform.DOScale(new Vector2(0, 0), 0.5f).From().SetUpdate(UpdateType.Normal, true).SetEase(Ease.OutBack);

        gameCompleteScoreText.text = Score.ToString();
        gameCompleteHiScoreText.text = hiScore.ToString();

        if (Score > star3Score)
        {
            gameCompleteStarThree.SetActive(true);
            gameCompleteStarTwo.SetActive(true);
            gameCompleteStarOne.SetActive(true);
        }
        else if (Score > star2Score)
        {
            gameCompleteStarTwo.SetActive(true);
            gameCompleteStarOne.SetActive(true);
        }
        else if (Score > star1Score)
        {
            gameCompleteStarOne.SetActive(true);
        }

        Time.timeScale = 0f;
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
            if (Score > PlayerPrefs.GetInt(levelHiScoreKey) && isGameCompleted())
            {
                PlayerPrefs.SetInt(levelHiScoreKey, Score);

                rateLevelStar(Score);

                hiScore = Score;
            }
        }
        // Save this score as hi-score if the selected level is played for the first time
        else
        {
            PlayerPrefs.SetInt(levelHiScoreKey, Score);

            rateLevelStar(Score);

            hiScore = Score;
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

    public IEnumerator showLevelText()
    {
        levelNumber.text = LevelsManager.Instance.selectedLevel.ToString();
        levelText.SetActive(true);
        greyPanel.SetActive(true);
        yield return levelText.transform.DOLocalMoveX(500, 0.8f).SetEase(Ease.OutQuart).From().WaitForCompletion();
        yield return levelText.transform.DOLocalMoveX(-500, 0.8f).SetEase(Ease.InQuart).WaitForCompletion();
        greyPanel.SetActive(false);
        Destroy(levelText);
    }
}
