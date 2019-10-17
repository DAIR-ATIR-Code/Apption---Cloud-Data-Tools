
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DTHelperStd.DataStructures;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using System.IO;
using System.Text;

namespace DTHelperStd
{
    public class MLHelper
    {
        public static PredictionEngine<Data, DataPrediction> CityPredictionEngine { get; private set; }
        public static PredictionEngine<Data, DataPrediction> FirstNamePredictionEngine { get; private set; }
        public static PredictionEngine<Data, DataPrediction> LastNamePredictionEngine { get; private set; }
        private static readonly string BaseModelsPath = @"../MLModels";
        private static readonly string CityModelPath= $"{BaseModelsPath}/CityModel.zip";
        private static readonly string FirstModelPath= $"{BaseModelsPath}/FirstNameModel.zip";
        private static readonly string LastNameModelPath= $"{BaseModelsPath}/LastNameModel.zip";


        public static void InitializeMLModel()
        {
            var mlContext = new MLContext(seed: 1);
            ITransformer cityTrainedModel, firstNameTrainedModel, lastNameTrainedModel;
            using (var stream = new FileStream(CityModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                cityTrainedModel = mlContext.Model.Load(stream);
            }
            using (var stream = new FileStream(FirstModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                firstNameTrainedModel = mlContext.Model.Load(stream);
            }
            using (var stream = new FileStream(LastNameModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                lastNameTrainedModel = mlContext.Model.Load(stream);
            }
            CityPredictionEngine = cityTrainedModel.CreatePredictionEngine<Data, DataPrediction>(mlContext);
            FirstNamePredictionEngine = firstNameTrainedModel.CreatePredictionEngine<Data, DataPrediction>(mlContext);
            LastNamePredictionEngine = lastNameTrainedModel.CreatePredictionEngine<Data, DataPrediction>(mlContext);

        }
    }
}

