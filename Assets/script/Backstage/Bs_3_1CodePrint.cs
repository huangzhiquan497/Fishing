//#define TEMP_ADD_FIREANDGAIN
using UnityEngine;
using System.Collections;

public class Bs_3_1CodePrint : MonoBehaviour {
 

    public tk2dTextMesh[] Text_ViewInfos;
    public Ctrl_InputNum Ctrl_CodeInputer
    {
        get
        {
            if (mCtrl_CodeInputer == null)
            {
                mCtrl_CodeInputer = Instantiate(Prefab_CtrlCodeInputer) as Ctrl_InputNum;
                mCtrl_CodeInputer.Text_Tile.text = LI_TileCtrlCodeInput.CurrentText;
                mCtrl_CodeInputer.Text_Tile.Commit();
                mCtrl_CodeInputer.transform.parent = BackstageMain.Singleton.transform;
                mCtrl_CodeInputer.transform.position = TsLocal_CtrlCodeInputer.position;
                mCtrl_CodeInputer.gameObject.SetActiveRecursively(false);
            }
            return mCtrl_CodeInputer;
        }
    }

    public Ctrl_InputNum Prefab_CtrlCodeInputer;
    private Ctrl_InputNum mCtrl_CodeInputer;
    public Transform TsLocal_CtrlCodeInputer;//������������λ

    public tk2dTextMesh Text_ResultMsg;
    public tk2dTextMesh Text_ResultInfo;
    public tk2dTextMesh Text_ResultCode;
    public tk2dTextMesh Text_ResultCountDown;

    public Renderer Rd_PrintCodeSuccessHint;
    public Renderer Rd_PrintCodeSuccessHintBG;


    public LanguageItem LI_CodePrintResultSucess;
    public LanguageItem LI_CodePrintResultWrong;
    public LanguageItem LI_TileCtrlCodeInput;//������������������
    //void OnEnable()
    public void Enter()
    {
        gameObject.SetActiveRecursively(true);
        //������ʾ������ 
        UpdateView();

        Text_ResultCode.GetComponent<Renderer>().enabled = false;
        Text_ResultMsg.GetComponent<Renderer>().enabled = false;
        Text_ResultInfo.GetComponent<Renderer>().enabled = false;
        Text_ResultCountDown.GetComponent<Renderer>().enabled = false;

        Rd_PrintCodeSuccessHint.enabled = false;
        Rd_PrintCodeSuccessHintBG.enabled = false;

        Ctrl_CodeInputer.gameObject.SetActiveRecursively(true);
        Ctrl_CodeInputer.EvtConfirm += Handle_InputConfirm;
        BackstageMain.Singleton.Cursor.gameObject.SetActiveRecursively(false);

        
        GameMain.Singleton.BSSetting.IsNeedPrintCodeAtGameStart.SetImmdiately(true); 
    }

    void OnDisable()
    {
        if (GameMain.IsEditorShutdown)
            return;

        Ctrl_CodeInputer.EvtConfirm -= Handle_InputConfirm;
        if (BackstageMain.Singleton.Cursor!=null)
            BackstageMain.Singleton.Cursor.gameObject.SetActiveRecursively(true);
    }

    void Handle_InputConfirm(int[] digits)
    {
        bool codePrintSuccess = true;
        //��������
        //�Ƿ���ȷ��
        uint codeInput = 0;
        for (int i = 0; i != 8; ++i )
            codeInput += (uint)(digits[7-i]*Mathf.Pow(10F, i)) ;

        BackStageSetting bsSetting = GameMain.Singleton.BSSetting;

        uint codeConfirm = HF_CodePrint.GenerateComfirmCode(bsSetting.His_GainTotal.Val, bsSetting.His_GainCurrent.Val
            ,bsSetting.Dat_IdLine.Val, (uint)bsSetting.Dat_IdTable.Val, (uint)bsSetting.His_NumCodePrint.Val
			, (uint)GameMain.Singleton.BSSetting.Dat_FormulaCode.Val, (uint)GameMain.GameIdx);
        uint gainIdx = 0;
        uint gainRatioMulti = 0;
        codePrintSuccess = HF_CodePrint.VerifyPrintCode(codeInput, bsSetting.His_GainTotal.Val
            , bsSetting.His_GainCurrent.Val
            , (uint)bsSetting.Dat_IdTable.Val
            , (uint)bsSetting.His_NumCodePrint.Val
            , codeConfirm
            , ref gainIdx
            , ref gainRatioMulti);

        //��������
        if (codePrintSuccess)
        {
            if (!bsSetting.CodePrintCurrentAction.Val)
                CurrentToTotalGain();//�鵽���� 
            else//���δ���,ǰ������,����ˮ
                ClearAllData();
            
            bsSetting.Dat_CodePrintDateTime.Val = System.DateTime.Now.Ticks;
            bsSetting.His_NumCodePrint.Val += 1;
            //bsSetting.Dat_GainAdjustIdx.SetImmdiately(((int)gainIdx));
          
            bsSetting.Dat_RemoteDiffucltFactor.SetImmdiately(gainRatioMulti);
        }


        StartCoroutine(_Coro_CodePrintResult(codePrintSuccess, digits));
    }

