using UnityEngine;
using System.Collections;

public class DestroyEffController : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
	
	}

    public void selfDestroy()
    {
        Destroy(this.gameObject);
    }
}
