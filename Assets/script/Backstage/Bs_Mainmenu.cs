using UnityEngine;
using System.Collections;

public class Bs_Mainmenu : MonoBehaviour {
    public CursorDimLocation[] CursorLocals;
    public LanguageItem[] OptionInfosStr;
    public tk2dTextMesh Text_OptionInfo;

    public int CurrentCursorIdx
    {
        set
        {
            if (value >= 0 && value < CursorLocals.Length)
            {
                mCurCursorIdx = value;
                mBsMain.UpdateCursor(CursorLocals[mCurCursorIdx]);
 

            }
        }
    }
    private int mCurCursorIdx;
    private bool mControlable = true;//是否可控制
    private BackstageMain mBsMain;
    
    void Awake()
    {
        mBsMain = BackstageMain.Singleton;
    }
    void OnEnable()
    {
        //BackStageSetting bs = GameMain.Singleton.BSSetting;
 
        //bs.His_GainTotal.Val = 323;
        //bs.His_GainPrevious.Val = 200;
        //bs.His_GainCurrent.Val = 123;
        
        GameMain.EvtInputKey += Handle_InputKey;
        //if (CursorLocals != null && CursorLocals.Length != 0)
            //mBsMain.UpdateCursor(CursorLocals[mCurCursorIdx]);

        mControlable = true;

        
        mBsMain.Ctrl_SystemSettingPswInput.EvtConfirm += Handle_SystemSettingPswInputterConfirm;
        mBsMain.Ctrl_SystemSettingPswInput.EvtDisable += Handle_SystemSettingPswInputterDisable;
        mBsMain.Ctrl_CodePrintConfirm.EvtResult += Handle_CodePrintConfirmResult;
        mBsMain.Ctrl_CodePrintConfirm.EvtDisable += Handle_CodePrintConfirmDisable;
 
        Text_OptionInfo.text = OptionInfosStr[mCurCursorIdx].CurrentText;
        Text_OptionInfo.Commit();
        StartCoroutine(_Coro_UpdateCursor());
    }
    IEnumerator _Coro_UpdateCursor()
    {
        yield return 0;
        mBsMain.UpdateCursor(CursorLocals[mCurCursorIdx]);
    }
    void OnDisable()
    {
        GameMain.EvtInputKey -= Handle_InputKey;
        mBsMain.Ctrl_SystemSettingPswInput.EvtConfirm -= Handle_SystemSettingPswInputterConfirm;
        mBsMain.Ctrl_SystemSettingPswInput.EvtDisable -= Handle_SystemSettingPswInputterDisable;
        mBsMain.Ctrl_CodePrintConfirm.EvtResult -= Handle_CodePrintConfirmResult;
        mBsMain.Ctrl_CodePrintConfirm.EvtDisable -= Handle_CodePrintConfirmDisable;
    }

    void Handle_CodePrintConfirmResult(int idx)
    {
        mControlable = true;
        if (idx == 0)//确定
        {
            //进入打码
            gameObject.SetActiveRecursively(false);
            mBsMain.Ctrl_CodePrintConfirm.gameObject.SetActiveRecursively(false);

            if (GameMain.Singleton.BSSetting.His_GainCurrent.Val != 0)
            {
                GameMain.Singleton.BSSetting.CodePrintCurrentAction.SetImmdiately(false);//归0状态
                GameMain.Singleton.BSSetting.IsCodePrintClearAllData.SetImmdiately(false);
                //mBsMain.WndCodePrint.gameObject.SetActiveRecursively(true);
                mBsMain.WndCodePrint.Enter(); 
            }
            else
                mBsMain.WndCodePrintSelect.gameObject.SetActiveRecursively(true);
        }
        else if (idx == 1)//取消
        {
            mBsMain.Ctrl_CodePrintConfirm.gameObject.SetActiveRecursively(false);
            mBsMain.UpdateCursor(CursorLocals[mCurCursorIdx]);
        }
    }

    void Handle_CodePrintConfirmDisable()
    {
        mControlable = true;
        mBsMain.UpdateCursor(CursorLocals[mCurCursorIdx]);
    }

