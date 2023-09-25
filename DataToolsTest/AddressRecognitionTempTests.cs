
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using Xunit;
using Xunit.Abstractions;
using CsvHelper;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DataTools;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;
using DTHelperStd;
using RecognizerTools;
using System.Globalization;

namespace DataToolsTest
{    
    public class AddressRecognitionTempTests
    {
        //private string MockCsvPath = @"C:\Users\dzhao\source\repos\Cloud Data Platform\DataTools\Samples\clean_mock_data.csv";
        private string EnvironicsCsvPath = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\environics.csv";
        //private string EnvironicsDirtyFilePath = @"C:\Users\dzhao\source\repos\Cloud Data Platform\DataTools\Samples\environics-dirty.csv";
        private string EnvironicsDirtierFilePath = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\environics-dirtier.csv";
		private string CityFilePath = $"{PathHelper.GetFolderRelativeToProject("DataTools\\Reference Data")}\\cdncity.csv";
		private string CityFilePath2 = $"{PathHelper.GetFolderRelativeToProject("DataTools\\Reference Data")}\\CanadaCity.csv";
		private readonly ITestOutputHelper output;

        public AddressRecognitionTempTests(ITestOutputHelper output)
        {
            this.output = output;
        }


        [Fact]
        private void GivenCityCSVFile_ThenCityReadIntoListAndTestedForMatch()
        {
			var Lines = File.ReadLines(CityFilePath).Select(a => a.Split(',')).ToList();
			string myText = "I come from Ottawa, I work at Apption";
			var q = Lines.Any(l => myText.Contains(l[0]));
			Assert.True(q);
			myText = "I come from Ottowa, I work at Apption";
			q = Lines.Any(l => myText.Contains(l[0]));
			Assert.False(q);
		}

		[Fact]
		private void GivenCSVFile_LinesCanBeReadInAndCitiesMatched()
		{
			var Lines = File.ReadLines(CityFilePath).Select(a => a.Split(',')).ToList();
			using (var fileReader = new StreamReader(EnvironicsCsvPath))
			using (var csv = new CsvReader(fileReader, CultureInfo.InvariantCulture))
			{
				csv.Read();
				csv.ReadHeader();
				while (csv.Read())
				{
					string address = csv.GetField<string>("city");
					var q = Lines.Any(l => address.Contains(l[0]));
					output.WriteLine("{0}: {1} ", address, q);
				}
			}			
		}

		[Fact]
		private void GivenCSVFile_LinesCanBeReadInAndCitiesMatchedCanadaCityCsv()
		{
			FileReader CityReader = new FileReader(CityFilePath2);
			var CityList = CityReader.GetByFieldToList("city");

			using (var fileReader = new StreamReader(EnvironicsCsvPath))
			using (var csv = new CsvReader(fileReader, CultureInfo.InvariantCulture))
			{
				csv.Read();
				csv.ReadHeader();
				while (csv.Read())
				{
					string address = csv.GetField<string>("city");
					var q = CityList.Any(l => address.Contains(l[0]));
					output.WriteLine("{0}: {1} ", address, q);
				}
			}
		}
	}
}

