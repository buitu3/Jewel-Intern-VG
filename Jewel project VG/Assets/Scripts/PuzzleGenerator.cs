using UnityEngine;
using System.Collections;
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

    private Transform unitHolder;
    private Transform unitBGHolder;

    //private float XStartPos = -2.6f;
    //private float YStartPos = -3.7f;
    private float XPadding = 0.73f;
    private float YPadding = 0.73f;

    private float unitDropTime = 0.6f;

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        unitHolder = new GameObject("Units Holder").transform;
        unitBGHolder = new GameObject("Units BG Holder").transform;

        // Init valueArr
        _valueARR = generateValueMatrix();
        // Init unitArr
        _unitARR = new GameObject[_columns, _rows];
        //Init unitPosArr
        _unitPosARR = new Vector2[_columns, _rows];

        // Init Jewel Puzzle and BG
        for (int YIndex = 0; YIndex < _rows; YIndex++)
        {
            for ( int XIndex = 0; XIndex < _columns; XIndex++)
            {
                Vector2 spawnPos = new Vector2(startingSpawnPos.position.x + XIndex * XPadding,
                                                startingSpawnPos.position.y + YIndex * YPadding);
                GameObject UnitBG = Instantiate(UnitBGPreb, spawnPos, Quaternion.identity) as GameObject;
                UnitBG.transform.SetParent(unitBGHolder);

                //int unitType = Random.Range(0, Unit.Length - 1);
                //Instantiate(Unit[unitType], spawnPos, Quaternion.identity);
                initUnit(spawnPos, XIndex, YIndex, _valueARR[XIndex, YIndex], 0);

                // Store position of each unit in the puzzle
                _unitPosARR[XIndex, YIndex] = spawnPos;
            }
        }

    }

    //==============================================
    // Methods
    //==============================================

    public void initUnit(Vector2 spawnPos, int XIndex, int YIndex, int value, UnitInfo.SpecialEff specialEff)
    {
        // Instantiate the unit
        _unitARR[XIndex, YIndex] = Instantiate(Unit[value], spawnPos, Quaternion.identity) as GameObject;
        _unitARR[XIndex, YIndex].transform.SetParent(unitHolder);

        // Set infomation for this unit
        UnitInfo unitInfo = _unitARR[XIndex, YIndex].GetComponent<UnitInfo>();
        unitInfo._XIndex = XIndex;
        unitInfo._YIndex = YIndex;
        unitInfo._value = value;
        unitInfo._unitEff = specialEff;
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
                        StartCoroutine(moveUnit(_unitARR[XIndex, YIndex], targetPos));

                        // Update Unit info
                        _unitARR[XIndex, YIndex - nullObjectCount] = _unitARR[XIndex, YIndex];
                        _unitARR[XIndex, YIndex - nullObjectCount].GetComponent<UnitInfo>()._YIndex -= nullObjectCount;

                        // Regen Units
                        //initUnit(_unitPosARR[XIndex, YIndex], XIndex, YIndex, Random.Range(0, Unit.Length - 1), 0);
                    }
                }
            }
            if (nullObjectCount > 0)
            {
                for (int i = 0; i < nullObjectCount; i++)
                {
                    initUnit(_unitPosARR[XIndex, _rows - i-1], XIndex, _rows - i - 1, Random.Range(0, Unit.Length - 1), 0);
                }
            }
        }
        // Update ScanUnitARr
        ChainedUnitsScanner.Instance.updateScanARR();
        yield return new WaitForSeconds(unitDropTime + 0.1f);
        ChainedUnitsScanner.Instance.scanAll();
    }

    private IEnumerator moveUnit(GameObject Unit, Vector2 targetPos)
    {
        yield return new WaitForSeconds(0.1f);
        Unit.transform.DOMove(targetPos, unitDropTime).SetEase(Ease.OutBounce);
    }
}
