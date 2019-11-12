using UnityEngine;
using System.Collections;

public class Fish : MonoBehaviour/* ,IPoolObj*/{
 
    public GameMain.Event_FishKilled EvtFishKilled;//�������¼�

    public int Odds = 1;//����

    [System.NonSerialized]
    public int OddBonus = 1;//���ڽ���������(��hitprocess��ֵ,��ֵӦ����kill����,��Ϊ���븴�ӻ�����,��������ʱ����)
    [System.NonSerialized]
    public float TimeDieAnimation = 1.38F;//������������ʱ��(Ĭ��:1.35��)

    /// <summary>
    /// //������˵��
    ///  ��ͨ��:    0~49
    ///  ��Χը��:  100~149
    ///  ͬ��ը��:  70~99
    /// </summary>
    public int TypeIndex = 0;//��������,˳������,���ظ�,���ڷ�������.��Ҫ�ֶ���ֵ.Ϊ��Ч��
    //public HittableType HittableType_ = HittableType.Normal;
    public bool HitByBulletOnly = false;//ֻ�ܱ��ӵ����й���

    public float AreaBombRadius = 1.6F;//�����Ƿ�Χը���Ļ�.��ը�ķ�Χ�뾶
    public int AreaBombOddLimit = 300;//ը����������,��������֮�����㲻�������ʼ���
    public Fish[] Prefab_SameTypeBombAffect;//������ͬ����ը��,��ը�Ǹ����͵�

    public bool IsLockable = true;//�Ƿ��ɱ�����

    public int FishTypeIdx;
    //[HideInInspector]
    public string HittableTypeS;
    //λ�Ʊ�ʶ,awake������
    public uint ID
    {
        get
        {
            if (mID == 0)
            {
                mID = mIDGenerateNow;
                ++mIDGenerateNow;
                if (mIDGenerateNow == 0)//��֤������0��ID
                    ++mIDGenerateNow;
            }
            return mID;
        }
    }

    
    //public tk2dAnimatedSprite Prefab_AniSwim;
    //public tk2dAnimatedSprite Prefab_AniDead;//pre,©д

    public GameObject Prefab_GoAniSwim;
    public GameObject Prefab_GoAniDead;//pre,©д

    public AudioClip[] Snds_Die;
 

    [System.NonSerialized]
    public bool Attackable = true;//�Ƿ��ɹ���
   
    private GameObject mPrefab;
    public GameObject Prefab
    {
        get { return mPrefab; }
        set { mPrefab = value; /*Debug.Log("Fish set prefab."); */}

    }
    private Transform mTs;

    private Renderer mRenderer;
    private uint mID = 0;
    private static uint mIDGenerateNow = 1;// ���ڼ��㵱ǰ��id
    private Swimmer mSwimmer;
    private tk2dSpriteAnimator mAnimationSprite;//mGoAniSprite��tk2dAnimatedSprite�������Ӷ������׸�tk2dAnimatedSprite
    private GameObject mGoAniSprite;
    public Swimmer swimmer
    {
        get
        {
            if (mSwimmer == null)
                mSwimmer = GetComponent<Swimmer>();
            return mSwimmer;
        }
    }

    public bool VisiableFish
    {
        set
        {
            if (mRenderer == null)
            {
                mRenderer = GetComponentInChildren<Renderer>();
            }
            mRenderer.enabled = value;
        }

    }

    public tk2dSpriteAnimator AniSprite
    {
        get
        {
            if (mAnimationSprite == null)
            {
                mGoAniSprite = Pool_GameObj.GetObj(Prefab_GoAniSwim);
                if(mGoAniSprite != null)
                    mGoAniSprite.SetActive(true);

                mAnimationSprite = mGoAniSprite.GetComponent<tk2dSpriteAnimator>();
                if (mAnimationSprite == null)
                    mAnimationSprite = mGoAniSprite.GetComponentInChildren<tk2dSpriteAnimator>();

                 
                
                Component[] renderers = mGoAniSprite.GetComponentsInChildren(typeof(Renderer));
                foreach (Component r in renderers)
                {
                    ((Renderer)r).enabled = true;
                }

                Transform tsAni = mGoAniSprite.transform;
                tsAni.parent = transform;
                tsAni.localPosition = Vector3.zero;
                tsAni.localRotation = Quaternion.identity;
               // tsAni.localScale.x = 1;
            }
            return mAnimationSprite;
        }
    }
    public void CopyDataTo(Fish tar)
    {
        
        tar.Attackable = Attackable;
        tar.mID = mID;
        
    }

// 
//     public void On_Reuse(GameObject prefab)
//     {
//         gameObject.SetActive(true);
//         prefab.GetComponent<Fish>().CopyDataTo(this);
//         VisiableFish = true;
//         collider.enabled = true;
//         ++GameMain.Singleton.NumFishAlive;
//         mAnimationSprite = AniSprite;//����һ�³�ʼ������
//  
//     }
// 
// 
//     public void On_Recycle()
//     {
//         StopAllCoroutines();
//         gameObject.SetActive(false);
//  
//         swimmer.CurrentState = Swimmer.State.Stop;
//        
//     }
    void Awake()
    {
        mTs = transform;
        swimmer.EvtSwimOutLiveArea += Handle_SwimOutLiveArea;
        ++GameMain.Singleton.NumFishAlive;
        mAnimationSprite = AniSprite;//����һ�³�ʼ������ 
        if (GameMain.EvtFishInstance != null)
            GameMain.EvtFishInstance(this);
    }

