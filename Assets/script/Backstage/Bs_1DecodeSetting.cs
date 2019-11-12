using UnityEngine;
using System.Collections;

public class Bs_1DecodeSetting : MonoBehaviour {
    public CursorDimLocation[] CursorLocals;

    public tk2dTextMesh Text_InsertCoinRatio;
    public tk2dTextMesh Text_ArenaType;
    public tk2dTextMesh Text_CodePrintDay;
    public tk2dTextMesh Text_SystemTime;
    public tk2dTextMesh Text_IsViewCodeBeatSuccess;

    public Bs_BoxDecoder Box_Decoder
    {
        get
        {
            if (mBoxDecoder == null)
            {
                mBoxDecoder = Instantiate(Prefab_BoxDecoder) as Bs_BoxDecoder;
                mBoxDecoder.transform.parent = BackstageMain.Singleton.transform;
                mBoxDecoder.transform.position = TsLocal_BoxDecoder.position;
                mBoxDecoder.gameObject.SetActiveRecursively(false);
            }
            return mBoxDecoder;
        }

    }
    public Bs_BoxDecoder Prefab_BoxDecoder;
    public Transform TsLocal_BoxDecoder;
    private Bs_BoxDecoder mBoxDecoder;


    public tk2dTextMesh Text_DeCodeSuccessInfo;
    public Renderer Rd_DecodeSuccessBG;

    public LanguageItem[] ArenaTexts;//��������
    public LanguageItem[] DecodeResultHintTexts;//����������ʾ��Ϣ
    public LanguageItem[] ViewCodeBeatSuccessTexts;//�����ɹ������Ƿ���ʾ
    private int mCurCursorIdx;
    private uint mCurTagId;//��ǰʹ�õĽ���������
    private bool mIsCursorMovable = true;

