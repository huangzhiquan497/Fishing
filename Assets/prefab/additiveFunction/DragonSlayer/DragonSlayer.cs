using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// ����:������
/// </summary>
/// <remarks>
/// ע��:
/// 1.�ɱ�������ɱ�����б��ڵ���������ּ��ʱ���һ��,(��Ϊ�����ƽ������)->��������ɱ������ֵļ��ʻᱻǿ�����һ��
/// 2.������������������пɱ�������ɱ�����㲻��ͬʱ����(������ҿ���ͨ���۲���������ʱ��ɱ����������������,�Ӷ���ñ������ߵ�����)
/// 3.�����㲻����˫�� (��Ϊ��������Щ����)
/// </remarks>
public class DragonSlayer : MonoBehaviour {
    public Fish Prefab_FishToGenerateDragonSlayer;//����DragonSlayer����

    public Fish[] Prefab_SlayFish;//�ɱ�ɱ������,���б�ɱ������ּ��ʱ���һ��
    public int GenerateRate_SlayFish = 1000;//�ɱ�ɱ����ĳ��ּ���
    public Fish Prefab_DragonSlayer;//������prefab
    public PersistentData<int, int>[] DragonSlayerScores;//�������ķ���,Ϊ0�Ļ���û�е�
	// Use this for initialization
    public GameObject Ef_DragonKilled;//������ɱ��ʱ����Ч��
    public GameObject PrefabGO_EfBigViewer;//������������ʾ

    private int mSlayFishAveragyOdd;//��ɱ���ƽ��odd,                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
    private Dictionary<int, Fish> mSlayFishCache;//�ɱ�ɱ����cache,��Prefab_SlayFish����
	void Awake () {
        //HitProcessor.FuncGetFishAddtiveForDieRatio += Func_GetFishOddAddtiveForDieRatio;
        HitProcessor.AddFunc_Odd(Func_GetFishOddAddtiveForDieRatio,null);
        GameMain.EvtFishKilled += Event_FishKilled;
        GameMain.EvtMainProcess_FinishPrelude += Event_MainProcess_FinishPrelude;
        GameMain.EvtMainProcess_FirstEnterScene += Event_MainProcess_FirstEnterScene;
        GameMain.EvtBGClearAllData_Before += Event_BackGroundClearAllData_Before;

        DragonSlayerScores = new PersistentData<int, int>[Defines.MaxNumPlayer];
        for (int i = 0; i != Defines.MaxNumPlayer; ++i)
            DragonSlayerScores[i] = new PersistentData<int, int>("DragonSlayerScores" + i.ToString());

        foreach (Fish f in Prefab_SlayFish)
        {
            RandomOddsNum rndOdd = f.GetComponent<RandomOddsNum>();
            if (rndOdd != null)
            {
                mSlayFishAveragyOdd += (rndOdd.MinOdds + rndOdd.MaxOdds) / 2;
            }
            else
                mSlayFishAveragyOdd += f.Odds;
        }

        mSlayFishCache = new Dictionary<int, Fish>();
        foreach (Fish f in Prefab_SlayFish)
        {
            mSlayFishCache.Add(f.TypeIndex, f);
        }

        mSlayFishAveragyOdd /= Prefab_SlayFish.Length;
        //GenerateDragonSlayer(0);

         
    }

    HitProcessor.OperatorOddFix Func_GetFishOddAddtiveForDieRatio(Player killer, Bullet b, Fish f, Fish fCauser)
    {
        //������������������
        if (Prefab_FishToGenerateDragonSlayer.TypeIndex != f.TypeIndex)
            return null;

        //��ӵ��������
        if (DragonSlayerScores[killer.Idx].Val != 0)
            return null;

        return new HitProcessor.OperatorOddFix(HitProcessor.Operator.Add,mSlayFishAveragyOdd);

    }

