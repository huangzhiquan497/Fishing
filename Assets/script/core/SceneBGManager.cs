using UnityEngine;
using System.Collections;

public class SceneBGManager : MonoBehaviour
{
    public GameObject[] Prefab_BackGrounds;
    public GameObject Prefab_Wave;
    public Shader OldBGShader;
    public Camera Camera_OldScene;

    public Transform Prefab_TsSweeper;

    public bool IsNewBGWhenStartGame = true;
    public float UseTime = 3F;
 
    private int CurrentBGIdx;
    private GameObject mCurrentBG;
    private GameObject mCurrentWave;
    //private  
    public GameObject CurrentBG
    {
        get
        {
            return mCurrentBG;
        }
    }

    void Start()
    {
        GameMain.EvtMainProcess_StartGame += Handle_StartGame;
    }

    void Handle_StartGame()
    {
        if(IsNewBGWhenStartGame)
            NewBG();
    }
    /// <summary>
    /// 初始化一个场景
    /// </summary>
    public void NewBG()
    {
        NewBG(-1);
    }

    /// <summary>
    /// 初始化一个场景
    /// </summary>
    /// <param name="bgIdx">如果为-1则随机初始化</param>
    public void NewBG(int bgIdx)
    {
        if (Prefab_BackGrounds.Length == 0)
            return;
        CurrentBGIdx = bgIdx < 0 ? Random.Range(0, Prefab_BackGrounds.Length) : bgIdx % Prefab_BackGrounds.Length;

        if (mCurrentBG != null)
        {
            Destroy(mCurrentBG.gameObject);
            mCurrentBG = null;
        }

        mCurrentBG = CreateBG(Prefab_BackGrounds[CurrentBGIdx]);
        mCurrentBG.transform.parent = Camera.main.transform;
        mCurrentBG.transform.position = new Vector3(0F, 0F, Defines.GlobleDepth_SceneBackground);

        if (mCurrentWave == null && Prefab_Wave != null)
        {
            mCurrentWave = Instantiate(Prefab_Wave) as GameObject;
        }
    }
    GameObject CreateBG(GameObject prefabRenderer)
    {
        GameObject bgParent = new GameObject("BG");
        Transform tsBgParent = bgParent.transform;
        //tsBgParent.parent = Camera.main.transform;
        //tsBgParent.localPosition = new Vector3(0F, 0F, 9.99F);
        Rect worldDim = GameMain.Singleton.WorldDimension;
        for (int i = 0; i != GameMain.Singleton.ScreenNumUsing; ++i)
        {
            GameObject goBG = Instantiate(prefabRenderer) as GameObject;
            goBG.transform.parent = tsBgParent;
            goBG.transform.localPosition = new Vector3(worldDim.x + worldDim.width * 0.5F * (1F / GameMain.Singleton.ScreenNumUsing + i), 0F, 0F);
        }
        return bgParent;
    }


    public void Sweep()
    {
        StartCoroutine("_Coro_NextBG");
    }

    IEnumerator _Coro_NextBG()
    {
        if (mCurrentBG == null)
            yield break;

        GameMain gm = GameMain.Singleton;
        //音效
        if (gm.SoundMgr.snd_Spindrift != null)
            gm.SoundMgr.PlayOneShot(gm.SoundMgr.snd_Spindrift);

        //打开背景camera
        Camera_OldScene.enabled = true;

        //创建newScene
        GameObject oldBG = mCurrentBG;
        CurrentBGIdx = (CurrentBGIdx ) % Prefab_BackGrounds.Length;
        mCurrentBG = CreateBG(Prefab_BackGrounds[CurrentBGIdx]);// Instantiate(Prefab_BackGrounds[CurrentBGIdx]) as GameObject;
        mCurrentBG.transform.parent = Camera_OldScene.transform;
        mCurrentBG.transform.localPosition = new Vector3(0F, 0F, Defines.GlobleDepth_SceneBackground);

 
        //创建新背景
        //初始化遮罩sweeper
        Transform tsSweeper = Instantiate(Prefab_TsSweeper) as Transform;
        tsSweeper.parent = transform;
        tsSweeper.position = new Vector3(gm.WorldDimension.xMax , 0F, Defines.GlobleDepth_SceneSweeper);
        
        Transform tsSweeperLeft = null;
        if (gm.ScreenNumUsing > 1)
        {
            tsSweeperLeft = Instantiate(Prefab_TsSweeper) as Transform;
            tsSweeperLeft.parent = transform;
            tsSweeperLeft.position = new Vector3(gm.WorldDimension.xMin, 0F, Defines.GlobleDepth_SceneSweeper);
            tsSweeperLeft.right = new Vector3(1F, 0F, 0F);
        }

        //扫过
        float elapse = 0F;
        float divFactor = gm.ScreenNumUsing > 1 ? 0.5F : 1F;//如果场景大于两个.则总长度除2
        float addtiveDistance = gm.ScreenNumUsing == 1 ? 0.84F : 0F;//当只有一个屏幕的时候移动额外的距离
        float spd = (gm.WorldDimension.width + addtiveDistance) * divFactor / UseTime;
 
        while (elapse < UseTime)
        {
            tsSweeper.position += spd * Time.deltaTime * tsSweeper.right;
            if(tsSweeperLeft != null)
                tsSweeperLeft.position += spd * Time.deltaTime * tsSweeperLeft.right;

            elapse += Time.deltaTime; 
            yield return 0;
        }


        //new清除所有鱼
        GameMain.Singleton.FishGenerator.KillAllImmediate();

        //减淡tsSweeper
        tk2dSprite sprAniSea = tsSweeper.GetComponentInChildren<tk2dSprite>() as tk2dSprite;

        tk2dSprite sprAniSeaLeft = tsSweeperLeft == null ? null : tsSweeperLeft.GetComponentInChildren<tk2dSprite>() as tk2dSprite;
        float fadeOutUseTime = 1F;
        elapse = 0F;
        Color aniCol = sprAniSea.color;
        aniCol.a = 1F;
        while (elapse < fadeOutUseTime)
        {
            aniCol.a = 1F - elapse / fadeOutUseTime;
            sprAniSea.color = aniCol;

            if(sprAniSeaLeft != null)
                sprAniSeaLeft.color = aniCol;

            elapse += Time.deltaTime;
            yield return 0;
        }
        

        ////new移动出鱼点放回原镜头
        //GameMain.Singleton.FishGenerator.transform.parent = Camera.main.transform;
        //GameMain.Singleton.FishGenerator.transform.localPosition = localPosFishGeneratorOri;

        //将新场景移动主镜头处 
        mCurrentBG.transform.parent = Camera.main.transform;
        mCurrentBG.transform.position = new Vector3(0F, 0F, Defines.GlobleDepth_SceneBackground);
        
        //关闭背景camera
        Camera_OldScene.enabled = false;

        //删除 sweeper
        Destroy(tsSweeper.gameObject);
        if (tsSweeperLeft != null)
            Destroy(tsSweeperLeft.gameObject);

 
        //删除旧场景
        Destroy(oldBG);


    }

