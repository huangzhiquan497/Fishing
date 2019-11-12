using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Ctrl_InputNum : MonoBehaviour {
    public delegate void Evt_InputConfirm(int[] digits);
    public Evt_InputConfirm EvtConfirm;
    public Event_Generic EvtDisable;
    public tk2dTextMesh Text_Tile;
    public tk2dTextMesh Text_ViewFront;
    public tk2dTextMesh Text_ViewCurrent;
    public tk2dTextMesh Text_ViewBack;

    public tk2dTextMesh Text_Cursor;
    public int Num
    {
        get { return mNum; }
        set{
            if (value != mNum)
            {
                mNum = value;
                ResetDigits();
            }
        }
    }
    private int mNum = 8;//����
    private int mNumEdited;//�Ѿ��༭��������Ŀ
    private bool mIsEditing = false;//���ڱ༭������,-1��ʾ��
    private int mIdxCursor = 0;
    private int[] mDigits;
    private bool mChangeIdxCursorAble = true;//�Ƿ����ƶ�����
    void OnEnable()
    {
        mDigits = new int[mNum];
        mNumEdited = 0;
        mIsEditing = false;
        mIdxCursor = 0;
        mChangeIdxCursorAble = true;

        GameMain.EvtInputKey += Handle_Input;

        UpdateCursor();
    }

    void OnDisable()
    {
        GameMain.EvtInputKey -= Handle_Input;
        if (EvtDisable != null)
            EvtDisable();
    }
    //��������
    public void ResetDigits()
    {
        mDigits = new int[mNum];
        mNumEdited = 0;
        mIsEditing = false;
        mIdxCursor = 0;
        mChangeIdxCursorAble = true;
        if(gameObject.active)
            UpdateCursor();
    }
    void UpdateCursor()
    {
        if (mIsEditing)
        {
            StopCoroutine("_Coro_CursorFlash");
            Text_Cursor.GetComponent<Renderer>().enabled = false;


            string strFront = "";
            string strCurrent = "";
            string strBack = "";
            for (int i = 0; i != mNumEdited; ++i )
            {
                if (i < mIdxCursor)//ǰ���ַ�
                {
                    strFront += mDigits[i].ToString();
                    strCurrent += ' ';
                    strBack += ' ';
                }
                else if (i > mIdxCursor)//�����ַ�
                    strBack += mDigits[i].ToString();
                else//���ڱ༭���ַ�
                {
                    strCurrent += mDigits[i].ToString();
                    strBack += ' ';

                }
            }

            Text_ViewFront.text = strFront;
            Text_ViewFront.Commit();
            Text_ViewCurrent.text = strCurrent.ToString();
            Text_ViewCurrent.Commit();
            Text_ViewBack.text = strBack;
            Text_ViewBack.Commit();
            StopCoroutine("_Coro_DigitFlash");
            StartCoroutine("_Coro_DigitFlash", Text_ViewCurrent.GetComponent<Renderer>());
        }
        else
        {
            Text_Cursor.GetComponent<Renderer>().enabled = true;
            string cursorStr = new string(' ', mIdxCursor);
            cursorStr += '-';
            Text_Cursor.text = cursorStr;
            Text_Cursor.Commit();
            StopCoroutine("_Coro_CursorFlash");
            StartCoroutine("_Coro_CursorFlash",Text_Cursor.GetComponent<Renderer>());

            StopCoroutine("_Coro_DigitFlash");
            Text_ViewCurrent.GetComponent<Renderer>().enabled = true;
            string strAll = "";
            for (int i = 0; i != mNumEdited; ++i )
            {
                strAll += mDigits[i].ToString();
            }
            Text_ViewFront.text = strAll;
            Text_ViewFront.Commit();
            Text_ViewBack.text = "";
            Text_ViewBack.Commit();
            Text_ViewCurrent.text = "";
            Text_ViewCurrent.Commit();

        }

        

    }


    void Handle_Input(int control, HpyInputKey key, bool down)
    {
        if (down && key == HpyInputKey.BS_Up)
        {
            if (mIsEditing)
            {
                //mDigits[mIdxCursor] = (mDigits[mIdxCursor] + 1) % 10;
                StopCoroutine("_Coro_ChangeCurrentCursorDigit");
                StartCoroutine("_Coro_ChangeCurrentCursorDigit", new KeyValuePair<int,float>(1,0F));
                mChangeIdxCursorAble = false;
            }
            else
            {
                //�ж��Ƿ��Ѿ��༭��
                if (mIdxCursor == mNumEdited)//���µ�λ��
                {
                    mDigits[mIdxCursor] = 0;
                    if (mNumEdited < mNum)
                        ++mNumEdited;
                }

                mIsEditing = true;
                StopCoroutine("_Coro_ChangeCurrentCursorDigit");
                StartCoroutine("_Coro_ChangeCurrentCursorDigit", new KeyValuePair<int, float>(1, 0.5F));
 
                mChangeIdxCursorAble = false;
            }
        
            
            UpdateCursor();
        }
        else if (down && key == HpyInputKey.BS_Down)
        {
   
            if (mIsEditing)
            {
                //--mDigits[mIdxCursor];
                //if (mDigits[mIdxCursor] < 0)
                //    mDigits[mIdxCursor] = 9;
                StopCoroutine("_Coro_ChangeCurrentCursorDigit");
                StartCoroutine("_Coro_ChangeCurrentCursorDigit", new KeyValuePair<int, float>(-1, 0));
                mChangeIdxCursorAble = false;
            }
            else
            {
                //�ж��Ƿ��Ѿ��༭��
                if (mIdxCursor == mNumEdited)//���µ�λ��
                {
                    mDigits[mIdxCursor] = 9;
                    if (mNumEdited < mNum)
                        ++mNumEdited;
                }

                mIsEditing = true;
                StopCoroutine("_Coro_ChangeCurrentCursorDigit");
                StartCoroutine("_Coro_ChangeCurrentCursorDigit", new KeyValuePair<int, float>(-1, 0.5F));
                mChangeIdxCursorAble = false;
            }

            UpdateCursor();
        }
        else if (down && key == HpyInputKey.BS_Left && mChangeIdxCursorAble )
        {
            //��Ч-��̨
            if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn); 

            if (mIdxCursor > 0)
                --mIdxCursor;

            if (mIsEditing)
            {
                mIsEditing = false;
            }
            UpdateCursor();
        }
        else if (down && key == HpyInputKey.BS_Right && mChangeIdxCursorAble)
        {
            //��Ч-��̨
            if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn); 

            if (mIdxCursor < mNumEdited && mIdxCursor +1 != mNum)
                ++mIdxCursor;



            if (mIsEditing)
            {
                mIsEditing = false;
            }
            UpdateCursor();
        }
        else if (down && key == HpyInputKey.BS_Confirm)
        {
            if (mNumEdited == mNum)
            {
                if (EvtConfirm != null)
                    EvtConfirm(mDigits);

                //��Ч-��̨
                if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                    GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn); 
            }
        }

        //���𰴼�//////////////////////////////////////////////////////////////////
        if (!down && key == HpyInputKey.BS_Up)
        {

            if (mIsEditing)
            {
                StopCoroutine("_Coro_ChangeCurrentCursorDigit");
                mChangeIdxCursorAble = true;
            }
            
        }
        else if (!down && key == HpyInputKey.BS_Down)
        {
            if (mIsEditing)
            {
                StopCoroutine("_Coro_ChangeCurrentCursorDigit");
                mChangeIdxCursorAble = true;
            }
        }
    }

    IEnumerator _Coro_CursorFlash(Renderer rd)
    {
        while (true)
        {
            rd.enabled = true;
            yield return new WaitForSeconds(0.5F);
            rd.enabled = false;
            yield return new WaitForSeconds(0.5F);
        }
    }

    IEnumerator _Coro_DigitFlash(Renderer rd)
    {
        while (true)
        {
            rd.enabled = true;
            yield return new WaitForSeconds(0.5F);
            rd.enabled = false;
            yield return new WaitForSeconds(0.5F);
        }
    }

    IEnumerator _Coro_ChangeCurrentCursorDigit(KeyValuePair<int,float> direct_delay)
    {
        if (direct_delay.Value != 0F)
        {
            //��Ч-��̨
            if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn); 
            yield return new WaitForSeconds(direct_delay.Value);
        }
        
        while (true)
        {
            mDigits[mIdxCursor] += direct_delay.Key;
            if (mDigits[mIdxCursor] < 0)
                mDigits[mIdxCursor] = 9;
            else if (mDigits[mIdxCursor] > 9)
                mDigits[mIdxCursor] = 0;
            UpdateCursor();

            //��Ч-��̨
            if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn); 
            yield return new WaitForSeconds(0.4F);
        }
    }

    public static ulong DigitToInt(int[] digits)
    {
        ulong outputVal = 0;
        int digitsLen = digits.Length;
        ulong[] pows = new ulong[digitsLen];

        for (int i = 0; i != digitsLen; ++i)
        {
            pows[i] = 1;
        }

        for (int i = 0; i != digitsLen; ++i)
            for (int powMulti = 0; powMulti != i; ++powMulti)
                pows[i] *= 10;

        for (int i = 0; i != digitsLen; ++i)
        {
            outputVal += (ulong)(digits[digitsLen - 1 - i]) * pows[i];
        }

        return outputVal;

    }
}
