CREATE PROCEDURE [dbo].[GetAllRoleData]
(
	@language NVARCHAR(32)
)
AS
BEGIN
	SELECT r.Id,r.RoleName
	FROM dbo.Roles r
	
	DECLARE @AllFunctionPermission	TABLE 
	(
		RoleId			INT,
		FunctionId		INT,
		ParentId		INT,
		Code			NVARCHAR(50),
		ParentCode		NVARCHAR(50),
		[Description]	NVARCHAR(50),
		[Level]			INT,
		IsAllow			BIT
	)
	INSERT INTO @AllFunctionPermission(RoleId,FunctionId,Code,ParentId,[Description],[Level],IsAllow)
	SELECT rp.RoleId,pt.Id,pt.Code,pt.ParentId,(CASE @language WHEN 'CHT' THEN fd.NameCHT WHEN 'CHS' THEN fd.NameCHS ELSE fd.NameENG END),pt.[Level],rp.[Status] 
	FROM dbo.RolePermission rp
		INNER JOIN dbo.PermissionTarget pt ON pt.Id = rp.TargetId
		INNER JOIN dbo.FunctionDescription fd ON fd.FunctionId = pt.Id
	WHERE pt.TargetType = 1
	
	UPDATE @AllFunctionPermission
	SET ParentCode = pt.Code 
	FROM @AllFunctionPermission ap 
		INNER JOIN dbo.PermissionTarget pt ON pt.Id = ap.ParentId
	
	SELECT RoleId, FunctionId, Code,[Level], ParentId, [Description], IsAllow 
	FROM @AllFunctionPermission
	
	DECLARE @AllDataPermission	TABLE 
	(
		RoleId	INT,
		TargetId	INT,
		Code		NVARCHAR(50),
		ParentId	INT,
		IExchangeCode	NVARCHAR(50),
		DataObjectId UNIQUEIDENTIFIER,
		IsAllow	BIT
	)
	INSERT INTO @AllDataPermission(RoleId,TargetId, Code, ParentId, IsAllow)
	SELECT rp.RoleId,rp.TargetId,pt.Code,pt.ParentId,rp.[Status]
	FROM dbo.RolePermission rp
		INNER JOIN dbo.PermissionTarget pt ON pt.Id = rp.TargetId
	WHERE pt.TargetType = 2
	
	UPDATE @AllDataPermission
	SET DataObjectId = pt.DataObjectId
	FROM @AllDataPermission ad 
		INNER JOIN dbo.PermissionTarget pt ON pt.Id = ad.TargetId
		
	UPDATE @AllDataPermission
	SET IExchangeCode = dbo.FV_GetDataPermissionExchangeCode(ad.TargetId)
	FROM @AllDataPermission ad 
		
	SELECT RoleId,TargetId, Code, ParentId, DataObjectId, IsAllow,IExchangeCode
	FROM @AllDataPermission
	
END

