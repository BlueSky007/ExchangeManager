using iExchange.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ManagerService.QuotationExchange
{
    internal enum QuotationCacheType
    {
        None = 0,
        File,
        MSMQ
    }

    internal static class QuotationExtension
    {
        internal static string ToQuotationFileItem(this iExchange.Common.OverridedQuotation overridedQuotation)
        {
            return string.Format(string.Format("{0},{1},{2:yyyy-MM-dd HH:mm:ss.fffffff},{3},{4},{5},{6},{7},{8},{9}\n",
                overridedQuotation.InstrumentID, overridedQuotation.QuotePolicyID, overridedQuotation.Timestamp,
                overridedQuotation.Origin, overridedQuotation.Ask, overridedQuotation.Bid, overridedQuotation.High, overridedQuotation.Low, overridedQuotation.DealerID, overridedQuotation.Volume));
        }

        internal static string ToQuotationFileItem(this iExchange.Common.OriginQuotation originQuotation)
        {
            return string.Format(string.Format("{0},{1:yyyy-MM-dd HH:mm:ss.fffffff},{2},{3},{4},{5},{6}\n",
                originQuotation.InstrumentID, originQuotation.Timestamp,
                originQuotation.Ask, originQuotation.Bid, originQuotation.High, originQuotation.Low, originQuotation.Volume));
        }
    }

    internal class MemoryBatch
    {
        private static readonly int _DefaultOverridedQuotationCapacity = 100;

        internal List<iExchange.Common.OverridedQuotation> _OverridedQuotations;
        internal List<iExchange.Common.OriginQuotation> _OriginQuotations;
        internal List<string> _FileNames;

        internal MemoryBatch()
        {
            this._OriginQuotations = new List<iExchange.Common.OriginQuotation>();
            this._OverridedQuotations = new List<iExchange.Common.OverridedQuotation>(MemoryBatch._DefaultOverridedQuotationCapacity);
            this._FileNames = new List<string>();
        }

        internal MemoryBatch(string fileName)
            : this()
        {
            try
            {
                this.LoadFromFile(fileName);
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("QuotationServer", string.Format("MemoryBatch.LoadFromFile error: {0}", exception), EventLogEntryType.Error);
                throw;
            }
        }

        internal ICollection<iExchange.Common.OverridedQuotation> OverridedQuotations
        {
            get { return this._OverridedQuotations; }
        }

        internal ICollection<iExchange.Common.OriginQuotation> OriginQuotations
        {
            get { return this._OriginQuotations; }
        }

        internal ICollection<string> FileNames
        {
            get { return this._FileNames; }
        }

        internal void Clear()
        {
            this._OriginQuotations.Clear();
            this._OverridedQuotations.Clear();
            this._FileNames.Clear();
        }

        internal void Add(IEnumerable<iExchange.Common.OriginQuotation> originQuotations, IEnumerable<iExchange.Common.OverridedQuotation> overridedQuotations, string fileName)
        {
            if (originQuotations != null)
            {
                this._OriginQuotations.AddRange(originQuotations);
            }
            if (overridedQuotations != null)
            {
                this._OverridedQuotations.AddRange(overridedQuotations);
            }
            this._FileNames.Add(fileName);
        }

        internal void LoadFromFile(string fileName)
        {
            string content = null;
            using (StreamReader streamReader = new StreamReader(fileName))
            {
                content = streamReader.ReadToEnd();
                streamReader.Close();
            }
            string[] lines = content.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string[] items = line.Split(new char[] { ',' }, StringSplitOptions.None);

                if (items.Length == 7)          // originQuotation
                {
                    DateTime timestamp = DateTime.Parse(items[1]);

                    Guid instrumentId = new Guid(items[0]);
                    string ask = items[2];
                    string bid = items[3];
                    string high = items[4];
                    string low = items[5];

                    iExchange.Common.OriginQuotation originQuotation = new iExchange.Common.OriginQuotation()
                    {
                        InstrumentID = instrumentId,
                        Timestamp = timestamp,
                        Ask = ask,
                        Bid = bid,
                        High = high,
                        Low = low,
                        Volume = items[6]
                    };
                    this._OriginQuotations.Add(originQuotation);

                }
                else if (items.Length == 10)                   // OverridedQuotation
                {
                    Guid instrumentId = new Guid(items[0]);
                    Guid quotePolicyId = new Guid(items[1]);
                    DateTime timestamp = DateTime.Parse(items[2]);
                    string origin = items[3];
                    string ask = items[4];
                    string bid = items[5];
                    string high = items[6];
                    string low = items[7];
                    Guid dealerId = new Guid(items[8]);

                    iExchange.Common.OverridedQuotation overridedQuotation = new iExchange.Common.OverridedQuotation()
                    {
                        InstrumentID = instrumentId,
                        QuotePolicyID = quotePolicyId,
                        Timestamp = timestamp,
                        Origin = origin,
                        Ask = ask,
                        Bid = bid,
                        High = high,
                        Low = low,
                        DealerID = new Guid(items[8]),
                        Volume = items[9]
                    };
                    this._OverridedQuotations.Add(overridedQuotation);
                }
            }

            this._FileNames.Add(fileName);
        }

    }

    internal class DatabaseBatch
    {
        private Dictionary<int, DataTable> _OriginDataTables = new Dictionary<int, DataTable>();        //key: OriginQuotation.Day
        private Dictionary<int, DataTable> _OverridedDataTables = new Dictionary<int, DataTable>();
        private List<string> _FileNames = new List<string>();

        private DataTable _LastSessionLastOriginQuotationTable;
        private Dictionary<Guid, DataRow> _LastSessionLastOriginQuotationRows = new Dictionary<Guid, DataRow>();

        private DataTable _LastSessionLastOverridedQuotationTable;
        private Dictionary<Guid, Dictionary<Guid, DataRow>> _LastSessionLastOverridedQuotationRows = new Dictionary<Guid, Dictionary<Guid, DataRow>>();

        internal DatabaseBatch()
        {
            this._LastSessionLastOriginQuotationTable = this.CreateEmptyDataTable(true);
            this._LastSessionLastOverridedQuotationTable = this.CreateEmptyDataTable(false);
        }

        internal DatabaseBatch(MemoryBatch memoryBatch)
            : this()
        {
            this.Add(memoryBatch);
        }

        internal Dictionary<int, DataTable> OriginDataTables
        {
            get { return this._OriginDataTables; }
        }

        internal Dictionary<int, DataTable> OverridedDataTables
        {
            get { return this._OverridedDataTables; }
        }

        internal List<string> FileNames
        {
            get { return this._FileNames; }
        }

        internal DataTable LastSessionLastOriginQuotationTable
        {
            get { return this._LastSessionLastOriginQuotationTable; }
        }

        internal DataTable LastSessionLastOverridedQuotationTable
        {
            get { return this._LastSessionLastOverridedQuotationTable; }
        }

        internal void Clear()
        {
            this._OriginDataTables.Clear();
            this._OverridedDataTables.Clear();
            this._LastSessionLastOriginQuotationRows.Clear();
            this._LastSessionLastOriginQuotationTable.Clear();
            this._LastSessionLastOverridedQuotationRows.Clear();
            this._LastSessionLastOverridedQuotationTable.Clear();

            this._FileNames.Clear();
        }

        internal bool IsEmpty()
        {
            return (this._OriginDataTables.Count == 0 && this._OverridedDataTables.Count == 0 && this._FileNames.Count == 0);
        }

        internal bool Add(MemoryBatch memoryBatch)
        {
            try
            {
                foreach (iExchange.Common.OriginQuotation originQuotation in memoryBatch.OriginQuotations)
                {
                    DataTable dataTable = null;
                    int day = originQuotation.Timestamp.Day;
                    if (this._OriginDataTables.ContainsKey(day))
                    {
                        dataTable = this._OriginDataTables[day];
                    }
                    else
                    {
                        dataTable = this.CreateEmptyDataTable(true);
                        this._OriginDataTables.Add(day, dataTable);
                    }
                    DataRow row = dataTable.NewRow();
                    row["InstrumentId"] = originQuotation.InstrumentID;
                    row["Timestamp"] = originQuotation.Timestamp;
                    row["Day"] = day;

                    row["Ask"] = originQuotation.Ask;
                    row["Bid"] = originQuotation.Bid;
                    row["High"] = originQuotation.High;
                    row["Low"] = originQuotation.Low;
                    if (!string.IsNullOrEmpty(originQuotation.Volume))
                    {
                        row["Volume"] = double.Parse(originQuotation.Volume);
                    }
                    dataTable.Rows.Add(row);


                    // update LastSessionLastOriginQuotations
                    DataRow originRow = null;
                    if (this._LastSessionLastOriginQuotationRows.ContainsKey(originQuotation.InstrumentID))
                    {
                        originRow = this._LastSessionLastOriginQuotationRows[originQuotation.InstrumentID];
                    }
                    else
                    {
                        originRow = this._LastSessionLastOriginQuotationTable.NewRow();
                        originRow["InstrumentId"] = originQuotation.InstrumentID;
                        this._LastSessionLastOriginQuotationTable.Rows.Add(originRow);
                        this._LastSessionLastOriginQuotationRows.Add(originQuotation.InstrumentID, originRow);
                    }
                    originRow["Timestamp"] = originQuotation.Timestamp;
                    originRow["Day"] = originQuotation.Timestamp.Day;
                    originRow["Ask"] = originQuotation.Ask;
                    originRow["Bid"] = originQuotation.Bid;
                    originRow["High"] = originQuotation.High;
                    originRow["Low"] = originQuotation.Low;
                    if (string.IsNullOrEmpty(originQuotation.Volume))
                    {
                        originRow["Volume"] = DBNull.Value;
                    }
                    else
                    {
                        originRow["Volume"] = double.Parse(originQuotation.Volume);
                    }
                    //
                }

                foreach (iExchange.Common.OverridedQuotation overridedQuotation in memoryBatch.OverridedQuotations)
                {
                    DataTable dataTable = null;
                    int day = overridedQuotation.Timestamp.Day;
                    if (this._OverridedDataTables.ContainsKey(day))
                    {
                        dataTable = this._OverridedDataTables[day];
                    }
                    else
                    {
                        dataTable = this.CreateEmptyDataTable(false);
                        this._OverridedDataTables.Add(day, dataTable);
                    }
                    DataRow row = dataTable.NewRow();
                    row["InstrumentId"] = overridedQuotation.InstrumentID;
                    row["QuotePolicyId"] = overridedQuotation.QuotePolicyID;
                    row["Timestamp"] = overridedQuotation.Timestamp;
                    row["Day"] = day;

                    row["Origin"] = overridedQuotation.Origin;
                    row["Ask"] = overridedQuotation.Ask;
                    row["Bid"] = overridedQuotation.Bid;
                    row["High"] = overridedQuotation.High;
                    row["Low"] = overridedQuotation.Low;
                    row["DealerId"] = overridedQuotation.DealerID;
                    if (string.IsNullOrEmpty(overridedQuotation.Volume))
                    {
                        row["Volume"] = DBNull.Value;
                    }
                    else
                    {
                        row["Volume"] = double.Parse(overridedQuotation.Volume);
                    }

                    dataTable.Rows.Add(row);


                    // update LastSessionLastOverridedQuotations
                    DataRow overridedRow = null;
                    Dictionary<Guid, DataRow> overridedRows = null;
                    if (this._LastSessionLastOverridedQuotationRows.ContainsKey(overridedQuotation.InstrumentID))
                    {
                        overridedRows = this._LastSessionLastOverridedQuotationRows[overridedQuotation.InstrumentID];
                    }
                    else
                    {
                        overridedRows = new Dictionary<Guid, DataRow>();
                        this._LastSessionLastOverridedQuotationRows.Add(overridedQuotation.InstrumentID, overridedRows);
                    }

                    if (overridedRows.ContainsKey(overridedQuotation.QuotePolicyID))
                    {
                        overridedRow = overridedRows[overridedQuotation.QuotePolicyID];
                    }
                    else
                    {
                        overridedRow = this._LastSessionLastOverridedQuotationTable.NewRow();
                        overridedRow["InstrumentId"] = overridedQuotation.InstrumentID;
                        overridedRow["QuotePolicyId"] = overridedQuotation.QuotePolicyID;

                        this._LastSessionLastOverridedQuotationTable.Rows.Add(overridedRow);
                        overridedRows.Add(overridedQuotation.QuotePolicyID, overridedRow);
                    }

                    overridedRow["Timestamp"] = overridedQuotation.Timestamp;
                    overridedRow["Day"] = overridedQuotation.Timestamp.Day;
                    overridedRow["Origin"] = overridedQuotation.Origin;
                    overridedRow["Ask"] = overridedQuotation.Ask;
                    overridedRow["Bid"] = overridedQuotation.Bid;
                    overridedRow["High"] = overridedQuotation.High;
                    overridedRow["Low"] = overridedQuotation.Low;
                    overridedRow["DealerId"] = overridedQuotation.DealerID;
                    if (string.IsNullOrEmpty(overridedQuotation.Volume))
                    {
                        overridedRow["Volume"] = DBNull.Value;
                    }
                    else
                    {
                        overridedRow["Volume"] = double.Parse(overridedQuotation.Volume);
                    }
                    //
                }

                this._FileNames.AddRange(memoryBatch.FileNames);
                return true;
            }
            catch (Exception exception)   // stream exception or file delete exception
            {
                AppDebug.LogEvent("QuotationServer", string.Format("DatabaseBatch.Add error: {0}", exception), EventLogEntryType.Error);
                throw;
            }
        }

        private DataTable CreateEmptyDataTable(bool isOriginQuotation)
        {
            if (isOriginQuotation)
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.AddRange(new DataColumn[]{ 
                    new DataColumn("InstrumentId",typeof(Guid)), 
                    new DataColumn("Timestamp",typeof(DateTime)),
                    new DataColumn("Day",typeof(int)),

                    new DataColumn("Ask",typeof(string)),
                    new DataColumn("Bid",typeof(string)),
                    new DataColumn("High",typeof(string)),
                    new DataColumn("Low",typeof(string)),
                    new DataColumn("Volume",typeof(float))            
                });

                return dataTable;
            }
            else
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.AddRange(new DataColumn[]{ 
                    new DataColumn("InstrumentId",typeof(Guid)), 
                    new DataColumn("QuotePolicyId",typeof(Guid)), 
                    new DataColumn("Timestamp",typeof(DateTime)),

                    new DataColumn("Day",typeof(int)),
                    new DataColumn("Origin",typeof(string)),
                    new DataColumn("Ask",typeof(string)),
                    new DataColumn("Bid",typeof(string)),
                    new DataColumn("High",typeof(string)),
                    new DataColumn("Low",typeof(string)),
                    new DataColumn("DealerId",typeof(Guid)),
                    new DataColumn("Volume",typeof(float))            
                });

                return dataTable;
            }
        }


    }
}