    IEnumerator _Coro_NextBGMoreThanOneScreen()
    {
        if (mCurrentBG == null)
            yield break;
        //音效
        if (GameMain.Singleton.SoundMgr.snd_Spindrift != null)
            GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_Spindrift);

        //打开背景camera
        Camera_OldScene.enabled = true;

        //创建newScene
        GameObject oldBG = mCurrentBG;
        CurrentBGIdx = (CurrentBGIdx + 1) % Prefab_BackGrounds.Length;
        mCurrentBG = CreateBG(Prefab_BackGrounds[CurrentBGIdx]);// Instantiate(Prefab_BackGrounds[CurrentBGIdx]) as GameObject;
        mCurrentBG.transform.parent = Camera_OldScene.transform;
        mCurrentBG.transform.localPosition = new Vector3(0F, 0F, 10F);


        //创建新背景
        //初始化遮罩sweeper
        Transform tsSweeper = Instantiate(Prefab_TsSweeper) as Transform;
        tsSweeper.parent = transform;
        tsSweeper.localPosition = new Vector3(2.3F, 0F, 2.5F);

        //扫过

        float startX = 2.3F;
        float endX = -2.5F;
        float spd = (endX - startX) / UseTime;

        while (true)
        {
            tsSweeper.localPosition += new Vector3(spd * Time.deltaTime, 0F, 0F);
            if (tsSweeper.localPosition.x < endX)
            {
                break;
            }

            yield return 0;
        }
        //减淡tsSweeper
        tk2dAnimatedSprite sprAniSea = tsSweeper.GetComponentInChildren<tk2dAnimatedSprite>() as tk2dAnimatedSprite;
        float fadeOutUseTime = 1F;
        float fadeOutElapse = 0F;
        Color aniCol = sprAniSea.color;
        aniCol.a = 1F;
        while (fadeOutElapse < fadeOutUseTime)
        {
            aniCol.a = 1F - fadeOutElapse / fadeOutUseTime;
            sprAniSea.color = aniCol;
            fadeOutElapse += Time.deltaTime;
            yield return 0;
        }


        //new清除所有鱼
        GameMain.Singleton.FishGenerator.KillAllImmediate();
        ////new移动出鱼点放回原镜头
        //GameMain.Singleton.FishGenerator.transform.parent = Camera.main.transform;
        //GameMain.Singleton.FishGenerator.transform.localPosition = localPosFishGeneratorOri;

        //将新场景移动主镜头处 
        mCurrentBG.transform.parent = Camera.main.transform;
        mCurrentBG.transform.localPosition = new Vector3(0F, 0F, 9.99F);

        //关闭背景camera
        Camera_OldScene.enabled = false;

        //删除 sweeper
        Destroy(tsSweeper.gameObject);


        //删除旧场景
        Destroy(oldBG);


    }
    //void OnGUI()
    //{
    //    if (GUILayout.Button("nextBG"))
    //    {
    //        Sweep();
    //    }
    //}
	
 
}
