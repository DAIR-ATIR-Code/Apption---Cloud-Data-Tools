
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using RecognizerTools;
using RecognizerTools.State;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Xunit;

namespace DataToolsTest
{
    public class DateRecognizerTest
    {
        [Fact]
        public void GivenYYYYMMDDDate_ThenParse()
        {
            var state = new RecognizerState();
            var dt = new DateRecognizer(null);
            dt.StartBatch(state);
            dt.IsMatch("20180416", CancellationToken.None);
            Assert.Contains("yyyyMMdd", dt.DateFormatTypes);
        }

        [Fact]
        public void GivenDDMMYYYYDate_ThenParse()
        {
            var state = new RecognizerState();
            var dt = new DateRecognizer(null);
            dt.StartBatch(state);
            dt.IsMatch("8/9/2018", CancellationToken.None);
            Assert.Contains("d/M/yyyy", dt.DateFormatTypes);
        }

        [Fact]
        public void GivenHHMMSSAMPMHour_ThenParse()
        {
            var state = new RecognizerState();
            var dt = new DateRecognizer(null);
            dt.StartBatch(state);
            dt.IsMatch("30.1.2018", CancellationToken.None);
            //var data = "09 01 2018";
            //DateTime result;
            //Assert.True(DateTime.TryParseExact(data, "M d yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result));
            var result = DateTime.ParseExact("25-Mar-2018", "dd-MMM-yyyy", CultureInfo.InvariantCulture);
            Assert.Contains("d.M.yyyy", dt.DateFormatTypes);
        }

        [Fact]
        public void GivenMoney_ThenParse()
        {
            //var mr = MoneyRecognizer();
            string money = "32013.1212";
            double m;
            CultureInfo culture;
            culture = CultureInfo.CreateSpecificCulture("en-CA");
            Assert.True(double.TryParse(money, NumberStyles.Currency , culture, out m));
        }

        [Fact]
        public void GivenLetter_ThenParse()
        {
            //var mr = MoneyRecognizer();
            string name = "Malinda Jaine";
            var state = new RecognizerState();
            var dt = new LetterRecognizer(null);
            dt.StartBatch(state);        
            Assert.True(dt.IsMatch(name, CancellationToken.None));
        }
    }
}

