using UnityEngine;
using System.Collections;

public class LevelsManager : MonoBehaviour {

    //==============================================
    // Constants
    //==============================================

    public static LevelsManager Instance;

    //==============================================
    // Fields
    //==============================================

    public TextAsset levelsInfo;
    [HideInInspector]
    public JSONObject levelsInfoJSON;
    public JSONObject selectedLevelInfoJSON;

    //[HideInInspector]
    public int selectedLevel = 0;

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        string levelsInfoString = levelsInfo.text;
        levelsInfoJSON = new JSONObject(levelsInfoString);
    }

    

    //==============================================
    // Methods
    //==============================================
}
