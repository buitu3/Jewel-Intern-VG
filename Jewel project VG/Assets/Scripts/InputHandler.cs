using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;

public class InputHandler : MonoBehaviour, IPointerDownHandler,IDragHandler
{
    //==============================================
    // Constants
    //==============================================

    public static InputHandler Instance;

    //==============================================
    // Fields
    //==============================================

    public GameObject unitHighlight;

    private GameObject focusedUnit;
    private LayerMask unitLayer;

    public float swapTime = 0.3f;

    public enum SwapType
    {
        up = 0,
        down,
        left,
        right,
        same,
        none
    }

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        unitLayer = LayerMask.GetMask("Unit");
    }

    public void OnPointerDown(PointerEventData data)
    {
        // Cast a ray from camera into pointer position
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D unitHit;
        unitHit = Physics2D.Raycast(camRay.origin, camRay.direction, 20f, unitLayer);

        // If ray hit on an Unit
        if (unitHit.collider != null)
        {
            // If there is none Unit focused
            if (GameController.Instance.currentState == GameController.GameState.idle)
            {
                if (unitHit.collider.gameObject.GetComponent<UnitInfo>()._negativeEff != UnitInfo.NegativeEff.frozen)
                {
                    // Focus the hitted Unit
                    GameController.Instance.currentState = GameController.GameState.focusUnit;
                    focusedUnit = unitHit.transform.gameObject;

                    unitHighlight.SetActive(true);
                    unitHighlight.transform.position = unitHit.transform.position;
                }
            }
            // If there is focused Unit
            else if (GameController.Instance.currentState == GameController.GameState.focusUnit)
            {
                SwapType swapType = checkSwapable(focusedUnit, unitHit.transform.gameObject);
                if (swapType == SwapType.same)
                {
                    return;
                }
                else if (swapType == SwapType.none)
                {
                    focusedUnit = null;
                    unitHighlight.SetActive(false);
                    GameController.Instance.currentState = GameController.GameState.idle;
                }
                else
                {
                    swapUnits(focusedUnit,
                            unitHit.transform.gameObject,
                            swapType);

                    GameController.Instance.currentState = GameController.GameState.scanningUnit;
                    StartCoroutine(ChainedUnitsScanner.Instance.scanUnitsAfterSwap(focusedUnit, unitHit.transform.gameObject));
                }
            }

            // Get the Unit if camray hit
            GameObject _unit = unitHit.collider.gameObject;
            //print(_unit.GetComponent<UnitInfo>()._XIndex + " : "
            //    + _unit.GetComponent<UnitInfo>()._YIndex + " : "
            //    + _unit.GetComponent<UnitInfo>()._value);

            //print(PuzzleGenerator.Instance._unitARR[_unit.GetComponent<UnitInfo>()._XIndex, +_unit.GetComponent<UnitInfo>()._YIndex].GetComponent<UnitInfo>()._XIndex + " : "
            //    + PuzzleGenerator.Instance._unitARR[_unit.GetComponent<UnitInfo>()._XIndex, +_unit.GetComponent<UnitInfo>()._YIndex].GetComponent<UnitInfo>()._YIndex + " : "
            //    + PuzzleGenerator.Instance._unitARR[_unit.GetComponent<UnitInfo>()._XIndex, +_unit.GetComponent<UnitInfo>()._YIndex].GetComponent<UnitInfo>()._value);

            print(ChainedUnitsScanner.Instance._scanUnitARR[_unit.GetComponent<UnitInfo>()._XIndex, +_unit.GetComponent<UnitInfo>()._YIndex]._isChained);

            //print(PuzzleGenerator.Instance._valueARR[_unit.GetComponent<UnitInfo>()._XIndex, +_unit.GetComponent<UnitInfo>()._YIndex]);

            //ChainedUnitsScanner.Instance.destroyAllLocalUnits(_unit.GetComponent<UnitInfo>()._XIndex, _unit.GetComponent<UnitInfo>()._YIndex);
        }
    }

    public void OnDrag(PointerEventData data)
    {
        if (GameController.Instance.currentState == GameController.GameState.focusUnit)
        {
            // Cast a ray from camera into pointer position
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D unitHit;
            unitHit = Physics2D.Raycast(camRay.origin, camRay.direction, 20f, unitLayer);

            // If ray hit on an Unit
            if (unitHit.collider != null)
            {
                SwapType swapType = checkSwapable(focusedUnit, unitHit.transform.gameObject);
                if (swapType == SwapType.same)
                {
                    return;
                }
                else if (swapType == SwapType.none)
                {
                    focusedUnit = null;
                    unitHighlight.SetActive(false);
                    GameController.Instance.currentState = GameController.GameState.idle;
                }
                else
                {
                    swapUnits(focusedUnit,
                            unitHit.transform.gameObject,
                            swapType);

                    GameController.Instance.currentState = GameController.GameState.scanningUnit;
                    StartCoroutine(ChainedUnitsScanner.Instance.scanUnitsAfterSwap(focusedUnit, unitHit.transform.gameObject));
                }
            }
        }
    }


    //==============================================
    // Methods
    //==============================================

    public SwapType checkSwapable(GameObject currentUnit, GameObject nextUnit)
    {
        UnitInfo currentUnitInfo = currentUnit.GetComponent<UnitInfo>();
        UnitInfo nextUnitInfo = nextUnit.GetComponent<UnitInfo>();
        int deltaX = nextUnitInfo._XIndex - currentUnitInfo._XIndex;
        int deltaY = nextUnitInfo._YIndex - currentUnitInfo._YIndex;
        //print(deltaX + ":::" + deltaY);

        if (nextUnitInfo._negativeEff == UnitInfo.NegativeEff.frozen)
        {
            return SwapType.none;
        }
        else if (deltaX == 0 && deltaY == 1)
        {
            //print("swap up");
            return SwapType.up;
        }
        else if (deltaX == 0 && deltaY == -1)
        {
            //print("swap down");
            return SwapType.down;
        }
        else if (deltaX == 1 && deltaY == 0)
        {
            //print("swap right");
            return SwapType.right;
        }
        else if (deltaX == -1 && deltaY == 0)
        {
            //print("swap left");
            return SwapType.left;
        }
        else if (deltaX == 0 && deltaY == 0)
        {
            //print("same unit");
            return SwapType.same;
        }
        else
        {
            //print("loose focus");
            return SwapType.none;
        }
    }

    public void swapUnits(GameObject Unit, GameObject otherUnit, SwapType swapType)
    {
        // Swap two Units position
        Vector2 tempPos = Unit.transform.position;
        Unit.transform.DOMove(otherUnit.transform.position, swapTime).SetEase(Ease.InQuad);
        otherUnit.transform.DOMove(tempPos, swapTime).SetEase(Ease.InQuad);

        // Unfocus focusedUnit and change state
        unitHighlight.SetActive(false);
        //focusedUnit = null;

        // Update swapped Units Info,unitARR,scanUnitARR and valueARR
        UnitInfo unitInfo = Unit.GetComponent<UnitInfo>();
        UnitInfo otherUnitInfo = otherUnit.GetComponent<UnitInfo>();

        PuzzleGenerator.Instance._valueARR[unitInfo._XIndex, unitInfo._YIndex] = otherUnitInfo._value;
        PuzzleGenerator.Instance._valueARR[otherUnitInfo._XIndex, otherUnitInfo._YIndex] = unitInfo._value;

        ChainedUnitsScanner.Instance._scanUnitARR[unitInfo._XIndex, unitInfo._YIndex]._value = otherUnitInfo._value;
        ChainedUnitsScanner.Instance._scanUnitARR[otherUnitInfo._XIndex, otherUnitInfo._YIndex]._value = unitInfo._value;

        PuzzleGenerator.Instance._unitARR[unitInfo._XIndex, unitInfo._YIndex] = otherUnit;
        PuzzleGenerator.Instance._unitARR[otherUnitInfo._XIndex, otherUnitInfo._YIndex] = Unit;

        int tempXIndex = unitInfo._XIndex;
        int tempYIndex = unitInfo._YIndex;
        //UnitInfo.SpecialEff tempUnitEff = unitInfo._unitEff;
        unitInfo._XIndex = otherUnitInfo._XIndex;
        unitInfo._YIndex = otherUnitInfo._YIndex;
        //unitInfo._unitEff = otherUnitInfo._unitEff;
        otherUnitInfo._XIndex = tempXIndex;
        otherUnitInfo._YIndex = tempYIndex;
        //otherUnitInfo._unitEff = tempUnitEff;
    }
}
