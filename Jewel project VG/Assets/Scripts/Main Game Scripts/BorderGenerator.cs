using UnityEngine;
using System.Collections;

public class BorderGenerator : MonoBehaviour {

    //==============================================
    // Constants
    //==============================================

    public static BorderGenerator Instance;

    //==============================================
    // Fields
    //==============================================

    public GameObject sharpCornerPref;
    public GameObject roundCornerPref;
    public GameObject borderLinePref;

    private float roundCornerOffset = 0.325f;
    private float sharpCornerOffset = 0.42f;
    private float borderOffset = 0.38f;

    //==============================================
    // Unity Methods
    //==============================================

    void Awawe()
    {
        Instance = this;
    }

    void Start()
    {
        //Instantiate(borderLinePref, new Vector2(this.transform.position.x,this.transform.position.y + borderOffset), 
        //    Quaternion.Euler(new Vector3(0, 0, 0)));
        //Instantiate(borderLinePref, new Vector2(this.transform.position.x, this.transform.position.y - borderOffset),
        //    Quaternion.Euler(new Vector3(0, 0, 0)));
        //Instantiate(borderLinePref, new Vector2(this.transform.position.x + borderOffset, this.transform.position.y),
        //    Quaternion.Euler(new Vector3(0, 0, 90)));
        //Instantiate(borderLinePref, new Vector2(this.transform.position.x - borderOffset, this.transform.position.y),
        //    Quaternion.Euler(new Vector3(0, 0, 90)));

        ////Instantiate(roundCornerPref, new Vector2(this.transform.position.x - roundCornerOffset, this.transform.position.y + roundCornerOffset),
        ////    Quaternion.Euler(new Vector3(0, 0, 0)));
        //Instantiate(sharpCornerPref, new Vector2(this.transform.position.x - sharpCornerOffset, this.transform.position.y + roundCornerOffset),
        //    Quaternion.Euler(new Vector3(0, 0, -90)));

        //Instantiate(roundCornerPref, new Vector2(this.transform.position.x + roundCornerOffset, this.transform.position.y + roundCornerOffset),
        //    Quaternion.Euler(new Vector3(0, 0, -90)));
        //Instantiate(roundCornerPref, new Vector2(this.transform.position.x + roundCornerOffset, this.transform.position.y - roundCornerOffset),
        //    Quaternion.Euler(new Vector3(0, 0, 180)));
        //Instantiate(roundCornerPref, new Vector2(this.transform.position.x - roundCornerOffset, this.transform.position.y - roundCornerOffset),
        //    Quaternion.Euler(new Vector3(0, 0, 90)));

        initBorder();
    }

    //==============================================
    // Methods
    //==============================================

    public void initBorder()
    {
        for (int YIndex = 0; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        {
            for (int XIndex = 0; XIndex < PuzzleGenerator.Instance._columns; XIndex++)
            {
                Vector2 unitPos = PuzzleGenerator.Instance._unitPosARR[XIndex, YIndex];
                UnitInfo unitInfo = PuzzleGenerator.Instance._unitARR[XIndex, YIndex].GetComponent<UnitInfo>();
    
                if (unitInfo._negativeEff == UnitInfo.NegativeEff.hollow)
                {
                    continue;
                }

                if (XIndex == 0 
                    || PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                {
                    Instantiate(borderLinePref, new Vector2(unitPos.x - borderOffset, unitPos.y),
                        Quaternion.Euler(new Vector3(0, 0, 90)));

                    if (YIndex == 0)
                    {
                        Instantiate(roundCornerPref, new Vector2(unitPos.x - roundCornerOffset, unitPos.y - roundCornerOffset),
                            Quaternion.Euler(new Vector3(0, 0, 90)));
                    }
                    else if (YIndex == PuzzleGenerator.Instance._rows - 1)
                    {
                        Instantiate(roundCornerPref, new Vector2(unitPos.x - roundCornerOffset, unitPos.y + roundCornerOffset),
                            Quaternion.Euler(new Vector3(0, 0, 0)));
                    }
                }

                if (XIndex == PuzzleGenerator.Instance._columns - 1
                    || PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                {
                    Instantiate(borderLinePref, new Vector2(unitPos.x + borderOffset, unitPos.y),
                        Quaternion.Euler(new Vector3(0, 0, 90)));

                    if (YIndex == 0)
                    {
                        Instantiate(roundCornerPref, new Vector2(unitPos.x + roundCornerOffset, unitPos.y - roundCornerOffset),
                            Quaternion.Euler(new Vector3(0, 0, 180)));
                    }
                    else if (YIndex == PuzzleGenerator.Instance._rows - 1)
                    {
                        Instantiate(roundCornerPref, new Vector2(unitPos.x + roundCornerOffset, unitPos.y + roundCornerOffset),
                            Quaternion.Euler(new Vector3(0, 0, -90)));
                    }
                }

                if (YIndex == 0 
                    || PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                {
                    Instantiate(borderLinePref, new Vector2(unitPos.x, unitPos.y - borderOffset),
                        Quaternion.Euler(new Vector3(0, 0, 0)));
                }

                if (YIndex == PuzzleGenerator.Instance._rows - 1
                    || PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                {
                    Instantiate(borderLinePref, new Vector2(unitPos.x, unitPos.y + borderOffset),
                        Quaternion.Euler(new Vector3(0, 0, 0)));
                }
            }
        }
    }
}
