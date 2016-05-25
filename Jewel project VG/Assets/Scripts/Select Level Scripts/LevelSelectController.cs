using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
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
    //public Image SFXImage;
    //public Image muteSFXImage;
    //public Image soundImage;
    //public Image muteSoundImage;

    public Slider SFXSlider;
    public Slider musicSlider;

    public GameObject settingPanel;

    public ScrollRect levelScroll;
    private RectTransform scrollTransform;

    public AudioClip clickSound;

    private bool muteSFX = false;
    private bool muteSound = false;

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

        SFXSlider.value = SoundController.Instance.sfxSource.volume;
        musicSlider.value = SoundController.Instance.musicSource.volume;

        //levelScroll.verticalNormalizedPosition = 0.5f;
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
        scrollTransform = levelScroll.GetComponent<RectTransform>();
        //float normalizePos = scrollTransform.anchorMin.y - _LevelBtnARR[unlockedLevel - 1].GetComponent<RectTransform>().anchoredPosition.y;
        //print(_LevelBtnARR[unlockedLevel - 1].GetComponentsInParent<RectTransform>()[1].transform.GetSiblingIndex());
        //print(_LevelBtnARR[unlockedLevel - 1].GetComponentsInParent<RectTransform>()[1].anchoredPosition.y);
        //print(levelScroll.content.rect.height);

        if (previousLevel != 0)
        {
            float normalizePos = _LevelBtnARR[previousLevel - 1].GetComponentsInParent<RectTransform>()[1].anchoredPosition.y / levelScroll.content.rect.height;
            print(normalizePos);
            levelScroll.verticalNormalizedPosition = normalizePos;
        }
        else
        {
            float normalizePos = _LevelBtnARR[unlockedLevel - 1].GetComponentsInParent<RectTransform>()[1].anchoredPosition.y / levelScroll.content.rect.height;
            print(normalizePos);
            levelScroll.verticalNormalizedPosition = normalizePos;
        }        

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

    public void showSettingsPanel()
    {
        SoundController.Instance.playOneShotClip(clickSound);

        settingPanel.SetActive(true);
        settingPanel.transform.DOScale(new Vector2(0, 0), 0.5f).From().SetUpdate(UpdateType.Normal, true).SetEase(Ease.OutBack);
    }

    public void hideSettingPanel()
    {
        SoundController.Instance.playOneShotClip(clickSound);

        settingPanel.SetActive(false);

        PlayerPrefs.SetFloat("SFX Volume", SoundController.Instance.sfxSource.volume);
        PlayerPrefs.SetFloat("Music Volume", SoundController.Instance.musicSource.volume);
    }

    public void changeSFXVolume(float volume)
    {
        SoundController.Instance.sfxSource.volume = volume;
    }

    public void changeMusicVolume(float volume)
    {
        SoundController.Instance.musicSource.volume = volume;
    }
}
