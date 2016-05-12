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

    public int selectedLevel;

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
    }

    //==============================================
    // Methods
    //==============================================
}
