
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DTHelperStd;
using NLog;
using System;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataTools
{
    public class FileAnalyzer: IDisposable
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        private const int MINIMUMSAMPLERECORDS = 1000;
        private const int SQLITEBATCHSIZE = 1500000;
        public FileObject FileObject;
        private RegexHelper _regex;
        public RegexHelper RegexHelper { get { return _regex; }  }

        private readonly object fileObjectLock = new object();
        private static string DatabaseFilePath = $"{PathHelper.GetFolderRelativeToProject("SQLite3")}\\filedata.db";
        private static CultureInfo CACulture = CultureInfo.GetCultureInfo("en-CA");
        private static CultureInfo GBCulture = CultureInfo.GetCultureInfo("en-GB");

		public SQLiteConnectionStringBuilder ConnectionString = new SQLiteConnectionStringBuilder
        {
            Version = 3,
            DataSource = DatabaseFilePath
        };
        //private bool _started;
        private Preferences _prefs;

        private SeparatorType? separator;
        private readonly char customeSeparator;

        public SeparatorType? Separator
        {
            get { return separator; }
            set { separator = value;
                if (separator.HasValue)
                    _regex = new RegexHelper(GetStringForSeparator(separator.Value, customeSeparator));
            }
        }


        public FileAnalyzer(string filePath, Preferences prefs, bool hasHeaders = false, SeparatorType? seperator = null, char userSeparator = '\0')
        {
            _prefs = prefs;
            customeSeparator = userSeparator;
            Separator = seperator;
            FileObject = new FileObject(filePath, hasHeaders);            
        }

        public async Task ProcessAllLines(BufferBlock<(string[], long)> target)
        {

            using (var fileStream = File.Open(FileObject.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var fileReader = new StreamReader(fileStream))
                {
                    string line;
                    if (FileObject.HasHeaders)
                    {
                        fileReader.ReadLine();
                    }

                    while ((line = await fileReader.ReadLineAsync()) != null)
                    {
                        var data = _regex.SplitLineIgnoreQuotedCommas(line, 0);
                        await target.SendAsync((data, fileStream.Position));
                    }
                    target.Complete();
                }
            }
        }


        public async Task AnalyzeAsync(CancellationToken tk)
        {
            await StreamReadFileAsync(tk);
        }

        public (IObservable<FileObject>, TaskCompletionSource<bool>) AnalyzeAsyncWithObserver(CancellationToken tk)
        {           
            var tcs = new TaskCompletionSource<bool>();            
            var observable = new Subject<FileObject>();
            var t = Task.Run(async () =>
            {
                try
                {
                    await StreamReadFileAsync(tk, observable);
                } catch (Exception ex)
                {
                    observable.OnError(ex);
                    Logger.Error(ex);
                }
            });
            observable.Subscribe(FileObject => { }, exception => { tcs.SetException(exception); }, () => { tcs.SetResult(true); });
            return (observable, tcs);
        }

        private async Task StreamReadFileAsync(CancellationToken tk, IObserver<FileObject> observer = null)
        {
            try
            {
                var count = 0;
                var lineNumber = 0;
                using (var fileStream = File.Open(FileObject.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        var inputLine = string.Empty;
                        var target = new BatchBlock<(string[], long)>(5000);
                        var sqlTarget = new BatchBlock<string[]>(5000);

                        //Must perform first iteration outside of blocking collection
                        var firstLine = _regex.SplitLineIgnoreQuotedCommas(await streamReader.ReadLineAsync(), 0);
                        lineNumber++;
                        if (firstLine != null)
                        {
							try
							{
								FileObject.Initialize(firstLine);
								if (!FileObject.HasHeaders)
								{
									count++;
									FillColumnMetaData(firstLine);
									sqlTarget.Post(firstLine);
								}
								FileObject.AddBytesProcessed(fileStream.Position);
							}
							catch (Exception ex)
							{
								Logger.Error(ex, $"Error processing first line of file");
								throw;
							}
						}


                        var consumer = Task.Run(async () => await ProcessFirstPass(observer, count, target));
                        var sqlConsumer = Task.Run(async () => await FillSQLiteTableAsync(observer, sqlTarget));

                        while (!tk.IsCancellationRequested && (inputLine = await streamReader.ReadLineAsync()) != null)
                        {
                            lineNumber++;
                            try
                            {                                
                                var strArray = _regex.SplitLineIgnoreQuotedCommas(inputLine, FileObject.TotalColumns);
                                target.Post((strArray, fileStream.Position));
                                sqlTarget.Post(strArray);
                            }
                            catch (Exception ex)
                            {
                                observer?.OnError(new InvalidOperationException($"Error with {FileObject.Filename} on line {lineNumber}", ex));
								Logger.Error(ex, $"Error with {FileObject.Filename} on line {lineNumber}");
								throw;
                            }
                        }

                        target.Complete();
                        sqlTarget.Complete();
                        consumer.Wait(tk);
                        try
                        {
                            FileObject.StepperText.SetValue("Preparing review...", 0);
                            observer?.OnNext(FileObject);
                            sqlConsumer.Wait(tk);
                        }
                        catch (Exception ex)
                        {
                            observer?.OnError(new InvalidOperationException($"Error loading data into SQLite", ex));
							Logger.Error(ex, $"Error loading data into SQLite");
							throw;
                        }
                        try
                        {
                            FileObject.StepperText.SetValue("Checking the primary key...", 0);
                            observer?.OnNext(FileObject);
                            CheckForFieldUniqueness();
                        }
                        catch (Exception ex)
                        {
                            observer?.OnError(new InvalidOperationException($"Error Checking for Field Uniqueness", ex));
							Logger.Error(ex, $"Error Checking for Field Uniqueness") ;
                            throw;
                        }
                        FileObject.StepperText.SetValue("Generating report data...", 0);
                        observer?.OnNext(FileObject);
                        FirstPassCompleted();
                    }
                }

                observer?.OnCompleted();
            } catch (Exception ex)
            {
				observer?.OnError(ex);
				Logger.Error(ex, "Exception occured while processing first pass");
				throw;
            }
        }

        private void FirstPassCompleted()
        {
            foreach (var kvp in FileObject.ColumnMetadata)
            {
                kvp.Value.TotalRecords = FileObject.TotalRecords;
                kvp.Value.UserIsNullable = kvp.Value.IsNullable;
                kvp.Value.FieldName = FileObject.Headers[kvp.Key];
                kvp.Value.SetFieldName(kvp.Value.FieldName);

                FileObject.TotalNull += kvp.Value.TotalNulls;
                FileObject.TotalNonNull += kvp.Value.TotalNonNullRecords;

                if (kvp.Value.TotalRecords != kvp.Value.TotalNulls)
                {
                    kvp.Value.UserSize = _prefs.RoundVarcharSize ? (int)Math.Ceiling(kvp.Value.MaxLength * 1.0 / 10.0) * 10 : kvp.Value.MaxLength;
                    kvp.Value.IsCompletelyLetter = ((float)kvp.Value.StringWithOnlyLetterCount / (kvp.Value.TotalRecords - kvp.Value.TotalNulls)) * 100 > _prefs.RecognizerThresHold;
                    kvp.Value.IsNumberWithSpecialCharacters = ((float)kvp.Value.StringWithOnlyNumberCount / (kvp.Value.TotalRecords - kvp.Value.TotalNulls)) * 100 > _prefs.RecognizerThresHold;
                }
                //Case for all null column
                else
                {
                    kvp.Value.IsInt = false;
                    kvp.Value.IsNumber = false;
                    kvp.Value.IsNumberWithSpecialCharacters = false;
                    kvp.Value.IsCompletelyLetter = false;
                    kvp.Value.UserSize = _prefs.VarCharSizeForEmptyColums;
                }
                kvp.Value.CheckPrimaryKey();

                //Assign the DataType by columnmetadata 
                var md = kvp.Value;
                if (md.IsInt)
                {
                    md.DataType = DataType.Number;
                    md.UserDataType = DataType.Number;

                    if (md.MaxValue > int.MaxValue || md.MinValue < int.MinValue)
                    {
                        md.StorageType = StorageType.Bigint;
                        md.UserStorageType = StorageType.Bigint;
                    }
                    else
                    {
                        md.StorageType = StorageType.Int;
                        md.UserStorageType = StorageType.Int;
                    }
                }
                else if (md.IsDecimal)
                {
                    md.DataType = DataType.Number;
                    md.UserDataType = DataType.Number;
                    md.StorageType = StorageType.Decimal;
                    md.UserStorageType = StorageType.Decimal;
                }
                //case for storageType char
                if (md.IsCompletelyLetter && md.MaxLength == md.AverageLength)
                {
                    md.StorageType = StorageType.Char;
                    md.UserStorageType = StorageType.Char;
                    md.UserSize = md.MaxLength;
                }
            }
        }

        private async Task<int> ProcessFirstPass(IObserver<FileObject> observer, int count, BatchBlock<(string[], long)> target)
        {
            while (await target.OutputAvailableAsync())
            {
                foreach (var csvArray in target.Receive())
                {
                    Interlocked.Increment(ref count);
                    if (FileObject.SampleData.All(x => x.Value.Count >= MINIMUMSAMPLERECORDS))
                    {
                        FileObject.FillSampleData = false;
                    }
                    FileObject.TotalRecords = count;
                    FillColumnMetaData(csvArray.Item1);
                    FileObject.AddBytesProcessed(csvArray.Item2);
                    observer?.OnNext(FileObject);
                }
            }
            return count;
        }

        private void CheckForFieldUniqueness()
        {
            using (var conn = new SQLiteConnection(ConnectionString.ToString()))
            {
                conn.Open();

                var command = conn.CreateCommand();
                var obj = new object();
                foreach (var metaData in FileObject.ColumnMetadata.Where(x => x.Value.TotalNulls == 0))
                {
                    command.CommandText = "SELECT COUNT (DISTINCT \"" + StringHelper.GetSQLiteColumnName(metaData.Key) + "\") FROM ImportFile";
                    obj = command.ExecuteScalar();
                    metaData.Value.IsUnique = Convert.ToInt32(obj) == FileObject.TotalRecords;
                }
            }
        }
        private async Task FillSQLiteTableAsync(IObserver<FileObject> observer, BatchBlock<string[]> sqlTarget)
        {
            try
            {
                var (createTableScript, columnList) = GenerateSQLiteSchema();
                if (_prefs.UseTempDBFile)
                    DatabaseFilePath = Path.GetTempFileName() + ".db";
                else
                {
                    //Workaround for Electron Path problems.
                    if (Path.GetFileName(Environment.CurrentDirectory) == ".bin")
                    {
                        DatabaseFilePath = Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\..\SQLite3\filedata.db");
                    }
                }


                ConnectionString = new SQLiteConnectionStringBuilder
                {
                    Version = 3,
                    DataSource = DatabaseFilePath
                };

                using (var conn = new SQLiteConnection(ConnectionString.ToString(), true))
                {
                    try
                    {
                        conn.Open();

                        using (var command = new SQLiteCommand(conn))
                        {
                            command.CommandText = $"DROP TABLE IF EXISTS ImportFile; {createTableScript.ToString()}";
                            command.ExecuteNonQuery();
                            var values = new StringBuilder();
                            var line = string.Empty;
                            var processedCount = 0;
                            SQLiteTransaction transaction = null;

                            while (await sqlTarget.OutputAvailableAsync())
                            {
                                foreach (var csvArray in sqlTarget.Receive())
                                {
                                    try
                                    {
                                        if (processedCount == 0)
                                        {
                                            transaction = conn.BeginTransaction();
                                        }

                                        values.Clear();

                                        for (var i = 0; i < csvArray.Length; i++)
                                        {
                                            values.Append($"'{csvArray[i].Replace("'", "''")}', ");
                                        }
                                        values.Length = values.Length - 2;
                                        command.CommandText = $"INSERT INTO ImportFile {columnList.ToString()} VALUES ({values.ToString()}); ";
                                        command.ExecuteNonQuery();
                                        processedCount++;

                                        if (processedCount == SQLITEBATCHSIZE)
                                        {
                                            transaction.Commit();
                                            processedCount = 0;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        observer.OnError(ex);
                                        Logger.Error(ex, "Error inserting data into SQLite");
                                        throw;
                                    }
                                }
                            }
                            transaction.Commit();
                        }
                    } finally {
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                observer?.OnError(ex);
                Logger.Error(ex, "Error loading into SQLlite");
                throw;
            }
        }
        private void FillColumnMetaData(string[] csvArrayLine)
        {
            var index = 0;
            foreach (var colValue in csvArrayLine)
            {
                //index++;
                Interlocked.Increment(ref index);
                var currentHeader = FileObject.ColumnMetadata[index];
                if (FileObject.FillSampleData)
                {
                    FileObject.SampleData[index].Add(colValue);
                }

                var valLength = colValue.Length;
                var valueWordCount = StringHelper.CountWords(colValue);

                //replace any symbols with empty string
                var cleanData = StringHelper.CleanSpecialCharacters(colValue);
               
                currentHeader.TotalLength += valLength;

                if (valLength == 0)
                {
                    currentHeader.IncrementNullCounter();
                }
                else
                {
                    currentHeader.TotalWords += valueWordCount;

                    if (currentHeader.MaxLength < valLength)
                    {
                        currentHeader.MaxLength = valLength;
                    }

                    if (currentHeader.MinLengthExceptNull == 0)
                    {
                        currentHeader.MinLengthExceptNull = valLength;
                    }

                    if (currentHeader.MinLengthExceptNull > valLength)
                    {
                        currentHeader.MinLengthExceptNull = valLength;
                    }

					if (double.TryParse(colValue, NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint, CACulture, out var r) ||
                        double.TryParse(colValue, NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint, GBCulture, out r))
					{
						if (currentHeader.MaxValue is null || r > currentHeader.MaxValue)
						{
							currentHeader.MaxValue = (long)r;
						}

						if (currentHeader.MinValue is null || r < currentHeader.MinValue)
						{
							currentHeader.MinValue = (long)r;
						}
					}

					if (currentHeader.IsInt && !long.TryParse(colValue, out var i))
					{
						currentHeader.IsInt = false;
					}

                    if (currentHeader.IsNumber && !double.TryParse(colValue, out var d))
                    {
                        currentHeader.IsNumber = false;
                    }

                    if (double.TryParse(cleanData, out var e))
					{
                        currentHeader.IncrementNumberOnlyCount();
                    }

                    if (StringHelper.IsLetterOnly(cleanData))
                    {
                        currentHeader.IncrementLetterOnlyCount();
                    }
                }
            }
        }

        private (string, string) GenerateSQLiteSchema()
        {
            var createTableScript = new StringBuilder();
            var columnList = new StringBuilder();
            using (var csvReader = new StreamReader(FileObject.FilePath))
            {
                var columnCount = _regex.SplitLineIgnoreQuotedCommas(csvReader.ReadLine(), 0).Length;

                columnList.Append("(");
                createTableScript.Append("CREATE TABLE IF NOT EXISTS ImportFile ( \n");
                for (var i = 1; i <= columnCount; i++)
                {
                    createTableScript.Append($"{StringHelper.GetSQLiteColumnName(i)} TEXT NULL,\n");
                    columnList.Append($"{StringHelper.GetSQLiteColumnName(i)},");
                }
                createTableScript.Length = createTableScript.Length - 2;
                createTableScript.Append(");");
                columnList.Length = columnList.Length - 1;
                columnList.Append(")");
            }

            return (createTableScript.ToString(), columnList.ToString());
        }
        public static char GetStringForSeparator(SeparatorType separator, char customSeparator)
        {
            switch (separator)
            {
                case SeparatorType.Comma:
                    return ',';
                case SeparatorType.Semicolon:
                    return ';';
                case SeparatorType.Tab:
                    return '\t';
                case SeparatorType.Pipe:
                    return '|';
                case SeparatorType.Custom:
                    return customSeparator;
                default:
                    throw new InvalidDataException("Invalid separator");
            }
        }

        public void Dispose()
        {
            if (_prefs.UseTempDBFile)
            {
                try
                {
                    File.Delete(DatabaseFilePath);
                } catch (Exception ex)
                {
                    Logger.Warn(ex, $"Cannot delete file {DatabaseFilePath}");
                }
                
            }
        }
    }
}

