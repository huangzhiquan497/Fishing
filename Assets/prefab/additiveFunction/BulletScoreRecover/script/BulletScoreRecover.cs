using UnityEngine;
using System.Collections;

public class BulletScoreRecover : MonoBehaviour {

    public delegate void Evt_BulletScoreStartRecover(int playerIdx, int score);
    public static Evt_BulletScoreStartRecover EvtBulletScoreStartRecover;
    private BackStageSetting mBss;

	void Awake () 
    {
        mBss = GameMain.Singleton.BSSetting;
	    GameMain.EvtPlayerGunFired += Handle_PlayerGunFired;
        GameMain.EvtBulletDestroy += Handle_PlayerBulletDestroy;
        GameMain.EvtMainProcess_FirstEnterScene += Handle_FirstEnterScene;
	}




    void Handle_FirstEnterScene()
    {
        foreach (Player p in GameMain.Singleton.Players)
        {
            if (mBss.Dat_PlayersBulletScore[p.Idx].Val > 0)
            {
                if(EvtBulletScoreStartRecover != null)
                    EvtBulletScoreStartRecover(p.Idx, mBss.Dat_PlayersBulletScore[p.Idx].Val);

                //mBss.Dat_PlayersScore[p.Idx].Val += mBss.Dat_PlayersBulletScore[p.Idx].Val;
                p.ChangeScore(mBss.Dat_PlayersBulletScore[p.Idx].Val);
                mBss.Dat_PlayersBulletScore[p.Idx].SetImmdiately(0);
            }
        }
    }

    IEnumerator _Coro_ScoreChanged(int pidx,int s)
    {
        yield return 0;
        mBss.Dat_PlayersBulletScore[pidx].Val += s;
    }

    void Handle_PlayerGunFired(Player p, Gun gun, int useScore)
    {
        StartCoroutine(_Coro_ScoreChanged(p.Idx, useScore));
        //mBss.Dat_PlayersBulletScore[p.Idx].Val += useScore;
    }

    void Handle_PlayerBulletDestroy(Bullet b)
    {
        StartCoroutine(_Coro_ScoreChanged(b.Owner.Idx, -b.Score));
        //mBss.Dat_PlayersBulletScore[playerIdx].Val -= score;
    }

}
