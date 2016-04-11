using UnityEngine;
using UnityEditor;
using System.IO;

namespace kode80.Utils
{
	public class ScriptableObjectUtility
	{
		public static void CreateAsset<T>( string fileTypeName = null) where T : ScriptableObject
		{
			T asset = ScriptableObject.CreateInstance<T> ();

			string path = AssetDatabase.GetAssetPath (Selection.activeObject);
			if (path == "") 
			{
				path = "Assets";
			} 
			else if (Path.GetExtension (path) != "") 
			{
				path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
			}

			fileTypeName = fileTypeName ?? typeof(T).ToString();
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/New " + fileTypeName + ".asset");

			ProjectWindowUtil.CreateAsset( asset, assetPathAndName);
		}
	}
}
