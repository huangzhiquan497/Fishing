using UnityEngine;
using System.Collections;

public class Test_ViewCurrentGainRatio : MonoBehaviour {
    public Rect windowRect = new Rect(20, 20, 120, 50);
    string[] difficultStrs = { "最容易", "容易", "难", "最难", "死难" };
    GameMain mGM;
    
	// Use this for initialization
	void Awake() {
        mGM = GameMain.Singleton;
	}
	 
    void OnGUI()
    {
        windowRect = GUILayout.Window(0, windowRect, DoMyWindow, "赔率信息");
       

    }

    void DoMyWindow(int windowID)
    {
        GUILayout.TextArea(string.Format("最容易:{0:f4}  容易:{1:f4}  难:{2:f4}  最难:{3:f4}  死难:{4:f4}"
           , GameOdds.GainRatios[0]
           , GameOdds.GainRatios[1]
           , GameOdds.GainRatios[2]
           , GameOdds.GainRatios[3]
           , GameOdds.GainRatios[4]));

        GUILayout.TextArea("当前难度：" + difficultStrs[(int)GameMain.Singleton.BSSetting.GameDifficult_.Val]);

        GUILayout.TextArea("当前赔率：" + GameOdds.GainRatio);
        int totalScore = 0;
        //for(int i = 0; i != Defines.MaxNumPlayer; ++i)
        foreach(Player p in mGM.Players)
        {
            totalScore += mGM.BSSetting.Dat_PlayersScore[p.Idx].Val;
        }
        GUILayout.TextArea("当前总分:" + totalScore);
        GUI.DragWindow();
    }
}