    /// <summary>
    /// ������������
    /// </summary>
    void ClearAllData()
    {
        if (GameMain.EvtBGClearAllData_Before != null)
            GameMain.EvtBGClearAllData_Before();


        BackStageSetting bsSetting = GameMain.Singleton.BSSetting;
        bsSetting.His_GainTotal.Val = 0;
        bsSetting.His_GainPrevious.Val = 0;
        bsSetting.His_GainCurrent.Val = 0;
        bsSetting.His_CoinUp.Val = 0;
        bsSetting.His_CoinDown.Val = 0;
        bsSetting.His_CoinInsert.Val = 0;
        bsSetting.His_CoinOut.Val = 0;
        bsSetting.His_TicketOut.Val = 0;

#if TEMP_ADD_FIREANDGAIN
        bsSetting.His_ScoreFire.Val = 0;
        bsSetting.His_ScoreGain.Val = 0;
#endif

        for (int i = 0; i != Defines.MaxNumPlayer; ++i)
        {
            bsSetting.Dat_PlayersScore[i].Val = 0;


            bsSetting.Dat_PlayersGunScore[i].Val = bsSetting.GetScoreMin();
            bsSetting.Dat_PlayersScoreWon[i].Val = 0;
        }
        bsSetting.TicketOutFragment.Val = 0;
    }

    /// <summary>
    /// ������ӯ��������ӯ����
    /// </summary>
    void CurrentToTotalGain()
    {
        BackStageSetting bsSetting = GameMain.Singleton.BSSetting;
        bsSetting.His_GainPrevious.Val += bsSetting.His_GainCurrent.Val;
        bsSetting.His_GainCurrent.Val = 0;
        bsSetting.His_GainTotal.Val = bsSetting.His_GainPrevious.Val;
        //for (int i = 0; i != Defines.MaxNumPlayer; ++i)
        //{
        //    bsSetting.Dat_PlayersScore[i].Val = 0;
        //    bsSetting.Dat_PlayersGunScore[i].Val = bsSetting.GetScoreMin();
        //}
 
        //bsSetting.TicketOutFragment.Val = 0;
    }
    //void Handle_InputKey(int control, HpyInputKey key, bool down)
    //{
    //    if (down && key == HpyInputKey.BS_Confirm)
    //    {
    //        BackToMainMenu();
    //    }
    //}
    /// <summary>
    /// �Ƿ��������ݶ���ʼ����
    /// </summary>
    /// <returns></returns>
    bool IsAllDataInited()
    { 
        BackStageSetting bsSetting = GameMain.Singleton.BSSetting;
        if (bsSetting.His_GainTotal.Val != 0)
            return false;
        if (bsSetting.His_GainPrevious.Val != 0)
            return false;
        if (bsSetting.His_GainCurrent.Val != 0)
            return false;
        if (bsSetting.His_CoinUp.Val != 0)
            return false;
        if (bsSetting.His_CoinDown.Val != 0)
            return false;
        if (bsSetting.His_CoinInsert.Val != 0)
            return false;
        if (bsSetting.His_CoinOut.Val != 0)
            return false;
        if (bsSetting.His_TicketOut.Val != 0)
            return false;
 
        for (int i = 0; i != Defines.MaxNumPlayer; ++i)
        {
            if(bsSetting.Dat_PlayersScore[i].Val != 0
            || bsSetting.Dat_PlayersGunScore[i].Val != bsSetting.GetScoreMin() 
            || bsSetting.Dat_PlayersScoreWon[i].Val != 0)
            { 
                return false;
            }
            
        }
        return true;
    }

