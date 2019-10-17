
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.IO;

namespace DTHelperStd
{
    public static class PathHelper
    {
		public static string GetFolderRelativeToProject(string testDataFolder)
		{
			List<string> finalPath = new List<string>();
			string startupPath = System.AppDomain.CurrentDomain.BaseDirectory;
			string drive = startupPath.Substring(0, startupPath.IndexOf(Path.DirectorySeparatorChar) + 1);
			var rootPathItems = startupPath.Split(Path.DirectorySeparatorChar);
			for (int i = 1; i < rootPathItems.Length - 1; i++)
			{
				if (rootPathItems[i].ToLower().Equals("bin"))
				{
					finalPath.RemoveAt(i - 2);
					break;
				}
				finalPath.Add(rootPathItems[i]);
			}

			var relativePathItems2 = testDataFolder.Replace('/', Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
			foreach (var path in relativePathItems2)
				finalPath.Add(path);

			return drive + Path.Combine(finalPath.ToArray());
		}
	}
}

