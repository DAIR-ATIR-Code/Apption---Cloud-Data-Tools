
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using DTHelperStd;
using RecognizerTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DataToolsTest
{
    [Collection("DatabaseTestCase")]
    public class FileEncodingTest
	{
	
		#region Unit Tests

		//[Fact]
		//private void GivenUTF16MACFile_ThenFileProcessed()
		//{
		//	string CsvPath = $"{PathHelper.GetFolderRelativeToProject("DataToolsTest\\EncodingTestFiles")}\\InventorySheetUTF16MAC.csv";
		//		AnalyzeFile(CsvPath);
		//}

		//[Fact]
		//private void GivenUTF16UNIXFile_ThenFileProcessed()
		//{
		//	string CsvPath = $"{PathHelper.GetFolderRelativeToProject("DataToolsTest\\EncodingTestFiles")}\\InventorySheetUTF16UNIX.csv";
		//	AnalyzeFile(CsvPath);
		//}
		//[Fact]
		//private void GivenUTF16WindowsFile_ThenFileProcessed()
		//{
		//	string CsvPath = $"{PathHelper.GetFolderRelativeToProject("DataToolsTest\\EncodingTestFiles")}\\InventorySheetUTF16WINDOWS.csv";
		//	AnalyzeFile(CsvPath);
		//}
		[Fact]
		private async Task GivenUTF8MACFile_ThenFileProcessed()
		{
			string CsvPath = $"{PathHelper.GetFolderRelativeToProject("DataToolsTest\\EncodingTestFiles")}\\InventorySheetUTF8MAC.csv";
			await AnalyzeFile(CsvPath);
		}
		[Fact]
		private async Task GivenUTF8UNIXFile_ThenFileProcessed()
		{
			string CsvPath = $"{PathHelper.GetFolderRelativeToProject("DataToolsTest\\EncodingTestFiles")}\\InventorySheetUTF8UNIX.csv";
			await AnalyzeFile(CsvPath);
		}
		[Fact]
		private async Task GivenUTF8WindowsFile_ThenFileProcessed()
		{
			string CsvPath = $"{PathHelper.GetFolderRelativeToProject("DataToolsTest\\EncodingTestFiles")}\\InventorySheetUTF8WINDOWS.csv";
			await AnalyzeFile(CsvPath);
		}

  //      [Fact]
		//private void GivenUTF8WindowsFile_abc()
		//{
  //          string destinationFile = @"C:\Users\dzhao\source\repos\Cloud Data Platform\DataTools\Samples\DemoFile3.txt";
  //          string demoFile1 = @"C:\Users\dzhao\Downloads\MOCK_DATA.csv";
  //          string demoFile2 = @"C:\Users\dzhao\Downloads\MOCK_DATA (1).csv";
  //          string demoFile3 = @"C:\Users\dzhao\Downloads\MOCK_DATA (2).csv";
  //          string demoFile4 = @"C:\Users\dzhao\Downloads\MOCK_DATA (3).csv";
  //          string demoFile5 = @"C:\Users\dzhao\Downloads\MOCK_DATA (4).csv";
  //          string demoFile6 = @"C:\Users\dzhao\Downloads\MOCK_DATA (5).csv";
  //          string demoFile7 = @"C:\Users\dzhao\Downloads\MOCK_DATA (6).csv";
  //          string demoFile8 = @"C:\Users\dzhao\Downloads\MOCK_DATA (7).csv";
  //          string demoFile9 = @"C:\Users\dzhao\Downloads\MOCK_DATA (8).csv";
  //          string demoFile10 = @"C:\Users\dzhao\Downloads\MOCK_DATA (9).csv";
  //          string demoFile11 = @"C:\Users\dzhao\Downloads\MOCK_DATA (10).csv";

  //          var demo1 = File.ReadAllLines(demoFile1);
  //          var demo2 = File.ReadAllLines(demoFile2);
  //          var demo3 = File.ReadAllLines(demoFile3);
  //          var demo4 = File.ReadAllLines(demoFile4);
  //          var demo5 = File.ReadAllLines(demoFile5);
  //          var demo6 = File.ReadAllLines(demoFile6);
  //          var demo7 = File.ReadAllLines(demoFile7);
  //          var demo8 = File.ReadAllLines(demoFile8);
  //          var demo9 = File.ReadAllLines(demoFile9);
  //          var demo10 = File.ReadAllLines(demoFile10);
  //          var demo11 = File.ReadAllLines(demoFile11);

  //          var insertList = new List<string>();
  //          insertList.AddRange(demo1);
  //          insertList.AddRange(demo2);
  //          insertList.AddRange(demo3);
  //          insertList.AddRange(demo4);
  //          insertList.AddRange(demo5);
  //          insertList.AddRange(demo6);
  //          insertList.AddRange(demo7);
  //          insertList.AddRange(demo8);
  //          insertList.AddRange(demo9);
  //          insertList.AddRange(demo10);
  //          insertList.AddRange(demo11);
  //          var regex = new RegexHelper(',');
  //          var newFile = new List<string>();
  //          var insertArray = insertList.ToArray();
  //          using (var fs = new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
  //          {
  //              using (var sr = new StreamReader(fs))
  //              {
  //                  var index = 0;
  //                  string line = sr.ReadLine();
  //                  var tempArr = regex.SplitLineIgnoreQuotedCommas(line,17);
  //                  tempArr[15] = insertArray[index++];
  //                  newFile.Add(String.Join(',', tempArr.ToArray()));
  //                  while ((line = sr.ReadLine()) != null)
  //                  {
  //                      var tempList = regex.SplitLineIgnoreQuotedCommas(line, 17).ToList();
  //                      tempList[15] = insertArray[index++];
  //                      newFile.Add(String.Join(',', tempList.ToArray()));
  //                  }
  //              }
  //          }
  //          File.WriteAllLines(destinationFile, newFile.ToArray());

  //      }
        //[Fact]
        //private void GivenUTF32MACFile_ThenFileProcessed()
        //{
        //	string CsvPath = $"{PathHelper.GetFolderRelativeToProject("DataToolsTest\\EncodingTestFiles")}\\InventorySheetUTF32MAC.csv";
        //	AnalyzeFile(CsvPath);
        //}
        //[Fact]
        //private void GivenUTF32UNIXFile_ThenFileProcessed()
        //{
        //	string CsvPath = $"{PathHelper.GetFolderRelativeToProject("DataToolsTest\\EncodingTestFiles")}\\InventorySheetUTF32UNIX.csv";
        //	AnalyzeFile(CsvPath);
        //}
        //[Fact]
        //private void GivenUTF32WindowsFile_ThenFileProcessed()
        //{
        //	string CsvPath = $"{PathHelper.GetFolderRelativeToProject("DataToolsTest\\EncodingTestFiles")}\\InventorySheetUTF32WINDOWS.csv";
        //	AnalyzeFile(CsvPath);
        //}




        #endregion
        #region private methods
        private static async Task AnalyzeFile(string CsvPath)
		{
            using (FileAnalyzer fileAnalyzer = new FileAnalyzer(CsvPath, new Preferences(), true, SeparatorType.Comma))
            {
                var (fileObject, tcs) = fileAnalyzer.AnalyzeAsyncWithObserver(CancellationToken.None);
                fileObject.Subscribe(newsubObject =>
                {
                    fileAnalyzer.FileObject = newsubObject;
                });
                await tcs.Task;
                //Assert.True(fileAnalyzer.FileObject.TotalRecords == 1000, $"Expected 1000, found {fileAnalyzer.FileObject.TotalRecords} records");
                //Assert.True(fileAnalyzer.FileObject.BytesProcessed == fileAnalyzer.FileObject.FileSize, $"bytesProcessed: {fileAnalyzer.FileObject.BytesProcessed}, fileSize:{fileAnalyzer.FileObject.FileSize}");
                Assert.Equal(1000, fileAnalyzer.FileObject.TotalRecords);
                Assert.Equal(fileAnalyzer.FileObject.FileSize, fileAnalyzer.FileObject.BytesProcessed);            
            }
        }
        #endregion
    }
}

