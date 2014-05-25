using SPDB.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace SPDB.CONSOLE
{
    sealed class CmdArguments
    {
        private readonly string name;

        public static readonly CmdArguments CREATE_DB = new CmdArguments("--create");
        public static readonly CmdArguments INSERT_DATA = new CmdArguments("--insert");
        public static readonly CmdArguments ARGUMENT_PREFIX = new CmdArguments("--");
        public static readonly CmdArguments FOR_DATE_TIME = new CmdArguments("--fordatetime");
        public static readonly CmdArguments HELP = new CmdArguments("--help");

        private CmdArguments(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return this.name;
        }
    }

    public class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            var arguments = GetOperationsWithValues(args);
            if (arguments.ContainsKey(CmdArguments.HELP.ToString()) || arguments.Count == 0)
            {
                System.Console.WriteLine("Possible commands:");
                System.Console.WriteLine("\t --create - Drops database if possible and creates a new one");
                System.Console.WriteLine("\t --insert [directoryPath] - Inserts data from files associated with problem. If direcotryPath is provided, then files are searched for there, if not, then they are searhed for in assembly directory");
                System.Console.WriteLine("\t --fordatetime [date time] - Predicts travel times for routes. If date and time is provided, then this date-time would be used, otherwise actual date-time is used. Date format: YYYY-MMM-DDD. Time format: HH:MM:SS");
                System.Console.WriteLine("\t --help - This page");
            }
            else
            {
                if (arguments.ContainsKey(CmdArguments.CREATE_DB.ToString()))
                {
                    var creator = new DatabaseCreator();
                    creator.CreateDatabase();
                }
                if (arguments.ContainsKey(CmdArguments.INSERT_DATA.ToString()))
                {
                    var directory = arguments[CmdArguments.INSERT_DATA.ToString()].FirstOrDefault();
                    if (directory == null)
                    {
                        directory = "";
                    }
                    var dataPreparator = new DataPreparator();
                    dataPreparator.PrepareData(directory);
                }
                if (arguments.ContainsKey(CmdArguments.FOR_DATE_TIME.ToString()))
                {
                    var predict = new RoadTimePredicter();
                    var dateString = arguments[CmdArguments.FOR_DATE_TIME.ToString()].FirstOrDefault();
                    var date = DateTime.Now;
                    if (dateString != null)
                    {
                        date = DateTime.Parse(dateString);
                    }
                    var result = predict.PredictAtDateTime(date);
                    foreach (var a in result)
                    {
                        log.Info(a.RoadNumber.ToString() + ": " + a.TimeInDecisecond.ToString("###########.00000000"));
                    }
                }
            }
#if DEBUG
            log.Info("FINISHED, press enter to exit");
            System.Console.ReadLine();
#else
            log.Info("FINISHED");
#endif
        }

        private static Dictionary<string, List<string>> GetOperationsWithValues(string[] arguments)
        {
            var result = new Dictionary<string, List<string>>();

            for (var i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] == CmdArguments.CREATE_DB.ToString())
                {
                    result.Add(arguments[i], null);
                }
                if (arguments[i] == CmdArguments.INSERT_DATA.ToString())
                {
                    result.Add(arguments[i], new List<string>());
                    if (i + 1 < arguments.Length && !arguments[i + 1].StartsWith(CmdArguments.ARGUMENT_PREFIX.ToString()))
                    {
                        result[arguments[i]].Add(arguments[i + 1]);
                        i++;
                    }
                }
                if (arguments[i] == CmdArguments.FOR_DATE_TIME.ToString())
                {
                    result.Add(arguments[i], new List<string>());
                    if (i + 2 < arguments.Length && !arguments[i + 1].StartsWith(CmdArguments.ARGUMENT_PREFIX.ToString()) && !arguments[i + 2].StartsWith(CmdArguments.ARGUMENT_PREFIX.ToString()))
                    {
                        result[arguments[i]].Add(ParseDate(arguments[i + 1], arguments[i + 2]).ToString(System.Globalization.CultureInfo.InvariantCulture));
                        i++;
                        i++;
                    }
                }
                if (arguments[i] == CmdArguments.HELP.ToString())
                {
                    result.Add(arguments[i], null);
                }
            }

            return result;
        }

        private static DateTime ParseDate(string date, string time)
        {
            if (!Regex.IsMatch(date, @"^\d{4}-\d{2}-\d{2}$"))
            {
                log.Error("Invalid date");
                return DateTime.Now;
            }

            if (!Regex.IsMatch(time, @"^\d{2}:\d{2}:\d{2}$"))
            {
                log.Error("Invalid time");
                return DateTime.Now;
            }
            var year = int.Parse(date.Substring(0, 4));
            var month = int.Parse(date.Substring(5, 2));
            var day = int.Parse(date.Substring(8, 2));

            var hour = int.Parse(time.Substring(0, 2));
            var minute = int.Parse(time.Substring(3, 2));
            var second = int.Parse(time.Substring(6, 2));

            return new DateTime(year, month, day, hour, minute, second);
        }
    }
}
