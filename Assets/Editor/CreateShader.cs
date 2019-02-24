using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

public class CreateShader
{
    [MenuItem("Assets/Create/Shader/HLSL Script")]
    static void CreateHLSLFile()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
        ScriptableObject.CreateInstance<CreateHLSLScript>(),
        GetSelectedPathOrFallback() + "/New HLSL.hlsl",   //创建文件初始名
        null,
       "Assets/Editor/Template/NewHLSLTemplate.hlsl");   //模版路径
    }

    public static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }
}

class CreateHLSLScript : EndNameEditAction
{
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        UnityEngine.Object o = CreateScriptAssetFromTemplate(pathName, resourceFile);
        ProjectWindowUtil.ShowCreatedAsset(o);
    }

    internal static UnityEngine.Object CreateScriptAssetFromTemplate(string pathName, string resourceFile)
    {
        string fullPath = Path.GetFullPath(pathName);   //完整路径
        StreamReader streamReader = new StreamReader(resourceFile);
        string text = streamReader.ReadToEnd();
        streamReader.Close();

        string fileName = Path.GetFileNameWithoutExtension(pathName);  //文件名

        //正则替换
        text = Regex.Replace(text, "#DATE#", DateTime.Now.ToShortDateString());
        text = Regex.Replace(text, "#AUTHOR#", "Drift");
        text = Regex.Replace(text, "#FILENAME#", fileName);
        var uppperName = fileName.ToUpper();
        uppperName = Regex.Replace(uppperName, @"\s+", "_");

        text = Regex.Replace(text, "#FILE_DEFINE_INCLUDED#", string.Format("CUSTOM_{0}_INCLUDED", uppperName));

        bool encoderShouldEmitUTF8Identifier = true;
        bool throwOnInvalidBytes = false;
        UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
        bool append = false;
        StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
        streamWriter.Write(text);
        streamWriter.Close();
        AssetDatabase.ImportAsset(pathName);
        return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
    }
}
