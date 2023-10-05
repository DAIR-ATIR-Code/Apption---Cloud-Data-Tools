
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using DTHelperStd;
using System.Collections.Generic;
using System.Text;
using Xunit;
using RecognizerTools;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace DataToolsTest
{
    [Collection("DatabaseTestCase")]
	public class SchemaGenerationTests : DataToolsTest, IDisposable
	{
		private string SchemaPath = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\InventorySheet.txt";

		private FileAnalyzer FileAnalyzer = null;
		private FileObject FileObject = null;

		public SchemaGenerationTests()
		{
			FileAnalyzer = new FileAnalyzer(GeneralCsv, new Preferences(), true, SeparatorType.Comma);
		}



		[Fact]
		private async Task GivenFileAnalyzer_ThenGenerateObjectPopulated()
		{
            var (fileObject, tcs) = FileAnalyzer.AnalyzeAsyncWithObserver(CancellationToken.None);
            fileObject
            .SubscribeOn(Scheduler.Default)
			.ObserveOn(new EventLoopScheduler())
			.Subscribe(newFileObject =>
			{
				//On next                    
				FileObject = newFileObject;

			}, () =>
			{
				// On complete
				Assert.True(FileObject.ColumnMetadata.Count == 7, $"Expected 7 columns, Result was {FileObject.ColumnMetadata.Count}");
			});
            await tcs.Task;
		}

		[Fact]
		private async Task GivenFileObject_ThenSchemaGenerated()
		{
            var (fileObject, tcs) = FileAnalyzer.AnalyzeAsyncWithObserver(CancellationToken.None);
            fileObject
            .Subscribe(newFileObject =>
			{
				//On next                    
				FileObject = newFileObject;

			}, () =>
			{
				// On complete				
				Assert.True(FileObject.ColumnMetadata.Count == 7, $"Expected 7 columns, Result was {FileObject.ColumnMetadata.Count}");
				StringBuilder createTableScript = new StringBuilder();
				createTableScript.Append("CREATE TABLE NEW_TABLE ( \n");
                var sc = new SchemaGenerator(FileObject, new Preferences());
				foreach (var columnDetails in FileObject.ColumnMetadata)
				{
					createTableScript.Append(GetColumnLineForSchema(columnDetails.Value.FieldName, columnDetails.Value.MaxLength, columnDetails.Value.StorageType));
				}
				createTableScript.Length = createTableScript.Length - 2;
				createTableScript.Append(") \nGO");

				System.IO.File.WriteAllText(SchemaPath, createTableScript.ToString());
			});
            await tcs.Task;

		}

		private string GetColumnLineForSchema(string columnName, int minSize, StorageType storageType)
		{
			string sizeString = storageType == StorageType.Varchar ? $"({minSize.ToString()})" : string.Empty;			
			return $"[{columnName.Replace(" ", "_")}] [{storageType.ToString()}]{sizeString} NULL,\n";
		}

        public void Dispose()
        {
            FileAnalyzer?.Dispose();
        }
    }
}

