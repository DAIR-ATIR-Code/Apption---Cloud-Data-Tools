
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Linq;
using System.Text.RegularExpressions;

namespace DTHelperStd
{
	public class RegexHelper
	{
		public static Regex ALPHANUMERIC_PATTERN = new Regex(@"\b[^a-zA-Z0-9]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public static Regex NUMBER_PATTERN1 = new Regex(@"\b^\d+(\.?\d+)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		//public static Regex NUMBER_PATTERN2 = new Regex(@"\b^\d+(\.?\d+)?(-)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		//public static Regex LETTER_PATTERN = new Regex(@"^[a-zA-Z]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public static Regex EVERYTHING_PATTERN = new Regex(@"\b(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public static Regex WORD_COUNTER = new Regex(@"\b[\S]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public static Regex IGNORE_QUOTED_COMMAS = new Regex("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)", RegexOptions.Compiled);

        public static Regex NAME_PATTERN = new Regex(@"^[a-zA-Z ']{2,30}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex CREDIT_CARD_PATTERN = new Regex(@"^\d{12,19}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //public static Regex DATE_PATTERN4 = new Regex(@"^(0?[1-9]|[12][0-9]|3[01])([ \/\-\.])(0?[1-9]|[12][0-9]|3[01])\2([0-9][0-9][0-9][0-9])(([ -])([0-1]?[0-9]|2[0-3]):[0-5]?[0-9]:[0-5]?[0-9])?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        //public static Regex DATE_PATTERN2 = new Regex(@"\b(0[1-9]|[12]\d|30|31)(0[1-9]|[12]\d|30|31)(\d{4})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        //public static Regex DATE_PATTERN3 = new Regex(@"\b(1[0-9]|2[0-9])(\d{2})(0[1-9]|1[0-2])(0[1-9]|[12]\d|30|31)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static Regex DATE_PATTERN1 = new Regex(@"^(0?[1-9]|1[0-2])([^\w\d\r\n:])(0?[1-9]|[12][0-9]|3[01])\2((20\d\d)|(1[789]\d\d)|\d{2})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static Regex DATE_PATTERN2 = new Regex(@"^(0?[1-9]|[12][0-9]|3[01])([^\w\d\r\n:])(0?[1-9]|1[0-2])\2((20\d\d)|(1[789]\d\d)|\d{2})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static Regex DATE_PATTERN3 = new Regex(@"^(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])((20\d\d)|(1[789]\d\d)|\d{2})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static Regex DATE_PATTERN4 = new Regex(@"(^((20\d\d)|(1[789]\d\d))(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])$)|(^((20\d\d)|(1[789]\d\d))(0[1-9]|[12][0-9]|3[01])(0[1-9]|1[0-2])$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static Regex DATE_PATTERN5 = new Regex(@"^((20\d\d)|(1[789]\d\d)|\d{2})([^\w\d\r\n:])(0?[1-9]|1[0-2])\4(0?[1-9]|[12][0-9]|3[01])$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        public static Regex IPV4_PATTERN = new Regex(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex ISBN_PATTERN = new Regex("[^0-9Xx]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex LATITUDE_PATTERN1 = new Regex(@"^[-+]?([1-8]?\d(\.\d+)?|90(\.0+)?),\s*[-+]?(180(\.0+)?|((1[0-7]\d)|([1-9]?\d))(\.\d+)?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex LATITUDE_PATTERN2 = new Regex(@"^[-+]?([1-8]?\d(\.\d+)?|90(\.0+)?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex LATITUDE_PATTERN3 = new Regex(@"^\s*[-+]?(180(\.0+)?|((1[0-7]\d)|([1-9]?\d))(\.\d+)?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex LATITUDE_PATTERN4 = new Regex(@"^[NnSs]?([0-8][0-9]([\.\s][0-5]\d)([\.\s][0-5]\d(\.\d\d?)?]?)|90([\.\s]00){2})[NnSs]?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex LATITUDE_PATTERN5 = new Regex(@"^[EeWw]?((0?\d\d|1[0-7]\d)([\.\s][0-5]\d)([\.\s][0-5]\d(\.\d\d?)?]?|180([\.\s]00){2}))[EeWw]?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex LATITUDE_PATTERN6 = new Regex("^[0-9]{1,2}[:|°][0-9]{1,2}[:|'](?:\b[0-9]+(?:\\.[0-9]*)?|\\.[0-9]+\b)\"?[N|S|E|W]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex MONEY_PATTERN1 = new Regex(@"^[\$\£\€\¥]+\-?([1-9]{1}[0-9]{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex MONEY_PATTERN2 = new Regex(@"^\-?[\$\£\€\¥]+([1-9]{1}\d{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex MONEY_PATTERN3 = new Regex(@"^\([\$\£\€\¥]+([1-9]{1}\d{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex PHONE_PATTERN = new Regex(@"^(\+?\d{1,3}\s?)?([\-])?\(?\d{3}\)?([\.\-])?\s*\d{3}([\.\-])?\s*\d{4}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex ADDRESS_PATTERN1 = new Regex(@"\b[P|p]?(OST|ost)?\.?\s*[O|o|0]?(ffice|FFICE)?\.?\s*[B|b][O|o|0]?[X|x]?\.?\s+[#]?(\d+)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex ADDRESS_PATTERN2 = new Regex(@"^\d+\w*\s*(?:(?:[\-\/]?\s*)?\d*(?:\s*\d+\/\s*)?\d+)?\s+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex EMAIL_PATTERN = new Regex(@"^([a-zA-Z0-9_\.-]+)@([\da-zA-Z\.-]+)\.([a-z\.]{2,6})$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex HEALTHCARD_PATTERN = new Regex(@"^\d{4}([\-\s*]?)\d{3}([\-\s*]?)\d{3}([\-\s*]?)[a-zA-Z]{2}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex HTTP_PATTERN = new Regex(@"/^((https?|ftp|file):\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$/", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex IPV6_PATTERN = new Regex(@"^(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex MARKETCAP_PATTERN = new Regex(@"^\$?\d+(\.\d+)?[MmBb]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex POSTALCODE_PATTERN1 = new Regex(@"^(?!.*[DFIOQU])[A-VXY][0-9][A-Z] ?[0-9][A-Z][0-9]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex POSTALCODE_PATTERN2 = new Regex(@"^[0-9]{5}(?:-[0-9]{4})?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex USERNAME_PATTERN = new Regex(@"^[a-zA-Z0-9_-]{3,16}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //public static Regex NUMBER_WITH_DOT_PATTERN = new Regex(@"^\-?[0-9]+(\.[0-9]+)\-?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly char _separator;
        private Regex _ignore_Quoted_Commas;

        public RegexHelper(char separator)
        {
            _separator = separator;
            _ignore_Quoted_Commas = new Regex("(?<=^|" + separator + ")(\"(?:[^\"]|\"\")*\"|[^" + separator + "]*)", RegexOptions.Compiled);
        }

        public string[] SplitLineIgnoreQuotedCommas(string inputLine, int totalColumns)
        {


            var totalSeparators = inputLine.Count(c => c == _separator);
            //note: the IGNORE_QUOTED_COMMAS is NOT separator friendly. Only looks at Commas for now.
            return (totalSeparators == (totalColumns - 1) || _separator != ',') ? inputLine.Split(_separator) : _ignore_Quoted_Commas.Matches(inputLine).Cast<Match>().Select(m => m.Value).ToArray();
        }
    }
}

