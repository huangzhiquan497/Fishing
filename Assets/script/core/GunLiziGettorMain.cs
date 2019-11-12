using UnityEngine;
using System.Collections;

public class GunLiziGettorMain : MonoBehaviour {
    public float LiziRatio = 0.0033F;
    private GunLiziGettor[] LiziGettors;
    void Awake()
    {
        GameMain.Evt_PlayerGainScoreFromFish += Handle_PlayerGainScoreFromFish;
        GameMain.EvtMainProcess_StartGame += Handle_StartGame;
        LiziGettors = new GunLiziGettor[Defines.NumPlayer];
        
    }

    void Handle_StartGame()
    {
        foreach (Player p in GameMain.Singleton.Players)
        {
            LiziGettors[p.Idx] = p.GetComponent<GunLiziGettor>();
        }
    }

    void Handle_PlayerGainScoreFromFish(Player p, int score, Fish firstFish, Bullet b)
    {

        //几率获得离子炮 
        if (firstFish.HittableTypeS == "Normal" && p.GunInst.GetPowerType() != GunPowerType.Lizi)
        {
            if (Random.Range(0F, 1F) < LiziRatio)
            {
                LiziGettors[p.Idx].GetLiziKaFrom(firstFish.transform.position);
            }
        }
    }
}