    IEnumerator _Coro_CodePrintResult(bool success, int[] digits)
    {
        //���ش�����
        Ctrl_CodeInputer.gameObject.SetActiveRecursively(false);
        string digitStr = "";
        foreach(int d in digits)
            digitStr += d;

        //��ʾ��������
        Text_ResultCode.GetComponent<Renderer>().enabled = true;
        Text_ResultCode.text = digitStr;
        Text_ResultCode.Commit();

        Text_ResultInfo.GetComponent<Renderer>().enabled = true;

        yield return new WaitForSeconds(0.5F);

        //��ʾ��Ϣ
        if (success && GameMain.Singleton.BSSetting.IsViewCodebeatSuccess.Val)
        {
            Rd_PrintCodeSuccessHint.enabled = true;
            Rd_PrintCodeSuccessHintBG.enabled = true;
        }

        //����
        Text_ResultMsg.GetComponent<Renderer>().enabled = true;
        Text_ResultMsg.text = success ? LI_CodePrintResultSucess.CurrentText : LI_CodePrintResultWrong.CurrentText;
        Text_ResultMsg.Commit();

        
        //����ʱ��
        Text_ResultCountDown.GetComponent<Renderer>().enabled = true;
        int countDownTime = 9;
        while (countDownTime >= 0)
        {
            
            Text_ResultCountDown.text = countDownTime.ToString();
            Text_ResultCountDown.Commit();

            --countDownTime;
            yield return new WaitForSeconds(1F);
        }

        if (success)
        {
            GameMain.Singleton.BSSetting.CodePrintCurrentAction.SetImmdiately(true);
            if (GameMain.Singleton.BSSetting.IsCodePrintClearAllData.Val)//Ҫ���ٴδ���
            {
                if (IsAllDataInited())//�����Ѿ���ʼ����ֱ�ӷ������˵�
                {
                    BackToMainMenu();
                }
                else
                {
                    
                    PrintCodeAgain();
                }
            }
            else
            {
                BackToMainMenu();
            }
        }
        else
        {
            PrintCodeAgain();
        }
 
    }

    /// <summary>
    /// �������˵�
    /// </summary>
    void BackToMainMenu()
    {
        
        GameMain.Singleton.BSSetting.IsNeedPrintCodeAtGameStart.SetImmdiately(false);

        gameObject.SetActiveRecursively(false);
        Bs_Mainmenu mainMenu = BackstageMain.Singleton.WndMainMenu;
       
        mainMenu.gameObject.SetActiveRecursively(true);
        mainMenu.CurrentCursorIdx = mainMenu.CursorLocals.Length-1;
    }

    void PrintCodeAgain()
    {
        Text_ResultCode.GetComponent<Renderer>().enabled = false;
        Text_ResultMsg.GetComponent<Renderer>().enabled = false;
        Text_ResultInfo.GetComponent<Renderer>().enabled = false;
        Text_ResultCountDown.GetComponent<Renderer>().enabled = false;
        Ctrl_CodeInputer.gameObject.SetActiveRecursively(true);
        UpdateView();
    }

    void UpdateView()
    {
        BackStageSetting bs = GameMain.Singleton.BSSetting;
        BackstageMain bsm = BackstageMain.Singleton;
        //������ʾ������
        Text_ViewInfos[0].text = bs.His_GainTotal.Val.ToString() +" "+ bsm.Unit_Coin.CurrentText;
        Text_ViewInfos[1].text = bs.His_GainCurrent.Val.ToString() + " " + bsm.Unit_Coin.CurrentText; 
        Text_ViewInfos[2].text = bs.Dat_IdTable.Val.ToString();
        Text_ViewInfos[3].text = bs.His_NumCodePrint.Val.ToString() + " " + bsm.Unit_Times.CurrentText;
        Text_ViewInfos[4].text = string.Format("{0:d4}",HF_CodePrint.GenerateComfirmCode(bs.His_GainTotal.Val,bs.His_GainCurrent.Val
            , bs.Dat_IdLine.Val, (uint)bs.Dat_IdTable.Val, (uint)bs.His_NumCodePrint.Val, (uint)GameMain.Singleton.BSSetting.Dat_FormulaCode.Val, (uint)GameMain.GameIdx));

        foreach (tk2dTextMesh t in Text_ViewInfos)
            t.Commit();
    }
}
