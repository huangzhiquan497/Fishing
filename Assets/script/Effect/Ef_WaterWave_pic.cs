using UnityEngine;
using System.Collections;

public class Ef_WaterWave_pic : MonoBehaviour {

    public Texture2D[] Tex_Waterwaves;
    //public Material[] Mtrl_Waterwaves;
    public float Fps = 10F;
    private Renderer mRenderer;
    private int mCurTexIdx = 0;
    private static int MtrlUnitTileX = 4;//һ���������ʵ�Tile
    void Start()
    {
        mRenderer = GetComponent<Renderer>();
        Vector2 texScale = mRenderer.material.mainTextureScale;
        texScale.x = MtrlUnitTileX * GameMain.Singleton.ScreenNumUsing;
        mRenderer.material.mainTextureScale = texScale;

        Vector3 ls = transform.localScale;
        ls.x *= GameMain.Singleton.ScreenNumUsing;

        gameObject.isStatic = false;
        transform.localScale = ls;
        transform.position = new Vector3(0F, 0F, Defines.GlobleDepth_WaterWave);
        gameObject.isStatic = true;
   
    }

    void Update()
    {
        int texIdxNew = (int)(Time.time * Fps) % Tex_Waterwaves.Length;
        if (texIdxNew != mCurTexIdx)
        {
            mCurTexIdx = texIdxNew;
            mRenderer.material.mainTexture = Tex_Waterwaves[mCurTexIdx];
        }

    }
    //public float Fps = 10F;
    //private Renderer mRenderer;

    //private static int MtrlUnitTileX = 4;//һ���������ʵ�Tile

    //IEnumerator Start()
    //{
    //    //foreach (Material m in Mtrl_Waterwaves)
    //    //{
    //    //    Vector2 texScale = m.mainTextureScale;
    //    //    //texScale.x = MtrlUnitTileX*GameMain.Singleton.ScreenNumUsing;
    //    //    m.mainTextureScale = texScale;
    //    //}
    //    mRenderer = renderer;
    //    Vector2 texScale = mRenderer.sharedMaterial.mainTextureScale;
    //    texScale.x = MtrlUnitTileX*GameMain.Singleton.ScreenNumUsing;
    //    mRenderer.sharedMaterial.mainTextureScale = texScale;

    //    Vector3 ls = transform.localScale;
    //    ls.x *= GameMain.Singleton.ScreenNumUsing;

    //    gameObject.isStatic = false;
    //    transform.localScale = ls;
    //    transform.position = new Vector3(0F, 0F, Defines.GlobleDepth_WaterWave);
    //    gameObject.isStatic = true;

    //    int curTexIdx = 0;
    //    int maxTexIdx = Tex_Waterwaves.Length;
        
    //    float waitTime = 1/Fps;
    //    while (true)
    //    {
    //        curTexIdx = (curTexIdx + 1)%maxTexIdx;
    //        //renderer.sharedMaterial.mainTexture = Mtrl_Waterwaves[curTexIdx];
    //        mRenderer.sharedMaterial.mainTexture = Tex_Waterwaves[curTexIdx];
    //        //mRenderer.sharedMaterial = Mtrl_Waterwaves[curTexIdx];

    //        yield return new WaitForSeconds(waitTime);
    //    }
    //}
}
