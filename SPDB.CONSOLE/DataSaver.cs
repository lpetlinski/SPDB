using SPDB.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDB.CONSOLE
{
    /// <summary>
    /// Class for saving result data into file.
    /// </summary>
    public class DataSaver
    {
        private const string DEFAULT_FILE = "result.csv";

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Tries to save predictions into given file.
        /// </summary>
        /// <param name="predictions">List of predictions to save</param>
        /// <param name="filePath">Wile name with path to save. If empty or null, then actual location with default name is used.</param>
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

        /// <summary>
        /// Aggregates road numbers from list of predictions, and returns it as a row of separated values.
        /// </summary>
        /// <param name="predictions">List of predictions computed by program.</param>
        /// <returns>Separated values</returns>
        private string GetRoadNumbers(List<PredictionResult> predictions)
        {
            return predictions.Select(p => p.RoadNumber.ToString()).Distinct().OrderBy(s => s).Aggregate("", (a, s) => a + SPDB.CONSOLE.Properties.Settings.Default.SaveSeparator + s);
        }

        /// <summary>
        /// Converts list of predictions into rows of separated values. Structure is: Date time, numbers separated by separator.
        /// </summary>
        /// <param name="predictions">List of predictions to save</param>
        /// <returns>List of rows to be written into file.</returns>
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

            return helper.Select((k) => k.Value.OrderBy(s => s.RoadNumber).Aggregate(k.Key.ToString("yyyy-MM-dd HH:mm:ss"), (a, r) => a + SPDB.CONSOLE.Properties.Settings.Default.SaveSeparator + r.TimeInDecisecond.ToString("#######.00000000000"))).ToList();
        }
    }
}
