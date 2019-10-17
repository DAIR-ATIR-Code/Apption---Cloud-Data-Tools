
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using DTHelperStd;
using NLog;
using RecognizerTools.State;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Unity;

namespace RecognizerTools
{
    public class SecondPass {
        private FileAnalyzer _fileAnalyzer;
        private readonly Preferences preferences;
        private static List<(Type, string, DataType)> _recognizers = new List<(Type, string, DataType)>();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IUnityContainer _container = new UnityContainer();
        private bool _started;

        private FileObject FileObject { get{ return _fileAnalyzer.FileObject; } }
        public bool IsColumnSensitive(int columnIndex)
        {
            if (_fileAnalyzer != null && (GetRecognizerTypeForDataType(FileObject.ColumnMetadata[columnIndex].DataType)?.GetInterfaces()?.Contains(typeof(ISensitiveRecognizer)) == true))
            {
                return true;
            }
            return false;
        }
        public static List<(Type, string, DataType)> GetAllRecognizerTypes()
		{
			return _recognizers;
		}
		public ColumnRecognizers[] ColumnRecognizers { get; set; } = null;
        public bool Started { get { return _started; } }

        static SecondPass()
		{
			RegisterRecognizers();
		}

        public List<IRecognizer> GetAllRecognizers()
        {
            return ColumnRecognizers.SelectMany(r => r.Recognizers).ToList();
        }

        public void CleanupProb()
        {
            //foreach (var result in SecondAnalysisResults)
            //{
            //    result.CleanupProb();
            //}
        }

        private T CreateRecognizer<T>() where T : IRecognizer
		{
			return _container.Resolve<T>();
		}

		public List<IRecognizer> GetRecognizerInstances(Type t, int column)
		{
            if (column == 0) throw new ArgumentException("column should always be >= 1");
            return ColumnRecognizers.FirstOrDefault(spa => spa.ColumnMetadata.ColumnIndex == column)?.Recognizers.Where(i => i.GetType() == t).ToList();
		}

        public List<IRecognizer> GetRecognizerInstances(ColumnMetadata column)
        {
            return GetRecognizerInstances(column.ColumnIndex);
        }

        public List<DataType> GetDataTypes(ColumnMetadata column)
        {
            return GetRecognizerInstances(column).Select(r => GetDatatypeForRecognizerType(r.GetType())).Distinct().ToList();
        }

        public List<DataType> GetDataTypesWithoutBasisTypes(ColumnMetadata column)
        {
            return GetRecognizerInstances(column).Where(r => !IsBasicRecognizer(r.GetType())).Select(r => GetDatatypeForRecognizerType(r.GetType())).Distinct().ToList();
        }

        public List<DataType> GetDataTypesDependOnProbability(ColumnMetadata column)
        {
            var recognizerList = GetRecognizerInstances(column).Where(r => (!IsBasicRecognizer(r.GetType()) && r.CurrentState.Probability > 0.4f));
            if (recognizerList.Count() >0)
            {
                return recognizerList.Select(r => GetDatatypeForRecognizerType(r.GetType())).Distinct().ToList();
            }
            return GetRecognizerInstances(column).Where(r => r.CurrentState.ProbabilityWithoutNull > 0.4f).Select(r => GetDatatypeForRecognizerType(r.GetType())).Distinct().ToList();
        }

        public List<IRecognizer> GetRecognizerInstancesDependOnProbability(ColumnMetadata column)
        {
            var recognizerWithoutBasis = GetRecognizerInstances(column).Where(r => (!IsBasicRecognizer(r.GetType()) && r.CurrentState.Probability > 0.4f));
            var recognizers = GetRecognizerInstances(column).Where(r => r.CurrentState.Probability > 0.4f);
            if (recognizerWithoutBasis.Count() > 0)
            {
                return recognizerWithoutBasis.ToList();
            }
            else if (recognizers.Count() > 0)
            {
                return recognizers.ToList();
            }else
            {
                return GetRecognizerInstances(column).Where(r => r.CurrentState.ProbabilityWithoutNull > 0.4f).ToList();
            }
        }

