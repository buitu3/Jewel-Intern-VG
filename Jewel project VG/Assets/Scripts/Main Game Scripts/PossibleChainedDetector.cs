using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class PossibleChainedDetector : MonoBehaviour {

    //==============================================
    // Constants
    //==============================================

    public static PossibleChainedDetector Instance;

    //==============================================
    // Fields
    //==============================================

    private int[,] _valueARR;

    private float stallTime = 0f;

    private bool suggesting = false;

    private Tween[] suggestTween = new Tween[5];

    private Vector2 zoomOutScale = new Vector2(1.4f, 1.4f);

    private IEnumerator suggestCoroutine;

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        //suggestCoroutine = suggestMove();

        _valueARR = new int[PuzzleGenerator.Instance._columns, PuzzleGenerator.Instance._rows];
        yield return new WaitForEndOfFrame();
        updateValueARR();        
    }

    void FixedUpdate()
    {
        if ((GameController.Instance.currentState != GameController.GameState.idle && suggesting))
        {
            stopBlow();
            suggesting = false;

            //StopCoroutine(suggestCoroutine);
            //StopCoroutine(suggestMove());
            StopAllCoroutines();
        }

        if (GameController.Instance.currentState == GameController.GameState.idle && !suggesting)
        {
            List<GameObject> suggestList = scanPossibleChained();
            if (suggestList.Count > 0)
            {
                suggesting = true;
                //StartCoroutine(suggestCoroutine);
                StartCoroutine(suggestMove(suggestList));
            }
            else
            {
                print("no more move");
                StartCoroutine(PuzzleShuffle.Instance.shufflePuzzle());
            }
        }
    }

    //==============================================
    // Methods
    //==============================================

    // Sync the scanARR with the current puzzle
    public void updateValueARR()
    {
        for (int YIndex = 0; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        {
            for (int XIndex = 0; XIndex < PuzzleGenerator.Instance._columns; XIndex++)
            {
                //_valueARR[XIndex, YIndex] = PuzzleGenerator.Instance._unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._value;
                if (!ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained)
                {
                    _valueARR[XIndex, YIndex] = ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._value;
                }
                else
                {
                    _valueARR[XIndex, YIndex] = -1;
                }
            }
        }
    }

    void blowUnits(List<GameObject> unitList)
    {
        for (int i = 0; i < unitList.Count; i++)
        {
            //suggestTween[i] = unitList[i].transform.DOPunchScale(punchScale, 3f, 0, 1f).SetLoops(-1).SetEase(Ease.Linear);
            suggestTween[i] = unitList[i].transform.DOScale(zoomOutScale, 1.2f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    void stopBlow()
    {
        for (int i = 0; i < suggestTween.Length; i++)
        {
            suggestTween[i].Restart();
            suggestTween[i].Kill();
        }
    }

    public IEnumerator suggestMove(List<GameObject> suggestList)
    {
        //suggesting = true;

        //List<GameObject> suggestList = scanPossibleChained();

        //if (suggestList.Count > 0)
        //{
        //    blowUnits(suggestList);
        //}
        //else
        //{
        //    print("no more move");
        //    StartCoroutine(PuzzleShuffle.Instance.shufflePuzzle());
        //}

        yield return new WaitForSeconds(5f);
        blowUnits(suggestList);

        //while (true)
        //{
        //    yield return new WaitForSeconds(2f);
        //    print("still suggest");
        //}
    }

    public List<GameObject> scanPossibleChained()
    {
        updateValueARR();

        List<GameObject> suggestList = new List<GameObject>();

        for (int YIndex = 0; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        {
            for (int XIndex = 0; XIndex < PuzzleGenerator.Instance._columns; XIndex++)
            {
                int value = _valueARR[XIndex, YIndex];
                UnitInfo unitInfo = PuzzleGenerator.Instance._unitARR[XIndex, YIndex].GetComponent<UnitInfo>();

                if (ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained 
                    || unitInfo._negativeEff != UnitInfo.NegativeEff.noEff)
                {
                    continue;
                }

                #region Special Units check
                if (value == PuzzleGenerator.Instance.Unit.Length - 1)
                {
                    if (YIndex > 0 
                        && PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff 
                        && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex - 1]._isChained)
                    {
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1]);
                        return suggestList;
                    }

                    if (YIndex < PuzzleGenerator.Instance._rows - 1
                        && PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                        && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex + 1]._isChained)
                    {
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1]);
                        return suggestList;
                    }

                    if (XIndex > 0
                        && PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                        && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex - 1, YIndex]._isChained)
                    {
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex]);
                        return suggestList;
                    }

                    if (XIndex < PuzzleGenerator.Instance._columns - 1
                        && PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                        && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex + 1, YIndex]._isChained)
                    {
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex]);
                        return suggestList;
                    }
                }

                if (unitInfo._unitEff != UnitInfo.SpecialEff.noEff)
                {
                    if (YIndex > 0
                        && PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].GetComponent<UnitInfo>()._unitEff != UnitInfo.SpecialEff.noEff
                        && PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                        && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex - 1]._isChained)
                    {
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1]);
                        return suggestList;
                    }

                    if (YIndex < PuzzleGenerator.Instance._rows - 1
                        && PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].GetComponent<UnitInfo>()._unitEff != UnitInfo.SpecialEff.noEff
                        && PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                        && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex + 1]._isChained)
                    {
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1]);
                        return suggestList;
                    }

                    if (XIndex > 0
                        && PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].GetComponent<UnitInfo>()._unitEff != UnitInfo.SpecialEff.noEff
                        && PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                        && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex - 1, YIndex]._isChained)
                    {
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex]);
                        return suggestList;
                    }

                    if (XIndex < PuzzleGenerator.Instance._columns - 1
                        && PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].GetComponent<UnitInfo>()._unitEff != UnitInfo.SpecialEff.noEff
                        && PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                        && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex + 1, YIndex]._isChained)
                    {
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                        suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex]);
                        return suggestList;
                    }
                }

                #endregion

                #region vertical check
                if (isNearLeftUpMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex + 2]);
                    return suggestList;
                }

                if (isNearMiddleUpMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 3]);
                    return suggestList;
                }

                if (isNearRightUpMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex + 2]);
                    return suggestList;
                }

                if (isNearLeftDownMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex - 1]);
                    return suggestList;
                }

                if (isNearMiddleDownMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 2]);
                    return suggestList;
                }

                if (isNearRightDownMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex - 1]);
                    return suggestList;
                }

                if (isFarLeftMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 2]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex + 1]);
                    return suggestList;
                }

                if (isFarRightMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 2]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex + 1]);
                    return suggestList;
                }
                #endregion

                #region horizontal check
                if (isNearDownLeftMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex - 1]);
                    return suggestList;
                }

                if (isNearMiddleLeftMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 3, YIndex]);
                    return suggestList;
                }

                if (isNearUpLeftMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex + 1]);
                    return suggestList;
                }

                if (isNearDownRightMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex - 1]);
                    return suggestList;
                }

                if (isNearMiddleRightMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex + 2, YIndex]);
                    return suggestList;
                }

                if (isNearUpRightMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex + 1]);
                    return suggestList;
                }

                if (isFarDownMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex    ]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex    ]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex - 1]);
                    return suggestList;
                }

                if (isFarUpMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex    ]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex    ]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex + 1]);
                    return suggestList;
                }

                #endregion
            }
        }
        return suggestList;
    }

    #region vertical possible match scan methods

    bool isNearLeftUpMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex > 0 && YIndex < PuzzleGenerator.Instance._rows - 2)
        {
            if (_valueARR[XIndex, YIndex + 1] == value && _valueARR[XIndex - 1, YIndex + 2] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 2].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex + 2].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex , YIndex + 2]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isNearMiddleUpMatch(int XIndex, int YIndex, int value)
    {
        if (YIndex < PuzzleGenerator.Instance._rows - 3)
        {
            if (_valueARR[XIndex, YIndex + 1] == value && _valueARR[XIndex    , YIndex + 3] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 2].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 3].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex + 2]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isNearRightUpMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex < PuzzleGenerator.Instance._columns - 1 && YIndex < PuzzleGenerator.Instance._rows - 2)
        {
            if (_valueARR[XIndex, YIndex + 1] == value && _valueARR[XIndex + 1, YIndex + 2] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 2].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex + 2].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex + 2]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isNearLeftDownMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex > 0 && YIndex > 0 && YIndex < PuzzleGenerator.Instance._rows - 1)
        {
            if (_valueARR[XIndex, YIndex + 1] == value && _valueARR[XIndex - 1, YIndex - 1] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex - 1]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isNearMiddleDownMatch(int XIndex, int YIndex, int value)
    {
        if (YIndex > 1 && YIndex < PuzzleGenerator.Instance._rows - 1)
        {
            if (_valueARR[XIndex, YIndex + 1] == value && _valueARR[XIndex    , YIndex - 2] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 2].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex - 1]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isNearRightDownMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex < PuzzleGenerator.Instance._columns - 1 && YIndex > 0 && YIndex < PuzzleGenerator.Instance._rows - 1)
        {
            if (_valueARR[XIndex, YIndex + 1] == value && _valueARR[XIndex + 1, YIndex - 1] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex - 1]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isFarLeftMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex > 0 && YIndex < PuzzleGenerator.Instance._rows - 2)
        {
            if (_valueARR[XIndex, YIndex + 2] == value && _valueARR[XIndex - 1, YIndex + 1] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex + 1]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isFarRightMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex < PuzzleGenerator.Instance._columns - 1 && YIndex < PuzzleGenerator.Instance._rows - 2)
        {
            if (_valueARR[XIndex, YIndex + 2] == value && _valueARR[XIndex + 1, YIndex + 1] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex + 1]._isChained)
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Horizontal possible match scan methods
    bool isNearDownLeftMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex > 1 && YIndex > 0)
        {
            if (_valueARR[XIndex - 1, YIndex    ] == value && _valueARR[XIndex - 2, YIndex - 1] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex - 2, YIndex]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isNearMiddleLeftMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex > 2)
        {
            if (_valueARR[XIndex - 1, YIndex] == value && _valueARR[XIndex - 3, YIndex    ] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex - 3, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex - 2, YIndex]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isNearUpLeftMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex > 1 && YIndex < PuzzleGenerator.Instance._rows - 1)
        {
            if (_valueARR[XIndex - 1, YIndex] == value && _valueARR[XIndex - 2, YIndex + 1] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex - 2, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex - 2, YIndex]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isNearDownRightMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex > 0 && XIndex < PuzzleGenerator.Instance._columns - 1 && YIndex > 0)
        {
            if (_valueARR[XIndex - 1, YIndex] == value && _valueARR[XIndex + 1, YIndex - 1] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex + 1, YIndex]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isNearMiddleRightMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex > 0 && XIndex < PuzzleGenerator.Instance._columns - 2)
        {
            if (_valueARR[XIndex - 1, YIndex] == value && _valueARR[XIndex + 2, YIndex    ] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex + 2, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex + 1, YIndex]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isNearUpRightMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex > 0 && XIndex < PuzzleGenerator.Instance._columns - 1 && YIndex < PuzzleGenerator.Instance._rows - 1)
        {
            if (_valueARR[XIndex - 1, YIndex] == value && _valueARR[XIndex + 1, YIndex + 1] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex + 1, YIndex]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isFarDownMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex > 1 && YIndex > 0)
        {
            if (_valueARR[XIndex - 2, YIndex] == value && _valueARR[XIndex - 1, YIndex - 1] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex - 1, YIndex]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    bool isFarUpMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex > 1 && YIndex < PuzzleGenerator.Instance._rows - 1)
        {
            if (_valueARR[XIndex - 2, YIndex] == value && _valueARR[XIndex - 1, YIndex + 1] == value && value >= 0
                && PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff
                && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex - 1, YIndex]._isChained)
            {
                return true;
            }
        }
        return false;
    }

    #endregion
}
