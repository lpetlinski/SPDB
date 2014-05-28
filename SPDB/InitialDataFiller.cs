using SPDB.MODEL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDB.DAL
{
    /// <summary>
    /// Class used for fill database with data.
    /// </summary>
    public class InitialDataFiller: BaseRepository
    {
        /// <summary>
        /// Fills database with data.
        /// </summary>
        /// <param name="roadParts">List of road parts to add to database.</param>
        /// <param name="measureDays">List of measure days to add to database.</param>
        /// <param name="measures">List of measures to add to database</param>
        /// <returns>True on success.</returns>
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

        /// <summary>
        /// Insers road parts into database.
        /// </summary>
        /// <param name="roadParts">Road parts to add to database.</param>
        /// <returns>True on success.</returns>
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

        /// <summary>
        /// Inserts measure days into database.
        /// </summary>
        /// <param name="measureDays">Measure days to add to database.</param>
        /// <returns>True on success.</returns>
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

        /// <summary>
        /// Inserts measures into database.
        /// 
        /// NOTE: There were problem to insert all this data to database in one sql, beacuse of timeout 9there are more than 5M rows to add).
        /// Because of that, data is inserted in parts of 100k.
        /// Also to speed everything up, data is first inserted into file and then read from it into database by bulk load.
        /// Also woth to notice, that bulk load with 5M dat also result in timeout :).
        /// </summary>
        /// <param name="measures">Measures to add.</param>
        /// <returns>True on success</returns>
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
