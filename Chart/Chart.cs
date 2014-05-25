using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace Chart
{
    public class Chart
    {
        private Dialog dialog = null;
        /// <summary>
        /// initialize chart control
        /// </summary>
        public Chart()
        {
            dialog = new Dialog();
            dialog.chart1.Series.Clear();
        }
        public void SetTitle(string title)
        {
            dialog.chart1.Titles.Add(title);
        }
        /// <summary>
        /// draw passed values.
        /// Dont sort it.
        /// </summary>
        /// <param name="seriesName"></param>
        /// <param name="sortedResults"></param>
        public void AddDataSeries(string seriesName, double[] sortedResults)
        {
            Series series = dialog.chart1.Series.Add(seriesName);
            foreach (double d in sortedResults)
            {
                series.Points.Add(d);
            }
        }
        public void DrawChart()
        {
            dialog.chart1.Update();
            dialog.Show();
        }
        public void DrawChartModal()
        {
            dialog.chart1.Update();
            dialog.ShowDialog();
        }
    }
}
