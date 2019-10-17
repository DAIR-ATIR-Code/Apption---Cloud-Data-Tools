
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using DTHelperStd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace RecognizerTools
{
    [StorageTypes(new[] { StorageType.Varchar, StorageType.Char, StorageType.Nchar, StorageType.NVarchar })]
    public class AddressRecognizer : Recognizer, ILetterWithNumberRecognizer, ILongStringRecognizer
    {
        private ColumnMetadata metadata;
        private const float MIN_PROB = 0.5f;
        public AddressRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            IncrementStats(data.Length);
            if (!cancellationToken.IsCancellationRequested)
            {
                if (DetermineAddress1ByProbability(ref data) > MIN_PROB)
                    return true;
            }
            return false;
        }

        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength + 1, MinimunX = metadata.MinLengthExceptNull == 0 ? 0 : metadata.MinLengthExceptNull };
        }

        public override string GetDescription()
        {
            return "Address";
        }

        private float DetermineAddress1ByProbability(ref string address1)
        {
            float score = 0.0f;
            int indexOfAddress = 0;
            Regex poBoxPattern = new Regex(@"\b[P|p]?(OST|ost)?\.?\s*[O|o|0]?(ffice|FFICE)?\.?\s*[B|b][O|o|0]?[X|x]?\.?\s+[#]?(\d+)\b");

            if (poBoxPattern.IsMatch(address1)) return 1.0f;
            string[] addressArr = ParseAddress1IntoParts(ref address1);

            //ex: 120 main st
            if (addressArr[0].Length != 0 && addressArr[1].Length != 0 && addressArr[2].Length != 0)
                score += 3.0f;

            //ex: 15 BL Labelle
            else if (addressArr[0].Length != 0 && addressArr[1].Length != 0 && addressArr[2].Length == 0)
                score += 2.0f;

            //ex: route 12
            else if (addressArr[0].Length != 0 && addressArr[1].Length == 0 && addressArr[2].Length != 0)
                score += 2.0f;

            //ex: main st
            else if (addressArr[0].Length == 0 && addressArr[1].Length != 0 && addressArr[2].Length != 0)
                score += 1.0f;

            //ex main
            else if (addressArr[0].Length == 0 && addressArr[1].Length != 0 && addressArr[2].Length == 0)
                score += 0.5f;
            if (addressArr[3].Length != 0) score += 0.5f;


            //parse the address           
            indexOfAddress = address1.IndexOf(addressArr[0]);
            if (indexOfAddress > address1.IndexOf(addressArr[1]) && address1.IndexOf(addressArr[1]) >= 0)
                indexOfAddress = address1.IndexOf(addressArr[1]);
            if (indexOfAddress > address1.IndexOf(addressArr[2]) && address1.IndexOf(addressArr[2]) >= 0)
                indexOfAddress = address1.IndexOf(addressArr[2]);
            address1 = address1.Substring(indexOfAddress, address1.Length - indexOfAddress);

            return score / 3.5f;
        }

        private string[] ParseAddress1IntoParts(ref string address)
        {
            //0 - civic number, 1 - street name, 2- street type, 3 - street Direction
            string[] addressArr = new string[4];


            addressArr[0] = GetCivicNum(address);
            addressArr[2] = GetStreetType(address);
            addressArr[3] = GetStreetDirection(address);
            addressArr[1] = GetStreetName(address, addressArr);

            return addressArr;
        }

        private string GetStreetName(string address, string[] addressArr)
        {
            if (addressArr[0].Length != 0)
                address = address.Replace(addressArr[0], "");
            if (addressArr[2].Length != 0)
                address = address.Replace(addressArr[2], "");
            if (addressArr[3].Length != 0)
                address = address.Replace(addressArr[3], "");
            address = address.Trim();
            return address;
        }

        private string GetStreetDirection(string address)
        {
            string[] addressArr = address.Split(' ');
            foreach (string term in addressArr)
            {
                if (ReferenceHelper.ReferenceDic["directions"].Contains(term))
                {
                    return term;
                }

            }
            return String.Empty;
        }

        private string GetStreetType(string address)
        {
            string[] addressArr = address.Split(' ');
            foreach (string term in addressArr.Reverse())
            {
                if (ReferenceHelper.ReferenceDic["streetNames"].Contains(term))
                {
                    return term;
                }
            }
            return String.Empty;
        }


        private string GetCivicNum(string address)
        {
            Regex rx = new Regex(@"^\d+\w*\s*(?:(?:[\-\/]?\s*)?\d*(?:\s*\d+\/\s*)?\d+)?\s+");
            Regex secondRx = new Regex(@"^\s*\d+\s*");
            Regex thirdRx = new Regex(@"^(\d+)([a-zA-Z]+)");

            string temp = address.Trim();
            string[] tempArr = temp.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string civicNum = String.Empty;

            //general civic number fg, 120 main st && hwy 120
            foreach (string item in tempArr)
            {
                Match numberMatch = rx.Match(temp);
                if (!numberMatch.Success)
                    numberMatch = secondRx.Match(temp);
                civicNum = numberMatch.Value.Trim();
                temp = temp.Substring(item.Length, temp.Length - item.Length).Trim();
                if (civicNum != String.Empty)
                    break;
            }

            if (civicNum != String.Empty)
            {
                //let the civic number only contains the number
                Match number = thirdRx.Match(civicNum);
                if (number.Success)
                    civicNum = number.Groups[1].Value.Trim();
            }
            return civicNum;
        }
    }
}

