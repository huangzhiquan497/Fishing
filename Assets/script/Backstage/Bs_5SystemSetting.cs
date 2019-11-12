using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.IO;

public class Bs_5SystemSetting : MonoBehaviour {
    enum UIState
    {
        Admin,//�����߽���
        Arena//���ؽ���
    }
    public Ctrl_InputNum Prefab_NumInputer;
    public Transform TsLocal_NumInputer;

    
    public tk2dTextMesh Text_Hint;//��ʾ��Ϣ


    public Wnd_OptionMoveAble Prefab_WndOptionMoverAdmin;
    public Wnd_OptionMoveAble Prefab_WndOptionMoverArena;


    public LanguageItem LI_SetupLineIdSuccess;//���ػ�:�����ߺųɹ�
    public LanguageItem LI_Enter3DigitLineID;//���ػ�:������3λ���ߺ�

    public LanguageItem LI_SetupPswSucess;//���ػ�:���������ɹ�

    public LanguageItem LI_EnteredPswNotSame;//���ػ�:�����������벻һ��
    public LanguageItem LI_EnterNDigitPsw;//���ػ�:������nλ����,(��c#string.format{0:d})

    public LanguageItem LI_PlzEnterNewLineID;//���ػ�:�������µ��ߺ�
    public LanguageItem LI_PlzEnterNewTableID;//���ػ�:�������µ�̨��
    public LanguageItem LI_PlzEnterNewPsw;//���ػ�:�������µ�����
    public LanguageItem LI_EnterPswAgain;//���ػ�:���ٴ���������

    public LanguageItem LI_PlzEnterFormulaCode;//���ػ�:���빫ʽ��
    public LanguageItem LI_SetupFormulaCodeSucess;//���ػ�:��ʽ�����óɹ�
    public LanguageItem LI_FormulaDigitNumWorng;//���ػ�:������8λ��ʽ��




    private Ctrl_InputNum mCurNumInputer;//��ǰ������������
    private Wnd_OptionMoveAble mCurWndOptioner;
    //private Wnd_OptionMoveAble mWndOptionMoverArena;
    private static HMACMD5 mHmacMd5;

    private static UIState mUIState;
    private int mCurSelectIdx = 0;
    public static HMACMD5 Cryptor
    {
        get
        {
            if (mHmacMd5 == null)
            {
                mHmacMd5 = new HMACMD5(System.Text.Encoding.ASCII.GetBytes("yidingyaochang"));
            }
            return mHmacMd5;
        }
    }
    
    

    string ReadGamePswMD5(string pswName)
    {
        string filename = Directory.GetCurrentDirectory() + "/DataFiles/"+pswName;
        
        if (!File.Exists(filename))
            return "";

        string output;
        using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
        {
            using(BinaryReader br = new BinaryReader(fs))
                output = br.ReadString();
        }
        
        return output;
    }

    void WritePswMD5(string pswName, string psw)
    {
        string filename = Directory.GetCurrentDirectory() + "/DataFiles/" + pswName;
        using (FileStream fs = new FileStream(filename, FileMode.Create))
        {
            using (BinaryWriter br = new BinaryWriter(fs))
                br.Write(psw);
        }
    }


    

    public bool TryEnter(int[] psw)
    {
        string pswStr = Ctrl_InputNum.DigitToInt(psw).ToString();
        string pswInputCryptedStr = 
            System.Text.Encoding.ASCII.GetString(Cryptor.ComputeHash(System.Text.Encoding.ASCII.GetBytes(pswStr)));
        string pswAdminSavedStr = ReadGamePswMD5("adminPsw");
        
        //arenaģʽ
        string pswArenaSavedStr = ReadGamePswMD5("arenaPsw");
        if (pswArenaSavedStr == "" && pswStr == "822228")//Ӳ��û������,
        {
            Enter(UIState.Arena);
            return true;
        }
        if (pswArenaSavedStr == pswInputCryptedStr) 
        {
            Enter(UIState.Arena);
            return true;
        }

        //����adminģʽ
        if (pswAdminSavedStr == "" && pswStr == "911119")//Ӳ��û������ 
        {
            Enter(UIState.Admin);
            return true;
        }

        if (pswAdminSavedStr == pswInputCryptedStr)
        {
            Enter(UIState.Admin);
            return true;
        }

        return false;
    }

