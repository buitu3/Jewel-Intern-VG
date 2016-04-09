using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    }

    //==============================================
    // Methods
    //==============================================

    // Sync the scanARR with the current puzzle
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

    //Scan all Units in the puzzle
    public void scanAll()
    {
        updateScanARR();
        chainedUnits = false;

        // Scan whole puzzle for chained Units
        for (int YIndex = 0; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        {
            for (int XIndex = 0; XIndex < PuzzleGenerator.Instance._columns; XIndex++)
            {
                //scanHorizontalThreeChainedSwap(XIndex, YIndex);
                //scanVerticalThreeChainedSwap(XIndex, YIndex);
                scanUnit(XIndex, YIndex);
            }
        }
        // If there are chained Units
        if (chainedUnits)
        {
            StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle());
            //PuzzleGenerator.Instance.reOrganizePuzzle();
        }
        else
        {
            StartCoroutine(scanUnitsAfterSwap(new GameObject(), new GameObject()));
            GameController.Instance.currentState = GameController.GameState.idle;
        }
    }
    
    // Scan two swapped Units
    public IEnumerator scanUnitsAfterSwap(GameObject focusedUnit, GameObject otherUnit)
    {
        yield return new WaitForSeconds(InputHandler.Instance.swapTime);

        UnitInfo focusedUnitInfo = focusedUnit.GetComponent<UnitInfo>();
        UnitInfo otherUnitInfo = otherUnit.GetComponent<UnitInfo>();

        // Check if swapped unit is the special "Destroy all" type
        // If true destroy all jewels that has the same type with the other swapped unit
        if (focusedUnitInfo._value == PuzzleGenerator.Instance.Unit.Length - 1)
        {
            destroyAllUnitsOfType(otherUnitInfo._value);
            disableUnit(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
            StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle());
        }
        else if (otherUnitInfo._value == PuzzleGenerator.Instance.Unit.Length - 1)
        {
            destroyAllUnitsOfType(focusedUnitInfo._value);
            disableUnit(otherUnitInfo._XIndex, otherUnitInfo._YIndex);
            StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle());
        }
        else
        {
            chainedUnits = false;

            // Scan focused Unit chain
            scanUnit(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
            // Scan other Unit chain
            scanUnit(otherUnitInfo._XIndex, focusedUnitInfo._YIndex);

            // If there are chained units
            if (chainedUnits)
            {
                StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle());
                //PuzzleGenerator.Instance.reOrganizePuzzle();
            }
            else
            {
                // Swap Units back
                yield return new WaitForSeconds(0.1f);
                InputHandler.SwapType swapType = InputHandler.Instance.checkLocalUnits(focusedUnit, otherUnit);
                InputHandler.Instance.swapUnits(focusedUnit, otherUnit, swapType);
                yield return new WaitForSeconds(InputHandler.Instance.swapTime);
                GameController.Instance.currentState = GameController.GameState.idle;
            }
        }
    }
    
    public void disableUnit(int XIndex, int YIndex)
    {
        PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
        _scanUnitARR[XIndex, YIndex]._isChained = true;
    }

    void destroyAllUnitsOfType(int type)
    {
        List<int> unitsXIndex = new List<int>();
        List<int> unitsYIndex = new List<int>();

        for (int YIndex = 0; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        {
            for (int XIndex = 0; XIndex < PuzzleGenerator.Instance._columns; XIndex++)
            {
                if (!_scanUnitARR[XIndex, YIndex]._isChained && _scanUnitARR[XIndex, YIndex]._value == type)
                {
                    //_scanUnitARR[XIndex, YIndex]._isChained = true;
                    //PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
                    //disableUnit(XIndex, YIndex);

                    unitsXIndex.Add(XIndex);
                    unitsYIndex.Add(YIndex);
                }
            }
        }
        destroyUnits(unitsXIndex, unitsYIndex);
    }

    void destroyAllUnitOfCol(int col)
    {
        List<int> unitsXIndex = new List<int>();
        List<int> unitsYIndex = new List<int>();

        for (int YIndex = 0 ; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        {
            if (!_scanUnitARR[col, YIndex]._isChained)
            {
                //disableUnit(col, YIndex);
                unitsXIndex.Add(col);
                unitsYIndex.Add(YIndex);
            }
        }
        destroyUnits(unitsXIndex, unitsYIndex);
    }

    void destroyUnits(List<int> unitsXIndex, List<int> unitsYIndex)
    {
        //if (_scanUnitARR[targetXIndex, targetYIndex]._value == PuzzleGenerator.Instance.Unit.Length)
        //{
        //    destroyAllUnitsOfType(_scanUnitARR[localsXIndex[0], localsYIndex[0]]._value);
        //}
        for (int i = 0; i < unitsYIndex.Count - 1; i++)
        {
            print(i);           
            switch (PuzzleGenerator.Instance._unitARR[unitsXIndex[i], unitsYIndex[i]].GetComponent<UnitInfo>()._unitEff)
            {
                case UnitInfo.SpecialEff.vLightning:
                    {
                        destroyAllUnitOfCol(unitsXIndex[i]);
                        break;
                    }

                case UnitInfo.SpecialEff.noEff:
                    {
                        disableUnit(unitsXIndex[i], unitsYIndex[i]);
                        break;
                    }

            }

        }
    }

    // Scan unit that is located in the specific index
    void scanUnit(int XIndex, int YIndex)
    {
        //scanHorizontalThreeChainedSwap(XIndex, YIndex);
        //scanVerticalThreeChainedSwap(XIndex, YIndex);
        
        // Horizontal 5 chained
        if (isRightHorizontalThreeChained(XIndex, YIndex) && isLeftHorizontalThreeChained(XIndex, YIndex))
        {
            // Disable chained Units
            _scanUnitARR[XIndex - 2, YIndex]._isChained = true;
            _scanUnitARR[XIndex -1, YIndex]._isChained = true;
            _scanUnitARR[XIndex   , YIndex]._isChained = true;
            _scanUnitARR[XIndex + 1, YIndex]._isChained = true;
            _scanUnitARR[XIndex + 2, YIndex]._isChained = true;
            PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex + 2, YIndex].SetActive(false);

            // Init special Unit
            PuzzleGenerator.Instance.upgradeUnit(XIndex, YIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Vertical 5 Chained
        if (isUpVerticalThreeChained(XIndex, YIndex) && isDownVerticalThreeChained(XIndex, YIndex))
        {
            // Disable chained Units
            _scanUnitARR[XIndex, YIndex - 2]._isChained = true;
            _scanUnitARR[XIndex, YIndex - 1]._isChained = true;
            _scanUnitARR[XIndex, YIndex   ]._isChained = true;
            _scanUnitARR[XIndex, YIndex + 1]._isChained = true;
            _scanUnitARR[XIndex, YIndex + 2]._isChained = true;
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 2].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 2].SetActive(false);

            // Init special Unit
            PuzzleGenerator.Instance.upgradeUnit(XIndex, YIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Down Left L 5 Chained
        if (isDownVerticalThreeChained(XIndex, YIndex) && isLeftHorizontalThreeChained(XIndex, YIndex))
        {
            _scanUnitARR[XIndex, YIndex - 2]._isChained = true;
            _scanUnitARR[XIndex, YIndex - 1]._isChained = true;
            _scanUnitARR[XIndex, YIndex]._isChained = true;
            _scanUnitARR[XIndex - 2, YIndex    ]._isChained = true;
            _scanUnitARR[XIndex - 1, YIndex    ]._isChained = true;
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 2].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex    ].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex    ].SetActive(false);

            // Init special Unit
            PuzzleGenerator.Instance.upgradeUnit(XIndex, YIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Up Left L 5 Chained
        if (isUpVerticalThreeChained(XIndex, YIndex) && isLeftHorizontalThreeChained(XIndex, YIndex))
        {
            _scanUnitARR[XIndex - 2, YIndex]._isChained = true;
            _scanUnitARR[XIndex - 1, YIndex]._isChained = true;
            _scanUnitARR[XIndex   , YIndex]._isChained = true;
            _scanUnitARR[XIndex   , YIndex + 1]._isChained = true;
            _scanUnitARR[XIndex   , YIndex + 2]._isChained = true;
            PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex    , YIndex + 1].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex    , YIndex + 2].SetActive(false);

            // Init special Unit
            PuzzleGenerator.Instance.upgradeUnit(XIndex, YIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Up right L 5 Chained
        if (isUpVerticalThreeChained(XIndex, YIndex) && isRightHorizontalThreeChained(XIndex, YIndex))
        {
            _scanUnitARR[XIndex    , YIndex]._isChained = true;
            _scanUnitARR[XIndex + 1, YIndex]._isChained = true;
            _scanUnitARR[XIndex + 2, YIndex]._isChained = true;
            _scanUnitARR[XIndex   , YIndex + 1]._isChained = true;
            _scanUnitARR[XIndex   , YIndex + 2]._isChained = true;
            PuzzleGenerator.Instance._unitARR[XIndex    , YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex + 2, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex   , YIndex + 1].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex   , YIndex + 2].SetActive(false);

            // Init special Unit
            PuzzleGenerator.Instance.upgradeUnit(XIndex, YIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Down right L 5 Chained
        if (isDownVerticalThreeChained(XIndex, YIndex) && isRightHorizontalThreeChained(XIndex, YIndex))
        {
            _scanUnitARR[XIndex, YIndex]._isChained = true;
            _scanUnitARR[XIndex + 1, YIndex]._isChained = true;
            _scanUnitARR[XIndex + 2, YIndex]._isChained = true;
            _scanUnitARR[XIndex, YIndex - 1]._isChained = true;
            _scanUnitARR[XIndex, YIndex - 2]._isChained = true;
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex + 2, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 2].SetActive(false);

            // Init special Unit
            PuzzleGenerator.Instance.upgradeUnit(XIndex, YIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Down T 5 Chained
        if (isDownVerticalThreeChained(XIndex, YIndex) && isMiddleHorizontalThreeChained(XIndex, YIndex))
        {
            _scanUnitARR[XIndex, YIndex - 1]._isChained = true;
            _scanUnitARR[XIndex, YIndex - 2]._isChained = true;
            _scanUnitARR[XIndex - 1, YIndex]._isChained = true;
            _scanUnitARR[XIndex, YIndex    ]._isChained = true;
            _scanUnitARR[XIndex + 1, YIndex]._isChained = true;
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 2].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);

            // Init special Unit
            PuzzleGenerator.Instance.upgradeUnit(XIndex, YIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Left T 5 Chained
        if (isMiddleVerticalThreeChained(XIndex, YIndex) && isLeftHorizontalThreeChained(XIndex, YIndex))
        {
            _scanUnitARR[XIndex - 1, YIndex]._isChained = true;
            _scanUnitARR[XIndex - 2, YIndex]._isChained = true;
            _scanUnitARR[XIndex   , YIndex]._isChained = true;
            _scanUnitARR[XIndex, YIndex - 1]._isChained = true;
            _scanUnitARR[XIndex, YIndex + 1]._isChained = true;
            PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex   , YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex   , YIndex - 1].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex   , YIndex + 1].SetActive(false);

            // Init special Unit
            PuzzleGenerator.Instance.upgradeUnit(XIndex, YIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Up T 5 Chained
        if (isUpVerticalThreeChained(XIndex, YIndex) && isMiddleHorizontalThreeChained(XIndex, YIndex))
        {
            _scanUnitARR[XIndex - 1, YIndex]._isChained = true;
            _scanUnitARR[XIndex   , YIndex]._isChained = true;
            _scanUnitARR[XIndex + 1, YIndex]._isChained = true;
            _scanUnitARR[XIndex, YIndex + 1]._isChained = true;
            _scanUnitARR[XIndex, YIndex + 2]._isChained = true;
            PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex    , YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 2].SetActive(false);

            // Init special Unit
            PuzzleGenerator.Instance.upgradeUnit(XIndex, YIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Right T 5 Chained
        if (isMiddleVerticalThreeChained(XIndex, YIndex) && isRightHorizontalThreeChained(XIndex, YIndex))
        {
            _scanUnitARR[XIndex   , YIndex - 1]._isChained = true;
            _scanUnitARR[XIndex   , YIndex]._isChained = true;
            _scanUnitARR[XIndex + 1, YIndex]._isChained = true;
            _scanUnitARR[XIndex + 2, YIndex]._isChained = true;
            _scanUnitARR[XIndex, YIndex + 1]._isChained = true;
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex + 2, YIndex].SetActive(false);
            PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].SetActive(false);

            // Init special Unit
            PuzzleGenerator.Instance.upgradeUnit(XIndex, YIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }
    }

    #region new Scan chained unit methods

    bool isLeftHorizontalThreeChained(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

        if (XIndex > 1)
        {
            if (_scanUnitARR[XIndex - 2, YIndex]._value == scanUnitValue
                    && _scanUnitARR[XIndex - 1, YIndex]._value == scanUnitValue
                    && !_scanUnitARR[XIndex - 2, YIndex]._isChained
                    && !_scanUnitARR[XIndex - 1, YIndex]._isChained
                    && !_scanUnitARR[XIndex, YIndex]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isMiddleHorizontalThreeChained(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

        if (XIndex > 0 && XIndex < PuzzleGenerator.Instance._columns - 1)
        {
            if (_scanUnitARR[XIndex - 1, YIndex]._value == scanUnitValue
                    && _scanUnitARR[XIndex + 1, YIndex]._value == scanUnitValue
                    && !_scanUnitARR[XIndex - 1, YIndex]._isChained
                    && !_scanUnitARR[XIndex, YIndex]._isChained
                    && !_scanUnitARR[XIndex + 1, YIndex]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isRightHorizontalThreeChained(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

        if (XIndex < PuzzleGenerator.Instance._columns - 2)
        {
            if (_scanUnitARR[XIndex + 1, YIndex]._value == scanUnitValue
                    && _scanUnitARR[XIndex + 2, YIndex]._value == scanUnitValue
                    && !_scanUnitARR[XIndex, YIndex]._isChained
                    && !_scanUnitARR[XIndex + 1, YIndex]._isChained
                    && !_scanUnitARR[XIndex + 2, YIndex]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isDownVerticalThreeChained(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

        if (YIndex > 1)
        {
            if (_scanUnitARR[XIndex, YIndex - 2]._value == scanUnitValue
                    && _scanUnitARR[XIndex, YIndex - 1]._value == scanUnitValue
                    && !_scanUnitARR[XIndex, YIndex - 2]._isChained
                    && !_scanUnitARR[XIndex, YIndex - 1]._isChained
                    && !_scanUnitARR[XIndex, YIndex]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isMiddleVerticalThreeChained(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

        if (YIndex > 0 && YIndex < PuzzleGenerator.Instance._rows - 1)
        {
            if (_scanUnitARR[XIndex, YIndex - 1]._value == scanUnitValue
                    && _scanUnitARR[XIndex, YIndex + 1]._value == scanUnitValue
                    && !_scanUnitARR[XIndex, YIndex - 1]._isChained
                    && !_scanUnitARR[XIndex, YIndex]._isChained
                    && !_scanUnitARR[XIndex, YIndex + 1]._isChained)
            {
                return true;
            }
        }       
        return false;
    }

    bool isUpVerticalThreeChained(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

        if (YIndex < PuzzleGenerator.Instance._rows - 2)
        {
            if (_scanUnitARR[XIndex, YIndex + 1]._value == scanUnitValue
                    && _scanUnitARR[XIndex, YIndex + 2]._value == scanUnitValue
                    && !_scanUnitARR[XIndex, YIndex]._isChained
                    && !_scanUnitARR[XIndex, YIndex + 1]._isChained
                    && !_scanUnitARR[XIndex, YIndex + 2]._isChained)
            {
                return true;
            }            
        }
        return false;
    }

    #endregion

    #region old Scan chained unit methods
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
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex]);
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex]);
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex].SetActive(false);
                PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].SetActive(false);
                PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);

                chainedUnits = true;
                return;
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
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex]);
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex]);
                PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].SetActive(false);
                PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
                PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);

                chainedUnits = true;
                return;
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
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex]);
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex + 2, YIndex]);
                PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
                PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);
                PuzzleGenerator.Instance._unitARR[XIndex + 2, YIndex].SetActive(false);

                chainedUnits = true;
                return;
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
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 2]);
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1]);
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 2].SetActive(false);
                PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].SetActive(false);
                PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);

                chainedUnits = true;
                return;
            }
        }

        // Middle Chained
        if (YIndex > 0 && YIndex < PuzzleGenerator.Instance._rows - 1)
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
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1]);
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1]);
                PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].SetActive(false);
                PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
                PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].SetActive(false);

                chainedUnits = true;
                return;
            }
        }

        // Up Chained
        if (YIndex < PuzzleGenerator.Instance._rows - 2)
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
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1]);
                //Destroy(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 2]);
                PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
                PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].SetActive(false);
                PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 2].SetActive(false);

                chainedUnits = true;
                return;
            }
        }
    }
    #endregion
}
