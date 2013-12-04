using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Manager.Common.QuotationEntities;

namespace ManagerService.DataAccess
{
    public class QuotationData
    {
        internal static void GetQuotationMetadata(
            Dictionary<string, QuotationSource> quotationSources,
            Dictionary<int, Instrument> instruments,
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
                    instrument.IsDerivative = (bool)reader["IsDerivative"];
                    instrument.InactiveTime = reader["InactiveTime"] == DBNull.Value ? null : (int?)reader["InactiveTime"];
                    instrument.UseWeightedPrice = reader["UseWeightedPrice"] == DBNull.Value ? null : (bool?)reader["UseWeightedPrice"];
                    instrument.IsSwitchUseAgio = reader["IsSwitchUseAgio"] == DBNull.Value ? null : (bool?)reader["IsSwitchUseAgio"];
                    instrument.AgioSeconds = reader["AgioSeconds"] == DBNull.Value ? null : (int?)reader["AgioSeconds"];
                    instrument.LeastTicks = reader["LeastTicks"] == DBNull.Value ? null : (int?)reader["LeastTicks"];
                    instruments.Add(instrument.Id, instrument);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    InstrumentSourceRelation relation = new InstrumentSourceRelation();
                    relation.Id = (int)reader["Id"];
                    relation.SourceId = (int)reader["SourceId"];
                    relation.SourceSymbol = (string)reader["SourceSymbol"];
                    relation.InstrumentId = (int)reader["InstrumentId"];
                    relation.Inverted = (bool)reader["Inverted"];
                    relation.IsActive = (bool)reader["IsActive"];
                    relation.IsDefault = (bool)reader["IsDefault"];
                    relation.Priority = (int)reader["Priority"];
                    relation.SwitchTimeout = (int)reader["SwitchTimeout"];
                    relation.AdjustPoints = (int)reader["AdjustPoints"];
                    relation.AdjustIncrement = (int)reader["AdjustIncrement"];
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
                    derivativeRelation.Id = (int)reader["InstrumentId"];
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
                    derivativeRelations.Add(derivativeRelation.Id, derivativeRelation);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    PriceRangeCheckRule priceRangeCheckRule = new PriceRangeCheckRule();
                    priceRangeCheckRule.Id = (int)reader["InstrumentId"];
                    priceRangeCheckRule.DiscardOutOfRangePrice = (bool)reader["DiscardOutOfRangePrice"];
                    priceRangeCheckRule.OutOfRangeType = (OutOfRangeType)(byte)reader["OutOfRangeType"];
                    priceRangeCheckRule.ValidVariation = (int)reader["ValidVariation"];
                    priceRangeCheckRule.OutOfRangeWaitTime = (int)reader["OutOfRangeWaitTime"];
                    priceRangeCheckRule.OutOfRangeCount = (int)reader["OutOfRangeCount"];
                    priceRangeCheckRules.Add(priceRangeCheckRule.Id, priceRangeCheckRule);
                }
                reader.NextResult();
                while(reader.Read())
                {
                    WeightedPriceRule weightedPriceRule = new WeightedPriceRule();
                    weightedPriceRule.Id = (int)reader["InstrumentId"];
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
                    weightedPriceRule.AskAverageWeight = (int)reader["AskAvarageWeight"];
                    weightedPriceRule.BidAverageWeight = (int)reader["BidAvarageWeight"];
                    weightedPriceRule.LastAverageWeight = (int)reader["LastAvarageWeight"];
                    weightedPriceRule.AskAdjust = (decimal)reader["AskAdjust"];
                    weightedPriceRule.BidAdjust = (decimal)reader["BidAdjust"];
                    weightedPriceRule.LastAdjust = (decimal)reader["LastAdjust"];
                    weightedPriceRules.Add(weightedPriceRule.Id, weightedPriceRule);
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
                new SqlParameter("@last", quotation.Last.HasValue ? (object)quotation.Last.Value : DBNull.Value),
                new SqlParameter("@high", quotation.High.HasValue ? (object)quotation.High.Value : DBNull.Value),
                new SqlParameter("@low", quotation.Low.HasValue ? (object)quotation.Low.Value : DBNull.Value));
        }

