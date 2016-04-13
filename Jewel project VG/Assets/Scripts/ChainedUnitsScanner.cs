﻿using UnityEngine;
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

public class ChainedUnitsScanner : MonoBehaviour
{

    //==============================================
    // Constants
    //==============================================

    public static ChainedUnitsScanner Instance;

    //==============================================
    // Fields
    //==============================================

    public GameObject HLightningDestroyEff;
    public GameObject VLightningDestroyEff;
    public GameObject ExplodeDestroyEff;
    public GameObject AllUnitsTypeDestroyEff;
    public GameObject[] unitDestroyEff;

    public AnimationClip lightningDestroyAllClip;

    public ScanUnit[,] _scanUnitARR;

    private bool chainedUnits;

    private float destroyAllDelay;
    private float delayTime;

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

        destroyAllDelay = lightningDestroyAllClip.length;
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
                scanSwappedUnit(XIndex, YIndex);
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
            GameController.Instance.currentState = GameController.GameState.idle;
        }
    }

    /// <summary>
    /// Scan two swapped units
    /// </summary>
    /// <param name="The highlighted unit"></param>
    /// <param name="The other unit"></param>
    /// <returns></returns>
    public IEnumerator scanUnitsAfterSwap(GameObject focusedUnit, GameObject otherUnit)
    {
        yield return new WaitForSeconds(InputHandler.Instance.swapTime);

        UnitInfo focusedUnitInfo = focusedUnit.GetComponent<UnitInfo>();
        UnitInfo otherUnitInfo = otherUnit.GetComponent<UnitInfo>();

        // Check if swapped unit is the special "Destroy all" type
        // If true destroy all jewels that has the same type with the other swapped unit
        if (focusedUnitInfo._value == PuzzleGenerator.Instance.Unit.Length - 1)
        {
            //destroyAllUnitsOfType(otherUnitInfo._value);
            StartCoroutine(destroyAllUnitsOfType(otherUnitInfo._value));
            disableUnit(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
            StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle());
        }
        else if (otherUnitInfo._value == PuzzleGenerator.Instance.Unit.Length - 1)
        {
            //destroyAllUnitsOfType(focusedUnitInfo._value);
            StartCoroutine(destroyAllUnitsOfType(focusedUnitInfo._value));
            disableUnit(otherUnitInfo._XIndex, otherUnitInfo._YIndex);
            StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle());
        }
        else
        {
            chainedUnits = false;

            // Scan focused Unit chain
            scanSwappedUnit(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
            // Scan other Unit chain
            scanSwappedUnit(otherUnitInfo._XIndex, otherUnitInfo._YIndex);

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

    public IEnumerator scanRegenUnits(List<GameObject> unitsList)
    {
        updateScanARR();
        chainedUnits = false;

        delayTime = 0f;

        List<int> unitsXIndex = new List<int>();
        List<int> unitsYIndex = new List<int>();

        for (int i = 0; i < unitsList.Count; i++)
        {
            unitsXIndex.Add(unitsList[i].GetComponent<UnitInfo>()._XIndex);
            unitsYIndex.Add(unitsList[i].GetComponent<UnitInfo>()._YIndex);
        }

        for (int i = 0; i < unitsList.Count; i++)
        {
            scanFiveUnitsChainedType(unitsXIndex[i], unitsYIndex[i]);
        }

        for (int i = 0; i < unitsList.Count; i++)
        {
            scanFourUnitsChainedType(unitsXIndex[i], unitsYIndex[i]);
        }

        for (int i = 0; i < unitsList.Count; i++)
        {
            scanThreeUnitsChainedType(unitsXIndex[i], unitsYIndex[i]);
        }

        yield return new WaitForSeconds(1.5f);

        // If there are chained units
        if (chainedUnits)
        {
            StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle());
            //PuzzleGenerator.Instance.reOrganizePuzzle();
        }
        else
        {
            GameController.Instance.currentState = GameController.GameState.idle;
        }
    }

    #region Disable,Destroy and Special Destroy units methods

    /// <summary>
    /// Disable unit and it's effect and update scanARR
    /// </summary>
    /// <param name="XIndex"></param>
    /// <param name="YIndex"></param>
    public void disableUnit(int XIndex, int YIndex)
    {
        GameObject unit = PuzzleGenerator.Instance._unitARR[XIndex, YIndex];
        unit.SetActive(false);
        _scanUnitARR[XIndex, YIndex]._isChained = true;

        switch (unit.GetComponent<UnitInfo>()._unitEff)
        {
            case (UnitInfo.SpecialEff.hLightning):
                {
                    unit.GetComponent<UnitInfo>().HorizontalLightningEff.SetActive(false);
                    break;
                }
            case (UnitInfo.SpecialEff.vLightning):
                {
                    unit.GetComponent<UnitInfo>().VerticalLightningEff.SetActive(false);
                    break;
                }
            case (UnitInfo.SpecialEff.explode):
                {
                    unit.GetComponent<UnitInfo>().ExplosiveSparkEff.SetActive(false);
                    break;
                }
            default: break;
        }
    }

    #region Destroy Special Effect Units Method
    IEnumerator destroyAllUnitsOfType(int type)
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

                    Instantiate(AllUnitsTypeDestroyEff, PuzzleGenerator.Instance._unitPosARR[XIndex, YIndex], Quaternion.identity);
                    yield return new WaitForSeconds(0.02f);
                    //delayTime += 0.02f;
                }
            }
        }
        destroyUnits(unitsXIndex, unitsYIndex);

    }

    void destroyAllUnitsOfColumn(int col)
    {
        List<int> unitsXIndex = new List<int>();
        List<int> unitsYIndex = new List<int>();

        for (int YIndex = 0; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        {
            if (!_scanUnitARR[col, YIndex]._isChained)
            {
                //disableUnit(col, YIndex);
                unitsXIndex.Add(col);
                unitsYIndex.Add(YIndex);
            }
        }
        destroyUnits(unitsXIndex, unitsYIndex);

        Instantiate(VLightningDestroyEff, new Vector2(PuzzleGenerator.Instance._unitPosARR[col, 0].x, 0f), VLightningDestroyEff.transform.rotation);
    }

    void destroyAllUnitsOfRow(int row)
    {
        List<int> unitsXIndex = new List<int>();
        List<int> unitsYIndex = new List<int>();

        for (int XIndex = 0; XIndex < PuzzleGenerator.Instance._columns; XIndex++)
        {
            if (!_scanUnitARR[XIndex, row]._isChained)
            {
                //disableUnit(col, YIndex);
                unitsXIndex.Add(XIndex);
                unitsYIndex.Add(row);
            }
        }
        destroyUnits(unitsXIndex, unitsYIndex);

        Instantiate(HLightningDestroyEff, new Vector2(0f, PuzzleGenerator.Instance._unitPosARR[0, row].y), HLightningDestroyEff.transform.rotation);
    }

    void destroyAllLocalUnits(int XIndex, int YIndex)
    {
        List<int> unitsXIndex = new List<int>();
        List<int> unitsYIndex = new List<int>();

        if (XIndex > 0 && XIndex < PuzzleGenerator.Instance._columns - 1 && YIndex > 0 && YIndex < PuzzleGenerator.Instance._rows - 1)
        {
            print("middle");
            if (!_scanUnitARR[XIndex - 1, YIndex - 1]._isChained) { unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex - 1); }
            if (!_scanUnitARR[XIndex, YIndex - 1]._isChained) { unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1); }
            if (!_scanUnitARR[XIndex + 1, YIndex - 1]._isChained) { unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex - 1); }
            if (!_scanUnitARR[XIndex - 1, YIndex]._isChained) { unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex); }
            if (!_scanUnitARR[XIndex + 1, YIndex]._isChained) { unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex); }
            if (!_scanUnitARR[XIndex - 1, YIndex + 1]._isChained) { unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex + 1); }
            if (!_scanUnitARR[XIndex, YIndex + 1]._isChained) { unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1); }
            if (!_scanUnitARR[XIndex + 1, YIndex + 1]._isChained) { unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex + 1); }
        }
        else if (XIndex == 0 && YIndex == 0)
        {
            print("bottom left");
            if (!_scanUnitARR[XIndex + 1, YIndex]._isChained) { unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex); }
            if (!_scanUnitARR[XIndex, YIndex + 1]._isChained) { unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1); }
            if (!_scanUnitARR[XIndex + 1, YIndex + 1]._isChained) { unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex + 1); }
        }
        else if (XIndex == 0 && YIndex == PuzzleGenerator.Instance._rows - 1)
        {
            print("top left");
            if (!_scanUnitARR[XIndex, YIndex - 1]._isChained) { unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1); }
            if (!_scanUnitARR[XIndex + 1, YIndex - 1]._isChained) { unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex - 1); }
            if (!_scanUnitARR[XIndex + 1, YIndex]._isChained) { unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex); }
        }
        else if (XIndex == PuzzleGenerator.Instance._columns - 1 && YIndex == PuzzleGenerator.Instance._rows - 1)
        {
            print("top right");
            if (!_scanUnitARR[XIndex - 1, YIndex - 1]._isChained) { unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex - 1); }
            if (!_scanUnitARR[XIndex, YIndex - 1]._isChained) { unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1); }
            if (!_scanUnitARR[XIndex - 1, YIndex]._isChained) { unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex); }
        }
        else if (XIndex == PuzzleGenerator.Instance._columns - 1 && YIndex == 0)
        {
            print("bottom right");
            if (!_scanUnitARR[XIndex - 1, YIndex]._isChained) { unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex); }
            if (!_scanUnitARR[XIndex - 1, YIndex + 1]._isChained) { unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex + 1); }
            if (!_scanUnitARR[XIndex, YIndex + 1]._isChained) { unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1); }
        }
        else if (XIndex == 0)
        {
            print("left");
            if (!_scanUnitARR[XIndex, YIndex - 1]._isChained) { unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1); }
            if (!_scanUnitARR[XIndex + 1, YIndex - 1]._isChained) { unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex - 1); }
            if (!_scanUnitARR[XIndex + 1, YIndex]._isChained) { unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex); }
            if (!_scanUnitARR[XIndex, YIndex + 1]._isChained) { unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1); }
            if (!_scanUnitARR[XIndex + 1, YIndex + 1]._isChained) { unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex + 1); }
        }
        else if (XIndex == PuzzleGenerator.Instance._columns - 1)
        {
            print("right");
            if (!_scanUnitARR[XIndex - 1, YIndex - 1]._isChained) { unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex - 1); }
            if (!_scanUnitARR[XIndex, YIndex - 1]._isChained) { unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1); }
            if (!_scanUnitARR[XIndex - 1, YIndex]._isChained) { unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex); }
            if (!_scanUnitARR[XIndex - 1, YIndex + 1]._isChained) { unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex + 1); }
            if (!_scanUnitARR[XIndex, YIndex + 1]._isChained) { unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1); }
        }
        else if (YIndex == 0)
        {
            print("bottom");
            if (!_scanUnitARR[XIndex - 1, YIndex]._isChained) { unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex); }
            if (!_scanUnitARR[XIndex + 1, YIndex]._isChained) { unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex); }
            if (!_scanUnitARR[XIndex - 1, YIndex + 1]._isChained) { unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex + 1); }
            if (!_scanUnitARR[XIndex, YIndex + 1]._isChained) { unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1); }
            if (!_scanUnitARR[XIndex + 1, YIndex + 1]._isChained) { unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex + 1); }
        }
        else if (YIndex == PuzzleGenerator.Instance._rows - 1)
        {
            print("top");
            if (!_scanUnitARR[XIndex - 1, YIndex - 1]._isChained) { unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex - 1); }
            if (!_scanUnitARR[XIndex, YIndex - 1]._isChained) { unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1); }
            if (!_scanUnitARR[XIndex + 1, YIndex - 1]._isChained) { unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex - 1); }
            if (!_scanUnitARR[XIndex - 1, YIndex]._isChained) { unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex); }
            if (!_scanUnitARR[XIndex + 1, YIndex]._isChained) { unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex); }
        }

        PuzzleGenerator.Instance._unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._unitEff = UnitInfo.SpecialEff.noEff;
        destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, _scanUnitARR[XIndex, YIndex]._value, UnitInfo.SpecialEff.noEff);

        Instantiate(ExplodeDestroyEff, PuzzleGenerator.Instance._unitPosARR[XIndex, YIndex], Quaternion.identity);
    }

    #endregion

    /// <summary>
    /// Check Info of each unit destroyed by DestroyAllJewel to call the suitable destroy type
    /// </summary>
    /// <param name="unitsXIndex"></param>
    /// <param name="unitsYIndex"></param>
    void destroyUnits(List<int> unitsXIndex, List<int> unitsYIndex)
    {
        //if (_scanUnitARR[targetXIndex, targetYIndex]._value == PuzzleGenerator.Instance.Unit.Length)
        //{
        //    destroyAllUnitsOfType(_scanUnitARR[localsXIndex[0], localsYIndex[0]]._value);
        //}
        for (int i = 0; i < unitsYIndex.Count; i++)
        {
            disableUnit(unitsXIndex[i], unitsYIndex[i]);

            switch (PuzzleGenerator.Instance._unitARR[unitsXIndex[i], unitsYIndex[i]].GetComponent<UnitInfo>()._unitEff)
            {
                case UnitInfo.SpecialEff.vLightning:
                    {
                        destroyAllUnitsOfColumn(unitsXIndex[i]);
                        break;
                    }

                case UnitInfo.SpecialEff.hLightning:
                    {
                        destroyAllUnitsOfRow(unitsYIndex[i]);
                        break;
                    }

                case UnitInfo.SpecialEff.explode:
                    {
                        destroyAllLocalUnits(unitsXIndex[i], unitsYIndex[i]);
                        break;
                    }

                case UnitInfo.SpecialEff.noEff:
                    {
                        break;
                    }

                default: break;
            }
        }
    }

    /// <summary>
    /// Check Swapped or Regenerated Units Info to call the suitable destroy type
    /// </summary>
    /// <param name="Xtarget"></param>
    /// <param name="Ytarget"></param>
    /// <param name="unitsXIndex"></param>
    /// <param name="unitsYIndex"></param>
    /// <param name="targetNextEff"></param>
    void destroyUnits(int Xtarget, int Ytarget, List<int> unitsXIndex, List<int> unitsYIndex, int targetNextValue, UnitInfo.SpecialEff targetNextEff)
    {
        // Auto correct variables
        if (targetNextValue == PuzzleGenerator.Instance.Unit.Length - 1)
        {
            targetNextEff = UnitInfo.SpecialEff.noEff;
        }

        disableUnit(Xtarget, Ytarget);

        // If the target unit has special Eff
        if (PuzzleGenerator.Instance._unitARR[Xtarget, Ytarget].GetComponent<UnitInfo>()._unitEff != UnitInfo.SpecialEff.noEff)
        {
            print(PuzzleGenerator.Instance._unitARR[Xtarget, Ytarget].GetComponent<UnitInfo>()._unitEff);
            // Trigger it's Eff
            switch (PuzzleGenerator.Instance._unitARR[Xtarget, Ytarget].GetComponent<UnitInfo>()._unitEff)
            {
                case UnitInfo.SpecialEff.vLightning:
                    {
                        destroyAllUnitsOfColumn(Xtarget);
                        break;
                    }

                case UnitInfo.SpecialEff.hLightning:
                    {
                        destroyAllUnitsOfRow(Ytarget);
                        break;
                    }

                case UnitInfo.SpecialEff.explode:
                    {
                        destroyAllLocalUnits(Xtarget, Ytarget);
                        break;
                    }

                default: break;
            }

            // If this Unit formed special chained type then trigger it's new eff again
            if (targetNextValue == PuzzleGenerator.Instance.Unit.Length - 1)
            {
                //destroyAllUnitsOfType(_scanUnitARR[Xtarget, Ytarget]._value);
                StartCoroutine(destroyAllUnitsOfType(_scanUnitARR[Xtarget, Ytarget]._value));
            }
            else
            {
                switch (targetNextEff)
                {
                    case UnitInfo.SpecialEff.vLightning:
                        {
                            destroyAllUnitsOfColumn(Xtarget);
                            break;
                        }

                    case UnitInfo.SpecialEff.hLightning:
                        {
                            destroyAllUnitsOfRow(Ytarget);
                            break;
                        }

                    case UnitInfo.SpecialEff.explode:
                        {
                            destroyAllLocalUnits(Xtarget, Ytarget);
                            break;
                        }

                    default: break;
                }
            }
        }

        // If the target doesn't has any special Eff
        else
        {
            // If formed special chained type,upgrade it
            if (targetNextValue == PuzzleGenerator.Instance.Unit.Length - 1)
            {
                PuzzleGenerator.Instance.upgradeToDestroyAllUnit(Xtarget, Ytarget);
            }
            else
            {
                if (targetNextEff != UnitInfo.SpecialEff.noEff)
                {
                    PuzzleGenerator.Instance.upgradeUnit(Xtarget, Ytarget, targetNextEff);
                }
            }

            // Then destroy all other unit that are in chain
            for (int i = 0; i < unitsYIndex.Count; i++)
            {
                disableUnit(unitsXIndex[i], unitsYIndex[i]);
                if (PuzzleGenerator.Instance._unitARR[unitsXIndex[i], unitsYIndex[i]].GetComponent<UnitInfo>()._value
                    == PuzzleGenerator.Instance.Unit.Length - 1)
                {
                    StartCoroutine(destroyAllUnitsOfType(PuzzleGenerator.Instance._unitARR[Xtarget, Ytarget].GetComponent<UnitInfo>()._value));
                }
                else
                {
                    switch (PuzzleGenerator.Instance._unitARR[unitsXIndex[i], unitsYIndex[i]].GetComponent<UnitInfo>()._unitEff)
                    {
                        case UnitInfo.SpecialEff.vLightning:
                            {
                                destroyAllUnitsOfColumn(unitsXIndex[i]);
                                break;
                            }

                        case UnitInfo.SpecialEff.hLightning:
                            {
                                destroyAllUnitsOfRow(unitsYIndex[i]);
                                break;
                            }

                        case UnitInfo.SpecialEff.explode:
                            {
                                destroyAllLocalUnits(unitsXIndex[i], unitsYIndex[i]);
                                break;
                            }

                        case UnitInfo.SpecialEff.noEff:
                            {
                                break;
                            }

                        default: break;
                    }
                }                
            }
        }
    }

    #endregion

    // Scan swapped unit that is located in the specific index
    /// <summary>
    /// Scan each swapped or regenerated units to find out it's chained types
    /// </summary>
    /// <param name="XIndex"></param>
    /// <param name="YIndex"></param>
    void scanSwappedUnit(int XIndex, int YIndex)
    {
        //scanHorizontalThreeChainedSwap(XIndex, YIndex);
        //scanVerticalThreeChainedSwap(XIndex, YIndex);
        List<int> unitsXIndex = new List<int>();
        List<int> unitsYIndex = new List<int>();

        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

        #region 5 Units chained
        // Horizontal 5 chained
        if (isRightThreeChained(XIndex, YIndex) && isLeftThreeChained(XIndex, YIndex))
        {
            //// Disable chained Units
            //_scanUnitARR[XIndex - 2, YIndex]._isChained = true;
            //_scanUnitARR[XIndex -1, YIndex]._isChained = true;
            //_scanUnitARR[XIndex   , YIndex]._isChained = true;
            //_scanUnitARR[XIndex + 1, YIndex]._isChained = true;
            //_scanUnitARR[XIndex + 2, YIndex]._isChained = true;
            //PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex + 2, YIndex].SetActive(false);

            //// Upgrade special Unit
            //PuzzleGenerator.Instance.upgradeToDestroyAllUnit(XIndex, YIndex);

            unitsXIndex.Add(XIndex - 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 2); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Vertical 5 Chained
        if (isUpThreeChained(XIndex, YIndex) && isDownThreeChained(XIndex, YIndex))
        {
            //// Disable chained Units
            //_scanUnitARR[XIndex, YIndex - 2]._isChained = true;
            //_scanUnitARR[XIndex, YIndex - 1]._isChained = true;
            //_scanUnitARR[XIndex, YIndex   ]._isChained = true;
            //_scanUnitARR[XIndex, YIndex + 1]._isChained = true;
            //_scanUnitARR[XIndex, YIndex + 2]._isChained = true;
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 2].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 2].SetActive(false);

            //// Init special Unit
            //PuzzleGenerator.Instance.upgradeToDestroyAllUnit(XIndex, YIndex);

            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 2);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 2);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Cross 5 Chained
        if (isMiddleHorizontalThreeChained(XIndex, YIndex) && isMiddleVerticalThreeChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Down Left L 5 Chained
        if (isDownThreeChained(XIndex, YIndex) && isLeftThreeChained(XIndex, YIndex))
        {
            //_scanUnitARR[XIndex, YIndex - 2]._isChained = true;
            //_scanUnitARR[XIndex, YIndex - 1]._isChained = true;
            //_scanUnitARR[XIndex, YIndex]._isChained = true;
            //_scanUnitARR[XIndex - 2, YIndex    ]._isChained = true;
            //_scanUnitARR[XIndex - 1, YIndex    ]._isChained = true;
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 2].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex    ].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex    ].SetActive(false);

            //// Init special Unit
            //PuzzleGenerator.Instance.upgradeToDestroyAllUnit(XIndex, YIndex);

            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 2);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex - 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Up Left L 5 Chained
        if (isUpThreeChained(XIndex, YIndex) && isLeftThreeChained(XIndex, YIndex))
        {
            //_scanUnitARR[XIndex - 2, YIndex]._isChained = true;
            //_scanUnitARR[XIndex - 1, YIndex]._isChained = true;
            //_scanUnitARR[XIndex   , YIndex]._isChained = true;
            //_scanUnitARR[XIndex   , YIndex + 1]._isChained = true;
            //_scanUnitARR[XIndex   , YIndex + 2]._isChained = true;
            //PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex    , YIndex + 1].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex    , YIndex + 2].SetActive(false);

            //// Init special Unit
            //PuzzleGenerator.Instance.upgradeToDestroyAllUnit(XIndex, YIndex);

            unitsXIndex.Add(XIndex - 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 2);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Up right L 5 Chained
        if (isUpThreeChained(XIndex, YIndex) && isRightThreeChained(XIndex, YIndex))
        {
            //_scanUnitARR[XIndex    , YIndex]._isChained = true;
            //_scanUnitARR[XIndex + 1, YIndex]._isChained = true;
            //_scanUnitARR[XIndex + 2, YIndex]._isChained = true;
            //_scanUnitARR[XIndex   , YIndex + 1]._isChained = true;
            //_scanUnitARR[XIndex   , YIndex + 2]._isChained = true;
            //PuzzleGenerator.Instance._unitARR[XIndex    , YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex + 2, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex   , YIndex + 1].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex   , YIndex + 2].SetActive(false);

            //// Init special Unit
            //PuzzleGenerator.Instance.upgradeToDestroyAllUnit(XIndex, YIndex);

            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 2);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Down right L 5 Chained
        if (isDownThreeChained(XIndex, YIndex) && isRightThreeChained(XIndex, YIndex))
        {
            //_scanUnitARR[XIndex, YIndex]._isChained = true;
            //_scanUnitARR[XIndex + 1, YIndex]._isChained = true;
            //_scanUnitARR[XIndex + 2, YIndex]._isChained = true;
            //_scanUnitARR[XIndex, YIndex - 1]._isChained = true;
            //_scanUnitARR[XIndex, YIndex - 2]._isChained = true;
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex + 2, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 2].SetActive(false);

            //// Init special Unit
            //PuzzleGenerator.Instance.upgradeToDestroyAllUnit(XIndex, YIndex);

            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 2);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Down T 5 Chained
        if (isDownThreeChained(XIndex, YIndex) && isMiddleHorizontalThreeChained(XIndex, YIndex))
        {
            //_scanUnitARR[XIndex, YIndex - 1]._isChained = true;
            //_scanUnitARR[XIndex, YIndex - 2]._isChained = true;
            //_scanUnitARR[XIndex - 1, YIndex]._isChained = true;
            //_scanUnitARR[XIndex, YIndex    ]._isChained = true;
            //_scanUnitARR[XIndex + 1, YIndex]._isChained = true;
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 2].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);

            //// Init special Unit
            //PuzzleGenerator.Instance.upgradeToDestroyAllUnit(XIndex, YIndex);

            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 2);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Left T 5 Chained
        if (isMiddleVerticalThreeChained(XIndex, YIndex) && isLeftThreeChained(XIndex, YIndex))
        {
            //_scanUnitARR[XIndex - 1, YIndex]._isChained = true;
            //_scanUnitARR[XIndex - 2, YIndex]._isChained = true;
            //_scanUnitARR[XIndex   , YIndex]._isChained = true;
            //_scanUnitARR[XIndex, YIndex - 1]._isChained = true;
            //_scanUnitARR[XIndex, YIndex + 1]._isChained = true;
            //PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex   , YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex   , YIndex - 1].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex   , YIndex + 1].SetActive(false);

            //// Init special Unit
            //PuzzleGenerator.Instance.upgradeToDestroyAllUnit(XIndex, YIndex);

            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Up T 5 Chained
        if (isUpThreeChained(XIndex, YIndex) && isMiddleHorizontalThreeChained(XIndex, YIndex))
        {
            //_scanUnitARR[XIndex - 1, YIndex]._isChained = true;
            //_scanUnitARR[XIndex   , YIndex]._isChained = true;
            //_scanUnitARR[XIndex + 1, YIndex]._isChained = true;
            //_scanUnitARR[XIndex, YIndex + 1]._isChained = true;
            //_scanUnitARR[XIndex, YIndex + 2]._isChained = true;
            //PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex    , YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 2].SetActive(false);

            //// Init special Unit
            //PuzzleGenerator.Instance.upgradeToDestroyAllUnit(XIndex, YIndex);

            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 2);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        // Right T 5 Chained
        if (isMiddleVerticalThreeChained(XIndex, YIndex) && isRightThreeChained(XIndex, YIndex))
        {
            //_scanUnitARR[XIndex   , YIndex - 1]._isChained = true;
            //_scanUnitARR[XIndex   , YIndex]._isChained = true;
            //_scanUnitARR[XIndex + 1, YIndex]._isChained = true;
            //_scanUnitARR[XIndex + 2, YIndex]._isChained = true;
            //_scanUnitARR[XIndex, YIndex + 1]._isChained = true;
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex + 2, YIndex].SetActive(false);
            //PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].SetActive(false);

            //// Init special Unit
            //PuzzleGenerator.Instance.upgradeToDestroyAllUnit(XIndex, YIndex);

            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }
        #endregion 5 Units chained

        #region 4 Units chained

        // Outer Left 4 chained
        if (isLeftThreeChained(XIndex - 1, YIndex)
                && _scanUnitARR[XIndex - 1, YIndex]._value == scanUnitValue
                && !_scanUnitARR[XIndex, YIndex]._isChained)
        {
            unitsXIndex.Add(XIndex - 3); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
            chainedUnits = true;
            return;
        }


        // Inner Left 4 chained
        if (XIndex < PuzzleGenerator.Instance._columns - 1)
        {
            if (isLeftThreeChained(XIndex, YIndex)
                && !_scanUnitARR[XIndex + 1, YIndex]._isChained
                && _scanUnitARR[XIndex + 1, YIndex]._value == scanUnitValue)
            {
                unitsXIndex.Add(XIndex - 2); unitsYIndex.Add(YIndex);
                unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
                unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
                destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
                chainedUnits = true;
                return;
            }
        }

        // Inner Right 4 chained
        if (XIndex > 0)
        {
            if (isRightThreeChained(XIndex, YIndex)
                && !_scanUnitARR[XIndex - 1, YIndex]._isChained
                && _scanUnitARR[XIndex - 1, YIndex]._value == scanUnitValue)
            {
                unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
                unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
                unitsXIndex.Add(XIndex + 2); unitsYIndex.Add(YIndex);
                destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
                chainedUnits = true;
                return;
            }
        }

        // Outer Right 4 chained
        if (isRightThreeChained(XIndex + 1, YIndex)
                && _scanUnitARR[XIndex + 1, YIndex]._value == scanUnitValue
                && !_scanUnitARR[XIndex, YIndex]._isChained)
        {
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 3); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
            chainedUnits = true;
            return;
        }

        // Outer Down 4 chained
        if (isDownThreeChained(XIndex, YIndex - 1)
                && _scanUnitARR[XIndex, YIndex - 1]._value == scanUnitValue
                && !_scanUnitARR[XIndex, YIndex]._isChained)
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 3);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 2);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
            chainedUnits = true;
            return;
        }

        // Inner Down 4 chained
        if (YIndex < PuzzleGenerator.Instance._rows - 1)
        {
            if (isDownThreeChained(XIndex, YIndex)
                && !_scanUnitARR[XIndex, YIndex + 1]._isChained
                && _scanUnitARR[XIndex, YIndex + 1]._value == scanUnitValue)
            {
                unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 2);
                unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
                unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
                destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
                chainedUnits = true;
                return;
            }
        }

        // Inner Up 4 chained
        if (YIndex > 0)
        {
            if (isUpThreeChained(XIndex, YIndex)
                && !_scanUnitARR[XIndex, YIndex - 1]._isChained
                && _scanUnitARR[XIndex, YIndex - 1]._value == scanUnitValue)
            {
                unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
                unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
                unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 2);
                destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
                chainedUnits = true;
                return;
            }
        }

        // Outer Up 4 chained
        if (isUpThreeChained(XIndex, YIndex + 1)
                && _scanUnitARR[XIndex, YIndex + 1]._value == scanUnitValue
                && !_scanUnitARR[XIndex, YIndex]._isChained)
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 2);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 3);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
            chainedUnits = true;
            return;
        }

        #endregion

        #region 3 Units chained

        // Left 3 chained
        if (isLeftThreeChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex - 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.noEff);
            chainedUnits = true;
            return;
        }

        // Middle Horizontal 3 chained
        if (isMiddleHorizontalThreeChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.noEff);
            chainedUnits = true;
            return;
        }

        // Right 3 chained
        if (isRightThreeChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 2); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.noEff);
            chainedUnits = true;
            return;
        }

        // Down 3 chained
        if (isDownThreeChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 2);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.noEff);
            chainedUnits = true;
            return;
        }

        // Middle Vertical 3 chained
        if (isMiddleVerticalThreeChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.noEff);
            chainedUnits = true;
            return;
        }

        // Up 3 chained
        if (isUpThreeChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 2);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.noEff);
            chainedUnits = true;
            return;
        }

        #endregion
    }

    #region Scan for chained units types methods

    void scanFiveUnitsChainedType(int XIndex, int YIndex)
    {
        List<int> unitsXIndex = new List<int>();
        List<int> unitsYIndex = new List<int>();

        if (isHorizontalFiveChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex - 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 2); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        if (isVerticalFiveChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 2);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 2);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        if (isCrossFiveChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        if (isDownLeftLFiveChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 2);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex - 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        if (isUpLeftLFiveChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex - 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 2);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        if (isUprightLFiveChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 2);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        if (isDownRightLFiveChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 2);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        if (isDownTFiveChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 2);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        if (isLeftTFiveChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        if (isUpTFiveChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 2);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }

        if (isRightTFiveChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, PuzzleGenerator.Instance.Unit.Length - 1, UnitInfo.SpecialEff.noEff);

            chainedUnits = true;
            return;
        }
    }

    void scanFourUnitsChainedType(int XIndex, int YIndex)
    {
        List<int> unitsXIndex = new List<int>();
        List<int> unitsYIndex = new List<int>();
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

        if (isOutterLeftFourChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex - 3); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
            chainedUnits = true;
            return;
        }

        if (isInnerLeftFourChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex - 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
            chainedUnits = true;
            return;
        }

        if (isInnerRightFourChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 2); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
            chainedUnits = true;
            return;
        }

        if (isOutterRightFourChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 3); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
            chainedUnits = true;
            return;
        }

        if (isOutterDownFourChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 3);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 2);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
            chainedUnits = true;
            return;
        }

        if (isInnerDownFourChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 2);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
            chainedUnits = true;
            return;
        }

        if (isInnerUpFourChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 2);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
            chainedUnits = true;
            return;
        }

        if (isOutterUpFourChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 2);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 3);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.explode);
            chainedUnits = true;
            return;
        }
    }

    void scanThreeUnitsChainedType(int XIndex, int YIndex)
    {
        List<int> unitsXIndex = new List<int>();
        List<int> unitsYIndex = new List<int>();
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

        if (isLeftThreeChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex - 2); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.noEff);
            chainedUnits = true;
            return;
        }

        if (isMiddleHorizontalThreeChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex - 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.noEff);
            chainedUnits = true;
            return;
        }

        if (isRightThreeChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex + 1); unitsYIndex.Add(YIndex);
            unitsXIndex.Add(XIndex + 2); unitsYIndex.Add(YIndex);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.noEff);
            chainedUnits = true;
            return;
        }

        if (isDownThreeChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 2);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.noEff);
            chainedUnits = true;
            return;
        }

        if (isMiddleVerticalThreeChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex - 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.noEff);
            chainedUnits = true;
            return;
        }

        if (isUpThreeChained(XIndex, YIndex))
        {
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 1);
            unitsXIndex.Add(XIndex); unitsYIndex.Add(YIndex + 2);
            destroyUnits(XIndex, YIndex, unitsXIndex, unitsYIndex, scanUnitValue, UnitInfo.SpecialEff.noEff);
            chainedUnits = true;
            return;
        }
    }

    #endregion

    #region Detect chained units types methods

    #region Detect 5 chained units types methods

    bool isHorizontalFiveChained(int XIndex, int YIndex)
    {
        if (isRightThreeChained(XIndex, YIndex) && isLeftThreeChained(XIndex, YIndex))
        {
            return true;
        }
        return false;
    }

    bool isVerticalFiveChained(int XIndex, int YIndex)
    {
        if (isUpThreeChained(XIndex, YIndex) && isDownThreeChained(XIndex, YIndex))
        {
            return true;
        }
        return false;
    }

    bool isCrossFiveChained(int XIndex, int YIndex)
    {
        if (isMiddleHorizontalThreeChained(XIndex, YIndex) && isMiddleVerticalThreeChained(XIndex, YIndex))
        {
            return true;
        }
        return false;
    }

    bool isDownLeftLFiveChained(int XIndex, int YIndex)
    {
        if (isDownThreeChained(XIndex, YIndex) && isLeftThreeChained(XIndex, YIndex))
        {
            return true;
        }
        return false;
    }

    bool isUpLeftLFiveChained(int XIndex, int YIndex)
    {
        if (isUpThreeChained(XIndex, YIndex) && isLeftThreeChained(XIndex, YIndex))
        {
            return true;
        }
        return false;
    }

    bool isUprightLFiveChained(int XIndex, int YIndex)
    {
        if (isUpThreeChained(XIndex, YIndex) && isRightThreeChained(XIndex, YIndex))
        {
            return true;
        }
        return false;
    }

    bool isDownRightLFiveChained(int XIndex, int YIndex)
    {
        if (isDownThreeChained(XIndex, YIndex) && isRightThreeChained(XIndex, YIndex))
        {
            return true;
        }
        return false;
    }

    bool isDownTFiveChained(int XIndex, int YIndex)
    {
        if (isDownThreeChained(XIndex, YIndex) && isMiddleHorizontalThreeChained(XIndex, YIndex))
        {
            return true;
        }
        return false;
    }

    bool isLeftTFiveChained(int XIndex, int YIndex)
    {
        if (isMiddleVerticalThreeChained(XIndex, YIndex) && isLeftThreeChained(XIndex, YIndex))
        {
            return true;
        }
        return false;
    }

    bool isUpTFiveChained(int XIndex, int YIndex)
    {
        if (isUpThreeChained(XIndex, YIndex) && isMiddleHorizontalThreeChained(XIndex, YIndex))
        {
            return true;
        }
        return false;
    }

    bool isRightTFiveChained(int XIndex, int YIndex)
    {
        if (isMiddleVerticalThreeChained(XIndex, YIndex) && isRightThreeChained(XIndex, YIndex))
        {
            return true;
        }
        return false;
    }

    #endregion

    #region Detect 4 chained units types methods

    bool isOutterLeftFourChained(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;
        if (isLeftThreeChained(XIndex - 1, YIndex)
                && _scanUnitARR[XIndex - 1, YIndex]._value == scanUnitValue
                && !_scanUnitARR[XIndex, YIndex]._isChained)
        {
            return true;
        }
        return false;
    }

    bool isInnerLeftFourChained(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;
        if (XIndex < PuzzleGenerator.Instance._columns - 1)
        {
            if (isLeftThreeChained(XIndex, YIndex)
                    && !_scanUnitARR[XIndex + 1, YIndex]._isChained
                    && _scanUnitARR[XIndex + 1, YIndex]._value == scanUnitValue)
            {
                return true;
            }
        }
        return false;
    }

    bool isInnerRightFourChained(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

        if (XIndex > 0)
        {
            if (isRightThreeChained(XIndex, YIndex)
                    && !_scanUnitARR[XIndex - 1, YIndex]._isChained
                    && _scanUnitARR[XIndex - 1, YIndex]._value == scanUnitValue)
            {
                return true;
            }
        }      
        return false;
    }

    bool isOutterRightFourChained(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;
        if (isRightThreeChained(XIndex + 1, YIndex)
                && _scanUnitARR[XIndex + 1, YIndex]._value == scanUnitValue
                && !_scanUnitARR[XIndex, YIndex]._isChained)
        {
            return true;
        }
        return false;
    }

    bool isOutterDownFourChained(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;
        if (isDownThreeChained(XIndex, YIndex - 1)
                && _scanUnitARR[XIndex, YIndex - 1]._value == scanUnitValue
                && !_scanUnitARR[XIndex, YIndex]._isChained)
        {
            return true;
        }
        return false;
    }

    bool isInnerDownFourChained(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;
        if (YIndex < PuzzleGenerator.Instance._rows - 1)
        {
            if (isDownThreeChained(XIndex, YIndex)
                    && !_scanUnitARR[XIndex, YIndex + 1]._isChained
                    && _scanUnitARR[XIndex, YIndex + 1]._value == scanUnitValue)
            {
                return true;
            }
        }
        return false;
    }

    bool isInnerUpFourChained(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;
        if (YIndex > 0)
        {
            if (isUpThreeChained(XIndex, YIndex)
                    && !_scanUnitARR[XIndex, YIndex - 1]._isChained
                    && _scanUnitARR[XIndex, YIndex - 1]._value == scanUnitValue)
            {
                return true;
            }
        }
        return false;
    }

    bool isOutterUpFourChained(int XIndex, int YIndex)
    {
        int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;
        if (isUpThreeChained(XIndex, YIndex + 1)
                && _scanUnitARR[XIndex, YIndex + 1]._value == scanUnitValue
                && !_scanUnitARR[XIndex, YIndex]._isChained)
        {
            return true;
        }
        return false;
    }

    #endregion

    #region new Detect 3 chained units types methods

    bool isLeftThreeChained(int XIndex, int YIndex)
    {
        if (XIndex > 1)
        {
            int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

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
        if (XIndex > 0 && XIndex < PuzzleGenerator.Instance._columns - 1)
        {
            int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

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

    bool isRightThreeChained(int XIndex, int YIndex)
    {
        if (XIndex < PuzzleGenerator.Instance._columns - 2)
        {
            int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

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

    bool isDownThreeChained(int XIndex, int YIndex)
    {
        if (YIndex > 1)
        {
            int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

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
        if (YIndex > 0 && YIndex < PuzzleGenerator.Instance._rows - 1)
        {
            int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

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

    bool isUpThreeChained(int XIndex, int YIndex)
    {
        if (YIndex < PuzzleGenerator.Instance._rows - 2)
        {
            int scanUnitValue = _scanUnitARR[XIndex, YIndex]._value;

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
                    && !_scanUnitARR[XIndex, YIndex]._isChained)
            {
                _scanUnitARR[XIndex - 2, YIndex]._isChained = true;
                _scanUnitARR[XIndex - 1, YIndex]._isChained = true;
                _scanUnitARR[XIndex, YIndex]._isChained = true;
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
                    && !_scanUnitARR[XIndex, YIndex]._isChained
                    && !_scanUnitARR[XIndex + 1, YIndex]._isChained
                    && !_scanUnitARR[XIndex + 2, YIndex]._isChained)
            {
                _scanUnitARR[XIndex, YIndex]._isChained = true;
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
                    && !_scanUnitARR[XIndex, YIndex]._isChained)
            {
                _scanUnitARR[XIndex, YIndex - 2]._isChained = true;
                _scanUnitARR[XIndex, YIndex - 1]._isChained = true;
                _scanUnitARR[XIndex, YIndex]._isChained = true;
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
                    && !_scanUnitARR[XIndex, YIndex]._isChained
                    && !_scanUnitARR[XIndex, YIndex + 1]._isChained)
            {
                _scanUnitARR[XIndex, YIndex - 1]._isChained = true;
                _scanUnitARR[XIndex, YIndex]._isChained = true;
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
