
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using DataTools;
using DTHelperStd;
using System.IO;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataToolsTest
{
	public class CsvToSqlTest : DataToolsTest
	{

		private readonly ITestOutputHelper output;

		public CsvToSqlTest(ITestOutputHelper output)
		{
			this.output = output;
		}

		//[Fact]
		//public void GivenSQLdb_ThenTestGetType()
		//{
		//	ImportCsvToSql("dummy_data");
		//	var connectString = new SQLiteConnectionStringBuilder
		//	{
		//		Version = 3,
		//		DataSource = DatabaseFilePath
		//	};
		//	using (var conn = new SQLiteConnection(connectString.ToString()))
		//	{
		//		conn.Open();
		//		var command = conn.CreateCommand();

		//		command.CommandText = "SELECT * FROM dummy LIMIT 1";
		//		var dataReader = command.ExecuteReader();
		//		while (dataReader.Read())
		//		{
		//			output.WriteLine($"column 1 get string: {dataReader.GetString(0)}");
		//			output.WriteLine($"column 1 data type name: { dataReader.GetDataTypeName(0)}");
		//			output.WriteLine($"column 1 get value: { dataReader.GetValue(0)}");
		//			//output.WriteLine($"data get int: { dataReader.GetInt32(1)}");
		//			output.WriteLine($"column 1 get name: { dataReader.GetName(0)}");
		//			//output.WriteLine($"data get byte: { dataReader.GetByte(1)}");

		//			Assert.True(dataReader.GetString(0) == "2017", $"Get string is {dataReader.GetString(0)}");
		//			Assert.True(dataReader.GetDataTypeName(0) == "TEXT", $"Get data type name is {dataReader.GetDataTypeName(0)}");
		//			Assert.True(dataReader.GetValue(0).ToString() == "2017", $"Get value is {dataReader.GetValue(0)}");
		//			Assert.True(dataReader.GetName(0) == "1", $"Get name is {dataReader.GetName(0)}");
		//		}
		//	}
		//}


        private void ImportCsvToSql(string fileName)
        {
            using(var writer = new StreamWriter(BatchFilePath,false))
            {
                writer.WriteLine("sqlite3 data.db \"DROP TABLE IF EXISTS dummy; \" \".separator \",\"\" \".import ../Samples/" + fileName + ".csv dummy\"");
            }
            using(var process = new Process())           
            {
                var startInfo = new ProcessStartInfo();
                var argu = @"/c CsvToSql";
                startInfo.Arguments = argu;
                startInfo.FileName = "cmd.exe";
                startInfo.WorkingDirectory = DatabasePath;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;

				process.StartInfo = startInfo;
				process.Start();

				process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
				output.WriteLine("output>>" + e.Data);
				process.BeginOutputReadLine();

				process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
				output.WriteLine("error>>" + e.Data);
				process.BeginErrorReadLine();

				process.WaitForExit();
			}

		}

	}


	[SQLiteFunction(Name = "REGEXP", Arguments = 2, FuncType = FunctionType.Scalar)]
	public class RegExSQLiteFunction : SQLiteFunction
	{
		public static SQLiteFunctionAttribute GetAttribute()
		{
			return (SQLiteFunctionAttribute)typeof(RegExSQLiteFunction).GetCustomAttributes(typeof(SQLiteFunctionAttribute), false).Single();
		}

		public override object Invoke(object[] args)
		{
			try
			{
				return Regex.IsMatch((string)args[1], (string)args[0]);
			}
			catch (Exception ex)
			{
				return ex;
			}
		}
	}
}

