
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Text;

namespace DataTools
{
    public class SchemaGenerator
    {
        private FileObject _generator;
        private Preferences _prefs;

        public SchemaGenerator(FileObject generatorObject, Preferences prefs)
        {
            _generator = generatorObject;
            _prefs = prefs;
        }
        public string BuildSQLString()
        {
            var createTableScript = new StringBuilder();
            createTableScript.Append("CREATE TABLE [" + _generator.GetSQLTableName()  + "] ( \n");
            var first = true;
            foreach (var columnDetails in _generator.ColumnMetadata)
            {
                if (columnDetails.Value.IsToImport)
                {
                    if (!first)
                        createTableScript.Append(",\n");
                    else
                        first = false;

                    createTableScript.Append(GetColumnLineForSchema(columnDetails.Key, columnDetails.Value));
                }
            }
            //createTableScript.Length = createTableScript.Length - 2;
            createTableScript.Append(")");

            return createTableScript.ToString();
        }

        private string GetColumnLineForSchema(int pos, ColumnMetadata column)
        {
            var actualSize = column.UserSize;
            if (actualSize == 0)
            {
                actualSize = _prefs.VarCharSizeForEmptyColums;
            }
            string sizeString = HasSize(column.StorageType) ? $"({actualSize.ToString()})" : string.Empty;

            if (column.IsNullable == column.UserIsNullable)
            {
                return $"{_generator.GetSQLColumnName(pos)} [{column.StorageType.ToString()}]{sizeString}{(column.IsNullable ? "" : " NOT NULL")}{(column.IsPrimaryKey ? " PRIMARY KEY" : "")}";
            }
            //Warning: Column has {ColumnMetedata.TotalNull} nulls
            return $"{_generator.GetSQLColumnName(pos)} [{column.StorageType.ToString()}]{sizeString}{(column.UserIsNullable ? "" : " NOT NULL")}{(column.IsPrimaryKey ? " PRIMARY KEY" : "")}";
        }

        private bool HasSize(StorageType storageType)
        {
            if (storageType == StorageType.Varchar ||
                storageType == StorageType.NVarchar ||
                storageType == StorageType.Nchar ||
                storageType == StorageType.Char)
            {
                return true;
            }
            return false;
        }
    }
}

