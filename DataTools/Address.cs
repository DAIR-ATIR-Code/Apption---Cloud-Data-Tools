
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Text;

namespace DataTools
{
    public class Address
    {
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Province { get; set; }
        public string Address1 { get; set; }

        public float ProbOfCity { get; set; }
    

        public float ProbOfProv { get; set; }

        public float ProbOfPostalCode { get; set; }

        public float ProbOfAddress1 { get; set; }

        //flag for identifing canada address or US address 0 - CA, 1 - US
        public int Flag { get; set; }
        public Address()
        {
            City = String.Empty;
            Province = String.Empty;
            Address1 = String.Empty;
            PostalCode = String.Empty;
            ProbOfAddress1 = 0.0f;
            ProbOfCity = 0.0f;
            ProbOfPostalCode = 0.0f;
            ProbOfProv = 0.0f;
            Flag = 0;
        }

        public Address(string address1, string city, string province, string postalCode)
        {
            City = city;
            Province = province;
            Address1 = address1;
            PostalCode = postalCode;
            ProbOfAddress1 = 0.0f;
            ProbOfCity = 0.0f;
            ProbOfPostalCode = 0.0f;
            ProbOfProv = 0.0f;
        }



        public override string ToString()
        {
            return Address1 + ": " + ProbOfAddress1 + "; "
                + City + ": " + ProbOfCity +"; " 
                + Province + ": " + ProbOfProv+ "; " 
                + PostalCode + ": "+ ProbOfPostalCode;
        }
    }
}

