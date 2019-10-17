
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
    [Collection("ReactiveMessagingTestCase")]
    public class ReactiveMessagingTest : DataToolsTest, IDisposable
    
	{
		private readonly object fileObjectLock = new object();
		FileObject FileObject = null;
        private RegexHelper _regex = new RegexHelper(',');

		#region Unit Tests


		[Fact]
		private void GivenLargeFile_ThenColumnStoreSavedToFilesHeaders()
		{
			//Analyze();
			var fileObject = GetFileObjectUpdates();
			fileObject.Subscribe(subObject =>
			{
				Assert.True(subObject.HasHeaders);
			});
			//fileObject.Subscribe(x => )
		}

		[Fact]
		private async Task GivenFileObjectUpdates_ThenBytesProcessedIsCalculated()
		{
			fileAnalyzer = new FileAnalyzer(GeneralCsv,new Preferences(), true, SeparatorType.Comma);
            var (fileObject, tcs) = fileAnalyzer.AnalyzeAsyncWithObserver(CancellationToken.None);
            fileObject.Subscribe(newsubObject =>
			{
				fileAnalyzer.FileObject = newsubObject;
			});
            await tcs.Task;
			Assert.True(fileAnalyzer.FileObject.TotalRecords == 1000, $"Total Records: {fileAnalyzer.FileObject.TotalRecords}");
			Assert.True(fileAnalyzer.FileObject.BytesProcessed == fileAnalyzer.FileObject.FileSize, $"bytesProcessed: {fileAnalyzer.FileObject.BytesProcessed}, fileSize:{fileAnalyzer.FileObject.FileSize}");
		}

		[Fact]
		private void GivenFile_ThenBytesProcessedCalculatedNonParallel()
		{
			FileObject = new FileObject(GeneralCsv, true);
			using (var fileStream = File.Open(FileObject.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				StreamReader csvreader = new StreamReader(fileStream);
				string inputLine = string.Empty;

				while ((inputLine = csvreader.ReadLine()) != null)
				{
					FileObject.AddBytesProcessed(fileStream.Position);
				}

				Assert.True(FileObject.BytesProcessed == FileObject.FileSize, $"bytesProcessed: {FileObject.BytesProcessed}, fileSize:{FileObject.FileSize}");
			}


		}
		[Fact]
		private void GivenFile_ThenBytesProcessedCalculatedNonParallelWithREGEX()
		{

			FileObject = new FileObject(CsvWithHeadersPath, true);
			using (var fileStream = File.Open(FileObject.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				StreamReader csvreader = new StreamReader(fileStream);
				string inputLine;

				while ((inputLine = csvreader.ReadLine()) != null)
				{
					string[] firstLine = _regex.SplitLineIgnoreQuotedCommas(inputLine, 0);

					FileObject.AddBytesProcessed(fileStream.Position);
				}
				Assert.True(FileObject.BytesProcessed == FileObject.FileSize, $"bytesProcessed: {FileObject.BytesProcessed}, fileSize:{FileObject.FileSize}");
			}
		}

		#endregion

		#region Private Methods
		private IObservable<FileObject> GetFileObjectUpdates()
		{
			return Observable.Create<FileObject>(observer =>
			{
				FileObject = new FileObject(CsvWithHeadersPath);
				int count = 0;
				FileObject.HasHeaders = true;
				StreamReader csvreader = new StreamReader(FileObject.FilePath);
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
				}

				using (BlockingCollection<string[]> bc = new BlockingCollection<string[]>())
				{

					var t = Enumerable.Range(0, Environment.ProcessorCount - 1)
					  .Select(_ => Task.Run(() =>
					  {
						  foreach (var csvArray in bc.GetConsumingEnumerable())
						  {
							  Interlocked.Increment(ref count);
							  FileObject.TotalRecords = count;
							  FillColumnMetaData(csvArray);
							  observer.OnNext(FileObject);
						  }
					  })).ToArray();

					//observer.OnCompleted();

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
				observer.OnCompleted();
				return Disposable.Empty;
			});
		}
		private void FillColumnMetaData(string[] csvArrayLine)
		{
			int index = 0;
			foreach (string colValue in csvArrayLine)
			{
				Interlocked.Increment(ref index);

				string columnHeader = FileObject.Headers[index];
				int valLength = colValue.Length;
				int valueWordCount = CountWords(colValue);

				//replace any symbols with empty string
				string cleanData = RegexHelper.ALPHANUMERIC_PATTERN.Replace(colValue, "");

				FileObject.ColumnMetadata[index].TotalLength += valLength;
				FileObject.ColumnMetadata[index].TotalWords += valueWordCount;
				if (FileObject.ColumnMetadata[index].TotalRecords == 0)
				{
					FileObject.ColumnMetadata[index].TotalRecords = 1;
				}
				if (valLength == 0)
				{
					FileObject.ColumnMetadata[index].IncrementNullCounter();
				}
				if (FileObject.ColumnMetadata[index].MaxLength < valLength)
				{
					FileObject.ColumnMetadata[index].MaxLength = valLength;
				}

				if (FileObject.ColumnMetadata[index].IsInt && !RegexHelper.NUMBER_PATTERN1.IsMatch(cleanData))
				{
					FileObject.ColumnMetadata[index].IsInt = false;
				}

				if (FileObject.ColumnMetadata[index].IsCompletelyLetter && !StringHelper.IsLetterOnly(cleanData))
				{
					FileObject.ColumnMetadata[index].IsCompletelyLetter = false;
				}
			}
		}

        private CancellationTokenSource tks = new CancellationTokenSource();
        private FileAnalyzer fileAnalyzer;

        private void PopulateColumnMetadataDictionary(IDictionary<int, string> dictionary)
		{
			lock (fileObjectLock)
			{
				var value = string.Empty;
				foreach (var pair in dictionary)
				{
                    lock (FileObject)
                    {
                        value = pair.Value;
                        FileObject.ColumnMetadata.TryAdd(pair.Key, new ColumnMetadata(pair.Key));
                    }
				}
			}
		}

		private void FillHeaders(FileObject fileObject, string[] csvArray)
		{
			lock (fileObjectLock)
			{
				int count2 = 1;
				//Array.ForEach(csvArray, y => fileObject.DictHead.Add(y, count));
				//Array.ForEach(csvArray, y => fileObject.DictHead.Add($"Field{count2}", count2++));

				//dont forget to check for has headers.			
				if (fileObject.Headers.Count == 0)
				{
					foreach (string value in csvArray)
					{
						fileObject.Headers.TryAdd(count2, fileObject.HasHeaders ? value : $"Field{count2}");
						count2++;
					}
				}
			}
		}

        public void Dispose()
        {
            fileAnalyzer?.Dispose();
        }


        #endregion
    }
}

