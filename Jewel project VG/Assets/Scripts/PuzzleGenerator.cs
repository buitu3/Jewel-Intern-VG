using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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


    public int _rows;
    public int _columns;

    public int[,] _valueARR;
    public GameObject[,] _unitARR;
    public Vector2[,] _unitPosARR;

    private List<GameObject>[] _poolARR;

    private Transform unitHolder;
    private Transform unitBGHolder;
    private Transform poolObjectHolder;

    //private float XStartPos = -2.6f;
    //private float YStartPos = -3.7f;
    private float XPadding = 0.73f;
    private float YPadding = 0.73f;
    private float regenYpos = 8f;

    private float unitDropTime = 0.8f;

    private int poolSize;

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        unitHolder = new GameObject("Units Holder").transform;
        unitBGHolder = new GameObject("Units BG Holder").transform;
        poolObjectHolder = new GameObject("Pool Object Holder").transform;

        // Init valueArr
        _valueARR = generateValueMatrix();
        // Init unitArr
        _unitARR = new GameObject[_columns, _rows];
        // Init unitPosArr
        _unitPosARR = new Vector2[_columns, _rows];

        // Init all Object pools
        _poolARR = new List<GameObject>[Unit.Length];
        Vector2 poolPos = new Vector2(-20, -20);
        poolSize = _rows * _columns;
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
        _valueARR[1, 1] = 2;
        _valueARR[2, 1] = 5;
        _valueARR[3, 1] = 6;
        _valueARR[1, 2] = 5;
        _valueARR[2, 2] = 1;
        _valueARR[3, 2] = 4;
        _valueARR[1, 3] = 7;
        _valueARR[2, 3] = 4;
        _valueARR[3, 3] = 4;
        _valueARR[4, 4] = 4;

        //_valueARR[4, 2] = 3;
        //_valueARR[5, 2] = 2;

        _valueARR[1, 0] = 6;
        // --------------------------------------------

        // Init Jewel Puzzle and BG
        for (int YIndex = 0; YIndex < _rows; YIndex++)
        {
            for ( int XIndex = 0; XIndex < _columns; XIndex++)
            {
                Vector2 spawnPos = new Vector2(startingSpawnPos.position.x + XIndex * XPadding,
                                                startingSpawnPos.position.y + YIndex * YPadding);

                // Store position of each unit in the puzzle
                _unitPosARR[XIndex, YIndex] = spawnPos;

                // Init Jewel BG
                GameObject UnitBG = Instantiate(UnitBGPreb, spawnPos, Quaternion.identity) as GameObject;
                UnitBG.transform.SetParent(unitBGHolder);

                //int unitType = Random.Range(0, Unit.Length - 1);
                //Instantiate(Unit[unitType], spawnPos, Quaternion.identity);
                initUnit(spawnPos, XIndex, YIndex, _valueARR[XIndex, YIndex], 0);
            }
        }

        GameController.Instance.currentState = GameController.GameState.scanningUnit;
        yield return new WaitForSeconds(1f);

        // ----------Fake unit value for testing ------------------

        //_unitARR[1, 0].GetComponent<UnitInfo>()._unitEff = UnitInfo.SpecialEff.vLightning;
        upgradeUnit(2, 3, UnitInfo.SpecialEff.explode);
        //upgradeUnit(3, 2, UnitInfo.SpecialEff.hLightning);
        upgradeUnit(0, 2, UnitInfo.SpecialEff.vLightning);

        // --------------------------------------------

        //----------------------------------------------------------------------------------
        // Remember to return when done tesing
        //----------------------------------------------------------------------------------

        // Scan whole puzzle for chained Units
        //ChainedUnitsScanner.Instance.scanAll();

        //----------------------------------------------------------------------------------
        //
        //----------------------------------------------------------------------------------

        // ----------Fake for testing ------------------

        List<GameObject> unitsList = new List<GameObject>();
        for (int XIndex = 0; XIndex < _columns; XIndex++)
        {
            for (int YIndex = 0; YIndex < _rows; YIndex++)
            {
                unitsList.Add(_unitARR[XIndex, YIndex]);
            }
        }
        StartCoroutine(ChainedUnitsScanner.Instance.scanRegenUnits(unitsList));
        // --------------------------------------------

        //GameController.Instance.currentState = GameController.GameState.idle;
    }


    //==============================================
    // Methods
    //==============================================

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

        //If this is regen Unit,move to it's original pos
        if (spawnPos.y != _unitPosARR[XIndex, YIndex].y)
        {
            _unitARR[XIndex, YIndex].transform.DOMove(_unitPosARR[XIndex, YIndex], unitDropTime).SetEase(Ease.InQuad);
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
        initUnit(PuzzleGenerator.Instance._unitPosARR[XIndex, YIndex], XIndex, YIndex, Unit.Length - 1, UnitInfo.SpecialEff.noEff);
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
        initUnit(PuzzleGenerator.Instance._unitPosARR[XIndex, YIndex], XIndex, YIndex, value, specialEff);
        ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._value = value;
        ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained = false;
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

    private int[,] generateValueMatrix()
    {
        int[,] valueMatrix = new int[_columns, _rows];

        for (int i = 0; i < _columns; i++)
        {
            for (int j = 0; j < _rows; j++)
            {
                valueMatrix[i, j] = Random.Range(0, Unit.Length - 1);
            }
        }
        return valueMatrix;
    }

    public IEnumerator reOrganizePuzzle()
    {
        //yield return new WaitForSeconds(1f);

        // Make Units fall down after destroy state
        for (int XIndex = 0; XIndex < _columns; XIndex++)
        {
            int nullObjectCount = 0;
            for (int YIndex = 0; YIndex < _rows; YIndex++)
            {
                // Search for emty space
                if (ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained)
                {
                    nullObjectCount += 1;
                }
                else
                {
                    // Make Unit fall down if there are empty space below
                    if (nullObjectCount > 0)
                    {
                        Vector3 targetPos = _unitPosARR[XIndex, YIndex - nullObjectCount];
                        StartCoroutine(dropUnit(_unitARR[XIndex, YIndex], targetPos));

                        // Update Unit info
                        _unitARR[XIndex, YIndex - nullObjectCount] = _unitARR[XIndex, YIndex];
                        _unitARR[XIndex, YIndex - nullObjectCount].GetComponent<UnitInfo>()._YIndex -= nullObjectCount;

                    }
                }
            }
            if (nullObjectCount > 0)
            {
                for (int i = 0; i < nullObjectCount; i++)
                {
                    Vector2 regenUnitSpawnPos = new Vector2(_unitPosARR[XIndex, _rows - i - 1].x, regenYpos + i * YPadding);
                    initUnit(regenUnitSpawnPos, XIndex, _rows - i - 1, Random.Range(0, Unit.Length - 1), 0);
                    //initUnit(_unitPosARR[XIndex, _rows - i - 1], XIndex, _rows - i - 1, Random.Range(0, Unit.Length - 1), 0);
                }
            }
        }
        yield return new WaitForSeconds(unitDropTime + 0.5f);

        // Temporaty disable --------------

        //ChainedUnitsScanner.Instance.scanAll();

        // --------------------------------

        // ----------Fake for testing ------------------

        List<GameObject> unitsList = new List<GameObject>();
        for (int XIndex = 0; XIndex < _columns; XIndex++)
        {
            for (int YIndex = 0; YIndex < _rows; YIndex++)
            {
                unitsList.Add(_unitARR[XIndex, YIndex]);
            }
        }
        StartCoroutine(ChainedUnitsScanner.Instance.scanRegenUnits(unitsList));
        // --------------------------------------------
    }

    private IEnumerator dropUnit(GameObject Unit, Vector2 targetPos)
    {
        yield return new WaitForSeconds(0.1f);
        Unit.transform.DOMove(targetPos, unitDropTime).SetEase(Ease.OutBounce);
    }
}
