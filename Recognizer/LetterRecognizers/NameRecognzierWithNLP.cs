
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

//using System;
//using System.Collections.Generic;
//using System.Text.RegularExpressions;
//using edu.stanford.nlp.ie.crf;

//namespace RecognizerTools
//{
//    public class NameRecognzierWithNLP : StringRecognizer
//    {
//        public override float Prob { get => _prob; set => throw new NotImplementedException(); }

//        public override void DetermineProbability(List<string> list)
//        {
//            int isName = 0;
//            //var classifiersDirecrory = @"C:\Users\dzhao\source\repos\Cloud Data Platform\DataTools\packages\stanford-ner-2016-10-31\classifiers";
//            //var classifier = new CRFClassifier();
//            // Loading 3 class classifier model
//            CRFClassifier classifier = CRFClassifier.getClassifierNoExceptions(@"C:\Users\dzhao\source\repos\Cloud Data Platform\DataTools\packages\stanford-ner-2016-10-31\classifiers\english.all.3class.distsim.crf.ser.gz");

//            //Console.WriteLine("{0}\n", classifier.classifyToString(s1));
//            //foreach (string data in list)
//            //{

//            //    if(classifier.classifyToString(data) == "PERSON")
//            //        isName++;
//            //}
//            string temp = string.Join(", ", list);
//            var classifierString = classifier.classifyToCharacterOffsets(temp);

//            _prob = (float)isName / list.Count;
//        }

//        public override string ToString()
//        {
//            return "Name recognizer probability: " + _prob;

//        }
//    }
//}

