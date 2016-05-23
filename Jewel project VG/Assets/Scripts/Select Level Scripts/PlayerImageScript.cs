using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PlayerImageScript : MonoBehaviour {

    public AnimationCurve floatingCurve;

	void Start ()
    {
        floating();
	}
	
    public void floating()
    {
        this.gameObject.transform.DOLocalMoveY(10f, 2.5f).SetRelative(true).SetLoops(-1).SetEase(floatingCurve);
    }

}
