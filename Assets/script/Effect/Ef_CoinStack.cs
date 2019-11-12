using UnityEngine;
using System.Collections;


public class Ef_CoinStack : MonoBehaviour {

    private CoinStackManager[] CoinStacks;//索引对应所属player的idx
	

	void Awake () 
    {
        GameMain.Evt_PlayerGainScoreFromFish += Handle_PlayerGainScoreFromFish;
        GameMain.EvtPlayerWonScoreChanged += Handle_PlayerWonScoreChanged;
        GameMain.EvtMainProcess_StartGame += Handle_StartGame;
        CoinStacks = new CoinStackManager[Defines.MaxNumPlayer];
	}
    void Start()
    {
        
    }

    void Handle_StartGame()
    {
        
        foreach (Player p in GameMain.Singleton.Players)
        {
            CoinStacks[p.Idx] = p.Ef_CoinStack;
        }

        foreach (Player p in GameMain.Singleton.Players)
        {
            if (GameMain.Singleton.BSSetting.Dat_PlayersScoreWon[p.Idx].Val != 0)
            {
                p.Ef_CoinStack.OneStack_SetNum(GameMain.Singleton.BSSetting.Dat_PlayersScoreWon[p.Idx].Val);
            }
        }
    }
    void Handle_PlayerGainScoreFromFish(Player p, int score, Fish fishFirst,Bullet b)
    {
        if (GameMain.Singleton.BSSetting.OutBountyType_.Val == OutBountyType.OutCoinButtom
            || GameMain.Singleton.BSSetting.OutBountyType_.Val == OutBountyType.OutTicketButtom)
        {
            int oddNum = fishFirst.HittableTypeS == "Normal" ? fishFirst.Odds : (score / b.Score);
            CoinStacks[p.Idx].RequestViewOneStack(score, oddNum, b.Score);
        }
    }

    void Handle_PlayerWonScoreChanged(Player p, int scoreNew)
    {
        CoinStacks[p.Idx].OneStack_SetNum(scoreNew);
    }
}