        public static int AddInstrument(InstrumentData instrumentData)
        {
            int instrumentId; 
            using (TransactionScope scope = new TransactionScope())
            {
                instrumentId = QuotationData.AddMetadataObject(instrumentData.Instrument);
                if (instrumentData.Instrument.IsDerivative)
                {
                    instrumentData.DerivativeRelation.Id = instrumentId;
                    QuotationData.AddMetadataObject(instrumentData.DerivativeRelation);
                }
                else
                {
                    instrumentData.PriceRangeCheckRule.Id = instrumentId;
                    QuotationData.AddMetadataObject(instrumentData.PriceRangeCheckRule);
                    if (instrumentData.WeightedPriceRule != null)
                    {
                        instrumentData.WeightedPriceRule.Id = instrumentId;
                        QuotationData.AddMetadataObject(instrumentData.WeightedPriceRule);
                    }
                }
                scope.Complete();
            }
            return instrumentId;
        }

        public static int AddMetadataObject(QuotationSource entity)
        {
            string sql = "INSERT QuotationSource(Name,AuthName,Password) VALUES (@name,@authName,@password);SELECT SCOPE_IDENTITY()";
            int objectId = (int)(decimal)DataAccess.GetInstance().ExecuteScalar(sql, CommandType.Text,
                new SqlParameter("@name", entity.Name),
                new SqlParameter("@authName", entity.AuthName),
                new SqlParameter("@password", entity.Password));
            return objectId;
        }

        public static int AddMetadataObject(Instrument entity)
        {
            string sql = "INSERT Instrument(Code,DecimalPlace,InactiveTime,UseWeightedPrice,IsDerivative,IsSwitchUseAgio,AgioSeconds,LeastTicks) VALUES (@code,@decimalPlace,@inactiveTime,@useWeightedPrice,@isDerivative,@isSwitchUseAgio,@agioSeconds,@leastTicks);SELECT SCOPE_IDENTITY()";
            int objectId = (int)(decimal)DataAccess.GetInstance().ExecuteScalar(sql, CommandType.Text,
                new SqlParameter("@code", entity.Code),
                new SqlParameter("@decimalPlace", entity.DecimalPlace),
                new SqlParameter("@inactiveTime", entity.InactiveTime.HasValue ? (object)entity.InactiveTime : DBNull.Value),
                new SqlParameter("@useWeightedPrice", entity.UseWeightedPrice.HasValue ? (object)entity.UseWeightedPrice : DBNull.Value),
                new SqlParameter("@isDerivative", entity.IsDerivative),
                new SqlParameter("@isSwitchUseAgio", entity.IsSwitchUseAgio.HasValue ? (object)entity.IsSwitchUseAgio : DBNull.Value),
                new SqlParameter("@agioSeconds", entity.AgioSeconds.HasValue ? (object)entity.AgioSeconds : DBNull.Value),
                new SqlParameter("@leastTicks", entity.LeastTicks.HasValue ? (object)entity.LeastTicks : DBNull.Value));
            return objectId;
        }

        public static int AddMetadataObject(InstrumentSourceRelation entity)
        {
            string sql = "INSERT InstrumentSourceRelation(SourceId,SourceSymbol,InstrumentId,Inverted,IsActive,IsDefault,Priority,SwitchTimeout,AdjustPoints,AdjustIncrement) VALUES (@sourceId,@sourceSymbol,@instrumentId,@inverted,@isActive,@isDefault,@priority,@switchTimeout,@adjustPoints,@adjustIncrement);SELECT SCOPE_IDENTITY()";
            int objectId = (int)(decimal)DataAccess.GetInstance().ExecuteScalar(sql, CommandType.Text,
                new SqlParameter("@sourceId", entity.SourceId),
                new SqlParameter("@sourceSymbol", entity.SourceSymbol),
                new SqlParameter("@instrumentId", entity.InstrumentId),
                new SqlParameter("@inverted", entity.Inverted),
                new SqlParameter("@isActive", entity.IsActive),
                new SqlParameter("@isDefault", entity.IsDefault),
                new SqlParameter("@priority", entity.Priority),
                new SqlParameter("@switchTimeout", entity.SwitchTimeout),
                new SqlParameter("@adjustPoints", entity.AdjustPoints),
                new SqlParameter("@adjustIncrement", entity.AdjustIncrement));
            return objectId;
        }