    /// <summary>
    /// 系统设置 密码输入框返回
    /// </summary>
    /// <param name="digits"></param>
    void Handle_SystemSettingPswInputterConfirm(int[] digits)
    {
        //mControlable = true;
        //mBsMain.Ctrl_SystemSettingPswInput.gameObject.SetActiveRecursively(false);
        if (mBsMain.WndSystemSetting.TryEnter(digits))
        {
            gameObject.SetActiveRecursively(false);
            mBsMain.Ctrl_SystemSettingPswInput.gameObject.SetActiveRecursively(false);
            //mBsMain.WndSystemSetting.gameObject.SetActiveRecursively(true);/
        }
    }

    /// <summary>
    /// 系统设置 密码输入框关闭
    /// </summary>
    /// <param name="digits"></param>
    void Handle_SystemSettingPswInputterDisable()
    {
        mControlable = true;
    }
    void PrevSelection()
    {
        --mCurCursorIdx;
        if (mCurCursorIdx < 0)
            mCurCursorIdx = CursorLocals.Length - 1;

        mBsMain.UpdateCursor(CursorLocals[mCurCursorIdx]);

        Text_OptionInfo.text = OptionInfosStr[mCurCursorIdx].CurrentText;
        Text_OptionInfo.Commit();

        //音效-后台
        if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
            GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);
    }
    void NextSelection()
    {
        mCurCursorIdx = (mCurCursorIdx + 1) % CursorLocals.Length;
        mBsMain.UpdateCursor(CursorLocals[mCurCursorIdx]);

        Text_OptionInfo.text = OptionInfosStr[mCurCursorIdx].CurrentText;
        Text_OptionInfo.Commit();

        //音效-后台
        if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
            GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);
    }

    IEnumerator _Coro_PrevSelection()
    {
        while (true)
        {
            PrevSelection();
            yield return new WaitForSeconds(Defines.TimeBackGroundJumpSelect);
        }
    }

    IEnumerator _Coro_NextSelection()
    {
        while (true)
        {
            NextSelection();
            yield return new WaitForSeconds(Defines.TimeBackGroundJumpSelect);
        }
    }


    void Handle_InputKey(int control, HpyInputKey key,bool down)
    {
        if (!mControlable)
            return;
 
        if(down && key == HpyInputKey.BS_Up)
        {
            PrevSelection();
            //StartCoroutine("_Coro_PrevSelection");
        }
        else if (down && key == HpyInputKey.BS_Down)
        {
            NextSelection();
            //StartCoroutine("_Coro_NextSelection");
            
        }
        //else if (!down && key == HpyInputKey.BS_Up)
        //{
        //    StopCoroutine("_Coro_PrevSelection");
        //}
        //else if (!down && key == HpyInputKey.BS_Down)
        //{
        //    StopCoroutine("_Coro_NextSelection");
        //}
        else if (down && key == HpyInputKey.BS_Confirm)
        {
            //音效-后台
            if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);

            if (mCurCursorIdx == 0)//参数设置
            {
                mBsMain.WndMainMenu.gameObject.SetActiveRecursively(false);
                mBsMain.WndParamSetting.SetActiveRecursively(true);
            }
            else if (mCurCursorIdx == 1)//解码调整
            {
                mBsMain.WndMainMenu.gameObject.SetActiveRecursively(false);
                mBsMain.WndDecodeSetting.gameObject.SetActiveRecursively(true);
            }
            else if (mCurCursorIdx == 2)//查账
            {
                mBsMain.WndMainMenu.gameObject.SetActiveRecursively(false);
                mBsMain.WndCheckBill.gameObject.SetActiveRecursively(true);
            }
            else if (mCurCursorIdx == 3)//报账
            {
                mBsMain.Ctrl_CodePrintConfirm.SelectIdx = 1;//出现在否处
                mBsMain.Ctrl_CodePrintConfirm.gameObject.SetActiveRecursively(true);//确认框
                
                mControlable = false;
            }
            else if (mCurCursorIdx == 4)//系统设置
            {
                mBsMain.Ctrl_SystemSettingPswInput.gameObject.SetActiveRecursively(true);
                mControlable = false;
            }
            else if (mCurCursorIdx == 5)//退出
            {
                mBsMain.WndMainMenu.gameObject.SetActiveRecursively(false);
                mBsMain.WndReboot.gameObject.SetActiveRecursively(true);
                mBsMain.Cursor.gameObject.SetActiveRecursively(false);
            }
        }



    }
 
}
