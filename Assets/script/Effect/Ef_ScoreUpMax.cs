using UnityEngine;
using System.Collections;

public class Ef_ScoreUpMax : MonoBehaviour {
    public tk2dSpriteAnimator Prefab_AniScoreMax;

    private static readonly float Local_OffsetYToPlayer = 112.7882496F;
    private tk2dSpriteAnimator[] mAniScoreMaxSignature;
 

	// Use this for initialization
	void Start () {
        mAniScoreMaxSignature = new tk2dSpriteAnimator[Defines.NumPlayer];

        GameMain.EvtPlayerScoreChanged += Handle_ScoreChanged;
        GameMain.EvtMainProcess_StartGame += Handle_StartGame;

	}
    void Handle_StartGame()
    {

        for (int i = 0; i != Defines.NumPlayer; ++i)
        {
            if (i < GameMain.Singleton.Players.Length)
            {
                Handle_ScoreChanged(GameMain.Singleton.Players[i], GameMain.Singleton.BSSetting.Dat_PlayersScore[i].Val, 0);
            }
        }
    }
    void Handle_ScoreChanged(Player p, int scoreNew, int scoreChanged)
    {
        if (scoreNew >= Defines.NumScoreUpMax)
        {
            if (mAniScoreMaxSignature[p.Idx] == null)
            {
                mAniScoreMaxSignature[p.Idx] = Instantiate(Prefab_AniScoreMax) as tk2dSpriteAnimator;
                mAniScoreMaxSignature[p.Idx].transform.parent = p.transform;
                mAniScoreMaxSignature[p.Idx].transform.localPosition = new Vector3(0F, Local_OffsetYToPlayer, -0.002F);
                mAniScoreMaxSignature[p.Idx].transform.localRotation = Quaternion.identity;
            }
        }
        else
        {
            if (mAniScoreMaxSignature[p.Idx] != null)
            {
                Destroy(mAniScoreMaxSignature[p.Idx].gameObject);
                mAniScoreMaxSignature[p.Idx] = null;
            }
        }
    }
}
