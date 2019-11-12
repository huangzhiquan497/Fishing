using UnityEngine;
using System.Collections;

public class ScenePreludeEx_Freeezebomb : MonoBehaviour {

    private ScenePrelude mSp;
	// Use this for initialization
    void Awake()
    {
        mSp = GetComponent<ScenePrelude>();
        if (mSp == null)
            return;

        GameMain.EvtFreezeBombActive += Handle_FreezeBombActive;
        GameMain.EvtFreezeBombDeactive += Handle_FreezeBombDeactive;
	
	}

    void Handle_FreezeBombActive()
    {
        mSp.Pause();
    }
    void Handle_FreezeBombDeactive()
    {
        mSp.Resume();
    }
	 
}
