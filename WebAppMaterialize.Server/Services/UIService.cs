
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using ElectronNET.API;
using ElectronNET.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppMaterialize.App.Services.Interfaces;

namespace WebAppMaterialize.Server.Services
{
    public class UIService : IUIService
    {
        public async Task<string[]> OpenFileDialog()
        {
            if (Electron.WindowManager.BrowserWindows.Count == 0)
            {
                throw new InvalidProgramException("Electron not found.");
            }
            return await OpenElectronFileDialog();
        }

        private async Task<string[]> OpenElectronFileDialog()
        {
            var mainWindow = Electron.WindowManager.BrowserWindows.First();
            var options = new OpenDialogOptions
            {
                Properties = new OpenDialogProperty[]
                {
                    OpenDialogProperty.openFile
                }
            };

            return await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
        }
    }
}

