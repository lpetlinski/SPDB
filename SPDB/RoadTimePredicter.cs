using MySql.Data.MySqlClient;
using SPDB.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDB.DAL
{
    /// <summary>
    /// Class used as a result of prediction.
    /// </summary>
    public class PredictionResult
    {
        /// <summary>
        /// Road number
        /// </summary>
        public int RoadNumber
        {
            get;
            internal set;
        }

        /// <summary>
        /// Predicted time in Deciseconds
        /// </summary>
        public float TimeInDecisecond
        {
            get;
            set;
        }

        /// <summary>
        /// Date for predictions.
        /// </summary>
        public DateTime PredictionDate
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Class used for predicting drive times.
    /// </summary>
    public class RoadTimePredicter: BaseRepository
    {
        /// <summary>
        /// Predicts travel times for all road parts for each of given dates.
        /// </summary>
        /// <param name="dateTimes">List of dateTimes to predict travel times for</param>
        /// <returns>List fo predictions for asked date-times.</returns>
        public List<PredictionResult> PredictAtDateTimes(List<DateTime> dateTimes)
        {
            var result = new List<PredictionResult>();

            log.Info("Starting: get predictions for dates...");

            if (this.OpenConnection())
            {
                try
                {
                    var i = 0;
                    foreach (var dateTime in dateTimes)
                    {
                        log.Info("Starting: get predicition for roads at: " + dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        var formattedDate = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                        var query = @"select rp.number as number, sum(m.time_in_decisecond*(2-m.error)*((select MAX(DATEDIFF('" + formattedDate + @"', MEASURE_DATE))
	                                from measure_day)-DATEDIFF('" + formattedDate + @"', md.MEASURE_DATE))/30.0) as sum_time, sum((2-m.error)*((select MAX(DATEDIFF('" + formattedDate + @"', MEASURE_DATE))
	                                from measure_day)-DATEDIFF('" + formattedDate + @"', md.MEASURE_DATE))/30.0) as weight
                                from (measure_day as md left join
	                                measure as m on md.id = m.measure_day_id) left join
	                                road_part as rp on rp.number = m.road_number
                                where ABS(TO_SECONDS(TIME('" + formattedDate + @"')) - TO_SECONDS(TIME(md.MEASURE_DATE))) = (
	                                select MIN(ABS(TO_SECONDS(TIME('" + formattedDate + @"')) - TO_SECONDS(TIME(MEASURE_DATE))))
	                                from measure_day
	                                where DATEDIFF('" + formattedDate + @"', MEASURE_DATE) > 0
                                ) AND DATEDIFF('" + formattedDate + @"', MEASURE_DATE) > 0
                                GROUP BY rp.number";
                        var command = new MySqlCommand(query, this.Connection);
                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var predictionRow = new PredictionResult()
                            {
                                RoadNumber = reader.GetInt32("number"),
                                TimeInDecisecond = reader.GetFloat("sum_time") / reader.GetFloat("weight"),
                                PredictionDate = dateTime
                            };
                            result.Add(predictionRow);
                        }
                        reader.Close();
                        log.Info("Finished: get predicition for roads at: " + dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        i++;
                        log.Info("Finished " + (((float)i) / dateTimes.Count*100).ToString("###.0") + "%: get predictions for dates.");
                    }
                }
                finally
                {
                    this.CloseConnection();
                }
            }

            log.Info("Finished: get predictions for dates.");

            return result;
        }
    }
}
