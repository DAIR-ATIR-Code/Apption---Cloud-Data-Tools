
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DTHelperStd;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace DataToolsTest
{
    public class CsvToSqlTestLargeFile : DataToolsTest
    {

        private readonly ITestOutputHelper output;
        private RegexHelper _regex;
        private readonly string SchemaPath = $"{PathHelper.GetFolderRelativeToProject("SQLite3")}\\Schema.txt";
        private int TotalColumns = 0;


        public CsvToSqlTestLargeFile(ITestOutputHelper output)
        {
            this.output = output;
            _regex = new RegexHelper(',');
        }



        [Fact]
        public void GivenCSVFile_ThenTotalColumnsAreKnown()
        {
            using (var csvReader = new StreamReader(CsvWithNoHeadersPath))
            {
                var columnCount = _regex.SplitLineIgnoreQuotedCommas(csvReader.ReadLine(), 0).Length;
                Assert.True(columnCount == 56, $"Expected 56, Calculated {columnCount}");
            }

        }

        [Fact]
        public void GivenTotalColumns_ThenSchemaGenerated()
        {
            using (var csvReader = new StreamReader(CsvWithNoHeadersPath))
            {
                var columnCount = _regex.SplitLineIgnoreQuotedCommas(csvReader.ReadLine(), 0).Length;
                Assert.True(columnCount == 56, $"Expected 56, Calculated {columnCount}");

                var createTableScript = new StringBuilder();
                createTableScript.Append("CREATE TABLE TEMPIMPORT ( \n");
                for (var i = 1; i <= 56; i++)
                {
                    createTableScript.Append($"Field{i} TEXT NULL,\n");
                }
                createTableScript.Length = createTableScript.Length - 2;
                createTableScript.Append(") \nGO");
                File.WriteAllText(SchemaPath, createTableScript.ToString());
            }
        }

        [Fact]
        public void GivenCSVFile_ThenSingleRowSavedToSqlite()
        {
            var columnList = new StringBuilder();
            var createTableScript = GenerateLargeFileSchema(ref columnList);

            var connectString = new SQLiteConnectionStringBuilder
            {
                Version = 3,
                DataSource = DatabaseFilePath
            };
            using (var conn = new SQLiteConnection(connectString.ToString()))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = "DROP TABLE IF EXISTS LargeFile;";
                command.ExecuteNonQuery();

                command.CommandText = createTableScript.ToString();
                command.ExecuteNonQuery();

                using (var csvReader = new StreamReader(CsvWithNoHeadersPath))
                {


                    var firstLine = _regex.SplitLineIgnoreQuotedCommas(csvReader.ReadLine(), 0);
                    var dt = new DataTable("LargeFile");

                    for (var i = 0; i < firstLine.Length; i++)
                    {
                        dt.Columns.Add($"Field{i + 1}");
                    }

                    var dr = dt.NewRow();
                    for (var i = 0; i < firstLine.Length; i++)
                    {
                        dr[i] = firstLine[i];
                    }
                    command.CommandText = $"SELECT * FROM {dt.TableName}";
                    command.ExecuteNonQuery();

                    dt.Rows.Add(dr);
                    var adapter = new SQLiteDataAdapter(command);
                    var builder = new SQLiteCommandBuilder(adapter);
                    adapter.Update(dt);
                }
                conn.Close();
            }
        }

        [Fact (Skip="database is malformed")]
        public void GivenCSVFile_ThenFileReadAndSavedToSQLLite()
        {
            var columnList = new StringBuilder();
            var createTableScript = GenerateLargeFileSchema(ref columnList);

            var connectString = new SQLiteConnectionStringBuilder
            {
                Version = 3,
                DataSource = DatabaseFilePath
            };
            using (var conn = new SQLiteConnection(connectString.ToString()))
            {
                conn.Open();

                using (var command = new SQLiteCommand(conn))
                {
                    command.CommandText = $"DROP TABLE IF EXISTS LargeFile; {createTableScript.ToString()}";
                    command.ExecuteNonQuery();
                    var values = new StringBuilder();
                    using (var transaction = conn.BeginTransaction())
                    {
                        var line = string.Empty;
                        using (var csvReader = new StreamReader(CsvWithNoHeadersPath))
                        {
                            //int count = 0;
                            while ((line = csvReader.ReadLine()) != null)
                            {
                                var inputLine = _regex.SplitLineIgnoreQuotedCommas(line, TotalColumns);

                                values.Clear();

                                for (var i = 0; i < inputLine.Length; i++)
                                {
                                    values.Append($"'{inputLine[i]}', ");
                                }
                                values.Length = values.Length - 2;
                                command.CommandText = $"INSERT INTO LargeFile {columnList.ToString()} VALUES ({values.ToString()}); ";
                                command.ExecuteNonQuery();
                            }
                        }
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        transaction.Commit();
                        output.WriteLine($"Time spent saving {stopwatch.Elapsed.TotalSeconds}");
                    }
                }
                conn.Close();
            }
        }

        [Fact(Skip = "database is malformed")]
        public void GivenCSVFile_ThenFileReadAndSavedToSQLLiteInBatches()
        {
            var columnList = new StringBuilder();
            var createTableScript = GenerateLargeFileSchema(ref columnList);

            var connectString = new SQLiteConnectionStringBuilder
            {
                Version = 3,
                DataSource = DatabaseFilePath
            };
            using (var conn = new SQLiteConnection(connectString.ToString()))
            {
                conn.Open();

                using (var command = new SQLiteCommand(conn))
                {
                    command.CommandText = $"DROP TABLE IF EXISTS LargeFile; {createTableScript.ToString()}";
                    command.ExecuteNonQuery();
                    var values = new StringBuilder();
                    var line = string.Empty;
                    using (var csvReader = new StreamReader(CsvWithNoHeadersPath))
                    {
                        var count = 0;
                        SQLiteTransaction transaction = null;
                        while ((line = csvReader.ReadLine()) != null)
                        {

                            if (count == 0)
                            {
                                transaction = conn.BeginTransaction();
                            }
                            var inputLine = _regex.SplitLineIgnoreQuotedCommas(line, TotalColumns);

                            values.Clear();

                            for (var i = 0; i < inputLine.Length; i++)
                            {
                                values.Append($"'{inputLine[i]}', ");
                            }
                            values.Length = values.Length - 2;
                            command.CommandText = $"INSERT INTO LargeFile {columnList.ToString()} VALUES ({values.ToString()}); ";
                            command.ExecuteNonQuery();
                            count++;

                            if (count == 1500000)
                            {
                                transaction.Commit();
                                count = 0;
                            }
                        }
                        transaction.Commit();
                    }

                }
                conn.Close();
            }
        }




        #region unwanted tests	
        //[Fact]
        //public void GivenCSVFile_ThenFileReadAndSavedToSQLLiteUsingParameterQuery()
        //{
        //	StringBuilder columnList = new StringBuilder();
        //	StringBuilder createTableScript = GenerateLargeFileSchemaParamterValues(ref columnList);
        //	var results = new List<int>();
        //	var connectString = new SQLiteConnectionStringBuilder
        //	{
        //		Version = 3,
        //		DataSource = DatabaseFilePath
        //	};
        //	using (var conn = new SQLiteConnection(connectString.ToString()))
        //	{
        //		conn.Open();

        //		using (var command = new SQLiteCommand(conn))
        //		{
        //			command.CommandText = $"DROP TABLE IF EXISTS LargeFile; {createTableScript.ToString()}";
        //			command.ExecuteNonQuery();
        //			command.CommandText = $"INSERT INTO LargeFile {columnList.ToString()}; ";
        //			using (var transaction = conn.BeginTransaction())
        //			{
        //				string line = string.Empty;
        //				using (StreamReader csvReader = new StreamReader(CsvWithNoHeadersPath))
        //				{
        //					while ((line = csvReader.ReadLine()) != null)
        //					{
        //						string[] inputLine = _regex.SplitLineIgnoreQuotedCommas(line, TotalColumns);
        //						command.Parameters.Clear();
        //						for (int i = 0; i < inputLine.Length; i++)
        //						{
        //							command.Parameters.AddWithValue($"@Field{i + 1}", inputLine[i]);
        //						}
        //						results.Add(command.ExecuteNonQuery());
        //					}
        //				}
        //				var stopwatch = new Stopwatch();
        //				stopwatch.Start();
        //				transaction.Commit();
        //				output.WriteLine($"Time spent saving {stopwatch.Elapsed.TotalSeconds}");
        //			}
        //		}
        //		conn.Close();
        //	}
        //}
        //[Fact]
        //public void GivenSQLBulkInsertHelper_ThenDataInsertedIntoTable()
        //{
        //	StringBuilder columnList = new StringBuilder();
        //	StringBuilder createTableScript = GenerateLargeFileSchemaParamterValues(ref columnList);


        //	var connectString = new SQLiteConnectionStringBuilder
        //	{
        //		Version = 3,
        //		DataSource = DatabaseFilePath
        //	};
        //	var conn = new SQLiteConnection(connectString.ToString());
        //	conn.Open();
        //	var command = new SQLiteCommand(conn);
        //	command.CommandText = $"DROP TABLE IF EXISTS LargeFile; {createTableScript.ToString()}";
        //	command.ExecuteNonQuery();


        //	SQLiteBulkInsertHelper ContactBlk = new SQLiteBulkInsertHelper(conn, "LargeFile");
        //	ContactBlk.AllowBulkInsert = true;

        //	using (StreamReader csvReader = new StreamReader(CsvWithNoHeadersPath))
        //	{
        //		TotalColumns = _regex.SplitLineIgnoreQuotedCommas(csvReader.ReadLine(), 0).Length;

        //		for (int i = 1; i <= 56; i++)
        //		{
        //			ContactBlk.AddParameter($"Field{i}", DbType.String);
        //		}

        //	}

        //	string line = string.Empty;
        //	using (StreamReader csvReader = new StreamReader(CsvWithNoHeadersPath))
        //	{
        //		while ((line = csvReader.ReadLine()) != null)
        //		{
        //			string[] inputLine = _regex.SplitLineIgnoreQuotedCommas(line, TotalColumns);
        //			ContactBlk.Insert(inputLine);					
        //		}
        //	}

        //	ContactBlk.Flush();

        //}
        #endregion
        private void ImportCsvToSql(string fileName)
        {
            using (var writer = new StreamWriter(BatchFilePath, false))
            {
                writer.WriteLine("sqlite3 data.db \".separator \",\"\" \".import ../Samples/" + fileName + ".csv dummy\"");
            }
            using (var process = new Process())
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
        private StringBuilder GenerateLargeFileSchema(ref StringBuilder columnList)
        {
            var createTableScript = new StringBuilder();
            using (var csvReader = new StreamReader(CsvWithNoHeadersPath))
            {
                var columnCount = _regex.SplitLineIgnoreQuotedCommas(csvReader.ReadLine(), 0).Length;
                Assert.True(columnCount == 56, $"Expected 56, Calculated {columnCount}");
                TotalColumns = columnCount;

                columnList.Append("(");
                createTableScript.Append("CREATE TABLE IF NOT EXISTS LargeFile ( \n");
                for (var i = 1; i <= 56; i++)
                {
                    createTableScript.Append($"Field{i} TEXT NULL,\n");
                    columnList.Append($"Field{i},");
                }
                createTableScript.Length = createTableScript.Length - 2;
                createTableScript.Append(");");
                columnList.Length = columnList.Length - 1;
                columnList.Append(")");
            }

            return createTableScript;
        }

        private StringBuilder GenerateLargeFileSchemaParamterValues(ref StringBuilder columnList)
        {
            var createTableScript = new StringBuilder();
            var valueScript = new StringBuilder();
            using (var csvReader = new StreamReader(CsvWithNoHeadersPath))
            {
                var columnCount = _regex.SplitLineIgnoreQuotedCommas(csvReader.ReadLine(), 0).Length;
                Assert.True(columnCount == 56, $"Expected 56, Calculated {columnCount}");
                TotalColumns = columnCount;

                columnList.Append("(");
                valueScript.Append(" VALUES (");
                createTableScript.Append("CREATE TABLE IF NOT EXISTS LargeFile ( \n");
                for (var i = 1; i <= 56; i++)
                {
                    createTableScript.Append($"Field{i} TEXT NULL,\n");
                    columnList.Append($"Field{i},");
                    valueScript.Append($"@Field{i},");
                }
                createTableScript.Length = createTableScript.Length - 2;
                createTableScript.Append(");");
                columnList.Length = columnList.Length - 1;
                columnList.Append(")");
                valueScript.Length = valueScript.Length - 1;
                valueScript.Append(")");
                columnList.Append(valueScript);
            }

            return createTableScript;
        }

    }
}