    void Enter(UIState state)
    {
        


        if (mCurWndOptioner != null)
        {
            Debug.LogWarning("������������Enter?");
            Destroy(mCurWndOptioner.gameObject);
            mCurWndOptioner = null;
        }

        gameObject.SetActiveRecursively(true);
        mUIState = state;

        switch (state)
        {
            case UIState.Admin:
                {
                    mCurWndOptioner = Instantiate(Prefab_WndOptionMoverAdmin, transform.position, Quaternion.identity) as Wnd_OptionMoveAble;

                    Transform ts = mCurWndOptioner.transform.Find("option4/ctrl_��ǰ��ʾ״̬");
                    Ctrl_OptionText ctrlOption = null;
                    if(ts!=null)
                        ctrlOption = ts.GetComponent<Ctrl_OptionText>();
                    if (ctrlOption != null)
                    {
                        ctrlOption.ViewIdx = GameMain.Singleton.BSSetting.Dat_GameShowLanguageSetup.Val ? 0 : 1;
                        ctrlOption.UpdateText();
                    }

                }
                break;
            case UIState.Arena:
                mCurWndOptioner = Instantiate(Prefab_WndOptionMoverArena, transform.position, Quaternion.identity) as Wnd_OptionMoveAble;
                break;
        }
        mCurWndOptioner.transform.parent = transform;
        mCurWndOptioner.transform.localPosition = Vector3.zero;
 
        mCurWndOptioner.EvtSelectChanged += Handle_OptionSelectorChanged;
        mCurWndOptioner.EvtConfirm += Handle_OptionSelectorConfirm;

        GameMain.EvtInputKey += Handle_Input;
 
        UpdateView();
        
    }

    void Exit()
    {
        if (GameMain.IsEditorShutdown)
            return;

        gameObject.SetActiveRecursively(false);
        if (mCurWndOptioner != null)
        {
            Destroy(mCurWndOptioner.gameObject);
            mCurWndOptioner = null;
        }
        

        if (mCurWndOptioner != null)
        { 
            Destroy(mCurWndOptioner.gameObject);
            mCurWndOptioner = null;
        }
         
        GameMain.EvtInputKey -= Handle_Input;
    }

    /// <summary>
    /// ������ʾ
    /// </summary>
    void UpdateView()
    {
        if(mCurWndOptioner == null)
            return;

        Ui_lineIdTableIdLink idLink = mCurWndOptioner.GetComponent<Ui_lineIdTableIdLink>();
        if (idLink == null)
            return;

        BackStageSetting bss = GameMain.Singleton.BSSetting;
        switch (mUIState)
        {
            case UIState.Admin:
                idLink.Text_LineID.text = string.Format("{0:d"+BackStageSetting.Digit_IdLine.ToString()+"}",bss.Dat_IdLine.Val);
                idLink.Text_LineID.Commit();

                idLink.Text_TableID.text = string.Format("{0:d" + BackStageSetting.Digit_IdTable.ToString() + "}", bss.Dat_IdTable.Val);
                idLink.Text_TableID.Commit();
                break;
            case UIState.Arena:
                idLink.Text_TableID.text = string.Format("{0:d" + BackStageSetting.Digit_IdTable.ToString() + "}", bss.Dat_IdTable.Val);
                idLink.Text_TableID.Commit();

                if (bss.Dat_FormulaCode.Val == 0xffffffff)
                    idLink.Text_FormulaCode.text = "-";
                else
                    idLink.Text_FormulaCode.text = string.Format("{0:d" + BackStageSetting.Digit_FormulaCode.ToString() + "}", bss.Dat_FormulaCode.Val);
                idLink.Text_FormulaCode.Commit();
                break;
        }
    }
    void Handle_Input(int control, HpyInputKey key, bool down)
    {
        if (key == HpyInputKey.BS_Cancel && down)
        {
            if (mCurNumInputer != null)
            {
                Destroy(mCurNumInputer.gameObject);
                mCurNumInputer = null;

                mCurWndOptioner.IsControlable = true;
            }
        }
        else if ((key == HpyInputKey.BS_Left||key == HpyInputKey.BS_Right) && down)
        {
            if (mCurSelectIdx == 4)
            {
                Transform ts = mCurWndOptioner.transform.Find("option4/ctrl_��ǰ��ʾ״̬");
                Ctrl_OptionText ctrlOption = null;
                if(ts!=null)
                    ctrlOption = ts.GetComponent<Ctrl_OptionText>();
                if (ctrlOption != null)
                {
                    PersistentData<bool,bool> isShowLanguageSetup = GameMain.Singleton.BSSetting.Dat_GameShowLanguageSetup;
                    isShowLanguageSetup.Val = !isShowLanguageSetup.Val;
                    ctrlOption.ViewIdx = isShowLanguageSetup.Val ? 0 : 1;
                    ctrlOption.UpdateText();

                }
                else
                {
                    Debug.LogError("ctrlOption == null");
                }
            }
        }
        
    }
 

