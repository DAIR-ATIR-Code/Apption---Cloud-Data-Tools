
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Newtonsoft.Json;
using DataTools;
using System.Threading;
using Xunit.Abstractions;

namespace DataToolsTest
{
    public class JSonSerializationTest
    {
        private ITestOutputHelper output;

        public JSonSerializationTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact (Skip ="Has Errors")]
        public void GivenColumnMetadata_SerializeAndDeserialize()
        {
            var cm = new ColumnMetadata(5);
            cm.FieldName = "FieldOne";
            cm.DataType = DataType.Address;
            var jsonText = JsonConvert.SerializeObject(cm);
            output.WriteLine(jsonText);
            var newCm = JsonConvert.DeserializeObject<ColumnMetadata>(jsonText);
            Assert.Equal(5, cm.ColumnIndex);
            Assert.Equal(cm.FieldName, newCm.FieldName);
            Assert.Equal(cm.DataType, newCm.DataType);
        }

        [Fact]
        public void GivenColumnMetadataWithUserType_SerializeAndDeserialize()
        {
            var cm = new ColumnMetadata(1);
            cm.FieldName = "FieldOne";
            cm.SetFieldName("RealField");
            cm.DataType = DataType.Address;
            cm.SetDataType(DataType.Country);
            var jsonText = JsonConvert.SerializeObject(cm);
            output.WriteLine(jsonText);
            var newCm = JsonConvert.DeserializeObject<ColumnMetadata>(jsonText);
            Assert.Equal("RealField", newCm.FieldName);
            Assert.Equal(DataType.Country, newCm.DataType);
        }

        [Fact]
        public void GivenColumnMetadataWithValidTypes_SerializeAndDeserialize()
        {
            var cm = new ColumnMetadata(1);
            cm.ValidStorageTypes = new[] { StorageType.Bigint};
            cm.FieldName = "FieldOne";
            cm.SetFieldName("RealField");
            cm.DataType = DataType.Address;
            cm.SetDataType(DataType.Country);
            var jsonText = JsonConvert.SerializeObject(cm);
            output.WriteLine(jsonText);
            var newCm = JsonConvert.DeserializeObject<ColumnMetadata>(jsonText);
            Assert.Equal("RealField", newCm.FieldName);
            Assert.Equal(DataType.Country, newCm.DataType);
            Assert.Single(newCm.ValidStorageTypes, StorageType.Bigint);
        }
    }
}

