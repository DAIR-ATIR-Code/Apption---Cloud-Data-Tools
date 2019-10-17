
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DTHelperStd;
using Fastenshtein;

namespace DataTools
{
    public class AddressAnalyzer
    {

        private static readonly string StreetFilePath = $"{PathHelper.GetFolderRelativeToProject("DataTools\\Reference Data")}\\StreetName.csv";
        private static readonly string City2FilePath = $"{PathHelper.GetFolderRelativeToProject("DataTools\\Reference Data")}\\CanadaCity2.csv";
        private static readonly string CAProvFilePath = $"{PathHelper.GetFolderRelativeToProject("DataTools\\Reference Data")}\\CanadaProvinces.csv";
        private static readonly string USProvFilePath = $"{PathHelper.GetFolderRelativeToProject("DataTools\\Reference Data")}\\USProvinces.csv";
        private static readonly string USCityFilePath = $"{PathHelper.GetFolderRelativeToProject("DataTools\\Reference Data")}\\uscitiesv1.4.csv";
        private static readonly string CityFilePath = $"{PathHelper.GetFolderRelativeToProject("DataTools\\Reference Data")}\\CanadaCity.csv";
        private static readonly string DirectionFilePath = $"{PathHelper.GetFolderRelativeToProject("DataTools\\Reference Data")}\\direction.csv";
        private static readonly string UnitFilePath = $"{PathHelper.GetFolderRelativeToProject("DataTools\\Reference Data")}\\unit.csv";

        private static FileReader CityReader = new FileReader(CityFilePath);
        private static FileReader City2Reader = new FileReader(City2FilePath);
        private static FileReader USCityReader = new FileReader(USCityFilePath);
        private static FileReader StreetReader = new FileReader(StreetFilePath);
        private static FileReader CAProvinceReader = new FileReader(CAProvFilePath);
        private static FileReader USProvinceReader = new FileReader(USProvFilePath);
        private static FileReader DirectionReader = new FileReader(DirectionFilePath);
        

        private readonly List<string> CityList = CityReader.GetByFieldToList("city");
        private readonly List<string> City2List = City2Reader.GetByFieldToList("Geographic name, english");
        private readonly List<string> USCityList = USCityReader.GetByFieldToList("city");

        private readonly List<string> CAProvinceList = CAProvinceReader.GetByFieldToList("provinces");
        private readonly List<string> USProvinceList = USProvinceReader.GetByFieldToList("provinces");
        private readonly string[] StreetData = StreetReader.GetByLineToArray();
        private readonly string[] DirectionData = DirectionReader.GetByLineToArray();


        public float DetermineIndIdByProbability(string id)
        {
            float score = 0.0f;

            if (Regex.IsMatch(id, @"^\d{10}$")) return 1.0f;
            char[] idArray = id.ToArray();
            int index = 0;
            while(index < idArray.Length && index < 10)
            {
                if (Char.IsDigit(idArray[index])) score++;
                index++;
            }
            return score/10.0f;
        }

        public float DetermineAddress1ByProbability(ref string address1)
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
            if (addressArr[3].Length != 0) score+= 0.5f;


            //parse the address           
            indexOfAddress = address1.IndexOf(addressArr[0]);
            if(indexOfAddress > address1.IndexOf(addressArr[1]) && address1.IndexOf(addressArr[1])>=0)
                indexOfAddress = address1.IndexOf(addressArr[1]);
            if(indexOfAddress > address1.IndexOf(addressArr[2]) && address1.IndexOf(addressArr[2]) >= 0)
                indexOfAddress = address1.IndexOf(addressArr[2]);
            address1 = address1.Substring(indexOfAddress, address1.Length - indexOfAddress);

            return score /3.5f;
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
            if(addressArr[0].Length != 0)
                address = address.Replace(addressArr[0], "");
            if(addressArr[2].Length != 0)
                address = address.Replace(addressArr[2], "");
            if(addressArr[3].Length != 0)
                address = address.Replace(addressArr[3], "");
            address = address.Trim();         
            return address;
        }

