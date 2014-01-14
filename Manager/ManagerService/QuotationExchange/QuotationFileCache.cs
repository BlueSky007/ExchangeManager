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

namespace ManagerService.QuotationExchange
{
    public class QuotationFileCache
    {
        QuotationServer _QuotationServer;
        private string _DirectoryName = "Quotations";
        private string _QuotationConnectionString;
        private int _BatchSize = 100;      //combine 100 files in one bulkCopy

        private bool _HasPendingQuotationsBeforeStart = true;
        private Queue<MemoryBatch> _Queue = new Queue<MemoryBatch>();
        private MemoryBatch _ActiveMemoryBatch = null;                                // null means need create new item and EnQueue
        private long _FileNumber = 0;

        private Thread _Thread = null;                                          //keep running until IIS abort it
        private TimeSpan _CommitFrequency = new TimeSpan(0, 0, 10);             // check directory every 10 seconds

        private object _QueueLock = new object();

        #region Constructors

        public QuotationFileCache(QuotationServer quotationServer, string directoryName, string connectionString)
        {
            this._QuotationServer = quotationServer;
            this._DirectoryName = directoryName;
            this._QuotationConnectionString = connectionString;

            if (!Directory.Exists(this._DirectoryName))
            {
                Directory.CreateDirectory(this._DirectoryName);
            }

            this.LoadExistingFilesBeforeStart();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directoryName">direcotry to writing quotation files</param>
        /// <param name="connectionString">database connection string</param>
        /// <param name="batchSize">how many OriginQuotations contains in one BulkCopy</param>
        public QuotationFileCache(QuotationServer quotationServer, string directoryName, string connectionString, int batchSize, TimeSpan commitFrequency)
            : this(quotationServer, directoryName, connectionString)
        {
            this._BatchSize = batchSize;
            this._CommitFrequency = commitFrequency;
        }
        #endregion

        #region Write to file
        public ICollection<iExchange.Common.OverridedQuotation> Add(ICollection<OriginQuotation> originQuotations, ICollection<OverridedQuotation> overridedQuotations)
        {
            List<iExchange.Common.OriginQuotation> lightOriginQuotations = (originQuotations == null ? null : new List<iExchange.Common.OriginQuotation>(originQuotations.Count));
            List<iExchange.Common.OverridedQuotation> lightOverridedQuotations = (overridedQuotations == null ? null : new List<iExchange.Common.OverridedQuotation>(overridedQuotations.Count));
            if (originQuotations != null)
            {
                foreach (OriginQuotation q in originQuotations.Where(o => o.ModifyState != ModifyState.Unchanged))
                {
                    q.Saved = true;
                    q.ModifyState = ModifyState.Unchanged;
                    lightOriginQuotations.Add(q.ToLightVersion());
                }
            }

            if (overridedQuotations != null)
            {
                foreach (OverridedQuotation q in overridedQuotations.Where(o => o.ModifyState != ModifyState.Unchanged))
                {
                    q.Saved = true;
                    q.ModifyState = ModifyState.Unchanged;
                    lightOverridedQuotations.Add(q.ToLightVersion());
                }
            }
            if ((lightOriginQuotations != null && lightOriginQuotations.Count > 0)
                    || (lightOverridedQuotations != null && lightOverridedQuotations.Count > 0))
            {
                this.Add(lightOriginQuotations, lightOverridedQuotations);
            }
            return lightOverridedQuotations;
        }

        private bool Add(IEnumerable<iExchange.Common.OriginQuotation> originQuotations, IEnumerable<iExchange.Common.OverridedQuotation> overridedQuotations)
        {
            if (originQuotations == null && overridedQuotations == null) return true;
            try
            {
                string fileName;

                lock (this._QueueLock)
                {
                    fileName = string.Format("{0}\\{1}.txt", this._DirectoryName, this._FileNumber);
                    if (this._ActiveMemoryBatch == null)
                    {
                        this._ActiveMemoryBatch = new MemoryBatch();
                        this._Queue.Enqueue(this._ActiveMemoryBatch);
                    }
                    this._ActiveMemoryBatch.Add(originQuotations, overridedQuotations, fileName);

                    if (this._ActiveMemoryBatch.FileNames.Count >= this._BatchSize)
                    {
                        this._ActiveMemoryBatch = null;
                    }

                    this._FileNumber++;
                }

                using (StreamWriter streamWriter = new StreamWriter(fileName, true))
                {
                    if (originQuotations != null)
                    {
                        foreach (iExchange.Common.OriginQuotation originQuotation in originQuotations)
                        {
                            streamWriter.Write(originQuotation.ToQuotationFileItem());
                        }
                    }
                    if (overridedQuotations != null)
                    {
                        foreach (iExchange.Common.OverridedQuotation overridedQuotation in overridedQuotations)
                        {
                            streamWriter.Write(overridedQuotation.ToQuotationFileItem());
                        }
                    }
                    streamWriter.Close();
                }
                return true;
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("QuotationServer", string.Format("DatabaseBatch.Add Error: {0}", exception), EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Load existing file to memory, set filenumber
        /// </summary>
        private void LoadExistingFilesBeforeStart()
        {
            IOrderedEnumerable<string> existingFilesBeforeStart = Directory.GetFiles(this._DirectoryName).OrderBy(o =>
            {
                int fileNumber = 0;
                if (int.TryParse(Path.GetFileNameWithoutExtension(Path.GetFileName(o)), out fileNumber))
                {
                    return fileNumber;
                }
                else
                {
                    return 0;
                }
            });

            MemoryBatch memoryBatch = new MemoryBatch();
            foreach (string fileName in existingFilesBeforeStart)
            {
                memoryBatch.LoadFromFile(fileName);

                if (memoryBatch.FileNames.Count >= this._BatchSize)
                {
                    this._Queue.Enqueue(memoryBatch);
                    memoryBatch = new MemoryBatch();
                }
            }
            if (memoryBatch.FileNames.Count > 0)
            {
                this._Queue.Enqueue(memoryBatch);
            }

            int result = 0;
            if (existingFilesBeforeStart != null)
            {
                foreach (string fileName in existingFilesBeforeStart)
                {
                    int fileNumber = 0;

                    if (int.TryParse(Path.GetFileNameWithoutExtension(Path.GetFileName(fileName)), out fileNumber) && fileNumber > result)
                    {
                        result = fileNumber;
                    }
                }
            }

            this._FileNumber = result + 1;
            this._HasPendingQuotationsBeforeStart = (this._Queue.Count > 0);
        }

        #endregion

        #region SaveToDatabase

        public void Start()
        {
            if (this._Thread != null) return;

            this._Thread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    if (this._HasPendingQuotationsBeforeStart)
                    {
                        if (this.SaveMemoryToDatabase())
                        {
                            this._HasPendingQuotationsBeforeStart = false;
                            if (this._QuotationServer.EnableChartGeneration)
                            {
                                this._QuotationServer.ChartQuotationManager.Start();
                            }
                        }
                    }
                    else if (!this._QuotationServer.EnableChartGeneration || this._QuotationServer.ChartQuotationManager.IsReady)
                    {
                        this.SaveMemoryToDatabase();
                    }
                    else
                    {
                        if (this._QuotationServer.EnableChartGeneration)
                        {
                            this._QuotationServer.ChartQuotationManager.Start();
                        }
                    }

                    Thread.Sleep(this._CommitFrequency);
                }
            }));
            this._Thread.Start();
        }

