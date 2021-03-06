﻿// <copyright file="MainControl.xaml.cs" company="Thomson Reuters">
// Copyright (c) 2014 All Right Reserved
// </copyright>
// <author>$Author$</author>
// <date>$LastChangedDate$</date>

using System.Runtime.Serialization;

namespace DailyIntervalDemo
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using ThomsonReuters.Desktop.SDK.DataAccess;
    using ThomsonReuters.Desktop.SDK.DataAccess.TimeSeries;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.ServiceModel.Description;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Runtime.Serialization;
    using ThomsonReuters.Desktop.SDK.DataAccess.TimeSeries.Metadata;
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        Task<Collection<BarRecord>> getRT_bar_data(string api_key, string par_ricname, string par_interval, string par_from_date, string par_to_date);
        [OperationContract]
        Task<Collection<NavRecord>> getRT_nav_data(string api_key, string par_ricname, string par_type, string par_interval, string par_from_date, string par_to_date);
        [OperationContract]
        Task<MetaDataRecord> getRT_meta_data(string api_key, string par_ricname);

    }

    public class Service1 : IService1
    {
        private static string REST_API_KEY = "1234";

        // public ITimeSeriesDataSubscription dataSubscription { get; set; }
        private ITimeSeriesDataRequest dataRequest { get; set; }
        private IMetadataRequest metadataRequest;

        //public string Ric { get; set; }
        //public static ObservableCollection<IBarData> Records = new ObservableCollection<IBarData>();
        private Collection<BarRecord> barRecCollection = new Collection<BarRecord>();
        private Collection<NavRecord> navRecCollection = new Collection<NavRecord>();

        private MetaDataRecord metaDataRec;

        private bool dataReqCompleted;
        private bool metaDataReqCompleted;
        private bool dataReqError;
        private bool metaDataReqError;
        private string view_type;
        private bool no_such_view;
        //private readonly AutoResetEvent _signal = new AutoResetEvent(false);



        // rt_mkt_data/s={par_ricname}&a={par_from_month}&b={par_from_day}&c={par_from_year}&d={par_to_month}&e={par_to_day}&f={par_to_year}&g={par_interval}&h={par_type}

        // rt_mkt_data/{par_ricname}/{par_type}/{par_interval}/{par_from_date}/{par_to_date}
        //      par_type : e - equity, f - fund, b - bond
        //      par_interval : d - daily, w - weekly, m - monthly, q - quarterly, y - yearly
        //      par_from_date : YYYYMMDD
        //      par_to_date : YYYYMMDD

        [WebInvoke(Method = "GET",
                    ResponseFormat = WebMessageFormat.Json,
                    UriTemplate = "rt_bar_data/{api_key}/{par_ricname}/{par_interval}/{par_from_date}/{par_to_date}")]
        public async Task<Collection<BarRecord>> getRT_bar_data(string api_key, string par_ricname, string par_interval, string par_from_date, string par_to_date)
        {

            if (api_key != REST_API_KEY)
                return null;

            dataReqCompleted = false;
            dataReqError = false;
            barRecCollection.Clear();
            navRecCollection.Clear();

            int cnt = 0;

            get_data_eikon_bar(par_ricname.ToUpper(), par_interval[0], par_from_date, par_to_date);

            await Task.Run(() =>
            {
                while (!dataReqError && !dataReqCompleted && cnt++ <= 30)
                {
                    Thread.Sleep(100);
                }
            });

            return barRecCollection;

        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "rt_nav_data/{api_key}/{par_ricname}/{par_type}/{par_interval}/{par_from_date}/{par_to_date}")]
        public async Task<Collection<NavRecord>> getRT_nav_data(string api_key, string par_ricname, string par_type, string par_interval, string par_from_date, string par_to_date)
        {
            if (api_key != REST_API_KEY)
                return null;

            barRecCollection.Clear();
            navRecCollection.Clear();


            /* try NAV view type */
            dataReqCompleted = false;
            dataReqError = false;
            int cnt = 0;
            no_such_view = false;

            this.view_type = "NAV";
            get_data_eikon_nav(par_ricname.ToUpper(), "NAV", par_interval[0], par_from_date, par_to_date);

            await Task.Run(() =>
            {
                while (!no_such_view && !dataReqError && !dataReqCompleted && cnt++ <= 30)
                {
                    Thread.Sleep(100);
                }
            });

            if(no_such_view)
            {
                /* try BID view type */
                no_such_view = false;
                dataReqCompleted = false;
                dataReqError = false;
                cnt = 0;

                this.view_type = "BID";
                get_data_eikon_nav(par_ricname.ToUpper(), "BID", par_interval[0], par_from_date, par_to_date);

                await Task.Run(() =>
                {
                    while (!no_such_view && !dataReqError && !dataReqCompleted && cnt++ <= 30)
                    {
                        Thread.Sleep(100);
                    }
                });

            }

            if (no_such_view)
            {
                /* try TRDPRC_1 view type */
                no_such_view = false;
                dataReqCompleted = false;
                dataReqError = false;
                cnt = 0;

                this.view_type = "TRDPRC_1";
                get_data_eikon_nav(par_ricname.ToUpper(), "TRDPRC_1", par_interval[0], par_from_date, par_to_date);

                await Task.Run(() =>
                {
                    while (!no_such_view && !dataReqError && !dataReqCompleted && cnt++ <= 30)
                    {
                        Thread.Sleep(100);
                    }
                });

            }

            return navRecCollection;

        }

        private void SubscriptionStatusChanged(IRequestStatus status)
        {


            if (status.Error > 0)
                dataReqError = true;

            if(status.Error == RequestErrorType.NoSuchView)
            {
                no_such_view = true;
            }
            //switch (status.Error)
            //{

            //    case RequestErrorType.NoSuchRic:
            //        // The specified RIC is not valid. 
            //        break;
            //    case RequestErrorType.NoSuchView:
            //        // The specified RIC is not valid for this view or the view does not exist. 
            //        break;

            //    default:
            //    case RequestErrorType.None:
            //        // RequestErrorType.None indicates no RequestErrorType occurs. 
            //        break;

            //}

            //switch (status.State)
            //{
            //    case RequestState.Blending:
            //        // Request is switching from historical data to realtime continuation. 
            //        break;

            //    case RequestState.Live:
            //        // Request is alive and may send updates. 
            //        break;
            //}
        }

        private void get_data_eikon_bar(string str_ricname, char chr_interval, string par_from_date, string par_to_date)
        {


            string format = "yyyyMMdd";
            DateTime dt_from = DateTime.ParseExact(par_from_date, format, System.Globalization.CultureInfo.InvariantCulture);
            DateTime dt_to = DateTime.ParseExact(par_to_date, format, System.Globalization.CultureInfo.InvariantCulture);

            CommonInterval interval = CommonInterval.Daily;

            if (chr_interval == 'd')
                interval = CommonInterval.Daily;
            else if (chr_interval == 'w')
                interval = CommonInterval.Weekly;
            else if (chr_interval == 'm')
                interval = CommonInterval.Monthly;
            else if (chr_interval == 'q')
                interval = CommonInterval.Quarterly;
            else if (chr_interval == 'y')
                interval = CommonInterval.Yearly;

            /* select the view depend on the type of security
            equity or etf - TRCPRC_1
            fund - NAV, TRCPRC_1
            bond - BID, DAILY_RETURN
            */
            //default
            this.view_type = "TRDPRC_1";           

            dataRequest = DataServices.Instance.TimeSeries.SetupDataRequest(str_ricname)
                    .From(dt_from).To(dt_to)
                    .WithView(this.view_type)
                    .WithInterval(interval)
                    .OnDataReceived(this.OnRestSubscriptionBarDataReceived)
                    .OnStatusUpdated(this.SubscriptionStatusChanged)
                    .CreateAndSend();

        }

        private void get_data_eikon_nav(string str_ricname, string v_view_type, char chr_interval, string par_from_date, string par_to_date)
        {


            string format = "yyyyMMdd";
            DateTime dt_from = DateTime.ParseExact(par_from_date, format, System.Globalization.CultureInfo.InvariantCulture);
            DateTime dt_to = DateTime.ParseExact(par_to_date, format, System.Globalization.CultureInfo.InvariantCulture);

            CommonInterval interval = CommonInterval.Daily;

            if (chr_interval == 'd')
                interval = CommonInterval.Daily;
            else if (chr_interval == 'w')
                interval = CommonInterval.Weekly;
            else if (chr_interval == 'm')
                interval = CommonInterval.Monthly;
            else if (chr_interval == 'q')
                interval = CommonInterval.Quarterly;
            else if (chr_interval == 'y')
                interval = CommonInterval.Yearly;

            /* select the view depend on the type of security
            equity or etf - TRCPRC_1
            fund - NAV, TRCPRC_1
            bond - BID, DAILY_RETURN
            */
            //default
       /*     this.view_type = "TRDPRC_1";

            if (chr_type == 'e')
                this.view_type = "TRDPRC_1";
            else if (chr_type == 'f')
                this.view_type = "NAV";
            else if (chr_type == 'b')
                this.view_type = "BID"; */

            /* try NAV, BID then TRDPRC_1 */

            dataRequest = DataServices.Instance.TimeSeries.SetupDataRequest(str_ricname)
                    .From(dt_from).To(dt_to)
                    .WithView(v_view_type)
                    .WithInterval(interval)
                    .OnDataReceived(this.OnRestSubscriptionNavDataReceived)
                    .OnStatusUpdated(this.SubscriptionStatusChanged)
                    .CreateAndSend();

        }

        private void OnRestSubscriptionBarDataReceived(DataChunk chunk)
        {

            // chunk.Records provides access to a dictionary of field -> value, so that
            // developpers can access values from field names and cast them in the proper type like in:
            //                  double open = (double) chunk.Records.First()["OPEN"];

            // In addition, for convenience Time Series API exposes shortcuts that allow developers 
            // to code against strongly typed accessors. This is done through conversion methods
            // such as ToBarRecords(), ToQuoteRecords(), ToTickRecords(), ToTradeRecords() and ToTradeAndQuoteRecords().
            // The full dictionary is still exposed in the resulting strongly-typed classes.

            // In this sample we know that records are typical "bar" records (open, close, high, low, volume) 
            // because we ask for a daily interval. So we can use ToBarRecords() helper method:

            if (this.view_type == "TRDPRC_1")
            {
                // BarData from TDRPRC_1 view
                foreach (IBarData record in chunk.Records.ToBarRecords())
                {
                    try
                    {
                        barRecCollection.Insert(0, new BarRecord(record));
                    }
                    catch
                    {

                    }
                }
            }

            if (chunk.IsLast)
            {
                dataReqCompleted = true;
            }
        }

        private void OnRestSubscriptionNavDataReceived(DataChunk chunk)
        {

            // chunk.Records provides access to a dictionary of field -> value, so that
            // developpers can access values from field names and cast them in the proper type like in:
            //                  double open = (double) chunk.Records.First()["OPEN"];

            // In addition, for convenience Time Series API exposes shortcuts that allow developers 
            // to code against strongly typed accessors. This is done through conversion methods
            // such as ToBarRecords(), ToQuoteRecords(), ToTickRecords(), ToTradeRecords() and ToTradeAndQuoteRecords().
            // The full dictionary is still exposed in the resulting strongly-typed classes.

            // In this sample we know that records are typical "bar" records (open, close, high, low, volume) 
            // because we ask for a daily interval. So we can use ToBarRecords() helper method:


            // NAV - VALUE
            // BID - CLOSE
            // TDRPRC_1 - CLOSE
            foreach (IBarData record in chunk.Records.ToBarRecords())
            {
                try
                {
                    navRecCollection.Insert(0, new NavRecord(record, this.view_type));
                }
                catch
                {

                }
            }
         

            if (chunk.IsLast)
            {
                dataReqCompleted = true;
            }
        }



        [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "rt_meta_data/{api_key}/{par_ricname}")]
        public async Task<MetaDataRecord> getRT_meta_data(string api_key, string par_ricname)
        {
            if (api_key != REST_API_KEY)
                return null;

            metaDataReqCompleted = false;
            metaDataReqError = false;
            int metaDataTimeOut = 0;

            metadataRequest = DataServices.Instance.TimeSeries.Metadata.GetCommonData(par_ricname, onRetCommonData, OnErrorCommonData);

            await Task.Run(() =>
            {
                while (!metaDataReqError && !metaDataReqCompleted && metaDataTimeOut++ <= 30)
                {
                    Thread.Sleep(100);
                }
            });

            return metaDataRec;

        }

        private void onRetCommonData(CommonMetadata mdata)
        {
            metaDataRec = new MetaDataRecord(mdata);
            metaDataReqCompleted = true;
        }

        private void OnErrorCommonData(TimeSeriesError error)
        {
            metaDataReqError = true;
        }
    }



    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl, INotifyPropertyChanged
    {
        private ITimeSeriesDataSubscription dataSubscription;
        public static ITimeSeriesDataRequest dataRequest { get; set; }
        private IMetadataRequest metadataRequest;

        private ServiceHost host;

        public MainControl()
        {
            if (!this.IsInDesignMode())
            {
                this.Ric = "EUR=";
                this.Records = new ObservableCollection<IBarData>();
                this.IsLoading = true;
                DataServices.Instance.Initialize("DailyIntervalDemo");
                this.IsLoading = false;

                string baseAddress = "http://" + Environment.MachineName + ":8877/eikon";
                host = new ServiceHost(typeof(Service1), new Uri(baseAddress));
                host.AddServiceEndpoint(typeof(IService1), new WebHttpBinding(), "").Behaviors.Add(new WebHttpBehavior());
                host.Open();
            }

            this.InitializeComponent();
        }

        private void RunButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Ensure call .Dispose() on no more used subscriptions
            if (this.dataSubscription != null)
            {
                this.dataSubscription.Dispose();
            }

            this.Records.Clear();
            this.requestTime = DateTime.Now;
            this.HistoricalDataCount = 0;
            this.IsLoading = true;

            GetCommonMeta(this.Ric);
            //GetCurrency(this.Ric);
            //GetHolidaysForRIC();

            /*
            this.dataSubscription = DataServices.Instance.TimeSeries.SetupDataSubscription(this.Ric)
                                        .WithInterval(CommonInterval.Daily)
                                        .OnDataReceived(this.OnSubscriptionDataReceived)
                                        .OnDataUpdated(this.OnSubscriptionDataUpdated)
                                        .OnStatusUpdated(status => this.Title = string.Format("Status: {0}         Error: {1}", status.State, status.Error))
                                        .CreateAndStart();
                                        */

            dataRequest = DataServices.Instance.TimeSeries.SetupDataRequest(this.Ric)
                               .From(DateTime.Today.AddYears(-1))
                               .WithInterval(CommonInterval.Daily)
                               .OnDataReceived(this.OnSubscriptionDataReceived)
                               .WithView("BID")
                               .CreateAndSend();
        }

        private void OnSubscriptionDataReceived(DataChunk chunk)
        {
            this.IsLoading = false;
            this.ResponseTime = (DateTime.Now - this.requestTime);
            this.HistoricalDataCount += chunk.Records.Count();
            // chunk.Records provides access to a dictionary of field -> value, so that
            // developpers can access values from field names and cast them in the proper type like in:
            //                  double open = (double) chunk.Records.First()["OPEN"];

            // In addition, for convenience Time Series API exposes shortcuts that allow developers 
            // to code against strongly typed accessors. This is done through conversion methods
            // such as ToBarRecords(), ToQuoteRecords(), ToTickRecords(), ToTradeRecords() and ToTradeAndQuoteRecords().
            // The full dictionary is still exposed in the resulting strongly-typed classes.

            // In this sample we know that records are typical "bar" records (open, close, high, low, volume) 
            // because we ask for a daily interval. So we can use ToBarRecords() helper method:
            foreach (IBarData record in chunk.Records.ToBarRecords())
            {
                this.Records.Insert(0, record);
            }
        }

        private void OnSubscriptionDataUpdated(IDataUpdate update)
        {
            // In this sample for clarity we assume that the realtime update contains at most a single point.
            // Robust real-world apps should iterate on update.Records.
            IData record = update.Records.FirstOrDefault();

            if (record != null)
            {
                if (update.UpdateType == UpdateType.ExistingPoint && this.Records.Count > 0)
                {
                    // In this inter-day context, receiving an update of type "ExistingPoint" means that
                    // the last received record, ie the record of the current day, is to be overriden.
                    // Here we simply remove / add the record.
                    this.Records.RemoveAt(0);
                }

                this.Records.Insert(0, record.ToBarRecord());
            }
        }

        private void GetCommonMeta(string ric)
        {
            // For the sake of conciseness both data and error callbacks are specified as lambda expressions.
            this.metadataRequest = DataServices.Instance.TimeSeries.Metadata.GetCommonData(ric, metaDatas =>
            {
                var meta = metaDatas;
            }, error =>
            {
                RequestErrorType errorType = error.Type;
                string errorMessage = error.Message;
                MessageBox.Show(errorMessage, errorType.ToString());
            });
        }


        // Data services consume native resources that must be disposed as soon as possible.
        // So, it's a recommended practice to dispose data request and susbcription
        // in a deterministic way, based on any kind of events indicating end of application.
        internal void CleanUp()
        {
            if (this.dataSubscription != null)
            {
                this.dataSubscription.Dispose();
            }

            host.Close();

        }

        #region UI logic

        private int historicalDataCount;
        private bool isLoading;
        private DateTime requestTime;
        private TimeSpan responseTime;
        private string title;

        public string Title
        {
            get { return this.title; }
            set
            {
                this.title = value;
                this.OnPropertyChanged("Title");
            }
        }

        public string Ric { get; set; }
        public ObservableCollection<IBarData> Records { get; private set; }

        public int HistoricalDataCount
        {
            get { return this.historicalDataCount; }
            set
            {
                this.historicalDataCount = value;
                this.OnPropertyChanged("HistoricalDataCount");
            }
        }

        public TimeSpan ResponseTime
        {
            get { return this.responseTime; }
            set
            {
                this.responseTime = value;
                this.OnPropertyChanged("ResponseTime");
            }
        }

        public bool IsLoading
        {
            get { return this.isLoading; }
            set
            {
                this.isLoading = value;
                this.OnPropertyChanged("IsLoading");
                this.OnPropertyChanged("NotIsLoading");
            }
        }

        public bool NotIsLoading
        {
            get { return !this.IsLoading; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RicKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                this.RunButton_OnClick(sender, e);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}