    /// <summary>
    /// ��Ӧ�¼�:������()
    /// </summary>
    /// <param name="killer"></param>
    /// <param name="b"></param>
    /// <param name="f"></param>
    void Event_FishKilled(Player killer, Bullet b, Fish f)
    {
        if (Prefab_FishToGenerateDragonSlayer.TypeIndex == f.TypeIndex)
        {
            //���ֵ�
            //�Ƿ��Ѵ���������
            if (DragonSlayerScores[killer.Idx].Val == 0)//ֻ��ͬʱ����һ��������
            {
                DragonSlayerScores[killer.Idx].Val = b.Score;//��¼��
                GenerateDragonSlayer(killer.Idx, b.Score);

                //Ч����ʾ
                //��ʾ�󽱺��
                for (int i = 0; i != GameMain.Singleton.ScreenNumUsing; ++i)
                    StartCoroutine(_Coro_EffectViewBigPad(killer.Idx, new Vector2(GameMain.Singleton.WorldDimension.x + (0.5F + i) * Defines.WorldDimensionUnit.width, 0F)));

            }

        }
    }

    /// <summary>
    /// ��Ӧ�¼�:�������,(��Ҫ���������ʱ���³�������)
    /// </summary>
    void Event_MainProcess_FinishPrelude()
    {
        for (int i = 0; i != Defines.MaxNumPlayer; ++i)
        {
            if (DragonSlayerScores[i].Val != 0)
                GenerateDragonSlayer(i, DragonSlayerScores[i].Val);
        }
    }

    /// <summary>
    /// ��Ӧ�¼�:��һ�ν��볡��
    /// </summary>
    void Event_MainProcess_FirstEnterScene()
    {
        for (int i = 0; i != Defines.MaxNumPlayer; ++i)
        {
            if (DragonSlayerScores[i].Val != 0)
                GenerateDragonSlayer(i, DragonSlayerScores[i].Val);
        }

        FishGenerator fg = GameMain.Singleton.FishGenerator;

        //���ó���ļ���
        FishGenerator.FishGenerateData[] fgds = fg.FishGenerateUniqueDatas;
        //foreach (FishGenerator.FishGenerateData fgd in fgds)
        foreach (Fish slayFish in Prefab_SlayFish)
        {
            for (int i = 0; i != fgds.Length; ++i)
            {
                if (fgds[i].Prefab_Fish.TypeIndex == slayFish.TypeIndex)
                {
                    fgds[i].Weight = GenerateRate_SlayFish;
                    
                    break;
                }
            }
        }

        fg.RefreshAllGenerateWeight();
    }

    /// <summary>
    /// ��̨˫������0
    /// </summary>
    void Event_BackGroundClearAllData_Before()
    {
        for (int i = 0; i != Defines.MaxNumPlayer; ++i)
            DragonSlayerScores[i].Val = 0;
    }

    void GenerateDragonSlayer(int playerIdx,int slayerScore)
    {
        if (slayerScore == 0)
        {
            Debug.LogWarning("slayerScore��������Ϊ0");
            return;
        } 

        FishGenerator fishGenrator = GameMain.Singleton.FishGenerator;
        Player p = GameMain.Singleton.Players[playerIdx];
        Transform tsPlayer = p.transform;
        Fish dragonSlayer = Instantiate(Prefab_DragonSlayer) as Fish;
        Transform tsDragonSlayer = dragonSlayer.transform;
        
        tsDragonSlayer.parent = fishGenrator.transform;//��fishGenerator��Ϊ���ڵ�,����ʱ����ɾ��
        tsDragonSlayer.position = tsPlayer.position;
        tsDragonSlayer.rotation = tsPlayer.rotation*Quaternion.Euler(0F,0F,90f);

        Vector3 tmpPos = tsDragonSlayer.localPosition;
        tmpPos.z = -3.1F;
        tsDragonSlayer.localPosition = tmpPos;

        dragonSlayer.swimmer.Go();

        FishEx_DragonSlayer fishExDragonSlayer = dragonSlayer.GetComponentInChildren<FishEx_DragonSlayer>();
        if (fishExDragonSlayer != null)
        {
            fishExDragonSlayer.Owner = p;
            fishExDragonSlayer.Creator = this;

            //Ч��:������ʾ���
            Transform tsTextPlayerId = fishExDragonSlayer.transform.Find("TextPlayerId");
            if (tsTextPlayerId != null)
            {

                tk2dTextMesh textPlayerID = tsTextPlayerId.GetComponent<tk2dTextMesh>();
                textPlayerID.text = (p.Idx + 1).ToString();
                textPlayerID.Commit();
            }
            else
            {
                Debug.LogError("��Ҫ��fishDragonSlayer�����·���tk2dtextmesh:TextPlayerId");
            }


        }
        else
        {
            Debug.LogError("����������gameobject������� fishExDragonSlayer.");
        }
        
       
    }

