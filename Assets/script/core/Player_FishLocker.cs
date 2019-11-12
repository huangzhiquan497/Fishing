using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player_FishLocker : MonoBehaviour {
    [System.Serializable]
    public class FishLockPosOffset
    {
        public Fish PrefabFish;
        public Vector3 PosOffsetLock;
    }

    public Transform TsTargeter;
    public tk2dSprite Spr_Target;
    public tk2dSprite Spr_TargetMoving;

    public string NameSprTarget;//ͣ����Ŀ���ϵ���׼����������,��������
    public string NameSprTargetMoving;//��׼���ƶ���������,��������

    public static readonly float MoveSpd = 2F;
    public static Fish[] Prefab_FishLockabe;

    public delegate void Event_ReLock(Fish f,Player p);
    public Event_ReLock EvtRelock;//������

    public Event_ReLock EvtTargetOnFish;//Ŀ���䵽������
    public Event_Generic EvtTargetLeaveFish;//Ŀ���뿪��

    public delegate void Event_Unlock(Player p);
    public Event_Unlock EvtUnlock;

    [System.NonSerialized]
    public bool IsLockable = true;//�Ƿ��ɽ�������

    private int mCurLockIdx;//��ǰ��������,��Prefab_FishLockabe����������
    private float mDepth;//��׼����������(�������������ص�)
    private Rect mStartLockArea;//��ʼ��׼ʱ������
    private Rect mChangeTargetArea;//�ı�Ŀ������


    private Transform mTs;
    private Player mPlayer;
    void Awake()
    {
        GameMain.EvtMainProcess_FinishChangeScene += Handle_FinishChangeScene;
        GameMain.EvtMainProcess_PrepareChangeScene += Handle_PrepareChangeScene;
    }


    void Start()
    {
        if (Prefab_FishLockabe == null)
        {
            Fish[] prefabAllFish = GameMain.Singleton.FishGenerator.Prefab_FishAll;
            List<Fish> tmpLst = new List<Fish>();
            foreach (Fish f in prefabAllFish)
            {
                if (f.IsLockable)
                    tmpLst.Add(f);
            }

            Prefab_FishLockabe = tmpLst.ToArray();
        }

        mTs = transform;
        mPlayer = GetComponent<Player>();


        mDepth = Defines.GlobleDepth_PlayerTargeter + 0.001F * mPlayer.Idx;//����idx����δ��ʼ����
        if (GameMain.Singleton.IsScreenNet()
            && GameMain.Singleton.BSSetting.IsBulletCrossWhenScreenNet.Val)
        {
            mChangeTargetArea = GameMain.Singleton.WorldDimension;
        }
        else
            mChangeTargetArea = mPlayer.AtScreenArea;
        mStartLockArea = mPlayer.AtScreenArea;
        
        Spr_TargetMoving.spriteId = Spr_TargetMoving.GetSpriteIdByName(NameSprTargetMoving + mPlayer.Idx%10);
        Spr_Target.spriteId = Spr_Target.GetSpriteIdByName(NameSprTarget + mPlayer.Idx % 10);
 
        Spr_TargetMoving.GetComponent<Renderer>().enabled = false;
        Spr_Target.GetComponent<Renderer>().enabled = false;

   

    }

    //����Ŀ����
    public void Lock(Fish tarFish)
    {
        if (tarFish == null)
            return;

        if (!IsLockable)
            return;
  

        //�ƶ�����ȥĿ���㴦
        StopCoroutine("_Coro_UnLockProcess");
        StopCoroutine("_Coro_LockProcess");
        StartCoroutine("_Coro_LockProcess", tarFish);
    }

    /// <summary>
    /// ������
    /// </summary>
    /// <returns></returns>
    public Fish Lock()
    {
        if (!IsLockable)
            return null;

        int lockIdxStart = mCurLockIdx;
        Dictionary<int,Fish>[] fishMap = GameMain.Singleton.FishGenerator.FishTypeIndexMap;
        Fish lockFish = null; 
        do 
        {
            Dictionary<int,Fish> tmpFishDict = fishMap[Prefab_FishLockabe[mCurLockIdx].TypeIndex];
            if(tmpFishDict != null
                && tmpFishDict.Count != 0)
            {
                foreach (KeyValuePair<int, Fish> kvp in tmpFishDict)
                {
                    FishEx_LockPos lockLocal = kvp.Value.GetComponent<FishEx_LockPos>();
                    //�жϷ�Χ..todo
                    if (kvp.Value != null 
                        && kvp.Value.Attackable
                        && mStartLockArea.Contains(lockLocal != null ? lockLocal.LockPos : kvp.Value.transform.position)
                        )
                    {
                        lockFish = kvp.Value;
                        mCurLockIdx = (mCurLockIdx + 1) % Prefab_FishLockabe.Length;//���´δӲ�ͬ���㿪ʼ��
                        break;
                    }
                }

                if (lockFish != null)  
                    break; 
            }

            mCurLockIdx = (mCurLockIdx + 1) % Prefab_FishLockabe.Length;
            if(lockIdxStart == mCurLockIdx)
                break;

        } while (true);

        //�Ҳ������Ļ��˳�
        if (lockFish == null)
            return null;


        //�ƶ�����ȥĿ���㴦
        StopCoroutine("_Coro_UnLockProcess");
        StopCoroutine("_Coro_LockProcess");
        StartCoroutine("_Coro_LockProcess",lockFish);

        //GameMain.Singleton.FishGenerator.FishLockable.
        return lockFish;
    }

    public void UnLock()
    {
        if (!IsLockable)
            return;
        if (EvtUnlock != null)
            EvtUnlock(mPlayer);

        StopCoroutine("_Coro_LockProcess");
        StartCoroutine("_Coro_UnLockProcess");
    }

    IEnumerator _Coro_LockProcess(Fish tarFish)
    { 
        Spr_TargetMoving.GetComponent<Renderer>().enabled = true;
        Spr_Target.GetComponent<Renderer>().enabled = false;


        Transform tsTarFish = tarFish.transform;
        //Vector3 offsetLock;
        //mFishPosOffsetBuff.TryGetValue(tarFish.TypeIndex, out offsetLock);
        FishEx_LockPos fishLockLocal = tarFish.GetComponent<FishEx_LockPos>();

        float movePercent = 1F;//vecPercent = 1 - movePercent ^ 2;//mp��������
        Vector3 startPos = TsTargeter.position;
        startPos.z = mDepth;
        
        while (true)
        {
            
      
            if (tarFish == null 
                || !tarFish.Attackable
                || !mChangeTargetArea.Contains(fishLockLocal!=null?fishLockLocal.LockPos:tsTarFish.position))
            {
 
                Fish f = Lock();
                if (f != null)
                { 
                    if (EvtRelock != null)
                        EvtRelock(f, mPlayer);
                }
                else
                {
                    UnLock();
                }
                yield break;
            }


            Vector3 tarVec = (fishLockLocal != null ? fishLockLocal.LockPos : tsTarFish.position) - startPos;
            tarVec.z = 0F;

            float vecPercent = 1F - movePercent*movePercent;
            movePercent -= MoveSpd * Time.deltaTime;

            TsTargeter.position = startPos + tarVec * vecPercent;

            if (movePercent < 0F)
            { 
                Spr_TargetMoving.GetComponent<Renderer>().enabled = false;
                Spr_Target.GetComponent<Renderer>().enabled = true;
                break;
            }
            yield return 0;
        }
        //Debug.Log("_Coro_LockProcess2" + tarFish.name);
        Vector3 tmpVec3;
        
        if (EvtTargetOnFish != null)
            EvtTargetOnFish(tarFish,mPlayer);
        //Quaternion rotateOri = Quaternion.AngleAxis(180F, Vector3.forward);
        float processPercent = 0F;
        Transform tsSpr_Target = Spr_Target.transform;
        while (true)
        {
            if (tarFish == null 
                || !tarFish.Attackable
                || !mChangeTargetArea.Contains(fishLockLocal != null ? fishLockLocal.LockPos : tsTarFish.position))
            {
                Fish f = Lock();
                if (f != null)
                {
                    if (EvtTargetLeaveFish != null)
                        EvtTargetLeaveFish();
                    if (EvtRelock != null)
                        EvtRelock(f, mPlayer);
                }
                else
                {
                    UnLock();
                }
                yield break;
            }

            tmpVec3 = fishLockLocal != null ? fishLockLocal.LockPos : tsTarFish.position;
            tmpVec3.z = mDepth;
            TsTargeter.position = tmpVec3;

            tsSpr_Target.rotation = Quaternion.AngleAxis( Mathf.Cos(Mathf.PI * 2F * processPercent)*25F, Vector3.forward);
            processPercent += Time.deltaTime * 0.5F;
             

            yield return 0;
        }

    }

    IEnumerator _Coro_UnLockProcess()
    {
        
        if (EvtTargetLeaveFish != null)
            EvtTargetLeaveFish();

        if (Spr_Target.GetComponent<Renderer>().enabled)
            Spr_TargetMoving.GetComponent<Renderer>().enabled = true;
        Spr_Target.GetComponent<Renderer>().enabled = false; 
        
        float movePercent = 1F;//vecPercent = 1 - movePercent ^ 2;//mp��������

      
        Vector3 tmpPos;
        Vector3 tarVec = TsTargeter.position - mTs.position;
        tarVec.z = 0F;
        while (true)
        {
            float vecPercent = movePercent *  movePercent;
            movePercent -= MoveSpd * Time.deltaTime;

            tmpPos = mTs.position + tarVec * vecPercent;
            tmpPos.z = Defines.GlobleDepth_PlayerTargeter;
            TsTargeter.position = tmpPos; 

            if (movePercent < 0F) 
            {
                yield return 0;
                TsTargeter.position = mTs.position;
                Spr_TargetMoving.GetComponent<Renderer>().enabled = false;
                Spr_Target.GetComponent<Renderer>().enabled = false;
                break;
            }
            yield return 0;
        }

        
    }

    void Handle_FinishChangeScene()
    {
        IsLockable = true;
    }

    void Handle_PrepareChangeScene()
    {
        
        UnLock();
        IsLockable = false; 
    }
}
