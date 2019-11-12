using UnityEngine;
using System.Collections;

/// <summary>
/// 
/// </summary>
public class Wnd_OptionMoveAble : MonoBehaviour {
    public delegate void Event_SelectChanged(int curIdx);
    public Event_SelectChanged EvtSelectChanged;
    public Event_SelectChanged EvtConfirm;

    public CursorDimLocation[] CursorLocals;
    [System.NonSerialized]
    public bool IsControlable = true;

    private int mCurCursorIdx;
     
    void SelectPrev()
    { 
        //可在主菜单移动

        --mCurCursorIdx;
        if (mCurCursorIdx < 0)
            mCurCursorIdx = CursorLocals.Length - 1;
        BackstageMain.Singleton.UpdateCursor(CursorLocals[mCurCursorIdx]);

        if (EvtSelectChanged != null)
            EvtSelectChanged(mCurCursorIdx);

        //音效-后台
        if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
            GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);


    }

    void SelectNext()
    { 
        mCurCursorIdx = (mCurCursorIdx + 1) % CursorLocals.Length;
        BackstageMain.Singleton.UpdateCursor(CursorLocals[mCurCursorIdx]);

        if (EvtSelectChanged != null)
            EvtSelectChanged(mCurCursorIdx);

        //音效-后台
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
    IEnumerator _Coro_SelectNext()
    {
        while (true)
        {
            SelectNext();
            yield return new WaitForSeconds(Defines.TimeBackGroundJumpSelect);
        }
    } 
    void OnEnable()
    {
 
        BackstageMain.Singleton.UpdateCursor(CursorLocals[mCurCursorIdx]);
        GameMain.EvtInputKey += Handle_InputKey; 
    }


    void OnDisable()
    {
        GameMain.EvtInputKey -= Handle_InputKey;
    }


    void Handle_InputKey(int control, HpyInputKey key, bool down)
    {
        if (!IsControlable)
            return;

        if (down && key == HpyInputKey.BS_Up)
        {
            SelectPrev();
            //StartCoroutine("_Coro_SelectPrev");
        }
        //else if (!down && key == HpyInputKey.BS_Up)
        //{
        //    //StopCoroutine("_Coro_SelectPrev");
        //}
        else if (down && key == HpyInputKey.BS_Down)
        {
            SelectNext();
            //StartCoroutine("_Coro_SelectNext");
        }
        //else if (!down && key == HpyInputKey.BS_Down)
        //{
        //    //StopCoroutine("_Coro_SelectNext");
        //}

        if (down && key == HpyInputKey.BS_Confirm)
        {
            if (EvtConfirm != null)
                EvtConfirm(mCurCursorIdx);
        }
    }
}
