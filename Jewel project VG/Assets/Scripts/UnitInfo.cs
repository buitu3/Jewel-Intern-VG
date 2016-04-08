using UnityEngine;
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

    //==============================================
    // Methods
    //==============================================
}
