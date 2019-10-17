
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions;
using RecognizerTools;
using DTHelperStd;
using DataTools;
using Unity;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Collections.Generic;
using NLog.Config;
using DataToolsTest.NLogXUnit;
using NLog;

namespace DataToolsTest
{
    [Collection("DatabaseTestCase")]
    public class RecognizerTests : DataToolsTest, IDisposable
    {
        private static readonly string FakeDataFilePath = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\General.csv";
        private static readonly string FakeBigDataFilePath = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\dummy_data.csv";
        private static readonly string TestFile = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\testFile.txt";
        // To add this file please remove from .gitignore
        //private static readonly string FakeBiggerDataFilePath = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\big-dummy-data.csv";

        public RecognizerTests(ITestOutputHelper output)
        {
            var config = new LoggingConfiguration();
            var xunitTarget = new NLogXUnitTarget(output);
            config.AddTarget("xunit", xunitTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, xunitTarget));
            LogManager.Configuration = config;
            logger = LogManager.GetCurrentClassLogger();
        }

        private static FileAnalyzer FileAnalyzer;


        [Fact]
        private async Task GivenCSVFile_ThenTestUnknownCSVFile()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var prefs = new Preferences();
            var cdt = new CloudDataTools(GeneralCsv, true, SeparatorType.Comma, prefs);

            await cdt.FileAnalyzer.AnalyzeAsync(CancellationToken.None);
            stopwatch.Stop();
            logger.Info($"First analysis time: {stopwatch.Elapsed}");

            stopwatch.Reset();
            stopwatch.Start();

            cdt.InitializeSecondPass();
            cdt.SecondPass.StartBatch(new List<(int, Type, RecognizerTools.State.RecognizerState)>());
            await cdt.SecondPass.Analyze();
            stopwatch.Stop();
            logger.Info($"Second analysis time: {stopwatch.Elapsed}");