        public static void AddMetadataObject(DerivativeRelation entity)
        {
            string sql = "INSERT DerivativeRelation(InstrumentId,UnderlyingInstrument1Id,UnderlyingInstrument1IdInverted,UnderlyingInstrument2Id,AdjustPoints,AdjustIncrement,AskOperand1Type,AskOperator1Type,AskOperand2Type,AskOperator2Type,AskOperand3,BidOperand1Type,BidOperator1Type,BidOperand2Type,BidOperator2Type,BidOperand3,LastOperand1Type,LastOperator1Type,LastOperand2Type,LastOperator2Type,LastOperand3) VALUES (@instrumentId,@underlyingInstrument1Id,@underlyingInstrument1IdInverted,@underlyingInstrument2Id,@adjustPoints,@adjustIncrement,@askOperand1Type,@askOperator1Type,@askOperand2Type,@askOperator2Type,@askOperand3,@bidOperand1Type,@bidOperator1Type,@bidOperand2Type,@bidOperator2Type,@bidOperand3,@lastOperand1Type,@lastOperator1Type,@lastOperand2Type,@lastOperator2Type,@lastOperand3);SELECT SCOPE_IDENTITY()";
            DataAccess.GetInstance().ExecuteScalar(sql, CommandType.Text,
                new SqlParameter("@instrumentId", entity.Id),
                new SqlParameter("@underlyingInstrument1Id", entity.UnderlyingInstrument1Id),
                new SqlParameter("@underlyingInstrument1IdInverted", entity.UnderlyingInstrument1IdInverted),
                new SqlParameter("@underlyingInstrument2Id", entity.UnderlyingInstrument2Id),
                new SqlParameter("@adjustPoints", entity.AdjustPoints),
                new SqlParameter("@adjustIncrement", entity.AdjustIncrement),
                new SqlParameter("@askOperand1Type", entity.AskOperand1Type),
                new SqlParameter("@askOperator1Type", entity.AskOperator1Type),
                new SqlParameter("@askOperand2Type", entity.AskOperand2Type),
                new SqlParameter("@askOperator2Type", entity.AskOperator2Type),
                new SqlParameter("@askOperand3", entity.AskOperand3),
                new SqlParameter("@bidOperand1Type", entity.BidOperand1Type),
                new SqlParameter("@bidOperator1Type", entity.BidOperator1Type),
                new SqlParameter("@bidOperand2Type", entity.BidOperand2Type),
                new SqlParameter("@bidOperator2Type", entity.BidOperator2Type),
                new SqlParameter("@bidOperand3", entity.BidOperand3),
                new SqlParameter("@lastOperand1Type", entity.LastOperand1Type),
                new SqlParameter("@lastOperator1Type", entity.LastOperator1Type),
                new SqlParameter("@lastOperand2Type", entity.LastOperand2Type),
                new SqlParameter("@lastOperator2Type", entity.LastOperator2Type),
                new SqlParameter("@lastOperand3", entity.LastOperand3));
        }
        public static void AddMetadataObject(PriceRangeCheckRule entity)
        {
            string sql = "INSERT PriceRangeCheckRule(InstrumentId,DiscardOutOfRangePrice,OutOfRangeType,ValidVariation,OutOfRangeWaitTime,OutOfRangeCount) VALUES (@instrumentId,@discardOutOfRangePrice,@outOfRangeType,@validVariation,@outOfRangeWaitTime,@outOfRangeCount)";
            DataAccess.GetInstance().ExecuteScalar(sql, CommandType.Text,
                new SqlParameter("@instrumentId", entity.Id),
                new SqlParameter("@discardOutOfRangePrice", entity.DiscardOutOfRangePrice),
                new SqlParameter("@outOfRangeType", (byte)entity.OutOfRangeType),
                new SqlParameter("@validVariation", entity.ValidVariation),
                new SqlParameter("@outOfRangeWaitTime", entity.OutOfRangeWaitTime),
                new SqlParameter("@outOfRangeCount", entity.OutOfRangeCount));
        }
        