    private string[] mArenaStrs;
    void OnEnable()
    {
        //mArenaStrs = new string[] { "С�ͳ���ר�ó���", "���ͳ���ר�ó���", "���ͳ���ר�ó���" };
        GameMain.EvtInputKey += Handle_InputKey;
        Box_Decoder.Ctrl_DecodeNum.Num = 12;//12λ��
        Box_Decoder.Ctrl_DecodeNum.EvtConfirm += Handle_DecodeNumInputConfirm;

        if (CursorLocals != null && CursorLocals.Length != 0)
            BackstageMain.Singleton.UpdateCursor(CursorLocals[mCurCursorIdx]);
        BackStageSetting bs = GameMain.Singleton.BSSetting;
        //��ȡ��Ϣ
        Text_InsertCoinRatio.text = string.Format("1 {0:s} {1:d} {2:s}",BackstageMain.Singleton.Unit_Coin.CurrentText,bs.InsertCoinScoreRatio.Val,BackstageMain.Singleton.Unit_Score.CurrentText);//"1  " + bs.InsertCoinScoreRatio.Val.ToString();
        Text_InsertCoinRatio.Commit();

        Text_ArenaType.text = ArenaTexts[(int)bs.ArenaType_.Val].CurrentText;
        Text_ArenaType.Commit();

        Text_CodePrintDay.text = bs.CodePrintDay.Val.ToString() +"  "+ BackstageMain.Singleton.Unit_Day.CurrentText;
        Text_CodePrintDay.Commit();

        System.DateTime now = System.DateTime.Now; //2009-01-01 00:00:00
        Text_SystemTime.text = string.Format("{0:d}-{1:d2}-{2:d2} {3:d2}:{4:d2}:{5:d2}"
            , now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        Text_SystemTime.Commit();

        Text_IsViewCodeBeatSuccess.text = bs.IsViewCodebeatSuccess.Val ? ViewCodeBeatSuccessTexts[1].CurrentText:ViewCodeBeatSuccessTexts[0].CurrentText;
        Text_IsViewCodeBeatSuccess.Commit();


        Text_DeCodeSuccessInfo.GetComponent<Renderer>().enabled = false;
        Rd_DecodeSuccessBG.enabled = false;

        StartCoroutine("_Coro_TimeUpdateView");
    }

    void OnDisable()
    {
        StopCoroutine("_Coro_TimeUpdateView");
        GameMain.EvtInputKey -= Handle_InputKey;
        Box_Decoder.Ctrl_DecodeNum.EvtConfirm -= Handle_DecodeNumInputConfirm;
        
    }

    void Start()
    {
        if (CursorLocals != null && CursorLocals.Length != 0)
            BackstageMain.Singleton.UpdateCursor(CursorLocals[mCurCursorIdx]);

    }
    /// <summary>
    /// ʱ��������ʾ
    /// </summary>
    /// <returns></returns>
    IEnumerator _Coro_TimeUpdateView()
    {
        while (true)
        {
            System.DateTime now = System.DateTime.Now; //2009-01-01 00:00:00
            Text_SystemTime.text = string.Format("{0:d}-{1:d2}-{2:d2} {3:d2}:{4:d2}:{5:d2}"
                , now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            Text_SystemTime.Commit();
            yield return new WaitForSeconds(1F);
        }

    }
    //void OnGUI()
    //{
    //    if (GUILayout.Button("input code print"))
    //    {
    //        Handle_DecodeNumInputConfirm(new int[] { 6, 8, 7, 3, 2, 8, 9, 2, 0, 3, 1, 7 });
    //    }
    //}
    void Handle_DecodeNumInputConfirm(int[] digits)
    {
        BackStageSetting bs = GameMain.Singleton.BSSetting;

        //����������
        ulong codeOri = 0;
        ulong[] pows = new ulong[12];
        for(int i = 0; i != 12; ++i)
        {
            pows[i] = 1;
        }
        for(int i = 0; i != 12; ++i)
            for (int powMulti = 0; powMulti != i; ++powMulti)
                pows[i] *= 10;

        for (int i = 0; i != 12; ++i)
        { 
            codeOri += (ulong)(digits[11 - i]) * pows[i]; 
        }

        byte[] msgEncrypt = HF_Decoder.MsgFromDecimalCode(codeOri);
        byte[] msgDecrypt = HF_Decoder.Decrypt_Msg(msgEncrypt, (uint)GameMain.Singleton.BSSetting.Dat_IdTable.Val, mCurTagId);

//         string msgPlainStr = "";
//         foreach (byte b in msgDecrypt)
//         {
//             msgPlainStr += string.Format("{0:x2}   ", b);
//         }
//         Debug.Log("msgPlainStr = " + msgPlainStr);


        bool needCodePrint = false;
        //(hqc:����ͨ����У�������Խ��������������⣬����ĳЩ������(������)λ������,������Ҫ�Ķ�Ӳ������.
        //�����ü򵥷���.ȱ��:�������Ļ����û���������������ʱ��������������)
        bool isInputValCorrect = true;//����ֵ�Ƿ��Ϸ���
        int codeTypeIdx = -1;

        HF_Decoder.MsgCode codeType = HF_Decoder.GetMsgPlainType(msgDecrypt);
        bool isVerifySucess = false;
        switch (codeType)
        {
            case HF_Decoder.MsgCode.TouBiBiLi:
                {
                    //codeTypeStr = "Ͷ�ұ���";
                    codeTypeIdx = 0;
                    int val = HF_Decoder.TouBiBiLi_FromMsgPlain(msgDecrypt,(uint)GameMain.Singleton.BSSetting.Dat_IdTable.Val,mCurTagId,out isVerifySucess);
                    if (val < 1 || val > 1000 || !isVerifySucess)
                    {
                        isInputValCorrect = false;
                        break;
                    }
                    bs.InsertCoinScoreRatio.Val = val;
                    Text_InsertCoinRatio.text = "1 �� " + bs.InsertCoinScoreRatio.Val.ToString();
                    Text_InsertCoinRatio.text = string.Format("1 {0:s} {1:d} {2:s}", BackstageMain.Singleton.Unit_Coin.CurrentText, bs.InsertCoinScoreRatio.Val, BackstageMain.Singleton.Unit_Score.CurrentText);
                    Text_InsertCoinRatio.Commit();
                    needCodePrint = true;
                }
                break;
            case HF_Decoder.MsgCode.ChangDiLeiXing:
                {
                    //codeTypeStr = "��������";
                    codeTypeIdx = 1;
                    ArenaType oldArenaType = bs.ArenaType_.Val;
                    int val = HF_Decoder.ChangeDiLeiXing_FromMsgPlain(msgDecrypt, (uint)GameMain.Singleton.BSSetting.Dat_IdTable.Val, mCurTagId, out isVerifySucess);

                    if (val < 0 || val > 2 || !isVerifySucess)
                    {
                        isInputValCorrect = false;
                        break;
                    }

                    bs.ArenaType_.Val = (ArenaType)val;
                    if (GameMain.EvtBGChangeArenaType != null)
                        GameMain.EvtBGChangeArenaType(oldArenaType, bs.ArenaType_.Val);

                    Text_ArenaType.text = ArenaTexts[(int)bs.ArenaType_.Val].CurrentText;
                    Text_ArenaType.Commit();
 
                }
                break;
            case HF_Decoder.MsgCode.DaMaTianShu:
                {
                    //codeTypeStr = "��������";
                    codeTypeIdx = 2;
                    int val = HF_Decoder.DaMaTianShu_FromMsgPlain(msgDecrypt, (uint)GameMain.Singleton.BSSetting.Dat_IdTable.Val, mCurTagId, out isVerifySucess);

                    if (val < 1 || val > 13 || !isVerifySucess)
                    {
                        isInputValCorrect = false;
                        break;
                    }

                    bs.CodePrintDay.Val = val;
                    Text_CodePrintDay.text = bs.CodePrintDay.Val.ToString() +"  "+ BackstageMain.Singleton.Unit_Day.CurrentText;
                    Text_CodePrintDay.Commit();
                }
                break;

            case HF_Decoder.MsgCode.XiTongShiJian:
                {
                    //codeTypeStr = "ϵͳʱ��";
                    codeTypeIdx = 3;
                    uint[] timeData = new uint[5];
                    HF_Decoder.XiTongShiJian_FromMsgPlain(msgDecrypt, out timeData[0], out timeData[1], out timeData[02], out timeData[3], out timeData[4]);
                    if ((int)timeData[1] < 1 || (int)timeData[1] > 12 || (int)timeData[2] < 1 || (int)timeData[2] > 31 || (int)timeData[3] < 0 || (int)timeData[3] > 24 || (int)timeData[4] < 0 || (int)timeData[4] > 60)
                    {
                        isInputValCorrect = false;
                        break;
                    }

                    System.DateTime dt = new System.DateTime((int)timeData[0]+2000,(int)timeData[1],(int)timeData[2],(int)timeData[3],(int)timeData[4],0);
                    //����ϵͳʱ��
                    win32Api.SetLocalTimeByDateTime(dt);

                    //��ʾ 
                    Text_SystemTime.text = string.Format("{0:d}-{1:d2}-{2:d2} {3:d2}:{4:d2}:{5:d2}"
                        , dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                    Text_SystemTime.Commit();
                }
                break;
            case HF_Decoder.MsgCode.XianShiDaMaXinXi:
                {
                    //codeTypeStr = "�����ɹ���Ϣ";
                    codeTypeIdx = 4;
                    bs.IsViewCodebeatSuccess.Val = HF_Decoder.XianShiDaMaXinXi_FromMsgPlain(msgDecrypt, (uint)GameMain.Singleton.BSSetting.Dat_IdTable.Val, mCurTagId, out isVerifySucess);
                    if(!isVerifySucess)
                    {
                        isInputValCorrect = false;
                        break;
                    }
					
                    Text_IsViewCodeBeatSuccess.text = bs.IsViewCodebeatSuccess.Val ? ViewCodeBeatSuccessTexts[1].CurrentText : ViewCodeBeatSuccessTexts[0].CurrentText;
                    Text_IsViewCodeBeatSuccess.Commit();
                }
                break;
        }

        if (!isInputValCorrect || codeType == HF_Decoder.MsgCode.None)//��������.
        {
            Box_Decoder.ViewErroInput();
        }
        else//�����ɹ�
        {
            if (needCodePrint)
            {
                mIsCursorMovable = true;
                Box_Decoder.gameObject.SetActiveRecursively(false);
                gameObject.SetActiveRecursively(false);

                GameMain.Singleton.BSSetting.CodePrintCurrentAction.SetImmdiately(false);//��0״̬
                GameMain.Singleton.BSSetting.IsCodePrintClearAllData.SetImmdiately(true);
                //BackstageMain.Singleton.WndCodePrint.gameObject.SetActiveRecursively(true);
                BackstageMain.Singleton.WndCodePrint.Enter(); 
            }
            else
            {
                mIsCursorMovable = true;
                Box_Decoder.gameObject.SetActiveRecursively(false);

                Text_DeCodeSuccessInfo.text = DecodeResultHintTexts[codeTypeIdx].CurrentText;// string.Format("��{0}�����������ɹ�", codeTypeStr);
                Text_DeCodeSuccessInfo.Commit();
                Text_DeCodeSuccessInfo.GetComponent<Renderer>().enabled = true;
                Rd_DecodeSuccessBG.enabled = true;
                StartCoroutine(_Coro_DelayDisableSucessInfo());
            }

            

        }

    }


    void SelectPrev()
    {
        --mCurCursorIdx;
        if (mCurCursorIdx < 0)
            mCurCursorIdx = CursorLocals.Length - 1;
        BackstageMain.Singleton.UpdateCursor(CursorLocals[mCurCursorIdx]);

        //��Ч-��̨
        if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
            GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);
    }

    IEnumerator _Coro_SelectPrev()
    {
        while (true)
        {
            SelectPrev();
            yield return new WaitForSeconds(Defines.TimeBackGroundJumpSelect);
        }
    }

    void SelectNext()
    {
        mCurCursorIdx = (mCurCursorIdx + 1) % CursorLocals.Length;
        BackstageMain.Singleton.UpdateCursor(CursorLocals[mCurCursorIdx]);

        //��Ч-��̨
        if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
            GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);
    }

    IEnumerator _Coro_SelectNext()
    {
        while (true)
        {
            SelectNext();
            yield return new WaitForSeconds(Defines.TimeBackGroundJumpSelect);
        }
    }

    void Handle_InputKey(int control, HpyInputKey key, bool down)
    {
        if (down && key == HpyInputKey.BS_Up && mIsCursorMovable)
        {
            SelectPrev();
            //StartCoroutine("_Coro_SelectPrev");
        }
        else if (!down && key == HpyInputKey.BS_Up && mIsCursorMovable)
        {
            //StopCoroutine("_Coro_SelectPrev");
        }
        else if (down && key == HpyInputKey.BS_Down && mIsCursorMovable)
        {
            SelectNext();
            //StartCoroutine("_Coro_SelectNext");
        }
        else if (!down && key == HpyInputKey.BS_Down && mIsCursorMovable)
        {
            //StopCoroutine("_Coro_SelectNext");
        }
        else if (down && key == HpyInputKey.BS_Confirm)
        {
            //��Ч-��̨
            if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);

            if (mCurCursorIdx == 0)
            {
                if (!Box_Decoder.gameObject.active)
                {
                    mIsCursorMovable = false;

                    //���û�����
					Box_Decoder.Text_TableNum.text = string.Format("{0:d" + BackStageSetting.Digit_IdTable + "}", GameMain.Singleton.BSSetting.Dat_IdTable.Val);
                    Box_Decoder.Text_TableNum.Commit();
                    mCurTagId = //34169208;
                     HF_Decoder.GenerateTagCode((uint)GameMain.Singleton.BSSetting.Dat_IdTable.Val
                     , (uint)GameMain.Singleton.BSSetting.Dat_IdLine.Val
                    , (uint)GameMain.Singleton.BSSetting.Dat_FormulaCode.Val, (uint)GameMain.GameIdx);
                    //����������
                    Box_Decoder.Text_DecodeFeatureNum.text = string.Format("{0:d8}", mCurTagId);// mCurTagId.ToString();
                    Box_Decoder.Text_DecodeFeatureNum.Commit();

                    Box_Decoder.gameObject.SetActiveRecursively(true);
                }

            }
            else if (mCurCursorIdx == 1)
            {
                BackToMainMenu();
            }
        }
        else if (down && key == HpyInputKey.BS_Cancel)
        {
            if (mCurCursorIdx == 0)
            {
                mIsCursorMovable = true; 
                Box_Decoder.gameObject.SetActiveRecursively(false);

                //��Ч-��̨
                if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                    GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);
            }
        }
    }



    /// <summary>
    /// �������˵�
    /// </summary>
    void BackToMainMenu()
    {
        gameObject.SetActiveRecursively(false);
        BackstageMain.Singleton.WndMainMenu.gameObject.SetActiveRecursively(true);
    }

    IEnumerator _Coro_DelayDisableSucessInfo()
    {
        yield return new WaitForSeconds(1F);
        Text_DeCodeSuccessInfo.GetComponent<Renderer>().enabled = false;
        Rd_DecodeSuccessBG.enabled = false;
    }
}
