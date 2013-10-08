using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common.Entities;
using System.Data;
using System.Data.SqlClient;

namespace ManagerService.DataAccess
{
    public class SourceInstrumentKey
    {
        public SourceInstrumentKey(int sourceId, int instrumentId)
        {
            this.SourceId = sourceId;
            this.InstrumentId = instrumentId;
        }

        public int SourceId { get; private set; }
        public int InstrumentId { get; private set; }
        public override bool Equals(object obj)
        {
            SourceInstrumentKey other = obj as SourceInstrumentKey;
            if (other == null) return false;
            return this.SourceId == other.SourceId && this.InstrumentId == other.InstrumentId;
        }
        public override int GetHashCode()
        {
            return string.Format("{0}|{1}", this.SourceId, this.InstrumentId).GetHashCode();
        }
    }

    public class QuotationData
    {
        internal static void GetQuotationMetadata(
            Dictionary<string, QuotationSource> quotationSources,
            Dictionary<string, Instrument> instruments,
            Dictionary<int, InstrumentSourceRelation> instrumentSourceRelations,
            Dictionary<int, DerivativeRelation> derivativeRelations,
            Dictionary<int, PriceRangeCheckRule> priceRangeCheckRules,
            Dictionary<int, WeightedPriceRule> weightedPriceRules,
            Dictionary<SourceInstrumentKey, LastQuotation> lastQuotations
            )
        {
            string sql = "dbo.GetInitialDataForQuotationManager";
            DataAccess.ExecuteReader(sql, CommandType.StoredProcedure, delegate(SqlDataReader reader)
            {
                while (reader.Read())
                {
                    QuotationSource quotationSource = new QuotationSource();
                    quotationSource.Id = (int)reader["Id"];
                    quotationSource.Name = (string)reader["Name"];
                    quotationSource.AuthName = (string)reader["AuthName"];
                    quotationSource.Password = (string)reader["Password"];
                    quotationSources.Add(quotationSource.Name, quotationSource);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    Instrument instrument = new Instrument();
                    instrument.Id = (int)reader["Id"];
                    instrument.Code = (string)reader["Code"];
                    instrument.MappingCode = (string)reader["MappingCode"];
                    instrument.DecimalPlace = (int)reader["DecimalPlace"];
                    instrument.Inverted = (bool)reader["Inverted"];
                    instrument.InactiveTime = (int)reader["InactiveTime"];
                    instrument.UseWeightedPrice = (bool)reader["UseWeightedPrice"];
                    instrument.Multiplier = reader["Multiplier"] == DBNull.Value ? null : (decimal?)reader["Multiplier"];
                    instrument.IsDerivative = (bool)reader["IsDerivative"];
                    instruments.Add(instrument.Code, instrument);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    InstrumentSourceRelation instrumentSourceRelation = new InstrumentSourceRelation();
                    instrumentSourceRelation.InstrumentId = (int)reader["InstrumentId"];
                    instrumentSourceRelation.SourceId = (int)reader["SourceId"];
                    instrumentSourceRelation.IsActive = (bool)reader["IsActive"];
                    instrumentSourceRelation.IsDefault = (bool)reader["IsDefault"];
                    instrumentSourceRelation.Priority = (int)reader["Priority"];
                    instrumentSourceRelation.SwitchTimeout = (int)reader["SwitchTimeout"];
                    instrumentSourceRelation.AdjustPoints = (decimal)reader["AdjustPoints"];
                    instrumentSourceRelation.AdjustIncrement = (decimal)reader["AdjustIncrement"];
                    instrumentSourceRelations.Add(instrumentSourceRelation.InstrumentId, instrumentSourceRelation);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    DerivativeRelation derivativeRelation = new DerivativeRelation();
                    derivativeRelation.InstrumentId = (int)reader["InstrumentId"];
                    derivativeRelation.UnderlyingInstrument1Id = (int)reader["UnderlyingInstrument1Id"];
                    derivativeRelation.UnderlyingInstrument1IdInverted = (bool)reader["UnderlyingInstrument1IdInverted"];
                    derivativeRelation.UnderlyingInstrument2Id = reader["UnderlyingInstrument2Id"] == DBNull.Value ? null : (int?)reader["UnderlyingInstrument2Id"];
                    derivativeRelation.AdjustPoints = (decimal)reader["AdjustPoints"];
                    derivativeRelation.AdjustIncrement = (decimal)reader["AdjustIncrement"];
                    derivativeRelation.AskOperand1Type = (byte)reader["AskOperand1Type"];
                    derivativeRelation.AskOperator1Type = reader["AskOperator1Type"] == DBNull.Value ? null : (byte?)reader["AskOperator1Type"];
                    derivativeRelation.AskOperand2Type = reader["AskOperand2Type"] == DBNull.Value ? null : (byte?)reader["AskOperand2Type"];
                    derivativeRelation.AskOperator2Type = (byte)reader["AskOperator2Type"];
                    derivativeRelation.AskOperand3 = (decimal)reader["AskOperand3"];
                    derivativeRelation.BidOperand1Type = (byte)reader["BidOperand1Type"];
                    derivativeRelation.BidOperator1Type = reader["BidOperator1Type"] == DBNull.Value ? null : (byte?)reader["BidOperator1Type"];
                    derivativeRelation.BidOperand2Type = reader["BidOperand2Type"] == DBNull.Value ? null : (byte?)reader["BidOperand2Type"];
                    derivativeRelation.BidOperator2Type = (byte)reader["BidOperator2Type"];
                    derivativeRelation.BidOperand3 = (decimal)reader["BidOperand3"];
                    derivativeRelation.LastOperand1Type = (byte)reader["LastOperand1Type"];
                    derivativeRelation.LastOperator1Type = reader["LastOperator1Type"] == DBNull.Value ? null : (byte?)reader["LastOperator1Type"];
                    derivativeRelation.LastOperand2Type = reader["LastOperand2Type"] == DBNull.Value ? null : (byte?)reader["LastOperand2Type"];
                    derivativeRelation.LastOperator2Type = (byte)reader["LastOperator2Type"];
                    derivativeRelation.LastOperand3 = (decimal)reader["LastOperand3"];
                    derivativeRelations.Add(derivativeRelation.InstrumentId, derivativeRelation);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    PriceRangeCheckRule priceRangeCheckRule = new PriceRangeCheckRule();
                    priceRangeCheckRule.InstrumentId = (int)reader["InstrumentId"];
                    priceRangeCheckRule.DiscardOutOfRangePrice = (bool)reader["DiscardOutOfRangePrice"];
                    priceRangeCheckRule.OutOfRangeType = (byte)reader["OutOfRangeType"];
                    priceRangeCheckRule.ValidVariation = (int)reader["ValidVariation"];
                    priceRangeCheckRule.OutOfRangeWaitTime = (int)reader["OutOfRangeWaitTime"];
                    priceRangeCheckRule.OutOfRangeCount = (int)reader["OutOfRangeCount"];
                    priceRangeCheckRules.Add(priceRangeCheckRule.InstrumentId, priceRangeCheckRule);
                }
                reader.NextResult();
                while(reader.Read())
                {
                    WeightedPriceRule weightedPriceRule = new WeightedPriceRule();
                    weightedPriceRule.InstrumentId = (int)reader["InstrumentId"];
                    weightedPriceRule.AskAskWeight = (int)reader["AskAskWeight"];
                    weightedPriceRule.AskBidWeight = (int)reader["AskBidWeight"];
                    weightedPriceRule.AskLastWeight = (int)reader["AskLastWeight"];
                    weightedPriceRule.BidAskWeight = (int)reader["BidAskWeight"];
                    weightedPriceRule.BidBidWeight = (int)reader["BidBidWeight"];
                    weightedPriceRule.BidLastWeight = (int)reader["BidLastWeight"];
                    weightedPriceRule.LastAskWeight = (int)reader["LastAskWeight"];
                    weightedPriceRule.LastBidWeight = (int)reader["LastBidWeight"];
                    weightedPriceRule.LastLastWeight = (int)reader["LastLastWeight"];
                    weightedPriceRule.AskAvarageWeight = (int)reader["AskAvarageWeight"];
                    weightedPriceRule.BidAvarageWeight = (int)reader["BidAvarageWeight"];
                    weightedPriceRule.LastAvarageWeight = (int)reader["LastAvarageWeight"];
                    weightedPriceRule.AskAdjust = (decimal)reader["AskAdjust"];
                    weightedPriceRule.BidAdjust = (decimal)reader["BidAdjust"];
                    weightedPriceRule.LastAdjust = (decimal)reader["LastAdjust"];
                    weightedPriceRules.Add(weightedPriceRule.InstrumentId, weightedPriceRule);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    LastQuotation lastQuotation = new LastQuotation();
                    lastQuotation.SourceId = (int)reader["SourceId"];
                    lastQuotation.InstrumentId = (int)reader["InstrumentId"];
                    lastQuotation.Timestamp = (DateTime)reader["Timestamp"];
                    lastQuotation.Ask = reader["Ask"] == DBNull.Value ? null : (string)reader["Ask"];
                    lastQuotation.Bid = reader["Bid"] == DBNull.Value ? null : (string)reader["Bid"];
                    lastQuotation.Last = reader["Last"] == DBNull.Value ? null : (string)reader["Last"];
                    lastQuotation.High = reader["High"] == DBNull.Value ? null : (string)reader["High"];
                    lastQuotation.Low = reader["Low"] == DBNull.Value ? null : (string)reader["Low"];
                    lastQuotations.Add(new SourceInstrumentKey(lastQuotation.SourceId, lastQuotation.InstrumentId), lastQuotation);
                }
            });
        }
    }
}
