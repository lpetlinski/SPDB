using MySql.Data.MySqlClient;
using SPDB.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDB.DAL
{
    public class PredictionResult
    {
        public int RoadNumber
        {
            get;
            internal set;
        }

        public float TimeInDecisecond
        {
            get;
            set;
        }
    }

    public class RoadTimePredicter: BaseRepository
    {
        public List<PredictionResult> PredictAtDateTime(DateTime dateTime)
        {
            var result = new List<PredictionResult>();

            log.Info("Starting: get predicition for roads at: " + dateTime.ToString("yyyy-MM-dd HH:mm:ss"));

            if (this.OpenConnection())
            {
                try
                {
                    var formattedDate = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    var query = @"select rp.number as number, sum(m.time_in_decisecond*(2-m.error)*LOG(5.477, (select MAX(DATEDIFF('" + formattedDate + @"', MEASURE_DATE))
	                                from measure_day)-DATEDIFF('" + formattedDate + @"', md.MEASURE_DATE))) as sum_time, sum((2-m.error)*LOG(5.477, (select MAX(DATEDIFF('" + formattedDate + @"', MEASURE_DATE))
	                                from measure_day)-DATEDIFF('" + formattedDate + @"', md.MEASURE_DATE))) as weight
                                from measure_day as md,
	                                measure as m,
	                                road_part as rp
                                where ABS(TO_SECONDS(TIME('" + formattedDate + @"')) - TO_SECONDS(TIME(md.MEASURE_DATE))) = (
	                                select MIN(ABS(TO_SECONDS(TIME('" + formattedDate + @"')) - TO_SECONDS(TIME(MEASURE_DATE))))
	                                from measure_day
	                                where DATEDIFF('" + formattedDate + @"', MEASURE_DATE) > 0
                                ) AND md.id = m.measure_day_id AND m.road_number = rp.number AND DATEDIFF('" + formattedDate + @"', MEASURE_DATE) > 0
                                GROUP BY rp.number";
                    var command = new MySqlCommand(query, this.Connection);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var predictionRow = new PredictionResult()
                        {
                            RoadNumber = reader.GetInt32("number"),
                            TimeInDecisecond = reader.GetFloat("sum_time") / reader.GetFloat("weight")
                        };
                        result.Add(predictionRow);
                    }
                    reader.Close();
                }
                finally
                {
                    this.CloseConnection();
                }
            }

            log.Info("Finished: get predicition for roads at: " + dateTime.ToString("yyyy-MM-dd HH:mm:ss"));

            return result;
        }
    }
}
