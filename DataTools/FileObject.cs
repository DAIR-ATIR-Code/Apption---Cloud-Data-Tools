
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataTools
{
    public class FileObject
    {
        public FileObject(string filePath, bool hasHeaders = false)
        {
            HasHeaders = hasHeaders;

            FileInfo = new FileInfo(filePath);
            if (FileInfo.Exists == false)
            {
                throw new FileNotFoundException($"Can't create new FileObject with filepath: {filePath}");
            }

            DetermineEncodingAndEOL();
        }


        private CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _initialized;

        public void CancelAnalysis()
        {
            _cts.Cancel();
        }

        private void PopulateColumnMetadataDictionary(IEnumerable<int> columns)
        {
            lock (this)
            {
                var value = string.Empty;
                foreach (var pair in columns)
                {
                    SampleData.TryAdd(pair, new List<string>());
                    ColumnMetadata.Add(pair, new ColumnMetadata(pair));
                }
            }
        }

		public string GetSQLTableName()
		{
			return UserTableName == string.Empty ? (Path.GetFileNameWithoutExtension(FileInfo.Name) == "upload" ? "MyTableName" : Path.GetFileNameWithoutExtension(FileInfo.Name)) : UserTableName;
		}
		public string UserTableName { get; set; } = string.Empty;
        public int TotalRecords { get; set; } = 0;
        public long FileSize => FileInfo.Length;
        public string Filename => FileInfo.Name;
        public string FilenameNoExt => Path.GetFileNameWithoutExtension(Filename);
        public long BytesProcessed { get; private set; } = 0;
        public int PercentProcessed => (int)(BytesProcessed * 100 / Math.Max(1, FileSize));
        public int TotalNull { get; set; } = 0;
        public int TotalNonNull { get; set; } = 0;
        public List<string> SensitiveFields { get; set; } = new List<string>();
        public List<string> QuestionFields { get; set; } = new List<string>();
        public List<string> ExclamationFields { get; set; } = new List<string>();
        
        public FileInfo FileInfo { get; private set; }
        public bool HasHeaders { get; set; } = true;
        public ConcurrentDictionary<int, string> Headers { get; set; } = new ConcurrentDictionary<int, string>(); //starts at 1
        public string ConnectionString { get; set; }
        public string[] StepperText { get; set; } = { "" };

        public string GetSQLColumnName(int pos)
        {
            if (pos == 0) throw new ArgumentException("pos should always be >= 1");
            if (!Headers.ContainsKey(pos))
            {
                throw new InvalidOperationException("Invalid Column Index");
            }

            var h = Headers[pos];
            if (string.IsNullOrWhiteSpace(h))
                return "Col" + pos;
            return $"[{h}]";
        }

        public ColumnMetadata FindColumnMetadata(string colName)
        {
            var idx = Headers.FirstOrDefault(kvp => kvp.Value == colName).Key;
            if (idx == 0)
            {
                return null;
            }

            return ColumnMetadata[idx];
        }

        public List<string> FindSampleData(string colName)
        {
            var idx = Headers.FirstOrDefault(kvp => kvp.Value == colName).Key;
            if (idx == 0)
            {
                return null;
            }

            return SampleData[idx];
        }

        public void Initialize(string[] headers)
        {
            if (_initialized)
                throw new InvalidOperationException("FileObject already initialized");
            _initialized = true;
            FillHeaders(headers);
            PopulateColumnMetadataDictionary(Headers.Keys);
        }

        private void FillHeaders(string[] csvArray)
        {
            lock (this)
            {
                var fieldCount = 1;
                if (Headers.Count == 0)
                {
                    foreach (var value in csvArray)
                    {
                        Headers.TryAdd(fieldCount, HasHeaders ? value.Replace(',', '_').Replace(" ", "_").Replace("\"", string.Empty) : $"Field{fieldCount}");
                        fieldCount++;
                    }
                }
            }
        }

        public void ResetBytesProcessed()
        {
            BytesProcessed = 0;
        }

        public Dictionary<int, ColumnMetadata> ColumnMetadata { get; set; } = new Dictionary<int, ColumnMetadata>();

        public ConcurrentDictionary<int, List<string>> SampleData { get; set; } = new ConcurrentDictionary<int, List<string>>();
        public bool FillSampleData { get; set; } = true;
        public string FilePath => FileInfo.FullName;
        public int TotalColumns => ColumnMetadata.Count;

        public Encoding CurrentEncoding { get; private set; }
        private int EndOfLineSize { get; set; }

        public void AddBytesProcessed(long position)
        {
            lock (this) { BytesProcessed = position; }
        }

        private void DetermineEncodingAndEOL()
        {
            string firstRead;
            using (var csvreader = new StreamReader(FilePath))
            {
                csvreader.Peek();
                CurrentEncoding = csvreader.CurrentEncoding;
                firstRead = csvreader.ReadLine();
            }
            using (var blockreader = new StreamReader(FilePath))
            {
                var bufferLength = firstRead.Length + 2;
                var buffer = new char[bufferLength];
                var firstBlock = blockreader.ReadBlock(buffer, 0, buffer.Length);

                if (buffer[bufferLength - 2] == '\r' && buffer[bufferLength - 1] == '\n')
                {
                    // windows
                    EndOfLineSize = 2;
                }
                else if (buffer[bufferLength - 2] == '\r' || buffer[bufferLength - 2] == '\n')
                {
                    // mac or linux
                    EndOfLineSize = 1;
                }
                else
                {
                    throw new InvalidDataException("EndOfLine not recognized");
                }
            }
        }
    }
}

