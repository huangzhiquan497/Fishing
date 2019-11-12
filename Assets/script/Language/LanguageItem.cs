using UnityEngine;
using System.Collections;

public class LanguageItem : MonoBehaviour {
    public string[] Texts;
    public string CurrentText
    {
        get
        {
            return Texts == null ? "" : Texts[((int)GameMain.Singleton.BSSetting.LaguageUsing.Val)%Texts.Length];
        }
    }
}
