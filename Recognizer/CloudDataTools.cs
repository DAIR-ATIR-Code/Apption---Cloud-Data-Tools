
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using RecognizerTools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace DataTools
{
    public class CloudDataTools
    {
        public Preferences Preferences { get; private set; }
        public FileAnalyzer FileAnalyzer { get; private set; }
        public SecondPass SecondPass { get; private set; }
        public FileObject FileObject { get {
                return FileAnalyzer.FileObject;
            } }

        public bool SecondPassStarted { get { return SecondPass == null?false:SecondPass.Started; } }

        public CloudDataTools(string filePath, bool hasHeaders, SeparatorType separatorType, Preferences preferences = null, Char customSeparator = '\0')
        {
            Preferences = preferences?? new Preferences();
            FileAnalyzer = new FileAnalyzer(filePath, Preferences, hasHeaders,separatorType, customSeparator);
            //var secondPassAnalyzers = new SecondPass(FileAnalyzer, Preferences);            
            
        }

        public (ExecuteSecondPass,IObservable<SecondAnalysisResult>) InitializeSecondPass()
        {
            
            var ex = new ExecuteSecondPass();
            
            var (sp,observable) = ex.Initialize(FileAnalyzer, Preferences);
            SecondPass = sp;
            return (ex, observable);
        }

        public SchemaGenerator GetSchemaGenerator()
        {
            return new SchemaGenerator(FileObject, Preferences);
        }

        public static bool HasSize(StorageType storageType)
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

