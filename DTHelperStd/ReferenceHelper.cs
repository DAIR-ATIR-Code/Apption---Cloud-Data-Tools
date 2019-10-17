
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace DTHelperStd
{
    public static class ReferenceHelper
    {

        public static Dictionary<string, HashSet<string>> ReferenceDic { get; } = new Dictionary<string, HashSet<string>>();
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private const string DATABASE_PATH = @"Resources\lookups.db";
        private const string PUBLISH_PATH = @"resources\app\bin\Resources\lookups.db";
        private const string PUBLISH_MAC_PATH = @"Resources/lookups.db";
        private const string VS_PATH = @"..\..\bin\Resources\lookups.db";

        public static string GetDBPath()
        {
            /*
             * If publish to Window, use <electronAppPath + PUBLISH_PATH>
             * else if publish to MAC, use <electronAppPath + PUBLISH_MAC_PATH>
             * else if running electron in VS, use <VS_PATH>
             */
            var databasePath = string.Empty;
#if ELECTRON
            databasePath = (Environment.CommandLine.IndexOf("electron") > -1) ? PUBLISH_PATH : DATABASE_PATH;
            Logger.Trace("ELECTRON is defined");
#elif MACOSX
            databasePath = Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), PUBLISH_MAC_PATH);
            Logger.Trace("MacOSX is defined");
#else 
            databasePath = (Environment.CommandLine.IndexOf("electron") > -1) ? VS_PATH : DATABASE_PATH;
            Logger.Trace("RELEASE/DEBUG is defined");
#endif
            return databasePath;
        }

        public static void InitializeReferenceDict()
        {


            var tables = GetTables();
            var connectString = new SQLiteConnectionStringBuilder
            {
                Version = 3,
                DataSource = GetDBPath()
            };

            using (var conn = new SQLiteConnection(connectString.ToString()))
            {
                conn.Open();

                foreach (var tableName in tables)
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        var tempSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        command.CommandText = "SELECT * FROM " + tableName;
                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            tempSet.Add(reader[0].ToString());
                        }
                        ReferenceDic.Add(tableName, tempSet);
                    }
                }

            }
        }

        private static List<string> GetTables()
        {            
            var tables = new List<string>();

            var connectString = new SQLiteConnectionStringBuilder
            {
                Version = 3,
                DataSource = GetDBPath()
            };
            try
            {

                using (var conn = new SQLiteConnection(connectString.ToString()))
                {
                    conn.Open();
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' ORDER BY 1";
                        var reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            tables.Add(reader[0].ToString());
                        }
                    }

                }
            }catch(Exception e)
            {
                Logger.Info($"connectString datasource path: {connectString.DataSource}");
                Logger.Info($"Current working dir: {Environment.CurrentDirectory} / App path: {System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}");
                Logger.Fatal(e,$"Cannot find database file");
            }
            return tables;
        }
    }
}

