
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using DTHelperStd;
using RecognizerTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DataToolsTest
{
    [Collection("DatabaseTestCase")]
    public class ImportCSVLargeFileTest : DataToolsTest, IDisposable
    {
        private readonly object fileObjectLock = new object();
        private FileObject FileObject = null;
        private FileAnalyzer FileAnalyzer = null;
        private RegexHelper _regex = new RegexHelper(',');



        #region Unit Tests

        [Fact]
        private void GivenLargeCSVFile_ThenFileReadUsingStreamReader()
        {
            FileObject fileObject = new FileObject(CsvWithNoHeadersPath);
            fileObject.HasHeaders = false;
            StreamReader csvreader = new StreamReader(fileObject.FilePath);
            string inputLine = "";
            List<string[]> arrayList = new List<string[]>();
            bool firstIteration = true;
            while ((inputLine = csvreader.ReadLine()) != null)
            {
                string[] csvArray = inputLine.Split(new char[] { ',' });
                if (firstIteration)
                {
                    FillHeaders(fileObject, csvArray);

                    firstIteration = false;
                    if (!fileObject.HasHeaders)
                    {
                        arrayList.Add(csvArray);
                    }
                }
                else
                {
                    arrayList.Add(csvArray);
                }
            }
        }

        [Fact]
        private void GivenLargeFile_ThenFilesNoHeadersAnalyzed()
        {
            AnalyzeFile(CsvWithNoHeadersPath, false);
        }

        [Fact]
        private void GivenLargeFile_ThenFilesWithHeadersAnalyzed()
        {
            AnalyzeFile(CsvWithHeadersPath, true);
        }
        [Fact]
        private void GivenFile_ThenSampleDataPopulated()
        {
            GivenLargeFile_ThenFilesWithHeadersAnalyzed();

            Assert.True(FileObject.FindSampleData("inventory number").Count >= 10, $"Inventory Number: Expected greater that 10 records, calculated {FileObject.FindSampleData("inventory number").Count} records");
            Assert.True(FileObject.FindSampleData("description").Count >= 10, $"Description: Expected greater that 10 records, calculated {FileObject.FindSampleData("description").Count} records");
            Assert.True(FileObject.FindSampleData("\"serial, number\"").Count >= 10, $"Serial Number: Expected greater that 10 records, calculated {FileObject.FindSampleData("\"serial, number\"").Count} records");
            Assert.True(FileObject.FindSampleData("date acquired").Count >= 10, $"Date Acquired: Expected greater that 10 records, calculated {FileObject.FindSampleData("date acquired").Count} records");
            Assert.True(FileObject.FindSampleData("vendor").Count >= 10, $"Vendor: Expected greater that 10 records, calculated {FileObject.FindSampleData("vendor").Count} records");
            Assert.True(FileObject.FindSampleData("value").Count >= 10, $"Value: Expected greater that 10 records, calculated {FileObject.FindSampleData("value").Count} records");
            Assert.True(FileObject.FindSampleData("cost").Count >= 10, $"Cost: Expected greater that 10 records, calculated {FileObject.FindSampleData("cost").Count} records");
        }

        [Fact]
        private async Task GivenNewFileAnalzyer_ThenFirstPassFileObjectPropertiesGenerated()
        {            FileAnalyzer = new FileAnalyzer(CsvWithHeadersPath, new Preferences(), true, SeparatorType.Comma);
            
                await FileAnalyzer.AnalyzeAsync(CancellationToken.None);
                Assert.True(FileAnalyzer.FileObject.FindColumnMetadata("inventory_number").MaxLength == 3);
                Assert.True(FileAnalyzer.FileObject.FindColumnMetadata("description").MaxLength == 35);
                Assert.True(FileAnalyzer.FileObject.FindColumnMetadata("serial__number").MaxLength == 16);
                Assert.True(FileAnalyzer.FileObject.FindColumnMetadata("date_acquired").MaxLength == 8);
                Assert.True(FileAnalyzer.FileObject.FindColumnMetadata("vendor").MaxLength == 38);
                Assert.True(FileAnalyzer.FileObject.FindColumnMetadata("cost").MaxLength == 6);
                Assert.True(FileAnalyzer.FileObject.FindColumnMetadata("value").MaxLength == 6);
                Assert.True(FileAnalyzer.FileObject.FindColumnMetadata("description").TotalNulls == 7, $"Expected 7 Nulls, Found {FileAnalyzer.FileObject.FindColumnMetadata("description").TotalNulls}");
                Assert.True(FileAnalyzer.FileObject.FindColumnMetadata("description").TotalRecords == 1000, $"Expected 1000 records, Found {FileAnalyzer.FileObject.FindColumnMetadata("description").TotalRecords}");
				Assert.True(FileAnalyzer.FileObject.FindColumnMetadata("value").IsNumberWithSpecialCharacters, $"Expected true but was false");
                //Assert.True(FileAnalyzer.FileObject.FindColumnMetadata("vendor").IsCompletelyLetter, $"Expected true but was false");
                Assert.True(!FileAnalyzer.FileObject.FindColumnMetadata("serial__number").IsCompletelyLetter, $"Expected true but was false");
                Assert.True(!FileAnalyzer.FileObject.FindColumnMetadata("serial__number").IsInt, $"Expected true but was false");
                Assert.True(FileAnalyzer.FileObject.FindColumnMetadata("serial__number").IsNumberWithLetter, $"Expected true but was false");
                Assert.Equal(83711, FileAnalyzer.FileObject.FileSize);

                Assert.True(FileAnalyzer.FileObject.FindSampleData("inventory_number").Count >= 10, $"Inventory Number: Expected greater that 10 records, calculated {FileAnalyzer.FileObject.FindSampleData("inventory_number").Count} records");
                Assert.True(FileAnalyzer.FileObject.FindSampleData("description").Count >= 10, $"Description: Expected greater that 10 records, calculated {FileAnalyzer.FileObject.FindSampleData("description").Count} records");
                Assert.True(FileAnalyzer.FileObject.FindSampleData("serial__number").Count >= 10, $"Serial Number: Expected greater that 10 records, calculated {FileAnalyzer.FileObject.FindSampleData("serial__number").Count} records");
                Assert.True(FileAnalyzer.FileObject.FindSampleData("date_acquired").Count >= 10, $"Date Acquired: Expected greater that 10 records, calculated {FileAnalyzer.FileObject.FindSampleData("date_acquired").Count} records");
                Assert.True(FileAnalyzer.FileObject.FindSampleData("vendor").Count >= 10, $"Vendor: Expected greater that 10 records, calculated {FileAnalyzer.FileObject.FindSampleData("vendor").Count} records");
                Assert.True(FileAnalyzer.FileObject.FindSampleData("value").Count >= 10, $"Value: Expected greater that 10 records, calculated {FileAnalyzer.FileObject.FindSampleData("value").Count} records");
                Assert.True(FileAnalyzer.FileObject.FindSampleData("cost").Count >= 10, $"Cost: Expected greater that 10 records, calculated {FileAnalyzer.FileObject.FindSampleData("cost").Count} records");            
		}

		[Fact]
		private void GivenCSVRecords_ThenHeadersExtractedIntoDictionary()
		{


			FileObject = new FileObject(CsvWithHeadersPath);

			FileObject.HasHeaders = true;
			StreamReader csvreader = new StreamReader(FileObject.FilePath);
			string inputLine = string.Empty;
			//Must perform first iteration outside of blocking collection
			string[] firstLine = _regex.SplitLineIgnoreQuotedCommas(csvreader.ReadLine(), 0);
			if (firstLine != null)
			{
				FillHeaders(FileObject, firstLine);
			}

			Assert.True(FileObject.Headers.Count == 7, $"Dictionary contains {FileObject.Headers.Count} records. Expected 7");
			Assert.True(FileObject.Headers[1] == "inventory number", $"Dictionary does not contain the key 'inventory number'");
			Assert.True(FileObject.Headers[5] == "vendor", $"Dictionary does not contain the key 'vendor'");
		}

		[Fact]
		private void GivenFile_ThenFileSizeKnown()
		{
			long length = new FileInfo(CsvWithHeadersPath).Length;
			Assert.Equal(83711, length);
			length = new FileInfo(CsvWithNoHeadersPath).Length;

            long lengthCRLF = 235731824;
            long lengthLF = 234986262;

            Assert.True(length == lengthCRLF || length == lengthLF);
		}


		[Fact]
		private async Task GivenVeryLargeFile_ThenFilesWithHeadersAnalyzed()
		{
			FileAnalyzer = new FileAnalyzer(CsvWithNoHeadersPath, new Preferences(), false, SeparatorType.Comma);
            var (fileService, tcs) = FileAnalyzer.AnalyzeAsyncWithObserver(CancellationToken.None);
            fileService
			.Subscribe(newFileObject =>
			{
				//On next                    
				FileObject = newFileObject;

			}, () =>
			{
				Assert.Equal(745562,FileObject.TotalRecords);
				

			});
            await tcs.Task;
		}


		[Fact]
		private async Task GivenVeryLargeFile_ThenUniquenessPopulated()
		{
			FileAnalyzer = new FileAnalyzer(InventorySheetNoHeadersPath,new Preferences(), false, SeparatorType.Comma);
            var (fileObject, tcs) = FileAnalyzer.AnalyzeAsyncWithObserver(CancellationToken.None);
            fileObject.Subscribe(newFileObject =>
			{
				//On next                    
				FileObject = newFileObject;

			}, () =>
			{
				Assert.True(FileObject.TotalRecords == 1000, $"Expected 1000, discovered {FileObject.TotalRecords}");

				CheckForFieldUniqueness();
				
			});
            await tcs.Task;
		}
		#endregion

		#region Private Methods

		private void CheckForFieldUniqueness()
		{
			using (var conn = new SQLiteConnection(FileAnalyzer.ConnectionString.ToString()))
			{
				conn.Open();

				var command = conn.CreateCommand();
				object obj = new object();
				foreach (var metaData in FileObject.ColumnMetadata)
				{
					command.CommandText = "SELECT COUNT (DISTINCT \"" + metaData.Key + "\") FROM ImportFile";
					obj = command.ExecuteScalar();
					metaData.Value.IsUnique = Convert.ToInt32(obj) == metaData.Value.TotalRecords;
				}
			}
		}
		private void AnalyzeFile(string filePath, bool hasHeaders)
		{
			FileObject = new FileObject(filePath);
			int count = 0;
			FileObject.HasHeaders = hasHeaders;
			using (var fileStream = File.Open(FileObject.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				StreamReader csvreader = new StreamReader(fileStream);
				string inputLine = string.Empty;
				//Must perform first iteration outside of blocking collection
				string[] firstLine = _regex.SplitLineIgnoreQuotedCommas(csvreader.ReadLine(), 0);

				if (firstLine != null)
				{
					FillHeaders(FileObject, firstLine);
					PopulateColumnMetadataDictionary(FileObject.Headers);
					if (!FileObject.HasHeaders)
					{
						count++;
						FillColumnMetaData(firstLine);
					}
					FileObject.AddBytesProcessed(fileStream.Position);
				}

				using (BlockingCollection<string[]> bc = new BlockingCollection<string[]>())
				{

					var t = Enumerable.Range(0, Environment.ProcessorCount - 1)
					  .Select(_ => Task.Run(() =>
					  {
						  foreach (var csvArray in bc.GetConsumingEnumerable())
						  {
							  Interlocked.Increment(ref count);
							  if (FileObject.SampleData.All(x => x.Value.Count >= 10))
							  {
								  FileObject.FillSampleData = false;
							  }
							  FileObject.TotalRecords = count;
							  FillColumnMetaData(csvArray);
							  FileObject.AddBytesProcessed(fileStream.Position);
						  }
					  })).ToArray();

					while ((inputLine = csvreader.ReadLine()) != null)
					{
						bc.Add(_regex.SplitLineIgnoreQuotedCommas(inputLine, FileObject.TotalColumns));
					}

					bc.CompleteAdding();
					Task.WaitAll(t);

					foreach (var kvp in FileObject.ColumnMetadata)
					{
						kvp.Value.TotalRecords = FileObject.TotalRecords;
					}


				}
			}
		}

		
		private void FillColumnMetaData(string[] csvArrayLine)
		{
			int index = 0;
			foreach (string colValue in csvArrayLine)
			{
				Interlocked.Increment(ref index);

				var columnHeader = index;

				if (FileObject.FillSampleData)
				{
					FileObject.SampleData[index].Add(colValue);
				}


				int valLength = colValue.Length;
				int valueWordCount = CountWords(colValue);

				//replace any symbols with empty string
				string cleanData = RegexHelper.ALPHANUMERIC_PATTERN.Replace(colValue, "");

				FileObject.ColumnMetadata[columnHeader].TotalLength += valLength;
				FileObject.ColumnMetadata[columnHeader].TotalWords += valueWordCount;
				if (FileObject.ColumnMetadata[columnHeader].TotalRecords == 0)
				{
					FileObject.ColumnMetadata[columnHeader].TotalRecords = 1;
				}
				if (valLength == 0)
				{
					FileObject.ColumnMetadata[columnHeader].IncrementNullCounter();
				}
				if (FileObject.ColumnMetadata[columnHeader].MaxLength < valLength)
				{
					FileObject.ColumnMetadata[columnHeader].MaxLength = valLength;
				}

				if (FileObject.ColumnMetadata[columnHeader].IsInt && !RegexHelper.NUMBER_PATTERN1.IsMatch(cleanData))
				{
					FileObject.ColumnMetadata[columnHeader].IsInt = false;
				}

				if (FileObject.ColumnMetadata[columnHeader].IsCompletelyLetter && !StringHelper.IsLetterOnly(cleanData))
				{
					FileObject.ColumnMetadata[columnHeader].IsCompletelyLetter = false;
				}
			}
		}
        
        private CancellationTokenSource tks = new CancellationTokenSource();

        private void PopulateColumnMetadataDictionary(IDictionary<int, string> dictionary)
		{
			lock (fileObjectLock)
			{
				string value = string.Empty;
				foreach (var pair in dictionary)
				{
					value = pair.Value;
					FileObject.ColumnMetadata.Add(pair.Key, new ColumnMetadata(pair.Key));
					FileObject.SampleData.TryAdd(pair.Key, new List<string>());
				}
			}
		}

		private void FillHeaders(FileObject fileObject, string[] csvArray)
		{
			lock (fileObjectLock)
			{
				int fieldCount = 1;			
				if (fileObject.Headers.Count == 0)
				{
					foreach (string value in csvArray)
					{
						fileObject.Headers.TryAdd(fieldCount, fileObject.HasHeaders ? value : $"Field{fieldCount}");
						fieldCount++;
					}
				}
			}
		}

        public void Dispose()
        {
            FileAnalyzer?.Dispose();
        }


        #endregion
    }
}

