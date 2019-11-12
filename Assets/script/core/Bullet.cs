using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Bullet : MonoBehaviour,IPoolObj {
    
    public static readonly Vector3 ColliderOffsetZ = Vector3.forward * 3000F;

    public float Speed = 1F;
    public int Score = 1;
    public int FishOddsMulti = 1;//鱼赔率再加倍.(用于离子炮X2)
    public tk2dTextMesh Text_CoinNum;
    
    public float RadiusStandardBoom = 0.175F;//爆炸最大半径,标准值: 普通子弹:0.175F, 粒子炮:0.35
    public float ScaleCollider = 0.5F;//网碰撞默认大小,当IsScaleWeb为true是无效,跟gamemain设置有关
    public GameObject Prefab_GoAnisprBullet;//子弹sprite prefab
    public StringSet Prefab_SpriteNameSet;//子弹spr名字集合
    [System.NonSerialized]
    public Player Owner;//所属player
    [System.NonSerialized]
    public Rect MoveArea;//移动区域

    
    
    private Fish mTargetFish;//锁定鱼
    [System.NonSerialized]
    public bool IsLockingFish = false;//是否锁定鱼子弹
    private Vector3 mPosSaved;
    private bool mIsDestroyed = false;
    private Transform mTs;
    private GameObject mBulletGO;
 

    private GameObject mPrefab;

    public bool IsBezierWhenLocking = true;
    private bool mBezierMoveLR;//left = false ; right = true;
    private Vector3 mSLineCurPostion;//跟踪弹当前的位置,用于计算移动百分比
    //private Vector3 mSLineDistance;//跟踪弹方向

    public GameObject Prefab
    {
        get { return mPrefab; }
        set { mPrefab = value; }

    }
    void Awake()
    {
 
        mTs = transform;
    }

    void CopyDataTo(Bullet tar)
    {
        tar.Speed = Speed;
        tar.Score = Score;
        tar.FishOddsMulti = FishOddsMulti;
        tar.RadiusStandardBoom = RadiusStandardBoom;
        //tar.IsScaleWeb = IsScaleWeb;
        tar.ScaleCollider = ScaleCollider;
        tar.mIsDestroyed = mIsDestroyed;
        tar.MoveArea = MoveArea;
    }

    public void On_Reuse(GameObject prefab)
    {
        gameObject.SetActive(true);
        prefab.GetComponent<Bullet>().CopyDataTo(this);
 
    }

    public void On_Recycle()
    {
        gameObject.SetActive(false);
        //gameObject.active = false;
        //gameObject.SetActiveRecursively(false);

        if (mBulletGO != null)
        {
            mBulletGO.SetActive(false);
            Pool_GameObj.RecycleGO(Prefab_GoAnisprBullet, mBulletGO);

            mBulletGO = null;
        }
        --Owner.GunInst.NumBulletInWorld;
        if (Owner.GunInst.NumBulletInWorld < 0)
            Debug.LogError("在场子弹数不能为负" + Owner.GunInst.NumBulletInWorld.ToString());
    }

    public void SelfDestroy()
    {
        if (mIsDestroyed)
            return;

        if (GameMain.EvtBulletDestroy != null)
            GameMain.EvtBulletDestroy(this);

        Pool_GameObj.RecycleGO(null, gameObject);
        mIsDestroyed = true;//Destroy不会立即使得OnTrigger失效,防止多次物理碰撞,
    }
 
    public void Fire(Player from, Fish tar,Quaternion dir)
    {
        GameMain gm = GameMain.Singleton;
        if (tar != null)
        {
            mTargetFish = tar;
            IsLockingFish = true;
            Vector3 targetDir = tar.transform.position - mTs.position;
            targetDir.z = 0F;
            targetDir.Normalize();
            targetDir = Quaternion.Euler(0F, 0F,Random.Range(-90F,90F)) * targetDir  +from.transform.up;//得出子弹偏向  Random.Range(0,2)==0?-90F:90F
            mTs.rotation =   Quaternion.FromToRotation(Vector3.up, targetDir);
            
        }
        else
        {
            IsLockingFish = false;
            mTs.rotation = dir;
        } 
        mTs.parent = gm.TopLeftTs;

        Text_CoinNum.text = Score.ToString();
        Text_CoinNum.Commit();

        Owner = from;
        ++Owner.GunInst.NumBulletInWorld;

        BackStageSetting bss = gm.BSSetting;

        if (bss.IsBulletCrossWhenScreenNet.Val && gm.IsScreenNet())
            MoveArea = gm.WorldDimension;
        else
            MoveArea = from.AtScreenArea;
 
        if (mBulletGO == null)
        {
            mBulletGO = Pool_GameObj.GetObj(Prefab_GoAnisprBullet);
            tk2dSpriteAnimator aniSprBullet = mBulletGO.GetComponent<tk2dSpriteAnimator>();
            if (aniSprBullet != null)
            {
                //aniSprBullet.clipId = aniSprBullet.GetClipIdByName(Prefab_SpriteNameSet.Texts[from.Idx % Prefab_SpriteNameSet.Texts.Length]);
                //Debug.Log(Prefab_SpriteNameSet.Texts[from.Idx % Prefab_SpriteNameSet.Texts.Length]);
                aniSprBullet.Play(Prefab_SpriteNameSet.Texts[from.Idx % Prefab_SpriteNameSet.Texts.Length]);
                //aniSprBullet.PlayFrom("1",0.1F);
            }

            mBulletGO.SetActive(true);
            mBulletGO.transform.parent = mTs;
            mBulletGO.transform.localPosition = Vector3.zero;
            mBulletGO.transform.localRotation = Quaternion.identity; 
            
        }

    }
 

	void Update () {
        if (mIsDestroyed)
            return;

        mPosSaved = mTs.position;
        
        if (IsLockingFish && mTargetFish != null)
        {
            if (!IsBezierWhenLocking)
            {
                Vector3 toward = mTargetFish.transform.position - mTs.position;
                toward.z = 0F;
                toward.Normalize();
                mTs.rotation = Quaternion.FromToRotation(Vector3.up, toward);
            }
            else
            {
                Vector3 toward = mTargetFish.transform.position - mTs.position;
                toward.z = 0F;

                Quaternion quatToTarget = Quaternion.FromToRotation(Vector3.up, toward);
                float limitDistance = 1F / toward.sqrMagnitude;
                if (limitDistance < 1F)
                    limitDistance = 1F;
                mTs.rotation = Quaternion.Slerp(mTs.rotation, quatToTarget, Time.deltaTime * Speed * 0.00651F * limitDistance);
            }
        }

        //if (!IsLockingFish || mTargetFish == null)
            mTs.position += Speed * Time.deltaTime * mTs.up;
        Vector3 curPos = mTs.position;

        if (curPos.y < MoveArea.yMin || curPos.y > MoveArea.yMax)
        {
            //curPos.y = -curPos.y;
            Vector3 dir = mTs.up;
            dir.y = -dir.y;
            mTs.up = dir;
            mTs.position = mPosSaved;
            Vector3 euler = mTs.localEulerAngles;
            euler.y = 180F;
            mTs.localEulerAngles = euler;
            IsLockingFish = false;
        }
        if (curPos.x < MoveArea.xMin || curPos.x > MoveArea.xMax)
        {
            Vector3 dir = mTs.up;
            dir.x = -dir.x;
            mTs.up = dir;
            mTs.position = mPosSaved;
            Vector3 euler = mTs.localEulerAngles;
            euler.y = 180F;
            mTs.localEulerAngles = euler;
            IsLockingFish = false;
        }



	}
    
    void OnTriggerEnter(Collider other)
    {
        if (mIsDestroyed)
            return;
 

        Fish fishFirst = other.GetComponent<Fish>();//被子弹击中的鱼
        if (fishFirst == null)
        {
            Debug.LogError("Fish在这里不可能是null!");
            return;
        }

        if (IsLockingFish && mTargetFish != null && mTargetFish.ID != fishFirst.ID)//锁目标
        {
            return;
        }

        HitProcessor.ProcessHit(this, fishFirst);
    }

}
