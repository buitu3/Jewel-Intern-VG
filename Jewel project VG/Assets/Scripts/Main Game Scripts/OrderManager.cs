using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class OrderManager : MonoBehaviour {

    public static OrderManager Instance;

    public Text orderCountText;
    public Text[] itemCountText;
    public Image[] itemImage;

    public Sprite[] jewelImages;

    private int[][] activeOrder;
    private List<int[]> orderList;

    private int totalOrder;
    private int orderLeft;

    void Awake()
    {
        Instance = this;

        orderList = new List<int[]>();

        List<JSONObject> orderListJSON = LevelsManager.Instance.selectedLevelInfoJSON.GetField("Order").list;
        for(int i = 0; i < orderListJSON.Count; i ++)
        {
            int itemNumber = (int) orderListJSON[i].list[0].i;
            int itemCount = (int)orderListJSON[i].list[1].i;

            orderList.Add(new int[2] { itemNumber, itemCount });
        }

        totalOrder = orderList.Count;
        orderLeft = totalOrder;
        orderCountText.text = orderList.Count.ToString();
    }

	void Start ()
    {
        activeOrder = new int[3][];

        for (int i = 0; i < activeOrder.Length; i++)
        {
            if (orderList.Count > 0)
            {
                activeOrder[i] = new int[2] { orderList[0][0], orderList[0][1] };

                itemImage[i].sprite = jewelImages[activeOrder[i][0]];
                itemCountText[i].text = activeOrder[i][1].ToString();

                orderList.RemoveAt(0);

                orderLeft--;
                updateOrderCountText(orderLeft);
            }
            else
            {
                activeOrder[i] = new int[2] { -5, 1 };
            }
        }
	}
	
    private void updateOrderCountText(int orderCount)
    {
        orderCountText.text = orderCount.ToString();
    }

    private void updateItemNumberText(int itemIndex, int itemCount)
    {
        itemCountText[itemIndex].text = itemCount.ToString();
    }
}
