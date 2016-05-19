using UnityEngine;
using System.Collections;

public class UnitBGGenerator : MonoBehaviour {

    //==============================================
    // Constants
    //==============================================

    public static UnitBGGenerator Instance;

    //==============================================
    // Fields
    //==============================================

    private int blueBGCount;
    public int BlueBGCount
    {
        get
        {
            return blueBGCount;
        }
    }

    public GameObject[] unitBGPreb;

    private GameObject[,] _unitBGARR;
    private int[,] _valueBGARR;

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        blueBGCount = 0;
        _unitBGARR = new GameObject[PuzzleGenerator.Instance._columns, PuzzleGenerator.Instance._rows];
        _valueBGARR = new int[PuzzleGenerator.Instance._columns, PuzzleGenerator.Instance._rows];

        initBlueBG();
    }

    //==============================================
    // Methods
    //==============================================

    void initBlueBG()
    {
        for (int YIndex = 0; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        {
            JSONObject YIndexJSON = LevelsManager.Instance.selectedLevelInfoJSON.GetField("Row " + YIndex);

            for (int XIndex = 0; XIndex < PuzzleGenerator.Instance._columns; XIndex++)
            {
                JSONObject XIndexJSON = YIndexJSON.GetField("Col " + XIndex);
                int unitBGLevel = (int)XIndexJSON.GetField("Unit Level").i;
                string negativeEff = XIndexJSON.GetField("Negative Eff").str;

                if (unitBGLevel > 0 && unitBGLevel <= unitBGPreb.Length)
                {
                    _unitBGARR[XIndex, YIndex] = Instantiate(unitBGPreb[unitBGLevel - 1], PuzzleGenerator.Instance._unitPosARR[XIndex, YIndex], Quaternion.identity) as GameObject;
                    _unitBGARR[XIndex, YIndex].transform.SetParent(PuzzleGenerator.Instance.unitBGHolder);

                    _valueBGARR[XIndex, YIndex] = unitBGLevel;
                    blueBGCount++;
                }
            }
        }
    }

    public void removeBG(int XIndex, int YIndex)
    {
        if (_unitBGARR[XIndex, YIndex] != null)
        {
            if (_valueBGARR[XIndex, YIndex] > 1)
            {
                _valueBGARR[XIndex, YIndex]--;

                Destroy(_unitBGARR[XIndex, YIndex]);
                _unitBGARR[XIndex, YIndex] = Instantiate(unitBGPreb[_valueBGARR[XIndex, YIndex] - 1],
                                                        PuzzleGenerator.Instance._unitPosARR[XIndex, YIndex], Quaternion.identity) as GameObject;
            }
            else if (_valueBGARR[XIndex, YIndex] == 1)
            {
                _valueBGARR[XIndex, YIndex]--;
                blueBGCount--;

                Destroy(_unitBGARR[XIndex, YIndex]);

                GameController.Instance.updateUnitBGCountText(blueBGCount);
                if (blueBGCount == 0)
                {
                    print("Game completed");
                }
            }
        }
    }
}
