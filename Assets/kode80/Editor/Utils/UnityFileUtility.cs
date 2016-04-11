using UnityEngine;
using System.Collections;

namespace kode80.Utils
{
	public class UnityFileUtility
	{
		public static string AssetRelativePath( string absolutePath)
		{
			string relativePath = null;
			string dataPath = Application.dataPath;

			if( absolutePath.StartsWith( dataPath))
			{
				relativePath = "Assets" + absolutePath.Substring( dataPath.Length);
			}

			return relativePath;
		}
	}
}
