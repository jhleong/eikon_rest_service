using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;using ThomsonReuters.Desktop.SDK.DataAccess.TimeSeries;

namespace DailyIntervalDemo
{
        [DataContract]
        public class BarRecord
        {

            [DataMember]
            public double close { get; set; }

            [DataMember]
            public double open { get; set; }

            [DataMember]
            public double high { get; set; }

            [DataMember]
            public double low { get; set; }

            [DataMember]
            public DateTime timestamp { get; set; }

        public BarRecord(IBarData bardata)
            {
                close = (double)bardata.Close;
                open = (double)bardata.Open;
                high = (double)bardata.High;
                low = (double)bardata.Low;
                timestamp = (DateTime)bardata.Timestamp;
            }

   
    }
}