        //external call flush must waiting for all pending .txt quotation files saved
        public bool Flush()
        {
            lock (this._QueueLock)
            {
                while (this._HasPendingQuotationsBeforeStart)
                {
                    AppDebug.LogEvent("QuotationServer", "QuotationFileCache.Flush, HasPendingQuotationsBeforeStart=True", EventLogEntryType.Warning);
                    Thread.Sleep(500);
                }
                return this.SaveMemoryToDatabase();
            }
        }

        ///////////////////////////////////////////////////
        //save memory quotations to database
        ///////////////////////////////////////////////////
        public bool SaveMemoryToDatabase()
        {
            try
            {
                //AppDebug.LogEvent("QuotationServer", string.Format("begin QuotationFileCache.SaveMemoryToDatabase, Queue.Count={0}", this._Queue.Count), EventLogEntryType.Information);
                lock (this._QueueLock)
                {
                    while (this._Queue.Count > 0)
                    {
                        MemoryBatch memoryBatch = this._Queue.Peek();
                        DatabaseBatch databaseBatch = new DatabaseBatch(memoryBatch);

                        if (this.SaveDatabaseBatch(databaseBatch))
                        {
                            databaseBatch.Clear();
                            this._Queue.Dequeue();
                        }

                        if (this._Queue.Count == 0 && this._ActiveMemoryBatch != null)
                        {
                            this._ActiveMemoryBatch = null;
                        }
                    }
                }
                //AppDebug.LogEvent("QuotationServer", string.Format("end QuotationFileCache.SaveMemoryToDatabase, Queue.Count={0}", this._Queue.Count), EventLogEntryType.Information);
                return true;
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("QuotationServer", string.Format("QuotationFileCache.SaveMemoryToDatabase error: {0}", exception), EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// if bulkcopy failed,  rollback and then use table value SP. if transaction commit success then delete files
        /// </summary>
        private bool SaveDatabaseBatch(DatabaseBatch databaseBatch)
        {
            bool isSavedSuccess = true;
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(this._QuotationConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlTransaction sqlTransaction = sqlConnection.BeginTransaction())
                    {

                        foreach (KeyValuePair<int, DataTable> keyValue in databaseBatch.OriginDataTables)
                        {
                            if (!SaveByBulkCopy(sqlConnection, sqlTransaction, keyValue.Value, keyValue.Key, true))
                            {
                                if (!SaveByTableValue(sqlConnection, sqlTransaction, keyValue.Value, keyValue.Key, true))
                                {
                                    isSavedSuccess = false;
                                    break;
                                }

                            }
                        }

                        foreach (KeyValuePair<int, DataTable> keyValue in databaseBatch.OverridedDataTables)
                        {
                            if (!SaveByBulkCopy(sqlConnection, sqlTransaction, keyValue.Value, keyValue.Key, false))
                            {
                                if (!SaveByTableValue(sqlConnection, sqlTransaction, keyValue.Value, keyValue.Key, false))
                                {
                                    isSavedSuccess = false;
                                    break;
                                }
                            }
                        }

                        if (isSavedSuccess)
                        {
                            SqlCommand sqlCommand = sqlConnection.CreateCommand();
                            sqlCommand.Transaction = sqlTransaction;
                            sqlCommand.CommandText = "dbo.P_UpdateLastSessionLastQuotation";
                            sqlCommand.CommandType = CommandType.StoredProcedure;
                            SqlParameter parameter = sqlCommand.Parameters.AddWithValue("@OriginQuotationTable", databaseBatch.LastSessionLastOriginQuotationTable);
                            parameter.SqlDbType = SqlDbType.Structured;
                            parameter = sqlCommand.Parameters.AddWithValue("@OverridedQuotationTable", databaseBatch.LastSessionLastOverridedQuotationTable);
                            parameter.SqlDbType = SqlDbType.Structured;

                            parameter = sqlCommand.Parameters.AddWithValue("@result", null);
                            parameter.Direction = ParameterDirection.ReturnValue;
                            sqlCommand.ExecuteNonQuery();

                            isSavedSuccess = (parameter.Value != DBNull.Value && (int)parameter.Value == 0);
                        }
                        if (isSavedSuccess)
                        {
                            sqlTransaction.Commit();
                        }
                        else
                        {
                            sqlTransaction.Rollback();
                        }
                    }

                    sqlConnection.Close();
                }
            }
            catch (Exception exception)
            {

                AppDebug.LogEvent("QuotationServer", string.Format("QuotationFileCache.SaveDatabaseBatch error: {0}, {1}", string.Join(",", databaseBatch.FileNames), exception), EventLogEntryType.Error);
                return false;
            }

            if (isSavedSuccess)
            {
                foreach (string fileName in databaseBatch.FileNames)
                {
                    try
                    {
                        File.Delete(fileName);
                    }
                    catch (Exception exception)
                    {
                        //isSavedSuccess = false;                        
                        AppDebug.LogEvent("QuotationServer", string.Format("DatabaseBatch.SaveToDatabaseInternal warning: {0}", exception), EventLogEntryType.Warning);
                    }
                }
            }
            return isSavedSuccess;
        }

        private bool SaveByBulkCopy(SqlConnection sqlConnection, SqlTransaction sqlTransaction, DataTable dataTable, int day, bool isOriginQuotation)
        {
            try
            {
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.CheckConstraints, sqlTransaction))
                {
                    //bulkCopy.ColumnMappings.Add("Id", "Id");
                    bulkCopy.ColumnMappings.Add("InstrumentId", "InstrumentId");
                    bulkCopy.ColumnMappings.Add("Timestamp", "Timestamp");
                    bulkCopy.ColumnMappings.Add("Ask", "Ask");
                    bulkCopy.ColumnMappings.Add("Bid", "Bid");
                    bulkCopy.ColumnMappings.Add("High", "High");
                    bulkCopy.ColumnMappings.Add("Low", "Low");
                    bulkCopy.ColumnMappings.Add("Day", "Day");
                    bulkCopy.ColumnMappings.Add("Volume", "Volume");

                    if (!isOriginQuotation)
                    {
                        bulkCopy.ColumnMappings.Add("QuotePolicyId", "QuotePolicyId");
                        bulkCopy.ColumnMappings.Add("Origin", "Origin");
                        bulkCopy.ColumnMappings.Add("DealerId", "DealerId");
                    }
                    bulkCopy.DestinationTableName = this.GetTableName(day, isOriginQuotation);
                    bulkCopy.BatchSize = dataTable.Rows.Count;
                    bulkCopy.WriteToServer(dataTable);
                    return true;
                }
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("QuotationServer", string.Format("QuotationFileCache.SaveDatabaseBatch error: {0}", exception), EventLogEntryType.Error);
                return false;
            }
        }

