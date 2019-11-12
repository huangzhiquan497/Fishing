using UnityEngine;
using System.Collections;

public class CusorDimEx_LanguageAutoSelect : MonoBehaviour
{
    public Vector3[] Prefab_DimsByLangauge;
    private CursorDimLocation mCursorDim;
    // Use this for initialization
    void OnEnable()
    {

        mCursorDim = GetComponent<CursorDimLocation>();
        if (mCursorDim == null||Prefab_DimsByLangauge==null ||Prefab_DimsByLangauge.Length == 0)
        {
            Debug.LogError("CusorDimEx_LanguageAutoSelect语言组件成员未赋值错误.");
            Destroy(this);
            return;
        }
        GameMain.EvtLanguageChange += Handle_LanguageChanged;
        mCursorDim.Dimension = Prefab_DimsByLangauge[(int)GameMain.Singleton.BSSetting.LaguageUsing.Val];
    }

    void OnDisable()
    {
        GameMain.EvtLanguageChange -= Handle_LanguageChanged;
    }
    void Handle_LanguageChanged(Language l)
    {
        if (mCursorDim != null)
            mCursorDim.Dimension = Prefab_DimsByLangauge[(int)GameMain.Singleton.BSSetting.LaguageUsing.Val];
    }
}
