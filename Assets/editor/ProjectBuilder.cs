using UnityEditor;
using System.IO;
using UnityEngine;

class ProjectBuilder
{
    static void Build()
    {
        
        string[] args = System.Environment.GetCommandLineArgs();
        string par_TargetPath = Directory.GetCurrentDirectory() + "/Release";
        string par_ProjectName = "UnNameProject";
        string scene = "Assets/Scene/main.unity";

        for (int i = 0; i != args.Length; ++i )
        {
            if (args[i].ToLower() == "-targetpath")
            {
                if (i+1 < args.Length)
                {
                    par_TargetPath = args[i + 1];
                }
            }
            else if (args[i].ToLower() == "-projectname")
            {
                if (i + 1 < args.Length)
                {
                    par_ProjectName = args[i + 1];
                }
            }
            else if (args[i].ToLower() == "-scenepath")
            {
                if (i+1 <args.Length)
                {
                    scene = args[i + 1];
                }
            }
        }
  
        //创建目录
        if (!Directory.Exists(par_TargetPath))
        {
            Directory.CreateDirectory(par_TargetPath);
        }


        string[] scenes = { scene };
        //确定目录
        int curProjectIdx = 0;
        string pathToBuild = par_TargetPath + "/" + par_ProjectName + "_"
            + System.DateTime.Now.Year.ToString() + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + "_" + curProjectIdx.ToString();

        while(Directory.Exists(pathToBuild))
        {
            ++curProjectIdx;
            pathToBuild = par_TargetPath + "/" + par_ProjectName + "_"
            + System.DateTime.Now.Year.ToString() + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + "_" + curProjectIdx.ToString();
        }

        //string prjSavePath = Directory.GetCurrentDirectory() + "/Release/" + NameProject +;
 
        BuildPipeline.BuildPlayer(scenes, pathToBuild+"/HpyFish.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
    }

}