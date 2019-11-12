using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Xml;
using System.IO;

public static class HappyFishesMenu_textureCutter{
    //public string OriginPath;
    public static string PathOri_Texture;

    public static string PathTex_Animation;
    [MenuItem("HappyFishes/ResourcesProcess/Cut Textures")]
    static void CutTextures()
    {
        PathOri_Texture = Application.dataPath + "/original/texture";
        PathTex_Animation = EditorUtility.OpenFolderPanel("Load png Textures of Directory", "", ""); //Application.dataPath + "/texture/animation";
        XmlDocument doc = new XmlDocument();
        doc.Load(Application.dataPath + "/original/renderTemplate.cfg");

        XmlNode node = doc.SelectSingleNode("HappyFishes_RenderTemplate");
        int count =2000000000;//测试用
        _Rescusion_ReadNode(node,ref count); 
        
    }



    [MenuItem("HappyFishes/ResourcesProcess/Cut BackGround Textures")]
    static void CutBackGroundTextures()
    {
        
        PathOri_Texture = Application.dataPath + "/original/texture";
        PathTex_Animation = EditorUtility.OpenFolderPanel("Load png Textures of Directory", "", ""); //Application.dataPath + "/texture/animation";
        XmlDocument doc = new XmlDocument();
        doc.Load(Application.dataPath + "/original/render.cfg");

        XmlNode node = doc.SelectSingleNode("HappyFishes_RenderData");
        int count = 2000000000;//测试用
        _Rescusion_ReadNode(node, ref count);
    }

    [MenuItem("HappyFishes/ResourcesProcess/Cut BigGoldKillFish Textures")]
    static void CutBigGoldKillFishTextures()
    {

        PathOri_Texture = Application.dataPath + "/original/texture";
        PathTex_Animation = EditorUtility.OpenFolderPanel("Load png Textures of Directory", "", ""); //Application.dataPath + "/texture/animation";
        XmlDocument doc = new XmlDocument();
        doc.LoadXml("<bigGold   type=\"animation\"    texture=\"BigGold\"  hotSpot=\"0.5 0.5\" firstFrame=\"0 0 100 100\"  fps=\"13\" frames=\"12\" play=\"true\"  grade=\"381\"/>");
        int count = 2000000000;//测试用
        _Rescusion_ReadNode(doc, ref count);
    }

    [MenuItem("HappyFishes/ResourcesProcess/Cut Webs Textures")]
    static void CutWebsTextures()
    {

        PathOri_Texture = Application.dataPath + "/original/texture";
        PathTex_Animation = EditorUtility.OpenFolderPanel("Load png Textures of Directory", "", ""); //Application.dataPath + "/texture/animation";
        XmlDocument doc = new XmlDocument();
        doc.LoadXml("<web   type=\"animation\"    texture=\"IMake_webs2\"  hotSpot=\"0.5 0.5\" firstFrame=\"0 0 200 200\"  fps=\"13\" frames=\"6\" play=\"true\"  grade=\"381\"/>");
        int count = 2000000000;//测试用
        _Rescusion_ReadNode(doc, ref count);
    }