        public List<IRecognizer> GetRecognizerInstances(ColumnMetadata column, DataType dt)
        {
            return GetRecognizerInstances(column).Where(r => GetDatatypeForRecognizerType(r.GetType()) == dt).ToList();
        }


        public IRecognizer GetWinningRecognizerForDataType(ColumnMetadata column, DataType dt)
        {
            return GetRecognizerInstances(column).Where(r => GetDatatypeForRecognizerType(r.GetType()) == dt).OrderByDescending(r => r.CurrentState.Probability).FirstOrDefault();
        }

        public (float probably,float probabilityWithoutNull) GetProbabilitiesForDataType(ColumnMetadata column, DataType dt)
        {
            var rec = GetRecognizerInstances(column).Where(r => GetDatatypeForRecognizerType(r.GetType()) == dt).OrderByDescending(r => r.CurrentState.Probability).FirstOrDefault();
            if (rec == null)
                return (0, 0);
            return (rec.CurrentState.Probability, rec.CurrentState.ProbabilityWithoutNull);
        }

        public List<IRecognizer>GetRecognizerInstances(int column)
        {
            if (column == 0) throw new ArgumentException("column should always be >= 1");
            return ColumnRecognizers.FirstOrDefault(spa => spa.ColumnMetadata.ColumnIndex == column)?.Recognizers;
        }

        public IRecognizer GetRecognizerInstance(Type recognizerType, int column)
		{
            if (column == 0) throw new ArgumentException("column should always be >= 1");
            return GetRecognizerInstances(recognizerType, column).FirstOrDefault();
		}

        public IRecognizer GetRecognizerInstance<T>(int column) where T:IRecognizer
        {
            if (column == 0) throw new ArgumentException("column should always be >= 1");
            return GetRecognizerInstances(typeof(T), column).FirstOrDefault();
        }

        public SecondPass(FileAnalyzer fileAnalyzer, Preferences preferences)
		{
			_fileAnalyzer = fileAnalyzer;
			this.preferences = preferences;
			_container.RegisterInstance(preferences);
			ColumnRecognizers = new ColumnRecognizers[FileObject.TotalColumns];

			//Initialize the reference data dictionary
			if (ReferenceHelper.ReferenceDic.Count == 0)
			{
				ReferenceHelper.InitializeReferenceDict();
			}
            if (MLHelper.CityPredictionEngine == null)
            {
                //MLHelper.InitializeMLModel();
            }

			foreach (var kvp in FileObject.ColumnMetadata)
			{
				ColumnRecognizers[kvp.Value.ColumnIndex - 1] = new ColumnRecognizers(kvp.Value, _container);
			}
		}

        public void StartBatch(List<(int, Type, RecognizerState)> allStates = null)
        {
            if (allStates == null)
                allStates = new List<(int, Type, RecognizerState)>();
            foreach (var item in ColumnRecognizers)
            {
                foreach (var rcg in item.Recognizers)
                {
                    var existingState = allStates.FirstOrDefault(tp => tp.Item1 == item.ColumnMetadata.ColumnIndex && tp.Item2.IsInstanceOfType(rcg)).Item3 ?? new RecognizerState();
                    rcg.StartBatch(existingState);
                }
            }
            _started = true;
        }

        public List<(int, Type, RecognizerState)> EndBatch()
        {
            var allStates = new List<(int, Type, RecognizerState)>();
            foreach (var item in ColumnRecognizers)
            {
                foreach (var rcg in item.Recognizers)
                {
                    var cs = rcg.EndBatch();
                    allStates.Add((item.ColumnMetadata.ColumnIndex, rcg.GetType(), cs));
                }
            }
            _started = false;
            return allStates;
        }