        private string GetStreetDirection(string address)
        {
            string[] addressArr = address.Split(' ');
            foreach (string term in addressArr)
                if (Array.FindIndex(DirectionData, t => t.Equals(term, StringComparison.InvariantCultureIgnoreCase)) > 0)
                    return term;
            return String.Empty;
        }

        private string GetStreetType(string address)
        {
            string[] addressArr = address.Split(' ');
            foreach (string term in addressArr.Reverse())
                if (Array.FindIndex(StreetData, t => t.Equals(term, StringComparison.InvariantCultureIgnoreCase)) > 0)
                    return term;
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

        public float DetermineCityByProbability(string targetCity,int flag)
        {
            List<float> probList = new List<float>();
            float maxScore = 0.0f;
            if(flag == 0)
            {
                foreach (string realCityName in City2List)
                {
                    int diff = Levenshtein.Distance(realCityName.ToLower(), targetCity.ToLower());
                    maxScore = (float)(realCityName.Length - diff) / realCityName.Length;
                    probList.Add(maxScore);
                    if (maxScore > 0.9)
                        return maxScore;
                }
                foreach (string realCityName in CityList)
                {
                    int diff = Levenshtein.Distance(realCityName.ToLower(), targetCity.ToLower());
                    maxScore = (float)(realCityName.Length - diff) / realCityName.Length;
                    probList.Add(maxScore);
                    if (maxScore > 0.9)
                        return maxScore;
                }
            }
            else if(flag == 1)
            {
                foreach (string realUSCityName in USCityList)
                {
                    int diff = Levenshtein.Distance(realUSCityName.ToLower(), targetCity.ToLower());
                    maxScore = (float)(realUSCityName.Length - diff) / realUSCityName.Length;
                    probList.Add(maxScore);
                    if (maxScore > 0.9)
                        return maxScore;
                }
            }           
            return probList.Max();
        }


        public float DetermineProvinceNameByProbability(string province, int flag)
        {
            List<float> probList = new List<float>();
            List<string> provinceList;

            if (flag == 1) provinceList = USProvinceList;
            else provinceList = CAProvinceList;

            foreach (string realProv in provinceList)
            {
                int diff = Levenshtein.Distance(province.ToLower(), realProv.ToLower());
                float possibleProb = (float)(realProv.Length - diff) / realProv.Length;
                probList.Add(possibleProb);
                if (possibleProb > 0.9) return possibleProb;
            }
            return probList.Max();
        }  

        public float DeterminePostalCodeByProbability(string postalCode,ref int flag)
        {
            Regex rxCA = new Regex(@"^(?!.*[DFIOQU])[A-VXY][0-9][A-Z] ?[0-9][A-Z][0-9]$");
            Regex rxPartialCA = new Regex(@"^(?!.*[DFIOQU])[A-VXY][0-9][A-Z]$");
            Regex rxUSA = new Regex(@"^[0-9]{5}(?:-[0-9]{4})?$");

            //intial check for the postal code pattern
            if (rxCA.IsMatch(postalCode))
                return 1.0f;
            else if(rxUSA.IsMatch(postalCode))
            {
                flag = 1;
                return 1.0f;
            }
            if (rxPartialCA.IsMatch(postalCode)) return 0.8f;

            //if valid postal code not find
            char[] postalCodeArr = postalCode.ToArray();
            float score = 0.0f;
            int index = 0;
            if (postalCode.Length == 6) score++;
            while(index < postalCodeArr.Length && index < 6)
            {
                if(index%2 == 0 && Char.IsLetter(postalCodeArr[index])) score++;
                else if (index%2 == 1 && Char.IsDigit(postalCodeArr[index])) score++;
                index++;
            }

            //Case handling for 6 letters English word
            if (Regex.IsMatch(postalCode, @"^[a-zA-Z]{5,6}"))
                score -= 3.0f;            
            return score/7.0f;
        }


        public float DetermineAddressByProbability(string address, ref Address addr)
        {
            //Format the input address
            address = address.Trim();
            address = Regex.Replace(address, "\\s+", " ");


            //Firstly postal code       
            DetermineAddressPart1(ref address, ref addr);

            //Then the province
            DetermineAddressPart2(ref address, ref addr);            

            //Then the city
            DetermineAddressPart3(ref address, ref addr);

            //Then the address
            DetermineAddressPart4(ref address, ref addr);


            //exception handler for provinces on, oh, or any tow char pharses 
            if (addr.ProbOfAddress1 < addr.ProbOfProv && addr.ProbOfPostalCode < 0.5 && addr.ProbOfCity <= addr.ProbOfProv && addr.ProbOfAddress1 <0.5)
                addr.ProbOfProv *= 0.5f;

            //case for only have address 1 
            if (addr.ProbOfCity < 0.5 && addr.ProbOfPostalCode < 0.5 && addr.ProbOfProv < 0.5 && addr.ProbOfAddress1 >= 0.5)
                return addr.ProbOfAddress1 * 0.8f;

            //case for incorrect province, without postal code,fg, of the form: 120 main st Ottawa
            if (addr.ProbOfProv <= 0.5 && addr.ProbOfPostalCode < 0.5 && addr.ProbOfAddress1 >= 0.5 && addr.ProbOfCity >= 0.5)
                return (addr.ProbOfAddress1 + addr.ProbOfCity) / 2.0f;
          
            //case for address without the city and province
            if (addr.ProbOfCity < 0.5 && addr.ProbOfPostalCode >= 0.5 && addr.ProbOfProv < 0.5 && addr.ProbOfAddress1 >= 0.5)
                return (addr.ProbOfPostalCode + addr.ProbOfAddress1) / 2.0f;

            //case for address without the postal code and city
            if (addr.ProbOfCity < 0.5 && addr.ProbOfPostalCode < 0.5 && addr.ProbOfProv >= 0.5 && addr.ProbOfAddress1 >= 0.5)
                return (addr.ProbOfProv + addr.ProbOfAddress1) / 2.0f;

            //case for address without the city
            if (addr.ProbOfCity < 0.5 && addr.ProbOfPostalCode >= 0.5 && addr.ProbOfProv >= 0.5 && addr.ProbOfAddress1 >= 0.5)
                return (addr.ProbOfProv + addr.ProbOfPostalCode + addr.ProbOfAddress1) / 3.0f;

            //case for address without the province
            if (addr.ProbOfCity >= 0.5 && addr.ProbOfPostalCode >= 0.5 && addr.ProbOfProv < 0.5 && addr.ProbOfAddress1 >= 0.5)
                return (addr.ProbOfPostalCode + addr.ProbOfAddress1 + addr.ProbOfCity) / 3.0f;

            //case for address without the postal code
            if (addr.ProbOfCity >= 0.5 && addr.ProbOfPostalCode < 0.5 && addr.ProbOfProv >= 0.5 && addr.ProbOfAddress1 >= 0.5)
                return (addr.ProbOfProv + addr.ProbOfAddress1 + addr.ProbOfCity) / 3.0f;
           

            return (addr.ProbOfAddress1 + addr.ProbOfCity + addr.ProbOfPostalCode + addr.ProbOfProv) / 4.0f;
        }

        private void DetermineAddressPart4(ref string address, ref Address addr)
        {
            addr.ProbOfAddress1 = DetermineAddress1ByProbability(ref address);
            addr.Address1 = address;
        }

        private void DetermineAddressPart3(ref string address, ref Address addr)
        {
            string[] possibleCityArr = address.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string possibleCity =String.Empty;

            if (address != String.Empty)
                possibleCity = possibleCityArr[possibleCityArr.Length-1];
            for (int i = possibleCityArr.Length - 2; i >= 0; i--)
            {             
                float probCity = DetermineCityByProbability(possibleCity,addr.Flag);
                if (addr.ProbOfCity < probCity)
                {
                    addr.ProbOfCity = probCity;
                    addr.City = possibleCity;
                }
                if (addr.ProbOfCity > probCity) break;
                possibleCity = possibleCityArr[i] + " " + possibleCity;
            }

            if (addr.ProbOfCity < 0.5)
            {
                addr.ProbOfCity *= 0.5f;
                addr.City = String.Empty;
            }
            if (addr.City.Length != 0)
                address = address.Substring(0, address.LastIndexOf(addr.City));
        }

        private void DetermineAddressPart2(ref string address, ref Address addr)
        {
            string[] dataArr = address.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = dataArr.Length - 1; i >= 0; i--)
            {
                float probOfPossibleProv = DetermineProvinceNameByProbability(dataArr[i],addr.Flag);
                if (addr.ProbOfProv < probOfPossibleProv)
                {
                    addr.ProbOfProv = probOfPossibleProv;
                    addr.Province = dataArr[i];
                }

                if (addr.ProbOfProv > 0.9)
                    break;

                if (probOfPossibleProv <= 0.5 && i > 0 && dataArr[i].Length != 2)
                {
                    string temp = dataArr[i - 1] + " " + dataArr[i];
                    float tempProb = DetermineProvinceNameByProbability(temp,addr.Flag);
                    if (addr.ProbOfProv < tempProb)
                    {
                        addr.ProbOfProv = tempProb;
                        addr.Province = temp;
                    }
                }
            }
            if (addr.ProbOfProv < 0.6)
            {
                addr.ProbOfProv *= 0.5f;
                addr.Province = String.Empty;
            }
            if (addr.Province.Length != 0)
                address = address.Substring(0, address.LastIndexOf(addr.Province));
        }

