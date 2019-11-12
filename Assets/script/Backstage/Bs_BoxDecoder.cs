using UnityEngine;
using System.Collections;


public class Bs_BoxDecoder : MonoBehaviour {
    public tk2dTextMesh Text_TableNum;//台号
    public tk2dTextMesh Text_DecodeFeatureNum;//解码特征码

    public Ctrl_InputNum Ctrl_DecodeNum;//获得解码条码控件

    public Renderer Rd_ErroInput;//验证错误显示框

    void OnEnable()
    {
        //Rd_ErroInput.enabled = false;
        Rd_ErroInput.gameObject.SetActiveRecursively(false);
        //renderer.enabled = true;
    }
 
    public void ViewErroInput()
    {
        StopCoroutine("_Coro_ViewErroInput");
        StartCoroutine("_Coro_ViewErroInput");
    }

    IEnumerator _Coro_ViewErroInput()
    {
        Rd_ErroInput.enabled = true;
        Rd_ErroInput.gameObject.SetActiveRecursively(true);
        //renderer.enabled = false;
        yield return new WaitForSeconds(3F);
        //Rd_ErroInput.enabled = false;
        Rd_ErroInput.gameObject.SetActiveRecursively(false);
        //renderer.enabled = true;
    }
}
