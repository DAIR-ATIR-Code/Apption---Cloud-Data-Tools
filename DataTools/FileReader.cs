
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DataTools
{
    public class FileReader
    {
        public string FilePath;

        public FileReader(string filePath)
        {
            FilePath = filePath;
        }

        public List<string> GetByLineToList()
        {
            return File.ReadLines(FilePath).ToList<string>();
        }
        public string[] GetByLineToArray()
        {
            return File.ReadLines(FilePath).ToArray<string>();
        }

        public List<string> GetByFieldToList(string field)
        {
            //encoding to read the French chars
            StreamReader fileReader = new StreamReader(FilePath, Encoding.GetEncoding("iso-8859-1"), true);
            //TextReader fileReader = File.OpenText(FilePath);
            List<string> fieldList = new List<string>();
            var csv = new CsvReader(fileReader, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
                fieldList.Add(csv.GetField<string>(field));
            return fieldList;
        }
        public HashSet<string> GetByFieldToHashSet(string field)
        {
            //encoding to read the French chars
            StreamReader fileReader = new StreamReader(FilePath, Encoding.GetEncoding("iso-8859-1"), true);
            //TextReader fileReader = File.OpenText(FilePath);
            var fieldSet = new HashSet<string>();
            var csv = new CsvReader(fileReader, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
                fieldSet.Add(csv.GetField<string>(field));
            return fieldSet;
        }

    }
}

