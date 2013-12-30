/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

USE [ManagerDB]
GO

IF NOT EXISTS(SELECT * FROM PermissionTarget pt)
BEGIN
	SET IDENTITY_INSERT PermissionTarget ON
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(101,1,'UserManager',1,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(102,1,'Configuration',1,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(103,1,'Quotation',1,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(104,1,'Dealing',1,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(105,1,'QueryReport',1,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(106,1,'LogAudit',1,1)

	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(201,101,'UserManager',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(202,101,'RoleManager',2,1)
	
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(211,102,'InstrumentManager',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(212,102,'QuoationSource',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(213,102,'QuotePolicy',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(214,102,'SettingParameter',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(215,102,'SettingScheduler',2,1)
	
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(221,103,'QuotationSource',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(222,103,'SourceQuotation',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(223,103,'QuotationMonitor',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(224,103,'AbnormalQuotation',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(225,103,'IExchangeQuotation',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(226,103,'AdjustSpreadSetting',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(227,103,'SourceRelation',2,1)

	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(231,104,'Quote',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(232,104,'OrderProcess',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(233,104,'LimitBatchProcess',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(234,104,'MooMocProcess',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(235,104,'DQOrderProcess',2,1)

	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(251,105,'OrderSearch',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(252,105,'ExecutedOrder',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(253,105,'OpenInterest',2,1)

	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(261,106,'LogAuditQuery',2,1)

	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(301,201,'Add',3,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(302,201,'Edit',3,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(303,201,'Delete',3,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(304,202,'Add',3,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(305,202,'Edit',3,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(306,202,'Delete',3,1)

	DBCC CHECKIDENT('PermissionTarget',RESEED, 1000)
END

IF NOT EXISTS(SELECT * FROM dbo.FunctionDescription fd)
BEGIN
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (101,N'用戶管理',N'用户管理',N'UserManager')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (102,N'配置管理',N'配置管理',N'Configuration')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (103,N'價格管理',N'价格管理',N'Quotation')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (104,N'交易管理',N'交易管理',N'Dealing')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (105,N'報表',N'报表',N'QueryReport')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (106,N'日誌',N'日志',N'LogAudit')

	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (201,N'用戶管理',N'用户管理',N'UserManager')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (202,N'角色管理',N'角色管理',N'RoleManager')

	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (211,N'商品管理',N'商品管理',N'InstrumentManager')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (213,N'報價策略',N'报价策略',N'QuotePolicy')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (214,N'參數設置',N'参数设置',N'SettingParameter')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (215,N'參數調度',N'参数调度',N'SettingScheduler')

	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (221,N'價格源',N'价格源',N'QuotationSource')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (222,N'源價格監控',N'源价格监控',N'SourceQuotation')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (223,N'價格監控',N'价格監控',N'QuotationMonitor')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (224,N'異常價格管理',N'异常价格管理',N'AbnormalQuotation')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (225,N'IExchange行情管理',N'IExchange行情管理',N'IExchangeQuotation')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (226,N'調水和價差設定',N'调水和价差设定',N'AutoAdjust And Spread Setting')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (227,N'價格源關係',N'价格源关系',N'SourceRelation')

	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (231,N'詢價',N'询价',N'Quote')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (232,N'單子處理',N'单子处理',N'OrderProcess')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (233,N'限價單批量處理',N'限价单批量处理',N'LimitBatchProcess')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (234,N'開市收市單處理',N'开市收市单处理',N'MooMocProcess')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (235,N'即市單處理',N'即市单处理',N'DQOrderProcess')

	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (251,N'單子查詢',N'单子查询',N'OrderSearch')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (252,N'成交單',N'成交单',N'ExecutedOrder')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (253,N'持倉匯總',N'持仓汇总',N'OpenInterest')

	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (261,N'日誌查詢',N'日志查询',N'LogAuditQuery')

	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (301,N'增加用户',N'增加用戶',N'AddUser')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (302,N'分配角色',N'分配角色',N'EditUserRole')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (303,N'删除用户',N'删除用户',N'DeleteUser')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (304,N'增加角色',N'增加角色',N'AddRole')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (305,N'分配权限',N'分配權限',N'EditPermission')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (306,N'删除角色',N'刪除角色',N'DeleteRole')
END

IF NOT EXISTS(SELECT * FROM Roles r)
BEGIN
	INSERT INTO dbo.Roles(RoleName) VALUES ('admin')

	DECLARE @userId UNIQUEIDENTIFIER
	SET @userId = NEWID()

	INSERT INTO dbo.Users(Id,[Name], [Password]) VALUES(@userId,'admin','29ac25660e3078e87e3097d3822e50d7')
	INSERT INTO dbo.UserInRole(UserId, RoleId) VALUES (@userId,1)
END

IF NOT EXISTS(SELECT * FROM dbo.ParameterDefine)
BEGIN
	   INSERT INTO [dbo].[ParameterDefine]
					   ([ParameterKey]
					   ,[SettingType]
					   ,[SqlDbType])
		SELECT 'InactiveWaitTime',0,8
		UNION SELECT 'EnquiryWaitTime',0,8
		UNION SELECT 'PriceOrderSetting',0,8
		UNION SELECT 'DisablePopup',0,2
		UNION SELECT 'AutoConfirm',0,2
		UNION SELECT 'LimitStopSummaryIsTimeRange',0,2
		UNION SELECT 'LimitStopSummaryTimeRangeValue',0,8
		UNION SELECT 'LimitStopSummaryPriceRangeValue',0,8
		UNION SELECT 'OriginInactiveTime',1,8
		UNION SELECT 'AlertVariation',1,8
		UNION SELECT 'NormalWaitTime',1,8
		UNION SELECT 'AlertWaitTime',1,8
		UNION SELECT 'MaxDQLot',1,5
		UNION SELECT 'MaxOtherLot',1,5
		UNION SELECT 'DQQuoteMinLot',1,5
		UNION SELECT 'AutoDQMaxLot',1,5
		UNION SELECT 'AutoLmtMktMaxLot',1,5
		UNION SELECT 'AcceptDQVariation',1,8
		UNION SELECT 'AcceptLmtVariation',1,8
		UNION SELECT 'AcceptCloseLmtVariation',1,8
		UNION SELECT 'CancelLmtVariation',1,8
		UNION SELECT 'IsBetterPrice',1,2
		UNION SELECT 'AutoAcceptMaxLot',1,5
		UNION SELECT 'AutoCancelMaxLot',1,5
		UNION SELECT 'AllowedNewTradeSides',1,8
		UNION SELECT 'HitTimes',1,8
		UNION SELECT 'PenetrationPoint',1,8
		UNION SELECT 'PriceValidTime',1,8
		UNION SELECT 'AutoDQDelay',1,8
		UNION SELECT 'HitPriceVariationForSTP',1,8
END