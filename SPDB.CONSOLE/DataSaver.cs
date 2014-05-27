using SPDB.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDB.CONSOLE
{
    public class DataSaver
    {
        private const string DEFAULT_FILE = "result.csv";

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void TryToSaveData(List<PredictionResult> predictions, string filePath)
        {
            var file = filePath;
            if (file == null || file.Length == 0)
            {
                file = DEFAULT_FILE;
            }

            var linesToAdd = new List<string>();
            linesToAdd.Add(this.GetRoadNumbers(predictions));
            linesToAdd.AddRange(this.ConvertPredictionsToLines(predictions));

            log.Info("Starting: Saving data to file: " + file + "...");
            File.WriteAllLines(file, linesToAdd);
            log.Info("Finished: Saving data to file: " + file + "...");
        }

        private string GetRoadNumbers(List<PredictionResult> predictions)
        {
            return predictions.Select(p => p.RoadNumber.ToString()).Distinct().Aggregate("", (a, s) => a + ";" + s);
        }

        private List<string> ConvertPredictionsToLines(List<PredictionResult> predictions)
        {
            var helper = new Dictionary<DateTime, List<PredictionResult>>();

            foreach (var prediction in predictions)
            {
                if (!helper.ContainsKey(prediction.PredictionDate))
                {
                    helper.Add(prediction.PredictionDate, new List<PredictionResult>());
                }
                helper[prediction.PredictionDate].Add(prediction);
            }

            return helper.Select((k) => k.Value.OrderBy(s => s.RoadNumber).Aggregate(k.Key.ToString("yyyy-MM-dd HH:mm:ss"), (a, r) => a + ";" + r.TimeInDecisecond.ToString("#######.00000000000"))).ToList();
        }
    }
}
