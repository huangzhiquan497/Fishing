using UnityEngine;
using System.Collections;

public class Ctrl_MsgBox : MonoBehaviour {
    public CursorDimLocation[] LocalButtoms;
    public delegate void Evt_Result(int result);
    public Evt_Result EvtResult;
    public Event_Generic EvtDisable;

    public int SelectIdx
    {
        set
        {
            if (value >= 0 && value < LocalButtoms.Length)
            {
                mCurIdx = value;
               
            }
        }
    }
    private int mCurIdx = 0;
    void OnEnable()
    { 
        GameMain.EvtInputKey += Handle_Input;
         
        BackstageMain.Singleton.UpdateCursor(LocalButtoms[mCurIdx]);
        BackstageMain.Singleton.Cursor.Ani_Cursor.GetComponent<Renderer>().enabled = false;
    }

    void OnDisable()
    {
        
        BackstageMain.Singleton.Cursor.Ani_Cursor.GetComponent<Renderer>().enabled = true;
        GameMain.EvtInputKey -= Handle_Input;
        if (EvtDisable != null)
            EvtDisable();
    }

    void SelectNext()
    {
        ++mCurIdx;
        if (mCurIdx > LocalButtoms.Length - 1)
            mCurIdx = 0;

        BackstageMain.Singleton.UpdateCursor(LocalButtoms[mCurIdx]);

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

    void SelectPrev()
    {
        --mCurIdx;
        if (mCurIdx < 0)
            mCurIdx = LocalButtoms.Length - 1;
        BackstageMain.Singleton.UpdateCursor(LocalButtoms[mCurIdx]);

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

    void Handle_Input(int control, HpyInputKey key, bool down)
    {
        if (down && key == HpyInputKey.BS_Left)
        {
            SelectPrev();
            //StartCoroutine("_Coro_SelectPrev");
        }
        else if (!down && key == HpyInputKey.BS_Left)
        {
            //StopCoroutine("_Coro_SelectPrev");
        }
        else if (down && key == HpyInputKey.BS_Right)
        {
            SelectNext();
            //StartCoroutine("_Coro_SelectNext");
        }
        else if (!down && key == HpyInputKey.BS_Right)
        {
            //StopCoroutine("_Coro_SelectNext");
        }
        else if (down && key == HpyInputKey.BS_Confirm)
        {
            if(EvtResult != null)
                EvtResult(mCurIdx);

            //��Ч-��̨
            if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);
        }
        else if (down && key == HpyInputKey.BS_Cancel)
        {
            gameObject.SetActiveRecursively(false);

            //��Ч-��̨
            if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);
        }
    }
}
