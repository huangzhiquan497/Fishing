using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{ 
    public AudioClip snd_AddScore;//�Ϸ� 
    public AudioClip snd_exchangeGun_1;  //����
    public AudioClip[] snd_Score; //����
    public AudioClip snd_Coin;//����
    public AudioClip snd_Gold;
    public AudioClip snd_bkBtn;//��̨������
    public AudioClip snd_Spindrift;//ת���˻�

    public AudioClip snd_Open;//����
    public AudioClip snd_Open_Lizi;//



    public AudioClip[] bgms;
    public AudioClip snd_GetLiziCard;

    public AudioSource AudioSrc_Bgm;

    public UI_VolumeViewer Prefab_SoundViewer;
    //public tk2dSprite Prefab_Spr_VolSettingBG;
    //public tk2dSprite Prefab_Spr_VolSettingTile;

    private List<AudioClip> mBgmsNeedPlay;
    //private int mNumPlayingSounds;
    //private Dictionary<int, int> mSoundPlayingNum;//key:����instance id,value:�������ڲ��ŵ���Ŀ
    //private static readonly int NumPlayingSoundSameTime = 3;
    //private tk2dSprite[] mSprVolSettingTiles;//������,���������Ƿ����ڵ���������С
    
    //private static readonly int NumVolumeTile = 16;//������ʾ����������
    void Awake()
    {
        GameMain.EvtInputKey += HandleBackGroundKeyEvent;
        GameMain.EvtBulletDestroy += Handle_BulletDestroy;
        //mSoundPlayingNum = new Dictionary<int,int>();
    }
    void Handle_BulletDestroy(Bullet b)
    {
        if (b.FishOddsMulti == 2)
            PlayOneShot(snd_Open_Lizi);
        else
            PlayOneShot(snd_Open);
    }

	void Start () {
       
        if (GetComponent<AudioSource>() == null)
            gameObject.AddComponent<AudioSource>();

        mBgmsNeedPlay = new List<AudioClip>();
        foreach (AudioClip ac in bgms)
            mBgmsNeedPlay.Add(ac);
        AudioSrc_Bgm.priority = 200;
        GetComponent<AudioSource>().priority = 10;
        GetComponent<AudioSource>().volume = GameMain.Singleton.BSSetting.Dat_SoundVolum.Val;
        AudioSrc_Bgm.volume = GameMain.Singleton.BSSetting.Dat_BGMVolum.Val;
        //audio.clip = testAudioClip;
        //audio.loop = false;
        //audio.Play();

        //tk2dSprite sprBG = Instantiate(Prefab_Spr_VolSettingBG) as tk2dSprite;//-0.35
        //sprBG.transform.parent = GameMain.Singleton.transform;
        //sprBG.transform.localPosition = new Vector3(0F, 0.0471406F, 0.5F);
        //float startLocalPosX = -0.348F;
        //float advanceX = 0.0465F;
        //for (int i = 0; i != 16; ++i )
        //{
        //    tk2dSprite sprTitle = Instantiate(Prefab_Spr_VolSettingTile) as tk2dSprite;
        //    sprTitle.transform.parent = sprBG.transform;
        //    sprTitle.transform.localPosition = new Vector3(startLocalPosX + advanceX * i, 0F, -0.02F);
        //}

	}

    /// <summary>
    /// �ı�����,ֻ��Ϊ+/- 1
    /// </summary>
    /// <param name="vec"></param>
    void ChangeVol(int vec)
    {
        SetVol(GameMain.Singleton.BSSetting.Dat_SoundVolum.Val + 1F / Defines.SoundVolumeLevelNum * vec);

        //��������
        // BackStageSetting bss = GameMain.Singleton.BSSetting;
        // bss.Dat_SoundVolum.Val = bss.Dat_SoundVolum.Val + 1F / Defines.SoundVolumeLevelNum * vec;

        //if (bss.Dat_SoundVolum.Val < 0F)
        //    bss.Dat_SoundVolum.Val = 0F;
        //else if (bss.Dat_SoundVolum.Val > 1F)
        //    bss.Dat_SoundVolum.Val = 1F;

        //audio.volume = AudioSrc_Bgm.volume =  bss.Dat_SoundVolum.Val;

        //if (GameMain.EvtSoundVolumeChanged != null)
        //    GameMain.EvtSoundVolumeChanged(bss.Dat_SoundVolum.Val);
    }

    public void SetVol(float percent)
    {
        BackStageSetting bss = GameMain.Singleton.BSSetting;
        

        if (percent < 0F)
            percent = 0F;
        else if (percent > 1F)
            percent = 1F;

        if (percent == bss.Dat_SoundVolum.Val
            &&percent == bss.Dat_BGMVolum.Val)
            return;
        

        bss.Dat_SoundVolum.Val = percent;
        bss.Dat_BGMVolum.Val = percent;
        
        GetComponent<AudioSource>().volume = AudioSrc_Bgm.volume = percent;

        if (GameMain.EvtSoundVolumeChanged != null)
            GameMain.EvtSoundVolumeChanged(percent);
    }


    public void SetVol(bool bgmOrEffect,float percent)
    {
        BackStageSetting bss = GameMain.Singleton.BSSetting;


        if (percent < 0F)
            percent = 0F;
        else if (percent > 1F)
            percent = 1F;

        
        if (bgmOrEffect)
        {
            if (percent == bss.Dat_BGMVolum.Val)
                return;
            bss.Dat_BGMVolum.Val = percent;
            AudioSrc_Bgm.volume = percent;
        }
        else
        {
            if (percent == bss.Dat_SoundVolum.Val)
                return;

            bss.Dat_SoundVolum.Val = percent;
            GetComponent<AudioSource>().volume = percent;
        }

        if (GameMain.EvtSoundVolumeChanged != null)
            GameMain.EvtSoundVolumeChanged(percent);
    }

    IEnumerator _Coro_ChangeVol(int vec)
    {
        while (true)
        {
            ChangeVol(vec);
            yield return new WaitForSeconds(Defines.TimeBackGroundJumpSelect);
        }
    }
    void HandleBackGroundKeyEvent(int control, HpyInputKey key,bool down)
    {
        //����Ϸ��
        if (GameMain.Singleton.IsInBackstage)
            return;

        if (down && key == HpyInputKey.BS_Left)
            StartCoroutine("_Coro_ChangeVol", -1);
        else if (down && key == HpyInputKey.BS_Right)
            StartCoroutine("_Coro_ChangeVol", 1);
        else if (!down && key == HpyInputKey.BS_Left)
            StopCoroutine("_Coro_ChangeVol");
        else if (!down && key == HpyInputKey.BS_Right)
            StopCoroutine("_Coro_ChangeVol");

    }


    public void PlayOneShot(AudioClip clip)
    {
//         int playingNum = 0;
//         if (!mSoundPlayingNum.TryGetValue(clip.GetInstanceID(), out playingNum))
//         {
//             mSoundPlayingNum.Add(clip.GetInstanceID(), 1);
//         }
//         else//���ڸ�����
//         {
//             if (playingNum >= NumPlayingSoundSameTime)
//             {
//                 return;
//             }
//             else
//             {
//                 mSoundPlayingNum[clip.GetInstanceID()] = playingNum + 1;
//             }
// 
//         }
        GetComponent<AudioSource>().PlayOneShot(clip);  
        //StartCoroutine(_Coro_ReduceNumPlayingSoundEnd(clip.length,clip.GetInstanceID()));
    }
    //IEnumerator _Coro_ReduceNumPlayingSoundEnd(float delay,int instanceID)
    //{
    //    yield return new WaitForSeconds(delay);
    //    --mSoundPlayingNum[instanceID];
    //    Debug.Log(mSoundPlayingNum[instanceID]);
    //}
    //�����µı�������
    public void PlayNewBgm()
    {
        AudioSrc_Bgm.Stop();
        StopCoroutine("_Coro_BgmProcess");
        StartCoroutine("_Coro_BgmProcess" );
    }


    public void StopBgm()
    {
        StopCoroutine("_Coro_BgmProcess");
        AudioSrc_Bgm.Stop();
    }
    //void OnGUI()
    //{
    //    if (GUILayout.Button("play new sound"))
    //    {
    //        PlayNewBgm();
    //    }
    //}

    IEnumerator _Coro_BgmProcess()
    {
        yield return 0;
        while (true)
        {
            if (mBgmsNeedPlay.Count == 0)
            {
                foreach (AudioClip ac in bgms)
                {
                    mBgmsNeedPlay.Add(ac);
                }
            }
            //Debug.Log("player new bgm");
            int IdxToPlay = Random.Range(0, mBgmsNeedPlay.Count);
            AudioSrc_Bgm.clip = mBgmsNeedPlay[IdxToPlay];
            mBgmsNeedPlay.RemoveAt(IdxToPlay);
            AudioSrc_Bgm.loop = false;
            AudioSrc_Bgm.Play();

            yield return new WaitForSeconds(AudioSrc_Bgm.clip.length);
        }
    }
}