        private void DetermineAddressPart1(ref string address, ref Address addr)
        {
            string[] dataArr = address.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            int flag = 0;
            for (int i = dataArr.Length - 1; i >= 0; i--)
            {
                float probOfPossiblePostalCode = DeterminePostalCodeByProbability(dataArr[i],ref flag);
                if (addr.ProbOfPostalCode < probOfPossiblePostalCode)
                {
                    addr.ProbOfPostalCode = probOfPossiblePostalCode;
                    addr.PostalCode = dataArr[i];
                }
                if (addr.ProbOfPostalCode > 0.9) break;

                if (probOfPossiblePostalCode < 0.5 && i > 0)
                {
                    string temp = dataArr[i - 1] + " " + dataArr[i];
                    float tempProb = DeterminePostalCodeByProbability(temp,ref flag);
                    if (addr.ProbOfPostalCode < tempProb)
                    {
                        addr.ProbOfPostalCode = tempProb;
                        addr.PostalCode = temp;
                    }
                }
            }
            addr.Flag = flag;
            if (addr.ProbOfPostalCode < 0.6)
            {
                addr.ProbOfPostalCode *= 0.5f;
                addr.PostalCode = String.Empty;
            }
            if (addr.PostalCode.Length != 0)
                address = address.Substring(0, address.LastIndexOf(addr.PostalCode));
        }


