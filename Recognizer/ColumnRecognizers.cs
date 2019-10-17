
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity;
using Unity.Resolution;

namespace RecognizerTools
{
    public class ColumnRecognizers
    {
        private const int SHORTLENGTH = 4;
        private const int LONGLENGTH = 11;
        public ColumnMetadata ColumnMetadata { get; }
        public List<IRecognizer> Recognizers { get; set; }

        public SecondAnalysisResult SecondAnalysisResult { get; }

        public CancellationToken Token { get; set; }

        private IUnityContainer _unity;

        private List<IRecognizer> CreateCommonRecognizers<S, T>() where S : IRecognizer where T : IRecognizer
        {

            var types = CommonRecognizersTypes<S, T>();
            return types.Select(t => _unity.Resolve(t, new ResolverOverride[] 
            {
                new ParameterOverride("metadata", ColumnMetadata)
            }) as IRecognizer).ToList();
        }

        private IEnumerable<Type> CommonRecognizersTypes<S, T>() where S : IRecognizer where T : IRecognizer
        {
            var s1 = SecondPass.GetAllRecognizerTypes().Select(t => t.Item1).Where(t => typeof(S).IsAssignableFrom(t));
            var t1 = SecondPass.GetAllRecognizerTypes().Select(t => t.Item1).Where(t => typeof(T).IsAssignableFrom(t));
            return s1.Intersect(t1);

        }


        public ColumnRecognizers(ColumnMetadata columnMetadata, IUnityContainer container)
        {
            _unity = container;
            SecondAnalysisResult = new SecondAnalysisResult(columnMetadata.ColumnIndex);
            ColumnMetadata = columnMetadata;

            if (ColumnMetadata.IsNumberWithSpecialCharacters && ColumnMetadata.AverageLengthExceptNull <= SHORTLENGTH
                && !ColumnMetadata.IsNumberWithLetter)
            {
                Recognizers = CreateCommonRecognizers<INumberRecognizer, IShortStringRecognizer>();
            }
            else if (ColumnMetadata.IsNumberWithSpecialCharacters && ColumnMetadata.AverageLengthExceptNull > SHORTLENGTH
                && ColumnMetadata.AverageLengthExceptNull < LONGLENGTH && !ColumnMetadata.IsNumberWithLetter)
            {
                Recognizers = CreateCommonRecognizers<INumberRecognizer, IMediumStringRecognizer>();
            }
            else if (ColumnMetadata.IsNumberWithSpecialCharacters && ColumnMetadata.AverageLengthExceptNull >= LONGLENGTH && !ColumnMetadata.IsNumberWithLetter)
            {
                Recognizers = CreateCommonRecognizers<INumberRecognizer, ILongStringRecognizer>();
            }
            else if (ColumnMetadata.IsCompletelyLetter && ColumnMetadata.AverageLengthExceptNull <= SHORTLENGTH && !ColumnMetadata.IsNumberWithLetter)
            {
                Recognizers = CreateCommonRecognizers<ILetterRecognizer, IShortStringRecognizer>();
            }
            else if (ColumnMetadata.IsCompletelyLetter && ColumnMetadata.AverageLengthExceptNull > SHORTLENGTH
                && ColumnMetadata.AverageLengthExceptNull < LONGLENGTH && !ColumnMetadata.IsNumberWithLetter)
            {
                Recognizers = CreateCommonRecognizers<ILetterRecognizer, IMediumStringRecognizer>();
            }
            else if (ColumnMetadata.IsCompletelyLetter && ColumnMetadata.AverageLengthExceptNull >= LONGLENGTH && !ColumnMetadata.IsNumberWithLetter)
            {
                Recognizers = CreateCommonRecognizers<ILetterRecognizer, ILongStringRecognizer>();
            }
            else if (ColumnMetadata.AverageLengthExceptNull <= SHORTLENGTH && ColumnMetadata.IsNumberWithLetter)
            {
                Recognizers = CreateCommonRecognizers<ILetterWithNumberRecognizer, IShortStringRecognizer>();
            }
            else if (ColumnMetadata.AverageLengthExceptNull > SHORTLENGTH && ColumnMetadata.AverageLengthExceptNull < LONGLENGTH && ColumnMetadata.IsNumberWithLetter)
            {
                Recognizers = CreateCommonRecognizers<ILetterWithNumberRecognizer, IMediumStringRecognizer>();
            }
            else if (ColumnMetadata.AverageLengthExceptNull >= LONGLENGTH && ColumnMetadata.IsNumberWithLetter)
            {
                Recognizers = CreateCommonRecognizers<ILetterWithNumberRecognizer, ILongStringRecognizer>();
            }

        }

        public string GetDescription()
        {
            return "Analysis result: \n" +
                "Column " + (ColumnMetadata.ColumnIndex) + " \n" +
                "IsNumber: " + ColumnMetadata.IsInt + "  \n" +
                "IsLetter: " + ColumnMetadata.IsCompletelyLetter + " \n" +
                "Average length: " + ColumnMetadata.AverageLengthExceptNull + " \n" +
                "Maximun length: " + ColumnMetadata.MaxLength + " \n" +
                "Empty space: " + ColumnMetadata.TotalNulls + " \n" +
                "Total count: " + ColumnMetadata.TotalRecords + " \n";

        }


        private IEnumerable<IRecognizer> CommonRecognizers(IEnumerable<IRecognizer> enumerable1, IEnumerable<IRecognizer> enumerable2)
        {
            var collection1 = enumerable1 as ICollection<IRecognizer>;
            var collection2 = enumerable2 as ICollection<IRecognizer>;
            var commonRecognizers = new List<IRecognizer>();
            if (collection1.Count < collection2.Count)
            {
                var temp = collection2;
                collection2 = collection1;
                collection1 = temp;
            }

            foreach (var recognizer1 in collection1)
            {
                foreach (var recognizer2 in collection2)
                {
                    if (recognizer1.GetType() == recognizer2.GetType())
                    {
                        commonRecognizers.Add(recognizer1);
                        break;
                    }
                }
            }
            return commonRecognizers as IEnumerable<IRecognizer>;
        }

    }
}
