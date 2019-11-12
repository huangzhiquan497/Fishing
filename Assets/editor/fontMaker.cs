using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.IO;
public class HappyFishesMenu_makeBGFont : ScriptableObject
{

    [MenuItem("HappyFishes/font/makeBGFont")]
    static void MakeBGFont()
    {
        string[] filePaths = Directory.GetFiles(Application.dataPath + "/original/font");

        foreach (string filepath in filePaths)
        {
            if (Path.GetExtension(filepath) != "")
                continue;

            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);

            foreach (XmlNode n in doc.ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Element)
                {
                    XmlAttribute attrSpace = n.Attributes["space"];
                    int space = 1;
                    if (attrSpace != null)
                        space = int.Parse(attrSpace.Value);

                    Dictionary<char, int[]> oriFontData = new Dictionary<char, int[]>();
                    foreach (XmlNode childNode in n.ChildNodes)
                    {
                        oriFontData.Add(childNode.Attributes["letter"].Value[0], Attribute_Dimension(childNode.InnerText));
                    }
                    GenerateFontProcess(n.Name, oriFontData,1370,466,space);
                }
            }
        }
    }

    [MenuItem("HappyFishes/font/makeSceneFont")]
    static void MakeSceneFont()
    {
        //  <weaponDigit      type="animation"  texture="元素_前景_零碎"    hotSpot="0.5 0.39"  firstFrame="675 64 19 24"     fps="0" frames="10"   grade="363" zorder ="1"/>
        //<bulletDigit      type="animation"  texture="元素_前景_零碎"    hotSpot="0.5 0.5"   firstFrame="675 45 15 19"     fps="0" frames="10"   grade="372"/>
        //<goldPileDigit    type="animation"  texture="元素_前景_零碎"    hotSpot="0.5 0.5"   firstFrame="675 46 15 18"     fps="0" frames="10"   grade="385"/>
        //<creditBoardDigit type="animation"  texture="元素_前景_零碎"    hotSpot="0.5 0.5"   firstFrame="675 88 27 35"     fps="0" frames="10"   grade="382" zorder ="2"/>
        //<highCreditDigit  type="animation"  texture="元素_前景_零碎"    hotSpot="0.5 0.5"   firstFrame="720 280 52 60"    fps="0" frames="10"   grade="353"/>
        //<creditDieFish    type="animation"  texture="元素_前景_零碎1"   hotSpot="0.5 0.5"   firstFrame="0 192.0 42 46"   fps="0" frames="10"   grade="411"/>

        Dictionary<string, int[]> oriFonts = new Dictionary<string, int[]>();
        oriFonts.Add("scence_weaponDigit", new int[] { 675, 64, 19, 24,1687,1221 });//0~1:字体起始x,y坐标;   2~3:单个字体宽高;   4~5:字体使用图片的宽高
        oriFonts.Add("scence_bulletDigit", new int[] { 675, 45, 15, 19, 1687, 1221 });
        oriFonts.Add("scence_goldPileDigit", new int[] { 675, 46, 15, 18, 1687, 1221 });
        oriFonts.Add("scence_creditBoardDigit", new int[] { 675, 88, 27, 35, 1687, 1221 });
        oriFonts.Add("scence_highCreditDigit", new int[] { 720, 280, 52, 60, 1687, 1221 });
        oriFonts.Add("scence_creditDieFish", new int[] { 0, 192, 42, 46 ,973,238});
       

        foreach (KeyValuePair<string,int[]> oriFont in oriFonts)
        {
            Dictionary<char, int[]> oriFontData = new Dictionary<char, int[]>();
            int[] dim = oriFont.Value;
            for (int i = 0; i != 10; ++i )//十个数字 :0~9
            {
                oriFontData.Add(i.ToString()[0], new int[] { dim[0]+i*dim[2],dim[1],dim[2],dim[3] });
            }
            GenerateFontProcess(oriFont.Key, oriFontData,dim[4],dim[5],-2);
        }
    }


    static void GenerateFontProcess(string fntName, Dictionary<char, int[]> oriFontData,int textureWidth,int textureHeight,int fontSpace)
    {
        //Dictionary<char, int[]> oriFontData = new Dictionary<char, int[]>();
        //foreach (XmlNode n in parentN.ChildNodes)
        //{
        //    //Debug.Log(n.InnerText);
        //    oriFontData.Add(n.Attributes["letter"].Value[0], Attribute_Dimension(n.InnerText));
        //}

        //创建字体文件
        string fntInfo = string.Format("info face=\"Arial\" size=32 bold=0 italic=0 charset=\"\" unicode=1 stretchH=100 smooth=1 aa=1 padding=0,0,0,0 spacing=1,1 outline=0\n"
                                     + "common lineHeight={0:d} base=26 scaleW={1:d} scaleH={2:d} pages=1 packed=0 alphaChnl=1 redChnl=0 greenChnl=0 blueChnl=0\n"
                                     + "page id=0 file=\"t1_0.tga\"\n"
                                     + "chars count={3:d}\n", oriFontData['0'][3], textureWidth, textureHeight, oriFontData.Count);
        
        foreach (KeyValuePair<char, int[]> kv in oriFontData)
        {
            fntInfo += string.Format("char id={0:d}   x={1:d}     y={2:d}     width={3:d}   height={4:d}    xoffset={5:d}     yoffset=0    xadvance={6:d}    page=0  chnl=15\n"
                            , (int)kv.Key, kv.Value[0], kv.Value[1], kv.Value[2], kv.Value[3], fontSpace, kv.Value[2] + fontSpace*2);
        }

        string fontDirectory = Application.dataPath + "/prefab/font/" + fntName;
        if (!Directory.Exists(fontDirectory))
            Directory.CreateDirectory(fontDirectory);

        FileStream fs = System.IO.File.Create(fontDirectory + "/" + fntName + ".fnt");
        StreamWriter stw = new StreamWriter(fs);
        stw.Write(fntInfo);
        stw.Close();
        fs.Close();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    /// <summary>
    /// [过期函数]
    /// </summary>
    /// <param name="parentN"></param>
    static void BGFontProcess(XmlNode parentN)
    {
        Dictionary<char, int[]> oriFontData = new Dictionary<char, int[]>();
        foreach (XmlNode n in parentN.ChildNodes)
        {
            //Debug.Log(n.InnerText);
            oriFontData.Add(n.Attributes["letter"].Value[0], Attribute_Dimension(n.InnerText));
        }
        
        //创建字体文件
       string fntInfo =  "info face=\"Arial\" size=32 bold=0 italic=0 charset=\"\" unicode=1 stretchH=100 smooth=1 aa=1 padding=0,0,0,0 spacing=1,1 outline=0\n"
                                    + "common lineHeight=32 base=26 scaleW=1370 scaleH=466 pages=1 packed=0 alphaChnl=1 redChnl=0 greenChnl=0 blueChnl=0\n"
                                    + "page id=0 file=\"t1_0.tga\"\n"
                                    + "chars count=" + oriFontData.Count.ToString() + "\n";
        foreach (KeyValuePair<char,int[]> kv in oriFontData)
        {
            fntInfo += string.Format("char id={0:d}   x={1:d}     y={2:d}     width={3:d}   height={4:d}    xoffset=0     yoffset=0    xadvance={3:d}    page=0  chnl=15\n"
                            , (int)kv.Key, kv.Value[0], kv.Value[1], kv.Value[2], kv.Value[3]);
        }

        string fontDirectory = Application.dataPath + "/prefab/font/" + parentN.Name;
        if (!Directory.Exists(fontDirectory))
            Directory.CreateDirectory(fontDirectory);
 
        FileStream fs = System.IO.File.Create(fontDirectory + "/" + parentN.Name + ".fnt");
        StreamWriter stw = new StreamWriter(fs);
        stw.Write(fntInfo);
        stw.Close();
        fs.Close();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 将字符串转为int[4]
    /// </summary>
    /// <param name="attriStr"></param>
    /// <returns></returns>
    static int[] Attribute_Dimension(string attriStr)
    {
        
        string[] delimiters = { " ", "  " };
        string[] spliteds = attriStr.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);

        //string spliedStr="";

        int[] outValue = spliteds == null ? null : new int[spliteds.Length];
        for (int i = 0; i != spliteds.Length; ++i)
        {
            //spliedStr += "    " + spliteds[i];
            //Debug.Log("spliteds["+i+"] = " + spliteds[i]);
            outValue[i] = Mathf.RoundToInt(float.Parse(spliteds[i]));
        }
        //Debug.Log("ori attribute text = " + attriStr + "  to " + spliedStr);
        return outValue;
    }
}

