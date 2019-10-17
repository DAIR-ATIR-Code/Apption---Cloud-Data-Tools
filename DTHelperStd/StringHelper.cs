
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTHelperStd
{
	public static class StringHelper
	{
		public static string CleanSpecialCharacters(string s)
		{
			return new StringBuilder(s)
				  .Replace("+", "")
				  .Replace("-", "")
				  .Replace("$", "")
				  .Replace("/", "")
                  .Replace(".", "")
                  .Replace("£", "")
				  .ToString();				  
		}

        //maybe move to helper class
        public static int CountWords(string s)
        {
            return s.Split().Length;
        }

        public static string GetSQLiteColumnName(int pos)
        {
            return "Col_" + pos;
        }
        public static bool IsLetterOnly(string data)
        {
            if (data.Length == 0)
                return false;
            for (var i = 0; i < data.Length; i++)
            {
                if (((data[i] < 'a' || data[i] > 'z') && (data[i] < 'A' || data[i] > 'Z')) && data[i] != ' ')
                    return false;
            }
            return true;
        }

    }
}

