using SPDB.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace SPDB.CONSOLE
{
    public class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            if (args.Any(s => s == "CREATE"))
            {
                var creator = new DatabaseCreator();
                creator.CreateDatabase();
                var preparator = new DataPreparator();
                //preparator.PrepareData(@"E:\studia\mgr 2 sem\SPDB\Projekt");
                preparator.PrepareData(@"C:\others\spdb dane");
                log.Info("FINISHED!");
            }
        }
    }
}