    void Handle_SwimOutLiveArea()
    {
        if (Attackable)
        {
            Attackable = false;
            Clear();
        }
    }

    //private bool mIsCleaned = false;
    /// <summary>
    /// ����,����Ļ����ʧ
    /// </summary>
    public void Clear()
    {
        //if (mIsCleaned)
        //    return;

        //mIsCleaned = true;
 
        if (GameMain.Singleton != null)
            --GameMain.Singleton.NumFishAlive;

        if (GameMain.EvtFishClear != null)
            GameMain.EvtFishClear(this);

        Attackable = false;

        Pool_GameObj.RecycleGO(Prefab_GoAniSwim, mGoAniSprite);
        mGoAniSprite.SetActive(false);
        Transform tsAniSwim = mGoAniSprite.transform;
        tsAniSwim.position = new Vector3(1000F, 0F, 0F);
        tsAniSwim.rotation = Quaternion.identity;
        tsAniSwim.localScale = Vector3.one;

        mGoAniSprite = null;
        mAnimationSprite = null;
 
//         if (!Pool_GameObj.RecycleGO(null, gameObject))
//         {
            Destroy(gameObject);


        //}
    }
 

  

    public void Kill(Player killer,Bullet b,float delayVisiableAnimation)
    {
        if (!Attackable)
            return;
        if (EvtFishKilled != null)
            EvtFishKilled(killer, b, this);

        if (GameMain.EvtFishKilled != null)
            GameMain.EvtFishKilled(killer,b,this);

        Die(killer,delayVisiableAnimation,b.FishOddsMulti);
    }

    void Die(Player killer,float delay,int oddsMulti)
    {
 
        Attackable = false;
        Vector3 deadWorldPos = mTs.position;

        //if (EvtDieStart != null)
        //    EvtDieStart = null;

        //������ײ��
        GetComponent<Collider>().enabled = false;

        //����ԭ����
        //AniSprite.renderer.enabled = false;
        //Destroy(AniSprite.renderer.gameObject);
        //foreach ()
        
        Component[] renderers = GetComponentsInChildren(typeof(Renderer));
        foreach (Component r in renderers)
        {
            ((Renderer)r).enabled = false;
        }

        float delayTotal = delay + TimeDieAnimation;
        //������������
        if (Prefab_GoAniDead != null)
        {

            GameObject goDieAnimation= Pool_GameObj.GetObj(Prefab_GoAniDead);
            goDieAnimation.SetActive(true);
            goDieAnimation.transform.parent = GameMain.Singleton.FishGenerator.transform;
            goDieAnimation.transform.position = new Vector3(mTs.position.x, mTs.position.y, Defines.GlobleDepth_DieFish);
            goDieAnimation.transform.rotation = mTs.rotation;

            RecycleDelay fishRecycleDelay = goDieAnimation.AddComponent<RecycleDelay>();
            fishRecycleDelay.delay = delayTotal;
            fishRecycleDelay.Prefab = Prefab_GoAniDead;
        }


        //�ɱ�
        if (Odds != 0)
            killer.Ef_FlyCoin.FlyFrom(deadWorldPos, oddsMulti * Odds, delayTotal);

        //����
        if(Odds <= 10)
        {
            if(GameMain.Singleton.SoundMgr.snd_Score != null)
                GameMain.Singleton.SoundMgr.PlayOneShot( GameMain.Singleton.SoundMgr.snd_Score[0] );
        }
        else if(Odds > 10 && Odds < 25)
        {
            if(GameMain.Singleton.SoundMgr.snd_Score != null)
                GameMain.Singleton.SoundMgr.PlayOneShot( GameMain.Singleton.SoundMgr.snd_Score[1] );
        }
        else
        {
            if(GameMain.Singleton.SoundMgr.snd_Score != null)
                GameMain.Singleton.SoundMgr.PlayOneShot( GameMain.Singleton.SoundMgr.snd_Score[2] );

        }
        if (Snds_Die != null && Snds_Die.Length != 0)
        {
            GameMain.Singleton.SoundMgr.PlayOneShot(Snds_Die[Random.Range(0, Snds_Die.Length)]);
        }

        //yield return new WaitForSeconds(delayTotal);

        //ɾ��������
        Clear();
        
    } 

    public void ClearAI()
    {
        Component[] fishAIs = GetComponents(typeof(IFishAI));
        for (int i = 0; i != fishAIs.Length; ++i )
        {
            Destroy(fishAIs[i]);
        }
    }


}
