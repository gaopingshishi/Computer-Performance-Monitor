//using LiveChartsCore.Defaults;
//using LiveChartsCore.SkiaSharpView.Painting;
//using LiveChartsCore.SkiaSharpView;
//using LiveChartsCore;
//using SkiaSharp;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace App.Common
//{
//    public class SeriesHelper
//    {
//        /// <summary>
//        /// 设置进度条颜色
//        /// </summary>
//        /// <param name="seriesList"></param>
//        /// <param name="color"></param>
//        public static void SetSeriesColor(IEnumerable<ISeries> seriesList, SKColor color)
//        {
//            var series = seriesList?.FirstOrDefault();
//            if (series is PieSeries<ObservableValue> seriesInfo)
//            {
//                if (seriesInfo.Fill is SolidColorPaint fillPaint)
//                {
//                    fillPaint.Color = color;
//                }
//            }
//        }

//        /// <summary>
//        /// 设置进度条值
//        /// </summary>
//        /// <param name="seriesList"></param>
//        /// <param name="value"></param>
//        public static void SetSeriesValue(IEnumerable<ISeries> seriesList, double value)
//        {
//            if (seriesList?.FirstOrDefault()?.Values is IEnumerable<ObservableValue> values && values.Any())
//            {
//                var obValue = values.FirstOrDefault();
//                if (obValue != null)
//                {
//                    obValue.Value = value;
//                }
//            }
//        }
//    }
//}
