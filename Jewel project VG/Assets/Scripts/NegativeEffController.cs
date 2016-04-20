using UnityEngine;
using System.Collections;

public class NegativeEffController : MonoBehaviour
{

    //==============================================
    // Constants
    //==============================================

    //==============================================
    // Fields
    //==============================================

    private Animator anim;

    //==============================================
    // Unity Methods
    //==============================================

    void Start()
    {
        anim = GetComponent<Animator>();
    }

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

    public void selfBreak()
    {
        anim.SetTrigger("Break");
    }
}