    static void _Rescusion_ReadNode(XmlNode node,ref int count)
    {   
        XmlNodeList nList = node.ChildNodes;

        foreach (XmlNode n in nList)
        {
			XmlAttributeCollection attrs = n.Attributes;
            XmlAttribute attrType = attrs == null ? null : attrs["type"];
         
            if (attrType != null && attrType.Value == "animation")
            { 
                //if (n.Name != "gold")
                //    continue;
				string texOriPath = FindFileAtPathResc(PathOri_Texture,attrs["texture"].Value + ".png");
       
                if (texOriPath == "")
                { 
                    goto tag_ReadNextNode;

                }
                texOriPath = texOriPath.Replace(Application.dataPath, "Assets");
                //Debug.Log("texOriPath = " + texOriPath + "    textureName = " + attrs["texture"].Value + ".png");
                //Texture2D
                Texture2D texOri = (Texture2D)AssetDatabase.LoadAssetAtPath(texOriPath, typeof(Texture2D));
				


				int textureNewNum = int.Parse(attrs["frames"].Value);
                //Debug.Log("textureNewNum = " + textureNewNum);
                //string.Format()

				int[] texDims= Attribute_firstFrame(attrs["firstFrame"].Value);
                //Debug.Log("creating texture :" + n.Name);
                int columMulti = 0;
                int pixelToGetX = texDims[0] ; 
                for(int i = 0; i != textureNewNum; ++i)
                {
                    //创建新texture
                    Texture2D texSub = new Texture2D(texDims[2], texDims[3]);

                    //复制像素
                    //int oneRowCanHoldNum = texOri.width / texDims[2];
                    //int getX = texDims[0]+(i+1)*texDims[2] > texOri.width ? 
                    //Debug.Log(texDims[0] + (i % oneRowCanHoldNum) * texDims[2]);
                    //Debug.Log(texOri.height - texDims[1] - (i / oneRowCanHoldNum) * texDims[3]); 

                    if (pixelToGetX  + texDims[2] > texOri.width)
                    {
                        pixelToGetX = 0;
                        ++columMulti ;
                    }
                    //Debug.Log("pixelToGetX = " + pixelToGetX + "   colMutli = " + columMulti);
                    Color[] colors = texOri.GetPixels(pixelToGetX
                                                    , texOri.height - texDims[1] - columMulti * texDims[3] - texDims[3]//y位置,unity图片00点为左下角.需转换
                                                    ,texDims[2],texDims[3]);

                    pixelToGetX += texDims[2];
                    texSub.SetPixels(colors);
                    texSub.Apply();

                    //存放在Asset目录内
                    //string texSubFloder = Application.dataPath + "/texture/animation/" + n.Name;
                    //if (!Directory.Exists(texSubFloder))
                    //    Directory.CreateDirectory(texSubFloder);


                    byte[] bytes = texSub.EncodeToPNG();
                    System.IO.FileStream fs = System.IO.File.Create(PathTex_Animation+"/" + n.Name +"_"+ i + ".png");
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Close();

                    GameObject.DestroyImmediate(texSub);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                //for (int i = 0; i != textureNewNum; ++i )
                //{
                //    SetUpTargetTexture("Assets/texture/animation/" + n.Name +"/"+ n.Name + i + ".png");
                //}
                //Debug.Log(n.Name);
                count = count - 1;
                if(count < 0 )
                    return ;
            }
        tag_ReadNextNode:
            _Rescusion_ReadNode(n,ref count);

        }
    }

    /// <summary>
    /// 从文件夹内递归找出指定文件的具体目录
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    static string FindFileAtPathResc(string path ,string fileName)
    {
        string[] paths = Directory.GetFiles(path, fileName, SearchOption.AllDirectories);
        //Debug.Log("path = " + path + "   fileName =" + fileName);
        //Debug.Log("paths = " + paths.Length);
		if(paths != null && paths.Length > 0)
			return paths[0];
		
        return "";
    }

    /// <summary>
    /// 属性fistFrame转换为Rect
    /// </summary>
    /// <param name="attriStr"></param>
    /// <returns></returns>
	static int[] Attribute_firstFrame(string attriStr)
    {
        string[] delimiters = {" ","  "};
        string[] spliteds = attriStr.Split(delimiters,System.StringSplitOptions.RemoveEmptyEntries);
       
        int[] outValue = spliteds==null ? null : new int[spliteds.Length];
        for (int i = 0; i != spliteds.Length; ++i)
        {
            //Debug.Log("spliteds["+i+"] = " + spliteds[i]);
            outValue[i] = Mathf.RoundToInt(float.Parse(spliteds[i]));
        }
        return outValue;
    }


    //设置路径纹理为GUI
    static void SetUpTargetTexture(string targetTexPath)
    { 
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(targetTexPath);
        importer.textureType = TextureImporterType.GUI;
        EditorUtility.SetDirty(importer);
        AssetDatabase.ImportAsset(targetTexPath);
    }
}
