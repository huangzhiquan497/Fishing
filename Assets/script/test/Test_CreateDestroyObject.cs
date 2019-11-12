using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Test_CreateDestroyObject : MonoBehaviour {
    public tk2dAnimatedSprite Prefab_testObj;
    private Stack<tk2dAnimatedSprite> mSprStack;
    void OnGUI()
    {
        if (GUILayout.Button("create"))
        {
            if (mSprStack == null)
                mSprStack = new Stack<tk2dAnimatedSprite>();
           mSprStack.Push(Instantiate(Prefab_testObj) as tk2dAnimatedSprite);
        }
        if (GUILayout.Button("destroy"))
        {
            if (mSprStack == null)
                mSprStack = new Stack<tk2dAnimatedSprite>();
            if (mSprStack.Count != 0)
            {
                Destroy(mSprStack.Pop().gameObject);
            }
            
            
        }
    }
}
