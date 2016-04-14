using UnityEngine;
using System.Collections;

public class PointTextController : MonoBehaviour {

    public float speed;
    private float lifeTime = 0.7f;

    private Transform pointTransform;

    private Vector3 nextPos;

    // Use this for initialization
    void Start ()
    {
        pointTransform = GetComponent<Transform>();

        //nextPos = new Vector3(0f, speed, 0f);

        //StartCoroutine(selfDeactivate(lifeTime));
	}

    void FixedUpdate()
    {
        pointTransform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    IEnumerator selfDeactivate(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        //Destroy(this.gameObject);
        this.gameObject.SetActive(false);
    }
}