            AssertGreaterThan(cdt.SecondPass.GetRecognizerInstance<LetterRecognizer>(1).CurrentState.Probability, 0.9);
            AssertGreaterThan(cdt.SecondPass.GetRecognizerInstance<PhoneRecognizer>(2).CurrentState.Probability, 0.9);
            AssertGreaterThan(cdt.SecondPass.GetRecognizerInstance<GenderRecognizer>(3).CurrentState.Probability, 0.9);
            AssertGreaterThan(cdt.SecondPass.GetRecognizerInstance<DateRecognizer>(4).CurrentState.Probability, 0.9);
            AssertGreaterThan(cdt.SecondPass.GetRecognizerInstance<CreditCardRecognizer>(5).CurrentState.Probability, 0.9);
            AssertGreaterThan(cdt.SecondPass.GetRecognizerInstance<EmailRecognizer>(6).CurrentState.Probability, 0.9);
            //AssertGreaterThan(secondPassAnalyzers.GetRecognizerInstance<LetterWithNumberRecognizer>(6).CurrentState.Probability, 0.9);
        }


        [Fact]
        private async Task GivenCSVFile_ThenTestUnknownBigCSVFile()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var prefs = new Preferences();
            var cdt = new CloudDataTools(FakeBigDataFilePath, true, SeparatorType.Comma, prefs);

            await cdt.FileAnalyzer.AnalyzeAsync(CancellationToken.None);
            stopwatch.Stop();
            logger.Info($"First analysis time: {stopwatch.Elapsed}");

            stopwatch.Reset();
            stopwatch.Start();
            var secondPassAnalyzers = new SecondPass(cdt.FileAnalyzer, prefs);

            cdt.InitializeSecondPass();
            cdt.SecondPass.StartBatch(new List<(int, Type, RecognizerTools.State.RecognizerState)>());
            await cdt.SecondPass.Analyze();
            stopwatch.Stop();
            logger.Info($"Second analysis time: {stopwatch.Elapsed}");

            ShowResult(secondPassAnalyzers);

            AssertGreaterThan(cdt.SecondPass.GetRecognizerInstance<NumberRecognizer>(1).CurrentState.Probability,0.9);
            AssertGreaterThan(cdt.SecondPass.GetRecognizerInstance<NumberRecognizer>(2).CurrentState.Probability, 0.9);
            AssertGreaterThan(cdt.SecondPass.GetRecognizerInstance<DateRecognizer>(3).CurrentState.Probability, 0.9);
            AssertGreaterThan(cdt.SecondPass.GetRecognizerInstance<LetterWithNumberRecognizer>(4).CurrentState.Probability, 0.9);
            AssertGreaterThan(cdt.SecondPass.GetRecognizerInstance<NumberRecognizer>(5).CurrentState.Probability, 0.9);
            AssertGreaterThan(cdt.SecondPass.GetRecognizerInstance<NumberRecognizer>(6).CurrentState.Probability, 0.9);
            AssertGreaterThan(cdt.SecondPass.GetRecognizerInstance<LetterWithNumberRecognizer>(7).CurrentState.Probability, 0.9);
        }

        private static void AssertGreaterThan(double x, double y)
        {
            Assert.True(x > y, $"{x} is not greater than {y}");
        }

        [Fact]
        private async Task GivenCSVFile_ThenTestTotalRowNumberInParallelProgramming()
        {
            FileAnalyzer = new FileAnalyzer(FakeBigDataFilePath, new Preferences(),false, SeparatorType.Comma);
            await FileAnalyzer.AnalyzeAsync(CancellationToken.None);

            var queue = new BufferBlock<string>(new DataflowBlockOptions { BoundedCapacity = 10 });

            var consumers = Enumerable.Range(0, Environment.ProcessorCount - 1)
                .Select(value => Task.Run<int>(async () =>
               {
                   return await ConsumeAsync(queue);
               })).ToArray();
            var producer = ProduceV2(queue);

            producer.Wait();
            await Task.WhenAll(consumers);

            var result = consumers.Select(t => (int)t.Result).Sum();
            Assert.Equal(397107, result);
        }


        [Fact]
        private async Task GivenCSVFile_ThenTestCancellationToken()
        {
            FileAnalyzer = new FileAnalyzer(FakeBigDataFilePath, new Preferences(),true, SeparatorType.Comma);
            await FileAnalyzer.AnalyzeAsync(CancellationToken.None);
            var source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            var queue = new BufferBlock<string>(new DataflowBlockOptions { BoundedCapacity = 10 , CancellationToken = token});

            
            var consumers = Enumerable.Range(0, Environment.ProcessorCount - 1)
                .Select(value => Task.Run<int>(async () =>
                {
                    return await ConsumeAsyncV1(queue);
                })).ToArray();

            var producer = ProduceV1(queue);

            var task1 = Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();
                Thread.Sleep(50);
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    Random random = new Random();                                       
                    if (random.Next(0, 50) == 0)
                    {
                        source.Cancel();
                        logger.Info("end of task1");
                        
                    }
                }
            });

            try
            {
                producer.Wait();
                await Task.WhenAll(consumers);
                task1.Wait();
            }
            catch (AggregateException e)
            {
                e.Handle((x) => {
                    if(x is TaskCanceledException)
                    {
                        logger.Info("Random number generator hit the 0");
                        logger.Info("Task finish");
                        return true;
                    }
                    return false;
                });
            }
            finally
            {
                source.Dispose();
            }
            Assert.True(token.IsCancellationRequested, $"CancellationToken: {token.IsCancellationRequested}");
        }

        private static async Task ProduceV1(BufferBlock<string> target)
        {
            //token.ThrowIfCancellationRequested();
            using (var fileReader = new StreamReader(FileAnalyzer.FileObject.FilePath))
            {
                string line;
                while ((line = fileReader.ReadLine()) != null)
                {
                    await target.SendAsync(line);
                }
                target.Complete();
            }
        }
        private int count = 0;
        private Logger logger;

        private async Task<int> ConsumeAsyncV1(BufferBlock<string> source)
        {
           
            while (await source.OutputAvailableAsync())
            {
                //token.ThrowIfCancellationRequested();
                string row = String.Empty;
                while (source.TryReceive(out row))
                {
                    Interlocked.Increment(ref count);
                    logger.Info($"{count}");
                }
            }
            return count;
        }
        #region private method
        private async Task<int> ConsumeAsync(BufferBlock<string> source)
        {
            int count = 0;
            while (await source.OutputAvailableAsync())
            {
                string row = String.Empty;
                
                while (source.TryReceive(out row))
                {
                    count++;
                }              
            }
            return count++;
        }
        private static async Task ProduceV2(BufferBlock<string> target)
        {
            using (var fileReader = new StreamReader(FileAnalyzer.FileObject.FilePath))
            {
                string line;
                if (FileAnalyzer.FileObject.HasHeaders)
                {
                    fileReader.ReadLine();
                }

                while ((line = fileReader.ReadLine()) != null)
                {
                    await target.SendAsync(line);
                }
                target.Complete();
            }
        }



        private void ShowResult(SecondPass secondPassAnalyzers)
        {
            using (var outputFile = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "../../../", "testResult.txt")))
            {
                foreach (var secondPassAnalyzer in secondPassAnalyzers.ColumnRecognizers)
                {
                    var probabilityDictionary = secondPassAnalyzer.SecondAnalysisResult.ProbabilityByRecognizer;
                    var graphDataDictionary = secondPassAnalyzer.SecondAnalysisResult.SummaryDataByRecognizer;

                    outputFile.WriteLine(secondPassAnalyzer.GetDescription());
                    if(probabilityDictionary.Count != 0)
                    {
                        foreach (var kvp in probabilityDictionary)
                        {
                            outputFile.WriteLine($"{kvp.Key}: {kvp.Value}");
                        }
                    }
                    else
                    {
                        outputFile.WriteLine("Empty column");
                    }

                    outputFile.WriteLine("");
                }
            }
            
        }

        public void Dispose()
        {
            FileAnalyzer?.Dispose();
        }




        #endregion
        //[Fact]
        //private async Task GivenCSVFile_ThenTestUnknowCSVFileWithFields()
        //{

        //    RegisterRecognizers();

        //    FileAnalyzer.Analyze();

        //    foreach (KeyValuePair<string, List<string>> keyValuePair in FileAnalyzer.FileObject.ColumnStore)
        //    {
        //        string key = keyValuePair.Key;
        //        logger.Info("\n list: {0}", key);
        //        logger.Info("Max length: {0}", FileAnalyzer.FileObject.ColumnMetadata[key].MaxLength);
        //        logger.Info("Average length: {0}", FileAnalyzer.FileObject.ColumnMetadata[key].AverageLength);
        //        logger.Info("IsNumber: {0}", FileAnalyzer.FileObject.ColumnMetadata[key].IsNumber);
        //        logger.Info("IsLetter: {0} \n", FileAnalyzer.FileObject.ColumnMetadata[key].IsCompletlyLetter);

        //        if (FileAnalyzer.FileObject.ColumnMetadata[key].IsNumber)
        //        {
        //            await DetermineProbabilityV2(Container.ResolveAll<INumberRecognizer>(), keyValuePair.Value);
        //        }
        //        else if (FileAnalyzer.FileObject.ColumnMetadata[key].IsCompletlyLetter)
        //        {
        //            await DetermineProbabilityV2(Container.ResolveAll<ILetterRecognizer>(), keyValuePair.Value);
        //        }
        //        else
        //        {
        //            await DetermineProbabilityV2(Container.ResolveAll<ILetterWithNumberRecognizer>(), keyValuePair.Value);
        //        }
        //    }


        //}

        //private async Task ProduceAll(BufferBlock<string> queue, List<string> list)
        //{
        //    var producer1 = Produce(queue, list);
        //    //var producer2 = Produce(queue, list);
        //    //var producer3 = Produce(queue, list);
        //    //var producer4 = Produce(queue, list);
        //    //var producer5 = Produce(queue, list);

        //    //await Task.WhenAll(producer1, producer2, producer3, producer4, producer5);
        //    await Task.WhenAll(producer1);
        //    queue.Complete();

        //}
        //private void DetermineProbability(IEnumerable<IRecognizer> recognizers, List<string> list)
        //{
        //    var probabilities = new ConcurrentDictionary<Type, float>();
        //    Parallel.ForEach(recognizers, recognizer => {
        //        var count = 0;
        //        var total = 0;
        //        var empty = 0;
        //        foreach (string data in list)
        //        {
        //            if (String.IsNullOrWhiteSpace(data))
        //                empty++;
        //            else if (recognizer.IsMatch(data))
        //                count++;
        //            total++;
        //        }
        //        probabilities.AddOrUpdate(recognizer.GetType(), (float)count / total, (t, c) => (float)count / total);
        //        logger.Info($"{recognizer.GetType()}: {probabilities[recognizer.GetType()]}");
        //    });

        //}

        //private async Task DetermineProbabilityV2(IEnumerable<IRecognizer> recognizers, List<string> list)
        //{
        //    //var results = new List<string>();
        //    var probabilities = new ConcurrentDictionary<Type, float>();
        //    //float prob = 0.0f;
        //    //var queue = new BufferBlock<string>(new DataflowBlockOptions { BoundedCapacity = 1000 } );
        //    var queue = new BufferBlock<string>(new DataflowBlockOptions { BoundedCapacity = 10 });

        //    var consumerOptions = new ExecutionDataflowBlockOptions { BoundedCapacity = 1 };
        //    var consumer1 = new ActionBlock<string>(async _ => await ConsumeAsync(queue, recognizers, probabilities), consumerOptions);
        //    var consumer2 = new ActionBlock<string>(async _ => await ConsumeAsync(queue, recognizers, probabilities), consumerOptions);
        //    var consumer3 = new ActionBlock<string>(async _ => await ConsumeAsync(queue, recognizers, probabilities), consumerOptions);
        //    var consumer4 = new ActionBlock<string>(async _ => await ConsumeAsync(queue, recognizers, probabilities), consumerOptions);
        //    var consumer5 = new ActionBlock<string>(async _ => await ConsumeAsync(queue, recognizers, probabilities), consumerOptions);
        //    var consumer6 = new ActionBlock<string>(async _ => await ConsumeAsync(queue, recognizers, probabilities), consumerOptions);
        //    var consumer7 = new ActionBlock<string>(async _ => await ConsumeAsync(queue, recognizers, probabilities), consumerOptions);

        //    var consumer = ConsumeAsync(queue, recognizers, probabilities);
        //    var linkOptions = new DataflowLinkOptions { PropagateCompletion = true, };
        //    queue.LinkTo(consumer1, linkOptions);
        //    queue.LinkTo(consumer2, linkOptions);
        //    queue.LinkTo(consumer3, linkOptions);
        //    queue.LinkTo(consumer4, linkOptions);
        //    queue.LinkTo(consumer5, linkOptions);
        //    queue.LinkTo(consumer6, linkOptions);
        //    queue.LinkTo(consumer7, linkOptions);

        //    var producers = ProduceAll(queue, list);
        //    //Produce(queue,list);
        //    //await Task.WhenAll(producers, consumer1, consumer2, consumer3, consumer4, consumer5);
        //    await Task.WhenAll(producers, consumer1.Completion, consumer2.Completion, consumer3.Completion, consumer4.Completion, consumer5.Completion, consumer6.Completion, consumer7.Completion);
        //    //await Task.WhenAll(producers, consumer1.Completion);
        //    //consumer.Wait();

        //    foreach (var recognizer in recognizers)
        //    {
        //        logger.Info($"{recognizer.GetType()}: {probabilities[recognizer.GetType()]}");
        //    }
        //}

        //private static async Task ConsumeAsync(BufferBlock<string> source, IEnumerable<IRecognizer> recognizers, ConcurrentDictionary<Type, float> probabilities)
        //{
        //    while (await source.OutputAvailableAsync())
        //    {
        //        string data = String.Empty;
        //        while (source.TryReceive(out data))
        //        {
        //            foreach (var recognizer in recognizers)
        //            {
        //                //if (String.IsNullOrWhiteSpace(data))
        //                //    empty++;
        //                if (recognizer.IsMatch(data))
        //                    recognizer.IncrementCount();
        //                //recognizer.IncrementTotal();

        //            }
        //        }
        //    }
        //    //foreach (var recognizer in recognizers)
        //    //{
        //    //    int total = recognizer.Total;
        //    //    int count = recognizer.Count;
        //    //    probabilities.AddOrUpdate(recognizer.GetType(), (float)recognizer.Count / recognizer.Total, (t, c) => (float)recognizer.Count / recognizer.Total);
        //    //}
        //}

        //private static async Task Produce(BufferBlock<string> target, List<string> column)
        //{
        //    foreach (var data in column)
        //    {
        //        await target.SendAsync(data);
        //    }
        //}
    }
}

