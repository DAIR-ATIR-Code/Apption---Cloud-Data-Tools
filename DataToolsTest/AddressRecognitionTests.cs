
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using Xunit;
using Xunit.Abstractions;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DataTools;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using DTHelperStd;
using System.Threading;
using System.Text.RegularExpressions;
using CsvHelper;

namespace DataToolsTest
{
    public class AddressRecognitionTests
    {
        private readonly string EnvironicsCsvPath = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\environics.csv";
        //private string EnvironicsDirtyFilePath = @"C:\Users\dzhao\source\repos\Cloud Data Platform\DataTools\Samples\environics-dirty.csv";
        private readonly string EnvironicsDirtierFilePath = $"{PathHelper.GetFolderRelativeToProject("Samples")}\\environics-dirtier.csv";
        private readonly ITestOutputHelper output;

        public AddressRecognitionTests(ITestOutputHelper output)
        {
            this.output = output;
        }


        [Fact]
        private void GivenCSVFile_ThenCityName()
        {
            var addrAnalyzer = new AddressAnalyzer();
            List<string> badDataList = new List<string>();
            using (var fileReader = new StreamReader(EnvironicsCsvPath))
            using (var csv = new CsvReader(fileReader))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    string city = csv.GetField<string>("city");
                    float cityProb = addrAnalyzer.DetermineCityByProbability(city, 0);
                    if (cityProb < 0.6) badDataList.Add(city);
                }
            }
            foreach (string badData in badDataList)
                output.WriteLine("Bad city name: {0} ", badData);
            Assert.Equal(155, badDataList.Count);
        }

        [Fact]
        private void GivenCSVFile_ThenProvinceName()
        {
            var addrAnalyzer = new AddressAnalyzer();
            List<string> badDataList = new List<string>();

            using (var fileReader = new StreamReader(EnvironicsCsvPath))
            using (var csv = new CsvReader(fileReader))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var province = csv.GetField<string>("prov");
                    float provProb = addrAnalyzer.DetermineProvinceNameByProbability(province, 0);
                    if (provProb < 0.6)
                    {
                        output.WriteLine("Bad Province: {0}, {1}", province, provProb);
                        badDataList.Add(province);
                    }
                }
            }
            foreach (string badData in badDataList)
                output.WriteLine("Bad province: {0} ", badData);
            Assert.Equal(241, badDataList.Count);
        }


        [Fact]
        private void GivenCSVFile_ThenPostalCode()
        {
            List<string> badDataList = new List<string>();
            var addrAnalyzer = new AddressAnalyzer();
            using (var fileReader = new StreamReader(EnvironicsCsvPath))
            using (var csv = new CsvReader(fileReader))
            {
                csv.Read();
                csv.ReadHeader();
                int flag = 0;
                while (csv.Read())
                {
                    var postalCode = csv.GetField<string>("postalCode");
                    float probOfPostalCode = addrAnalyzer.DeterminePostalCodeByProbability(postalCode, ref flag);
                    if (probOfPostalCode < 0.6) badDataList.Add(postalCode);
                }
            }
            foreach (string badData in badDataList)
                output.WriteLine("Bad postal code: {0} ", badData);
            Assert.Equal(100, badDataList.Count);
        }

        [Fact]
        private void GivenCSVFile_ThenAddressValidation()
        {
            List<float> confidenceList = new List<float>();
            var addrAnalyzer = new AddressAnalyzer();

            var buffer = new BlockingCollection<string>();
            var queue = new BlockingCollection<string>(500);


            var producers = Enumerable.Range(0, 1)
                .Select(_ => Task.Run(() =>
                {
                    foreach (string address in buffer.GetConsumingEnumerable())
                        queue.Add(address);
                })).ToArray();

            var consumer = Enumerable.Range(0, 7)
              .Select(_ => Task.Run(() =>
              {
                  float prob = 0.0f;
                  foreach (string address in queue.GetConsumingEnumerable())
                  {
                      Address addr = new Address();
                      prob = addrAnalyzer.DetermineAddressByProbability(address, ref addr);
                      output.WriteLine("address probability: {0}; {1}; {2}", prob, address, addr.ToString());
                  }
              })).ToArray();

            using (var fileReader = new StreamReader(EnvironicsDirtierFilePath))
            using (var csv = new CsvReader(fileReader))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var address = csv.GetField<string>("address");
                    buffer.Add(address);
                }
                buffer.CompleteAdding();
                Task.Factory.ContinueWhenAll(producers, _ =>
                {
                    queue.CompleteAdding();
                });
                Task.WaitAll(consumer);
            }
        }

        [Fact]
        private void GivenCSVFile_ThenAddressValidationForCleanData()
        {
            TextReader fileReader = File.OpenText(EnvironicsCsvPath);
            var csv = new CsvReader(fileReader);
            var addrAnalyzer = new AddressAnalyzer();

            string address1 = string.Empty;
            string city = string.Empty;
            string prov = string.Empty;
            string postalCode = string.Empty;
            int flag;

            //0 - address1 probability, 1 - city probability, 2 - province probability, 3 - postalCode probability
            float[] probArr = new float[4];

            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                flag = 0;
                address1 = csv.GetField<string>("address1");
                city = csv.GetField<string>("city");
                prov = csv.GetField<string>("prov");
                postalCode = csv.GetField<string>("postalCode");


                probArr[3] = addrAnalyzer.DeterminePostalCodeByProbability(postalCode, ref flag);
                probArr[2] = addrAnalyzer.DetermineProvinceNameByProbability(prov, flag);
                probArr[1] = addrAnalyzer.DetermineCityByProbability(city, flag);
                probArr[0] = addrAnalyzer.DetermineAddress1ByProbability(ref address1);


                float probabilityOfAddress = probArr.Sum() / 4.0f;
                output.WriteLine("address probability: {0}; {1}, {2}, {3}, {4}, {5} ", probabilityOfAddress, address1, city, prov, postalCode, flag);
            }
        }


        #region Old Test
        //[Fact]
        //private void GivenCSVFile_ThenTestParticularData()
        //{
        //    string data = "Under the deal, the Caisse will receive $1.54 billion in cash and $150 million in convertible debentures that will be convertible into Quebecor class-B shares. " +
        //    "The convertible debentures will have a six - year term maturing in and will bear 132 Kincardine Hwy Walkerton ON N0G2V0 an annual interest rate of 4.0 per cent. " +
        //    "Caisse chief executive Michael Sabia said the deal allows the pension fund manager to reallocate the money to new investment opportunities in Quebec companies. " +
        //    "Through the convertible debenture, la Caisse maintains an interest in the business, while providing Quebecor with increased financial flexibility to pursue its growth plan,” Sabia said in a statement.";
        //    var addrAnalyzer = new AddressAnalyzer();
        //    Address addr = new Address();
        //    string data = "Westpointe Center 1550 Coraopolis Heights Rd  Moon Township AB 15108";
        //    string[] dataArr = Regex.Split(data, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*),(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
        //    output.WriteLine("address probability: {0}; {1}; {2}", addrAnalyzer.DetermineAddressByProbability(data, ref addr), data, addr.ToString());
        //    foreach (string term in dataArr)
        //    {
        //        output.WriteLine("probability: {0} {1}", addrAnalyzer.DetermineAddress1ByProbability(term), term);
        //    }
        //     output.WriteLine("score: {0}", addrAnalyzer.AddressScore(data));
        //}


        //[Fact]
        //private void GivenCSVFile_ThenIsDataCompleted()
        //{
        //    TextReader fileReader = File.OpenText(EnvironicsCsvPath);
        //    var csv = new CsvReader(fileReader);
        //    var addrAnalyzer = new AddressAnalyzer();
        //    try
        //    {
        //        csv.Read();
        //        csv.ReadHeader();
        //        while (csv.Read())
        //        {
        //            var id = csv.GetField<string>("IndId");
        //            var firstName = csv.GetField<string>("firstName");
        //            var lastName = csv.GetField<string>("lastName");
        //            var jobTitle = csv.GetField<string>("jobTitle");
        //            var siteName = csv.GetField<string>("siteName");
        //            var address1 = csv.GetField<string>("address1");
        //            var city = csv.GetField<string>("city");
        //            var postalCode = csv.GetField<string>("postalCode");
        //            var province = csv.GetField<string>("prov");

        //            //bool isCompleted = addrAnalyzer.DetermineCompletion(id, firstName,lastName,
        //            // jobTitle, siteName, address1, city, province, postalCode);
        //            float isCompleted = addrAnalyzer.DetermineCompletion(id, firstName, lastName,
        //                jobTitle, siteName, address1, city, province, postalCode);
        //            output.WriteLine("IsComplete: {0}", isCompleted);
        //            Assert.True(isCompleted >= 0.85, $"Data not completed, score is {isCompleted}.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        output.WriteLine("Error: {0}", ex.Message);

        //    }

        //}


        //[Fact]
        //private void GivenCSVFile_ThenFindTheAddress()
        //{
        //    var addrAnalyzer = new AddressAnalyzer();
        //    var fileReader = new FileReader(EnvironicsCsvPath);
        //    List<string> badDataList = new List<string>();
        //    var dataList = fileReader.GetByLineToList();
        //    dataList.RemoveAt(0);
        //    List<string> addressList = new List<string>();
        //    foreach (string data in dataList)
        //    {
        //        string[] dataArr = Regex.Split(data, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*),(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
        //        float[] probabilityArr = new float[dataArr.Length];
        //        int i = 0;
        //        foreach (string term in dataArr)
        //        {
        //            //output.WriteLine("probability: {0} {1}", addrAnalyzer.DetermineAddress1ByProbability(term), term);
        //            probabilityArr[i++] = addrAnalyzer.DetermineAddress1ByProbability(term);

        //        }
        //        int possibleAddressIndex = Array.IndexOf(probabilityArr, probabilityArr.Max());
        //        addressList.Add(dataArr[possibleAddressIndex]);
        //    }

        //    foreach (string address in addressList)
        //        output.WriteLine("Address data: {0} ", address);
        //    Assert.Equal(78, badDataList.Count);
        //}

        //[Fact]
        //private void GivenCSVFile_ThenAddress2()
        //{
        //    TextReader fileReader = File.OpenText(EnvironicsCsvPath);
        //    var csv = new CsvReader(fileReader);
        //    var addrAnalyzer = new AddressAnalyzer();
        //    List<string> badDataList = new List<string>();
        //    csv.Read();
        //    csv.ReadHeader();
        //    while (csv.Read())
        //    {
        //        string address2 = csv.GetField<string>("address2");
        //        float isAddress2 = addrAnalyzer.DetermineAddress2(address2);
        //        if (address2.Length == 0) continue;
        //        output.WriteLine("address2: {0}", address2);
        //        output.WriteLine("isAddress2: {0}", isAddress2);

        //        if (isAddress2 < 0.4 && isAddress2 > 0) badDataList.Add(address2);
        //    }
        //    foreach (string badData in badDataList)
        //        output.WriteLine("Bad address2: {0} ", badData);
        //    Assert.Equal(51, badDataList.Count);
        //}
        #endregion
    }
}

