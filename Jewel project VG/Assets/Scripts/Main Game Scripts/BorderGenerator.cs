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

    private Transform borderHolder;
    private GameObject border;

    private float roundCornerOffset = 0.325f;
    private float sharpCornerXOffset = 0.424f;
    private float sharpCornerYOffset = 0.304f;
    private float borderOffset = 0.38f;

    private enum BorderType
    {
        topLine = 0,
        bottomLine,
        leftLine,
        rightLine,
        topLeftRoundCorner,
        topRightRoundCorner,
        bottomLeftRoundCorner,
        bottomRightRoundCorner,
        topLeftSharpCorner,
        topRightSharpCorner,
        bottomLeftSharpCorner,
        bottomRightSharpCorner
    }

    //==============================================
    // Unity Methods
    //==============================================

    void Awawe()
    {
        Instance = this;
    }

    void Start()
    {
        borderHolder = new GameObject("Borders Holder").transform;
        border = new GameObject();

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

        initPuzzleBorder();
    }

    //==============================================
    // Methods
    //==============================================

    public void initPuzzleBorder()
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

                bool hasTopLine = false;
                bool hasBottomLine = false;
                bool hasLeftLine = false;
                bool hasRightLine = false;

                #region Border line check and init
                if (XIndex == 0
                    || PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                {
                    // left line
                    initBorder(unitPos.x, unitPos.y, BorderType.leftLine);

                    hasLeftLine = true;
                }
                

                if (XIndex == PuzzleGenerator.Instance._columns - 1
                    || PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                {
                    initBorder(unitPos.x, unitPos.y, BorderType.rightLine);

                    hasRightLine = true;
                }

                if (YIndex == 0 
                    || PuzzleGenerator.Instance._unitARR[XIndex, YIndex - 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                {
                    initBorder(unitPos.x, unitPos.y, BorderType.bottomLine);

                    hasBottomLine = true;
                }

                if (YIndex == PuzzleGenerator.Instance._rows - 1
                    || PuzzleGenerator.Instance._unitARR[XIndex, YIndex + 1].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.hollow)
                {
                    initBorder(unitPos.x, unitPos.y, BorderType.topLine);

                    hasTopLine = true;
                }
                #endregion

                #region Corner check and init
                // Left side corner check and init
                if (hasLeftLine)
                {
                    // top left corner check
                    if (YIndex == PuzzleGenerator.Instance._rows - 1)
                    {
                        // top left round c
                        initBorder(unitPos.x, unitPos.y, BorderType.topLeftRoundCorner);
                    }
                    else if (XIndex == 0)
                    {
                        if (hasTopLine)
                        {
                            // top left round c
                            initBorder(unitPos.x, unitPos.y, BorderType.topLeftRoundCorner);
                        }
                    }
                    else
                    {
                        if (PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex + 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow)
                        {
                            // top left sharp c
                            initBorder(unitPos.x, unitPos.y, BorderType.topLeftSharpCorner);
                        }
                        else if (hasTopLine)
                        {
                            // top left round c
                            initBorder(unitPos.x, unitPos.y, BorderType.topLeftRoundCorner);
                        }
                    }

                    // bottom left corner check
                    if (YIndex == 0)
                    {
                        // bottom left round c
                        initBorder(unitPos.x, unitPos.y, BorderType.bottomLeftRoundCorner);
                    }
                    else if (XIndex == 0)
                    {
                        if (hasBottomLine)
                        {
                            // bottom left round c
                            initBorder(unitPos.x, unitPos.y, BorderType.bottomLeftRoundCorner);
                        }
                    }
                    else
                    {
                        if (PuzzleGenerator.Instance._unitARR[XIndex - 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow)
                        {
                            // bottom left sharp c
                            initBorder(unitPos.x, unitPos.y, BorderType.bottomLeftSharpCorner);
                        }
                        else if (hasBottomLine)
                        {
                            // bottom left round c
                            initBorder(unitPos.x, unitPos.y, BorderType.bottomLeftRoundCorner);
                        }
                    }
                }

                if (hasRightLine)
                {
                    // top right corner check
                    if (YIndex == PuzzleGenerator.Instance._rows - 1)
                    {
                        // top right round c
                        initBorder(unitPos.x, unitPos.y, BorderType.topRightRoundCorner);
                    }
                    else if (XIndex == PuzzleGenerator.Instance._columns - 1)
                    {
                        if (hasTopLine)
                        {
                            // top right round c
                            initBorder(unitPos.x, unitPos.y, BorderType.topRightRoundCorner);
                        }
                    }
                    else
                    {
                        if (PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex + 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow)
                        {
                            // top right sharp c
                            initBorder(unitPos.x, unitPos.y, BorderType.topRightSharpCorner);
                        }
                        else if (hasTopLine)
                        {
                            // top right round c
                            initBorder(unitPos.x, unitPos.y, BorderType.topRightRoundCorner);
                        }
                    }

                    // bottom right corner check
                    if (YIndex == 0)
                    {
                        // bottom right round c
                        initBorder(unitPos.x, unitPos.y, BorderType.bottomRightRoundCorner);
                    }
                    else if (XIndex == PuzzleGenerator.Instance._columns - 1)
                    {
                        if (hasBottomLine)
                        {
                            // bottom right round c
                            initBorder(unitPos.x, unitPos.y, BorderType.bottomRightRoundCorner);
                        }
                    }
                    else
                    {
                        if (PuzzleGenerator.Instance._unitARR[XIndex + 1, YIndex - 1].GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.hollow)
                        {
                            // bottom right sharp c
                            initBorder(unitPos.x, unitPos.y, BorderType.bottomRightSharpCorner);
                        }
                        else if (hasBottomLine)
                        {
                            // bottom right round c
                            initBorder(unitPos.x, unitPos.y, BorderType.bottomRightRoundCorner);
                        }
                    }
                }
                #endregion
            }
        }
    }

    private void initBorder(float unitXPos, float unitYPos, BorderType borderType)
    {
        switch (borderType)
        {
            case BorderType.topLine:
                {
                    border = Instantiate(borderLinePref, new Vector2(unitXPos, unitYPos + borderOffset), 
                        Quaternion.identity) as GameObject;
                    break;
                }
            case BorderType.bottomLine:
                {
                    border = Instantiate(borderLinePref, new Vector2(unitXPos, unitYPos - borderOffset), 
                        Quaternion.identity) as GameObject;
                    break;
                }
            case BorderType.leftLine:
                {
                    border = Instantiate(borderLinePref, new Vector2(unitXPos - borderOffset, unitYPos), 
                        Quaternion.Euler(new Vector3(0, 0, 90))) as GameObject;
                    break;
                }
            case BorderType.rightLine:
                {
                    border = Instantiate(borderLinePref, new Vector2(unitXPos + borderOffset, unitYPos), 
                        Quaternion.Euler(new Vector3(0, 0, 90))) as GameObject;
                    break;
                }
            case BorderType.topLeftRoundCorner:
                {
                    border = Instantiate(roundCornerPref, new Vector2(unitXPos - roundCornerOffset, unitYPos + roundCornerOffset),
                            Quaternion.Euler(new Vector3(0, 0, 0))) as GameObject;
                    break;
                }
            case BorderType.topRightRoundCorner:
                {
                    border = Instantiate(roundCornerPref, new Vector2(unitXPos + roundCornerOffset, unitYPos + roundCornerOffset),
                            Quaternion.Euler(new Vector3(0, 0, -90))) as GameObject;
                    break;
                }
            case BorderType.bottomLeftRoundCorner:
                {
                    border = Instantiate(roundCornerPref, new Vector2(unitXPos - roundCornerOffset, unitYPos - roundCornerOffset),
                            Quaternion.Euler(new Vector3(0, 0, 90))) as GameObject;
                    break;
                }
            case BorderType.bottomRightRoundCorner:
                {
                    border = Instantiate(roundCornerPref, new Vector2(unitXPos + roundCornerOffset, unitYPos - roundCornerOffset),
                            Quaternion.Euler(new Vector3(0, 0, 180))) as GameObject;
                    break;
                }
            case BorderType.topLeftSharpCorner:
                {
                    border = Instantiate(sharpCornerPref, new Vector2(unitXPos - sharpCornerXOffset, unitYPos + sharpCornerYOffset),
                            Quaternion.Euler(new Vector3(0, 0, -90))) as GameObject;
                    break;
                }
            case BorderType.topRightSharpCorner:
                {
                    border = Instantiate(sharpCornerPref, new Vector2(unitXPos + sharpCornerXOffset, unitYPos + sharpCornerYOffset),
                            Quaternion.Euler(new Vector3(0, 0, 0))) as GameObject;
                    break;
                }
            case BorderType.bottomLeftSharpCorner:
                {
                    border = Instantiate(sharpCornerPref, new Vector2(unitXPos - sharpCornerXOffset, unitYPos - sharpCornerYOffset),
                            Quaternion.Euler(new Vector3(0, 0, 180))) as GameObject;
                    break;
                }
            case BorderType.bottomRightSharpCorner:
                {
                    border = Instantiate(sharpCornerPref, new Vector2(unitXPos + sharpCornerXOffset, unitYPos - sharpCornerYOffset),
                            Quaternion.Euler(new Vector3(0, 0, 90))) as GameObject;
                    break;
                }
        }

        border.transform.SetParent(borderHolder);
    }

}
