using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using ThomsonReuters.Desktop.SDK.DataAccess.TimeSeries;
using ThomsonReuters.Desktop.SDK.DataAccess.TimeSeries.Metadata;

namespace DailyIntervalDemo

{
        [DataContract]
        public class BarRecord
        {

            [DataMember]
            public double cl { get; set; } // CLOSE

            [DataMember]
            public double op { get; set; }  // OPEN

            [DataMember]
            public double hi { get; set; }  // HIGH

            [DataMember]
            public double lo { get; set; } // LOW

            [DataMember]
            public double vol { get; set; } // VOLUME

            [DataMember]
            public string ts { get; set; }  // TimeStamp

        public BarRecord(IBarData bardata)
            {            
                cl = (double)bardata.Close;
                op = (double)bardata.Open;
                hi = (double)bardata.High;
                lo = (double)bardata.Low;
                vol = (double)bardata.Volume; 
                DateTime dt_ts = (DateTime)bardata.Timestamp;
                ts = dt_ts.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            }

   
        }

    [DataContract]
    public class NavRecord
    {

        [DataMember]
        public double nav { get; set; }

        [DataMember]
        public string ts { get; set; }

        public NavRecord(IData idata, string view_type)
        {
            // NAV - VALUE
            // BID - CLOSE
            // TDRPRC_1 - CLOSE

            if (view_type == "NAV")
            {
                nav = (double)idata["VALUE"];
            }
            else if (view_type == "BID")
            {
                nav = (double)idata["CLOSE"];
            }
            else if (view_type == "TDRPRC_1")
            {
                nav = (double)idata["CLOSE"];
            }

            DateTime dt_ts = (DateTime)idata.Timestamp;
            ts = dt_ts.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        }


    }


    [DataContract]
    public class MetaDataRecord
    {

        [DataMember]
        public int bond_type { get; set; }

        [DataMember]
        public string currency { get; set; }

        [DataMember]
        public string exchange { get; set; }

        [DataMember]
        public string expiry { get; set; }

        [DataMember]
        public int instrument_type { get; set; }

        [DataMember]
        public int language_id { get; set; }

        [DataMember]
        public string language_name { get; set; }

        [DataMember]
        public string long_name { get; set; }

        [DataMember]
        public int market_sector { get; set; }

        [DataMember]
        public string maturity { get; set; }

        [DataMember]
        public string notes { get; set; }

        [DataMember]
        public int rec_type { get; set; }

        [DataMember]
        public string ric_name { get; set; }

        [DataMember]
        public string timezone { get; set; }

        public MetaDataRecord(CommonMetadata mdata)
        {            
            bond_type = mdata.BondType;
            currency = mdata.Currency;
            exchange = mdata.Exchange;
            expiry = mdata.Expiry.ToString("s");
            instrument_type = (int)mdata.InstrumentType;
            language_id = mdata.LanguageId;
            language_name = mdata.LanguageName;
            long_name = mdata.LongName;
            market_sector = (int)mdata.MarketSector;
            maturity = mdata.Maturity.ToString("s");
            notes = mdata.Notes;
            rec_type = mdata.RecType;
            ric_name = mdata.RicName;
            timezone = mdata.TimeZone;
        }


    }

}