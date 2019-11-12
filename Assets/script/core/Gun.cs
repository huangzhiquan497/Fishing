using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {
    //定义
    enum RotateState
    {
        Idle,
        Left,
        Right
    }

    public enum ControlMode
    {
        Manual,//手动控制
        Auto//自动瞄准
    }

    //变量
    public tk2dSpriteAnimator AniSpr_GunPot;
    public Transform TsGun;
    public tk2dTextMesh Text_NeedCoin;

    public Transform local_GunFire;
    public Transform local_EffectGunFire;
    public GameObject Prefab_GunFire;
    public Bullet Prefab_BulletNormal;

    public float RotateSpeed = 100F;//角度/秒
    public float RotateRange = 180F;//角度
    public float Cooldown = 0.5F;//发射冷却
    public float CooldownMultiFix = 1F;//发射冷却调整
    public int NumBulletLimit = 15;//子弹限制
    [System.NonSerialized]
    public GunType GunType_;
    [System.NonSerialized]
    public int NumBulletInWorld = 0;//在场面上的子弹数量
    //public float 

    [System.NonSerialized]
    public bool Fireable = true;//是否可发射
    [System.NonSerialized]
    public bool Rotatable = true;//是否可转动
    public int NumDivide = 1;//子弹分裂数目
    
    public AnimationCurve Curve_GunShakeOffset;
    public AudioClip Snd_Fire;
    public AudioClip Snd_Equip;


    private RotateState mRotateState; 
 

    private float mCooldownRemain = 0F;//冷却剩余时间
    private float mLastFireTime;//最后一次发炮的时间
    private bool mFiring = false; 
    private Player mOwner;
    //public ControlMode ControlMode_
    //{
    //    get
    //    {
    //        return mControlMode;
    //    }
    //}

    //private ControlMode mControlMode;//瞄准模式
    //private Fish mLockedTarget;//锁定的目标

    public bool FastFirable = false;//是否可以快速开火
    public float FastFireSpeedUp = 1.5f;//快速开火状态下 加速百分比
    private static readonly float FastFireAddPrepareNumTime = 0.3F;//快速开火增加PrepareNum数目的时间间隔
    private static readonly int FastFireEnablePrepareNum = 2;//开启快速开火准备次数
    private static readonly float FastFireDuration = 1.5F;//快速开火持续时间

    private int mFastFire_PrepareNum = 0;//快速射击需要储蓄的按键数(在一定时间间隔内按下)
    private float mPreStartFireTime = 0F;//上次触发开火时间
    private bool mIsFastFire = false;//快速开火状态

    private bool mEffect_ShakeCoroLock = false;//效果是否在播放中
    

    public void CopyDataTo(Gun tar)
    {
        tar.RotateSpeed = RotateSpeed;
        tar.RotateRange = RotateRange;
        tar.Cooldown = Cooldown;
        tar.NumBulletLimit = NumBulletLimit;
        tar.NumBulletInWorld = NumBulletInWorld;
        tar.Fireable = Fireable; 
        tar.mRotateState = mRotateState;
        

        tar.mCooldownRemain = mCooldownRemain;
        tar.mLastFireTime = mLastFireTime;
        tar.mFiring = mFiring; 
        tar.mOwner = mOwner;

        //tar.mControlMode = mControlMode;
        tar.Rotatable = Rotatable;
        //tar.mLockedTarget = mLockedTarget;

        tar.mFastFire_PrepareNum = mFastFire_PrepareNum;
        tar.mIsFastFire = mIsFastFire;
        tar.CooldownMultiFix = CooldownMultiFix;
        tar.FastFireSpeedUp = FastFireSpeedUp;
        tar.FastFirable = FastFirable;
    }

    void Start()
    {
        
        mOwner = transform.parent.GetComponent<Player>();
        Text_NeedCoin.text = GameMain.Singleton.BSSetting.Dat_PlayersGunScore[mOwner.Idx].Val.ToString();
        Text_NeedCoin.Commit();

        //StartCoroutine(_Coro_Fire());

        if (mRotateState != RotateState.Idle)
        {
            StartCoroutine("_Coro_Rotate", mRotateState == RotateState.Left ? true:false);
        }

        //if (mControlMode == ControlMode.Auto)
        //{
        //    StartCoroutine(_Coro_Locking());
        //}

        if (mIsFastFire)
        {
            //StopCoroutine("_Coro_StopFastFireDelay");
            StartCoroutine("_Coro_StopFastFireDelay");
        }
    }
    //public void LockAt(Fish t)
    //{
    //    mLockedTarget = t;
    //    mControlMode = ControlMode.Auto;
    //    StopCoroutine("_Coro_Locking");
    //    StartCoroutine("_Coro_Locking");
    //}

    //public void UnLock()
    //{
    //    mLockedTarget = null;
    //    mControlMode = ControlMode.Manual;
    //    StopCoroutine("_Coro_Locking");
    //}

    //IEnumerator _Coro_Locking()
    //{
    //    if (mLockedTarget == null || !mLockedTarget.Attackable)
    //        yield break;
    //    float rotateRrangeHalf = RotateRange * 0.5f;
    //    FishEx_LockPos loclLocal = mLockedTarget.GetComponent<FishEx_LockPos>();
    
    //    Transform tsTarget = mLockedTarget.transform;
    //    while (tsTarget != null)
    //    {

    //        Vector3 upToward = (loclLocal!=null?loclLocal.LockBulletPos:tsTarget.position) - TsGun.position;
    //        upToward.z = 0F;

    //        Quaternion preRotation = TsGun.localRotation; 
    //        TsGun.rotation = Quaternion.Slerp(TsGun.rotation, Quaternion.FromToRotation(Vector3.up, upToward), 10F*Time.deltaTime);

    //        if (TsGun.localEulerAngles.z > rotateRrangeHalf && TsGun.localEulerAngles.z < (360F - rotateRrangeHalf))
    //        {
    //            if (TsGun.localEulerAngles.z < 180F) 
    //            {
    //                TsGun.RotateAroundLocal(Vector3.forward, -1.0F * (TsGun.localEulerAngles.z - rotateRrangeHalf) * Mathf.Deg2Rad);
    //            }
    //            else
    //            {
    //                TsGun.RotateAroundLocal(Vector3.forward, (360F - rotateRrangeHalf - TsGun.localEulerAngles.z) * Mathf.Deg2Rad);
    //            }
    //        }
    //        yield return 0;
    //    }
    //}

    public void StartFire()
    {
        mFiring = true;

        if (FastFirable)
        {
            if (Time.time - mPreStartFireTime < FastFireAddPrepareNumTime)
            {
                ++mFastFire_PrepareNum;

                if (mFastFire_PrepareNum >= FastFireEnablePrepareNum)
                {
                    mIsFastFire = true;
                    CooldownMultiFix = 1F / FastFireSpeedUp;
                    StopCoroutine("_Coro_StopFastFireDelay");
                    StartCoroutine("_Coro_StopFastFireDelay");
                }
            }
            else
            {
                mFastFire_PrepareNum = 0;
            }

            mPreStartFireTime = Time.time;
        }
    }

    /// <summary>
    /// 停止普通开火
    /// </summary>
    public void StopNormalFire()
    {
        mFiring = false;
    }


    /// <summary>
    /// 停止所有开火
    /// </summary>
    public void StopFire()
    {
        mFiring = false;

        mFastFire_PrepareNum = 0;
        mIsFastFire = false;
        CooldownMultiFix = 1F;
        StopCoroutine("_Coro_StopFastFireDelay");
    }

    IEnumerator _Coro_StopFastFireDelay()
    {
        yield return new WaitForSeconds(FastFireDuration);
        mIsFastFire = false;
        CooldownMultiFix = 1F;
    }
    void Update()
    {
        //开火流程
        if (mIsFastFire)
            goto TAG_JUMP_FIRING_JUDGE_UPDATE;

        if (!mFiring)
        {
            return;
        }

    TAG_JUMP_FIRING_JUDGE_UPDATE://跳过mFiring判断


        if (NumBulletInWorld > NumBulletLimit)
        {
            return;
        }

        if (!Fireable)
        {
            return;
        }

        //换枪cd
        //if (mCooldownRemain != 0F)
        //{
        //    //while (mCooldownRemain >= 0F)
        //    if (mCooldownRemain >= 0F)
        //    {
        //        mCooldownRemain -= Time.deltaTime;
        //        //yield return 0;
        //        return;
        //    }
            
        //    mCooldownRemain = 0F;

        //}
        if (Time.time - mLastFireTime < mCooldownRemain)
        {
            return;
        }
        //没币
        int curScore = GameMain.Singleton.BSSetting.Dat_PlayersScore[mOwner.Idx].Val;
        if (curScore <= 0)
        {
            return;
        }
        //**扣钱(跳过该循环逻辑别写在该判断下)
        int useScore = GameMain.Singleton.BSSetting.Dat_PlayersGunScore[mOwner.Idx].Val;

        if (curScore < useScore)
            useScore = curScore;

        if (!mOwner.ChangeScore(-useScore))
        {
            return;
        }

        //动画
        //AniSpr_GunPot.Play(AniSpr_GunPot.clipId, 0F);
        //AniSpr_GunPot.PlayFrom(AniSpr_GunPot.DefaultClip, 0F);
        if(!mEffect_ShakeCoroLock)
            StartCoroutine(_Coro_Effect_GunShake());
        //效果
        GameObject gunfire = Instantiate(Prefab_GunFire) as GameObject;
        gunfire.transform.parent = mOwner.transform;

        gunfire.transform.localScale = local_EffectGunFire.localScale;
        gunfire.transform.position = local_EffectGunFire.position;
        gunfire.transform.rotation = local_EffectGunFire.rotation;

        //音效
        if (Snd_Fire != null)
            GameMain.Singleton.SoundMgr.PlayOneShot(Snd_Fire);

        if (GameMain.EvtPlayerGunFired != null)
            GameMain.EvtPlayerGunFired(mOwner, this, useScore);

        mCooldownRemain = Cooldown * CooldownMultiFix;
        mLastFireTime = Time.time;
        //while (mCooldownRemain >= 0F)
        //{
        //    mCooldownRemain -= Time.deltaTime;
        //    yield return 0;
        //}
        //mCooldownRemain = 0F;
    }
    IEnumerator _Coro_Fire()
    {

        while (true)
        {
            if (mIsFastFire)
                goto TAG_JUMP_FIRING_JUDGE;

            if (!mFiring)
            {
                yield return 0;
                continue;
            }
            
            TAG_JUMP_FIRING_JUDGE://跳过mFiring判断
                

            if (NumBulletInWorld > NumBulletLimit)
            {
                yield return 0;
                continue;
            }

            if (!Fireable)
            {
                yield return 0;
                continue;
            }

            //换枪cd
            if (mCooldownRemain != 0F)
            {
                while (mCooldownRemain >= 0F)
                {
                    mCooldownRemain -= Time.deltaTime;
                    yield return 0;
                }
                mCooldownRemain = 0F;
                
            }
            //没币
            int curScore = GameMain.Singleton.BSSetting.Dat_PlayersScore[mOwner.Idx].Val;
            if (curScore <= 0)
            {
                yield return 0;
                continue;
            }
            //**扣钱(跳过该循环逻辑别写在该判断下)
            int useScore = GameMain.Singleton.BSSetting.Dat_PlayersGunScore[mOwner.Idx].Val;
           
            if (curScore < useScore )
                useScore = curScore;

            if (!mOwner.ChangeScore(-useScore))
            {
                yield return 0;
                continue;
            }

            //动画
            //AniSpr_GunPot.Play(AniSpr_GunPot.clipId,0F);
            //AniSpr_GunPot.PlayFrom(AniSpr_GunPot.DefaultClip,0F);
            StartCoroutine(_Coro_Effect_GunShake());
            //效果
            GameObject gunfire = Instantiate(Prefab_GunFire) as GameObject;
            gunfire.transform.parent = mOwner.transform;

            gunfire.transform.localScale = local_EffectGunFire.localScale;
            gunfire.transform.position = local_EffectGunFire.position;
            gunfire.transform.rotation = local_EffectGunFire.rotation;

            //音效
            if (Snd_Fire != null)
                GameMain.Singleton.SoundMgr.PlayOneShot(Snd_Fire);

            if (GameMain.EvtPlayerGunFired != null)
				GameMain.EvtPlayerGunFired(mOwner,this, useScore);

            mCooldownRemain = Cooldown * CooldownMultiFix;
            while (mCooldownRemain>=0F)
            {
                mCooldownRemain -= Time.deltaTime;
                yield return 0;
            }
            mCooldownRemain = 0F;
        }


    }


    public void AdvanceNeedScore()
    { 

        BackStageSetting bss = GameMain.Singleton.BSSetting;
        int gunScoreCurrent = bss.Dat_PlayersGunScore[mOwner.Idx].Val;
        gunScoreCurrent += bss.ScoreChangeValue.Val;
        if (gunScoreCurrent > bss.ScoreMax.Val)
        {
            gunScoreCurrent = bss.GetScoreMin();
 
        }

        Text_NeedCoin.text = gunScoreCurrent.ToString();
        Text_NeedCoin.Commit();

        bss.Dat_PlayersGunScore[mOwner.Idx].Val = gunScoreCurrent;//记录

        //音效
        if (GameMain.Singleton.SoundMgr.snd_exchangeGun_1 != null)
            GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_exchangeGun_1);
    }

    /// <summary>
    /// 直接设置押分
    /// </summary>
    public void SetNeedScore(int newScore)
    {
        BackStageSetting bss = GameMain.Singleton.BSSetting;

        Text_NeedCoin.text = newScore.ToString();
        Text_NeedCoin.Commit();

        bss.Dat_PlayersGunScore[mOwner.Idx].Val = newScore;//记录

        //音效
        if (GameMain.Singleton.SoundMgr.snd_exchangeGun_1 != null)
            GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_exchangeGun_1);
    }

    public void RotateTo(bool left)
    {
        if (mRotateState == RotateState.Left || mRotateState == RotateState.Right)
            return;

        mRotateState = left ? RotateState.Left:RotateState.Right;
        StartCoroutine("_Coro_Rotate", left);
    }

    public void StopRotate()
    {
        StopCoroutine("_Coro_Rotate");
        mRotateState = RotateState.Idle;
        //if (left && mRotateState == RotateState.Left)
        //{
        //    StopCoroutine("_Coro_Rotate");
        //    mRotateState = RotateState.Idle;
        //}
        //else if (!left && mRotateState == RotateState.Right)
        //{
        //    StopCoroutine("_Coro_Rotate");
        //    mRotateState = RotateState.Idle;
        //}
    }
    public IEnumerator _Coro_Rotate(bool left)
    {
        float direct = left?1F:-1F; 
        float rotateRrangeHalf = RotateRange * 0.5f;
        while (true)
        {
            if (!Rotatable)
            {
                yield return 0;
                continue;
            }
            //if (mControlMode == ControlMode.Auto)
            //{
            //    yield return 0;
            //    continue;
            //}

            //Quaternion preRotation = TsGun.localRotation;

            TsGun.RotateAroundLocal(Vector3.forward, direct * Time.deltaTime * RotateSpeed * Mathf.Deg2Rad);
			if (TsGun.localEulerAngles.z > rotateRrangeHalf && TsGun.localEulerAngles.z < (360F - rotateRrangeHalf))
            {
				if (direct > 0) 
				{
					TsGun.RotateAroundLocal(Vector3.forward, -1.0F * (TsGun.localEulerAngles.z - rotateRrangeHalf) * Mathf.Deg2Rad);
				}
				else
				{
					TsGun.RotateAroundLocal(Vector3.forward, (360F - rotateRrangeHalf - TsGun.localEulerAngles.z) * Mathf.Deg2Rad);
				}
            }
            yield return 0;
        }
    }

    public IEnumerator _Coro_Effect_GunShake()
    {
        mEffect_ShakeCoroLock = true;
        float time = 0.1F;
        float elapse = 0F;
        Transform tsAniGun = AniSpr_GunPot.transform;
        Vector3 oriPos = tsAniGun.localPosition;
        while (elapse <time)
        {
            tsAniGun.localPosition = oriPos + (Curve_GunShakeOffset.Evaluate(elapse/time)) *(tsAniGun.localRotation * Vector3.up ) ;

            elapse += Time.deltaTime;
            yield return 0;
        }
        tsAniGun.localPosition = oriPos;
        mEffect_ShakeCoroLock = false;
    }

    /// <summary>
    /// 获得枪的功能类型
    /// </summary>
    /// <returns></returns>
    public GunPowerType GetPowerType()
    {
        return GunType_<= GunType.NormalQuad ? GunPowerType.Normal : GunPowerType.Lizi;
    }

    public GunLevelType GetLevelType()
    {
        return GunNeedScoreToLevelType(GameMain.Singleton.BSSetting.Dat_PlayersGunScore[mOwner.Idx].Val);
    }
    /// <summary>
    /// 从分数转换到枪的等级类型
    /// </summary>
    /// <returns></returns>
    public static GunLevelType GunNeedScoreToLevelType(int curScore)
    { 
        //int curScore = GameMain.Singleton.BSSetting.Dat_PlayersGunScore[mOwner.Idx].Val;

        //for (int i = 0; i != Defines.ChangeGunNeedScore.Length - 1; ++i)
        //{
        //    if (curScore >= Defines.ChangeGunNeedScore[i] && curScore < Defines.ChangeGunNeedScore[i + 1])
        //    {
        //        return (GunLevelType)i;
        //    }
        //} 
        int idx = 0;
        for (idx = 0; idx != GameMain.Singleton.GunStylesScore.Length; ++idx)
        {
            if (curScore <= GameMain.Singleton.GunStylesScore[idx])
            {
                break;
            }
        }
        
        //Debug.LogError("枪超出ChangeGunNeedScore范围");
        return (GunLevelType)idx;
    }

    public static GunType CombineGunLevelAndPower(GunLevelType glt, GunPowerType gpt)
    {
        switch (glt)
        {
            case GunLevelType.Dbl:
                switch (gpt)
                {
                    case GunPowerType.Normal:
                        return GunType.Normal;
                    case GunPowerType.Lizi:
                        return GunType.Lizi;
                }
                break;
            case GunLevelType.Tri:
                switch (gpt)
                {
                    case GunPowerType.Normal:
                        return GunType.NormalTri;
                    case GunPowerType.Lizi:
                        return GunType.LiziTri;
                }
                break;
            case GunLevelType.Quad:
                switch (gpt)
                {
                    case GunPowerType.Normal:
                        return GunType.NormalQuad;
                    case GunPowerType.Lizi:
                        return GunType.LiziQuad;
                }
                break;

        }
        return GunType.Normal;
    }
}