    /// <summary>
    /// ��FishEx_DragonSlayer�ص�(�����¼���������)
    /// </summary>
    /// <param name="ds"></param>
    /// <param name="owner"></param>
    public void On_FishExDragonSlayerTouchFish(FishEx_DragonSlayer ds, Fish fishTouched,Player owner)
    {
        //�Ӵ�����fish�Ѿ�����
        if (fishTouched == null || !fishTouched.Attackable)
            return;

        if (!mSlayFishCache.ContainsKey(fishTouched.TypeIndex))
            return;
        //*Ϊ�˵��ú�������һ����ʱ��bullet,�ǳ�Ż��,����û�취,��Ϊ�ܶ�ط�ʹ���������.�Ժ���Ҫע���¼�,���ú�����д��
        GameObject tmpBulletGo = new GameObject();
        Bullet b = tmpBulletGo.AddComponent<Bullet>();
        b.FishOddsMulti = 1;
        b.Score = DragonSlayerScores[owner.Idx].Val;

        fishTouched.Kill(owner, b, 0F);

        owner.GainScore(fishTouched.Odds * DragonSlayerScores[owner.Idx].Val
            , fishTouched.Odds
            , DragonSlayerScores[owner.Idx].Val
            );
       
        //ɾ��֮ǰ������
        Destroy(tmpBulletGo);

        //dragonSlayerScore����
        DragonSlayerScores[owner.Idx].Val = 0;

        //ɾ��dragonSlayer
        ds.Clear();

        //����Ч��
        GameObject efGo = Instantiate(Ef_DragonKilled) as GameObject;
        Vector3 worldPos = fishTouched.transform.position;
        worldPos.z = Defines.GlobleDepth_BombParticle;
        efGo.transform.position = worldPos;
    }



    IEnumerator _Coro_EffectViewBigPad(int playeridx, Vector2 center)
    {
        GameObject goEffect = Instantiate(PrefabGO_EfBigViewer) as GameObject;
        goEffect.transform.position = new Vector3(center.x, center.y, Defines.GlobleDepth_PrepareInBG);
        goEffect.transform.localScale = new Vector3(0F, 1F, 1F);

        tk2dTextMesh textPlayerID = goEffect.transform.Find("TextPlayerId").GetComponent<tk2dTextMesh>();
        textPlayerID.text = (playeridx + 1).ToString();
        textPlayerID.Commit();

        iTween.ScaleTo(goEffect, iTween.Hash("scale", Vector3.one, "easeType", iTween.EaseType.spring, "time", 0.2F));
        yield return new WaitForSeconds(5F);
        iTween.ScaleTo(goEffect, iTween.Hash("scale", new Vector3(0F, 1F, 1F), "easeType", iTween.EaseType.easeInQuad, "loopType", iTween.LoopType.none, "time", 0.2F));
        yield return new WaitForSeconds(1.1f);
        Destroy(goEffect);
    }
}
