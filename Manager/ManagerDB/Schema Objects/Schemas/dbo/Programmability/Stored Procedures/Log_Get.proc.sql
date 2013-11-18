﻿CREATE PROCEDURE [dbo].[Log_Get]
(
   @fromDate   DATETIME,
   @toDate     DATETIME,
   @logType    TINYINT
)
AS
BEGIN
    --LogQuote
    IF @logType = 1
	BEGIN
	     SELECT l.[Id],l.IP,l.[Event],l.ExchangeCode,l.Timestamp,l.UserId,u.Name as UserName, lq.[Lot], lq.[AnswerLot], lq.[Ask] , 
		       lq.[Bid],  lq.[IsBuy] , lq.[CustomerId],lq.[CustomerName],lq.[InstrumentId],lq.[InstrumentCode],lq.[SendTime]
		FROM [dbo].[LogQuote] lq
		INNER JOIN [dbo].[Log] l ON l.Id = lq.Id
		INNER JOIN [dbo].[Users] u ON u.Id = l.UserId
		WHERE l.Timestamp >= @fromDate AND l.Timestamp <= @toDate
	END
	--LogOrder
	ELSE IF @logType = 2    
	BEGIN
	    SELECT l.[Id],l.IP,l.[Event],l.ExchangeCode,l.Timestamp,l.UserId,u.Name as UserName ,lo.[OperationType],lo.[OrderId],lo.[OrderCode],
               lo.[AccountCode],lo.[InstrumentCode],lo.[IsBuy],lo.[IsOpen],lo.[Lot],lo.[SetPrice],lo.[OrderTypeId] ,lo.[OrderRelation],lo.[TransactionCode]
		FROM [dbo].[LogOrder] lo
		INNER JOIN [dbo].[Log] l ON l.Id = lo.Id
		INNER JOIN [dbo].[Users] u ON u.Id = l.UserId
		WHERE l.Timestamp >= @fromDate AND l.Timestamp <= @toDate
	END
	--Log Price
	ELSE IF @logType = 3   
	BEGIN
	    SELECT l.[Id],l.IP,l.[Event],l.ExchangeCode,l.Timestamp,l.UserId,u.Name as UserName,lp.InstrumentId,lp.InstrumentCode,lp.OperationType,lp.Price,lp.Diff
		FROM [dbo].[LogPrice] lp
		INNER JOIN [dbo].[Log] l ON l.Id = lp.Id
		INNER JOIN [dbo].[Users] u ON u.Id = l.UserId
		WHERE l.Timestamp >= @fromDate AND l.Timestamp <= @toDate
	END
	--Log SourceChanged
	ELSE IF @logType = 4   
	BEGIN
	    SELECT l.[Id],l.IP,l.[Event],l.ExchangeCode,l.Timestamp,l.UserId,u.Name as UserName,lsc.FromSourceId,qsf.Name AS FromSourceName,lsc.ToSourceId, qst.Name AS ToSourceName,lsc.IsDefault,lsc.Priority
		FROM [dbo].[LogSourceChange] lsc
		INNER JOIN [dbo].[Log] l ON l.Id = lsc.Id
		INNER JOIN [dbo].[Users] u ON u.Id = l.UserId
		LEFT OUTER JOIN [dbo].[QuotationSource] qsf ON qsf.Id = lsc.FromSourceId
		LEFT OUTER JOIN [dbo].[QuotationSource] qst ON qst.Id = lsc.ToSourceId
		WHERE l.Timestamp >= @fromDate AND l.Timestamp <= @toDate
	END
   
END