        public async Task Analyze(IObserver<SecondAnalysisResult> progressMonitor = null)
		{
            if (!_started) throw new InvalidOperationException("Batch has not been initialized");
            var bufferBlock = new BufferBlock<(string[], long)>(new DataflowBlockOptions { BoundedCapacity = 1000, CancellationToken = CancellationToken.None });
            ExceptionDispatchInfo exDetails = null;
            int taskCompleted = 0;
            var taskCounts = Math.Max(2, Environment.ProcessorCount - 1);
            logger.Info($"Using {taskCounts} tasks for the analysis");
            var consumers = Enumerable.Range(0, 1)
                .Select(_ => Task.Run(async () =>
                {
                    try
                    {
                        await ConsumeAsyncV2(bufferBlock, progressMonitor);
                        Interlocked.Increment(ref taskCompleted);
                        logger.Info($"Completed {taskCompleted} out of {taskCounts} tasks");
                    }
                    catch (Exception ex)
                    {
                        logger.Fatal(ex, $"Error consuming file for second pass: {ex.Message}");
                        exDetails = ExceptionDispatchInfo.Capture(ex);
                    }
                })).ToArray();
            exDetails?.Throw();
            await _fileAnalyzer.ProcessAllLines(bufferBlock);
            logger.Info("All lines processed");
            await Task.WhenAll(consumers);
            logger.Info("Analysis completed");
		}


		private async Task ConsumeAsyncV2(BufferBlock<(string[], long)> source, IObserver<SecondAnalysisResult> progressMonitor)
		{
			DateTime lastUpdate = DateTime.MinValue;
			int dataLength = 0;
			while (await source.OutputAvailableAsync())
			{

				while (source.TryReceive(out var data))
				{
					dataLength = data.Item1.Length;
					bool updateStats = false;

					if (!(progressMonitor is null) && (DateTime.Now - lastUpdate) > TimeSpan.FromSeconds(preferences.UIUpdateFrequencySecondPass))
					{
						updateStats = true;
						lastUpdate = DateTime.Now;
					}

					for (var i = 0; i < data.Item1.Length; i++)
					{
                        var metadata = FileObject.ColumnMetadata[i+1];
						if (ColumnRecognizers[i].Recognizers != null)
						{
							foreach (var recognizer in ColumnRecognizers[i].Recognizers)
							{
                                if (recognizer.IsMatch(data.Item1[i], ColumnRecognizers[i].Token))
								{
									recognizer.IncrementCount();
								}
                                if (updateStats)
                                {
                                    UpdateRecognizerStats(recognizer, metadata);
                                    progressMonitor.OnNext(ColumnRecognizers[i].SecondAnalysisResult);
                                }
                            }

						}
					}
                    FileObject.AddBytesProcessed(data.Item2);

				}
			}
			// update stats one last time (the 2 second increment above wont make it to 100%
			for (var i = 0; i < dataLength; i++)
			{
                var metadata = FileObject.ColumnMetadata[i+1];
                foreach (var recognizer in ColumnRecognizers[i].Recognizers)
                {
                    UpdateRecognizerStats(recognizer, metadata);
                }
				progressMonitor?.OnNext(ColumnRecognizers[i].SecondAnalysisResult);
			}
		}

        private void UpdateRecognizerStats(IRecognizer recognizer, ColumnMetadata metadata)
        {

            recognizer.CurrentState.Probability = 0;
            recognizer.CurrentState.ProbabilityWithoutNull = 0;
            recognizer.CurrentState.Summary = recognizer.GetStatus();
            recognizer.CurrentState.Probability = (float)recognizer.Count / metadata.TotalRecords;
            
            if (metadata.TotalRecords != metadata.TotalNulls)
            {
                recognizer.CurrentState.ProbabilityWithoutNull = (float)recognizer.Count / (metadata.TotalRecords - metadata.TotalNulls);                
            }
            else
            {
                recognizer.CurrentState.ProbabilityWithoutNull = 0;
            }

        }



