// <copyright file="MainControl.xaml.cs" company="Thomson Reuters">
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

    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        Task<Collection<BarRecord>> GetData(string id);
    }

    public class Service1 : IService1
    {
        public ITimeSeriesDataSubscription dataSubscription { get; set; }
       // public static ITimeSeriesDataRequest dataSubscription { get; set; }
        public string Ric { get; set; }
        public static ObservableCollection<IBarData> Records = new ObservableCollection<IBarData>();
        public static Collection<BarRecord> barRecCollection = new Collection<BarRecord>();
        private static bool dataCompleted;
        //private readonly AutoResetEvent _signal = new AutoResetEvent(false);

        [WebInvoke(Method = "GET",
                    ResponseFormat = WebMessageFormat.Json,
                    UriTemplate = "data/{id}")]
        public async Task<Collection<BarRecord>> GetData(string id)
        {
            this.Ric = id;
            dataCompleted = false;
            Records.Clear();
            barRecCollection.Clear();

            dataSubscription = DataServices.Instance.TimeSeries.SetupDataSubscription(this.Ric)
                                        .WithInterval(CommonInterval.Daily)
                                        .OnDataReceived(this.OnSubscriptionDataReceived2)
                                        .OnDataUpdated(this.OnSubscriptionDataUpdated2)
                                        .CreateAndStart();

            /*  Task waitEikonDataTaskAsync =  waitEikonDataAsync();

              await waitEikonDataTaskAsync(); */

            // await waitEikonDataAsync();

            await Task.Run(() =>
            {
                while (!dataCompleted)
                {
                    Thread.Sleep(100);
                }
            });


            //new Thread(() =>
            //{
            //    Thread.CurrentThread.IsBackground = true;
            //    /* run your code here */
            //    dataSubscription = DataServices.Instance.TimeSeries.SetupDataRequest(this.Ric)
            //                    .From(DateTime.Today.AddYears(-5))
            //                    .WithInterval(CommonInterval.Monthly)
            //                    .OnDataReceived(OnSubscriptionDataReceived2)
            //                    .CreateAndSend();
            //}).Start();


            //System.Threading.Thread.Sleep(10000);
            //  _signal.WaitOne();

            return barRecCollection;
            // lookup person with the requested id 
            /* return new Person()
             {
                 Id = Convert.ToInt32(id),
                 Name = "Leo Messi"
             };*/
        }


        /*
    private  Task waitEikonDataAsync()
        {


            return Task.Factory.StartNew(() =>
            {
                while (!dataCompleted)
                {
                    Thread.Sleep(100);
                }
            }
            );


            
        } */

    private void OnSubscriptionDataReceived2(DataChunk chunk)
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
            foreach (IBarData record in chunk.Records.ToBarRecords())
            {
                Records.Insert(0, record);
                barRecCollection.Insert(0, new BarRecord(record));
            }

            if (chunk.IsLast)
            {
                dataCompleted = true;
            }
        }

    private void OnSubscriptionDataUpdated2(IDataUpdate update)
    {
            // In this sample for clarity we assume that the realtime update contains at most a single point.
            // Robust real-world apps should iterate on update.Records.
            IData record = update.Records.FirstOrDefault();

            if (record != null)
            {
                if (update.UpdateType == UpdateType.ExistingPoint && Records.Count > 0)
                {
                    // In this inter-day context, receiving an update of type "ExistingPoint" means that
                    // the last received record, ie the record of the current day, is to be overriden.
                    // Here we simply remove / add the record.
                    Records.RemoveAt(0);
                    barRecCollection.RemoveAt(0);
                    dataCompleted = true;
                  //  _signal.Set();
                }

                Records.Insert(0, record.ToBarRecord());
                barRecCollection.Insert(0, new BarRecord(record.ToBarRecord()));
            }
        }
}



    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl, INotifyPropertyChanged
    {
        private ITimeSeriesDataSubscription dataSubscription;
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

                string baseAddress = "http://" + Environment.MachineName + ":8733/service1";
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

            this.dataSubscription = DataServices.Instance.TimeSeries.SetupDataSubscription(this.Ric)
                                        .WithInterval(CommonInterval.Daily)
                                        .OnDataReceived(this.OnSubscriptionDataReceived)
                                        .OnDataUpdated(this.OnSubscriptionDataUpdated)
                                        .OnStatusUpdated(status => this.Title = string.Format("Status: {0}         Error: {1}", status.State, status.Error))
                                        .CreateAndStart();
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