        public static void AddMetadataObject(WeightedPriceRule entity)
        {
            string sql = "INSERT WeightedPriceRule(InstrumentId,Multiplier,AskAskWeight,AskBidWeight,AskLastWeight,BidAskWeight,BidBidWeight,BidLastWeight,LastAskWeight,LastBidWeight,LastLastWeight,AskAvarageWeight,BidAvarageWeight,LastAvarageWeight,AskAdjust,BidAdjust,LastAdjust) VALUES (@instrumentId,@multiplier,@askAskWeight,@askBidWeight,@askLastWeight,@bidAskWeight,@bidBidWeight,@bidLastWeight,@lastAskWeight,@lastBidWeight,@lastLastWeight,@askAvarageWeight,@bidAvarageWeight,@lastAvarageWeight,@askAdjust,@bidAdjust,@lastAdjust);SELECT SCOPE_IDENTITY()";
            DataAccess.GetInstance().ExecuteScalar(sql, CommandType.Text,
                new SqlParameter("@instrumentId", entity.Id),
                new SqlParameter("@multiplier", entity.Multiplier),
                new SqlParameter("@askAskWeight", entity.AskAskWeight),
                new SqlParameter("@askBidWeight", entity.AskBidWeight),
                new SqlParameter("@askLastWeight", entity.AskLastWeight),
                new SqlParameter("@bidAskWeight", entity.BidAskWeight),
                new SqlParameter("@bidBidWeight", entity.BidBidWeight),
                new SqlParameter("@bidLastWeight", entity.BidLastWeight),
                new SqlParameter("@lastAskWeight", entity.LastAskWeight),
                new SqlParameter("@lastBidWeight", entity.LastBidWeight),
                new SqlParameter("@lastLastWeight", entity.LastLastWeight),
                new SqlParameter("@askAvarageWeight", entity.AskAverageWeight),
                new SqlParameter("@bidAvarageWeight", entity.BidAverageWeight),
                new SqlParameter("@lastAvarageWeight", entity.LastAverageWeight),
                new SqlParameter("@askAdjust", entity.AskAdjust),
                new SqlParameter("@bidAdjust", entity.BidAdjust),
                new SqlParameter("@lastAdjust", entity.LastAdjust));
        }

        internal static void UpdateMetadataObject(MetadataType type, int objectId, Dictionary<string, object> fieldsAndValues)
        {
            string tableName, keyFieldName;
            ServiceHelper.GetTableName(type, out tableName, out keyFieldName);
            List<string> sets = new List<string>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            foreach (string key in fieldsAndValues.Keys)
            {
                sets.Add(string.Format("{0}=@{0}", key));
                sqlParameters.Add(new SqlParameter("@" + key, fieldsAndValues[key]));
            }
            string sql = string.Format("UPDATE {0} SET {1} WHERE {2}={3}", tableName, string.Join(",", sets), keyFieldName, objectId);
            DataAccess.GetInstance().ExecuteNonQuery(sql, CommandType.Text, sqlParameters.ToArray());
        }

        internal static void DeleteMetadataObject(MetadataType type, int objectId)
        {
            string tableName, keyFieldName;
            ServiceHelper.GetTableName(type, out tableName, out keyFieldName);
            string sql = string.Format("DELETE {0} WHERE {1}={2}", tableName, keyFieldName, objectId);
            DataAccess.GetInstance().ExecuteNonQuery(sql, CommandType.Text);
        }

        internal static void SwitchActiveSource(int oldRelationId, int newRelationId)
        {
            string sql = "BEGIN TRAN; UPDATE InstrumentSourceRelation SET IsActive = 1 WHERE Id = @newRelationId; UPDATE InstrumentSourceRelation SET IsActive = 0 WHERE Id = @oldRelationId; COMMIT;";
            DataAccess.GetInstance().ExecuteNonQuery(sql, CommandType.Text, new SqlParameter("@oldRelationId", oldRelationId), new SqlParameter("@newRelationId", newRelationId));
        }

        internal static void SwitchDefaultSource(int oldRelationId, int newRelationId)
        {
            string sql = "BEGIN TRAN; UPDATE InstrumentSourceRelation SET IsDefault = 1 WHERE Id = @newRelationId; UPDATE InstrumentSourceRelation SET IsDefault = 0 WHERE Id = @oldRelationId; COMMIT;";
            DataAccess.GetInstance().ExecuteNonQuery(sql, CommandType.Text, new SqlParameter("@oldRelationId", oldRelationId), new SqlParameter("@newRelationId", newRelationId));
        }
    }
}
