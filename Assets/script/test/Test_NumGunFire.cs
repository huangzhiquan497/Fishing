using UnityEngine;
using System.Collections;

public class Test_NumGunFire : MonoBehaviour {
    public int[] PlayerFireNum;
    private float mStartTime;
    private bool mCounting = false;
	// Use this for initialization
	void Start () {
        PlayerFireNum = new int[Defines.NumPlayer];
        GameMain.EvtPlayerGunFired += Handle_PlayerFire;
	}

    void OnGUI()
    {
        if (mCounting)
        {
            if (GUILayout.Button("StopCount"))
            {
                uint fireTotal = 0;
                foreach (int pf in PlayerFireNum)
                {
                    fireTotal += (uint)pf;
                }

                for (int i = 0; i != Defines.NumPlayer; ++i)
                    GameMain.EvtInputKey(i, HpyInputKey.Fire, false);

                mCounting = false;
                float useTime = Time.time - mStartTime;
                Debug.Log("Time = " + useTime +"    avgFireCount = " + fireTotal / Defines.NumPlayer
                    +"fire per sec = "+ useTime / (fireTotal/Defines.NumPlayer));

            }
        }
        else
        {
            if (GUILayout.Button("StartFireCount"))
            {
                mStartTime = Time.time;

                for (int i = 0; i != Defines.NumPlayer; ++i)
                    GameMain.EvtInputKey(i, HpyInputKey.Fire, true);

                mCounting = true;
            }
        }
    }
    void Handle_PlayerFire(Player p, Gun gun, int useScore)
    {
        ++PlayerFireNum[p.Idx];
    }
}
