using UnityEngine;
using System.Collections;

public class PlayerCreator : MonoBehaviour {
    public int PlayerId;
    public int PControllerId;//������ID
    public string SprNameGunBottom = "wpBottom0";//��̨�׶�������
    public string SprNameScoreBG;//�����Ƶײ���������
    public Vector3 CoinStacksLocalPos = new Vector3(-0.2293366F, -0.1180472F, -0.01F);
    public Vector3 ScoreBGLocalPos = new Vector3(0.4592035F, -0.07126641F, 0F);

    public ColorSet Prefab_CreditBoardColors;
    public StringSet Prefab_WpDecoSpriteNames;
	public Player CreatePlayer()
    {
        Player p = Instantiate(GameMain.Singleton.Prefab_Player,transform.position,transform.rotation) as Player;
        p.Idx = PlayerId;
        p.CtrllerIdx = PControllerId;
        p.name = "Player" + PlayerId;
        //p.Spr_GunBottom.spriteId = p.Spr_GunBottom.GetSpriteIdByName(SprNameGunBottom);
        Transform tsWpDeco = p.transform.Find("Ani_WeaponBottomDeco");
        if (tsWpDeco != null)
        {
            tk2dSprite sprWpDeco = tsWpDeco.GetComponent<tk2dSprite>();
            sprWpDeco.spriteId = p.Spr_GunBottom.GetSpriteIdByName(Prefab_WpDecoSpriteNames.Texts[PlayerId % Prefab_WpDecoSpriteNames.Texts.Length]);
        }


        Transform tsUICreditboard = p.transform.Find("UI_CreditBoard");
        //p.Spr_ScoreBG.spriteId = p.Spr_ScoreBG.GetSpriteIdByName(SprNameScoreBG);
        if (tsUICreditboard != null)
        {
            tsUICreditboard.localPosition = ScoreBGLocalPos;
        }

        Transform tsUICreditboardBG = p.transform.Find("UI_CreditBoard/creditBoardBG");
        //tk2dSlicedSprite sliSprCreaditBoardBG = tsUICreditboardBG.GetComponent<tk2dSlicedSprite>();
        //if (sliSprCreaditBoardBG != null)
          //  sliSprCreaditBoardBG.color = Prefab_CreditBoardColors.Colors[PlayerId % Prefab_CreditBoardColors.Colors.Length];



        p.Ef_CoinStack.transform.localPosition = CoinStacksLocalPos;
        return p;   
    }

}
