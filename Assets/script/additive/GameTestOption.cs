using UnityEngine;
using System.Collections;

public class GameTestOption : MonoBehaviour {
    public float TimeNormalScene = 300F;
    public float GameSpeed = 1F;
    public bool OneKillDie = false;

    public FishGeneratConfig FishGenerateNew;
    public bool FishGenerate_IsEmitFish = true;

    public bool Prelude_IsRandom = true;//是否随机鱼阵
    public int Prelude_StartIdx = 0;//第一个出鱼阵的序号

    public float RatioGet_Lizi = 0.0033F;//离子炮获得几率
    
    GameMain mGM;
    GunLiziGettorMain mGunLiziGettor;
	// Use this for initialization
	void Awake () {
        mGM = GameMain.Singleton;
        mGM.TimeNormalScene = TimeNormalScene;
        mGM.GameSpeed = GameSpeed;
        Time.timeScale = GameSpeed;
        mGM.OneKillDie = OneKillDie;

        mGM.ScenePreludeMgr.IsRandomPrelude = Prelude_IsRandom;
        mGM.ScenePreludeMgr.PreludeIdxStart = Prelude_StartIdx;

        //GameOdds.RatioGet_Lizi = RatioGet_Lizi;
        mGunLiziGettor = FindObjectOfType(typeof(GunLiziGettorMain)) as GunLiziGettorMain;
        if (mGunLiziGettor != null)
            mGunLiziGettor.LiziRatio = RatioGet_Lizi;

        mGM.FishGenerator.IsGenerate = FishGenerate_IsEmitFish;
	}
	void Start()
    {
        //因为 FishGeneratSetter在Awake处设置
        if (FishGenerateNew != null)
        {
            FishGenerator fg = mGM.FishGenerator;
            if (fg == null)
                return;
            fg.FishGenerateDatas = FishGenerateNew.FishGenerateDatas;
            fg.FishGenerateUniqueDatas = FishGenerateNew.FishGenerateUniqueDatas;
            fg.FishQueueData = FishGenerateNew.FishQueueData;


            fg.Interval_FishGenerate = FishGenerateNew.Interval_FishGenerate;
            fg.Interval_FishUniqueGenerate = FishGenerateNew.Interval_FishUniqueGenerate;
            fg.Interval_QueuGenerate = FishGenerateNew.Interval_QueuGenerate;
            fg.Interval_FlockGenerate = FishGenerateNew.Interval_FlockGenerate;
        }
    }


    void FixedUpdate()
    {
        if (mGM.TimeNormalScene != TimeNormalScene)
        {
            mGM.TimeNormalScene = TimeNormalScene;
        }

        if (Time.timeScale != GameSpeed)
        {
            mGM.GameSpeed = GameSpeed;
            Time.timeScale = GameSpeed;
        }

        if (mGunLiziGettor != null && mGunLiziGettor.LiziRatio != RatioGet_Lizi)
        {
            mGunLiziGettor.LiziRatio = RatioGet_Lizi;
        }

        if (mGM.FishGenerator.IsGenerate != FishGenerate_IsEmitFish)
        {
            mGM.FishGenerator.IsGenerate = FishGenerate_IsEmitFish;
        }

        if (mGM.OneKillDie != OneKillDie)
        {
            mGM.OneKillDie = OneKillDie;
        }
    }


}
