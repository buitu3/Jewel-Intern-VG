using UnityEngine;
using System.Collections;

public class DestroyEffController : MonoBehaviour {

    //==============================================
    // Constants
    //==============================================

    //==============================================
    // Fields
    //==============================================

    //==============================================
    // Unity Methods
    //==============================================

    //==============================================
    // Methods
    //==============================================

    public void selfDestroy()
    {
        Destroy(this.gameObject);
    }

    public void selfDeactivate()
    {
        this.gameObject.SetActive(false);
    }
}