    void Handle_OptionSelectorConfirm(int selectIdx)
    {
        switch (mUIState)
        {
            case UIState.Admin:
                if (selectIdx == 0)//�ߺ�����
                {
                    SetupLineIDOrTableID(true, BackStageSetting.Digit_IdLine);
                }
                else if (selectIdx == 1)
                {
                    SetupLineIDOrTableID(false, BackStageSetting.Digit_IdTable);
                }
                else if (selectIdx == 2)//����Ա��������
                {
                    SetupPsw("adminPsw");
                }
                else if (selectIdx == 3)//������������
                {
                    SetupPsw("arenaPsw");
                }
                else if (selectIdx == 5)//�������˵�
                {
                    Exit();
                    BackstageMain.Singleton.WndMainMenu.gameObject.SetActiveRecursively(true);
                }
                break;
            case UIState.Arena:
                if (selectIdx == 0)//̨������
                {
                    SetupLineIDOrTableID(false, BackStageSetting.Digit_IdTable);
                }
                else if (selectIdx == 1)//���ù�ʽ��
                {
                    SetupFormulaCode();
                }
                else if (selectIdx == 2)//������������
                {
                    SetupPsw("arenaPsw");
                }

                else if (selectIdx == 3)//�������˵�
                {
                    Exit();
                    BackstageMain.Singleton.WndMainMenu.gameObject.SetActiveRecursively(true);
                }
                break;
        }
    }

    void SetupFormulaCode()
    {
        mCurWndOptioner.IsControlable = false;
        //����һ��������
        mCurNumInputer = Instantiate(Prefab_NumInputer) as Ctrl_InputNum;
        mCurNumInputer.Num = BackStageSetting.Digit_FormulaCode;
        mCurNumInputer.Text_Tile.text = LI_PlzEnterFormulaCode.CurrentText;
        mCurNumInputer.Text_Tile.Commit();
        mCurNumInputer.transform.parent = transform;
        mCurNumInputer.transform.localPosition = TsLocal_NumInputer.localPosition;


        mCurNumInputer.EvtConfirm = (int[] digits) =>
        {

            if (digits.Length == BackStageSetting.Digit_FormulaCode)
            {
                uint formulaCodeNew = (uint)Ctrl_InputNum.DigitToInt(digits);
				if (formulaCodeNew == 0)
					formulaCodeNew = 0xffffffff;
                GameMain.Singleton.BSSetting.Dat_FormulaCode.SetImmdiately(formulaCodeNew);
                Destroy(mCurNumInputer.gameObject);
                mCurNumInputer = null;
                mCurWndOptioner.IsControlable = true;
                ViewHint(LI_SetupFormulaCodeSucess.CurrentText);

                UpdateView();
               
            }
            else
                ViewHint(LI_FormulaDigitNumWorng.CurrentText);

        };
    }

