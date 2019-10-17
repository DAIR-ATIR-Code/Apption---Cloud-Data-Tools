
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using DTHelperStd;
using RecognizerTools;
using RecognizerTools.SQL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace DataToolsTest
{
    [Collection("NCOATestCase")]
    public class ParseCsvAndLoadToDBTest
    {
        [Theory (Skip ="Unknown Error")]
        //[InlineData("NCOA_EXTRACT_20181107.txt", SeparatorType.Pipe, 9, true)]
        //[InlineData("NCOA_EXTRACT_20181120.txt", SeparatorType.Pipe, 344354, true)]
        //[InlineData("dummy_data.csv", SeparatorType.Comma, 397107, false)]
        [InlineData("General.csv", SeparatorType.Comma, 1000, true)]
        //[InlineData("dates.csv", SeparatorType.Comma, 1000, true)]
        private async Task GivenNCOA_ThenFilesWithHeadersAnalyzedBulk(string fileName, SeparatorType separatorType, int totalRecords, bool hasHeader)
        {
            var ncoa = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\{fileName}";
            var prefs = new Preferences();
            var fileAnalyzer = new FileAnalyzer(ncoa, prefs, hasHeader, separatorType);
            FileObject fileObject = null;
            fileObject = await PerformFirstPass(fileAnalyzer, fileObject);
            Assert.Equal(totalRecords, fileObject.TotalRecords);
            var ex = new ExecuteSecondPass();
            var observable = ex.Initialize(fileAnalyzer, prefs);


            //observable
            //.SubscribeOn(Scheduler.Default)
            //.ObserveOn(new EventLoopScheduler())
            //.Subscribe(result =>
            //{
            //    resultFactory.AddResult(result);

            //}, () =>
            //{
            //    resultFactory.CleanupProb();
            //});

            await ex.PerformSecondPassAnalysis();
            var connStr = @"Server=CPCDEV1;Initial Catalog=DataToolsTestDB;Trusted_Connection=True;";
            var sql = new ExecuteSQLCopy(connStr);
            var obs = await sql.Initialize(fileAnalyzer, prefs);
            var createTableSql = await sql.ExecuteSQLCreateTable();
            var copied = await sql.PerformBulkCopyToSQL();
            Assert.Contains($"CREATE TABLE [{fileObject.GetSQLTableName()}]", createTableSql);
            Assert.Equal(totalRecords, copied);
        }

        [Theory]
        [InlineData("NCOA_EXTRACT_20181107.txt", SeparatorType.Pipe, 9, true)]
        //[InlineData("NCOA_EXTRACT_20181120.txt", SeparatorType.Pipe, 344354, true)]
        //[InlineData("dummy_data.csv", SeparatorType.Comma, 397107, false)]
        //[InlineData("General.csv", SeparatorType.Comma, 1000, true)]
        //[InlineData("dates.csv", SeparatorType.Comma, 1000, true)]
        private async Task GivenNCOA_ThenFilesWithHeadersAnalyzed(string fileName, SeparatorType separatorType, int totalRecords, bool hasHeader)
        {
            var ncoa = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\{fileName}";
            var prefs = new Preferences();
            var fileAnalyzer = new FileAnalyzer(ncoa, prefs, hasHeader, separatorType);
            FileObject fileObject = null;
            fileObject = await PerformFirstPass(fileAnalyzer, fileObject);
            Assert.Equal(totalRecords, fileObject.TotalRecords);
            var ex = new ExecuteSecondPass();
            var observable = ex.Initialize(fileAnalyzer, prefs);

            //observable
            //.SubscribeOn(Scheduler.Default)
            //.ObserveOn(new EventLoopScheduler())
            //.Subscribe(result =>
            //{
            //    resultFactory.AddResult(result);

            //}, () =>
            //{
            //    resultFactory.CleanupProb();
            //});

            await ex.PerformSecondPassAnalysis();
            var connStr = @"Server=CPCDEV1;Initial Catalog=DataToolsTestDB;Trusted_Connection=True;";
            var sql = new ExecuteSQLCopy(connStr);
            var obs = await sql.Initialize(fileAnalyzer, prefs);
            var createTableSql = await sql.ExecuteSQLCreateTable();
            var copied = await sql.PerformCopyToSQL();
            Assert.Contains($"CREATE TABLE [{fileObject.GetSQLTableName()}]", createTableSql);
            Assert.Equal(totalRecords, copied);
        }

        private static async Task<FileObject> PerformFirstPass(FileAnalyzer fileAnalyzer, FileObject fileObject)
        {
            var (observable, tcs) = fileAnalyzer.AnalyzeAsyncWithObserver(CancellationToken.None);
            observable
            .Subscribe(newFileObject =>
            {
                //On next                    
                fileObject = newFileObject;

            }, ex =>
            {
            },
            () =>
            {
                //completed
            });
            await tcs.Task;
            return fileObject;
        }


    }
}

