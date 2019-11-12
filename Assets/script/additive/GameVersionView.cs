using UnityEngine;
using System.Collections;

public class GameVersionView : MonoBehaviour
{
    private string GameDevVersion = "-";//¿ª·¢°æ±¾ºÅ
    public tk2dTextMesh PrefabText;
    private tk2dTextMesh[] mViewingText;
	// Use this for initialization

    void Awake( )
    {
        string[] s = System.Environment.CurrentDirectory.Split('\\');
        GameDevVersion = s[s.Length-1];
    }

	void Start () {
        GameMain.EvtInputKey += Handle_InputKey;
        mViewingText = new tk2dTextMesh[GameMain.Singleton.ScreenNumUsing];
        
	}

    void Handle_InputKey( int control, HpyInputKey key, bool down )
    {
        if (down && key == HpyInputKey.BS_Cancel)
        {
            StopCoroutine("_Coro_DelayViewVersion");
            StartCoroutine("_Coro_DelayViewVersion");
        }
        else if (!down && key == HpyInputKey.BS_Cancel)
        {

            StopCoroutine("_Coro_DelayViewVersion");
            for(int i = 0; i < GameMain.Singleton.ScreenNumUsing; i++)
            {
                if(mViewingText[i] != null)
                {
                    Destroy( mViewingText[i].gameObject );
                    mViewingText[i] = null;
                }
            }
        }
    }

    IEnumerator _Coro_DelayViewVersion( )
    {
        yield return new WaitForSeconds( 3F );
        Rect mRWorl = GameMain.Singleton.WorldDimension;

        //Debug.Log( "in" );
        transform.position = new Vector3( mRWorl.x + 0.01F, mRWorl.y , Defines.GlobleDepth_PrepareInBG );
        for(int i = 0; i < GameMain.Singleton.ScreenNumUsing; i++)
        {
            if(PrefabText != null && mViewingText[i] == null)
            {

                mViewingText[i] = Instantiate( PrefabText ) as tk2dTextMesh;
                mViewingText[i].text = GameDevVersion;
                mViewingText[i].Commit( );

                //Debug.Log("transform.position ="+transform.position.x);
                mViewingText[i].transform.parent = transform;
                mViewingText[i].transform.localPosition = new Vector3( mRWorl.width * 0.5F * i, 0F, 0F );
            }
        }
    }

}
