using UnityEngine;
using UnityEngine.UI;
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
    public Text unitScoreText;
    public Canvas scoreTextCanvas;

    public AnimationClip lightningDestroyAllClip;

    public AudioClip unitDestroySound;
    public AudioClip explosionDestroySound;
    public AudioClip lightingDestroySound;
    public AudioClip destroyAllUnitSound;

    public AudioClip frozenBreakSound;
    public AudioClip lockBreakSound;

    public AudioClip unSwapUnitSound;

    public ScanUnit[,] _scanUnitARR;

    private Transform specialEffHolder;

    private List<GameObject>[] _specialEffPoolARR;
    private int specialEffPoolSize;

    private List<Text> unitScoreTextPool;
    private int unitScoreTextPoolSize;

    private int bonusPoint = 0;
    private int maxBonusPoint = 130;

    private bool chainedUnits;
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
        specialEffHolder = new GameObject("Special Effs Holder").transform;

        // Init special Eff pool
        _specialEffPoolARR = new List<GameObject>[unitDestroyEff.Length];
        Vector2 poolPos = new Vector2(-20, -20);
        specialEffPoolSize = PuzzleGenerator.Instance._rows * PuzzleGenerator.Instance._columns;
        for (int i = 0; i < _specialEffPoolARR.Length; i++)
        {
            _specialEffPoolARR[i] = new List<GameObject>();
            for (int j = 0; j < specialEffPoolSize; j++)
            {
                GameObject destroyEff = Instantiate(unitDestroyEff[i], poolPos, Quaternion.identity) as GameObject;
                destroyEff.SetActive(false);
                _specialEffPoolARR[i].Add(destroyEff);
                destroyEff.transform.SetParent(specialEffHolder);
            }
        }

        // Init unitScoreText Pool
        unitScoreTextPool = new List<Text>();
        unitScoreTextPoolSize = PuzzleGenerator.Instance._rows * PuzzleGenerator.Instance._columns;
        for (int i = 0; i < unitScoreTextPoolSize; i++)
        {
            Text scoreText = Instantiate(unitScoreText, poolPos, Quaternion.identity) as Text;
            scoreText.gameObject.SetActive(false);
            unitScoreTextPool.Add(scoreText);
            scoreText.transform.SetParent(scoreTextCanvas.transform, false);
        }

        //Text a = Instantiate(unitScoreText, new Vector2(0, 0), Quaternion.identity) as Text;
        //a.text = "130";
        //a.transform.SetParent(scoreTextCanvas.transform, false);
    }
    
    //==============================================
    // Methods
    //==============================================

    public void initScanUnitArr()
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

    // Sync the choosen scanUnits with the current puzzle
    public void updateScanUnits(List<GameObject> unitList)
    {
        for (int i = 0; i < unitList.Count; i++)
        {
            int XIndex = unitList[i].GetComponent<UnitInfo>()._XIndex;
            int YIndex = unitList[i].GetComponent<UnitInfo>()._YIndex;

            if (PuzzleGenerator.Instance._unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow)
            {
                _scanUnitARR[XIndex, YIndex]._value = PuzzleGenerator.Instance._unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._value;
                _scanUnitARR[XIndex, YIndex].pointMultiplier = 0;
                _scanUnitARR[XIndex, YIndex]._isChained = false;
            }     
        }
    }

    #region Scan Units methods
    /// <summary>
    /// Scan two swapped units
    /// </summary>
    /// <param name="The highlighted unit"></param>
    /// <param name="The other unit"></param>
    /// <returns></returns>
    public IEnumerator scanUnitsAfterSwap(GameObject focusedUnit, GameObject otherUnit)
    {
        PuzzleGenerator.Instance.turnCountToUpgrade++;

        yield return new WaitForSeconds(InputHandler.Instance.swapTime);

        UnitInfo focusedUnitInfo = focusedUnit.GetComponent<UnitInfo>();
        UnitInfo otherUnitInfo = otherUnit.GetComponent<UnitInfo>();

        bonusPoint = 0;

        if(focusedUnitInfo._value == PuzzleGenerator.Instance.Unit.Length - 1 
            && otherUnitInfo._value == PuzzleGenerator.Instance.Unit.Length - 1)
        {
            print("destroy all");
            // Disable the UnitBG that in the same position as the two destroyAll Unit
            UnitBGGenerator.Instance.removeBG(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
            UnitBGGenerator.Instance.removeBG(otherUnitInfo._XIndex, otherUnitInfo._YIndex);

            disableUnit(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
            disableUnit(otherUnitInfo._XIndex, otherUnitInfo._YIndex);
            destroyAllUnitsInPuzzle();

            GameController.Instance.reduceMovesCount();
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle(true));
        }
        else if ((focusedUnitInfo._value == PuzzleGenerator.Instance.Unit.Length - 1 && otherUnitInfo._unitEff != UnitInfo.SpecialEff.noEff)
           || (otherUnitInfo._value == PuzzleGenerator.Instance.Unit.Length - 1 && focusedUnitInfo._unitEff != UnitInfo.SpecialEff.noEff))
        {
            List<UnitInfo> infoList = new List<UnitInfo>();
            UnitInfo.SpecialEff specialEff = UnitInfo.SpecialEff.noEff;

            if (focusedUnitInfo._value == PuzzleGenerator.Instance.Unit.Length - 1)
            {
                infoList = PuzzleGenerator.Instance.getUnitsOfTypeInfo(otherUnitInfo._value);
                specialEff = otherUnitInfo._unitEff;

                UnitBGGenerator.Instance.removeBG(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
                disableUnit(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
            }
            else if (otherUnitInfo._value == PuzzleGenerator.Instance.Unit.Length - 1)
            {
                infoList = PuzzleGenerator.Instance.getUnitsOfTypeInfo(focusedUnitInfo._value);
                specialEff = focusedUnitInfo._unitEff;

                UnitBGGenerator.Instance.removeBG(otherUnitInfo._XIndex, otherUnitInfo._YIndex);
                disableUnit(otherUnitInfo._XIndex, otherUnitInfo._YIndex);
            }

            // Upgrade all other normal units that have the same color with the special swapped units
            if(specialEff == UnitInfo.SpecialEff.explode)
            {
                for (int i = 0; i < infoList.Count; i++)
                {
                    PuzzleGenerator.Instance.upgradeUnit(infoList[i]._XIndex, infoList[i]._YIndex, specialEff, infoList[i]._negativeEff);
                    yield return new WaitForSeconds(0.05f);
                }
            }
            else if (specialEff == UnitInfo.SpecialEff.vLightning || specialEff == UnitInfo.SpecialEff.hLightning)
            {
                for (int i = 0; i < infoList.Count; i++)
                {
                    if (infoList[i]._unitEff == UnitInfo.SpecialEff.vLightning || infoList[i]._unitEff == UnitInfo.SpecialEff.hLightning)
                    {
                        continue;
                    }

                    int randomLighting = Random.Range(0, 2);
                    switch (randomLighting)
                    {
                        case 0:
                            {
                                PuzzleGenerator.Instance.upgradeUnit(infoList[i]._XIndex, infoList[i]._YIndex, UnitInfo.SpecialEff.vLightning, infoList[i]._negativeEff);
                                break;
                            }
                        case 1:
                            {
                                PuzzleGenerator.Instance.upgradeUnit(infoList[i]._XIndex, infoList[i]._YIndex, UnitInfo.SpecialEff.hLightning, infoList[i]._negativeEff);
                                break;
                            }
                    }
                    yield return new WaitForSeconds(0.05f);
                }
            }

            // Trigger all upgraded units
            for (int i = 0; i < infoList.Count; i++)
            {
                if (!_scanUnitARR[infoList[i]._XIndex, infoList[i]._YIndex]._isChained)
                {
                    switch (infoList[i]._unitEff)
                    {
                        case UnitInfo.SpecialEff.vLightning:
                            {
                                disableUnitEffect(infoList[i]);
                                destroyAllUnitsOfColumn(infoList[i]._XIndex, infoList[i]._YIndex);
                                break;
                            }
                        case UnitInfo.SpecialEff.hLightning:
                            {
                                disableUnitEffect(infoList[i]);
                                destroyAllUnitsOfRow(infoList[i]._XIndex, infoList[i]._YIndex);
                                break;
                            }
                        case UnitInfo.SpecialEff.explode:
                            {
                                disableUnitEffect(infoList[i]);
                                destroyAllLocalUnits(infoList[i]._XIndex, infoList[i]._YIndex);
                                break;
                            }
                    }           
                }
                yield return new WaitForSeconds(0.1f);
            }

            GameController.Instance.reduceMovesCount();
            yield return new WaitForSeconds(0.2f);
            StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle(true));
        }
        // Check if swapped unit is the special "Destroy all" type
        // If true destroy all jewels that has the same type with the other swapped unit
        else if (focusedUnitInfo._value == PuzzleGenerator.Instance.Unit.Length - 1)
        {
            // Disable the UnitBG that in the same position as the current destroyAll Unit
            UnitBGGenerator.Instance.removeBG(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);

            //destroyAllUnitsOfType(otherUnitInfo._value);
            StartCoroutine(destroyAllUnitsOfType(otherUnitInfo._value));
            disableUnit(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
            GameController.Instance.reduceMovesCount();

            yield return new WaitForSeconds(0.5f);
            StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle(true));
        }
        else if (otherUnitInfo._value == PuzzleGenerator.Instance.Unit.Length - 1)
        {
            // Disable the UnitBG that in the same position as the current destroyAll Unit
            UnitBGGenerator.Instance.removeBG(otherUnitInfo._XIndex, otherUnitInfo._YIndex);

            //destroyAllUnitsOfType(focusedUnitInfo._value);
            StartCoroutine(destroyAllUnitsOfType(focusedUnitInfo._value));
            disableUnit(otherUnitInfo._XIndex, otherUnitInfo._YIndex);
            GameController.Instance.reduceMovesCount();

            yield return new WaitForSeconds(0.5f);
            StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle(true));
        }
        else if ((focusedUnitInfo._unitEff == UnitInfo.SpecialEff.hLightning || focusedUnitInfo._unitEff == UnitInfo.SpecialEff.vLightning)
            && (otherUnitInfo._unitEff == UnitInfo.SpecialEff.hLightning || otherUnitInfo._unitEff == UnitInfo.SpecialEff.vLightning))
        {
            print("cross lightning");

            //disableUnit(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);

            disableUnitEffect(focusedUnitInfo);
            disableUnitEffect(otherUnitInfo);

            destroyAllUnitsOfColumn(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
            destroyAllUnitsOfRow(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);

            GameController.Instance.reduceMovesCount();
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle(true));
        }
        else if (((focusedUnitInfo._unitEff == UnitInfo.SpecialEff.hLightning || focusedUnitInfo._unitEff == UnitInfo.SpecialEff.vLightning)
            && otherUnitInfo._unitEff == UnitInfo.SpecialEff.explode)
            || ((otherUnitInfo._unitEff == UnitInfo.SpecialEff.hLightning || otherUnitInfo._unitEff == UnitInfo.SpecialEff.vLightning)
            && focusedUnitInfo._unitEff == UnitInfo.SpecialEff.explode))
        {
            print("explode lighting");

            disableUnitEffect(focusedUnitInfo);
            disableUnitEffect(otherUnitInfo);

            if (focusedUnitInfo._XIndex > 0)
            {
                destroyAllUnitsOfColumn(focusedUnitInfo._XIndex - 1, focusedUnitInfo._YIndex);
            }
            destroyAllUnitsOfColumn(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
            if (focusedUnitInfo._XIndex < PuzzleGenerator.Instance._columns - 1)
            {
                destroyAllUnitsOfColumn(focusedUnitInfo._XIndex + 1, focusedUnitInfo._YIndex);
            }

            if (focusedUnitInfo._YIndex > 0)
            {
                destroyAllUnitsOfRow(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex - 1);
            }
            destroyAllUnitsOfRow(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
            if (focusedUnitInfo._YIndex < PuzzleGenerator.Instance._rows - 1)
            {
                destroyAllUnitsOfRow(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex + 1);
            }

            GameController.Instance.reduceMovesCount();
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle(true));
        }
        else if (focusedUnitInfo._unitEff == UnitInfo.SpecialEff.explode && otherUnitInfo._unitEff == UnitInfo.SpecialEff.explode)
        {
            print("double explode");

            disableUnitEffect(focusedUnitInfo);
            disableUnitEffect(otherUnitInfo);

            destroyAllLocalUnits(focusedUnitInfo._XIndex, focusedUnitInfo._YIndex);
            destroyAllLocalUnits(otherUnitInfo._XIndex, otherUnitInfo._YIndex);

            GameController.Instance.reduceMovesCount();
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle(true));
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
                GameController.Instance.reduceMovesCount();
                yield return new WaitForSeconds(0.5f);
                StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle(true));
                //PuzzleGenerator.Instance.reOrganizePuzzle();
            }
            else
            {
                // Swap Units back
                yield return new WaitForSeconds(0.1f);

                SoundController.Instance.playOneShotClip(unSwapUnitSound);

                InputHandler.SwapType swapType = InputHandler.Instance.checkSwapable(focusedUnit, otherUnit);
                InputHandler.Instance.swapUnits(focusedUnit, otherUnit, swapType);
                yield return new WaitForSeconds(InputHandler.Instance.swapTime);
                GameController.Instance.currentState = GameController.GameState.idle;
            }
        }
    }

    public IEnumerator scanRegenUnits(List<GameObject> unitsList)
    {
        //updateScanARR();
        updateScanUnits(unitsList);
        chainedUnits = false;

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

        //yield return new WaitForSeconds(0.4f);
        if (chainedUnits)
        {
            yield return new WaitForSeconds(0.5f);
        }

        StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle(chainedUnits));

        //// If there are chained units
        //if (chainedUnits)
        //{
        //    StartCoroutine(PuzzleGenerator.Instance.reOrganizePuzzle());
        //    //PuzzleGenerator.Instance.reOrganizePuzzle();
        //}
        //else
        //{
        //    bonusPoint = 0;
        //    //-----------------------------------------------------------
        //    //--------------- Temporary added ------------------------
        //    //-----------------------------------------------------------

        //    //GameController.Instance.currentState = GameController.GameState.idle;

        //    //-----------------------------------------------------------
        //    //-----------------------------------------------------------


        //    //-----------------------------------------------------------
        //    //--------------- Temporary disabled ------------------------
        //    //-----------------------------------------------------------

        //    //StartCoroutine(PuzzleGenerator.Instance.scanHollowUnits());

        //    //-----------------------------------------------------------
        //    //-----------------------------------------------------------
        //}
    }

    #endregion

    #region Disable,Destroy and Special Destroy units methods

    /// <summary>
    /// Disable unit and it's effect and update scanARR
    /// </summary>
    /// <param name="XIndex"></param>
    /// <param name="YIndex"></param>
    public void disableUnit(int XIndex, int YIndex)
    {
        GameObject unit = PuzzleGenerator.Instance._unitARR[XIndex, YIndex];
        UnitInfo unitInfo = unit.GetComponent<UnitInfo>();
        unit.SetActive(false);
        _scanUnitARR[XIndex, YIndex]._isChained = true;

        switch (unitInfo._unitEff)
        {
            case (UnitInfo.SpecialEff.hLightning):
                {
                    unitInfo.HorizontalLightningEff.SetActive(false);
                    break;
                }
            case (UnitInfo.SpecialEff.vLightning):
                {
                    unitInfo.VerticalLightningEff.SetActive(false);
                    break;
                }
            case (UnitInfo.SpecialEff.explode):
                {
                    unitInfo.ExplosiveSparkEff.SetActive(false);
                    break;
                }
            default: break;
        }
        
        //switch (unitInfo._negativeEff)
        //{
        //    case (UnitInfo.NegativeEff.frozen):
        //        {
        //            unitInfo.FrozenEff.SetActive(false);
        //            break;
        //        }
        //    case (UnitInfo.NegativeEff.locked):
        //        {
        //            unitInfo.LockEff.SetActive(false);
        //            break;
        //        }
        //}

        if (unitInfo._value != PuzzleGenerator.Instance.Unit.Length - 1)
        {
            if (unitInfo.FrozenEff.activeSelf)
            {
                unitInfo.FrozenEff.SetActive(false);
            }
            if (unitInfo.LockEff.activeSelf)
            {
                unitInfo.LockEff.SetActive(false);
            }
        }        
    }

    #region Destroy Special Effect Units Method

    void destroyAllUnitsInPuzzle()
    {
        List<int> unitsXIndex = new List<int>();
        List<int> unitsYIndex = new List<int>();

        for (int YIndex = 0; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        {
            for (int XIndex = 0; XIndex < PuzzleGenerator.Instance._columns; XIndex++)
            {
                if (!_scanUnitARR[XIndex, YIndex]._isChained)
                {
                    unitsXIndex.Add(XIndex);
                    unitsYIndex.Add(YIndex);
                }                
            }
        }

        destroyUnits(unitsXIndex, unitsYIndex);
        SoundController.Instance.playOneShotClip(destroyAllUnitSound);
    }

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

                    //Instantiate(AllUnitsTypeDestroyEff, PuzzleGenerator.Instance._unitPosARR[XIndex, YIndex], Quaternion.identity);
                    //yield return new WaitForSeconds(0.02f);
                    //delayTime += 0.02f;
                }
            }
        }

        destroyUnits(unitsXIndex, unitsYIndex);

        SoundController.Instance.playOneShotClip(destroyAllUnitSound);

        for (int i = 0; i < unitsXIndex.Count; i++)
        {
            Instantiate(AllUnitsTypeDestroyEff, PuzzleGenerator.Instance._unitPosARR[unitsXIndex[i], unitsYIndex[i]], Quaternion.identity);
            yield return new WaitForSeconds(0.02f);
        }

    }

    void destroyAllUnitsOfColumn(int col, int YTarget)
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
        
        // Remove the targeted Unit effect to prevent infinity overlapped destroy effect
        PuzzleGenerator.Instance._unitARR[col, YTarget].GetComponent<UnitInfo>()._unitEff = UnitInfo.SpecialEff.noEff;
        destroyUnits(col, YTarget, unitsXIndex, unitsYIndex, _scanUnitARR[col, YTarget]._value, UnitInfo.SpecialEff.noEff);

        Instantiate(VLightningDestroyEff, new Vector2(PuzzleGenerator.Instance._unitPosARR[col, 0].x, 0f), VLightningDestroyEff.transform.rotation);
    }

    void destroyAllUnitsOfRow(int XTarget, int row)
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

        // Remove the targeted Unit effect to prevent infinity overlapped destroy effect
        PuzzleGenerator.Instance._unitARR[XTarget, row].GetComponent<UnitInfo>()._unitEff = UnitInfo.SpecialEff.noEff;
        destroyUnits(XTarget, row, unitsXIndex, unitsYIndex, _scanUnitARR[XTarget, row]._value, UnitInfo.SpecialEff.noEff);

        Instantiate(HLightningDestroyEff, new Vector2(0f, PuzzleGenerator.Instance._unitPosARR[0, row].y), HLightningDestroyEff.transform.rotation);
    }

    void destroyAllLocalUnits(int XTarget, int YTarget)
    {
        List<int> unitsXIndex = new List<int>();
        List<int> unitsYIndex = new List<int>();

        // Add local Units to disable list based on the targeted Unit position
        if (XTarget > 0 && XTarget < PuzzleGenerator.Instance._columns - 1 && YTarget > 0 && YTarget < PuzzleGenerator.Instance._rows - 1)
        {
            if (!_scanUnitARR[XTarget - 1, YTarget - 1]._isChained) { unitsXIndex.Add(XTarget - 1); unitsYIndex.Add(YTarget - 1); }
            if (!_scanUnitARR[XTarget, YTarget - 1]._isChained) { unitsXIndex.Add(XTarget); unitsYIndex.Add(YTarget - 1); }
            if (!_scanUnitARR[XTarget + 1, YTarget - 1]._isChained) { unitsXIndex.Add(XTarget + 1); unitsYIndex.Add(YTarget - 1); }
            if (!_scanUnitARR[XTarget - 1, YTarget]._isChained) { unitsXIndex.Add(XTarget - 1); unitsYIndex.Add(YTarget); }
            if (!_scanUnitARR[XTarget + 1, YTarget]._isChained) { unitsXIndex.Add(XTarget + 1); unitsYIndex.Add(YTarget); }
            if (!_scanUnitARR[XTarget - 1, YTarget + 1]._isChained) { unitsXIndex.Add(XTarget - 1); unitsYIndex.Add(YTarget + 1); }
            if (!_scanUnitARR[XTarget, YTarget + 1]._isChained) { unitsXIndex.Add(XTarget); unitsYIndex.Add(YTarget + 1); }
            if (!_scanUnitARR[XTarget + 1, YTarget + 1]._isChained) { unitsXIndex.Add(XTarget + 1); unitsYIndex.Add(YTarget + 1); }
        }
        else if (XTarget == 0 && YTarget == 0)
        {
            if (!_scanUnitARR[XTarget + 1, YTarget]._isChained) { unitsXIndex.Add(XTarget + 1); unitsYIndex.Add(YTarget); }
            if (!_scanUnitARR[XTarget, YTarget + 1]._isChained) { unitsXIndex.Add(XTarget); unitsYIndex.Add(YTarget + 1); }
            if (!_scanUnitARR[XTarget + 1, YTarget + 1]._isChained) { unitsXIndex.Add(XTarget + 1); unitsYIndex.Add(YTarget + 1); }
        }
        else if (XTarget == 0 && YTarget == PuzzleGenerator.Instance._rows - 1)
        {
            if (!_scanUnitARR[XTarget, YTarget - 1]._isChained) { unitsXIndex.Add(XTarget); unitsYIndex.Add(YTarget - 1); }
            if (!_scanUnitARR[XTarget + 1, YTarget - 1]._isChained) { unitsXIndex.Add(XTarget + 1); unitsYIndex.Add(YTarget - 1); }
            if (!_scanUnitARR[XTarget + 1, YTarget]._isChained) { unitsXIndex.Add(XTarget + 1); unitsYIndex.Add(YTarget); }
        }
        else if (XTarget == PuzzleGenerator.Instance._columns - 1 && YTarget == PuzzleGenerator.Instance._rows - 1)
        {
            if (!_scanUnitARR[XTarget - 1, YTarget - 1]._isChained) { unitsXIndex.Add(XTarget - 1); unitsYIndex.Add(YTarget - 1); }
            if (!_scanUnitARR[XTarget, YTarget - 1]._isChained) { unitsXIndex.Add(XTarget); unitsYIndex.Add(YTarget - 1); }
            if (!_scanUnitARR[XTarget - 1, YTarget]._isChained) { unitsXIndex.Add(XTarget - 1); unitsYIndex.Add(YTarget); }
        }
        else if (XTarget == PuzzleGenerator.Instance._columns - 1 && YTarget == 0)
        {
            if (!_scanUnitARR[XTarget - 1, YTarget]._isChained) { unitsXIndex.Add(XTarget - 1); unitsYIndex.Add(YTarget); }
            if (!_scanUnitARR[XTarget - 1, YTarget + 1]._isChained) { unitsXIndex.Add(XTarget - 1); unitsYIndex.Add(YTarget + 1); }
            if (!_scanUnitARR[XTarget, YTarget + 1]._isChained) { unitsXIndex.Add(XTarget); unitsYIndex.Add(YTarget + 1); }
        }
        else if (XTarget == 0)
        {
            if (!_scanUnitARR[XTarget, YTarget - 1]._isChained) { unitsXIndex.Add(XTarget); unitsYIndex.Add(YTarget - 1); }
            if (!_scanUnitARR[XTarget + 1, YTarget - 1]._isChained) { unitsXIndex.Add(XTarget + 1); unitsYIndex.Add(YTarget - 1); }
            if (!_scanUnitARR[XTarget + 1, YTarget]._isChained) { unitsXIndex.Add(XTarget + 1); unitsYIndex.Add(YTarget); }
            if (!_scanUnitARR[XTarget, YTarget + 1]._isChained) { unitsXIndex.Add(XTarget); unitsYIndex.Add(YTarget + 1); }
            if (!_scanUnitARR[XTarget + 1, YTarget + 1]._isChained) { unitsXIndex.Add(XTarget + 1); unitsYIndex.Add(YTarget + 1); }
        }
        else if (XTarget == PuzzleGenerator.Instance._columns - 1)
        {
            if (!_scanUnitARR[XTarget - 1, YTarget - 1]._isChained) { unitsXIndex.Add(XTarget - 1); unitsYIndex.Add(YTarget - 1); }
            if (!_scanUnitARR[XTarget, YTarget - 1]._isChained) { unitsXIndex.Add(XTarget); unitsYIndex.Add(YTarget - 1); }
            if (!_scanUnitARR[XTarget - 1, YTarget]._isChained) { unitsXIndex.Add(XTarget - 1); unitsYIndex.Add(YTarget); }
            if (!_scanUnitARR[XTarget - 1, YTarget + 1]._isChained) { unitsXIndex.Add(XTarget - 1); unitsYIndex.Add(YTarget + 1); }
            if (!_scanUnitARR[XTarget, YTarget + 1]._isChained) { unitsXIndex.Add(XTarget); unitsYIndex.Add(YTarget + 1); }
        }
        else if (YTarget == 0)
        {
            if (!_scanUnitARR[XTarget - 1, YTarget]._isChained) { unitsXIndex.Add(XTarget - 1); unitsYIndex.Add(YTarget); }
            if (!_scanUnitARR[XTarget + 1, YTarget]._isChained) { unitsXIndex.Add(XTarget + 1); unitsYIndex.Add(YTarget); }
            if (!_scanUnitARR[XTarget - 1, YTarget + 1]._isChained) { unitsXIndex.Add(XTarget - 1); unitsYIndex.Add(YTarget + 1); }
            if (!_scanUnitARR[XTarget, YTarget + 1]._isChained) { unitsXIndex.Add(XTarget); unitsYIndex.Add(YTarget + 1); }
            if (!_scanUnitARR[XTarget + 1, YTarget + 1]._isChained) { unitsXIndex.Add(XTarget + 1); unitsYIndex.Add(YTarget + 1); }
        }
        else if (YTarget == PuzzleGenerator.Instance._rows - 1)
        {
            if (!_scanUnitARR[XTarget - 1, YTarget - 1]._isChained) { unitsXIndex.Add(XTarget - 1); unitsYIndex.Add(YTarget - 1); }
            if (!_scanUnitARR[XTarget, YTarget - 1]._isChained) { unitsXIndex.Add(XTarget); unitsYIndex.Add(YTarget - 1); }
            if (!_scanUnitARR[XTarget + 1, YTarget - 1]._isChained) { unitsXIndex.Add(XTarget + 1); unitsYIndex.Add(YTarget - 1); }
            if (!_scanUnitARR[XTarget - 1, YTarget]._isChained) { unitsXIndex.Add(XTarget - 1); unitsYIndex.Add(YTarget); }
            if (!_scanUnitARR[XTarget + 1, YTarget]._isChained) { unitsXIndex.Add(XTarget + 1); unitsYIndex.Add(YTarget); }
        }

        // Remove the targeted Unit effect to prevent infinity overlapped destroy effect
        PuzzleGenerator.Instance._unitARR[XTarget, YTarget].GetComponent<UnitInfo>()._unitEff = UnitInfo.SpecialEff.noEff;
        destroyUnits(XTarget, YTarget, unitsXIndex, unitsYIndex, _scanUnitARR[XTarget, YTarget]._value, UnitInfo.SpecialEff.noEff);

        Instantiate(ExplodeDestroyEff, PuzzleGenerator.Instance._unitPosARR[XTarget, YTarget], Quaternion.identity);
    }

    #endregion

    /// <summary>
    /// Check Info of each unit destroyed by DestroyAllJewel to call the suitable destroy type
    /// </summary>
    /// <param name="unitsXIndex"></param>
    /// <param name="unitsYIndex"></param>
    void destroyUnits(List<int> unitsXIndex, List<int> unitsYIndex)
    {
        if (bonusPoint < maxBonusPoint)
        {
            bonusPoint += 10;
        }

        for (int i = 0; i < unitsYIndex.Count; i++)
        {
            UnitInfo unitInfo = PuzzleGenerator.Instance._unitARR[unitsXIndex[i], unitsYIndex[i]].GetComponent<UnitInfo>();

            if (unitInfo._negativeEff == UnitInfo.NegativeEff.frozen)
            {
                unitInfo._negativeEff = UnitInfo.NegativeEff.noEff;
                unitInfo.FrozenEff.GetComponent<NegativeEffController>().selfBreak();

                SoundController.Instance.playOneShotClip(frozenBreakSound);

                continue;
            }
            else if (unitInfo._negativeEff == UnitInfo.NegativeEff.locked)
            {
                unitInfo._negativeEff = UnitInfo.NegativeEff.noEff;
                unitInfo.LockEff.GetComponent<NegativeEffController>().selfBreak();

                SoundController.Instance.playOneShotClip(lockBreakSound);

                continue;
            }

            // Disable the UnitBG that in the same position as the current chained Unit
            UnitBGGenerator.Instance.removeBG(unitsXIndex[i], unitsYIndex[i]);

            disableUnit(unitsXIndex[i], unitsYIndex[i]);

            switch (unitInfo._unitEff)
            {
                case UnitInfo.SpecialEff.vLightning:
                    {
                        destroyAllUnitsOfColumn(unitsXIndex[i], unitsYIndex[i]);

                        SoundController.Instance.playOneShotClip(lightingDestroySound);

                        break;
                    }

                case UnitInfo.SpecialEff.hLightning:
                    {
                        destroyAllUnitsOfRow(unitsXIndex[i], unitsYIndex[i]);

                        SoundController.Instance.playOneShotClip(lightingDestroySound);

                        break;
                    }

                case UnitInfo.SpecialEff.explode:
                    {
                        destroyAllLocalUnits(unitsXIndex[i], unitsYIndex[i]);

                        SoundController.Instance.playOneShotClip(explosionDestroySound);

                        break;
                    }

                case UnitInfo.SpecialEff.noEff:
                    {
                        activateDestroyEffAnimation(unitInfo._value, PuzzleGenerator.Instance._unitPosARR[unitInfo._XIndex, unitInfo._YIndex]);

                        // Activate bonus unit score text
                        getUnitScoreText(bonusPoint, PuzzleGenerator.Instance._unitPosARR[unitInfo._XIndex, unitInfo._YIndex]);
                        GameController.Instance.updateScore(bonusPoint);

                        SoundController.Instance.playSingleClip(unitDestroySound);

                        break;
                    }
                default: break;
            }
        }
    }

    /// <summary>
    /// Check Swapped or Regenerated Units Info to call the suitable destroy type
    /// </summary>
    /// <param name="Xtarget">The X index of the targeted Unit</param>
    /// <param name="Ytarget">The Y index of the targeted Unit</param>
    /// <param name="unitsXIndex">List of all X indexs of other units that are in chain</param>
    /// <param name="unitsYIndex">List of all Y indexs of other units that are in chain</param>
    /// <param name="targetNextEff"></param>
    void destroyUnits(int Xtarget, int Ytarget, List<int> unitsXIndex, List<int> unitsYIndex, int targetNextValue, UnitInfo.SpecialEff targetNextEff)
    {
        // Auto correct variables
        if (targetNextValue == PuzzleGenerator.Instance.Unit.Length - 1)
        {
            targetNextEff = UnitInfo.SpecialEff.noEff;
        }

        UnitInfo targetInfo = PuzzleGenerator.Instance._unitARR[Xtarget, Ytarget].GetComponent<UnitInfo>();
        if (targetInfo._negativeEff == UnitInfo.NegativeEff.frozen)
        {
            targetInfo._negativeEff = UnitInfo.NegativeEff.noEff;
            targetInfo.FrozenEff.GetComponent<NegativeEffController>().selfBreak();

            SoundController.Instance.playOneShotClip(frozenBreakSound);
        }
        else if (targetInfo._negativeEff == UnitInfo.NegativeEff.locked)
        {
            targetInfo._negativeEff = UnitInfo.NegativeEff.noEff;
            targetInfo.LockEff.GetComponent<NegativeEffController>().selfBreak();

            SoundController.Instance.playOneShotClip(lockBreakSound);
        }
        else
        {
            // Disable the UnitBG that in the same position as the targeted Unit
            UnitBGGenerator.Instance.removeBG(Xtarget, Ytarget);

            disableUnit(Xtarget, Ytarget);
        }

        #region Target Unit has special effect
        if (PuzzleGenerator.Instance._unitARR[Xtarget, Ytarget].GetComponent<UnitInfo>()._unitEff != UnitInfo.SpecialEff.noEff)
        {
            // Trigger it's Eff
            switch (PuzzleGenerator.Instance._unitARR[Xtarget, Ytarget].GetComponent<UnitInfo>()._unitEff)
            {
                case UnitInfo.SpecialEff.vLightning:
                    {
                        destroyAllUnitsOfColumn(Xtarget, Ytarget);

                        SoundController.Instance.playOneShotClip(lightingDestroySound);

                        break;
                    }

                case UnitInfo.SpecialEff.hLightning:
                    {
                        destroyAllUnitsOfRow(Xtarget, Ytarget);

                        SoundController.Instance.playOneShotClip(lightingDestroySound);

                        break;
                    }

                case UnitInfo.SpecialEff.explode:
                    {
                        destroyAllLocalUnits(Xtarget, Ytarget);

                        SoundController.Instance.playOneShotClip(explosionDestroySound);

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
                            destroyAllUnitsOfColumn(Xtarget, Ytarget);

                            SoundController.Instance.playOneShotClip(lightingDestroySound);

                            break;
                        }

                    case UnitInfo.SpecialEff.hLightning:
                        {
                            destroyAllUnitsOfRow(Xtarget, Ytarget);

                            SoundController.Instance.playOneShotClip(lightingDestroySound);

                            break;
                        }

                    case UnitInfo.SpecialEff.explode:
                        {
                            destroyAllLocalUnits(Xtarget, Ytarget);

                            SoundController.Instance.playOneShotClip(explosionDestroySound);

                            break;
                        }

                    default: break;
                }
            }
            // Then destroy all other unit that are in chain
            if (bonusPoint < maxBonusPoint)
            {
                bonusPoint += 10;
            }

            for (int i = 0; i < unitsYIndex.Count; i++)
            {
                UnitInfo unitInfo = PuzzleGenerator.Instance._unitARR[unitsXIndex[i], unitsYIndex[i]].GetComponent<UnitInfo>();

                if (unitInfo._negativeEff == UnitInfo.NegativeEff.frozen)
                {
                    unitInfo._negativeEff = UnitInfo.NegativeEff.noEff;
                    unitInfo.FrozenEff.GetComponent<NegativeEffController>().selfBreak();

                    SoundController.Instance.playOneShotClip(frozenBreakSound);

                    continue;
                }
                else if (unitInfo._negativeEff == UnitInfo.NegativeEff.locked)
                {
                    unitInfo._negativeEff = UnitInfo.NegativeEff.noEff;
                    unitInfo.LockEff.GetComponent<NegativeEffController>().selfBreak();

                    SoundController.Instance.playOneShotClip(lockBreakSound);

                    continue;
                }

                // Disable the UnitBG that in the same position as the current chained Unit
                UnitBGGenerator.Instance.removeBG(unitsXIndex[i], unitsYIndex[i]);

                disableUnit(unitsXIndex[i], unitsYIndex[i]);

                if (unitInfo._value
                    == PuzzleGenerator.Instance.Unit.Length - 1)
                {
                    StartCoroutine(destroyAllUnitsOfType(PuzzleGenerator.Instance._unitARR[Xtarget, Ytarget].GetComponent<UnitInfo>()._value));
                }
                else
                {
                    switch (unitInfo._unitEff)
                    {
                        case UnitInfo.SpecialEff.vLightning:
                            {
                                destroyAllUnitsOfColumn(unitsXIndex[i], unitsYIndex[i]);

                                SoundController.Instance.playOneShotClip(lightingDestroySound);

                                break;
                            }

                        case UnitInfo.SpecialEff.hLightning:
                            {
                                destroyAllUnitsOfRow(unitsXIndex[i], unitsYIndex[i]);

                                SoundController.Instance.playOneShotClip(lightingDestroySound);

                                break;
                            }

                        case UnitInfo.SpecialEff.explode:
                            {
                                destroyAllLocalUnits(unitsXIndex[i], unitsYIndex[i]);

                                SoundController.Instance.playOneShotClip(explosionDestroySound);

                                break;
                            }

                        case UnitInfo.SpecialEff.noEff:
                            {
                                activateDestroyEffAnimation(unitInfo._value, PuzzleGenerator.Instance._unitPosARR[unitInfo._XIndex, unitInfo._YIndex]);

                                // Activate bonus unit score text
                                getUnitScoreText(bonusPoint, PuzzleGenerator.Instance._unitPosARR[unitInfo._XIndex, unitInfo._YIndex]);
                                GameController.Instance.updateScore(bonusPoint);

                                SoundController.Instance.playSingleClip(unitDestroySound);

                                break;
                            }

                        default: break;
                    }
                }
            }
        }
        #endregion
        // If the target doesn't has any special Eff
        else
        {
            //if (targetInfo._negativeEff != UnitInfo.NegativeEff.noEff)
            //{

            //}
            // If formed special chained type,upgrade it
            if (targetNextValue == PuzzleGenerator.Instance.Unit.Length - 1)
            {
                PuzzleGenerator.Instance.upgradeToDestroyAllUnit(Xtarget, Ytarget);
            }
            else if (targetNextEff != UnitInfo.SpecialEff.noEff)
            {
                PuzzleGenerator.Instance.upgradeUnit(Xtarget, Ytarget, targetNextEff);
            }
            else
            {
                activateDestroyEffAnimation(_scanUnitARR[Xtarget, Ytarget]._value, PuzzleGenerator.Instance._unitPosARR[Xtarget, Ytarget]);

                // Activate bonus unit score text
                if (bonusPoint < maxBonusPoint)
                {
                    getUnitScoreText(bonusPoint + 10, PuzzleGenerator.Instance._unitPosARR[Xtarget, Ytarget]);
                    GameController.Instance.updateScore(bonusPoint + 10);
                }
                else
                {
                    getUnitScoreText(maxBonusPoint, PuzzleGenerator.Instance._unitPosARR[Xtarget, Ytarget]);
                    GameController.Instance.updateScore(maxBonusPoint);
                }
            }

            // Then destroy all other unit that are in chain
            if (bonusPoint < maxBonusPoint)
            {
                bonusPoint += 10;
            }

            for (int i = 0; i < unitsYIndex.Count; i++)
            {
                UnitInfo unitInfo = PuzzleGenerator.Instance._unitARR[unitsXIndex[i], unitsYIndex[i]].GetComponent<UnitInfo>();

                if (unitInfo._negativeEff == UnitInfo.NegativeEff.frozen)
                {
                    unitInfo._negativeEff = UnitInfo.NegativeEff.noEff;
                    unitInfo.FrozenEff.GetComponent<NegativeEffController>().selfBreak();

                    SoundController.Instance.playOneShotClip(frozenBreakSound);

                    continue;
                }
                else if (unitInfo._negativeEff == UnitInfo.NegativeEff.locked)
                {
                    unitInfo._negativeEff = UnitInfo.NegativeEff.noEff;
                    unitInfo.LockEff.GetComponent<NegativeEffController>().selfBreak();

                    SoundController.Instance.playOneShotClip(frozenBreakSound);

                    continue;
                }

                // Disable the UnitBG that in the same position as the current chained Unit
                UnitBGGenerator.Instance.removeBG(unitsXIndex[i], unitsYIndex[i]);

                disableUnit(unitsXIndex[i], unitsYIndex[i]);

                if (unitInfo._value
                    == PuzzleGenerator.Instance.Unit.Length - 1)
                {
                    StartCoroutine(destroyAllUnitsOfType(PuzzleGenerator.Instance._unitARR[Xtarget, Ytarget].GetComponent<UnitInfo>()._value));
                }
                else
                {
                    switch (unitInfo._unitEff)
                    {
                        case UnitInfo.SpecialEff.vLightning:
                            {
                                destroyAllUnitsOfColumn(unitsXIndex[i], unitsYIndex[i]);

                                SoundController.Instance.playOneShotClip(lightingDestroySound);

                                break;
                            }

                        case UnitInfo.SpecialEff.hLightning:
                            {
                                destroyAllUnitsOfRow(unitsXIndex[i], unitsYIndex[i]);

                                SoundController.Instance.playOneShotClip(lightingDestroySound);

                                break;
                            }

                        case UnitInfo.SpecialEff.explode:
                            {
                                destroyAllLocalUnits(unitsXIndex[i], unitsYIndex[i]);

                                SoundController.Instance.playOneShotClip(explosionDestroySound);

                                break;
                            }

                        case UnitInfo.SpecialEff.noEff:
                            {
                                activateDestroyEffAnimation(unitInfo._value, PuzzleGenerator.Instance._unitPosARR[unitInfo._XIndex, unitInfo._YIndex]);

                                // Activate bonus unit score text
                                getUnitScoreText(bonusPoint, PuzzleGenerator.Instance._unitPosARR[unitInfo._XIndex, unitInfo._YIndex]);
                                GameController.Instance.updateScore(bonusPoint);

                                SoundController.Instance.playSingleClip(unitDestroySound);

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

    #region Scan for specified chained units types methods

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

    public void activateDestroyEffAnimation(int value, Vector2 spawnPos)
    {
        GameObject destroyEff = null;

        for (int i = 0; i < specialEffPoolSize; i++)
        {
            destroyEff = _specialEffPoolARR[value][i];
            if (!destroyEff.activeSelf)
            {
                destroyEff.transform.position = spawnPos;
                destroyEff.SetActive(true);
                break;
            }
        }
    }

    public void getUnitScoreText(int value, Vector2 spawnPos)
    {
        Text unitScoreText = null;

        for (int i = 0; i < unitScoreTextPoolSize; i++)
        {
            unitScoreText = unitScoreTextPool[i];
            if (!unitScoreText.gameObject.activeSelf)
            {
                unitScoreText.transform.position = spawnPos;
                unitScoreText.text = value.ToString();
                unitScoreText.gameObject.SetActive(true);
                break;
            }
        }
    }

    private void disableUnitEffect(UnitInfo unitInfo)
    {
        switch (unitInfo._unitEff)
        {
            case (UnitInfo.SpecialEff.hLightning):
                {
                    unitInfo.HorizontalLightningEff.SetActive(false);
                    break;
                }
            case (UnitInfo.SpecialEff.vLightning):
                {
                    unitInfo.VerticalLightningEff.SetActive(false);
                    break;
                }
            case (UnitInfo.SpecialEff.explode):
                {
                    unitInfo.ExplosiveSparkEff.SetActive(false);
                    break;
                }
            default: break;
        }

        unitInfo._unitEff = UnitInfo.SpecialEff.noEff;
    }
}
