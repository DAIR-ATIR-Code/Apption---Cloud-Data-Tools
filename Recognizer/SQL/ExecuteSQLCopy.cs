
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using NLog;
using System;
using System.Linq;
using System.Data.SqlClient;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RecognizerTools.SQL
{
    public class ExecuteSQLCopy
    {
        private FileAnalyzer fileAnalyzer;        
        private Preferences preferences;
        private long totalRows;
        private Subject<(DateTime, long)> progress;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string connStr;
        private Subject<long> recordService;

        public ExecuteSQLCopy(string connStr, Subject<long> recordService = null)
        {
            this.connStr = connStr;
            this.recordService = recordService;
        }

        public async Task<string> ExecuteSQLCreateTable()
        {
            var sc = new SchemaGenerator(fileAnalyzer.FileObject, preferences);
            var createTableSql = sc.BuildSQLString();
            using (var con = new SqlConnection(connStr))
            {
                await con.OpenAsync();

                if (preferences.DropTableIfExists)
                {
                    using (var command = new SqlCommand($"DROP TABLE IF EXISTS[{fileAnalyzer.FileObject.GetSQLTableName()}]", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                using (var command = new SqlCommand(createTableSql, con))
                {
                    command.ExecuteNonQuery();
                }
                con.Close();
            }
            return createTableSql;
        }

        public async Task<IObservable<(DateTime, long)>> Initialize(FileAnalyzer analyzer, Preferences prefs)
        {
            this.fileAnalyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
            this.preferences = prefs;

            totalRows = 0;
            progress = new Subject<(DateTime, long)>();
            return progress;

        }

        public async Task<long> PerformBulkCopyToSQL()
        {
            var hb = DateTime.MinValue;
            using (var con = new SqlConnection(connStr))
            {
                await con.OpenAsync();


                var fac = new SecondPass(fileAnalyzer, preferences);
                var queue = new BufferBlock<(string[], long)>(new DataflowBlockOptions { BoundedCapacity = 1000 });
                var sqlReader = new SQLDataReader(fileAnalyzer.FileObject, queue);
                var subject = new Subject<int>();
                var consumer = Task.Run(async () =>
                {
                   
                    using (var bulkCopy = new SqlBulkCopy(con))
                    {
                        try
                        {
                            bulkCopy.SqlRowsCopied += (s, e) =>
                            {
                                hb = DateTime.Now;
                                totalRows = e.RowsCopied;
                                progress.OnNext((hb, e.RowsCopied));
                            };
                            bulkCopy.BulkCopyTimeout = preferences.SQLTimeoutMinutes * 60;
                            bulkCopy.DestinationTableName = fileAnalyzer.FileObject.GetSQLTableName();
                            bulkCopy.BatchSize = preferences.SQLBatchSize;
                            bulkCopy.EnableStreaming = true;
                            bulkCopy.NotifyAfter = 100;
                            await bulkCopy.WriteToServerAsync(sqlReader, CancellationToken.None);
                        }catch (Exception ex)
                        {
                            logger.Fatal(ex);
                        }
                    }
                    
                    
                });

                await fileAnalyzer.ProcessAllLines(queue);
                await consumer;
                totalRows = sqlReader.RowsCopied;
                progress.OnNext((DateTime.Now, totalRows));
                progress.OnCompleted();
            }
            return totalRows;

        }

        public async Task<long> PerformCopyToSQL()
        {
            var hb = DateTime.MinValue;
            using (var con = new SqlConnection(connStr))
            {
                await con.OpenAsync();
                
                var queue = new BufferBlock<(string[], long)>(new DataflowBlockOptions { BoundedCapacity = 1000 });
                var sqlReader = new SQLDataReader(fileAnalyzer.FileObject, queue);
                var subject = new Subject<int>();
                var columns = fileAnalyzer.FileObject.ColumnMetadata.OrderBy(tp => tp.Key).ToList();
                var colNames = String.Join(",", columns.Select(kvp => fileAnalyzer.FileObject.GetSQLColumnName(kvp.Key)));
                var parameters = columns.Select(kvp => (kvp.Key, kvp.Value, "@Col" + kvp.Key)).ToList();
                var parametersString = String.Join(",", parameters.Select(tp => tp.Item3));
                var sqlStatement = $"INSERT INTO [{fileAnalyzer.FileObject.GetSQLTableName()}] ({colNames}) VALUES ({parametersString})";
                long recordsInserted = 0;
                var consumer = Task.Run(async () =>
                {

                    using (var cmd = new SqlCommand(sqlStatement))
                    {
                        try
                        {
                            cmd.Connection = con;
                            cmd.CommandType = System.Data.CommandType.Text;
                            var allParameters = parameters.Select(p => (p.Key, cmd.Parameters.Add(p.Item3, p.Value.GetSqlDbType(), p.Value.MaxLength))).ToList();
                            while (sqlReader.Read())
                            {
                                for (int i = 0; i < allParameters.Count; i++)
                                {
                                    var param = allParameters[i];
                                    if (sqlReader.IsDBNull(i))
                                        param.Item2.Value = DBNull.Value;
                                    else
                                        param.Item2.Value = sqlReader.GetValue(i);
                                }
                                recordsInserted += await cmd.ExecuteNonQueryAsync();
                                recordService?.OnNext(recordsInserted);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Fatal(ex,$"Error with row {recordsInserted}");
                        }
                    }


                });

                await fileAnalyzer.ProcessAllLines(queue);
                await consumer;
                totalRows = sqlReader.RowsCopied;
                progress.OnNext((DateTime.Now, totalRows));
                progress.OnCompleted();
            }
            return totalRows;

        }

    }
}


