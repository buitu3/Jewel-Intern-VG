using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class PuzzleGenerator : MonoBehaviour {

    //==============================================
    // Constants
    //==============================================

    public static PuzzleGenerator Instance;

    //==============================================
    // Fields
    //==============================================

    public GameObject[] Unit;
    public GameObject UnitBGPreb;
    public Transform startingSpawnPos;

    public TextAsset levelsInfo;
    private JSONObject levelsInfoJSON; 

    public int _rows;
    public int _columns;

    public int[,] _valueARR;
    public GameObject[,] _unitARR;
    public Vector2[,] _unitPosARR;
    private GameObject[,] _unitBGARR;

    public AnimationCurve curveTween;

    private List<GameObject>[] _poolARR;

    private List<GameObject> unitsList;
    private List<GameObject> temporaryPushRightUnitList;
    private List<Tween> waitTweenList;
    private List<int[]> unbounceList;

    private Transform unitHolder;
    public Transform unitBGHolder;
    private Transform poolObjectHolder;

    private bool canPushUnit;
    private bool hasUnitToRegen;
    private bool hasBouncingUnit;
    private bool pushingDelay;

    //private float XStartPos = -2.6f;
    //private float YStartPos = -3.7f;
    private float XPadding = 0.73f;
    private float YPadding = 0.73f;
    private float regenYpos;

    private float unitDropTime = 0.3f;
    private float regenUnitDropTime = 0.3f;
    private float baseUnitDropTime = 0.1f;
    private float unitPushTime = 0.1f;
    private float unitBounceTime = 0.2f;

    private int turnsToUpgradeRandomLightningUnit = 4;
    [HideInInspector]
    public int turnCountToUpgrade = 0;

    private int poolSize;

    private enum unitPushType
    {
        None = 0,
        Left,
        Right,
        typeCounts
    }

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {

        Instance = this;
        //Time.timeScale = 0.5f;
    }

    IEnumerator Start()
    {
        GameController.Instance.currentState = GameController.GameState.initPuzzle;

        unitHolder = new GameObject("Units Holder").transform;
        unitBGHolder = new GameObject("Units BG Holder").transform;
        poolObjectHolder = new GameObject("Pool Object Holder").transform;

        unbounceList = new List<int[]>();

        regenYpos = startingSpawnPos.transform.position.y + (_rows - 1) * YPadding;

        waitTweenList = new List<Tween>();

        // Init valueArr
        _valueARR = generateValueMatrix();
        // Init unitArr
        _unitARR = new GameObject[_columns, _rows];
        // Init unitPosArr
        _unitPosARR = new Vector2[_columns, _rows];
        // Init unitBGArr
        _unitBGARR = new GameObject[_columns, _rows];

        // Init all Object pools
        _poolARR = new List<GameObject>[Unit.Length];
        Vector2 poolPos = new Vector2(-20, -20);
        poolSize = _rows * _columns / 2;
        for (int i = 0; i < _poolARR.Length; i++)
        {
            _poolARR[i] = new List<GameObject>();
            for (int j = 0; j < poolSize; j++)
            {
                GameObject jewel = Instantiate(Unit[i], poolPos, Quaternion.identity) as GameObject;
                jewel.SetActive(false);
                _poolARR[i].Add(jewel);
                jewel.transform.SetParent(poolObjectHolder);
            }
        }

        // Fake unit value for testing
        //for (int i = 0; i < _columns; i++)
        //{
        //    for (int j = 0; j < _rows; j++)
        //    {
        //        _valueARR[i, j] = 0;
        //    }
        //}
        //for (int j = 0; j < _rows; j++)
        //{
        //    _valueARR[7, j] = 2;
        //}
        //_valueARR[1, 1] = 2;
        //_valueARR[2, 2] = 1;
        //_valueARR[3, 2] = 4;
        //_valueARR[2, 5] = 7;
        //_valueARR[2, 3] = 4;
        //_valueARR[3, 3] = 4;
        //_valueARR[4, 4] = 4;

        //_valueARR[2, 7] = 4;
        //_valueARR[3, 7] = 4;
        //_valueARR[4, 7] = 4;
        //_valueARR[5, 7] = 4;
        //_valueARR[7, 7] = 4;

        //_valueARR[4, 2] = 3;
        //_valueARR[5, 2] = 2;

        if (LevelsManager.Instance.selectedLevel == 7)
        {
            //_valueARR[0, 0] = 3;
            _valueARR[0, 1] = 3;
            _valueARR[1, 2] = 3;
            _valueARR[0, 3] = 3;
            //_valueARR[0, 4] = 3;

            //_valueARR[3, 0] = 5;
            //_valueARR[4, 1] = 5;
            //_valueARR[3, 2] = 5;
        }
        else if (LevelsManager.Instance.selectedLevel == 6)
        {
            _valueARR[3, 0] = 5;
            _valueARR[4, 1] = 5;
            _valueARR[3, 2] = 5;
        }
        else if (LevelsManager.Instance.selectedLevel == 8)
        {
            //_valueARR[0, 7] = 5;
            //_valueARR[1, 6] = 5;
            //_valueARR[1, 8] = 5;

            _valueARR[0, 0] = 5;
            _valueARR[1, 0] = 5;
            _valueARR[2, 1] = 5;
            _valueARR[3, 0] = 5;
            //_valueARR[4, 0] = 5;
        }
        else if (LevelsManager.Instance.selectedLevel == 2)
        {
            _valueARR[1, 1] = 7;
            _valueARR[6, 7] = 7;

            //_valueARR[3, 3] = 5;
            //_valueARR[2, 6] = 5;
        }

        // --------------------------------------------

        // Init Jewel Puzzle and BG
        string levelsInfoString = levelsInfo.text;
        levelsInfoJSON = new JSONObject(levelsInfoString);
        yield return StartCoroutine(initPuzzle());
        //StartCoroutine(initPuzzle());

        GameController.Instance.currentState = GameController.GameState.scanningUnit;

        //---------------------------------------------------------------------
        // ---------------- Fake value for testing ----------------------------
        //---------------------------------------------------------------------

        //for (int i = 0; i < 8; i++)
        //{
        //    _unitARR[i, 5].GetComponent<SpriteRenderer>().enabled = false;
        //    _unitARR[i, 5].GetComponent<BoxCollider2D>().enabled = false;
        //}
        //yield return null;
        //upgradeUnit(0, 5, UnitInfo.SpecialEff.noEff, UnitInfo.NegativeEff.hollow);

        //---------------------------------------------------------------------
        //---------------------------------------------------------------------
        yield return StartCoroutine(GameController.Instance.showLevelText());
        //yield return new WaitForSeconds(1f);

        // ----------Fake unit value for testing ------------------      

        //if (LevelsManager.Instance.selectedLevel == 7)
        //{
        //    upgradeUnit(0, 1, UnitInfo.SpecialEff.explode, UnitInfo.NegativeEff.noEff);
        //    upgradeUnit(0, 2, UnitInfo.SpecialEff.explode, UnitInfo.NegativeEff.noEff);

        //    //upgradeUnit(2, 5, UnitInfo.SpecialEff.noEff, UnitInfo.NegativeEff.empty);

        //    //upgradeUnit(7, 1, UnitInfo.SpecialEff.explode, UnitInfo.NegativeEff.noEff);
        //    //upgradeUnit(7, 2, UnitInfo.SpecialEff.explode, UnitInfo.NegativeEff.noEff);

        //    //upgradeUnit(4, 1, UnitInfo.SpecialEff.explode, UnitInfo.NegativeEff.noEff);
        //}
        //else if (LevelsManager.Instance.selectedLevel == 6)
        //{
        //    upgradeUnit(4, 1, UnitInfo.SpecialEff.explode, UnitInfo.NegativeEff.noEff);
        //}
        //else if (LevelsManager.Instance.selectedLevel == 1)
        //{
        //    //upgradeUnit(4, 1, UnitInfo.SpecialEff.explode, UnitInfo.NegativeEff.frozen);

        //    //upgradeUnit(2, 6, UnitInfo.SpecialEff.explode, UnitInfo.NegativeEff.frozen);
        //}
        //else if (LevelsManager.Instance.selectedLevel == 10)
        //{
        //    upgradeUnit(3, 8, UnitInfo.SpecialEff.vLightning, UnitInfo.NegativeEff.noEff);
        //    upgradeUnit(3, 9, UnitInfo.SpecialEff.explode, UnitInfo.NegativeEff.noEff);
        //}

        // --------------------------------------------

        unitsList = new List<GameObject>();
        for (int XIndex = 0; XIndex < _columns; XIndex++)
        {
            for (int YIndex = 0; YIndex < _rows; YIndex++)
            {
                // If this Unit is hollowed,mark it as chained to prevent future restoration
                if (_unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                {
                    ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained = true;
                }
                else if (_unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.empty)
                {
                    ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained = true;
                }
                else
                {
                    //unitsList.Add(_unitARR[XIndex, YIndex]);
                }
            }
        }
        //StartCoroutine(ChainedUnitsScanner.Instance.scanRegenUnits(unitsList));

        List<GameObject> possibleList = PossibleChainedDetector.Instance.scanPossibleChained();
        if (possibleList.Count == 0)
        {
            PuzzleShuffle.Instance.shufflePuzzle();
        }

        GameController.Instance.currentState = GameController.GameState.idle;
    }


    //==============================================
    // Methods
    //==============================================

    #region Init Unit methods

    public void initUnit(Vector2 spawnPos, int XIndex, int YIndex, int value, UnitInfo.SpecialEff specialEff)
    {
        // Get unit from pool
        //_unitARR[XIndex, YIndex] = Instantiate(Unit[value], spawnPos, Quaternion.identity) as GameObject;
        _unitARR[XIndex, YIndex] = getJewel(value, spawnPos);
        _unitARR[XIndex, YIndex].transform.SetParent(unitHolder);

        // Set infomation for this unit
        UnitInfo unitInfo = _unitARR[XIndex, YIndex].GetComponent<UnitInfo>();
        unitInfo._XIndex = XIndex;
        unitInfo._YIndex = YIndex;
        unitInfo._value = value;
        unitInfo._unitEff = specialEff;

        // Enable specialEff image if this is SpecialEff unit
        switch (specialEff)
        {
            case UnitInfo.SpecialEff.vLightning:
                {
                    unitInfo.VerticalLightningEff.SetActive(true);
                    break;
                }
            case UnitInfo.SpecialEff.hLightning:
                {
                    unitInfo.HorizontalLightningEff.SetActive(true);
                    break;
                }
            case UnitInfo.SpecialEff.explode:
                {
                    unitInfo.ExplosiveSparkEff.SetActive(true);
                    break;
                }
            default: break;
        }

        ////If this is regen Unit,move to it's original pos
        //if (spawnPos.y != _unitPosARR[XIndex, YIndex].y)
        //{
        //    GameObject target = _unitARR[XIndex, YIndex];

        //    //_unitARR[XIndex, YIndex].transform.DOMove(_unitPosARR[XIndex, YIndex], regenUnitDropTime).SetEase(Ease.InSine);

        //    int distance = _rows - YIndex;
        //    _unitARR[XIndex, YIndex].transform.DOMove(_unitPosARR[XIndex, YIndex], distance * baseUnitDropTime).SetEase(Ease.InSine);

        //    //print(XIndex + ";;;;;;;;;" + YIndex + "'''''''''" + distance);

        //    //_unitARR[XIndex, YIndex].transform.DOMove(_unitPosARR[XIndex, YIndex], regenUnitDropTime).SetEase(curveTween);

        //    //target.transform.DOMove(_unitPosARR[XIndex, YIndex], regenUnitDropTime - 0.1f).
        //    //    OnComplete(() => target.transform.DOMoveY(target.transform.position.y + 0.1f, 0.1f).SetEase(Ease.OutSine).SetLoops(2, LoopType.Yoyo)).
        //    //    SetEase(Ease.InSine);
        //}
    }

    public void initUnit(Vector2 spawnPos, int XIndex, int YIndex, int value,
        UnitInfo.SpecialEff specialEff, UnitInfo.NegativeEff negativeEff)
    {
        // Get unit from pool
        //_unitARR[XIndex, YIndex] = Instantiate(Unit[value], spawnPos, Quaternion.identity) as GameObject;
        _unitARR[XIndex, YIndex] = getJewel(value, spawnPos);
        _unitARR[XIndex, YIndex].transform.SetParent(unitHolder);

        // Set infomation for this unit
        UnitInfo unitInfo = _unitARR[XIndex, YIndex].GetComponent<UnitInfo>();
        unitInfo._XIndex = XIndex;
        unitInfo._YIndex = YIndex;
        unitInfo._value = value;
        unitInfo._unitEff = specialEff;
        unitInfo._negativeEff = negativeEff;

        // Enable specialEff if this is SpecialEff unit
        switch (specialEff)
        {
            case UnitInfo.SpecialEff.vLightning:
                {
                    unitInfo.VerticalLightningEff.SetActive(true);
                    break;
                }
            case UnitInfo.SpecialEff.hLightning:
                {
                    unitInfo.HorizontalLightningEff.SetActive(true);
                    break;
                }
            case UnitInfo.SpecialEff.explode:
                {
                    unitInfo.ExplosiveSparkEff.SetActive(true);
                    break;
                }
            default: break;
        }

        // Enable negativeEff if this is negativeEff unit
        switch (negativeEff)
        {
            case UnitInfo.NegativeEff.frozen:
                {
                    unitInfo.FrozenEff.SetActive(true);
                    break;
                }
            case UnitInfo.NegativeEff.locked:
                {
                    unitInfo.LockEff.SetActive(true);
                    break;
                }
            case UnitInfo.NegativeEff.hollow:
                {
                    break;
                }
            default: break;
        }

        ////If this is regen Unit,move to it's original pos
        //if (spawnPos.y != _unitPosARR[XIndex, YIndex].y)
        //{
        //    GameObject target = _unitARR[XIndex, YIndex];

        //    //_unitARR[XIndex, YIndex].transform.DOMove(_unitPosARR[XIndex, YIndex], regenUnitDropTime).SetEase(Ease.InSine);

        //    int distance = _rows - YIndex;
        //    _unitARR[XIndex, YIndex].transform.DOMove(_unitPosARR[XIndex, YIndex], distance * baseUnitDropTime).SetEase(Ease.InSine);

        //    //_unitARR[XIndex, YIndex].transform.DOMove(_unitPosARR[XIndex, YIndex], regenUnitDropTime).SetEase(curveTween);

        //    //target.transform.DOMove(_unitPosARR[XIndex, YIndex], unitDropTime - 0.1f).
        //    //    OnComplete(() => target.transform.DOMoveY(target.transform.position.y + 0.1f, 0.1f).SetEase(Ease.OutSine).SetLoops(2, LoopType.Yoyo)).
        //    //    SetEase(Ease.InSine);
        //}
    }

    public void initRegenUnit(Vector2 spawnPos, int XIndex, int YIndex, int value, UnitInfo.SpecialEff specialEff, int dropDistance)
    {
        // Get unit from pool
        //_unitARR[XIndex, YIndex] = Instantiate(Unit[value], spawnPos, Quaternion.identity) as GameObject;
        _unitARR[XIndex, YIndex] = getJewel(value, spawnPos);
        _unitARR[XIndex, YIndex].transform.SetParent(unitHolder);

        // Set infomation for this unit
        UnitInfo unitInfo = _unitARR[XIndex, YIndex].GetComponent<UnitInfo>();
        unitInfo._XIndex = XIndex;
        unitInfo._YIndex = YIndex;
        unitInfo._value = value;
        unitInfo._unitEff = specialEff;

        // Enable specialEff image if this is SpecialEff unit
        switch (specialEff)
        {
            case UnitInfo.SpecialEff.vLightning:
                {
                    unitInfo.VerticalLightningEff.SetActive(true);
                    break;
                }
            case UnitInfo.SpecialEff.hLightning:
                {
                    unitInfo.HorizontalLightningEff.SetActive(true);
                    break;
                }
            case UnitInfo.SpecialEff.explode:
                {
                    unitInfo.ExplosiveSparkEff.SetActive(true);
                    break;
                }
            default: break;
        }

        //Move this unit to it's original pos
        if (spawnPos.y != _unitPosARR[XIndex, YIndex].y)
        {
            GameObject target = _unitARR[XIndex, YIndex];

            //_unitARR[XIndex, YIndex].transform.DOMove(_unitPosARR[XIndex, YIndex], regenUnitDropTime).SetEase(Ease.InSine);

            //_unitARR[XIndex, YIndex].transform.DOMove(_unitPosARR[XIndex, YIndex], dropDistance * baseUnitDropTime).SetEase(Ease.Linear);

            //print(XIndex + ";;;;;;;;;" + YIndex + "'''''''''" + distance);

            //_unitARR[XIndex, YIndex].transform.DOMove(_unitPosARR[XIndex, YIndex], regenUnitDropTime).SetEase(curveTween);

            //target.transform.DOMove(_unitPosARR[XIndex, YIndex], regenUnitDropTime - 0.1f).
            //    OnComplete(() => target.transform.DOMoveY(target.transform.position.y + 0.1f, 0.1f).SetEase(Ease.OutSine).SetLoops(2, LoopType.Yoyo)).
            //    SetEase(Ease.InSine);

            if(getItemIndexFromUnbounceList(XIndex, YIndex) != -1)
            {
                _unitARR[XIndex, YIndex].transform.DOMove(_unitPosARR[XIndex, YIndex], dropDistance * baseUnitDropTime).SetEase(Ease.Linear);
            }
            else
            {
                Tween t = _unitARR[XIndex, YIndex].transform.DOMove(_unitPosARR[XIndex, YIndex], dropDistance * baseUnitDropTime).SetEase(Ease.Linear);
                Tweener k = _unitARR[XIndex, YIndex].transform.DOLocalMoveY(0.1f, unitBounceTime/2).
                    SetLoops(2, LoopType.Yoyo).SetEase(Ease.Linear).SetRelative(true);
                k.Pause();
                t.OnComplete(() => k.Play());
            }
        }
    }


    /// <summary>
    /// Upgrade this Unit into Special destroy all type
    /// </summary>
    /// <param name="XIndex"></param>
    /// <param name="YIndex"></param>
    /// <param name="value"></param>
    /// <param name="specialEff"></param>
    public void upgradeToDestroyAllUnit(int XIndex, int YIndex)
    {
        ChainedUnitsScanner.Instance.disableUnit(XIndex, YIndex);
        initUnit(_unitPosARR[XIndex, YIndex], XIndex, YIndex, Unit.Length - 1, UnitInfo.SpecialEff.noEff);
        ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._value = Unit.Length - 1;
        ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained = false;
    }

    /// <summary>
    /// Add Special eff into this Unit
    /// </summary>
    /// <param name="XIndex"></param>
    /// <param name="YIndex"></param>
    /// <param name="value"></param>
    /// <param name="specialEff"></param>
    public void upgradeUnit(int XIndex, int YIndex, UnitInfo.SpecialEff specialEff)
    {
        int value = _unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._value;

        ChainedUnitsScanner.Instance.disableUnit(XIndex, YIndex);
        initUnit(_unitPosARR[XIndex, YIndex], XIndex, YIndex, value, specialEff);
        ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._value = value;
        ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained = false;
    }

    public void upgradeUnit(int XIndex, int YIndex, UnitInfo.SpecialEff specialEff, UnitInfo.NegativeEff negativeEff)
    {
        int value = _unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._value;

        ChainedUnitsScanner.Instance.disableUnit(XIndex, YIndex);
        initUnit(_unitPosARR[XIndex, YIndex], XIndex, YIndex, value, specialEff, negativeEff);
        ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._value = value;
        ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained = false;
    }

    #endregion

    private IEnumerator initPuzzle()
    {
        //JSONObject puzzleInfoJSon = levelsInfoJSON.GetField("Level " + LevelsManager.Instance.selectedLevel);
        JSONObject puzzleInfoJSon = LevelsManager.Instance.selectedLevelInfoJSON;

        for (int YIndex = 0; YIndex < _rows; YIndex++)
        {            
            for (int XIndex = 0; XIndex < _columns; XIndex++)
            {               
                Vector2 spawnPos = new Vector2(startingSpawnPos.position.x + XIndex * XPadding,
                                                startingSpawnPos.position.y + YIndex * YPadding);

                // Store position of each unit in the puzzle
                _unitPosARR[XIndex, YIndex] = spawnPos;

                // Init Jewel BG
                _unitBGARR[XIndex, YIndex] = Instantiate(UnitBGPreb, spawnPos, Quaternion.identity) as GameObject;
                _unitBGARR[XIndex, YIndex].transform.SetParent(unitBGHolder);

                initUnit(spawnPos, XIndex, YIndex, _valueARR[XIndex, YIndex], 0);
            }
        }

        //yield return null;
        ChainedUnitsScanner.Instance.initScanUnitArr();

        for (int YIndex = 0; YIndex < _rows; YIndex++)
        {
            JSONObject YIndexJSON = puzzleInfoJSon.GetField("Row " + YIndex);

            for (int XIndex = 0; XIndex < _columns; XIndex++)
            {                
                JSONObject XIndexJSON = YIndexJSON.GetField("Col " + XIndex);
                string specialEff = XIndexJSON.GetField("Special Eff").str;
                string negativeEff = XIndexJSON.GetField("Negative Eff").str;

                if (negativeEff == "hollow")
                {
                    //ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained = true;

                    //_unitARR[XIndex, YIndex].GetComponent<SpriteRenderer>().enabled = false;
                    //_unitARR[XIndex, YIndex].GetComponent<BoxCollider2D>().enabled = false;
                    _unitBGARR[XIndex, YIndex].SetActive(false);
                    upgradeUnit(XIndex, YIndex, UnitInfo.SpecialEff.noEff, UnitInfo.NegativeEff.hollow);
                    _unitARR[XIndex, YIndex].GetComponent<SpriteRenderer>().enabled = false;
                    _unitARR[XIndex, YIndex].GetComponent<BoxCollider2D>().enabled = false;
                }
                else if (negativeEff == "empty")
                {
                    upgradeUnit(XIndex, YIndex, UnitInfo.SpecialEff.noEff, UnitInfo.NegativeEff.empty);
                    //_unitARR[XIndex, YIndex].SetActive(false);
                    _unitARR[XIndex, YIndex].GetComponent<SpriteRenderer>().enabled = false;
                    _unitARR[XIndex, YIndex].GetComponent<BoxCollider2D>().enabled = false;
                    //print(_unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._value);

                }
                else if (negativeEff == "frozen")
                {
                    upgradeUnit(XIndex, YIndex, UnitInfo.SpecialEff.noEff, UnitInfo.NegativeEff.frozen);
                }
                else if (negativeEff == "locked")
                {
                    upgradeUnit(XIndex, YIndex, UnitInfo.SpecialEff.noEff, UnitInfo.NegativeEff.locked);
                }


                if (specialEff == "vLightning")
                {
                    upgradeUnit(XIndex, YIndex, UnitInfo.SpecialEff.vLightning, UnitInfo.NegativeEff.noEff);
                }
                else if (specialEff == "hLightning")
                {
                    upgradeUnit(XIndex, YIndex, UnitInfo.SpecialEff.hLightning, UnitInfo.NegativeEff.noEff);
                }
                else if (specialEff == "explode")
                {
                    upgradeUnit(XIndex, YIndex, UnitInfo.SpecialEff.explode, UnitInfo.NegativeEff.noEff);
                }

            }
        }

        //BorderGenerator.Instance.initBorder();

        yield return null;
    }

    #region Reorganize Puzzle methods

    public IEnumerator reOrganizePuzzle(bool hasChained)
    {
        unitsList.Clear();
        //unitsList = new List<GameObject>();

        if (hasChained)
        {
            // Make Units fall down after destroy state
            for (int XIndex = 0; XIndex < _columns; XIndex++)
            {
                reOrganizePuzzleCol(XIndex);
            }
            ChainedUnitsScanner.Instance.updateScanUnits(unitsList);

            //----------- Temporary disabled --------------
            //yield return new WaitForSeconds(0.05f);
            //---------------------------------------------
            //----------- Temporary changed ---------------
            //yield return new WaitForSeconds(0.15f);
            //---------------------------------------------

            StartCoroutine(scanEmptyUnits(hasChained));
            //StartCoroutine(ChainedUnitsScanner.Instance.scanRegenUnits(unitsList));
        }
        else
        {
            yield return null;
            StartCoroutine(scanEmptyUnits(hasChained));
        }
        
        //StartCoroutine(scanHollowUnits());
        //GameController.Instance.currentState = GameController.GameState.idle;
    }

    private void reOrganizePuzzleCol(int XIndex)
    {
        int nullObjectCount = 0;

        List<int> hollowUnitsYIndex = new List<int>();
        List<int> nullUnitsYIndex = new List<int>();
        List<GameObject> refreshUnits = new List<GameObject>();

        for (int YIndex = 0; YIndex < _rows; YIndex++)
        {
            // If current Index is hollowed,remember it
            if (_unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
            {
                if (YIndex == 0)
                {
                    continue;
                }
                else
                {
                    if (!hollowUnitsYIndex.Contains(YIndex))
                    {
                        hollowUnitsYIndex.Add(YIndex);
                    }
                }
            }
            // If current Index is empty space,remember it
            else if (ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained)
            {
                nullObjectCount++;
                nullUnitsYIndex.Add(YIndex);
            }
            // If object is frozen,reset hollowUnitcount and nullObjectCount to prevent wrong falling down behaviour
            else if (_unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen)
            {
                nullObjectCount = 0;
                hollowUnitsYIndex.Clear();
                nullUnitsYIndex.Clear();
            }
            else
            {
                // Make Unit fall down if there are empty space below
                if (nullObjectCount > 0)
                {
                    //print(XIndex + ":::" + YIndex + ":::" + nullObjectCount);
                    //Vector3 targetPos = _unitPosARR[XIndex, YIndex - nullObjectCount];
                    //StartCoroutine(dropUnit(_unitARR[XIndex, YIndex], targetPos, nullObjectCount));

                    if (!unitsList.Contains(_unitARR[XIndex, YIndex]))
                    {
                        unitsList.Add(_unitARR[XIndex, YIndex]);
                    }
                    if (!refreshUnits.Contains(_unitARR[XIndex, YIndex]))
                    {
                        refreshUnits.Add(_unitARR[XIndex, YIndex]);
                    }

                    // Calculate where to drop unit
                    int dropDistance = nullObjectCount;
                    for (int i = 1; i <= dropDistance; i++)
                    {
                        //print(XIndex + "aaasd" + YIndex + "dfsfd" + i);
                        if (_unitARR[XIndex, YIndex - i].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                        {
                            dropDistance++;
                        }
                    }

                    //StartCoroutine(dropUnit(XIndex, YIndex, dropDistance));
                    dropUnit(XIndex, YIndex, dropDistance);

                    //for (int i = 0; i < hollowUnitsYIndex.Count; i++)
                    //{
                    //    if ((YIndex - dropDistance + 1) == hollowUnitsYIndex[i])
                    //    {
                    //        hollowUnitcount--;
                    //        hollowUnitsYIndex.Remove(i);
                    //        break;
                    //    }
                    //}

                    //// Make scanUnit that has fallen down unit become chained
                    //ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained = true;

                    //// Update Unit info
                    //_unitARR[XIndex, YIndex - nullObjectCount] = _unitARR[XIndex, YIndex];
                    //_unitARR[XIndex, YIndex - nullObjectCount].GetComponent<UnitInfo>()._YIndex -= nullObjectCount;

                    // Add this unit to list to scan again

                }
            }
        }

        //Regen units
        if (nullObjectCount > 0)
        {
            int count = nullObjectCount;
            //int hollowCount = 0;

            for (int i = 0; i < nullObjectCount; i++)
            {
                if (_unitARR[XIndex, _rows - i - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                {
                    nullObjectCount++;
                    continue;
                }

                Vector2 regenUnitSpawnPos = new Vector2(_unitPosARR[XIndex, _rows - i - 1].x, regenYpos + (nullObjectCount - i) * YPadding);

                int dropDistance = nullObjectCount;
                initRegenUnit(regenUnitSpawnPos, XIndex, _rows - i - 1, Random.Range(0, Unit.Length - 1), 0, dropDistance);

                // Add this unit to list to scan again
                unitsList.Add(_unitARR[XIndex, _rows - i - 1]);
                refreshUnits.Add(_unitARR[XIndex, _rows - i - 1]);
                //initUnit(_unitPosARR[XIndex, _rows - i - 1], XIndex, _rows - i - 1, Random.Range(0, Unit.Length - 1), 0);
            }
        }
        ChainedUnitsScanner.Instance.updateScanUnits(refreshUnits);
    }

    #endregion

    #region Scan for pushable Units methods

    public IEnumerator scanEmptyUnits(bool hasChained)
    {
        //unitsList = new List<GameObject>();

        canPushUnit = false;
        if (hasChained)
        {
            //yield return new WaitForSeconds(unitDropTime);
            yield return StartCoroutine(waitForTweenComplete());
            //print("complete");
        }

        yield return StartCoroutine(findPushUnitsInPuzzle());
        //findPushUnitsInPuzzle();

        if (!canPushUnit)
        {
            // If there are regenUnits,scan them
            if (hasChained)
            {
                yield return StartCoroutine(waitForTweenComplete());
                yield return new WaitForSeconds(0.15f);
                StartCoroutine(ChainedUnitsScanner.Instance.scanRegenUnits(unitsList));
            }
            else
            {
                GameController.Instance.currentState = GameController.GameState.idle;

                if (!GameController.Instance.isGameCompleted)
                {
                    GameController.Instance.checkIfGameOver();
                }
                else
                {
                    GameController.Instance.gameCompleted();
                }
                
                if (turnCountToUpgrade >= turnsToUpgradeRandomLightningUnit)
                {
                    upgradeRandomUnitIntoLightning();
                    turnCountToUpgrade = 0;
                }
            }           
        }
        else
        {
            yield return StartCoroutine(waitForTweenComplete());
            yield return new WaitForSeconds(0.15f);
            StartCoroutine(ChainedUnitsScanner.Instance.scanRegenUnits(unitsList));
        }
                
        //-------------------------------------------------
        //--------------- Temporary Added -----------------
        //-------------------------------------------------

        //GameController.Instance.currentState = GameController.GameState.idle;

        //-------------------------------------------------
        //-------------------------------------------------

    }

    /* Old findPushUnitsInPuzzle() method
    private IEnumerator findPushUnitsInPuzzle()
    {
        //waitTweenList.Clear();
        unbounceList.Clear();

        bool hasUnitToPush = false;
        List<int> pushColList = new List<int>();
        temporaryPushRightUnitList = new List<GameObject>();

        #region Find push right column
        for (int XIndex = _columns - 2; XIndex >= 0; XIndex--)
        {
            bool canPushInCol = findPushRightUnitInCol(XIndex);
            if (canPushInCol)
            {
                hasUnitToPush = true;
                if (!pushColList.Contains(XIndex))
                {
                    pushColList.Add(XIndex);
                }
                if (!pushColList.Contains(XIndex + 1))
                {
                    pushColList.Add(XIndex + 1);
                }
            }
        }

        ////yield return new WaitForSeconds(unitPushTime + 0.05f);
        //yield return new WaitForSeconds(0.1f);

        //if (hasUnitToPush)
        //{
        //    for (int i = 0; i < pushColList.Count; i++)
        //    {
        //        reOrganizePuzzleCol(pushColList[i]);
        //    }
        //}
        //yield return new WaitForSeconds(unitDropTime + 0.05f);

        //if (hasUnitToPush)
        //{
        //    yield return StartCoroutine(findPushUnitsInPuzzle());
        //}
        #endregion

        //if (hasUnitToPush)
        //{
        //    yield return new WaitForSeconds(0.3f);
        //}

        //yield return new WaitForSeconds(0.3f);

        #region Find push left column
        //hasUnitToPush = false;

        if (!hasUnitToPush)
        {
            for (int XIndex = 1; XIndex < _columns; XIndex++)
            {
                bool canPushInCol = findPushLeftUnitInCol(XIndex);
                if (canPushInCol)
                {
                    hasUnitToPush = true;
                    if (!pushColList.Contains(XIndex))
                    {
                        pushColList.Add(XIndex);
                    }
                    if (!pushColList.Contains(XIndex - 1))
                    {
                        pushColList.Add(XIndex - 1);
                    }
                }
            }
        }       

        //yield return new WaitForSeconds(unitPushTime + 0.05f);
        //yield return new WaitForSeconds(0.1f);
        //if (hasUnitToPush)
        //{
        //    yield return new WaitForSeconds(0.02f);
        //}

        if (hasUnitToPush)
        {
            for (int i = 0; i < pushColList.Count; i++)
            {
                reOrganizePuzzleCol(pushColList[i]);
            }
        }
        //yield return new WaitForSeconds(unitDropTime + 0.05f);
        //yield return new WaitForSeconds(0.5f);
        if (hasUnitToPush)
        {
            //yield return new WaitForSeconds(regenUnitDropTime);
            yield return new WaitForSeconds(unitPushTime);
            //yield return new WaitForSeconds(2f);
        }

        if (hasUnitToPush)
        {
            yield return StartCoroutine(findPushUnitsInPuzzle());
        }
        #endregion
    }
    */

    private IEnumerator findPushUnitsInPuzzle()
    {
        //unbounceList.Clear();

        bool hasUnitToPush = false;
        hasUnitToRegen = false;
        hasBouncingUnit = false;
        pushingDelay = false;

        //List<int> pushColList = new List<int
        int pushCol = -1;
        temporaryPushRightUnitList = new List<GameObject>();

        for (int YIndex = _rows - 1; YIndex >= 0; YIndex--)
        {
            for (int XIndex = 0; XIndex < _columns; XIndex++)
            {
                hasUnitToPush = findUnitPushTypeAndPush(XIndex, YIndex);

                if (hasUnitToPush)
                {
                    pushCol = XIndex;
                    break;
                }
            }
            if (hasUnitToPush)
            {
                break;
            }
        }

        if (hasUnitToPush && pushCol >= 0)
        {
            reOrganizePuzzleCol(pushCol);
        }

        if (hasUnitToRegen)
        {
            //print("regen");
            yield return new WaitForSeconds(baseUnitDropTime);
        }
        if (hasBouncingUnit)
        {
            //print("bounce");
            yield return new WaitForSeconds(unitBounceTime);
        }
        if (pushingDelay)
        {
            yield return new WaitForSeconds(unitPushTime);
        }

        if (hasUnitToPush)
        {
            yield return StartCoroutine(findPushUnitsInPuzzle());
        }

        //yield return null;
    }

    private bool findUnitPushTypeAndPush(int XIndex, int YIndex)
    {
        if (!ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained
                && _unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen)
        {
            unitPushType pushType = checkIfCanPush(XIndex, YIndex);
            switch (pushType)
            {
                case unitPushType.Right:
                    {
                        // The push Unit must not be floating,the position this Unit is pushed into must
                        // below the highest frozen unit in the same column
                        if (hasFrozenUnitInCol(XIndex + 1) && !noFrozenUnitAbove(XIndex + 1, YIndex - 1)
                            //&& !ChainedUnitsScanner.Instance._scanUnitARR[col, YIndex - 1]._isChained
                            )
                        {
                            if (YIndex < _rows - 1)
                            {
                                //// Ignore if the push unit is not the highest among all units that below the highest frozen unit in the same col
                                //if (!isAboveHighestFrozenUnitInCol(XIndex, YIndex)
                                //&& (!ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex + 1]._isChained)
                                //&& _unitARR[XIndex, YIndex + 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen
                                //|| _unitARR[XIndex + 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                                //{
                                //    //print(col + ",,," + YIndex + ",,,cant push");
                                //    //return false;
                                //    break;
                                //}

                                if (checkIfCanPush(XIndex, YIndex + 1) == unitPushType.Right)
                                {
                                    break;
                                }

                                if (_unitARR[XIndex + 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                                {
                                    //print(col + ",,," + YIndex + ",,,cant push");
                                    //return false;
                                    break;
                                }
                            }

                            //if (!temporaryPushRightUnitList.Contains(_unitARR[XIndex, YIndex]))
                            //{
                            //    temporaryPushRightUnitList.Add(_unitARR[XIndex, YIndex]);
                            //}

                            canPushUnit = true;
                            pushUnit(XIndex, YIndex, pushType);

                            if(getItemIndexFromUnbounceList(XIndex + 1, YIndex - 1) != -1)
                            {
                                getDropableUnitIndexList(XIndex, YIndex);
                            }
                            else if (checkIfCanPush(XIndex + 1, YIndex - 1) != unitPushType.None ||
                                (YIndex - 2 > 0 && ChainedUnitsScanner.Instance._scanUnitARR[XIndex + 1, YIndex - 2]._isChained &&
                                _unitARR[XIndex + 1, YIndex - 2].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow))
                            {
                                //print("unbounce");
                                getDropableUnitIndexList(XIndex, YIndex);
                                //print(XIndex + "~~~~~~~" + YIndex + "~~~~~~~~~~~" + unbounceList.Count);
                            }
                            else if (XIndex > 0 && ChainedUnitsScanner.Instance._scanUnitARR[XIndex - 1, YIndex - 1]._isChained
                                && _unitARR[XIndex - 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow
                                && _unitARR[XIndex - 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen)
                            {
                                print("still can push");
                                getDropableUnitIndexList(XIndex, YIndex);
                            }
                            else if (YIndex == _rows - 1)
                            {
                                hasBouncingUnit = true;
                            }
                            else if (noFrozenUnitAbove(XIndex, YIndex))
                            {
                                //print(hasBouncingUnit);
                                hasBouncingUnit = true;
                                pushingDelay = true;
                                //print("has bounce");
                            }

                            if(YIndex == _rows - 1)
                            {
                                hasUnitToRegen = true;                                
                            }

                            moveUnitToTheEnd(XIndex + 1, YIndex - 1);

                            return true;
                        }
                        break;
                    }

                case unitPushType.Left:
                    {
                        // The push Unit must not be floating,the position this Unit is pushed into must
                        // below the highest frozen unit in the same column
                        if (hasFrozenUnitInCol(XIndex - 1) && !noFrozenUnitAbove(XIndex - 1, YIndex - 1)
                            //&& !ChainedUnitsScanner.Instance._scanUnitARR[col, YIndex - 1]._isChained
                            )
                        {
                            // Ignore if the push unit is not the highest among units that below frozen unit in the col
                            if (YIndex < _rows - 1)
                            {
                                //if (!isAboveHighestFrozenUnitInCol(XIndex, YIndex)
                                //&& (!ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex + 1]._isChained)
                                //&& _unitARR[XIndex, YIndex + 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen
                                //|| _unitARR[XIndex - 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow
                                //)
                                //{
                                //    //print(col + ",,," + YIndex + ",,,cant push");
                                //    //return false;
                                //    break;
                                //}

                                if (checkIfCanPush(XIndex, YIndex + 1) == unitPushType.Left)
                                {
                                    break;
                                }

                                if (_unitARR[XIndex - 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                                {
                                    //print(col + ",,," + YIndex + ",,,cant push");
                                    //return false;
                                    break;
                                }
                            }

                           
                            //if (!temporaryPushRightUnitList.Contains(_unitARR[XIndex, YIndex]))
                            //{
                            //    temporaryPushRightUnitList.Add(_unitARR[XIndex, YIndex]);
                            //}

                            canPushUnit = true;
                            //StartCoroutine(pushUnit(col, YIndex, pushType));
                            pushUnit(XIndex, YIndex, pushType);

                            if (getItemIndexFromUnbounceList(XIndex - 1, YIndex - 1) != -1)
                            {
                                //print("has item");
                                getDropableUnitIndexList(XIndex, YIndex);
                            }
                            else if (checkIfCanPush(XIndex - 1, YIndex - 1) != unitPushType.None ||
                                (ChainedUnitsScanner.Instance._scanUnitARR[XIndex - 1, YIndex - 2]._isChained &&
                                _unitARR[XIndex - 1, YIndex - 2].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow))
                            {
                                //print("unbounce");
                                getDropableUnitIndexList(XIndex, YIndex);
                                //print(XIndex + "~~~~~~~" + YIndex + "~~~~~~~~~~~" + unbounceList.Count);
                            }
                            else if (YIndex < _rows - 1 && ChainedUnitsScanner.Instance._scanUnitARR[XIndex + 1, YIndex - 1]._isChained
                                && _unitARR[XIndex + 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow
                                && _unitARR[XIndex + 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen)
                            {
                                print("still can push");
                                getDropableUnitIndexList(XIndex, YIndex);
                            }
                            else if (YIndex == _rows - 1)
                            {
                                hasBouncingUnit = true;
                            }
                            else if (noFrozenUnitAbove(XIndex, YIndex))
                            {
                                print(hasBouncingUnit);
                                hasBouncingUnit = true;
                                pushingDelay = true;
                                print("has bounce");
                            }

                            if (YIndex == _rows - 1)
                            {
                                hasUnitToRegen = true;
                            }

                            moveUnitToTheEnd(XIndex - 1, YIndex - 1);

                            return true;
                        }
                        break;
                    }
            }
        }
        return false;
    }

    // Find the highest unit in a column that can be push right then push it to the right
    private bool findPushRightUnitInCol(int col)
    {
        for (int YIndex = _rows - 1; YIndex >= 0; YIndex--)
        {
            if (!ChainedUnitsScanner.Instance._scanUnitARR[col, YIndex]._isChained
                && _unitARR[col, YIndex].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen)
            {
                unitPushType pushType = checkIfCanPush(col, YIndex);

                // The push Unit must not be floating,the position this Unit is pushed into must
                // below the highest frozen unit in the same column
                if (pushType == unitPushType.Right && hasFrozenUnitInCol(col + 1)
                    && !noFrozenUnitAbove(col + 1, YIndex - 1)
                    //&& !ChainedUnitsScanner.Instance._scanUnitARR[col, YIndex - 1]._isChained
                    )
                {
                    if (YIndex < _rows - 1)
                    {
                        // Ignore if the push unit is not the highest among all units that below the highest frozen unit in the same col
                        if (!noFrozenUnitAbove(col, YIndex)
                        && (!ChainedUnitsScanner.Instance._scanUnitARR[col, YIndex + 1]._isChained)
                        && _unitARR[col, YIndex + 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen
                        || _unitARR[col + 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                        {
                            //print(col + ",,," + YIndex + ",,,cant push");
                            //return false;
                            continue;
                        }
                    }

                    if (!temporaryPushRightUnitList.Contains(_unitARR[col, YIndex]))
                    {
                        temporaryPushRightUnitList.Add(_unitARR[col, YIndex]);
                    }

                    canPushUnit = true;
                    //StartCoroutine(pushUnit(col, YIndex, pushType));
                    pushUnit(col, YIndex, pushType);

                    if (checkIfCanPush(col + 1, YIndex - 1) != unitPushType.None ||
                        (ChainedUnitsScanner.Instance._scanUnitARR[col + 1, YIndex - 2]._isChained &&
                        _unitARR[col + 1, YIndex - 2].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow))
                    {
                        //print("unbounce");
                        getDropableUnitIndexList(col, YIndex);
                        //print(col + "~~~~~~~" + YIndex + "~~~~~~~~~~~" + unbounceList.Count);
                    }

                    moveUnitToTheEnd(col + 1, YIndex - 1);

                    //// If only the unit on top right can repalce this unit's position,then push it to this unit positin on the same turn
                    //if (YIndex < _rows - 1 && col < _columns - 1)
                    //{
                    //    if (col == 0)
                    //    {
                    //        if (_unitARR[col, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen
                    //            && _unitARR[col + 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen)
                    //        {
                    //            StartCoroutine(pushUnit(col + 1, YIndex + 1, unitPushType.Left));
                    //        }
                    //    }
                    //    else if (col > 0)
                    //    {
                    //        if (_unitARR[col, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen
                    //            && _unitARR[col + 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen
                    //            && _unitARR[col - 1, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen)
                    //        {
                    //            StartCoroutine(pushUnit(col + 1, YIndex + 1, unitPushType.Left));
                    //        }
                    //    }
                    //}

                    return true;
                }
            }
        }
        return false;
    }

    // Find the highest unit in a column that can be push left then push it to the left
    private bool findPushLeftUnitInCol(int col)
    {
        for (int YIndex = _rows - 1; YIndex >= 0; YIndex--)
        {
            if (!ChainedUnitsScanner.Instance._scanUnitARR[col, YIndex]._isChained
                && _unitARR[col, YIndex].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen)
            {
                unitPushType pushType = checkIfCanPush(col, YIndex);

                // The push Unit must not be floating,the position this Unit is pushed into must
                // below the highest frozen unit in the same column
                if (pushType == unitPushType.Left && hasFrozenUnitInCol(col - 1)
                    && !noFrozenUnitAbove(col - 1, YIndex - 1)
                    //&& !ChainedUnitsScanner.Instance._scanUnitARR[col, YIndex - 1]._isChained
                    )
                {
                    // Ignore if the push unit is not the highest among units that below frozen unit in the col
                    if (YIndex < _rows - 1)
                    {
                        if (!noFrozenUnitAbove(col, YIndex)
                        && (!ChainedUnitsScanner.Instance._scanUnitARR[col, YIndex + 1]._isChained)
                        && _unitARR[col, YIndex + 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen
                        || _unitARR[col - 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow
                        || temporaryPushRightUnitList.Contains(_unitARR[col, YIndex]))
                        {
                            //print(col + ",,," + YIndex + ",,,cant push");
                            //return false;
                            continue;
                        }

                        //if (checkIfCanPush(col, YIndex + 1) == unitPushType.Left)
                        //{
                        //    continue;
                        //}

                        //if (_unitARR[col - 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow
                        //|| temporaryPushRightUnitList.Contains(_unitARR[col, YIndex]))
                        //{
                        //    //print(col + ",,," + YIndex + ",,,cant push");
                        //    //return false;
                        //    continue;
                        //}
                    }

                    if (!temporaryPushRightUnitList.Contains(_unitARR[col, YIndex]))
                    {
                        temporaryPushRightUnitList.Add(_unitARR[col, YIndex]);
                    }

                    canPushUnit = true;
                    //StartCoroutine(pushUnit(col, YIndex, pushType));
                    pushUnit(col, YIndex, pushType);

                    if (checkIfCanPush(col - 1, YIndex - 1) != unitPushType.None ||
                        (ChainedUnitsScanner.Instance._scanUnitARR[col - 1, YIndex - 2]._isChained &&
                        _unitARR[col - 1, YIndex - 2].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow))
                    {
                        //print("unbounce");
                        getDropableUnitIndexList(col, YIndex);
                        //print(col + "~~~~~~~" + YIndex + "~~~~~~~~~~~" + unbounceList.Count);
                    }

                    moveUnitToTheEnd(col - 1, YIndex - 1);

                    //------------------------------------
                    //--------Temporary disabled----------
                    //------------------------------------

                    //moveOtherUnitsAboveInColToTheEnd(col, YIndex);

                    //------------------------------------
                    //------------------------------------

                    // If only the unit on top left can repalce this unit's position,then push it to this unit positin on the same turn
                    if (YIndex < _rows - 1 && col > 0)
                    {
                        if (col == _columns - 1)
                        {
                            if (_unitARR[col, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen
                                && _unitARR[col - 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen)
                            {
                                //StartCoroutine(pushUnit(col - 1, YIndex + 1, unitPushType.Right));
                                pushUnit(col - 1, YIndex + 1, unitPushType.Right);
                            }
                        }
                        else if (col < _columns - 1)
                        {
                            if (_unitARR[col, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen
                                && _unitARR[col - 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen
                                && _unitARR[col + 1, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen)
                            {
                                //StartCoroutine(pushUnit(col - 1, YIndex + 1, unitPushType.Right));
                                pushUnit(col - 1, YIndex + 1, unitPushType.Right);
                            }
                        }
                    }

                    return true;
                }
            }
        }
        return false;
    }

    private unitPushType checkIfCanPush(int XIndex, int YIndex)
    {
        if (_unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen 
            || _unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow
            || ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained)
        {
            return unitPushType.None;
        }

        if (YIndex > 0)
        {
            if (XIndex == 0)
            {
                if (ChainedUnitsScanner.Instance._scanUnitARR[XIndex + 1, YIndex - 1]._isChained
                    //&& _unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen
                    && _unitARR[XIndex + 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow)
                {
                    return unitPushType.Right;
                }
            }
            else if (XIndex == _columns - 1)
            {
                if (ChainedUnitsScanner.Instance._scanUnitARR[XIndex - 1, YIndex - 1]._isChained
                    //&& _unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen
                    && _unitARR[XIndex - 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow)
                {
                    return unitPushType.Left;
                }
            }
            else
            {
                if (ChainedUnitsScanner.Instance._scanUnitARR[XIndex + 1, YIndex - 1]._isChained
                    //&& _unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen
                    && _unitARR[XIndex + 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow)
                {
                    return unitPushType.Right;
                }
                else if (ChainedUnitsScanner.Instance._scanUnitARR[XIndex - 1, YIndex - 1]._isChained
                    //&& _unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen
                    && _unitARR[XIndex - 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow)
                {
                    return unitPushType.Left;
                }
            }
        }
        return unitPushType.None;
    }

    #endregion

    #region Move Units methods
    private void moveUnitToTheEnd(int XIndex, int YIndex)
    {
        bool canDrop = false;
        unitPushType push = checkIfCanPush(XIndex, YIndex);

        if (YIndex > 0)
        {
            //print(XIndex + ".............." + YIndex + ".........." + "scan");
            for (int i = YIndex - 1; i >= 0; i--)
            {
                if (_unitARR[XIndex, i].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                {
                    //print("continue");
                    continue;
                }

                if (!ChainedUnitsScanner.Instance._scanUnitARR[XIndex, i]._isChained)
                {
                    //print("break");
                    break;
                }
                else
                {
                    //print("drop");
                    canDrop = true;
                    break;
                }
            }
        }

        if (canDrop)
        {
            int dropDistance = 0;
            for (int i = YIndex - 1; i >= 0; i--)
            {
                if (ChainedUnitsScanner.Instance._scanUnitARR[XIndex, i]._isChained
                    && _unitARR[XIndex, i].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow)
                {
                    dropDistance++;
                }
            }
            dropUnit(XIndex, YIndex, dropDistance);
            moveUnitToTheEnd(XIndex, YIndex - dropDistance);
            //print(dropDistance);
            //print("droooooopuuuuu");
        }
        else if (push != unitPushType.None)
        {
            //print(XIndex + "[[[[[[[[[[[[[" + YIndex + "]]]]]]]]]]]]]]]]" + push);
            pushUnit(XIndex, YIndex, push);
            switch (push)
            {
                case unitPushType.Left:
                    {
                        moveUnitToTheEnd(XIndex - 1, YIndex - 1);
                        break;
                    }
                case unitPushType.Right:
                    {
                        moveUnitToTheEnd(XIndex + 1, YIndex - 1);
                        break;
                    }
            }
        }
    }

    private void moveOtherUnitsAboveInColToTheEnd(int XIndex, int YIndex)
    {
        if (YIndex < _rows - 1)
        {
            for (int i = YIndex + 1; i <= _rows - 1; i++)
            {
                if (_unitARR[XIndex, i].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                {
                    continue;
                }
                else if (_unitARR[XIndex, i].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen)
                {
                    break;
                }
                else
                {
                    moveUnitToTheEnd(XIndex, i);
                }
            }
        }
    }

    private void pushUnit(int XIndex, int YIndex, unitPushType pushType)
    {
        GameObject target = _unitARR[XIndex, YIndex];
        switch (pushType)
        {
            case unitPushType.Left:
                {
                    //unitsList.Remove(_unitARR[XIndex, YIndex]);
                    //unitsList.Add(_unitARR[XIndex, YIndex]);
                    if(!unitsList.Contains(_unitARR[XIndex, YIndex]))
                    {
                        unitsList.Add(_unitARR[XIndex, YIndex]);
                    }

                    _unitARR[XIndex - 1, YIndex - 1] = _unitARR[XIndex, YIndex];
                    _unitARR[XIndex - 1, YIndex - 1].GetComponent<UnitInfo>()._XIndex--;
                    _unitARR[XIndex - 1, YIndex - 1].GetComponent<UnitInfo>()._YIndex--;

                    ChainedUnitsScanner.Instance._scanUnitARR[XIndex - 1, YIndex - 1]._isChained = false;
                    ChainedUnitsScanner.Instance._scanUnitARR[XIndex    , YIndex    ]._isChained = true;

                    //---------------------------------------------
                    //------------Temporary Disabled---------------
                    //---------------------------------------------

                    ////Tween tween = _unitARR[XIndex - 1, YIndex + 1].transform.DOMove(_unitPosARR[XIndex , YIndex], 0.3f)
                    ////    .SetEase(Ease.OutBounce).OnComplete(() => StartCoroutine(dropUnit(XIndex, YIndex, borrowNeed - 1)));

                    //// If the Tween that attched to this gameobject 's transform is still running,wait until it complete
                    //if (DOTween.TweensByTarget(target.transform) != null && DOTween.TweensByTarget(target.transform).Count > 0)
                    //{
                    //    yield return DOTween.TweensByTarget(target.transform)[0].WaitForCompletion();
                    //    if (DOTween.TweensByTarget(target.transform) != null)
                    //    {
                    //        yield return DOTween.TweensByTarget(target.transform)[0].WaitForCompletion();
                    //    }
                    //}

                    //Tween t = target.transform.DOMove(_unitPosARR[XIndex - 1, YIndex - 1], unitPushTime)
                    //    .SetEase(Ease.InSine);

                    ////yield return tween.WaitForCompletion();
                    ////yield return new WaitForSeconds(unitDropTime + 0.5f);

                    //---------------------------------------------
                    //---------------------------------------------

                    List<Tween> tweenList = DOTween.TweensByTarget(target.transform);
                    if (tweenList != null)
                    {
                        Tween t = target.transform.DOMove(_unitPosARR[XIndex - 1, YIndex - 1], unitPushTime).SetEase(Ease.Linear);
                        t.Pause();
                        //yield return tweenList[tweenList.Count - 1].WaitForCompletion();
                        //t.Play();

                        tweenList[tweenList.Count - 1].OnComplete(() => t.Play());
                    }
                    else
                    {
                        target.transform.DOMove(_unitPosARR[XIndex - 1, YIndex - 1], unitPushTime).SetEase(Ease.Linear);
                    }

                    break;
                }
            case unitPushType.Right:
                {
                    //unitsList.Remove(_unitARR[XIndex, YIndex]);
                    //unitsList.Add(_unitARR[XIndex, YIndex]);

                    if (!unitsList.Contains(_unitARR[XIndex, YIndex]))
                    {
                        unitsList.Add(_unitARR[XIndex, YIndex]);
                    }

                    _unitARR[XIndex + 1, YIndex - 1] = _unitARR[XIndex, YIndex];
                    _unitARR[XIndex + 1, YIndex - 1].GetComponent<UnitInfo>()._XIndex++;
                    _unitARR[XIndex + 1, YIndex - 1].GetComponent<UnitInfo>()._YIndex--;

                    ChainedUnitsScanner.Instance._scanUnitARR[XIndex + 1, YIndex - 1]._isChained = false;
                    ChainedUnitsScanner.Instance._scanUnitARR[XIndex    , YIndex    ]._isChained = true;


                    //---------------------------------------------
                    //------------Temporary Disabled---------------
                    //---------------------------------------------

                    //// If the Tween that attched to this gameobject 's transform is still running,wait until it complete
                    //if (DOTween.TweensByTarget(target.transform) != null)
                    //{
                    //    yield return DOTween.TweensByTarget(target.transform)[0].WaitForCompletion();
                    //    if (DOTween.TweensByTarget(target.transform) != null)
                    //    {
                    //        yield return DOTween.TweensByTarget(target.transform)[0].WaitForCompletion();
                    //        if (DOTween.TweensByTarget(target.transform) != null)
                    //        {
                    //            yield return DOTween.TweensByTarget(target.transform)[0].WaitForCompletion();
                    //        }
                    //    }
                    //}

                    ////if (DOTween.TweensByTarget(target.transform) != null && DOTween.TweensByTarget(target.transform).Count > 0)
                    ////{
                    ////    yield return DOTween.TweensByTarget(target.transform)[DOTween.TweensByTarget(target.transform).Count - 1].WaitForCompletion();
                    ////}

                    //Tween  t = target.transform.DOMove(_unitPosARR[XIndex + 1, YIndex - 1], unitPushTime)
                    //    .SetEase(Ease.InSine);

                    //yield return tween.WaitForCompletion();

                    //---------------------------------------------
                    //---------------------------------------------
                    //---------------------------------------------

                    List<Tween> tweenList = DOTween.TweensByTarget(target.transform);
                    if (tweenList != null)
                    {
                        Tween t = target.transform.DOMove(_unitPosARR[XIndex + 1, YIndex - 1], unitPushTime).SetEase(Ease.Linear);
                        t.Pause();
                        //yield return tweenList[tweenList.Count - 1].WaitForCompletion();
                        //t.Play();

                        tweenList[tweenList.Count - 1].OnComplete(() => t.Play());
                    }
                    else
                    {
                        target.transform.DOMove(_unitPosARR[XIndex + 1, YIndex - 1], unitPushTime).SetEase(Ease.Linear);
                    }

                    break;
                }
                //yield return null;
        }
    }

    /// <summary>
    /// Drop the Unit with the given Index a distance
    /// </summary>
    /// <param name="XIndex"></param>
    /// <param name="YIndex"></param>
    /// <param name="distanceInUnit"></param>
    /// <returns></returns>
    private void dropUnit(int XIndex, int YIndex, int distanceInUnit)
    {
        Vector3 targetPos = _unitPosARR[XIndex, YIndex - distanceInUnit];

        // Make scanUnit that has fallen down unit become chained
        ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained = true;
        ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex - distanceInUnit]._isChained = false;

        // Update Unit info
        _unitARR[XIndex, YIndex - distanceInUnit] = _unitARR[XIndex, YIndex];
        _unitARR[XIndex, YIndex - distanceInUnit].GetComponent<UnitInfo>()._YIndex -= distanceInUnit;

        GameObject targetObject = _unitARR[XIndex, YIndex - distanceInUnit];

        bool unBounce = false;
        if (getItemIndexFromUnbounceList(XIndex, YIndex - distanceInUnit) != -1)
        {
            unBounce = true;
        }

        //Unit.transform.DOMove(targetPos, unitDropTime).SetEase(Ease.OutBounce);

        //_unitARR[XIndex, YIndex - distanceInUnit].transform.DOMove(targetPos, unitDropTime).SetEase(Ease.OutBounce);        

        // ------------------------------------------------------
        // ----------------- Temporary Disabled -----------------
        // ------------------------------------------------------

        //// If the Tween that attached to this gameobject 's transform is still running,wait until it complete
        //if (DOTween.TweensByTarget(targetObject.transform) != null)
        //{
        //    yield return DOTween.TweensByTarget(targetObject.transform)[0].WaitForCompletion();
        //    yield return new WaitForEndOfFrame();
        //    if (DOTween.TweensByTarget(targetObject.transform) != null)
        //    {
        //        yield return DOTween.TweensByTarget(targetObject.transform)[0].WaitForCompletion();
        //        yield return null;
        //        if (DOTween.TweensByTarget(targetObject.transform) != null)
        //        {
        //            yield return DOTween.TweensByTarget(targetObject.transform)[0].WaitForCompletion();
        //        }
        //    }
        //}
        ////_unitARR[XIndex, YIndex - distanceInUnit].transform.DOMove(targetPos, unitDropTime).SetEase(Ease.InSine);

        ////targetObject.transform.DOMove(targetPos, unitDropTime).SetEase(Ease.InSine);
        //targetObject.transform.DOMove(targetPos, unitDropTime).SetEase(curveTween);
        ////targetObject.transform.DOMove(targetPos, unitDropTime - 0.1f).
        ////    OnComplete(() => targetObject.transform.DOMoveY(targetObject.transform.position.y + 0.1f, 0.1f).SetEase(Ease.OutSine).SetLoops(2, LoopType.Yoyo)).
        ////    SetEase(Ease.InSine);

        // ------------------------------------------------------
        // ------------------------------------------------------

        List<Tween> tweenList = DOTween.TweensByTarget(targetObject.transform);
        if (tweenList != null)
        {
            unitPushType pushType = checkIfCanPush(XIndex, YIndex - distanceInUnit);
            if (pushType != unitPushType.None)
            {
                switch (pushType)
                {
                    case unitPushType.Left:
                        {
                            if (hasDropableUnitAbove(XIndex - 1, YIndex))
                            {
                                Tween t = targetObject.transform.DOMove(targetPos, baseUnitDropTime * distanceInUnit).SetEase(Ease.Linear);
                                t.Pause();
                                Tweener k = targetObject.transform.DOLocalMoveY(0.1f, unitBounceTime/2).
                                    SetLoops(2, LoopType.Yoyo).SetEase(Ease.Linear).SetRelative(true);
                                k.Pause();

                                tweenList[tweenList.Count - 1].OnComplete(() => t.Play());
                                t.OnComplete(() => k.Play());
                            }
                            else
                            {
                                Tween t = targetObject.transform.DOMove(targetPos, baseUnitDropTime * distanceInUnit).SetEase(Ease.Linear);
                                t.Pause();
                                tweenList[tweenList.Count - 1].OnComplete(() => t.Play());
                            }
                            break;
                        }

                    case unitPushType.Right:
                        {
                            if (hasDropableUnitAbove(XIndex + 1, YIndex))
                            {
                                Tween t = targetObject.transform.DOMove(targetPos, baseUnitDropTime * distanceInUnit).SetEase(Ease.Linear);
                                t.Pause();
                                Tweener k = targetObject.transform.DOLocalMoveY(0.1f, unitBounceTime/2).
                                    SetLoops(2, LoopType.Yoyo).SetEase(Ease.Linear).SetRelative(true);
                                k.Pause();

                                tweenList[tweenList.Count - 1].OnComplete(() => t.Play());
                                t.OnComplete(() => k.Play());
                            }
                            else
                            {
                                Tween t = targetObject.transform.DOMove(targetPos, baseUnitDropTime * distanceInUnit).SetEase(Ease.Linear);
                                t.Pause();
                                tweenList[tweenList.Count - 1].OnComplete(() => t.Play());
                            }
                            break;
                        }
                }

                //Tween t = targetObject.transform.DOMove(targetPos, baseUnitDropTime * distanceInUnit).SetEase(Ease.Linear);
                //t.Pause();
                ////yield return tweenList[tweenList.Count - 1].WaitForCompletion();
                ////t.Play();
                //tweenList[tweenList.Count - 1].OnComplete(() => t.Play());
            }
            else if (unBounce)
            {
                Tween t = targetObject.transform.DOMove(targetPos, baseUnitDropTime * distanceInUnit).SetEase(Ease.Linear);
                t.Pause();
                tweenList[tweenList.Count - 1].OnComplete(() => t.Play());
            }
            else
            {
                //print(XIndex + ">>>>>>>>>>>" + (YIndex - distanceInUnit) + ">>>>>>>>" + "curve");
                Tween t = targetObject.transform.DOMove(targetPos, baseUnitDropTime * distanceInUnit).SetEase(Ease.Linear);
                t.Pause();
                Tweener k = targetObject.transform.DOLocalMoveY(0.1f, unitBounceTime/2).
                    SetLoops(2, LoopType.Yoyo).SetEase(Ease.Linear).SetRelative(true);           
                k.Pause();

                //yield return tweenList[tweenList.Count - 1].WaitForCompletion();
                //t.Play();

                tweenList[tweenList.Count - 1].OnComplete(() => t.Play());
                t.OnComplete(() => k.Play());

            }            
        }
        else
        {
            //if (checkIfCanPush(XIndex, YIndex - distanceInUnit) != unitPushType.None)
            //{
            //    targetObject.transform.DOMove(targetPos, unitDropTime).SetEase(Ease.InSine);
            //}
            //else
            //{
            //    targetObject.transform.DOMove(targetPos, unitDropTime).SetEase(curveTween);
            //}

            //targetObject.transform.DOMove(targetPos, unitDropTime).SetEase(Ease.InSine);

            unitPushType pushType = checkIfCanPush(XIndex, YIndex - distanceInUnit);
            if (pushType != unitPushType.None)
            {
                switch (pushType)
                {
                    case unitPushType.Left:
                        {
                            if (hasDropableUnitAbove(XIndex - 1, YIndex))
                            {
                                Tween t = targetObject.transform.DOMove(targetPos, baseUnitDropTime * distanceInUnit).SetEase(Ease.Linear);
                                Tweener k = targetObject.transform.DOLocalMoveY(0.1f, unitBounceTime/2).
                                    SetLoops(2, LoopType.Yoyo).SetEase(Ease.Linear).SetRelative(true);
                                k.Pause();
                                t.OnComplete(() => k.Play());
                            }
                            else
                            {
                                targetObject.transform.DOMove(targetPos, baseUnitDropTime * distanceInUnit).SetEase(Ease.Linear);
                            }
                            break;
                        }

                    case unitPushType.Right:
                        {
                            if (hasDropableUnitAbove(XIndex + 1, YIndex))
                            {
                                Tween t = targetObject.transform.DOMove(targetPos, baseUnitDropTime * distanceInUnit).SetEase(Ease.Linear);
                                Tweener k = targetObject.transform.DOLocalMoveY(0.1f, unitBounceTime/2).
                                    SetLoops(2, LoopType.Yoyo).SetEase(Ease.Linear).SetRelative(true);
                                k.Pause();
                                t.OnComplete(() => k.Play());
                            }
                            else
                            {
                                targetObject.transform.DOMove(targetPos, baseUnitDropTime * distanceInUnit).SetEase(Ease.Linear);
                            }
                            break;
                        }
                }

                //targetObject.transform.DOMove(targetPos, baseUnitDropTime * distanceInUnit).SetEase(Ease.Linear);
            }
            else if (unBounce)
            {
                targetObject.transform.DOMove(targetPos, baseUnitDropTime * distanceInUnit).SetEase(Ease.Linear);
            }
            else
            {
                Tween t = targetObject.transform.DOMove(targetPos, baseUnitDropTime * distanceInUnit).SetEase(Ease.Linear);
                Tweener k = targetObject.transform.DOLocalMoveY(0.1f, unitBounceTime/2).
                    SetLoops(2, LoopType.Yoyo).SetEase(Ease.Linear).SetRelative(true);
                k.Pause();
                t.OnComplete(() => k.Play());
            }
        }

        //if (YIndex < _rows - 1 && _unitARR[XIndex, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen)
        //{
        //    bool canPush = false;
        //    if (XIndex == 0)
        //    {
        //        if (_unitARR[XIndex + 1, YIndex + 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen
        //            && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex + 1, YIndex + 1]._isChained)
        //        {
        //            canPush = true;
        //            pushUnit(XIndex + 1, YIndex + 1, unitPushType.Left);
        //        }
        //    }
        //    else if (XIndex == _columns - 1)
        //    {
        //        if (_unitARR[XIndex - 1, YIndex + 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen
        //            && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex - 1, YIndex + 1]._isChained)
        //        {
        //            canPush = true;
        //            pushUnit(XIndex - 1, YIndex + 1, unitPushType.Right);
        //        }
        //    }
        //    else
        //    {
        //        if (_unitARR[XIndex + 1, YIndex + 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen
        //            && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex + 1, YIndex + 1]._isChained)
        //        {
        //            canPush = true;
        //            pushUnit(XIndex + 1, YIndex + 1, unitPushType.Left);
        //        }
        //        else if (_unitARR[XIndex - 1, YIndex + 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen
        //            && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex - 1, YIndex + 1]._isChained)
        //        {
        //            canPush = true;
        //            pushUnit(XIndex - 1, YIndex + 1, unitPushType.Right);
        //        }
        //    }

        //    if (canPush && distanceInUnit > 1)
        //    {
        //        dropUnit(XIndex, YIndex, distanceInUnit - 1);
        //    }
        //}
    }

    #endregion

    public void upgradeRandomUnitIntoLightning()
    {
        int randomXIndex = Random.Range(0, _columns);
        int randomYIndex = Random.Range(0, _rows);
        int randomLighting = Random.Range(0, 2);

        if (ChainedUnitsScanner.Instance._scanUnitARR[randomXIndex, randomYIndex]._isChained || 
            _unitARR[randomXIndex, randomYIndex].GetComponent<UnitInfo>()._unitEff != UnitInfo.SpecialEff.noEff ||
            _unitARR[randomXIndex, randomYIndex].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.noEff ||
            ChainedUnitsScanner.Instance._scanUnitARR[randomXIndex, randomYIndex]._value == Unit.Length - 1)
        {
            upgradeRandomUnitIntoLightning();
        }
        else
        {
            switch (randomLighting)
            {
                case 0:
                    {
                        upgradeUnit(randomXIndex, randomYIndex, UnitInfo.SpecialEff.vLightning, UnitInfo.NegativeEff.noEff);
                        break;
                    }
                case 1:
                    {
                        upgradeUnit(randomXIndex, randomYIndex, UnitInfo.SpecialEff.hLightning, UnitInfo.NegativeEff.noEff);
                        break;
                    }
            }            
        }
    }

    private int[,] generateValueMatrix()
    {
        //int[,] valueMatrix = new int[_columns, _rows];

        //for (int i = 0; i < _columns; i++)
        //{
        //    for (int j = 0; j < _rows; j++)
        //    {
        //        valueMatrix[i, j] = Random.Range(0, Unit.Length - 1);
        //    }
        //}
        //return valueMatrix;

        List<int>[,] valueListARR = new List<int>[_columns, _rows];

        for (int XIndex = 0; XIndex < _columns; XIndex++)
        {
            for (int YIndex = 0; YIndex < _rows; YIndex++)
            {
                valueListARR[XIndex, YIndex] = new List<int>();

                for (int i = 0; i < Unit.Length - 1; i++)
                {
                    valueListARR[XIndex, YIndex].Add(i);
                }
            }
        }

        int[,] valueMatrix = new int[_columns, _rows];

        for (int XIndex = 0; XIndex < _columns; XIndex++)
        {
            for (int YIndex = 0; YIndex < _rows; YIndex++)
            {
                // Scan current Unit's relatives to remove value that shouldn't be choosen
                if (XIndex > 1)
                {
                    if (valueMatrix[XIndex - 1, YIndex] == valueMatrix[XIndex - 2, YIndex])
                    {
                        valueListARR[XIndex, YIndex].Remove(valueMatrix[XIndex - 1, YIndex]);
                    }
                }
                if (YIndex > 1)
                {
                    if (valueMatrix[XIndex, YIndex - 1] == valueMatrix[XIndex, YIndex - 2])
                    {
                        valueListARR[XIndex, YIndex].Remove(valueMatrix[XIndex, YIndex - 1]);
                    }
                }

                // Select a random possible value from valueList if there is still value to choose
                if (valueListARR[XIndex, YIndex].Count != 0)
                {
                    valueMatrix[XIndex, YIndex] = valueListARR[XIndex, YIndex][Random.Range(0, valueListARR[XIndex, YIndex].Count)];
                    valueListARR[XIndex, YIndex].Remove(valueMatrix[XIndex, YIndex]);
                }
                // If there is no possible value left,recover current list and backtrack to previous unit
                else
                {
                    //valueListARR[XIndex, YIndex] = new List<int>();
                    for (int i = 0; i < Unit.Length - 1; i++)
                    {
                        valueListARR[XIndex, YIndex].Add(i);
                    }

                    if (XIndex == 0)
                    {
                        YIndex -= 1;
                        XIndex = _columns - 1;
                    }
                    else
                    {
                        XIndex -= 1;
                    }
                    continue;
                }


                //valueMatrix[XIndex, YIndex] = Random.Range(0, Unit.Length - 1);
            }
        }
        return valueMatrix;
    }

    private void getDropableUnitIndexList(int XIndex, int YIndex)
    {
        for (int i = YIndex; i <= _rows - 1; i++)
        {
            if (_unitARR[XIndex, i].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
            {
                continue;
            }
            else if (_unitARR[XIndex, i].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen)
            {
                break;
            }
            else
            {
                //if (!unbounceList.Contains(new int[2]  { XIndex, i})){ }
                unbounceList.Add(new int[2] { XIndex, i });

                if (i == _rows - 1)
                {
                    hasUnitToRegen = true;
                }
            }
        }
    }

    private int getItemIndexFromUnbounceList(int XIndex, int YIndex)
    {
        int result = -1;

        for (int i = 0; i < unbounceList.Count; i++)
        {
            if (unbounceList[i][0] == XIndex && unbounceList[i][1] == YIndex)
            {
                result = i;
                unbounceList.RemoveAt(i);
            }
        }
        return result;
    }

    private bool hasDropableUnitAbove(int XIndex, int YIndex)
    {
        for (int i = YIndex - 1; i <= _rows - 1; i++)
        {
            if (_unitARR[XIndex, i].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
            {
                continue;
            }
            else if (_unitARR[XIndex, i].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen)
            {
                break;
            }
            else if (!ChainedUnitsScanner.Instance._scanUnitARR[XIndex, i]._isChained)
            {
                return true;
            }
        }

        return false; 
    }

    private bool hasFrozenUnitInCol(int col)
    {
        for (int YIndex = 0; YIndex < _rows; YIndex++)
        {
            if (_unitARR[col, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen)
            {
                return true;
            }
        }
        return false;
    }
    
    // Check if this Unit is above the highest frozen unit in the same column
    private bool noFrozenUnitAbove(int XIndex, int YIndex)
    {
        for (int i = YIndex + 1; i < _rows; i++)
        {
            if (_unitARR[XIndex, i].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.frozen)
            {
                return false;
            }
        }
        return true;
    }

    public GameObject getJewel(int value, Vector2 spawnPos)
    {
        GameObject jewel = null;

        for (int i = 0; i < poolSize; i++)
        {
            jewel = _poolARR[value][i];
            if (!jewel.activeSelf)
            {
                jewel.transform.position = spawnPos;
                jewel.SetActive(true);
                break;
            }
        }
        return jewel;
    }

    private IEnumerator waitForTweenComplete()
    {
        while (DOTween.TotalPlayingTweens() != 0)
        {
            //print("yielding");
            yield return new WaitForEndOfFrame();
        }        
    }

    public List<UnitInfo> getUnitsOfTypeInfo(int value)
    {
        List<UnitInfo> infoList = new List<UnitInfo>();

        for (int YIndex = 0; YIndex < _rows; YIndex++)
        {
            for (int XIndex = 0; XIndex < _columns; XIndex++)
            {
                UnitInfo unitInfo = _unitARR[XIndex, YIndex].GetComponent<UnitInfo>();
                if (unitInfo._value == value
                    && !ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained 
                    && unitInfo._negativeEff != UnitInfo.NegativeEff.hollow
                    && unitInfo._negativeEff != UnitInfo.NegativeEff.empty)
                {
                    infoList.Add(unitInfo);
                }
            }
        }

        print(infoList.Count);

        return infoList;
    }
}
