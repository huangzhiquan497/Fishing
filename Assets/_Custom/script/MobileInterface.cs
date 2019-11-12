using UnityEngine;
using System.Collections;

public class MobileInterface : MonoBehaviour {
    /// <summary>
    /// 子弹击中鱼事件
    /// </summary>
    public static System.Action<Player, Bullet, Fish> EvtBulletHitFish;
    public static System.Action<bool,Player, Bullet, Fish> EvtHitFishResult;

    public static Player Player_
    {
        get
        {
            if (mPlayer == null)
            {
                if(GameMain.Singleton.Players != null)
                    mPlayer = GameMain.Singleton.Players[0];
            }
            return mPlayer;
        }
    }

    private static Player mPlayer;
    private void Start()
    {
        GameMain.Singleton.SceneBGMgr.IsNewBGWhenStartGame = false;
        HitProcessor.Evt_Hit += Handle_HitFish;
        HitProcessor.Evt_HitResult += Handle_HitFishResult;
    }
    private void Handle_HitFishResult(bool isKill, Player p, Bullet b, Fish f)
    {
        if (EvtHitFishResult != null)
            EvtHitFishResult(isKill, p, b, f);
    }
    private void Handle_HitFish(bool isMiss, Player p, Bullet b, Fish f)
    {
        if(isMiss)
            return;

        if (EvtBulletHitFish != null)
            EvtBulletHitFish(p, b, f);
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    /// <param name="sceneIdx">场景索引</param>
    public static void StartGame(int sceneIdx)
    {
        GameMain.Singleton.StartGame();
        GameMain.Singleton.SceneBGMgr.NewBG(sceneIdx);
    }

    /// <summary>
    /// 开始出鱼.只有在游戏正常状态才有效
    /// </summary>
    public static void FishGenerate_StartGen()
    {
        if(GameMain.State_ == GameMain.State.Normal)
            GameMain.Singleton.FishGenerator.StartFishGenerate();
    }


    /// <summary>
    /// 开始出鱼.只有在游戏正常状态才有效
    /// </summary>
    public static void FishGenerate_StopGen()
    {
        if (GameMain.State_ == GameMain.State.Normal) 
            GameMain.Singleton.FishGenerator.StartFishGenerate();
    }

    /// <summary>
    /// 声音总开关
    /// </summary>
    /// <param name="isOn">true:开 false:关</param>
    public static void TurnSound(bool isOn)
    {
       // Debug.Log(string.Format("TurnSound isOn = {0}  ", isOn ));
        GameMain.Singleton.SoundMgr.SetVol(isOn ? 1F : 0F);
    }

    /// <summary>
    /// 音乐音效开关
    /// </summary>
    /// <param name="isOn"></param>
    /// <param name="bgmOrEffect">true:背景音乐,false:音效</param>
    public static void TurnSound(bool isOn, bool bgmOrEffect)
    {
       // Debug.Log(string.Format("TurnSound isOn = {0} bgmOrEffect= {1} ", isOn, bgmOrEffect));
        GameMain.Singleton.SoundMgr.SetVol(bgmOrEffect, isOn ? 1F : 0F);
    }
    /// <summary>
    /// 改变玩家分数
    /// </summary>
    /// <param name="numAdd">增减量</param>
    public static void ChangePlayerScore(int numAdd)
    {
        //if (GameMain.Singleton.Players != null && GameMain.Singleton.Players[0] != null)
        //{
        //    GameMain.Singleton.Players[0].ChangeScore(numAdd);
        //}
        if (Player_ == null)
            return;
        Player_.ChangeScore(numAdd);
     
    }

    /// <summary>
    /// 获得玩家当前分数
    /// </summary>
    /// <returns></returns>
    public static int GetPlayerScore()
    {
        return GameMain.Singleton.BSSetting.Dat_PlayersScore[0].Val;
    }

    /// <summary>
    /// 设置玩家当前分数
    /// </summary>
    /// <param name="newScore"></param>
    public static void SetPlayerFireScore(int newScore)
    {
        if (Player_ == null)
            return;
        if (newScore <= 0)
            return;
        Player_.ChangeNeedScore(newScore);
        //Input.mousePosition
    }

    public static int GetPlayerFireScore()
    {
         if (Player_ == null)
            return 0;

         return GameMain.Singleton.BSSetting.Dat_PlayersGunScore[Player_.Idx].Val;
    }

    /// <summary>
    /// 玩家瞄准某个位置
    /// </summary>
    /// <param name="worldPos">世界坐标(忽略Z轴),可由Camera.main.ScreenToWorldPoint(Input.mousePosition)获得</param>
    /// <remarks>
    /// 参考代码
    /////void Update()
    ////{
    ////    if (Input.GetMouseButtonDown(0))
    ////    {
    ////        Player_StartFire();
    ////    }

    ////    else if (Input.GetMouseButtonUp(0))
    ////    {
    ////        Player_StopFire();
    ////    }

    ////    if (Input.GetMouseButton(0))
    ////    {
    ////        Vector3 worldpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    ////        Player_Aim(worldpos);
    ////    }
    ////}
    /// </remarks>
    public static void Player_Aim(Vector3 worldPos)
    {
        //if (GameMain.Singleton.Players[0] == null)
        //    return;
        if (Player_ == null)
            return;

        Transform tsGun = Player_.GunInst.TsGun;
        worldPos.z = tsGun.position.z;
        Vector3 lookDirect = worldPos - tsGun.position;
        tsGun.rotation = Quaternion.LookRotation(Vector3.forward, lookDirect);//gun's firepot is upward

    }

    /// <summary>
    /// 玩家向当前方向 开始开火 (开始后需要调用Player_StopFire才会停止)
    /// </summary>
    public static void Player_StartFire()
    {
        if (Player_ == null)
            return;
        Player_.GunInst.StartFire();
    }

    public static void Player_StopFire()
    {
        if (Player_ == null)
            return;
        Player_.GunInst.StopFire();
    }

     
}
