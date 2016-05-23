using UnityEngine;
using DG.Tweening;
using System.Collections;

public class UnitInfo : MonoBehaviour
{

    //==============================================
    // Constants
    //==============================================

    //==============================================
    // Fields
    //==============================================

    //[HideInInspector]
    public int _XIndex;
    //[HideInInspector]
    public int _YIndex;
    //[HideInInspector]
    public int _value;
    //[HideInInspector]
    public SpecialEff _unitEff;
    //[HideInInspector]
    public NegativeEff _negativeEff;

    public GameObject HorizontalLightningEff;
    public GameObject VerticalLightningEff;
    public GameObject ExplosiveSparkEff;

    public GameObject FrozenEff;
    public GameObject LockEff;

    public enum SpecialEff
    {
        noEff = 0,
        hLightning,
        vLightning,
        explode,
        _effsCount
    }

    public enum NegativeEff
    {
        noEff = 0,
        frozen,
        locked,
        hollow,
        _effsCount
    }

    //==============================================
    // Unity Methods
    //==============================================

    //void Start()
    //{
    //    transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0f), 3f, 0, 1f).SetLoops(-1).SetEase(Ease.Linear);
    //}

    //==============================================
    // Methods
    //==============================================

}
