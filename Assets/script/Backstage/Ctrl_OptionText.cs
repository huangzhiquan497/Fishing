using UnityEngine;
using System.Collections;


/// <summary>
/// 文字选项控件
/// </summary>
/// <remarks>
/// 事件：
/// 1.
/// </remarks>
public class Ctrl_OptionText : MonoBehaviour
{
    public delegate void Event_Advancing(bool avance);
    public Event_Advancing EvtAdvanceing;

    //public string[] Text_Selectable;
    public LanguageItem[] Text_SelectableL;
    
    public float IntervalCoroChange = 0.3F;

    public int ViewIdx
    {
        get
        {
            return mCurSelectIdx;
        }
        set
        {
            if (value < 0)
            {
                mCurSelectIdx = 0;
                Debug.LogWarning("Ctrl_OptionText:" + gameObject.name + "   设置的ViewIdx小于0!!");
            }
            else if (value > Text_SelectableL.Length - 1)
            {
                mCurSelectIdx = Text_SelectableL.Length - 1;
                Debug.LogWarning("Ctrl_OptionText:" + gameObject.name + "   设置的ViewIdx大于限定范围!!");
            }
            else
                mCurSelectIdx = value;


            if (mText == null)
                mText = GetComponentInChildren<tk2dTextMesh>();
            mText.text = Text_SelectableL[mCurSelectIdx].CurrentText;
            mText.Commit();
        }
    }


    private int mCurSelectIdx;
    private tk2dTextMesh mText;

    void Awake()
    {
        if (mText == null)
            mText = GetComponentInChildren<tk2dTextMesh>();


    }
    void Start()
    {
        
        if (mText != null &Text_SelectableL != null && Text_SelectableL.Length != 0)
        {
            UpdateText();
        }
    }

    public void UpdateText()
    {
        mText.text = Text_SelectableL[mCurSelectIdx].CurrentText;
        mText.Commit();
    }
    /// <summary>
    /// 选项推进
    /// </summary>
    public void OptionAdvance()
    {
        mCurSelectIdx = (mCurSelectIdx + 1) % Text_SelectableL.Length;
        UpdateText();
    }
    /// <summary>
    /// 选项倒退
    /// </summary>
    public void OptionReverse()
    {
        --mCurSelectIdx;
        if (mCurSelectIdx < 0)
            mCurSelectIdx = Text_SelectableL.Length - 1;
        
        UpdateText();
    }


    /// <summary>
    /// 改变值
    /// </summary>
    /// <param name="advance">true:递增,false:递减</param>
    public void StartChange(bool advance)
    {
        StartCoroutine(_Coro_Change(advance));
    }
    public void StopChange()
    {
        StopAllCoroutines();
    }
    IEnumerator _Coro_Change(bool advance)
    {
        while (true)
        {
            if (advance)
                OptionAdvance();
            else
                OptionReverse();

            if (EvtAdvanceing != null)
                EvtAdvanceing(advance);

            //音效-后台
            if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);


            yield return new WaitForSeconds(IntervalCoroChange);
        }
    }
}
