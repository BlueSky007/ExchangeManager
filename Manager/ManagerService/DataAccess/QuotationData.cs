﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Manager.Common.QuotationEntities;

namespace ManagerService.DataAccess
{
    public class QuotationData
    {
        internal static void GetQuotationMetadata(
            Dictionary<string, QuotationSource> quotationSources,
            Dictionary<string, Instrument> instruments,
            Dictionary<int, Dictionary<int, InstrumentSourceRelation>> instrumentSourceRelations,
            Dictionary<int, DerivativeRelation> derivativeRelations,
            Dictionary<int, PriceRangeCheckRule> priceRangeCheckRules,
            Dictionary<int, WeightedPriceRule> weightedPriceRules,
            Dictionary<SourceInstrumentKey, LastQuotation> lastQuotations
            )
        {
            string sql = "dbo.GetInitialDataForQuotationManager";
            DataAccess.GetInstance().ExecuteReader(sql, CommandType.StoredProcedure, delegate(SqlDataReader reader)
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
                    instrument.IsDerivative = (bool)reader["IsDerivative"];
                    instrument.IsSwitchUseAgio = (bool)reader["IsSwitchUseAgio"];
                    instrument.AgioSeconds = reader["AgioSeconds"] == DBNull.Value ? null : (int?)reader["AgioSeconds"];
                    instrument.LeastTicks = reader["LeastTicks"] == DBNull.Value ? null : (int?)reader["LeastTicks"];
                    instruments.Add(instrument.Code, instrument);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    InstrumentSourceRelation relation = new InstrumentSourceRelation();
                    relation.InstrumentId = (int)reader["InstrumentId"];
                    relation.SourceId = (int)reader["SourceId"];
                    relation.IsActive = (bool)reader["IsActive"];
                    relation.IsDefault = (bool)reader["IsDefault"];
                    relation.Priority = (int)reader["Priority"];
                    relation.SwitchTimeout = (int)reader["SwitchTimeout"];
                    relation.AdjustPoints = (double)reader["AdjustPoints"];
                    relation.AdjustIncrement = (double)reader["AdjustIncrement"];
                    //instrumentSourceRelations.Add(new SourceInstrumentKey(instrumentSourceRelation.SourceId, instrumentSourceRelation.InstrumentId), instrumentSourceRelation);
                    Dictionary<int, InstrumentSourceRelation> sources;
                    if (!instrumentSourceRelations.TryGetValue(relation.InstrumentId, out sources))
                    {
                        sources = new Dictionary<int, InstrumentSourceRelation>();
                        instrumentSourceRelations.Add(relation.InstrumentId, sources);
                    }
                    sources.Add(relation.SourceId, relation);
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
                    priceRangeCheckRule.OutOfRangeType = (OutOfRangeType)(byte)reader["OutOfRangeType"];
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
                    weightedPriceRule.Multiplier = (decimal)reader["Multiplier"];
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

        internal static void SaveLastQuotation(PrimitiveQuotation quotation)
        {
            DataAccess.GetInstance().ExecuteNonQuery("dbo.LastQuotation_Set", CommandType.StoredProcedure,
                new SqlParameter("@sourceId", quotation.SourceId),
                new SqlParameter("@instrumentId", quotation.InstrumentId),
                new SqlParameter("@timestamp", quotation.Timestamp),
                new SqlParameter("@ask", quotation.Ask),
                new SqlParameter("@bid", quotation.Bid),
                new SqlParameter("@last", quotation.Last),
                new SqlParameter("@high", quotation.High),
                new SqlParameter("@low", quotation.Low));
        }
    }
}
