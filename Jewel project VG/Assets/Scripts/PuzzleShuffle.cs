using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class PuzzleShuffle : MonoBehaviour {

    //==============================================
    // Constants
    //==============================================

    public static PuzzleShuffle Instance;

    //==============================================
    // Fields
    //==============================================

    public Transform shufflePos;

    private List<GameObject> shuffleUnitsList;
    private List<GameObject> unitsList;
    private List<int> unitsXIndex;
    private List<int> unitsYIndex;

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        unitsList = new List<GameObject>();
        shuffleUnitsList = new List<GameObject>();
        unitsXIndex = new List<int>();
        unitsYIndex = new List<int>();
    }

    //==============================================
    // Methods
    //==============================================

    public void onBtnShuffleClick()
    {
        StartCoroutine(shufflePuzzle());
    }

    public IEnumerator shufflePuzzle()
    {
        GameController.Instance.currentState = GameController.GameState.shufflePuzzle;
        yield return new WaitForSeconds(0.5f);

        unitsList.Clear();
        shuffleUnitsList.Clear();
        unitsXIndex.Clear();
        unitsYIndex.Clear();

        for (int YIndex = 0; YIndex < PuzzleGenerator.Instance._rows; YIndex++)
        {
            for (int XIndex = 0; XIndex < PuzzleGenerator.Instance._columns; XIndex++)
            {
                if (!ChainedUnitsScanner.Instance._scanUnitARR[XIndex, YIndex]._isChained)
                {
                    if (!unitsList.Contains(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]))
                    {
                        unitsList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                    }

                    if (PuzzleGenerator.Instance._unitARR[XIndex, YIndex].GetComponent<UnitInfo>()._negativeEff == UnitInfo.NegativeEff.noEff)
                    {
                        if(!shuffleUnitsList.Contains(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]))
                        {
                            shuffleUnitsList.Add(PuzzleGenerator.Instance._unitARR[XIndex, YIndex]);
                        }

                        unitsXIndex.Add(XIndex);
                        unitsYIndex.Add(YIndex);
                    }
                }
            }
        }

        for(int i = 0; i < shuffleUnitsList.Count; i++)
        {
            shuffleUnitsList[i].transform.DOMove(shufflePos.position, 1.5f);
        }

        yield return new WaitForSeconds(2f);

        for (int i = 0; i < unitsXIndex.Count; i++)
        {
            GameObject randomUnit = shuffleUnitsList[Random.Range(0, shuffleUnitsList.Count)];

            randomUnit.GetComponent<UnitInfo>()._XIndex = unitsXIndex[i];
            randomUnit.GetComponent<UnitInfo>()._YIndex = unitsYIndex[i];

            PuzzleGenerator.Instance._unitARR[unitsXIndex[i], unitsYIndex[i]] = randomUnit;

            shuffleUnitsList.Remove(randomUnit);

            randomUnit.transform.DOMove(PuzzleGenerator.Instance._unitPosARR[unitsXIndex[i], unitsYIndex[i]], 1.5f);
        }

        yield return new WaitForSeconds(2f);

        //GameController.Instance.currentState = GameController.GameState.idle;

        StartCoroutine(ChainedUnitsScanner.Instance.scanRegenUnits(unitsList));
    }
}