        private bool SaveByTableValue(SqlConnection sqlConnection, SqlTransaction sqlTransaction, DataTable dataTable, int day, bool isOriginQuotation)
        {
            try
            {
                string procedureName = (isOriginQuotation ? "dbo.P_AddOriginQuotationHistory2" : "dbo.P_AddOverridedQuotationHistory2");
                SqlCommand sqlCommand = new SqlCommand(procedureName, sqlConnection, sqlTransaction);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                SqlParameter parameter = sqlCommand.Parameters.AddWithValue("@table", dataTable);
                parameter.SqlDbType = SqlDbType.Structured;

                parameter = sqlCommand.Parameters.AddWithValue("@tableName", this.GetTableName(day, isOriginQuotation));
                parameter = sqlCommand.Parameters.AddWithValue("@result", null);
                parameter.Direction = ParameterDirection.ReturnValue;
                sqlCommand.ExecuteNonQuery();
                return (parameter.Value != DBNull.Value && (int)parameter.Value == 0);
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("QuotationServer", string.Format("QuotationFileCache.SaveDatabaseBatch error: {0}", exception), EventLogEntryType.Error);
                return false;
            }
        }

        private string GetTableName(int day, bool isOriginQuotation)
        {
            return (isOriginQuotation ? "OriginQuotationHistory" : "OverridedQuotationHistory")
                + (day < 10 ? ("0" + day.ToString()) : day.ToString());
        }
        #endregion

    }
}
