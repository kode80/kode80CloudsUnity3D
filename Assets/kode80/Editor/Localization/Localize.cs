using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.Localization
{
	public sealed class Localize 
	{
		#region Public

		public static GUIContent GUI( string text, string toolTip=null, string imagePath=null)
		{
			if( imagePath != null) 
			{
				return new GUIContent( text, AssetDatabase.LoadAssetAtPath<Texture2D>( imagePath), toolTip);
			} 

			return new GUIContent( text, toolTip);
		}

		public static string STR( string text, string description=null)
		{
			return text;
		}

		public static Texture2D IMG( string imagePath, string description=null)
		{
			return AssetDatabase.LoadAssetAtPath<Texture2D>( imagePath);
		}

		#endregion

		
		#region Private
		
		private static Localize _instance;
		private static Localize instance { 
			get {
				if (_instance == null) {
					_instance = new Localize ();
				}
				return _instance;
			}
		}
		
		private Localize ()
		{
		}
		
		#endregion
	}
}
