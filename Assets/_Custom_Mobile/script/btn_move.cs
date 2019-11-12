using UnityEngine;
using System.Collections;

public class btn_move : MonoBehaviour
{
    float startTime;
    float delayTime = 0;
    public int dir = 1;

    float x = 125;

    public int btnID;
    bool musicOn = true;
    bool EffectOn = true;
    // Use this for initialization
    void Start()
    {

        if (btnID == 1 && GameMain.Singleton.BSSetting.Dat_BGMVolum.Val == 0.0f)
        {

            float z = gameObject.transform.localPosition.z;

            gameObject.transform.localPosition = new Vector3(35, 100, z);
            dir = 0;
            x = 35;
            musicOn = false;
        }
        if (btnID == 2 && GameMain.Singleton.BSSetting.Dat_SoundVolum.Val == 0.0f)
        {
            float z = gameObject.transform.localPosition.z;

            gameObject.transform.localPosition = new Vector3(35, 24, z);
            dir = 0;
            x = 35;
            EffectOn = false;
        }
    }

    //IEnumerator Move(Vector3 origin,Vector3 target)
    //{
    //}
    IEnumerator Move(Vector3 target, float moveTime)
    {
        yield return 0;
    }

    IEnumerator move(float moveTime)
    {
        float y = gameObject.transform.localPosition.y;
        float z = gameObject.transform.localPosition.z;

        //yield return new WaitForSeconds(delayTime);
        switch (dir)
        {
            case 0:
                while (moveTime + startTime > Time.time)
                {
                    gameObject.transform.localPosition = new Vector3(x, y, z);
                    x += 90 / (moveTime / Time.deltaTime);
                    yield return 0;
                }
                x = 125;
                gameObject.transform.localPosition = new Vector3(x, y, z);
                dir = 1;
                //MobileInterface.TurnSound(false);
                break;
            case 1:
                while (moveTime + startTime > Time.time)
                {
                    gameObject.transform.localPosition = new Vector3(x, y, z);
                    x -= 90 / (moveTime / Time.deltaTime);
                    yield return 0;
                }
                x = 35;
                gameObject.transform.localPosition = new Vector3(x, y, z);
                dir = 0;
                //  MobileInterface.TurnSound(true);
                break;
        }
    }
    void OnMouseDown()
    {
        startTime = Time.time + delayTime;
        StartCoroutine(move(0.1f));
        switch (btnID)
        {
            case 1:
                wiipay.EvtLog("Game_Setting_Music");
                musicOn = !musicOn;
               // Debug.Log(btnID);
                MobileInterface.TurnSound(musicOn, true);
                break;
            case 2:
                wiipay.EvtLog("Game_Setting_Effect");
                EffectOn = !EffectOn;
                MobileInterface.TurnSound(EffectOn, false);
                //   Debug.Log(btnID);
                break;
            case 3:
                wiipay.EvtLog("Game_Setting_Notice");
                //   Debug.Log(btnID);
                break;
            case 4:
                wiipay.EvtLog("Game_Setting_Prompt");
                //  Debug.Log(btnID);
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
