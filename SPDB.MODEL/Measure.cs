using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDB.MODEL
{
    public class Measure
    {
        public int Id
        {
            get;
            set;
        }

        public RoadPart RoadPart
        {
            get;
            set;
        }

        public MeasureDay Day
        {
            get;
            set;
        }

        public int TimeInDeciseconds
        {
            get;
            set;
        }

        public float Error
        {
            get;
            set;
        }
    }
}