        //public float DetermineAddress2(string address2)
        //{
        //    float score = 0.0f;
        //    var textReader = new FileReader(UnitFilePath);
        //    string[] unitDataArr = textReader.GetByLineToArray();
        //    string[] addressArr = address2.Split(' ');
        //    foreach (string term in addressArr)
        //    {
        //        if (Regex.IsMatch(term, @"^[0-9]*$")) score++;
        //        if (Array.FindIndex(unitDataArr, t => t.Equals(term, StringComparison.InvariantCultureIgnoreCase)) > 0) score += 2;
        //    }
        //    if (Regex.IsMatch(address2, @"^(?=.*[a-zA-Z])(?=.*[0-9])")) score++;

        //    return score / 4.0f;
        //}

        //public float DetermineCompletion(string id, string firstName, string lastName, string jobTitle, string siteName, string address1, string city, string province, string postalCode)
        //{
        //    float score = 0.0f;
        //    float total = 9.0f;
        //    if (id.Length == 10) score++;
        //    if (firstName.Length != 0) score++;
        //    if (lastName.Length != 0) score++;
        //    if (jobTitle.Length != 0) score++;
        //    if (siteName.Length != 0) score++;
        //    if (address1.Length != 0) score++;
        //    if (city.Length != 0) score++;
        //    if (province.Length != 0) score++;
        //    if (postalCode.Length != 0) score++;
        //    return (float)(score / total);
        //    //if ((float)(score / total) > 0.90) return true;
        //    //return false;
        //}

    }
}

