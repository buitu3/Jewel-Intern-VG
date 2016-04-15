using UnityEngine;
using System.Collections;

public class PossibleChainedDetector : MonoBehaviour {

    //==============================================
    // Constants
    //==============================================

    public static PossibleChainedDetector Instance;

    //==============================================
    // Fields
    //==============================================

    private int[,] _valueARR;

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _valueARR = new int[PuzzleGenerator.Instance._columns, PuzzleGenerator.Instance._rows];

        updateValueARR();
    }

    //==============================================
    // Methods
    //==============================================

    // Sync the scanARR with the current puzzle
    public void updateValueARR()
    {
        //for (int YIndex = 0; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        //{
        //    for (int XIndex = 0; XIndex < PuzzleGenerator.Instance._columns; XIndex++)
        //    {
        //        //_valueARR[XIndex, YIndex] = PuzzleGenerator.Instance._unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._value;
        //        _valueARR[XIndex, YIndex] = ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._value;
        //    }
        //}
    }

    void scanPossibleVerticalChained()
    {

    }
}
