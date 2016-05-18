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

    public GameObject blueUnitBGPreb;

    private GameObject[,] _blueBGARR;
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
        _blueBGARR = new GameObject[PuzzleGenerator.Instance._columns, PuzzleGenerator.Instance._rows];
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

                if (unitBGLevel == 1)
                {
                    _blueBGARR[XIndex, YIndex] = Instantiate(blueUnitBGPreb, PuzzleGenerator.Instance._unitPosARR[XIndex, YIndex], Quaternion.identity) as GameObject;
                    _valueBGARR[XIndex, YIndex] = 1;
                    blueBGCount++;
                }
            }
        }
    }

    public void removeBG(int XIndex, int YIndex)
    {
        if (_blueBGARR[XIndex, YIndex] != null && _valueBGARR[XIndex, YIndex] == 1)
        {
            Destroy(_blueBGARR[XIndex, YIndex]);
            _valueBGARR[XIndex, YIndex]--;
            blueBGCount--;
        }
    }
}
