using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ef_FishGather : MonoBehaviour {
    public tk2dSprite PrefabSpr;
    public string[] SpriteName;
    public Vector3[] LocalPoss;
    public GameObject PrefabGO_BGGatherView;//�ռ���ʾ�ı���
    public Vector3 LocalPlayerPos_BGGatherView = new Vector3(0.3864782F,0.07634497F,-0.09000015F);//�ռ��㱳����Player�еı�������
    public float Duration_FinishPad = 5F;//�ռ���ɽ�������ʾʱ��
    public GameObject PrefabGO_EfBackground;//�ռ���ɽ�����prefab
    

    public GameObject PrefabGO_EfBigViewer;//�ý���ʾ���
    
    public AudioClip Snd_GetPrize;


    private Transform[] TsPlayerGatherBGs;//�ռ��㱳��,���������id
    private FishGather mFg;
    void Awake()
    {
        mFg = GetComponent<FishGather>();
        if (mFg == null)
        {
            Debug.LogError("Ef_FishGather ��Ҫ�� FishGather����ͬһ���ű�.");
            return;
        }
        mFg.EvtPlayerGatherFishInited += Evt_PlayerGatherFishInit;
        mFg.EvtPlayerGatherFishActive += Evt_PlayerGatherFishActive;
        mFg.EvtPlayerGatheredAllFish += Evt_PlayerGatheredAllFish;
        mFg.EvtPayBonus += Evt_PayBonus;
        TsPlayerGatherBGs = new Transform[Defines.MaxNumPlayer];
        
        
        //for (int i = 0; i != GameMain.Singleton.Players.Length; ++i)
        foreach(Player p in GameMain.Singleton.Players)
        {
            
            GameObject bgGOGather = Instantiate(PrefabGO_BGGatherView) as GameObject;

            Transform tsCreditBoard = p.transform.Find("UI_CreditBoard");
            if (tsCreditBoard == null)
            {
                Debug.LogError("GameObject.Player�±�������ΪUI_CreditBoard��Go");
                continue;
            }

            if (bgGOGather == null )
                continue;
            Transform bgTSGather = bgGOGather.transform;
            bgTSGather.parent = tsCreditBoard.transform; 
            bgTSGather.localPosition = LocalPlayerPos_BGGatherView;
            bgTSGather.localRotation = Quaternion.identity;
            TsPlayerGatherBGs[p.Idx] = bgGOGather.transform;
        }
    }

    void Evt_PlayerGatherFishInit(Player player, Fish fish, int gatherIdx)
    {
        tk2dSprite spr = Instantiate(PrefabSpr) as tk2dSprite;
        spr.spriteId = spr.GetSpriteIdByName(SpriteName[gatherIdx]);
        Transform tsSpr = spr.transform;
        tsSpr.parent = TsPlayerGatherBGs[player.Idx];
        tsSpr.localPosition = LocalPoss[gatherIdx];
        tsSpr.localRotation = Quaternion.identity;
        //StartCoroutine(_Coro_FlashFishSpr(spr));
    }

    void Evt_PlayerGatherFishActive(Player player, Fish fish, int gatherIdx)
    {
        tk2dSprite spr = Instantiate(PrefabSpr) as tk2dSprite;
        spr.spriteId = spr.GetSpriteIdByName(SpriteName[gatherIdx]);
        Transform tsSpr = spr.transform;
        tsSpr.parent = TsPlayerGatherBGs[player.Idx];
        tsSpr.localPosition = LocalPoss[gatherIdx];
        tsSpr.localRotation = Quaternion.identity;

        //�ռ���������
        if (TsPlayerGatherBGs[player.Idx].childCount >= mFg.CountFishNeedGather)
        {
            StartCoroutine(_Coro_EffectFlashAllSpr(TsPlayerGatherBGs[player.Idx],mFg.TimeDelayBonus));
        }
        else
            StartCoroutine(_Coro_FlashFishSpr(spr,5F));

    }

    void Evt_PlayerGatheredAllFish(Player player, Fish fish, int gatherIdx)
    {
        
        
    }

    void Evt_PayBonus(Player player, int bouns)
    {

        StartCoroutine(_Coro_EffectProcessing(bouns, player));
        //��ʾ�󽱺��
        for (int i = 0; i != GameMain.Singleton.ScreenNumUsing; ++i)
            StartCoroutine(_Coro_EffectViewBigPad(player.Idx, bouns, new Vector2(GameMain.Singleton.WorldDimension.x + (0.5F + i) * Defines.WorldDimensionUnit.width, 0F)));
    }

    /// <summary>
    /// ��˸���о���
    /// </summary>
    /// <param name="parentTs"></param>
    /// <returns></returns>
    IEnumerator _Coro_EffectFlashAllSpr(Transform parentTs,float elapse)
    {
        //�Ȱ����о����ƶ�����(��ֹ����˸�ڼ�����µ�,�����Ժ���ʾ������)
        GameObject goFlashFishGahterSpr = new GameObject("tempFlashfishGatherSpr");
        List<Transform> sprAll = new List<Transform>();//����ֱ������parent�ᵼ�±�������

        foreach (Transform ts in parentTs)
        {
            sprAll.Add(ts);
        }
        
        foreach (Transform ts in sprAll)
        {
            ts.parent = goFlashFishGahterSpr.transform;
            StartCoroutine(_Coro_FlashFishSpr(ts.GetComponent<tk2dSprite>(), elapse));
        }


        yield return new WaitForSeconds(elapse);
        Destroy(goFlashFishGahterSpr);
    }

    /// <summary>
    /// ��˸һ������
    /// </summary>
    /// <param name="spr"></param>
    /// <param name="elapse"></param>
    /// <returns></returns>
    IEnumerator _Coro_FlashFishSpr(tk2dSprite spr,float elapse)
    { 

        //spr.color 
        float flashElapse = elapse;
        float flashTotalElapse = flashElapse;
        float flashSpd = 6F;
        Color colorFix = spr.color;
        while (flashElapse > 0F)
        {
            if (spr == null)
                yield break;

            colorFix.a = Mathf.Abs(Mathf.Cos(flashElapse / flashTotalElapse * Mathf.PI * flashSpd));
            spr.color = colorFix;
            flashElapse -= Time.deltaTime;
            yield return 0;
        }
        colorFix.a = 1F;
        spr.color = colorFix;
    }

    //���ͷ�Ͻ�����
    IEnumerator _Coro_EffectProcessing(int num, Player killer )
    {
        if (PrefabGO_EfBackground == null)
            yield break;
        //����
        GameMain.Singleton.SoundMgr.PlayOneShot(Snd_GetPrize);

        //����������
        GameObject goEffect = Instantiate(PrefabGO_EfBackground) as GameObject;
        tk2dSprite aniSpr = goEffect.GetComponentInChildren<tk2dSprite>();
        Transform tsEffect = goEffect.transform;
        tk2dTextMesh text = goEffect.GetComponentInChildren<tk2dTextMesh>();

        text.text = num.ToString();
        text.Commit();

        //��ʼ��λ����
        Vector3 originLocalPos = new Vector3(0.385F, -0.5F, -0.19F);
        Vector3 targetLocalPos = new Vector3(0.385F, 0.5F, -0.19F);

        tsEffect.parent = killer.transform;
        tsEffect.localPosition = originLocalPos;
        tsEffect.localRotation = Quaternion.identity;


        //ҡ������
        iTween.RotateAdd(text.gameObject, iTween.Hash("z", 8F, "time", 0.27F, "looptype", iTween.LoopType.pingPong, "easetype", iTween.EaseType.easeInOutSine));


        //�����ƶ�
        float elapse = 0F;
        float useTime = 0.2F;
        while (elapse < useTime)
        {
            tsEffect.localPosition = originLocalPos + (targetLocalPos - originLocalPos) * (elapse / useTime);
            elapse += Time.deltaTime;
            yield return 0;
        }
        tsEffect.localPosition = targetLocalPos;
        yield return new WaitForSeconds(Duration_FinishPad);

        //����
        elapse = 0F;
        useTime = 0.2F;
        while (elapse < useTime)
        {
            aniSpr.color = new Color(1F, 1F, 1F, 1F - elapse / useTime);
            text.color = new Color(1F, 1F, 1F, 1F - elapse / useTime);
            text.Commit();
            elapse += Time.deltaTime;
            yield return 0;
        }


        Destroy(goEffect.gameObject); 
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="playeridx"></param>
    /// <param name="score"></param>
    /// <param name="center"></param>
    /// <returns></returns>
    IEnumerator _Coro_EffectViewBigPad(int playeridx,int score,Vector2 center)
    {
        GameObject goEffect = Instantiate(PrefabGO_EfBigViewer) as GameObject;
        goEffect.transform.position = new Vector3(center.x, center.y, Defines.GlobleDepth_PrepareInBG);
        goEffect.transform.localScale = new Vector3(0F, 1F, 1F);

        tk2dTextMesh textPlayerID = goEffect.transform.Find("TextPlayerId").GetComponent<tk2dTextMesh>();
        tk2dTextMesh textScore = goEffect.transform.Find("TextScore").GetComponent<tk2dTextMesh>();
        textPlayerID.text = (playeridx + 1).ToString();
        textPlayerID.Commit();
        textScore.text = score.ToString();
        textScore.Commit();

        iTween.ScaleTo(goEffect, iTween.Hash("scale", Vector3.one, "easeType", iTween.EaseType.spring, "time", 0.2F));
        yield return new WaitForSeconds(5F);
        iTween.ScaleTo(goEffect, iTween.Hash("scale",new Vector3(0F,1F,1F), "easeType", iTween.EaseType.easeInQuad, "loopType", iTween.LoopType.none, "time", 0.2F));
        yield return new WaitForSeconds(1.1f);
        Destroy(goEffect);
    }

    
}
