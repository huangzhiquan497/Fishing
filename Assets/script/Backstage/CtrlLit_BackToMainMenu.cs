using UnityEngine;
using System.Collections;

public class CtrlLit_BackToMainMenu : MonoBehaviour {

    void OnEnable()
    { 
        GameMain.EvtInputKey += Handle_Input; 
    }

    void OnDisable()
    {
        GameMain.EvtInputKey -= Handle_Input;
    }
    void Handle_Input(int control, HpyInputKey key, bool down)
    {
        if (down && key == HpyInputKey.BS_Cancel)
        {
            gameObject.SetActiveRecursively(false);
            BackstageMain.Singleton.WndMainMenu.gameObject.SetActiveRecursively(true);

            //“Ù–ß-∫ÛÃ®
            if (GameMain.Singleton.SoundMgr.snd_bkBtn != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_bkBtn);
        }

    }
}
