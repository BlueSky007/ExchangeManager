﻿/*
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
	
	--INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(211,102,'InstrumentManager',2,1)
	--INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(213,102,'QuotePolicy',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(214,102,'SettingParameter',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(215,102,'SettingScheduler',2,1)
	
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(221,103,'QuotationSource',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(222,103,'SourceQuotation',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(223,103,'QuotationMonitor',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(224,103,'AbnormalQuotation',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(225,103,'ExchangeQuotation',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(226,103,'AdjustSpreadSetting',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(227,103,'SourceRelation',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(228,103,'QuotationChartWindow',2,1)

	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(231,104,'Quote',2,1)
	--INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(232,104,'OrderProcess',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(233,104,'LimitBatchProcess',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(234,104,'MooMocProcess',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(235,104,'DQOrderProcess',2,1)

	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(251,105,'OrderSearch',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(252,105,'ExecutedOrder',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(253,105,'OpenInterest',2,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(254,105,'AccountStatus',2,1)

	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(261,106,'LogAuditQuery',2,1)

	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(301,201,'AddUser',3,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(302,201,'EditUser',3,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(303,201,'DeleteUser',3,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(304,202,'AddRole',3,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(305,202,'EditRole',3,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(306,202,'DeleteRole',3,1)

	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(307,226,'AddRelation',3,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(308,226,'SetAdjust',3,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(309,226,'SetSpread',3,1)

	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(310,225,'Modify',3,1)

	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(320,221,'Modify',3,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(321,223,'Modify',3,1)
	INSERT INTO [dbo].[PermissionTarget]([Id],[ParentId],[Code],[Level],TargetType)VALUES(322,227,'Modify',3,1)

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

	--INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (211,N'商品管理',N'商品管理',N'InstrumentManager')
	--INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (213,N'報價策略',N'报价策略',N'QuotePolicy')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (214,N'參數設置',N'参数设置',N'SettingParameter')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (215,N'參數調度',N'参数调度',N'SettingScheduler')

	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (221,N'價格源',N'价格源',N'QuotationSource')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (222,N'源價格監控',N'源价格监控',N'SourceQuotation')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (223,N'價格監控',N'价格監控',N'QuotationMonitor')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (224,N'異常價格管理',N'异常价格管理',N'AbnormalQuotation')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (225,N'Exchange價格管理',N'Exchange价格管理',N'ExchangeQuotation')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (226,N'調水和價差設定',N'调水和价差设定',N'AutoAdjust And Spread Setting')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (227,N'價格源關係',N'价格源关系',N'SourceRelation')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (228,N'報價圖形',N'报价图形',N'QuotationChart')

	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (231,N'詢價',N'询价',N'Quote')
	--INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (232,N'單子處理',N'单子处理',N'OrderProcess')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (233,N'限價單批量處理',N'限价单批量处理',N'LimitBatchProcess')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (234,N'開市收市單處理',N'开市收市单处理',N'MooMocProcess')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (235,N'即市單處理',N'即市单处理',N'DQOrderProcess')

	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (251,N'單子查詢',N'单子查询',N'OrderSearch')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (252,N'成交單',N'成交单',N'ExecutedOrder')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (253,N'持倉匯總',N'持仓汇总',N'OpenInterest')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (254,N'帳戶狀態查詢',N'账户状态查询',N'AccountStatus')

	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (261,N'日誌查詢',N'日志查询',N'LogAuditQuery')

	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (301,N'增加用户',N'增加用戶',N'AddUser')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (302,N'分配角色',N'分配角色',N'EditUserRole')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (303,N'删除用户',N'删除用户',N'DeleteUser')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (304,N'增加角色',N'增加角色',N'AddRole')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (305,N'分配权限',N'分配權限',N'EditPermission')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (306,N'删除角色',N'刪除角色',N'DeleteRole')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (307,N'添加关系',N'添加關係',N'AddRelation')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (308,N'设置調水',N'設置調水',N'SetAdjust')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (309,N'设置价差',N'設置價差',N'SetSpread')

	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (310,N'數據修改',N'数据修改',N'Modify')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (320,N'數據修改',N'数据修改',N'Modify')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (321,N'數據修改',N'数据修改',N'Modify')
	INSERT INTO dbo.[FunctionDescription](FunctionId,NameCHT,NameCHS,NameENG) VALUES (322,N'數據修改',N'数据修改',N'Modify')
END

IF NOT EXISTS(SELECT * FROM Roles r)
BEGIN
	INSERT INTO dbo.Roles(RoleName) VALUES ('admin')
	INSERT INTO dbo.Roles(RoleName) VALUES ('DefaultRole')
	INSERT INTO dbo.RolePermission(RoleId,TargetId,[Status]) VALUES(2,101,1)
	INSERT INTO dbo.RolePermission(RoleId,TargetId,[Status]) VALUES(2,102,1)
	INSERT INTO dbo.RolePermission(RoleId,TargetId,[Status]) VALUES(2,103,1)
	INSERT INTO dbo.RolePermission(RoleId,TargetId,[Status]) VALUES(2,104,1)
	INSERT INTO dbo.RolePermission(RoleId,TargetId,[Status]) VALUES(2,105,1)
	INSERT INTO dbo.RolePermission(RoleId,TargetId,[Status]) VALUES(2,106,1)

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
		SELECT 'InactiveWaitTime',1,8
		UNION SELECT 'EnquiryWaitTime',1,8
		UNION SELECT 'PriceOrderSetting',1,8
		UNION SELECT 'DisablePopup',1,2
		UNION SELECT 'AutoConfirm',1,2
		UNION SELECT 'ConfirmRejectDQOrder',1,2
		UNION SELECT 'LimitStopSummaryIsTimeRange',1,2
		UNION SELECT 'LimitStopSummaryTimeRangeValue',1,8
		UNION SELECT 'LimitStopSummaryPriceRangeValue',1,8
		UNION SELECT 'OriginInactiveTime',2,8
		UNION SELECT 'AlertVariation',2,8
		UNION SELECT 'NormalWaitTime',2,8
		UNION SELECT 'AlertWaitTime',2,8
		UNION SELECT 'MaxDQLot',2,5
		UNION SELECT 'MaxOtherLot',2,5
		UNION SELECT 'DQQuoteMinLot',2,5
		UNION SELECT 'AutoDQMaxLot',2,5
		UNION SELECT 'AutoLmtMktMaxLot',2,5
		UNION SELECT 'AcceptDQVariation',2,8
		UNION SELECT 'AcceptLmtVariation',2,8
		UNION SELECT 'AcceptCloseLmtVariation',2,8
		UNION SELECT 'CancelLmtVariation',2,8
		UNION SELECT 'IsBetterPrice',2,2
		UNION SELECT 'AutoAcceptMaxLot',2,5
		UNION SELECT 'AutoCancelMaxLot',2,5
		UNION SELECT 'AllowedNewTradeSides',2,8
		UNION SELECT 'HitTimes',2,8
		UNION SELECT 'PenetrationPoint',2,8
		UNION SELECT 'PriceValidTime',2,8
		UNION SELECT 'AutoDQDelay',2,8
		UNION SELECT 'HitPriceVariationForSTP',2,8
		UNION SELECT 'MaxDQLot',3,5
		UNION SELECT 'MaxOtherLot',3,5
		UNION SELECT 'DQQuoteMinLot',3,5
		UNION SELECT 'AutoDQMaxLot',3,5
		UNION SELECT 'AutoLmtMktMaxLot',3,5
		UNION SELECT 'AcceptDQVariation',3,8
		UNION SELECT 'AcceptLmtVariation',3,8
		UNION SELECT 'AcceptCloseLmtVariation',3,8
		UNION SELECT 'CancelLmtVariation',3,8
		UNION SELECT 'IsBetterPrice',3,3
		UNION SELECT 'AutoAcceptMaxLot',3,5
		UNION SELECT 'AutoCancelMaxLot',3,5
		UNION SELECT 'AllowedNewTradeSides',3,8
		UNION SELECT 'HitTimes',3,8
		UNION SELECT 'PenetrationPoint',3,8
		UNION SELECT 'PriceValidTime',3,8
		UNION SELECT 'AutoDQDelay',3,8
		UNION SELECT 'HitPriceVariationForSTP',3,8
END

IF NOT EXISTS(SELECT * FROM [dbo].[SoundDefine])
BEGIN
	   INSERT INTO [dbo].[SoundDefine]([SoundKey] ,[SoundType])
		SELECT 'DQNewOrder',0
		UNION SELECT 'DQDealerIntervene',0
		UNION SELECT 'DQCancelOrder',0
		UNION SELECT 'DQTradeSucceed',0
		UNION SELECT 'DQTradeFailed',0
		UNION SELECT 'DQAlertHiLo',0
		UNION SELECT 'LimitNewOrder',1
		UNION SELECT 'LimitDealerIntervene',1
		UNION SELECT 'LimitCancelOrderRequest',1
		UNION SELECT 'LimitCancelOrder',1
		UNION SELECT 'LimitTradeSucceed',1
		UNION SELECT 'LimitTradeFailed',1
		UNION SELECT 'LimitHit',1
		UNION SELECT 'OutOfRange',2
		UNION SELECT 'Inactive',2
		UNION SELECT 'Enquiry',2
END