		private static void RegisterRecognizer<T>(string name, DataType dt) where T : IRecognizer
		{
			if (_recognizers.Select(tp => tp.Item1).Contains(typeof(T)))
			{
				throw new InvalidDataException($"Recognizer {typeof(T).ToString()} has already been registered");
			}

			_recognizers.Add((typeof(T), name, dt));
		}

		public static void RegisterRecognizers()
		{
			RegisterRecognizer<CreditCardRecognizer>("CreditCard", DataType.CreditCard);
			RegisterRecognizer<DateRecognizer>("Date", DataType.Date);
			RegisterRecognizer<IPv4Recognizer>("IPv4", DataType.IPv4);
			RegisterRecognizer<LatitudeOrLongitudeRecognizer>("LatitudeOrLongitude", DataType.Latitude);
			RegisterRecognizer<MoneyRecognizer>("Money", DataType.Money);
			RegisterRecognizer<SINRecognizer>("SIN", DataType.SIN);
			RegisterRecognizer<PhoneRecognizer>("Phone", DataType.Phone);
			RegisterRecognizer<ISBNRecognizer>("ISBN", DataType.ISBN);
			RegisterRecognizer<NumberRecognizer>("Number", DataType.Number);
			RegisterRecognizer<FirstNameRecognizer>("FirstName", DataType.FirstName);
			RegisterRecognizer<LastNameRecognizer>("LastName", DataType.LastName);

			RegisterRecognizer<TitleRecognizer>("Title", DataType.Title);
			RegisterRecognizer<CountryRecognizer>("Country", DataType.Country);
			RegisterRecognizer<EthnicityRecognizer>("Ethnicity", DataType.Ethnicity);
			RegisterRecognizer<StockSymbolsRecognizer>("StockSymbols", DataType.StockSymbol);
			//RegisterRecognizer<NameRecognizer>("Name", DataType.Name);
			RegisterRecognizer<GenderRecognizer>("Gender", DataType.Gender);
			RegisterRecognizer<CurrencyCodeRecognizer>("CurrencyCode", DataType.CurrencyCode);
			RegisterRecognizer<CityRecognizer>("City", DataType.City);
			RegisterRecognizer<LetterRecognizer>("Text", DataType.String);

			RegisterRecognizer<EmailRecognizer>("Email", DataType.Email);
			RegisterRecognizer<AddressRecognizer>("Address", DataType.Address);
			RegisterRecognizer<HealthcardRecognizer>("Healthcard", DataType.Healthcard);
			RegisterRecognizer<HTTPRecognizer>("HTTP", DataType.HTTP);
			//RegisterRecognizer<UsernameRecognizer>("Username", DataType.Username);
			RegisterRecognizer<PostalCodeRecognizer>("PostalCode", DataType.PostalCode);
			RegisterRecognizer<DateSpecialRecognizer>("DateSpecial", DataType.Date_Special);
			RegisterRecognizer<IPv6Recognizer>("IPv6", DataType.IPv6);
			RegisterRecognizer<MarketCapRecognizer>("MarketCap", DataType.MarketCap);
			RegisterRecognizer<LetterWithNumberRecognizer>("Text(With Number)", DataType.Any);

		}


		public static Type GetRecognizerTypeForDataType(DataType selectDataType)
		{
			return _recognizers.Find(tp => tp.Item3 == selectDataType).Item1;
		}

		public static DataType GetDatatypeForRecognizerType(Type recognizer)
		{
			return _recognizers.Find(tp => tp.Item1 == recognizer).Item3;
		}

        public static bool IsSensitive(ColumnMetadata column)
        {
            return IsSensitive(column.DataType);
        }

        public static bool IsSensitive(DataType dataType)
        {

            if (GetRecognizerTypeForDataType(dataType)?.GetInterfaces()?.Contains(typeof(ISensitiveRecognizer)) == true)
            {
                return true;
            }
            return false;
        }
        public static bool IsBasicRecognizer(Type recognizer)
        {
            return recognizer == typeof(NumberRecognizer) || recognizer == typeof(LetterRecognizer) || recognizer == typeof(LetterWithNumberRecognizer);
        }
    }
}
