using UnityEngine;
using System.Collections;

public class Module_Bullet : MonoBehaviour {
    [System.NonSerialized]
    public bool[] Fireable;//Ë÷ÒýÊÇÍæ¼Òid

    public Bullet[] Prefab_BulletNormal;
    public Bullet[] Prefab_BulletLizi;

	void Start () {
        Fireable = new bool[Defines.NumPlayer];
        for (int i = 0; i != Fireable.Length; ++i)
            Fireable[i] = true;
            
        GameMain.EvtPlayerGunFired += Handle_GunFire;
        HitProcessor.Evt_Hit += Handle_BulletHit;
	}

    void Handle_BulletHit(bool isMiss, Player p, Bullet b, Fish f)
    {

 
        if (!isMiss)
        {
            b.SelfDestroy();
        }
            
        
    }

    void Handle_GunFire(Player p, Gun g, int score)
    {
        if (!Fireable[p.Idx])
            return;
        Bullet prefabBullet = Get_PrefabBullet_Used(g);
 
        if(prefabBullet == null)
            return;

        Bullet b = Pool_GameObj.GetObj(prefabBullet.gameObject).GetComponent<Bullet>();

        b.transform.position = g.local_GunFire.position;
        b.Score = score;
        b.Fire(p, null, g.AniSpr_GunPot.transform.rotation);
    }

    public Bullet Get_PrefabBullet_Used(Gun g)
    {
        Bullet prefabBullet;
        if (g.GetPowerType() == GunPowerType.Normal)
        {
            prefabBullet = Prefab_BulletNormal[(int)g.GetLevelType()];
        }
        else if (g.GetPowerType() == GunPowerType.Lizi)
        {
            prefabBullet = Prefab_BulletLizi[(int)g.GetLevelType()];
        }
        else
        {
            return null;
        }
        return prefabBullet;
    }
}
