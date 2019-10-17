
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace RecognizerTools.SQL
{
    public class SQLDataReader : IDataReader
    {
        private long totalRecords;
        private FileObject fileObject;
        private BufferBlock<(string[], long)> queue;
        private (string[], long) data;
        private long _rowsCopied = 0;

        public long RowsCopied
        {
            get { return _rowsCopied; }
        }


        public SQLDataReader(FileObject fileObject, BufferBlock<(string[], long)> queue)
        {
            this.totalRecords = fileObject.TotalRecords;
            this.fileObject = fileObject;
            this.queue = queue;
        }

        public object this[int i] => throw new NotImplementedException();

        public object this[string name] => throw new NotImplementedException();

        public int Depth => throw new NotImplementedException();

        public bool IsClosed => throw new NotImplementedException();

        public int RecordsAffected => throw new NotImplementedException();

        public int FieldCount => this.fileObject.TotalColumns;

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int i)
        {
            var md = fileObject.ColumnMetadata[i + 1];
            var currentValue = data.Item1[i];
            try
            {
                //this is THE METHOD
                switch (md.StorageType)
                {
                    case StorageType.Char:
                        return currentValue.ToCharArray();
                    case StorageType.Varchar:
                    case StorageType.Nchar:
                    case StorageType.NVarchar:
                        return currentValue;
                    case StorageType.Decimal:
                        if (md.IsNumberWithMinus)
                        {
                            if (currentValue.EndsWith("-"))
                            {
                                currentValue = currentValue.Replace("-", "").Insert(0, "-");
                            }
                        }
                        return Convert.ToDecimal(currentValue);
                    case StorageType.Int:
                        if (md.IsNumberWithMinus)
                        {
                            if (currentValue.EndsWith("-"))
                            {
                                currentValue = currentValue.Replace("-", "").Insert(0, "-");
                            }
                        }
                        return Convert.ToInt32(currentValue);
                    case StorageType.Bigint:
                        return Convert.ToInt64(currentValue);
                    case StorageType.Smallint:
                        return Convert.ToInt16(currentValue);
                    case StorageType.Bit:
                        return Convert.ToBoolean(currentValue);
                    case StorageType.Datetime2:
                    case StorageType.Date:
                    case StorageType.Datetime:
                        return DateTime.ParseExact(currentValue, md.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                    case StorageType.Uniqueidentifier:
                        return currentValue;
					case StorageType.Money:
						return currentValue.Replace("$","");
                    default:
                        throw new NotImplementedException();
                }
            } catch (Exception ex)
            {
                throw new InvalidOperationException($"Error converting value '{currentValue}' to {md.StorageType}",ex);
            }
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return fileObject.ColumnMetadata[i + 1].IsNullable && String.IsNullOrWhiteSpace(data.Item1[i]);
        }

        //public bool NextResult()
        //{
            
        //}

        public bool NextResult()
        {
            return (_rowsCopied < totalRecords);
        }

        public bool Read()
        {
            if (_rowsCopied >= totalRecords)
                return false;
            data = queue.Receive();
            _rowsCopied++;
            return true;
        }
    }
}

