using UnityEngine;
using System.Collections;

public class Ctrl_OptionNum : MonoBehaviour {
    public int NumMax = 10;
    public int NumMin = 1;
    public float IntervalCoroChange = 0.3F;//协同改变速度
    [System.NonSerialized]
    public string ViewTextFormat = "{0:d}";

    public int NumViewing
    {
        get { return mCurrentNum; }
        set
        {
            if (value > NumMax)
                mCurrentNum = NumMin;
            else if (value < NumMin)
                mCurrentNum = NumMax;
            else
                mCurrentNum = value;

            UpdateText();
        }

    }

    private int mCurrentNum = 0;
    private tk2dTextMesh mText;

    public delegate int Func_GetChangeValue(int current);
    private Func_GetChangeValue mGetChangeValueFunc;

    public void UpdateText()
    {
        if (mText == null)
            mText = GetComponentInChildren<tk2dTextMesh>();
        mText.text = string.Format(ViewTextFormat,mCurrentNum.ToString());
        mText.Commit();
 
    }

    public void StartChangeNumViewing(int advance)
    {
        StartCoroutine(_Coro_ChangeNumViewing(advance));
    }
    public void StopChangeNumViewing()
    {
        StopAllCoroutines();
    }
    IEnumerator _Coro_ChangeNumViewing(int advance)
    {
        while (true)
        {
            NumViewing += advance;
            //音效-后台
            if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);

            yield return new WaitForSeconds(IntervalCoroChange);
        }
    }

    //使用函数来获得变化值

    public void StartChangeNumViewing(Func_GetChangeValue func)
    {
        StartCoroutine(_Coro_ChangeNumViewing(func));
    } 
    IEnumerator _Coro_ChangeNumViewing(Func_GetChangeValue func)
    {
        while (true)
        {
            NumViewing += func(NumViewing);
            //音效-后台
            if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);

            yield return new WaitForSeconds(IntervalCoroChange);
        }
    }







}
