using SPDB.MODEL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDB.DAL
{
    public class InitialDataFiller: BaseRepository
    {
        public bool FillDatabaseWithData(List<RoadPart> roadParts, List<MeasureDay> measureDays, List<Measure> measures)
        {

            var rpResult = this.InsertRoadParts(roadParts);
            var mdResult = false;
            var mResult = false;
            if (rpResult)
            {
                mdResult = this.InsertMeasureDays(measureDays);
                if (mdResult)
                {
                    mdResult = this.InsertMeasures(measures);
                }
            }
            return rpResult && mdResult && mdResult;
        }

        private bool InsertRoadParts(List<RoadPart> roadParts)
        {
            log.Info("Starting: fill road data...");
            var roadPartInsert = new StringBuilder();
            roadPartInsert.Append("INSERT INTO ROAD_PART(NUMBER, LENGTH) VALUES ");
            foreach (var roadPart in roadParts)
            {
                roadPartInsert.Append("(");
                roadPartInsert.Append(roadPart.Number);
                roadPartInsert.Append(", ");
                roadPartInsert.Append(roadPart.Length);
                roadPartInsert.Append("),");
            }
            roadPartInsert.Remove(roadPartInsert.Length - 1, 1);
            var result = this.ExecuteNonQuery(roadPartInsert.ToString());
            log.Info("Finished: fill road data.");
            return result;
        }

        private bool InsertMeasureDays(List<MeasureDay> measureDays)
        {
            log.Info("Starting: fill measure days data...");
            var list = new List<string>();

            foreach (var measureDay in measureDays)
            {
                var measureDaysInsert = new StringBuilder();
                measureDaysInsert.Append("INSERT INTO MEASURE_DAY(ID, MEASURE_DATE) VALUES (");
                measureDaysInsert.Append(measureDay.Id);
                measureDaysInsert.Append(", '");
                measureDaysInsert.Append(measureDay.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                measureDaysInsert.Append("')");
                list.Add(measureDaysInsert.ToString());
            }
            var result = this.ExecuteNonQueries(list.ToArray());
            log.Info("Finished: fill measure days data.");
            return result;
        }

        private bool InsertMeasures(List<Measure> measures)
        {
            log.Info("Starting: fill measure data...");
            log.Info("Starting: Creating temporary file...");
            var result = true;
            for (var i = 0; i < (measures.Count / 100000) + 1; i++)
            {
                if (!result)
                {
                    break;
                }
                var list = new List<string>();
                for (var j = 0; j < 100000; j++)
                {
                    if (measures.Count <= i * 100000 + j)
                    {
                        break;
                    }
                    var measuresInsert = new StringBuilder();
                    measuresInsert.Append(measures[i * 100000 + j].Id);
                    measuresInsert.Append(",");
                    measuresInsert.Append(measures[i * 100000 + j].TimeInDeciseconds);
                    measuresInsert.Append(",");
                    measuresInsert.Append(measures[i * 100000 + j].Error.ToString("0.000000000", System.Globalization.CultureInfo.InvariantCulture));
                    measuresInsert.Append(",");
                    measuresInsert.Append(measures[i * 100000 + j].RoadPart.Number);
                    measuresInsert.Append(",");
                    measuresInsert.Append(measures[i * 100000 + j].Day.Id);
                    list.Add(measuresInsert.ToString());
                }
                File.AppendAllLines("temp.txt", list);
                var percent = (((i + 1) * 100000.0) / measures.Count) * 100;
                if (percent > 100)
                {
                    percent = 100;
                }
                var filePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).Replace("\\", "\\\\");
                var query = @"LOAD DATA INFILE '" + filePath + @"\\\\temp.txt' INTO TABLE MEASURE FIELDS TERMINATED BY ',' LINES TERMINATED BY '\r\n'";
                result = this.ExecuteNonQuery(query);
                File.Delete("temp.txt");
                log.Info("Finished " + percent.ToString("###.00") + "% : Creating temporary file");
            }
            log.Info("Finished: fill measure data.");
            return result;
        }
    }
}
