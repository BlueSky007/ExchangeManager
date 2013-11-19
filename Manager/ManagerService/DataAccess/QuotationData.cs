using System;
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
            //Dictionary<int, Dictionary<int, InstrumentSourceRelation>> instrumentSourceRelations,
            Dictionary<int, Dictionary<string, InstrumentSourceRelation>> instrumentSourceRelations,
            Dictionary<int, DerivativeRelation> derivativeRelations,
            Dictionary<int, PriceRangeCheckRule> priceRangeCheckRules,
            Dictionary<int, WeightedPriceRule> weightedPriceRules,
            Dictionary<int, GeneralQuotation> lastQuotations
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
                    relation.SourceId = (int)reader["SourceId"];
                    relation.SourceSymbol = (string)reader["SourceSymbol"];
                    relation.InstrumentId = (int)reader["InstrumentId"];
                    relation.IsActive = (bool)reader["IsActive"];
                    relation.IsDefault = (bool)reader["IsDefault"];
                    relation.Priority = (int)reader["Priority"];
                    relation.SwitchTimeout = (int)reader["SwitchTimeout"];
                    relation.AdjustPoints = (double)reader["AdjustPoints"];
                    relation.AdjustIncrement = (double)reader["AdjustIncrement"];
                    //Dictionary<int, InstrumentSourceRelation> sources;
                    //if (!instrumentSourceRelations.TryGetValue(relation.InstrumentId, out sources))
                    //{
                    //    sources = new Dictionary<int, InstrumentSourceRelation>();
                    //    instrumentSourceRelations.Add(relation.InstrumentId, sources);
                    //}
                    //sources.Add(relation.SourceId, relation);

                    Dictionary<string, InstrumentSourceRelation> relations;
                    if (!instrumentSourceRelations.TryGetValue(relation.SourceId, out relations))
                    {
                        relations = new Dictionary<string, InstrumentSourceRelation>();
                        instrumentSourceRelations.Add(relation.SourceId, relations);
                    }
                    relations.Add(relation.SourceSymbol, relation);
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
                    derivativeRelation.AskOperand1Type = (OperandType)(byte)reader["AskOperand1Type"];
                    derivativeRelation.AskOperator1Type = reader["AskOperator1Type"] == DBNull.Value ? null : (OperatorType?)(byte)reader["AskOperator1Type"];
                    derivativeRelation.AskOperand2Type = reader["AskOperand2Type"] == DBNull.Value ? null : (OperandType?)(byte)reader["AskOperand2Type"];
                    derivativeRelation.AskOperator2Type = (OperatorType)(byte)reader["AskOperator2Type"];
                    derivativeRelation.AskOperand3 = (decimal)reader["AskOperand3"];
                    derivativeRelation.BidOperand1Type = (OperandType)(byte)reader["BidOperand1Type"];
                    derivativeRelation.BidOperator1Type = reader["BidOperator1Type"] == DBNull.Value ? null : (OperatorType?)(byte)reader["BidOperator1Type"];
                    derivativeRelation.BidOperand2Type = reader["BidOperand2Type"] == DBNull.Value ? null : (OperandType?)(byte)reader["BidOperand2Type"];
                    derivativeRelation.BidOperator2Type = (OperatorType)(byte)reader["BidOperator2Type"];
                    derivativeRelation.BidOperand3 = (decimal)reader["BidOperand3"];
                    derivativeRelation.LastOperand1Type = (OperandType)(byte)reader["LastOperand1Type"];
                    derivativeRelation.LastOperator1Type = reader["LastOperator1Type"] == DBNull.Value ? null : (OperatorType?)(byte)reader["LastOperator1Type"];
                    derivativeRelation.LastOperand2Type = reader["LastOperand2Type"] == DBNull.Value ? null : (OperandType?)(byte)reader["LastOperand2Type"];
                    derivativeRelation.LastOperator2Type = (OperatorType)(byte)reader["LastOperator2Type"];
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
                    GeneralQuotation lastQuotation = new GeneralQuotation();
                    lastQuotation.SourceId = (int)reader["SourceId"];
                    lastQuotation.InstrumentId = (int)reader["InstrumentId"];
                    lastQuotation.Timestamp = (DateTime)reader["Timestamp"];
                    lastQuotation.Ask = (double)reader["Ask"];
                    lastQuotation.Bid = (double)reader["Bid"];
                    lastQuotation.Last = reader["Last"] == DBNull.Value ? null : (double?)reader["Last"];
                    lastQuotation.High = reader["High"] == DBNull.Value ? null : (double?)reader["High"];
                    lastQuotation.Low = reader["Low"] == DBNull.Value ? null : (double?)reader["Low"];
                    lastQuotations.Add(lastQuotation.InstrumentId, lastQuotation);
                }
            });
        }

        internal static void SaveLastQuotation(GeneralQuotation quotation)
        {
            DataAccess.GetInstance().ExecuteNonQuery("dbo.LastQuotation_Set", CommandType.StoredProcedure,
                new SqlParameter("@instrumentId", quotation.InstrumentId),
                new SqlParameter("@sourceId", quotation.SourceId),
                new SqlParameter("@timestamp", quotation.Timestamp),
                new SqlParameter("@ask", quotation.Ask),
                new SqlParameter("@bid", quotation.Bid),
                new SqlParameter("@last", quotation.Last),
                new SqlParameter("@high", quotation.High),
                new SqlParameter("@low", quotation.Low));
        }

        private static bool UpdateQuotationSource(QuotationSource quotationSource)
        {
            SqlParameter id = new SqlParameter("@id", quotationSource.Id) { Direction = ParameterDirection.InputOutput };
            DataAccess.GetInstance().ExecuteNonQuery("dbo.QuotationSource_Set", CommandType.StoredProcedure,
                id,
                new SqlParameter("@name", quotationSource.Name),
                new SqlParameter("@authName", quotationSource.AuthName),
                new SqlParameter("@password", quotationSource.Password));

            if (quotationSource.Id == 0)
            {
                quotationSource.Id = (int)id.Value;
            }
            return true;
        }

        private static void AddInstrument(Instrument instrument)
        {
            string sql = "INSERT Instrument(Code, DecimalPlace, Inverted, InactiveTime, UseWeightedPrice, IsDerivative, IsSwitchUseAgio, AgioSeconds, LeastTicks) VALUES ({0});SELECT SCOPE_IDENTITY()";
            string values = string.Join(",", instrument.Code,
                instrument.DecimalPlace, instrument.Inverted, instrument.InactiveTime, instrument.UseWeightedPrice,
                instrument.IsDerivative, instrument.IsSwitchUseAgio,
                (instrument.AgioSeconds.HasValue ? instrument.AgioSeconds.Value.ToString() : "NULL"),
                (instrument.LeastTicks.HasValue ? instrument.LeastTicks.Value.ToString() : "NULL"));
            sql = string.Format(sql, values);
            instrument.Id = (int)DataAccess.GetInstance().ExecuteScalar(sql, CommandType.Text);
        }

        private static void AddInstrumentSourceRelation(InstrumentSourceRelation relation)
        {
            string sql = "INSERT InstrumentSourceRelationInstrumentSourceRelation(SourceId, SourceSymbol, InstrumentId, IsActive, IsDefault, Priority, SwitchTimeout, AdjustPoints, AdjustIncrement) VALUES ({0});SELECT SCOPE_IDENTITY()";
            string values = string.Join(",", relation.SourceId,
                relation.InstrumentId, relation.IsActive, relation.IsDefault, relation.Priority,
                relation.SwitchTimeout, relation.AdjustPoints, relation.AdjustIncrement);
            sql = string.Format(sql, values);
            relation.Id = (int)DataAccess.GetInstance().ExecuteScalar(sql, CommandType.Text);
        }

        internal static IMetadataObject AddMetadataObject(MetadataType type, Dictionary<string, string> fields)
        {
            switch (type)
            {
                case MetadataType.QuotationSource:
                    QuotationSource source = QuotationSource.Convert(fields);
                    QuotationData.UpdateQuotationSource(source);
                    return source;
                case MetadataType.Instrument:
                    Instrument instrument = Instrument.Convert(fields);
                    QuotationData.AddInstrument(instrument);
                    return instrument;
                case MetadataType.InstrumentSourceRelation:
                    InstrumentSourceRelation relation = InstrumentSourceRelation.Convert(fields);
                    QuotationData.AddInstrumentSourceRelation(relation);
                    return relation;
                case MetadataType.DerivativeRelation:
                    //DerivativeRelation derivativeRelation = DerivativeRelation.Convert(fields);
                    //QuotationData.AddDerivativeRelation(derivativeRelation);
                    //return derivativeRelation;
                case MetadataType.PriceRangeCheckRule:
                    //PriceRangeCheckRule priceRangeCheckRule PriceRangeCheckRule.Convert(fields);
                    //QuotationData.AddPriceRangeCheckRule(priceRangeCheckRule);
                    //return priceRangeCheckRule;
                case MetadataType.WeightedPriceRule:
                    //WeightedPriceRule weightedPriceRule = WeightedPriceRule.Convert(fields);
                    //QuotationData.AddWeightedPriceRule(weightedPriceRule);
                    //return weightedPriceRule;
                default:
                    break;
            }
            return null;
        }

        internal static void UpdateMetadataObject(MetadataType type, int objectId, Dictionary<string, string> fields)
        {
            string tableName, keyFieldName;
            Helper.GetTableName(type, out tableName, out keyFieldName);
            List<string> sets = new List<string>();
            foreach (string key in fields.Keys)
            {
                sets.Add(string.Format("{0}={1}", key, fields[key]));
            }
            string sql = string.Format("UPDATE {0} SET {1} WHERE {2}={3}", tableName, string.Join(",", sets), keyFieldName, objectId);
            DataAccess.GetInstance().ExecuteNonQuery(sql, CommandType.Text);
        }

        internal static void DeleteMetadataObject(MetadataType type, int objectId)
        {
            string tableName, keyFieldName;
            Helper.GetTableName(type, out tableName, out keyFieldName);
            string sql = string.Format("DELETE {0} WHERE {1}={2}", tableName, keyFieldName, objectId);
            DataAccess.GetInstance().ExecuteNonQuery(sql, CommandType.Text);
        }
    }
}
