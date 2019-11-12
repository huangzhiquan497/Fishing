using UnityEngine;
using System.Collections;

public class Test_CoroutineCallStackDepth : MonoBehaviour {

	
    void OnGUI()
    {
        if (GUILayout.Button("call"))
        {
            StartCoroutine(_Coro_TestCall());
        }

    }

    IEnumerator _Coro_TestCall()
    {
        yield return 0;
        Debug.Log("_Coro_testCcall");
        StartCoroutine(_Coro_TestCall());
    }

}
