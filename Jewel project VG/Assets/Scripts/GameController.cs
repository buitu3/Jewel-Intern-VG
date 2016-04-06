using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

    //==============================================
    // Constants
    //==============================================

    public static GameController Instance;

    //==============================================
    // Fields
    //==============================================

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

    //==============================================
    // Methods
    //==============================================
}
