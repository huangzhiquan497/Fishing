using UnityEngine;
using System.Collections;

public class Bs_2CheckBill : MonoBehaviour {
    
    public CursorDimLocation CursorLocal;
    public tk2dTextMesh[] Text_ViewInfos;
    void OnEnable()
    {
        GameMain.EvtInputKey += Handle_InputKey;


        BackStageSetting bs = GameMain.Singleton.BSSetting;
        BackstageMain bsm = BackstageMain.Singleton;
 
        //设置显示的数据
        Text_ViewInfos[0].text = bs.His_GainTotal.Val.ToString() + " " + bsm.Unit_Coin.CurrentText;
        Text_ViewInfos[1].text = bs.His_GainPrevious.Val.ToString() + " " + bsm.Unit_Coin.CurrentText;
        Text_ViewInfos[2].text = bs.His_GainCurrent.Val.ToString() + " " + bsm.Unit_Coin.CurrentText;
        Text_ViewInfos[3].text = bs.His_CoinUp.Val.ToString() + " " + bsm.Unit_Coin.CurrentText;
        Text_ViewInfos[4].text = bs.His_CoinDown.Val.ToString() + " " + bsm.Unit_Coin.CurrentText;
        Text_ViewInfos[5].text = bs.His_CoinInsert.Val.ToString() + " " + bsm.Unit_Coin.CurrentText;
        Text_ViewInfos[6].text = bs.His_CoinOut.Val.ToString() + " " + bsm.Unit_Coin.CurrentText;
        Text_ViewInfos[7].text = bs.His_TicketOut.Val.ToString() + " " + bsm.Unit_Ticket.CurrentText;
        Text_ViewInfos[8].text = bs.Dat_IdLine.Val.ToString();
        Text_ViewInfos[9].text = bs.Dat_IdTable.Val.ToString();
        Text_ViewInfos[10].text = bs.GetRemainRuntime().ToString() + " " + bsm.Unit_Minute.CurrentText;
        Text_ViewInfos[11].text = bs.His_NumCodePrint.Val.ToString() + " " + bsm.Unit_Times.CurrentText;

        foreach (tk2dTextMesh t in Text_ViewInfos)
        {
            t.Commit();
        }

        BackstageMain.Singleton.UpdateCursor(CursorLocal); 
    }

    void OnDisable()
    {
        GameMain.EvtInputKey -= Handle_InputKey;

    }

    //播放声音(正版有)
    IEnumerator _Coro_PlaySound()
    {
        while (true)
        {
            //音效-后台
            if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);
            yield return new WaitForSeconds(Defines.TimeBackGroundJumpSelect);
        }
    }

    void Handle_InputKey(int control, HpyInputKey key, bool down)
    {
        if (down && (key == HpyInputKey.BS_Up || key == HpyInputKey.BS_Down))
        {
            StopCoroutine("_Coro_PlaySound");
            StartCoroutine("_Coro_PlaySound");
        }
        else if (!down && (key == HpyInputKey.BS_Up || key == HpyInputKey.BS_Down))
        {
            StopCoroutine("_Coro_PlaySound");
        }
        if (down && key == HpyInputKey.BS_Confirm)
        {
            //音效-后台
            if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);
            BackToMainMenu();
        }
    }


    /// <summary>
    /// 返回主菜单
    /// </summary>
    void BackToMainMenu()
    {
        gameObject.SetActiveRecursively(false);
        BackstageMain.Singleton.WndMainMenu.gameObject.SetActiveRecursively(true);
    }
}
