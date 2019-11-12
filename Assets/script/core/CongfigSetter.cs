using UnityEngine;
using System.Xml;
using System.IO;
using System.Security.Cryptography;


public class CongfigSetter : MonoBehaviour
{
    public GainAdjuster GainAdjusterRef;
    readonly string KeyCrypt = "MMyidingyaochangMMyidingyaochang";//32字节
    readonly string IVCrypt = "IVyidingyaochang";//16字节

	void Awake () 
    {
        string fileConfigPath = Directory.GetCurrentDirectory() + "/config";
        if (File.Exists(fileConfigPath))
        {
            ReadAndParseFile(fileConfigPath);
        }
        Destroy(this);
	}

    void ReadAndParseFile(string fullpath)
    {
        if (!File.Exists(fullpath))
            return;

        //解密操作
        string plainText = "";
        using (FileStream encryptDataFs = new FileStream(fullpath, FileMode.Open))
        {
            plainText = DecryptStringFromBytes_Aes(encryptDataFs,
                System.Text.Encoding.ASCII.GetBytes(KeyCrypt),
                System.Text.Encoding.ASCII.GetBytes(IVCrypt));
        }


        //xml操作
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(plainText);

        XmlNode root = xmlDoc.SelectSingleNode("GameConfig");
        XmlNodeList dataNodes = root.ChildNodes;

        //控件赋值
        int tmpI = 0;
        float tmpF = 0F;
        foreach (XmlElement dataNode in dataNodes)
        {
            XmlAttribute tmpAttr = null;

            tmpAttr = dataNode.Attributes["MainVersion"];//主版本号
            if (tmpAttr != null)
            {
                if (int.TryParse(tmpAttr.Value, out tmpI))
                {
                    GameMain.MainVersion = tmpI;
                } 
            }

            tmpAttr = dataNode.Attributes["SubVersion"];//副版本号
            if (tmpAttr != null)
            {
                if (int.TryParse(tmpAttr.Value, out tmpI))
                {
                    GameMain.SubVersion = tmpI;
                }
            }

            tmpAttr = dataNode.Attributes["IDLine"];//线号
            if (tmpAttr != null)
            {
                if (int.TryParse(tmpAttr.Value, out tmpI))
                {
                    GameMain.Singleton.BSSetting.Dat_IdLine.Val = tmpI;
                }
            }

            tmpAttr = dataNode.Attributes["IDTable"];//台号
            if (tmpAttr != null)
            {
                if (int.TryParse(tmpAttr.Value, out tmpI))
                {
                    GameMain.Singleton.BSSetting.Dat_IdTable.Val = tmpI;
                }
            }

            tmpAttr = dataNode.Attributes["DifficultFactor"];//难度系数
            if (tmpAttr != null)
            {
                if (int.TryParse(tmpAttr.Value, out tmpI))
                {
                    if (GameOdds.DifficultFactor == 1F)//初始化顺序无关，如果未赋值才根据配置文件改变
                        GameOdds.DifficultFactor = tmpI;
 
                }
            }

            tmpAttr = dataNode.Attributes["ScoreUpLimit"];//爆机
            if (tmpAttr != null)
            {
                if (int.TryParse(tmpAttr.Value, out tmpI))
                {
                    if (tmpI > 0)
                        Defines.NumScoreUpMax = tmpI;
                }
            }
            
            tmpAttr = dataNode.Attributes["GiftCoinSmall"];//炒场 小
            if (tmpAttr != null)
            {
                if (int.TryParse(tmpAttr.Value, out tmpI))
                {
                    if (GainAdjusterRef != null)
                        GainAdjusterRef.SetGiftCoin(tmpI, -1, -1);
                }
            }
            tmpAttr = dataNode.Attributes["GiftCoinMedium"];//炒场 中
            if (tmpAttr != null)
            {
                if (int.TryParse(tmpAttr.Value, out tmpI))
                {
                    if (GainAdjusterRef != null)
                        GainAdjusterRef.SetGiftCoin(-1, tmpI, -1);
                }
            }
            tmpAttr = dataNode.Attributes["GiftCoinLarge"];//炒场 大
            if (tmpAttr != null)
            {
                if (int.TryParse(tmpAttr.Value, out tmpI))
                {
                    if (GainAdjusterRef != null)
                        GainAdjusterRef.SetGiftCoin(-1, -1, tmpI);
                }
            }
            tmpAttr = dataNode.Attributes["GameIdx"];//游戏序号
            if (tmpAttr != null)
            {
                if (int.TryParse(tmpAttr.Value, out tmpI))
                {
                    GameMain.GameIdx = tmpI;
                }
            }

            //赔率设置
            tmpAttr = dataNode.Attributes["GainRatios_Easiest"];
            if (tmpAttr != null)
            {
                if (float.TryParse(tmpAttr.Value, out tmpF))
                {
                    GameOdds.GainRatios[(int)GameDifficult.VeryEasy] = tmpF;
                }
            }

            tmpAttr = dataNode.Attributes["GainRatios_Easy"];
            if (tmpAttr != null)
            {
                if (float.TryParse(tmpAttr.Value, out tmpF))
                {
                    GameOdds.GainRatios[(int)GameDifficult.Easy] = tmpF;
                }
            }

            tmpAttr = dataNode.Attributes["GainRatios_Hard"];
            if (tmpAttr != null)
            {
                if (float.TryParse(tmpAttr.Value, out tmpF))
                {
                    GameOdds.GainRatios[(int)GameDifficult.Hard] = tmpF;
                }
            }

            tmpAttr = dataNode.Attributes["GainRatios_VeryHard"];
            if (tmpAttr != null)
            {
                if (float.TryParse(tmpAttr.Value, out tmpF))
                {
                    GameOdds.GainRatios[(int)GameDifficult.VeryHard] = tmpF;
                }
            }

            tmpAttr = dataNode.Attributes["GainRatios_Hardest"];
            if (tmpAttr != null)
            {
                if (float.TryParse(tmpAttr.Value, out tmpF))
                {
                    GameOdds.GainRatios[(int)GameDifficult.DieHard] = tmpF;
                }
            }
        }

    }

    static string DecryptStringFromBytes_Aes(Stream cipherStream, byte[] Key, byte[] IV)
    {
        // Check arguments. 
        //if (cipherText == null || cipherText.Length <= 0)
        //    throw new ArgumentNullException("cipherText");
        if (Key == null || Key.Length <= 0)
            throw new System.Exception("Key");
        if (IV == null || IV.Length <= 0)
            throw new System.Exception("Key");

        // Declare the string used to hold 
        // the decrypted text. 
        string plaintext = null;

        // Create an AesCryptoServiceProvider object 
        // with the specified key and IV. 
        using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decrytor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption. 
            //using (Stream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(cipherStream, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        // Read the decrypted bytes from the decrypting stream 
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }

        }

        return plaintext;

    }
}
