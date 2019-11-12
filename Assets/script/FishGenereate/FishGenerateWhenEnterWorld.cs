using UnityEngine;
using System.Collections;

/// <summary>
/// 单进入鱼的创建范围时,创建指定Fish
/// </summary>
/// <remarks>
/// 事件:
///    1.Msg_FishGenerateWhenEnterWorld_Generated(Fish f) 生成鱼
/// 注意:
///    1.用于开头阵列
///    2.创建鱼的同时删除该类
/// 
/// </remarks>
public class FishGenerateWhenEnterWorld : MonoBehaviour {

    public Fish Prefab_Fish;
    public bool IsClearAI = true;
    public float BornDimScaleE = 3F;//todo 有bug,不能小于2,否则生成就会删除,带出鱼机制改进

    public delegate void Evt_FishGenerated(Fish f);
    public Evt_FishGenerated EvtFishGenerated;

    private Rect m_BornDim;
    private Transform mTs;
    void Start()
    {
        mTs = transform;
        m_BornDim.x = GameMain.Singleton.WorldDimension.x - Prefab_Fish.swimmer.BoundCircleRadius * BornDimScaleE;
        m_BornDim.y = GameMain.Singleton.WorldDimension.y - Prefab_Fish.swimmer.BoundCircleRadius * BornDimScaleE;
        m_BornDim.width = GameMain.Singleton.WorldDimension.width + 2F * Prefab_Fish.swimmer.BoundCircleRadius * BornDimScaleE;
        m_BornDim.height = GameMain.Singleton.WorldDimension.height + 2F * Prefab_Fish.swimmer.BoundCircleRadius * BornDimScaleE;
    }

   

	void Update () {
	    if(m_BornDim.Contains(mTs .position))//进入生成区域
        {
            Fish f = Instantiate(Prefab_Fish) as Fish;
            if (IsClearAI)
                f.ClearAI();
            f.swimmer.SetLiveDimension(10000);
            f.AniSprite.playAutomatically = false;
            f.AniSprite.PlayFrom(f.AniSprite.DefaultClip,Time.time);

            f.gameObject.AddComponent<FishDimenSetWhenEnterWorld>();

            f.transform.parent = mTs.parent;
            f.transform.localPosition = new Vector3(mTs.localPosition.x,mTs.localPosition.y,mTs.localPosition.z);
            f.transform.localRotation = mTs.localRotation;
            f.transform.localScale = mTs.localScale;
            SendMessage("Msg_FishGenerateWhenEnterWorld_Generated",f, SendMessageOptions.DontRequireReceiver);
            if (EvtFishGenerated != null)
                EvtFishGenerated(f);

            Destroy(gameObject);

        }
	}

    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "Light Gizmo.tiff", true);
    }
}
