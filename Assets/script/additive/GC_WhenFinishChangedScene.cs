using UnityEngine;
using System.Collections;

public class GC_WhenFinishChangedScene : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameMain.EvtMainProcess_FinishChangeScene += Handle_MainProcess_FinishChangeScene;
	}
	
    void Handle_MainProcess_FinishChangeScene()
    {
        System.GC.Collect();
	}
}
