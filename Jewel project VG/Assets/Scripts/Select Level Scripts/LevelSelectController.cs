using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;

public class LevelSelectController : MonoBehaviour {

    //==============================================
    // Constants
    //==============================================

    public static LevelSelectController Instance;

    //==============================================
    // Fields
    //==============================================

    public Button[] _LevelBtnARR;

    public Sprite lockedSprite;
    public Sprite passedSprite;
    public Sprite previousPlayedSprite;

    public Image playerImage;

    public GameObject panel;

    public AudioClip clickSound;

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        Instance = this;

    }

    void Start()
    {
        loadLevelsInfo();
    }

    //==============================================
    // Methods
    //==============================================

    void loadLevelsInfo()
    {
        int previousLevel = LevelsManager.Instance.selectedLevel;

        int unlockedLevel = 1;
        if (PlayerPrefs.HasKey("Unlocked Level"))
        {
            unlockedLevel = PlayerPrefs.GetInt("Unlocked Level");
        }

        //panel.transform.position = _LevelBtnARR[unlockedLevel - 1].transform.position;

        // Unlock all unlocked levels
        for (int i = 0; i <= unlockedLevel - 1; i++)
        {
            if (i == unlockedLevel - 1)
            {
                if (previousLevel != 0)
                {
                    //_LevelBtnARR[previousLevel - 1].GetComponent<Image>().sprite = previousPlayedSprite;
                    //_LevelBtnARR[previousLevel - 1].interactable = true;

                    playerImage.transform.position = _LevelBtnARR[previousLevel - 1].transform.position;

                    //if (previousLevel - 1 != i)
                    //{
                    //    _LevelBtnARR[i].GetComponent<Image>().sprite = passedSprite;
                    //    _LevelBtnARR[i].interactable = true;
                    //}
                }
                else
                {
                    //_LevelBtnARR[i].GetComponent<Image>().sprite = previousPlayedSprite;
                    //_LevelBtnARR[i].interactable = true;

                    playerImage.transform.position = _LevelBtnARR[i].transform.position;
                }

                _LevelBtnARR[i].GetComponent<Image>().sprite = previousPlayedSprite;
                _LevelBtnARR[i].interactable = true;

            }
            else if (i < unlockedLevel - 1)
            {
                _LevelBtnARR[i].GetComponent<Image>().sprite = passedSprite;
                _LevelBtnARR[i].interactable = true;
            }

            // Show star rating of each level
            string levelStarKey = new StringBuilder("Level " + (i + 1) + " Star").ToString();
            int star = 0;
            if (PlayerPrefs.HasKey(levelStarKey))
            {
                star = PlayerPrefs.GetInt(levelStarKey);                
            }
            //int star = (int)LevelsManager.Instance.levelsInfoJSON.GetField("Level " + (i + 1)).GetField("Star").i;
            _LevelBtnARR[i].GetComponent<StarsManager>().updateStar(star);
        }
    }

    public void loadMainGame(int level)
    {
        SoundController.Instance.playOneShotClip(clickSound);

        LevelsManager.Instance.selectedLevel = level;
        LevelsManager.Instance.selectedLevelInfoJSON = LevelsManager.Instance.levelsInfoJSON.GetField("Level " + level);
        SceneManager.LoadScene("Main Game Scene");
    }
}
