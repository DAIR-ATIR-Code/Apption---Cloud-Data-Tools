
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Xunit;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using DataTools;
using DTHelperStd;
using System.Text.RegularExpressions;
using RecognizerTools;
using System.Collections.Concurrent;

namespace DataToolsTest
{
    public class DataToolsTest
    {
		public string CsvWithHeadersPath = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\InventorySheet.csv";
		public string GeneralCsv= $"{PathHelper.GetFolderRelativeToProject("Samples")}\\General.csv";
		public string CsvWithNoHeadersPath = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\LargeFile230.txt";
		public string InventorySheetNoHeadersPath = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\InventorySheetNoHeaders.csv";
        public string DemoFilePath = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\DemoFile.txt";
        public string DemoFileNoHeaderPath = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\DemoFile2.txt";

        public string DatabasePath = $"{PathHelper.GetFolderRelativeToProject("SQLite3")}";
        public string BatchFilePath = $"{PathHelper.GetFolderRelativeToProject("SQLite3")}\\CsvToSql.bat";
        public string DatabaseFilePath = $"{PathHelper.GetFolderRelativeToProject("SQLite3")}\\data.db";



        public static int CountWords(string s)
		{
			MatchCollection collection = Regex.Matches(s, @"[\S]+");
			return collection.Count;
		}		
	}
}

