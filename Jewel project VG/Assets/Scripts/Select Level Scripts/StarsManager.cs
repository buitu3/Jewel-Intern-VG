using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StarsManager : MonoBehaviour {

    //==============================================
    // Constants
    //==============================================

    //==============================================
    // Fields
    //==============================================

    public Image starOneImg;
    public Image starTwoImg;
    public Image starThreeImg;

    public Sprite darkStarSprite;
    public Sprite lightStarSprite;

    //==============================================
    // Unity Methods
    //==============================================

    //==============================================
    // Methods
    //==============================================

    public void updateStar(int star)
    {
        switch (star)
        {
            case 3:
                {
                    starOneImg.sprite = lightStarSprite;
                    starTwoImg.sprite = lightStarSprite;
                    starThreeImg.sprite = lightStarSprite;
                    break;
                }
            case 2:
                {
                    starOneImg.sprite = lightStarSprite;
                    starTwoImg.sprite = lightStarSprite;
                    break;
                }
            case 1:
                {
                    starOneImg.sprite = lightStarSprite;
                    break;
                }
        }
    }
}
