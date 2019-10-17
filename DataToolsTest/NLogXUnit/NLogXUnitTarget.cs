
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using NLog;
using NLog.Targets;
using Xunit.Abstractions;

namespace DataToolsTest.NLogXUnit
{
    [Target("XUnit")]
    public class NLogXUnitTarget : TargetWithLayoutHeaderAndFooter
    {
        private ITestOutputHelper _output;

        public NLogXUnitTarget(ITestOutputHelper output)
        {
            _output = output;
        }
        /// <summary>
        /// Writes the specified logging event to the Console.Out or
        /// Console.Error depending on the value of the Error flag.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <remarks>
        /// Note that the Error option is not supported on .NET Compact Framework.
        /// </remarks>
        protected override void Write(LogEventInfo logEvent)
        {
            this.Output(this.Layout.Render(logEvent));
        }

        private void Output(string s)
        {
            _output.WriteLine(s);
        }
    }
}

