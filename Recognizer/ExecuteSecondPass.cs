
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NLog;

namespace RecognizerTools
{
    public class ExecuteSecondPass
    {
        private IObserver<SecondAnalysisResult> progressMonitor;
        private FileObject fileObject;
        private SecondPass secondPass;
        private static float MAX_PROB = 1.0f;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, int> NoHeaderFieldNameDictionary;

        public ExecuteSecondPass(SecondPass secondPass = null)
        {
            this.secondPass = secondPass;
        }

        //    var storageTypes = (StorageType[])Enum.GetValues(typeof(StorageType));
        //    if (SecondAnalysisResult != null && ColumnMetadata.DataType != DataType.Custom)
        //    {
        //        var selectType = SecondPassAnalyzerCollection.GetRecognizerForDataType(ColumnMetadata.DataType);
        //        var instance = SecondPassAnalyzers?.GetRecognizerInstance(selectType, ColumnMetadata.ColumnIndex);
        //        storageTypes = instance == null ? new StorageType[] { StorageType.Varchar} : instance.GetStorageTypes();
        //    }

        public async Task PerformSecondPassAnalysis()
        {
            secondPass.StartBatch();
            await secondPass.Analyze(progressMonitor);

            //Case for no header
            InitializeNoHeaderDictionary();

            for (var i = 0; i < fileObject.TotalColumns; i++)
            {
                var column = fileObject.ColumnMetadata[i + 1];

                var instances = secondPass.GetRecognizerInstances(i + 1).Where(r => r.CurrentState.Probability > 0).OrderByDescending(r => r.CurrentState.Probability);
                var instancesWithoutBasisType = instances?.Where(r => r.CurrentState.Probability > 0.4 && !SecondPass.IsBasicRecognizer(r.GetType()));

                var instance = instancesWithoutBasisType?.Count() > 0 ? instancesWithoutBasisType?.FirstOrDefault() : instances?.FirstOrDefault();

                var SelectDataType = instance == null ? DataType.String : SecondPass.GetDatatypeForRecognizerType(instance?.GetType());
                column.DataType = SelectDataType;
                column.SetDataType(SelectDataType);

                column.ValidStorageTypes = instance == null ? new StorageType[] { StorageType.Varchar } : instance.GetStorageTypes();
                column.SecondPassCompleted = true;

                column.StorageType = column.ValidStorageTypes.FirstOrDefault();
				column.UserStorageType = column.ValidStorageTypes.FirstOrDefault();

                //Case for Datetime
				if (typeof(DateRecognizer).IsInstanceOfType(instance))
                {
                    var dt = instance as DateRecognizer;
                    column.DateTimeFormat = dt.DateFormatTypes.ToArray();
                    if (column.DateTimeFormat is null)
                    {
                        logger.Fatal($"Error: Cannot determine date format for column {i + 1}");
                        throw new InvalidOperationException($"Cannot determine date format for column {i + 1}");
                    }
                }

                //Case for Date involve char fg, 19-Mar-2017
                if (typeof(DateSpecialRecognizer).IsInstanceOfType(instance))
                {
                    var dt = instance as DateSpecialRecognizer;
                    column.DateTimeFormat = dt.DateFormatTypes.ToArray();
                    if (column.DateTimeFormat is null)
                    {
                        logger.Fatal($"Error: Cannot determine date format for column {i + 1}");
                        throw new InvalidOperationException($"Cannot determine date format for column {i + 1}");
                    }
                }

				//Case for not 100% number
				if (typeof(NumberRecognizer).IsInstanceOfType(instance))
				{
					var dt = instance as NumberRecognizer;
					column.IsNumberWithMinus = dt._isNumberWithMinus;
					//Warning: Data is not complete number, involve some characters
					if (instance.CurrentState.Probability != MAX_PROB)
					{
						logger.Warn($"Warning: Column{i + 1} identify as Number but involving characters");
						column.StorageType = StorageType.Varchar;
						column.UserStorageType = StorageType.Varchar;
					}
				}
				//Case for storageType char
				if (column.MaxLength == column.AverageLength &&
					column.StorageType == StorageType.Varchar &&
					column.TotalNulls != column.TotalRecords)
				{
					column.StorageType = StorageType.Char;
					column.UserStorageType = StorageType.Char;
					column.UserSize = column.MaxLength;
				}

				//Case for file no header
				if (!fileObject.HasHeaders)
				{
					if (column.IsUnique && column.DataType == DataType.Number)
					{
						column.SetFieldName("id");
						if (NoHeaderFieldNameDictionary["id"] == 0)
						{
							NoHeaderFieldNameDictionary["id"]++;
						}
						else
						{
                            column.SetFieldName(column.FieldName+ NoHeaderFieldNameDictionary["id"]++);
						}
						fileObject.Headers[column.ColumnIndex] = column.FieldName;
					}
					else
					{
						if (column.DataType == DataType.Any)
						{
							column.SetFieldName("TextValue");
						}
						else
						{
                            column.SetFieldName((column.DataType == DataType.Date || column.DataType == DataType.String || column.DataType == DataType.Money) ? string.Format("{0}Value", column.DataType.ToString()) : column.DataType.ToString());

						}
						if (NoHeaderFieldNameDictionary[column.DataType.ToString()] == 0)
						{
							NoHeaderFieldNameDictionary[column.DataType.ToString()]++;
						}
						else
						{
                            column.SetFieldName(column.FieldName + NoHeaderFieldNameDictionary[column.DataType.ToString()]++);
						}
                        fileObject.Headers[column.ColumnIndex] = column.FieldName;
                    }
                }

                //Assign the number of sensitive data, question marks and exclamation marks
                if (column.IsExclamationMark)
                {
                    fileObject.ExclamationFields.Add($"Field{column.ColumnIndex} ({column.DataType})");
                }
                if (column.IsQuestionMark)
                {
                    fileObject.QuestionFields.Add($"Field{column.ColumnIndex}   ({column.DataType})");
                }
                if (IsSensitive(column.ColumnIndex))
                {
                    fileObject.SensitiveFields.Add($"Field{column.ColumnIndex}   ({column.DataType})");
                }

            }
            progressMonitor.OnCompleted();
		}


        private void InitializeNoHeaderDictionary()
		{
			NoHeaderFieldNameDictionary = new Dictionary<string, int>();
			foreach (var dataType in Enum.GetValues(typeof(DataType)))
			{
				NoHeaderFieldNameDictionary.Add(dataType.ToString(), 0);
			}
			NoHeaderFieldNameDictionary.Add("id", 0);
		}

		public (SecondPass, Subject<SecondAnalysisResult>) Initialize(FileAnalyzer fileAnalyzer, Preferences preferences)
		{
			this.fileObject = fileAnalyzer.FileObject;
            if (secondPass == null)
                secondPass = new SecondPass(fileAnalyzer, preferences);
            //Assign the cancellationToke in GeneratorColumn to secondPassAnalyzer
            //for (var i = 0; i < fileObject.TotalColumns; i++)
            //{
            //    secondPass.ColumnRecognizers[i].Token = fileObject.ColumnMetadata[i + 1].AnalysisToken;
            //}

            var subject = new Subject<SecondAnalysisResult>();
            progressMonitor = subject;
            return (secondPass,subject);
        }

        public bool IsSensitive(int columnIndex)
        {
            if (SecondPass.GetRecognizerTypeForDataType(fileObject.ColumnMetadata[columnIndex].DataType)?.GetInterfaces()?.Contains(typeof(ISensitiveRecognizer)) == true)
            {
                return true;
            }
            return false;
        }
    }
}

