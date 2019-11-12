using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ef_BubbleManager : MonoBehaviour {
    //public Transform[] Local_BubbleEmitPosition;
    public ParticleSystem Par_Bubble;
    /*//Ð­Í¬º¯Êý´íÎóBUG
	IEnumerator Start () {

        Rect worldDim = GameMain.Singleton.WorldDimension;
        Transform tsPar = Par_Bubble.transform;
	    while (true)
	    {
            tsPar.position = new Vector3(Random.Range(worldDim.xMin,worldDim.xMax)
                ,Random.Range(worldDim.yMin,worldDim.yMax)
                ,Defines.GlobleDepth_SceneBubblePar);
            Vector3 toward = Vector3.zero - tsPar.position+ Random.onUnitSphere * 80F;
            toward.z = 0F;
            tsPar.forward = toward;
            Par_Bubble.Play();

            yield return new WaitForSeconds(Par_Bubble.duration+3F);
	    }
	}
    */

    private float mElapse = 0F;
    private float mEmitInterval;
    void Awake()
    {
        mEmitInterval = Par_Bubble.duration + 3F;
    }
    void Update()
    {
        if (mElapse < mEmitInterval)
        {
            mElapse += Time.deltaTime;
        }
        else
        {
            mElapse = 0F;

            Rect worldDim = GameMain.Singleton.WorldDimension;
            Transform tsPar = Par_Bubble.transform;
            tsPar.position = new Vector3(Random.Range(worldDim.xMin,worldDim.xMax)
                ,Random.Range(worldDim.yMin,worldDim.yMax)
                ,Defines.GlobleDepth_SceneBubblePar);
            Vector3 toward = Vector3.zero - tsPar.position+ Random.onUnitSphere * 80F;
            toward.z = 0F;
            tsPar.forward = toward;
            Par_Bubble.Play();
        }
    }
    //void OnGUI()
    //{
    //    if (GUILayout.Button("stop play"))
    //    {
    //        Par_Bubble.Stop();
    //    }
    //}
}
