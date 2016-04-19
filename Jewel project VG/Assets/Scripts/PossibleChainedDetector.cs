﻿using UnityEngine;
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

    private Tween[] suggestTween = new Tween[3];

    private Vector2 punchScale = new Vector2(1.4f, 1.4f);

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
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
        }
        if (GameController.Instance.currentState == GameController.GameState.idle && !suggesting)
        {
            stallTime += Time.fixedDeltaTime;
            if (stallTime > 2f)
            {
                suggestMove();
                stallTime = 0f;
                suggesting = true;
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
                //if (!ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained)
                //{
                //    _valueARR[XIndex, YIndex] = ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._value;

                //}
            }
        }
    }

    void blowUnits(List<GameObject> unitList)
    {
        for (int i = 0; i < suggestTween.Length; i++)
        {
            //suggestTween[i] = unitList[i].transform.DOPunchScale(punchScale, 3f, 0, 1f).SetLoops(-1).SetEase(Ease.Linear);
            suggestTween[i] = unitList[i].transform.DOScale(punchScale, 2f).SetLoops(-1, LoopType.Yoyo);
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

    void suggestMove()
    {
        List<GameObject> suggestList = scanPossibleChained();
        if (suggestList.Count > 0)
        {
            blowUnits(suggestList);
            print("asdf");
            for (int i = 0;i < suggestList.Count; i++)
            {
                print(suggestList[i].GetComponent<UnitInfo>()._XIndex + "---" + suggestList[i].GetComponent<UnitInfo>()._YIndex);
            }
        }
        else
        {
            print("no more move");
        }
    }

    List<GameObject> scanPossibleChained()
    {
        updateValueARR();

        List<GameObject> suggestList = new List<GameObject>();

        for (int YIndex = 0; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        {
            for (int XIndex = 0; XIndex < PuzzleGenerator.Instance._columns; XIndex++)
            {
                int value = _valueARR[XIndex, YIndex];

                //if (isNearLeftUpMatch(XIndex, YIndex, value))
                //{
                //    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex    ]);
                //    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex + 1]);
                //    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex + 2]);
                //}

                //if (isNearMiddleUpMatch(XIndex, YIndex, value))
                //{
                //    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex    ]);
                //    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex + 1]);
                //    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex + 3]);
                //}

                //if (isNearRightUpMatch(XIndex, YIndex, value))
                //{
                //    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex    ]);
                //    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex + 1]);
                //    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex + 2]);
                //}

                if (isNearLeftDownMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex    ]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex + 1]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex - 1]);
                }

                if (isNearMiddleDownMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex    ]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex + 1]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex - 2]);
                }

                if (isNearRightDownMatch(XIndex, YIndex, value))
                {
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex    ]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex    , YIndex + 1]);
                    suggestList.Add(PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex - 1]);
                }
            }
        }

        return suggestList;
    }

    #region vertical possible match scan methods

    bool isNearLeftUpMatch(int XIndex, int YIndex, int value)
    {
        if (XIndex > 0 && YIndex < PuzzleGenerator.Instance._rows - 2)
        {
            if (_valueARR[XIndex, YIndex + 1] == value && _valueARR[XIndex - 1, YIndex + 2] == value)
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
            if (_valueARR[XIndex, YIndex + 1] == value && _valueARR[XIndex    , YIndex + 3] == value)
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
            if (_valueARR[XIndex, YIndex + 1] == value && _valueARR[XIndex + 1, YIndex + 2] == value)
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
            if (_valueARR[XIndex, YIndex + 1] == value && _valueARR[XIndex - 1, YIndex - 1] == value)
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
            if (_valueARR[XIndex, YIndex + 1] == value && _valueARR[XIndex    , YIndex - 2] == value)
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
            if (_valueARR[XIndex, YIndex + 1] == value && _valueARR[XIndex + 1, YIndex - 1] == value)
            {
                return true;
            }
        }
        return false;
    }

    //bool isFarLeftMatch(int XIndex, int YIndex, int value)
    //{
    //    if (XIndex < PuzzleGenerator.Instance._columns - 2 && YIndex > 0)
    //    {
    //        if (_valueARR[XIndex, YIndex + 2] == value && _valueARR[XIndex - 1, YIndex + 1] == value)
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //bool isFarRightMatch(int XIndex, int YIndex, int value)
    //{
    //    if (XIndex < PuzzleGenerator.Instance._columns - 2 && YIndex < PuzzleGenerator.Instance._columns - 1)
    //    {
    //        if (_valueARR[XIndex, YIndex + 2] == value && _valueARR[XIndex - 1, YIndex + 1] == value)
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    #endregion
}
