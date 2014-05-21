using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDB.DAL
{
    public abstract class BaseRepository
    {
        private MySqlConnection connection;

        public MySqlConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        public BaseRepository()
        {
            var connectionString = new StringBuilder();
            connectionString.Append("SERVER=");
            connectionString.Append(SPDB.DAL.Properties.Settings.Default.DbServer);
            connectionString.Append(";DATABASE=");
            connectionString.Append(SPDB.DAL.Properties.Settings.Default.DbDatabase);
            connectionString.Append(";UID=");
            connectionString.Append(SPDB.DAL.Properties.Settings.Default.DbUsername);
            connectionString.Append(";PASSWORD=");
            connectionString.Append(SPDB.DAL.Properties.Settings.Default.DbPassword);
            this.connection = new MySqlConnection(connectionString.ToString());
        }

        protected bool OpenConnection()
        {
            try
            {
                this.connection.Open();
                return true;
            }
            catch (MySqlException exc)
            {
                switch (exc.Number)
                {
                    case 0:
                        //TODO Cannot connect to server
                        break;
                    case 1045:
                        //TODO invalid login or password
                        break;
                }
                return false;
            }
        }

        protected bool CloseConnection()
        {
            try
            {
                this.connection.Close();
                return true;
            }
            catch(MySqlException exc)
            {
                //TODO handle exception
                return false;
            }
        }
    }
}
