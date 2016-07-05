using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class OrderManager : MonoBehaviour {

    public static OrderManager Instance;

    public Text orderCountText;
    public Text firstItemCountText;
    public Text secondItemCountText;
    public Text thirdItemCountText;

    public Sprite[] jewelImages;

    void Awake()
    {
        Instance = this;

        List<JSONObject> orderListJSON = LevelsManager.Instance.selectedLevelInfoJSON.GetField("Order").list;

        orderCountText.text = orderListJSON.Count.ToString();
    }

	void Start ()
    {
	
	}
	

}
