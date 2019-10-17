
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using DTHelperStd;
using NLog;
using RecognizerTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using WebAppMaterialize.App.Services.Interfaces;

namespace WebAppMaterialize.App.Services
{
	public enum StatePage
	{
        Connect,
		Analysis,
		Review,
        Classification,
        //Recommend,
		Upload
	}

	public class AppState
	{
        public IUIService UIService { get; private set; }

        public StatePage currentPage = StatePage.Connect;
        public CloudDataTools CloudDataTools { get; private set; }

        public Preferences Preferences { get; set; } = new Preferences();

		public int FirstPassProgress { get; set; } = 0;
        public int SecondPassProgress { get; set; } = 0;
        public bool IsProcessOneFinished { get; set; } = false;
        public bool IsProcessTwoFinished { get; set; } = true;


        public PreRequiredObject PreRequiredObject { get; set; } = new PreRequiredObject();

        public string UserFileName { get; set; } = string.Empty;

        //public List<string> ErrorText { get; set; } = new List<string>();
        private CancellationTokenSource tks = new CancellationTokenSource();

		private static Logger Logger = LogManager.GetCurrentClassLogger();

		public AppState(IUIService uiService)
		{
            UIService = uiService;
        }

		public AppState()
		{
		}

		public async Task AnalyzeFiles()
		{
            var obj = await FilesInterop.GetFilePath();
            var filepath = obj.ToString();
			if (!File.Exists(filepath))
			{
				Logger.Error("Fail in file upload or File not selected");                
				//AddError("File not selected. Please select a file");
				ErrorInterop.Alert("File not selected. Please select a file");
				NotifyStateChanged();
			}
			else
			{
                await Task.Run(() =>
                {
                    AnalyzeFile(filepath);
                });
			}
		}

		//private void AddError(string errorText)
		//{
		//	ErrorText.Add(errorText);
		//}

		private void AnalyzeFile(string filepath)
		{
            Logger.Trace("Begin first pass file analysis");
            CloudDataTools = new CloudDataTools(filepath, PreRequiredObject.HasHeaders, PreRequiredObject.SeparatorType);

            UpdateStatePage(StatePage.Analysis);
            var (fileService, tcs) = CloudDataTools.FileAnalyzer.AnalyzeAsyncWithObserver(tks.Token);
            var scheduler = TaskPoolScheduler.Default;
			fileService
			.Sample(TimeSpan.FromSeconds(1))
			.SubscribeOn(scheduler)
			.ObserveOn(new EventLoopScheduler())
			.Subscribe(newFileObject =>
			{
				//On next                    
                if (newFileObject.PercentProcessed > FirstPassProgress)
                {
                    FirstPassProgress = newFileObject.PercentProcessed;
                    NotifyProgressBarChanged();
                }
                if (newFileObject.PercentProcessed > 99)
                {
                    IsProcessOneFinished = true;
                    NotifyStateChanged();
                }
            },
			ex =>
			{
				Logger.Error(ex, "Error in first pass subscriber");
                ErrorInterop.Alert("Error in first pass subscriber. Program restarts. \nFor more detail, hit F9.");
                Reset();
            }, () =>
			{
                IsProcessOneFinished = true;
                NotifyStateChanged();
                // On complete
                CloudDataTools.FileObject.ConnectionString = PreRequiredObject.ConnectionString;
                CloudDataTools.FileObject.UserTableName = Path.GetFileNameWithoutExtension(FilesInterop.GetFileName().Result.ToString());
                UserFileName = string.Empty;
                UpdateStatePage(StatePage.Review);

			});
			tcs.Task.Wait();          
            Logger.Trace("End first pass file analysis");
		}

		public async Task SecondAnalysis()
		{
            Logger.Trace("Begin second pass file analysis");
            IsProcessTwoFinished = false;
            CloudDataTools.FileObject.ResetBytesProcessed();

            UpdateStatePage(StatePage.Classification);
            NotifyStateChanged();
            await Task.Run(async () => 
            {
                await PerformSecondAnalysis();
            });
            Logger.Trace("End second pass file analysis");
        }

        private async Task PerformSecondAnalysis()
        {
            var (ex, observable) = CloudDataTools.InitializeSecondPass();
            NotifyStateChanged();

            var scheduler = TaskPoolScheduler.Default;
            observable
            .SubscribeOn(scheduler)
            .ObserveOn(new EventLoopScheduler())
            .Subscribe(result =>
            {
                // On next
                if (CloudDataTools.FileObject.PercentProcessed > SecondPassProgress)
                {
                    SecondPassProgress = CloudDataTools.FileObject.PercentProcessed;
                    NotifyProgressBarChanged();
                }
            },
            exception =>
            {
                Logger.Error(exception, "Error in second pass file analysis");
                ErrorInterop.Alert("Error in second pass file analysis. Program restarts. \nFor more detail, hit F9.");
                Reset();
            }, () =>
            {
                // On complete
                IsProcessTwoFinished = true;
                UpdateStatePage(StatePage.Classification);
            });
            await ex.PerformSecondPassAnalysis();
        }

        public void Reset()
		{
            Logger.Info("Begin reset");
			Type type = this.GetType();
			PropertyInfo[] properties = type.GetProperties();
			var newState = new AppState(this.UIService);
			try
			{
				for (int i = 0; i < properties.Count(); ++i)
				{
					if (properties[i].CanWrite)
						properties[i].SetValue(this, properties[i].GetValue(newState));
				}
				UpdateStatePage(StatePage.Connect);

			}
			catch (Exception e)
			{
                Logger.Error(e, $"Error in reset: {e.Message}");
            }
            Logger.Info("Reset completed");
		}



		public void UpdateStatePage(StatePage newState)
		{
            Logger.Info($"Update page to {newState.ToString()}");
			currentPage = newState;
			NotifyStateChanged();
		}

		public event Action OnChange;
		private void NotifyStateChanged()
		{
			lock (this)
			{
				OnChange?.Invoke();
			}
		}

        public event Action OnProgressBarChange;
        private void NotifyProgressBarChanged()
        {
            lock (this)
            {
                OnProgressBarChange?.Invoke();
            }
        }
    }
}

