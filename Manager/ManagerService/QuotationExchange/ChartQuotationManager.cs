using iExchange.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace ManagerService.QuotationExchange
{
    #region ChartQuotation

    internal enum ChartType
    {
        Minute1 = 1,
        Minute5 = 5,
        Minute15 = 15,
        Minute30 = 30,
        Hour1 = 60,
        Hour2 = 120,
        Hour3 = 180,
        Hour4 = 240,
        Hour5 = 300,
        Hour6 = 360,
        Hour7 = 420,
        Hour8 = 480,
    }


    internal class ChartQuotation
    {
        private ChartType _ChartType;
        private DateTime _DateTime;
        private DateTime _NextChartDateTime;
        private Guid _InstrumentId;
        private Guid _QuotePolicyId;

        private decimal _Open;
        private decimal _Close;
        private decimal _High;
        private decimal _Low = decimal.MaxValue;
        private double _Volume;

        private string _Key;

        #region Properties

        internal ChartType ChartType
        {
            get { return _ChartType; }
        }

        internal DateTime DateTime
        {
            get { return this._DateTime; }
            private set
            {
                this._DateTime = value;
                this._NextChartDateTime = this._DateTime.AddMinutes((int)this._ChartType);
            }
        }

        internal DateTime NextChartDateTime
        {
            get { return this._NextChartDateTime; }
        }

        internal Guid InstrumentId
        {
            get { return this._InstrumentId; }
        }

        internal Guid QuotePolicyId
        {
            get { return this._QuotePolicyId; }
        }

        internal decimal Open
        {
            get { return this._Open; }
            set { this._Open = value; }
        }

        internal decimal Close
        {
            get { return this._Close; }
            set { this._Close = value; }
        }

        internal decimal High
        {
            get { return this._High; }
            set { this._High = value; }
        }

        internal decimal Low
        {
            get { return this._Low; }
            set { this._Low = value; }
        }

        internal double Volume
        {
            get { return this._Volume; }
            set { this._Volume = value; }
        }

        internal string Key
        {
            get
            {
                if (string.IsNullOrEmpty(this._Key))
                {
                    this._Key = string.Format("{0}{1}{2}{3:yyyy-MM-dd HH:mm:ss}", this._ChartType, this._InstrumentId, this._QuotePolicyId, this._DateTime);
                }
                return this._Key;
            }
        }
        #endregion
        private ChartQuotation() { }
        internal ChartQuotation(ChartType chartType, DateTime dateTime, Guid instrumentId, Guid quotePolicyId, decimal open, decimal close, decimal high, decimal low, double volume)
        {
            this._ChartType = chartType;
            this.DateTime = dateTime;
            this._InstrumentId = instrumentId;
            this._QuotePolicyId = quotePolicyId;

            this._Open = open;
            this._Close = close;
            this._High = high;
            this._Low = low;
            this._Volume = volume;
        }

        internal static ChartQuotation Create(ChartType chartType, DataRow row)
        {
            return new ChartQuotation(chartType, (DateTime)row["Date"], (Guid)row["InstrumentId"], (Guid)row["QuotePolicyId"],
                (decimal)row["Open"], (decimal)row["Close"], (decimal)row["High"], (decimal)row["Low"],
                row["Volume"] == DBNull.Value ? 0 : (double)row["Volume"]);
        }
    }
    #endregion


    internal class ChartQuotationManager
    {
        private static ChartType[] _AllChartTypes = new ChartType[]{
												   ChartType.Minute1,
												   ChartType.Minute5,
                                                   ChartType.Minute15,
                                                   ChartType.Minute30,
                                                   ChartType.Hour1,
												   ChartType.Hour2,
												   ChartType.Hour3,
												   ChartType.Hour4,
												   ChartType.Hour5,
                                                   ChartType.Hour6,
                                                   ChartType.Hour7,
                                                   ChartType.Hour8
			    };
        private List<ChartType> _PendingInitializeChartTypes = new List<ChartType>();
        private Dictionary<Guid, Dictionary<Guid, Dictionary<ChartType, ChartQuotation>>> _InstrumentChartQuotations = new Dictionary<Guid, Dictionary<Guid, Dictionary<ChartType, ChartQuotation>>>();    //key:instrumentid, quotepolicy, ChartType
        private Dictionary<string, ChartQuotation> _PendingSaveChartQuotations = new Dictionary<string, ChartQuotation>();
        private string _QuotationConnectionString;
        private object _Lock = new object();

        private bool _IsReady = false;              // actually, is a enum of 3 state: NeedInitialize, Initializng, Initialized

        private Thread _Thread;                     //keep running until IIS abort it
        private TimeSpan _SaveFrequency;
        private bool _isSuspended = true;

        internal bool HighBid { get; set; }
        internal bool LowBid { get; set; }
        private string _ChartCommandTimeOut;
        private List<iExchange.Common.OverridedQuotation> _QuotationsBeforeInitialized;
        private static int _QuotationsBeforeInitializedCapacity = 1000;

        internal ChartQuotationManager(string quotationConnectionString, TimeSpan saveFrequency, bool highBid, bool lowBid,string chartCommandTimeOut)
        {
            this._QuotationConnectionString = quotationConnectionString;
            this._SaveFrequency = saveFrequency;
            this.HighBid = highBid;
            this.LowBid = lowBid;
            this._ChartCommandTimeOut = chartCommandTimeOut;
            this._QuotationsBeforeInitialized = new List<iExchange.Common.OverridedQuotation>(ChartQuotationManager._QuotationsBeforeInitializedCapacity);
            this._PendingInitializeChartTypes.AddRange(ChartQuotationManager._AllChartTypes);
        }

        /// <summary>
        /// Data initialized
        /// </summary>
        internal bool IsReady
        {
            get { return this._IsReady; }
            set { this._IsReady = value; }
        }

        private void InternalStart()
        {
            if (this._Thread != null)
            {
                return;
            }

            this._Thread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    if (this._PendingInitializeChartTypes.Count > 0)
                    {
                        while (this._PendingInitializeChartTypes.Count > 0)
                        {
                            try
                            {
                                ChartType chartType = this._PendingInitializeChartTypes[0];
                                DataSet dataSet = this.GetChartData(chartType);

                                foreach (DataRow row in dataSet.Tables[0].Rows)
                                {
                                    ChartQuotation chartQuotation = ChartQuotation.Create(chartType, row);
                                    if (!this._InstrumentChartQuotations.ContainsKey(chartQuotation.InstrumentId))
                                    {
                                        this._InstrumentChartQuotations.Add(chartQuotation.InstrumentId, new Dictionary<Guid, Dictionary<ChartType, ChartQuotation>>());
                                    }
                                    Dictionary<Guid, Dictionary<ChartType, ChartQuotation>> quotePolicyChartQuotations = this._InstrumentChartQuotations[chartQuotation.InstrumentId];
                                    if (!quotePolicyChartQuotations.ContainsKey(chartQuotation.QuotePolicyId))
                                    {
                                        quotePolicyChartQuotations.Add(chartQuotation.QuotePolicyId, new Dictionary<ChartType, ChartQuotation>());
                                    }
                                    Dictionary<ChartType, ChartQuotation> chartQuotations = quotePolicyChartQuotations[chartQuotation.QuotePolicyId];
                                    if (!chartQuotations.ContainsKey(chartType))
                                    {
                                        chartQuotations.Add(chartType, chartQuotation);
                                    }
                                    else
                                    {
                                        //throw new Exception("InitializeInternal error!");
                                        chartQuotations[chartType] = chartQuotation;
                                    }
                                }
                                this._PendingInitializeChartTypes.RemoveAt(0);
                            }
                            catch (Exception exception)
                            {
                                AppDebug.LogEvent("QuotationServer", string.Format("ChartQuotationManager.InternalStart error: {0}", exception), EventLogEntryType.Warning);
                            }
                            Thread.Sleep(this._SaveFrequency);
                        }
                        this.IsReady = true;    //start initializing
                        lock (this._Lock)
                        {
                            this.Add(this._QuotationsBeforeInitialized);
                            this._QuotationsBeforeInitialized.Clear();
                        }
                    }
                    else
                    {
                        if (!this._isSuspended)
                        {
                            this.SaveToDatabase();
                        }
                    }
                    Thread.Sleep(this._SaveFrequency);
                }
            }));
            this._Thread.IsBackground = true;
            this._Thread.Start();
        }

        internal void Suspend()
        {
            lock (this._Lock)
            {
                this._isSuspended = true;
            }
        }

        internal void Start()
        {
            lock (this._Lock)
            {
                this._isSuspended = false;
                this.InternalStart();
            }
        }

        internal void UpdateHighLow(Guid instrumentId, DataTable dataTable, bool isUpdateHigh)
        {
            lock (this._Lock)
            {
                if (this.IsReady && this._InstrumentChartQuotations.ContainsKey(instrumentId))
                {
                    Dictionary<Guid, Dictionary<ChartType, ChartQuotation>> quotePolicyChartQuotations = this._InstrumentChartQuotations[instrumentId];
                    foreach (DataRow row in dataTable.Rows)
                    {
                        Guid quotePolicyId = (Guid)row["QuotePolicyId"];
                        string ask = (string)row["Ask"];
                        string bid = (string)row["Bid"];
                        decimal price = (this.HighBid && this.LowBid ? decimal.Parse(bid) : (!this.HighBid && !this.LowBid ? decimal.Parse(ask) : (decimal.Parse(ask) + decimal.Parse(bid)) / 2));

                        if (quotePolicyChartQuotations.ContainsKey(quotePolicyId))
                        {
                            Dictionary<ChartType, ChartQuotation> chartQuotations = quotePolicyChartQuotations[quotePolicyId];
                            foreach (ChartQuotation chartQuotation in chartQuotations.Values)
                            {
                                if (isUpdateHigh)
                                {
                                    chartQuotation.Open = (chartQuotation.Open > price ? price : chartQuotation.Open);
                                    chartQuotation.Close = (chartQuotation.Close > price ? price : chartQuotation.Close);
                                    chartQuotation.High = (chartQuotation.High > price ? price : chartQuotation.High);
                                    chartQuotation.Low = (chartQuotation.Low > price ? price : chartQuotation.Low);
                                }
                                else
                                {
                                    chartQuotation.Open = (chartQuotation.Open < price ? price : chartQuotation.Open);
                                    chartQuotation.Close = (chartQuotation.Close < price ? price : chartQuotation.Close);
                                    chartQuotation.High = (chartQuotation.High < price ? price : chartQuotation.High);
                                    chartQuotation.Low = (chartQuotation.Low < price ? price : chartQuotation.Low);
                                }
                            }
                        }

                        foreach (ChartQuotation chartQuotation in this._PendingSaveChartQuotations.Values)
                        {
                            if (chartQuotation.QuotePolicyId == quotePolicyId)
                            {
                                if (isUpdateHigh)
                                {
                                    chartQuotation.Open = (chartQuotation.Open > price ? price : chartQuotation.Open);
                                    chartQuotation.Close = (chartQuotation.Close > price ? price : chartQuotation.Close);
                                    chartQuotation.High = (chartQuotation.High > price ? price : chartQuotation.High);
                                    chartQuotation.Low = (chartQuotation.Low > price ? price : chartQuotation.Low);
                                }
                                else
                                {
                                    chartQuotation.Open = (chartQuotation.Open < price ? price : chartQuotation.Open);
                                    chartQuotation.Close = (chartQuotation.Close < price ? price : chartQuotation.Close);
                                    chartQuotation.High = (chartQuotation.High < price ? price : chartQuotation.High);
                                    chartQuotation.Low = (chartQuotation.Low < price ? price : chartQuotation.Low);
                                }
                            }
                        }
                    }
                }
            }
        }

        private DataSet GetChartData(ChartType chartType)
        {
            using (SqlConnection sqlConnection = new SqlConnection(this._QuotationConnectionString))
            {
                try
                {
                    sqlConnection.Open();
                    //SqlCommand command = sqlConnection.CreateCommand();
                    //command.CommandText = "SELECT TOP 10 * FROM ChartMinutes1 cm";
                    //command.CommandType = CommandType.Text;
                    //int commandTimeout = string.IsNullOrEmpty(this._ChartCommandTimeOut) ? 60 * 90 : (int)TimeSpan.Parse(this._ChartCommandTimeOut).TotalSeconds;
                    //command.CommandTimeout = commandTimeout;     //default 90 minutes
                    //SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    //dataAdapter.SelectCommand = command;
                    //DataSet dataSet = new DataSet();
                    //dataAdapter.Fill(dataSet);
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandText = "dbo.P_GetChartData2ForQuotationServer";
                    command.CommandType = CommandType.StoredProcedure;

                    int commandTimeout = 60;//string.IsNullOrEmpty(this._ChartCommandTimeOut) ? 60 * 90 : (int)TimeSpan.Parse(this._ChartCommandTimeOut).TotalSeconds;
                    command.CommandTimeout = commandTimeout;     //default 90 minutes
                    command.Parameters.Add(new SqlParameter("@chartType", chartType.ToString()));

                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    dataAdapter.SelectCommand = command;
                    DataSet dataSet = new DataSet();
                    dataAdapter.Fill(dataSet);

                    //string[] tableNames = new string[]{
                    //                                   "Minute1",
                    //                                   "Minute5",
                    //                                   "Minute15",
                    //                                   "Minute30",
                    //                                   "Hour1",
                    //                                   "Hour2",
                    //                                   "Hour3",
                    //                                   "Hour4",
                    //                                   "Hour5",
                    //                                   "Hour6",
                    //                                   "Hour7",
                    //                                   "Hour8"
                    //};

                    //if (dataSet.Tables.Count != tableNames.Length)
                    //    throw new ApplicationException("Get initial data failed");

                    //for (int i = 0; i < dataSet.Tables.Count; i++)
                    //{
                    //    dataSet.Tables[i].TableName = tableNames[i];
                    //}

                    return dataSet;
                }
                catch (Exception ex)
                {
                    AppDebug.LogEvent("FillDataSet", string.Format("{0}", ex.ToString()), EventLogEntryType.Error);
                }
                return null;
            }
        }

        private DateTime Round(ChartType chartType, DateTime dateTime)
        {
            if ((int)chartType >= (int)ChartType.Hour1)
            {
                return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour / ((int)chartType / 60) * ((int)chartType / 60), 0, 0, dateTime.Kind);
            }
            else
            {
                return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, dateTime.Kind).AddMinutes((dateTime.Minute / (int)chartType) * (int)chartType);
            }
        }

        internal void Add(ICollection<iExchange.Common.OverridedQuotation> quotations)
        {
            lock (this._Lock)
            {
                if (!this.IsReady)    //if try initialize every time if init failed,  it will block quotationServer
                {
                    this._QuotationsBeforeInitialized.AddRange(quotations);
                }
                else
                {
                    foreach (iExchange.Common.OverridedQuotation quotation in quotations)
                    {
                        this.Add(quotation);
                    }
                }
            }
        }

        private void Add(iExchange.Common.OverridedQuotation quotation)
        {
            lock (this._Lock)
            {
                try
                {
                    decimal price = (this.HighBid && this.LowBid ? decimal.Parse(quotation.Bid) : (!this.HighBid && !this.LowBid ? decimal.Parse(quotation.Ask) : (decimal.Parse(quotation.Ask) + decimal.Parse(quotation.Bid)) / 2));

                    if (!this._InstrumentChartQuotations.ContainsKey(quotation.InstrumentID))
                    {
                        this._InstrumentChartQuotations.Add(quotation.InstrumentID, new Dictionary<Guid, Dictionary<ChartType, ChartQuotation>>());
                    }
                    Dictionary<Guid, Dictionary<ChartType, ChartQuotation>> quotePolicyChartQuotations = this._InstrumentChartQuotations[quotation.InstrumentID];

                    if (!quotePolicyChartQuotations.ContainsKey(quotation.QuotePolicyID))
                    {
                        Dictionary<ChartType, ChartQuotation> chartQuotations = new Dictionary<ChartType, ChartQuotation>();
                        foreach (ChartType chartType in ChartQuotationManager._AllChartTypes)
                        {
                            chartQuotations.Add(chartType, new ChartQuotation(chartType, this.Round(chartType, quotation.Timestamp), quotation.InstrumentID, quotation.QuotePolicyID, price, price, price, price, 0));
                        }
                        quotePolicyChartQuotations.Add(quotation.QuotePolicyID, chartQuotations);
                    }
                    else
                    {
                        Dictionary<ChartType, ChartQuotation> chartQuotations = quotePolicyChartQuotations[quotation.QuotePolicyID];
                        List<ChartQuotation> newChartQuotations = new List<ChartQuotation>();
                        foreach (ChartType chartType in ChartQuotationManager._AllChartTypes)
                        {
                            DateTime chartTime = this.Round(chartType, quotation.Timestamp);
                            if (chartQuotations.ContainsKey(chartType))
                            {
                                ChartQuotation latestChartQuotation = chartQuotations[chartType];

                                if (chartTime == latestChartQuotation.DateTime)
                                {
                                    if (latestChartQuotation.High < price) latestChartQuotation.High = price;
                                    if (latestChartQuotation.Low > price) latestChartQuotation.Low = price;
                                    latestChartQuotation.Close = price;
                                }
                                else if (chartTime > latestChartQuotation.DateTime)
                                {
                                    this._PendingSaveChartQuotations.Add(latestChartQuotation.Key, latestChartQuotation);
                                    chartQuotations[chartType] = new ChartQuotation(chartType, this.Round(chartType, chartTime), quotation.InstrumentID, quotation.QuotePolicyID, price, price, price, price, 0);
                                }
                                else
                                {
                                    AppDebug.LogEvent("QuotationServer", string.Format("ChartQuotationManager.Add error: Timestamp should great or equal than last timestamp. {0}", quotation), EventLogEntryType.Error);
                                    //throw new ArgumentOutOfRangeException("quotation.Timestamp", "Timestamp should great or equal than last timestamp");
                                }
                            }
                            else
                            {
                                chartQuotations.Add(chartType, new ChartQuotation(chartType, this.Round(chartType, chartTime), quotation.InstrumentID, quotation.QuotePolicyID, price, price, price, price, 0));
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    AppDebug.LogEvent("QuotationServer", string.Format("ChartQuotationManager.Add error: {0}", exception), EventLogEntryType.Error);
                }
            }
        }

        //todo: deal with saving failed;
        internal bool SaveToDatabase()
        {
            List<ChartQuotation> pendingSaveChartQuotations = null;
            lock (this._Lock)
            {
                if (this._InstrumentChartQuotations.Count > 0)
                {
                    foreach (Dictionary<Guid, Dictionary<ChartType, ChartQuotation>> quotePolicyChartQuotations in this._InstrumentChartQuotations.Values)
                    {
                        foreach (Dictionary<ChartType, ChartQuotation> chartQuotations in quotePolicyChartQuotations.Values)
                        {
                            List<ChartQuotation> newChartQuotations = new List<ChartQuotation>();
                            foreach (ChartQuotation chartQuotation in chartQuotations.Values)
                            {
                                if (DateTime.Now > chartQuotation.NextChartDateTime)
                                {
                                    this._PendingSaveChartQuotations.Add(chartQuotation.Key, chartQuotation);
                                    newChartQuotations.Add(chartQuotation);
                                }
                            }
                            foreach (ChartQuotation chartQuotation in newChartQuotations)
                            {
                                chartQuotations.Remove(chartQuotation.ChartType);
                            }
                        }
                    }
                }

                if (this._PendingSaveChartQuotations.Count > 0)
                {
                    pendingSaveChartQuotations = new List<ChartQuotation>();
                    pendingSaveChartQuotations.AddRange(this._PendingSaveChartQuotations.Values);
                    this._PendingSaveChartQuotations.Clear();
                }
            }

            if (pendingSaveChartQuotations != null && pendingSaveChartQuotations.Count > 0)
            {
                DataTable dataTable = this.CreateEmptyDataTable();
                foreach (ChartQuotation chartQuotation in pendingSaveChartQuotations)
                {
                    DataRow row = dataTable.NewRow();
                    row["ChartType"] = chartQuotation.ChartType.ToString();
                    row["InstrumentId"] = chartQuotation.InstrumentId;
                    row["QuotePolicyId"] = chartQuotation.QuotePolicyId;
                    row["Date"] = chartQuotation.DateTime;
                    row["Open"] = chartQuotation.Open;
                    row["Close"] = chartQuotation.Close;
                    row["High"] = chartQuotation.High;
                    row["Low"] = chartQuotation.Low;
                    row["Volume"] = chartQuotation.Volume;

                    dataTable.Rows.Add(row);
                }

                try
                {
                    using (SqlConnection sqlConnection = new SqlConnection(this._QuotationConnectionString))
                    {
                        SqlCommand sqlCommand = sqlConnection.CreateCommand();
                        sqlCommand.CommandText = "dbo.P_AddChartData";
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.CommandTimeout = 60 * 30;
                        SqlParameter sqlParameter = sqlCommand.Parameters.AddWithValue("@chartDataTable", dataTable);
                        sqlParameter.SqlDbType = SqlDbType.Structured;

                        sqlParameter = sqlCommand.Parameters.AddWithValue("@result", null);
                        sqlParameter.Direction = ParameterDirection.ReturnValue;
                        sqlConnection.Open();
                        sqlCommand.ExecuteNonQuery();

                        if (sqlParameter.Value != DBNull.Value && (int)sqlParameter.Value == 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception exception)
                {
                    string message = string.Empty;
                    try
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            dataTable.WriteXml(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            XmlDocument xmlDocument = new XmlDocument();
                            xmlDocument.Load(memoryStream);
                            message = xmlDocument.OuterXml;
                        }
                    }
                    catch { }
                    AppDebug.LogEvent("QuotationServer", string.Format("ChartQuotationManager.SaveToDatabase error: {0}{1}", exception, message), EventLogEntryType.Error);
                    return false;
                }
            }
            return true;
        }

        private DataTable CreateEmptyDataTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.AddRange(new DataColumn[]{ 
                    new DataColumn("ChartType",typeof(string)),
                    new DataColumn("InstrumentId",typeof(Guid)), 
                    new DataColumn("QuotePolicyId",typeof(Guid)),
                    new DataColumn("Date",typeof(DateTime)),

                    new DataColumn("Open",typeof(string)),
                    new DataColumn("Close",typeof(string)),
                    new DataColumn("High",typeof(string)),
                    new DataColumn("Low",typeof(string)),
                    new DataColumn("Volume",typeof(float))            
                });

            return dataTable;
        }

    }
}
