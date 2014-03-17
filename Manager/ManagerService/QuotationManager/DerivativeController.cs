using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common.QuotationEntities;
using Manager.Common;
using System.Diagnostics;

namespace ManagerService.Quotation
{
    public class DerivativeController
    {
        // Map for InstrumentId - DerivativeRelation
        private Dictionary<int, DerivativeRelation> _DerivativeRelations;

        private LastQuotationManager _LastQuotationManager;
        private int _DeriveRecursiveLevel = 0;

        public DerivativeController(Dictionary<int, DerivativeRelation> derivativeRelations, LastQuotationManager lastQuotationManager)
        {
            this._DerivativeRelations = derivativeRelations;
            this._LastQuotationManager = lastQuotationManager;
        }

        public void Derive(GeneralQuotation quotation, List<GeneralQuotation> quotations)
        {
            this._DeriveRecursiveLevel++;
            if (this._DeriveRecursiveLevel > 50)
            {
                this._DeriveRecursiveLevel = 0;
                throw new Exception(string.Format("DerivativeController.Derive too deep SourceId:{0},InstrumentId:{1},OriginCode:{2}", quotation.SourceId, quotation.InstrumentId, quotation.OriginCode));
            }
            try
            {
                var derivativeRelations = this._DerivativeRelations.Values.Where(r => r.UnderlyingInstrument1Id == quotation.InstrumentId || r.UnderlyingInstrument2Id == quotation.InstrumentId);
                foreach (var relation in derivativeRelations)
                {
                    GeneralQuotation GeneralQuotation = this.Derive(quotation, relation);
                    if (GeneralQuotation != null)
                    {
                        quotations.Add(GeneralQuotation);
                        this.Derive(GeneralQuotation, quotations);
                    }
                }
                this._DeriveRecursiveLevel--;
            }
            catch
            {
                this._DeriveRecursiveLevel = 0;
                throw;
            }
        }

        private GeneralQuotation Derive(GeneralQuotation quotation, DerivativeRelation relation)
        {
            GeneralQuotation quotation1, quotation2 = null;
            if (quotation.InstrumentId == relation.UnderlyingInstrument1Id)
            {
                quotation1 = quotation;
                if (relation.UnderlyingInstrument2Id.HasValue)
                {
                    if (!this._LastQuotationManager.LastAccepted.TryGetLastQuotation(relation.UnderlyingInstrument2Id.Value, out quotation2))
                    {
                        Logger.AddEvent(TraceEventType.Warning, "DerivativeController.Derive can not get Last quotation by instrumentId:{0}", relation.UnderlyingInstrument2Id.Value);
                        return null;
                    }
                }
            }
            else
            {
                quotation2 = quotation;
                if (!this._LastQuotationManager.LastAccepted.TryGetLastQuotation(relation.UnderlyingInstrument1Id, out quotation1))
                {
                    Logger.AddEvent(TraceEventType.Warning, "DerivativeController.Derive can not get Last quotation by instrumentId:{0}", relation.UnderlyingInstrument1Id);
                    return null;
                }
            }

            GeneralQuotation GeneralQuotation = new GeneralQuotation { InstrumentId = relation.Id, SourceId = quotation.SourceId, Timestamp = quotation.Timestamp };
            double price;
            if (this.TryGetPrice(relation.AskOperand1Type, relation.AskOperator1Type,
                relation.AskOperand2Type, relation.AskOperator2Type,
                (double)relation.AskOperand3,
                relation, GeneralQuotation, quotation1, quotation2, out price))
            {
                GeneralQuotation.Ask = price;
            }
            else
            {
                return null;
            }

            if (this.TryGetPrice(relation.BidOperand1Type, relation.BidOperator1Type,
                relation.BidOperand2Type, relation.BidOperator2Type,
                (double)relation.BidOperand3,
                relation, GeneralQuotation, quotation1, quotation2, out price))
            {
                GeneralQuotation.Bid = price;
            }
            else
            {
                return null;
            }

            if (this.TryGetPrice(relation.LastOperand1Type, relation.LastOperator1Type,
                relation.LastOperand2Type, relation.LastOperator2Type,
                (double)relation.LastOperand3,
                relation, GeneralQuotation, quotation1, quotation2, out price))
            {
                GeneralQuotation.Last = price;
            }
            else
            {
                GeneralQuotation.Last = null;
            }

            return GeneralQuotation;
        }

        private bool TryGetPrice(OperandType operand1Type, OperatorType? operator1Type, OperandType? operand2Type, OperatorType operator2Type, double operand3,
            DerivativeRelation relation, GeneralQuotation GeneralQuotation, GeneralQuotation quotation1, GeneralQuotation quotation2, out double price)
        {
            price = 0;
            // operand1
            switch (operand1Type)
            {
                case OperandType.Ask:
                    price = relation.UnderlyingInstrument1IdInverted ? 1 / quotation1.Ask : quotation1.Ask;
                    break;
                case OperandType.Bid:
                    price = relation.UnderlyingInstrument1IdInverted ? 1 / quotation1.Bid : quotation1.Bid;
                    break;
                case OperandType.Last:
                    if (quotation1.Last.HasValue)
                    {
                        price = relation.UnderlyingInstrument1IdInverted ? 1 / quotation1.Last.Value : quotation1.Last.Value;
                    }
                    else
                    {
                        Logger.AddEvent(TraceEventType.Warning, "DerivativeController.Derive operand1 quotation1.Last==null quotation1.instrumentId:{0}", quotation1.InstrumentId);
                        return false;
                    }
                    break;
                default:
                    Logger.AddEvent(TraceEventType.Warning, "DerivativeController.Derive operand1 operand1Type is Invalid, relation.instrumentId:{0}, operand1Type:{1}",
                        relation.Id, operand1Type);
                    return false;
            }

            if (quotation2 != null && operator1Type.HasValue)
            {
                // operand2
                double operand2;
                switch (operand2Type.Value)
                {
                    case OperandType.Ask:
                        operand2 = quotation2.Ask;
                        break;
                    case OperandType.Bid:
                        operand2 = quotation2.Bid;
                        break;
                    case OperandType.Last:
                        if (quotation1.Last.HasValue)
                        {
                            operand2 = quotation2.Last.Value;
                        }
                        else
                        {
                            Logger.AddEvent(TraceEventType.Warning, "DerivativeController.Derive operand2 quotation1.Last==null, quotation2.instrumentId:{0}", quotation2.InstrumentId);
                            return false;
                        }
                        break;
                    default:
                        Logger.AddEvent(TraceEventType.Warning, "DerivativeController.Derive operand2Type is Invalid, relation.instrumentId:{0}, operand2Type:{1}",
                            relation.Id, operand2Type);
                        return false;
                }
                if (operator1Type.Value == OperatorType.Multiply)
                {
                    price *= operand2;
                }
                else
                {
                    price /= operand2;
                }
            }

            // operand3
            if (operator2Type == OperatorType.Multiply)
            {
                price *= operand3;
            }
            else
            {
                price /= operand3;
            }
            return true;
        }
    }
}