    /// <summary>
    /// �����ߺŻ���̨��
    /// </summary>
    /// <param name="lineIdOrTableID">true:�ߺ�,false:̨��</param>
    /// <param name="numLen"></param>
    void SetupLineIDOrTableID(bool lineIdOrTableID,int numLen)
    {
        mCurWndOptioner.IsControlable = false;
        //����һ��������
        mCurNumInputer = Instantiate(Prefab_NumInputer) as Ctrl_InputNum;
        mCurNumInputer.Num = numLen;
        mCurNumInputer.Text_Tile.text = lineIdOrTableID ? LI_PlzEnterNewLineID.CurrentText: LI_PlzEnterNewTableID.CurrentText;
        mCurNumInputer.Text_Tile.Commit();
        mCurNumInputer.transform.parent = transform;
        mCurNumInputer.transform.localPosition = TsLocal_NumInputer.localPosition;


        mCurNumInputer.EvtConfirm = (int[] digits) =>
        {

            if (digits.Length == numLen)
            {
                if(lineIdOrTableID)
                    GameMain.Singleton.BSSetting.Dat_IdLine.SetImmdiately((int)Ctrl_InputNum.DigitToInt(digits));
                else
                    GameMain.Singleton.BSSetting.Dat_IdTable.SetImmdiately((int)Ctrl_InputNum.DigitToInt(digits));
                Destroy(mCurNumInputer.gameObject);
                mCurNumInputer = null;
                mCurWndOptioner.IsControlable = true;
                ViewHint(LI_SetupLineIdSuccess.CurrentText);
                UpdateView();
            }
            else
                ViewHint(LI_Enter3DigitLineID.CurrentText);

        };

    }


    void SetupPsw(string pswName)
    {
         mCurWndOptioner.IsControlable = false;
            //����һ��������
            mCurNumInputer = Instantiate(Prefab_NumInputer) as Ctrl_InputNum;
            mCurNumInputer.Num = Defines.PswLength;
            mCurNumInputer.Text_Tile.text = LI_PlzEnterNewPsw.CurrentText;
            mCurNumInputer.Text_Tile.Commit();
            mCurNumInputer.transform.parent = transform;
            mCurNumInputer.transform.localPosition = TsLocal_NumInputer.localPosition;

            ulong pswInputFirst = 0;
            int pswInputState = 1;//1��һ������,2�ڶ�������
            mCurNumInputer.EvtConfirm = (int[] digits) =>
            {
                if (digits.Length == Defines.PswLength)
                {
                    if (pswInputState == 1)
                    {
                        pswInputFirst = Ctrl_InputNum.DigitToInt(digits);
                        mCurNumInputer.ResetDigits();
                        mCurNumInputer.Text_Tile.text = LI_EnterPswAgain.CurrentText;
                        mCurNumInputer.Text_Tile.Commit();
                        pswInputState = 2;
                    }
                    else if (pswInputState == 2)
                    {
                        ulong pswInputSecond = Ctrl_InputNum.DigitToInt(digits);//�ڶ�����������
                        if (pswInputFirst == pswInputSecond)
                        {

                            byte[] md5Psw = Cryptor.ComputeHash(System.Text.Encoding.ASCII.GetBytes(pswInputSecond.ToString()));
                            string md5PswStr = System.Text.Encoding.ASCII.GetString(md5Psw);
                            WritePswMD5(pswName, md5PswStr);

                            Destroy(mCurNumInputer.gameObject);
                            mCurNumInputer = null;
                            mCurWndOptioner.IsControlable = true;

                            ViewHint(LI_SetupPswSucess.CurrentText);
                        }
                        else//�����������벻һ��
                        {
                            mCurNumInputer.ResetDigits();
                            mCurNumInputer.Text_Tile.text = LI_PlzEnterNewPsw.CurrentText;
                            mCurNumInputer.Text_Tile.Commit();
                            pswInputState = 1;
                            ViewHint(LI_EnteredPswNotSame.CurrentText);
                        }
                    }

                }
                else
                    ViewHint(string.Format(LI_EnterNDigitPsw.CurrentText,Defines.PswLength));
            };
    }
    void ViewHint(string erroStr)
    {
        StopCoroutine("_Coro_ViewHint");
        StartCoroutine("_Coro_ViewHint",erroStr);
    }
    IEnumerator _Coro_ViewHint(string ErroStr)
    {
        //Text_Hint.renderer.enabled = false;
        Text_Hint.GetComponent<Renderer>().enabled = true;
        Text_Hint.text = ErroStr;
        Text_Hint.Commit();
        yield return new WaitForSeconds(3F);

        Text_Hint.GetComponent<Renderer>().enabled = false;
    }

    void Handle_OptionSelectorChanged(int i)
    {
        mCurSelectIdx = i;
    }
     
}
