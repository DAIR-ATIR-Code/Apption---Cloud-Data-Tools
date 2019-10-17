
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Xunit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataTools;

namespace DataToolsTest
{
    public class ColumnMetaDataTest : DataToolsTest
    {

		Dictionary<string, ColumnMetadata> ColumnMetadata = new Dictionary<string, ColumnMetadata>();

		#region Unit Tests
		
		
		[Fact]
		private void GivenColumns_ThenMetaDataObjectFilled()
		{
			//Dictionary<string, int> columnDetails = new Dictionary<string, int>();
			//using (TextReader fileReader = File.OpenText(CsvWithHeadersPath))
			//{
			//	var csv = new CsvReader(fileReader);
			//	csv.Configuration.HasHeaderRecord = true;
			//	var records = csv.GetRecords<dynamic>().ToList();
			//	var dictionary = records[0] as IDictionary<string, object>;
			//	ColumnMetadata = GetColumnMetadataDictionary(dictionary);
			//	foreach (IDictionary<string, object> dictionaryRow in records)
			//	{
			//		foreach (KeyValuePair<string, object> pair in dictionaryRow)
			//		{
			//			string key = pair.Key;
			//			int valueLength = pair.Value.ToString().Length;
			//			int valueWordCount = CountWords(pair.Value.ToString());
			//			ColumnMetadata[key].TotalLength += valueLength;
			//			ColumnMetadata[key].TotalWords += valueWordCount;
			//			if (ColumnMetadata[key].TotalRecords == 0)
			//			{
			//				ColumnMetadata[key].TotalRecords = records.Count();
			//			}
			//			if (valueLength == 0)
			//			{
			//				ColumnMetadata[key].TotalNulls += 1; 
			//			}
			//			if (ColumnMetadata[key].MaxLength < valueLength)
			//			{
			//				ColumnMetadata[key].MaxLength = valueLength;
			//			}
			//		}
			//	}
			//}
			//Assert.True(ColumnMetadata["description"].MaxLength == 33);
			//Assert.True(ColumnMetadata["serial number"].AverageLength == 16);
		}
		#endregion
	}
}

