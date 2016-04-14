using UnityEngine;
using System;
using System.Collections;

namespace kode80.Versioning
{
	public class SemanticVersion
	{
		public int Major { get; private set; }
		public int Minor { get; private set; }
		public int Patch { get; private set; }

		public SemanticVersion( int major = 0, int minor = 0, int patch = 0)
		{
			Major = major;
			Minor = minor;
			Patch = patch;
		}

		public static SemanticVersion Parse( string versionString)
		{
			string[] components = versionString.Split( '.');
			int major = 0, minor = 0, patch = 0;

			if( components.Length < 3) { return null; }
			if( int.TryParse( components[0], out major) == false) { return null; }
			if( int.TryParse( components[1], out minor) == false) { return null; }
			if( int.TryParse( components[2], out patch) == false) { return null; }

			return new SemanticVersion( major, minor, patch);
		}

		public static bool operator <( SemanticVersion a, SemanticVersion b)
		{
			return a.Major < b.Major || a.Minor < b.Minor || a.Patch < b.Patch;
		}

		public static bool operator >( SemanticVersion a, SemanticVersion b)
		{
			return a.Major > b.Major || a.Minor > b.Minor || a.Patch > b.Patch;
		}

		public override string ToString()
		{
			return Major + "." + Minor + "." + Patch;
		}
	}
}
