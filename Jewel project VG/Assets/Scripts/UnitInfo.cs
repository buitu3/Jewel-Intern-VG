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

    public GameObject HorizontalLightningEff;
    public GameObject VerticalLightningEff;
    public GameObject ExplosiveSparkEff;

    public enum SpecialEff
    {
        noEff = 0,
        hLightning,
        vLightning,
        explode,
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
