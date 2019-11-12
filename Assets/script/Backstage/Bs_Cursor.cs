using UnityEngine;
using System.Collections;

public class Bs_Cursor : MonoBehaviour {
    public tk2dAnimatedSprite Ani_Cursor;
    public tk2dSprite Spr_optionFront;
    public Vector2 SprOptionFrontOrginalDimens;//选中框原来大小
    

    public void SetDimens(Vector3 dim)
    {
        Transform ts =Spr_optionFront.transform;
        //ts.localPosition = new Vector3(dim.x * 0.5F, 0F, 0F);
        ts.localScale = new Vector3(dim.x / SprOptionFrontOrginalDimens.x, dim.y / SprOptionFrontOrginalDimens.y, 0F);
    }


}
