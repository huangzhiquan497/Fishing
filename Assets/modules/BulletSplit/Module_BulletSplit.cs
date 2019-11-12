using UnityEngine;
using System.Collections;

/// <summary>
/// 子弹分裂
/// </summary>
public class Module_BulletSplit : MonoBehaviour {
    public Module_Bullet ModuleBullet;
    public float[] WidthBulletNormal;//子弹宽度(索引是N管炮)
    public float[] WidthBulletLizi;//子弹宽度(索引是N管炮)

    public StringSet nameSetAniBulletNor;//子弹精灵名字(索引是N管炮)
    public StringSet nameSetAniBulletLizi;//子弹精灵名字(索引是N管炮)

    public GameObject Prefab_AniBullet;
    
	// Use this for initialization
	void Start () {
        if (nameSetAniBulletLizi == null || nameSetAniBulletNor == null || Prefab_AniBullet == null)
        {
            Debug.LogError("Module_BulletSplit有变量未赋值");
            return;
        }
        for (int i = 0; i != ModuleBullet.Fireable.Length; ++i)
        {
            ModuleBullet.Fireable[i] = false;
        }
        GameMain.EvtPlayerGunFired += Handle_GunFire;
        HitProcessor.AddFunc_Odd(Func_GetFishDieRatioAddtive, null);
	}

    void Handle_GunFire(Player p, Gun g, int score)
    {
        //Debug.Log("Handle_GunFire");
        Bullet prefabBullet = ModuleBullet.Get_PrefabBullet_Used(g);
        //tk2dAnimatedSprite ani;
   
        if (prefabBullet == null)
            return;
        GunLevelType gLvType = g.GetLevelType();
        GunPowerType gPowerType = g.GetPowerType();
        int NumInstance = 2 + (int)gLvType;//生成的数量
        float widthBullet = WidthBulletNormal[(int)g.GetLevelType()];
        Vector3 posOffset = new Vector3(-widthBullet * NumInstance / 2F, 0F);
        for (int i = 0; i != NumInstance; ++i)
        {
            Bullet b = Pool_GameObj.GetObj(prefabBullet.gameObject).GetComponent<Bullet>();
            b.Prefab_GoAnisprBullet = Prefab_AniBullet;
            b.Prefab_SpriteNameSet = gPowerType == GunPowerType.Normal ? nameSetAniBulletNor : nameSetAniBulletLizi;
 
            BulletEx_Splitor bEx_Splitor = b.gameObject.AddComponent<BulletEx_Splitor>();
            bEx_Splitor.FactorSplit = NumInstance;

            b.transform.position = g.local_GunFire.position + g.AniSpr_GunPot.transform.rotation * posOffset;
            posOffset.x += widthBullet;
            b.Score = score;
            b.Fire(p, null, g.AniSpr_GunPot.transform.rotation);
        }
        
    }

    HitProcessor.OperatorOddFix Func_GetFishDieRatioAddtive(Player killer, Bullet b, Fish f,Fish fCauser)
    {
        BulletEx_Splitor bSplitor = b.GetComponent<BulletEx_Splitor>();
        if (bSplitor == null)
        {
            return null;
        }
        else
        {
            return new HitProcessor.OperatorOddFix(HitProcessor.Operator.LastModule, bSplitor.FactorSplit);
        }

        
    }
}
