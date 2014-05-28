using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace SPDB.DAL
{
    /// <summary>
    /// Base repository to be base class for every repository class.
    /// </summary>
    public abstract class BaseRepository
    {
        private MySqlConnection connection;

        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected MySqlConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        /// <summary>
        /// Creates new repository, and creates connection string to database.
        /// </summary>
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

        /// <summary>
        /// Opens connection to database.
        /// </summary>
        /// <returns>True if everything went ok.</returns>
        protected bool OpenConnection()
        {
            try
            {
                this.connection.Open();
                log.Info("Connected to database");
                return true;
            }
            catch (MySqlException exc)
            {
                switch (exc.Number)
                {
                    case 0:
                        log.Error("Cannot connect to database. Exception:" + exc.ToString());
                        //TODO Cannot connect to server
                        break;
                    case 1045:
                        log.Error("Invalid login or password. Exception " + exc.ToString());
                        //TODO invalid login or password
                        break;
                    default:
                        log.Error("Other error from db. Stack trace: " + exc.ToString());
                        break;
                }
                return false;
            }
        }

        /// <summary>
        /// Closes database connection.
        /// </summary>
        /// <returns>True if connection was closed successfully.</returns>
        protected bool CloseConnection()
        {
            try
            {
                this.connection.Close();
                log.Info("Connection closed");
                return true;
            }
            catch (MySqlException exc)
            {
                log.Error("Closing connection failed");
                return false;
            }
        }

        /// <summary>
        /// Executes given non query.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <returns>True on success.</returns>
        protected bool ExecuteNonQuery(string query)
        {
            return this.ExecuteNonQueries(new string[] { query });
        }

        /// <summary>
        /// Executes array of queries in one transaction.
        /// </summary>
        /// <param name="queries">Queries to execute</param>
        /// <returns>True on success.</returns>
        protected bool ExecuteNonQueries(string[] queries)
        {
            var result = true;
            if (this.OpenConnection())
            {
                try
                {
                    var transaction = this.Connection.BeginTransaction();

                    try
                    {
                        foreach (var query in queries)
                        {
                            var command = new MySqlCommand(query, this.Connection, transaction);
                            command.ExecuteNonQuery();
                            log.Debug("Executed Query: " + query);
                        }
                        log.Info("Commiting transaction");
                        transaction.Commit();
                        log.Info("Commit succeded");
                    }
                    catch (Exception exc)
                    {
                        log.Error("Commiting transaction failed. Stack Trace: " + exc.ToString());
                        transaction.Rollback();
                        result = false;
                    }
                }
                finally
                {
                    this.CloseConnection();
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
    }
}
