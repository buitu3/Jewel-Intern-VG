using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        int previousLevel = LevelsManager.Instance.selectedLevel;
        int unlockedLevel = (int)LevelsManager.Instance.levelsInfoJSON.GetField("Unlocked Level").i;

        for(int i = 0; i < _LevelBtnARR.Length; i++)
        {           
            if(i == unlockedLevel - 1)
            {
                _LevelBtnARR[i].GetComponent<Image>().sprite = previousPlayedSprite;
                _LevelBtnARR[i].interactable = true;
            }
            else if (i < unlockedLevel - 1)
            {
                _LevelBtnARR[i].GetComponent<Image>().sprite = passedSprite;
                _LevelBtnARR[i].interactable = true;
            }

            if (previousLevel != 0 && i == previousLevel - 1)
            {

            }

            int star = (int)LevelsManager.Instance.levelsInfoJSON.GetField("Level " + (i + 1)).GetField("Star").i;
            _LevelBtnARR[i].GetComponent<StarsManager>().updateStar(star);
        }
    }

    //==============================================
    // Methods
    //==============================================

    public void loadMainGame(int level)
    {
        LevelsManager.Instance.selectedLevel = level;
        SceneManager.LoadScene("Main Game Scene");
    }
}
