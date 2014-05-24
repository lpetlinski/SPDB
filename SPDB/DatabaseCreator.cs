using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDB.DAL
{
    public class DatabaseCreator : BaseRepository
    {
        public void CreateDatabase()
        {
            log.Info("Creating database started");
            var dropDay = "DROP TABLE IF EXISTS MEASURE_DAY";
            var createDay = @"CREATE TABLE MEASURE_DAY
                            (
                                ID INT NOT NULL,
                                MEASURE_DATE DATETIME NOT NULL,

                                PRIMARY KEY(ID)
                            )";
            var dropRoad = "DROP TABLE IF EXISTS ROAD_PART";
            var createRoad = @"CREATE TABLE ROAD_PART
                                (
                                    NUMBER INT NOT NULL,
                                    LENGTH INT NOT NULL,

                                    PRIMARY KEY(NUMBER)
                                )";
            var dropMeasure = "DROP TABLE IF EXISTS MEASURE";
            var createMeasure = @"CREATE TABLE MEASURE
                                (
                                    ID INT NOT NULL,
                                    TIME_IN_DECISECOND INT NOT NULL,
                                    ERROR DECIMAL(10,9) NOT NULL,
                                    ROAD_NUMBER INT NOT NULL,
                                    MEASURE_DAY_ID INT NOT NULL,

                                    PRIMARY KEY(ID),

                                    FOREIGN KEY (ROAD_NUMBER)
                                        REFERENCES ROAD_PART(NUMBER),
                                    FOREIGN KEY (MEASURE_DAY_ID)
                                        REFERENCES MEASURE_DAY(ID)
                                )";
            var toExecute = new string[6];
            toExecute[0] = dropMeasure;
            toExecute[1] = dropRoad;
            toExecute[2] = dropDay;
            toExecute[3] = createDay;
            toExecute[4] = createRoad;
            toExecute[5] = createMeasure;
            if (this.ExecuteNonQueries(toExecute))
            {
                log.Info("Database created");
            }
        }
    }
}
