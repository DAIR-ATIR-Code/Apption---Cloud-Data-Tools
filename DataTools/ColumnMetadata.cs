
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace DataTools
{

	public enum StorageType
	{
		Varchar,
		NVarchar,
		Decimal,
		Int,
		Bigint,
		Smallint,
		Char,
		Nchar,
		Bit,
		Datetime2,
		Datetime,
		Uniqueidentifier,
        Date,
        Money,
    }

	public enum DataType
	{
		String,
		Number,
		City,
		Country,
		CurrencyCode,
		Ethnicity,
		Gender,
		Name,
		StockSymbol,
		Title,
		CreditCard,
		Date,
		IPv4,
		ISBN,
		Latitude,
		Money,
		Phone,
		SIN,
		Address,
		Date_Special,
		Email,
		Healthcard,
		HTTP,
		IPv6,
		Any,
		MarketCap,
		PostalCode,
		Username,
		Custom,
        FirstName,
        LastName
    }

	public enum SeparatorType
	{
		Comma,
		Semicolon,
		Tab,
		Pipe,
        Custom
	}

    [DataContract]
	public class ColumnMetadata
	{
		public ColumnMetadata(int key)
		{
			ColumnIndex = key;
            ValidStorageTypes = (StorageType[])Enum.GetValues(typeof(StorageType));
        }

        //public RecognizerSummary RecognizerSummary { get; set; };
        [DataMember]
        public float Probability { get; set; }
        [DataMember]
        public float ProbabilityWithoutNull { get; set; }

        [DataMember]
        private string _fieldName;

        public string FieldName
        {
            get {
                if (!string.IsNullOrWhiteSpace(UserFieldName))
                    return UserFieldName;
                return _fieldName;
            }
            set { _fieldName = value; }
        }

        [DataMember]
        public string UserFieldName { private get; set; } = null;

        [DataMember]
        private StorageType storageType;

        public StorageType StorageType
        {
            get
            {
                return UserStorageType == null ? storageType : UserStorageType.Value;
            }
            set { storageType = value; }
        }

        [DataMember]
        public StorageType? UserStorageType { private get; set; } = null;
        [DataMember]
        public int MaxLength { get; set; } = 0;
        [DataMember]
        public int UserSize { get; set; }
		public bool IsNullable { get { return TotalNulls > 0; } }
        [DataMember]
        public bool UserIsNullable { get; set; } = false;

        [DataMember]
        private DataType dataType;
        private DataType recognizedDataType;

        public DataType DataType
        {
            get {
                return UserDataType ?? recognizedDataType;
            }
            set { recognizedDataType = value; }
        }

        [DataMember]
        public DataType? UserDataType { private get; set; }

        public void SetFieldName(string name)
        {
            UserFieldName = name;
        }

        public void SetStorageType(StorageType storage)
        {
            UserStorageType = storage;
        }

        public void SetDataType(DataType dataType)
        {
            UserDataType = dataType;
        }
        public SqlDbType GetSqlDbType()
        {
            switch (StorageType)
            {
                case StorageType.Varchar:
                    return SqlDbType.VarChar;
                case StorageType.NVarchar:
                    return SqlDbType.NVarChar;
                case StorageType.Decimal:
                    return SqlDbType.Decimal;
                case StorageType.Int:
                    return SqlDbType.Int;
                case StorageType.Bigint:
                    return SqlDbType.BigInt;
                case StorageType.Smallint:
                    return SqlDbType.SmallInt;
                case StorageType.Char:
                    return SqlDbType.Char;
                case StorageType.Nchar:
                    return SqlDbType.NChar;
                case StorageType.Bit:
                    return SqlDbType.Bit;
                case StorageType.Datetime2:
                    return SqlDbType.DateTime2;
                case StorageType.Datetime:
                    return SqlDbType.DateTime;
                case StorageType.Uniqueidentifier:
                    return SqlDbType.UniqueIdentifier;
                case StorageType.Date:
                    return SqlDbType.Date;
                case StorageType.Money:
                    return SqlDbType.Money;
                default:
                    throw new InvalidOperationException($"Type {StorageType} is not mapped");
            }
        }

        [DataMember]
        public SeparatorType Separator { get; set; }
        [DataMember]
        private int NullCounter = 0;
        [DataMember]
        private int _stringWithOnlyNumberCount = 0;
        [DataMember]
        private int _stringWithOnlyLetterCount = 0;
        [DataMember]
        public int ColumnIndex { get; set; } = 0;
        [DataMember]
        public int MinLengthExceptNull { get; set; } = 0;
        [DataMember]
        public long? MinValue { get; set; }
        [DataMember]
        public long? MaxValue { get; set; }
        [DataMember]
        public int TotalWords { get; set; } = 0;
        [DataMember]
        public int TotalLength { get; set; } = 0;
		public int TotalNulls => NullCounter;
        [DataMember]
        public int TotalRecords { get; set; } = 0;
		public int TotalNonNullRecords => TotalRecords == 0 ? 0 : TotalRecords - TotalNulls;
		public decimal AverageLength => TotalRecords == 0 ? 0 : TotalLength / TotalRecords;
		public decimal AverageLengthExceptNull => TotalRecords == 0 || TotalNulls == TotalRecords ? 0 : TotalLength / (TotalRecords - TotalNulls);
		public decimal AverageNumberOfWords => TotalRecords == 0 ? 0 : TotalWords / TotalRecords;
        [DataMember]
        public bool IsUnique { get; set; } = false;
        [DataMember]
        public bool IsPrimaryKey { get; set; }
        [DataMember]
        public bool IsInt { get; set; } = true;
        [DataMember]
        public bool IsNumber { get; set; } = true;
        [DataMember]
        public bool IsNumberWithSpecialCharacters { get; set; }

		public bool IsDecimal => IsNumber && !IsInt;
        [DataMember]
        public bool IsCompletelyLetter { get; set; }

        public int StringWithOnlyNumberCount => _stringWithOnlyNumberCount;
        public int StringWithOnlyLetterCount => _stringWithOnlyLetterCount; 
        public void IncrementNumberOnlyCount() => Interlocked.Increment(ref _stringWithOnlyNumberCount);
        public void IncrementLetterOnlyCount() => Interlocked.Increment(ref _stringWithOnlyLetterCount);

        public bool IsNumberWithLetter => !IsNumberWithSpecialCharacters && !IsCompletelyLetter;
		public void IncrementNullCounter() => Interlocked.Increment(ref NullCounter);
        [DataMember]
        public string[] DateTimeFormat { get; set; } = new string[] { "yyyymmdd" };
        [DataMember]
        public bool IsNumberWithMinus { get; set; } = false;
        [DataMember]
        public bool IsToImport { get; set; } = true;
		public bool IsExclamationMark => decimal.Divide(TotalNulls,TotalRecords) >= 0.2m;
        public bool IsQuestionMark => IsInt;
        [DataMember]
        public bool IsDisplayed { get; set; } = true;
        [DataMember]
        public StorageType[] ValidStorageTypes { get; set; }
        [DataMember]
        public bool SecondPassCompleted { get; set; }

        public List<string> GetMetadataStats()
		{
			List<string> metaDataStats = new List<string>();
			metaDataStats.Add($"Total Nulls: {TotalNulls.ToString()} ({((float)TotalNulls/TotalRecords).ToString("P")})");
			metaDataStats.Add($"Average Length: {AverageLength.ToString()}");
			metaDataStats.Add($"Max Length: {MaxLength.ToString()}");
			metaDataStats.Add($"Is Unique: {IsUnique.ToString()} ");
            if(MaxValue != null && MinValue != null)
            {
                metaDataStats.Add($"Max Value: {MaxValue.ToString()} ");
                metaDataStats.Add($"Min Value: {MinValue.ToString()} ");
            }
			return metaDataStats;
		}

		public bool IsPrimaryKeyCandidate()
		{
			return IsUnique && IsInt && !IsDecimal;
		}

		internal void CheckPrimaryKey()
		{
			if (IsPrimaryKeyCandidate())
			{
				IsPrimaryKey = true;
			}
		}
	}
}

