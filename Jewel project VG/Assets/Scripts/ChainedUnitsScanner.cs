using UnityEngine;
using System.Collections;

[System.Serializable]
public class ScanUnit
{
    public int _value;
    public int pointMultiplier = 0;
    public bool _isChained;

    public ScanUnit(int value)
    {
        _value = value;
    }
}

public class ChainedUnitsScanner : MonoBehaviour {

    //==============================================
    // Constants
    //==============================================

    public static ChainedUnitsScanner Instance;

    //==============================================
    // Fields
    //==============================================

    public ScanUnit[,] _scanUnitARR;

    private bool chainedUnits;

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Init scanUnit Array 
        _scanUnitARR = new ScanUnit[PuzzleGenerator.Instance._columns, PuzzleGenerator.Instance._rows];
        for (int YIndex = 0; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        {
            for (int XIndex = 0; XIndex < PuzzleGenerator.Instance._columns; XIndex++)
            {
                _scanUnitARR[XIndex, YIndex] = new ScanUnit(PuzzleGenerator.Instance._valueARR[XIndex, YIndex]);
            }
        }

        //scanThreeChainedAll();
    }

    //==============================================
    // Methods
    //==============================================

    public void updateScanARR()
    {
        for (int YIndex = 0; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        {
            for (int XIndex = 0; XIndex < PuzzleGenerator.Instance._columns; XIndex++)
            {
                _scanUnitARR[XIndex, YIndex]._value = PuzzleGenerator.Instance._unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._value;
                _scanUnitARR[XIndex, YIndex].pointMultiplier = 0;
                _scanUnitARR[XIndex, YIndex]._isChained = false;
            }
        }
    }

    public void scanAll()
    {
        chainedUnits = false;

        // Scan whole puzzle for chained Units
        for (int YIndex = 0; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        {
            for (int XIndex = 0; XIndex < PuzzleGenerator.Instance._columns; XIndex++)
            {
                scanHorizontalThreeChainedSwap(XIndex, YIndex);
                scanVerticalThreeChainedSwap(XIndex, YIndex);

                // If there are chained Units
                if (chainedUnits)
                {
                    StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle());
                }
                else
                {
                    GameController.Instance.currentState = GameController.GameState.idle;
                }
            }
        }
    }

    public IEnumerator scanUnitsAfterSwap(GameObject focusedUnit, GameObject otherUnit)
    {
        yield return new WaitForSeconds(InputHandler.Instance.swapTime);
        chainedUnits = false;
        UnitInfo focusedUnitInfo = focusedUnit.GetComponent<UnitInfo>();
        UnitInfo otherUnitInfo = otherUnit.GetComponent<UnitInfo>();

        // Scan focused Unit chain
        scanHorizontalThreeChainedSwap(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
        scanVerticalThreeChainedSwap(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
        // Scan other Unit chain
        scanHorizontalThreeChainedSwap(otherUnitInfo._XIndex, otherUnitInfo._YIndex);
        scanVerticalThreeChainedSwap(otherUnitInfo._XIndex, otherUnitInfo._YIndex);

        // If there are chained units
        if (chainedUnits)
        {
            StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle());
        }
        else
        {
            GameController.Instance.currentState = GameController.GameState.idle;
        }
    }

    void scanHorizontalThreeChainedSwap(int XIndex, int YIndex) 
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

        // Right Chained
        if (XIndex > 1)
        {
            if (_scanUnitARR[XIndex - 2, YIndex]._value == scanUnitValue
                    && _scanUnitARR[XIndex - 1, YIndex]._value == scanUnitValue
                    && !_scanUnitARR[XIndex - 2, YIndex]._isChained
                    && !_scanUnitARR[XIndex - 1, YIndex]._isChained
                    && !_scanUnitARR[XIndex    , YIndex]._isChained)
            {
                _scanUnitARR[XIndex - 2, YIndex]._isChained = true;
                _scanUnitARR[XIndex - 1, YIndex]._isChained = true;
                _scanUnitARR[XIndex    , YIndex]._isChained = true;
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex]);
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex]);
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex]);
                //PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].SetActive(false);
                //PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
                //PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);

                chainedUnits = true;
            }
        }

        // Middle Chained
        if (XIndex > 0 && XIndex < PuzzleGenerator.Instance._columns - 1)
        {
            if (_scanUnitARR[XIndex - 1, YIndex]._value == scanUnitValue
                    && _scanUnitARR[XIndex + 1, YIndex]._value == scanUnitValue
                    && !_scanUnitARR[XIndex - 1, YIndex]._isChained
                    && !_scanUnitARR[XIndex, YIndex]._isChained
                    && !_scanUnitARR[XIndex + 1, YIndex]._isChained)
            {
                _scanUnitARR[XIndex - 1, YIndex]._isChained = true;
                _scanUnitARR[XIndex, YIndex]._isChained = true;
                _scanUnitARR[XIndex + 1, YIndex]._isChained = true;
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex]);
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex]);

                chainedUnits = true;
            }
        }

        // Left Chained
        if (XIndex < PuzzleGenerator.Instance._columns - 2)
        {
            if (_scanUnitARR[XIndex + 1, YIndex]._value == scanUnitValue
                    && _scanUnitARR[XIndex + 2, YIndex]._value == scanUnitValue
                    && !_scanUnitARR[XIndex    , YIndex]._isChained
                    && !_scanUnitARR[XIndex + 1, YIndex]._isChained
                    && !_scanUnitARR[XIndex + 2, YIndex]._isChained)
            {
                _scanUnitARR[XIndex    , YIndex]._isChained = true;
                _scanUnitARR[XIndex + 1, YIndex]._isChained = true;
                _scanUnitARR[XIndex + 2, YIndex]._isChained = true;
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex]);
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex]);
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex + 2, YIndex]);

                chainedUnits = true;
            }
        }
    }


    void scanVerticalThreeChainedSwap(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

        // Down Chained
        if (YIndex > 1)
        {
            if (_scanUnitARR[XIndex, YIndex - 2]._value == scanUnitValue
                    && _scanUnitARR[XIndex, YIndex - 1]._value == scanUnitValue
                    && !_scanUnitARR[XIndex, YIndex - 2]._isChained
                    && !_scanUnitARR[XIndex, YIndex - 1]._isChained
                    && !_scanUnitARR[XIndex, YIndex   ]._isChained)
            {
                _scanUnitARR[XIndex, YIndex - 2]._isChained = true;
                _scanUnitARR[XIndex, YIndex - 1]._isChained = true;
                _scanUnitARR[XIndex, YIndex   ]._isChained = true;
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 2]);
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1]);
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex   ]);

                chainedUnits = true;
            }
        }

        // Middle Chained
        if (YIndex > 0 && YIndex < PuzzleGenerator.Instance._columns - 1)
        {
            if (_scanUnitARR[XIndex, YIndex - 1]._value == scanUnitValue
                    && _scanUnitARR[XIndex, YIndex + 1]._value == scanUnitValue
                    && !_scanUnitARR[XIndex, YIndex - 1]._isChained
                    && !_scanUnitARR[XIndex, YIndex   ]._isChained
                    && !_scanUnitARR[XIndex, YIndex + 1]._isChained)
            {
                _scanUnitARR[XIndex, YIndex - 1]._isChained = true;
                _scanUnitARR[XIndex, YIndex   ]._isChained = true;
                _scanUnitARR[XIndex, YIndex + 1]._isChained = true;
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1]);
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex   ]);
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1]);

                chainedUnits = true;
            }
        }

        // Up Chained
        if (YIndex < PuzzleGenerator.Instance._columns - 2)
        {
            if (_scanUnitARR[XIndex, YIndex + 1]._value == scanUnitValue
                    && _scanUnitARR[XIndex, YIndex + 2]._value == scanUnitValue
                    && !_scanUnitARR[XIndex, YIndex]._isChained
                    && !_scanUnitARR[XIndex, YIndex + 1]._isChained
                    && !_scanUnitARR[XIndex, YIndex + 2]._isChained)
            {
                _scanUnitARR[XIndex, YIndex]._isChained = true;
                _scanUnitARR[XIndex, YIndex + 1]._isChained = true;
                _scanUnitARR[XIndex, YIndex + 2]._isChained = true;
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1]);
                Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 2]);

                chainedUnits = true;
            }
        }
    }

}
