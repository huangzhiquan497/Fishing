using UnityEngine;
using System.Collections;

public class LocalScale_LanguageAutoSelect : MonoBehaviour {


    public Vector3[] LocalScales;

    // Use this for initialization
    void OnEnable()
    {


        if (LocalScales == null || LocalScales.Length == 0)
        {
            Debug.LogError("LocalPos_LanguageAutoSelect语言组件成员未赋值错误.");
            Destroy(this);
            return;
        }
        GameMain.EvtLanguageChange += Handle_LanguageChanged;
        transform.localScale = LocalScales[(int)GameMain.Singleton.BSSetting.LaguageUsing.Val];
    }

    void OnDisable()
    {
        GameMain.EvtLanguageChange -= Handle_LanguageChanged;
    }
    void Handle_LanguageChanged(Language l)
    {
        transform.localScale = LocalScales[(int)GameMain.Singleton.BSSetting.LaguageUsing.Val];
    }
}
