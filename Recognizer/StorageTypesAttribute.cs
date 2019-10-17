
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using DataTools;

namespace RecognizerTools
{
    public class StorageTypesAttribute : Attribute
    {
        private StorageType[] storageType;

        public StorageType[] StorageTypes
        {
            get { return storageType; }            
        }


        public StorageTypesAttribute(StorageType[] storageType)
        {
            this.storageType = storageType;
        }
    }
}
