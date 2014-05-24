using SPDB.DAL;
using SPDB.MODEL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDB.CONSOLE
{
    public class DataPreparator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool PrepareData(string directory)
        {
            log.Info("Data preparation started");
            if (!this.CheckFilesExistence(directory))
            {
                return false;
            }

            var roadParts = this.LoadRoadParts(directory);
            var measureDays = new List<MeasureDay>();
            var measures = new List<Measure>();
            this.LoadMeasuresAndDays(directory, roadParts, measureDays, measures);
            this.LoadErrorValues(directory, measures);

            var dbFiller = new InitialDataFiller();
            if (!dbFiller.FillDatabaseWithData(roadParts, measureDays, measures))
            {
                return false;
            }

            log.Info("Data preparation finished");
            return true;
        }

        private List<RoadPart> LoadRoadParts(string directory)
        {
            log.Info("Starting: loading road parts...");
            var lines = File.ReadLines(directory + @"\" + SPDB.CONSOLE.Properties.Settings.Default.RouteLengthFile);
            var result = new List<RoadPart>();
            foreach (var line in lines.Where((s, i) => i != 0))
            {
                var limiter = line.IndexOf(',');
                var route = int.Parse(line.Substring(0, limiter));
                var length = int.Parse(line.Substring(limiter + 1));
                result.Add(new RoadPart()
                {
                    Number = route,
                    Length = length
                });
            }
            log.Info("Finished: loading road parts.");
            return result;
        }

        private void LoadMeasuresAndDays(string directory, List<RoadPart> roadParts, List<MeasureDay> measureDays, List<Measure> measures)
        {
            log.Info("Starting: loading measures and days...");
            var lines = File.ReadLines(directory + @"\" + SPDB.CONSOLE.Properties.Settings.Default.DataFile);
            var dayId = 1;
            var measureId = 1;
            foreach (var line in lines.Where(s =>  s.Last() != ',' && s.Last() != 'x'))
            {
                var firstLimiterPos = line.IndexOf(',');
                var date = line.Substring(0, firstLimiterPos);
                var restData = line.Substring(firstLimiterPos + 1);
                restData += ",";
                var MeasureDay = new MeasureDay()
                {
                    Date = this.ParseDate(date),
                    Id = dayId++
                };
                measureDays.Add(MeasureDay);
                foreach (var roadPart in roadParts)
                {
                    var limiterPos = restData.IndexOf(',');
                    var nextData = restData.Substring(0, limiterPos);
                    measures.Add(new Measure()
                    {
                        Day = MeasureDay,
                        Id = measureId++,
                        RoadPart = roadPart,
                        TimeInDeciseconds = int.Parse(nextData)
                    });
                    restData = restData.Substring(limiterPos + 1);
                }
            }
            log.Info("Finished: loading measures and days.");
        }

        private void LoadErrorValues(string directory, List<Measure> measures)
        {
            log.Info("Starting: loading error values...");
            var lines = File.ReadLines(directory + @"\" + SPDB.CONSOLE.Properties.Settings.Default.ErrorFile);
            var measurePosition = 0;
            foreach (var line in lines.Where(s => s.Last() != ',' && s.Last() != 'x'))
            {
                var limiterPos = line.IndexOf(',');
                var restData = line.Substring(limiterPos + 1) + ',';
                while(restData.Length > 0)
                {
                    limiterPos = restData.IndexOf(',');
                    var nextData = restData.Substring(0, limiterPos);
                    measures[measurePosition].Error = float.Parse(nextData, System.Globalization.CultureInfo.InvariantCulture);
                    measurePosition++;
                    restData = restData.Substring(limiterPos + 1);
                }
            }
            log.Info("Finished: loading error values.");
        }

        private DateTime ParseDate(string dateWithTime)
        {
            var limiterPos = dateWithTime.IndexOf('-');
            var year = int.Parse(dateWithTime.Substring(0, limiterPos));
            var restData = dateWithTime.Substring(limiterPos + 1);
            limiterPos = restData.IndexOf("-");
            var month = int.Parse(restData.Substring(0, limiterPos));
            restData = restData.Substring(limiterPos + 1);
            limiterPos = restData.IndexOf(" ");
            var day = int.Parse(restData.Substring(0, limiterPos));
            restData = restData.Substring(limiterPos + 1);
            limiterPos = restData.IndexOf(":");
            var hour = int.Parse(restData.Substring(0, limiterPos));
            restData = restData.Substring(limiterPos + 1);
            var minute = int.Parse(restData);
            return new DateTime(year, month, day, hour, minute, 0);
        }

        private bool CheckFilesExistence(string directory)
        {
            log.Info("Starting: checking file existence...");
            if (!File.Exists(directory + @"\" + SPDB.CONSOLE.Properties.Settings.Default.RouteLengthFile))
            {
                log.Error("File with road lengths doesn't exist");
                return false;
            }

            if (!File.Exists(directory + @"\" + SPDB.CONSOLE.Properties.Settings.Default.DataFile))
            {
                log.Error("File with measures data doesn't exist");
                return false;
            }

            if (!File.Exists(directory + @"\" + SPDB.CONSOLE.Properties.Settings.Default.ErrorFile))
            {
                log.Error("File with measures errors doesn't exist");
                return false;
            }
            log.Info("Finished: checking file existence.");
            return true;
        }
    }
}
