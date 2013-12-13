CREATE PROCEDURE [dbo].[FunctionTree_GetData]
(
	@userId	UNIQUEIDENTIFIER,
	@language	NVARCHAR(50)
)
AS 
BEGIN
	DECLARE @categorys TABLE 
	(
		CategoryId	INT,
		Code	NVARCHAR(50),
		[Description]	NVARCHAR(50)
	)
	
	DECLARE @modules TABLE 
	(
		ModuleId	INT,
		parentCode	NVARCHAR(50),
		Code	NVARCHAR(50),
		[Description]	NVARCHAR(50)
	)		
	
	DECLARE @IsAdmin BIT = 0
	IF(EXISTS(SELECT r.RoleName FROM dbo.UserInRole ui INNER JOIN dbo.Roles r ON r.Id = ui.RoleId WHERE ui.UserId=@userId AND r.RoleName='admin'))
	SET @IsAdmin=1
	
	IF(@IsAdmin=0)
	BEGIN
		DECLARE @AllPermission TABLE 
		(
			Id	INT,
			ParentId	INT,
			[Status]	BIT
		)
		INSERT INTO @AllPermission(Id,ParentId)
		SELECT pt.Id,pt.ParentId
		FROM dbo.PermissionTarget pt
		WHERE pt.TargetType = 1 AND pt.[Level]=3
		
		UPDATE @AllPermission
		SET [Status] = rp.[Status]
		FROM @AllPermission ap
			INNER JOIN dbo.RolePermission rp ON rp.TargetId = ap.Id
			INNER JOIN UserInRole uir ON uir.RoleId = rp.RoleId
		WHERE uir.UserId=@userId
		
		IF(EXISTS(SELECT * FROM @AllPermission WHERE [Status] IS NULL))
		BEGIN
			UPDATE @AllPermission
			SET [Status] = rp.[Status]
			FROM @AllPermission ap 
				INNER JOIN dbo.PermissionTarget pt ON pt.Id=ap.ParentId
				INNER JOIN dbo.RolePermission rp ON rp.TargetId = pt.Id
				INNER JOIN UserInRole uir ON uir.RoleId = rp.RoleId
			WHERE uir.UserId=@userId AND ap.[Status] IS NULL
		END			
		IF(EXISTS(SELECT * FROM @AllPermission WHERE [Status] IS NULL))
		BEGIN
			UPDATE @AllPermission
			SET [Status] = rp.[Status]
			FROM @AllPermission ap 
				INNER JOIN dbo.PermissionTarget pt ON pt.Id=ap.ParentId
				INNER JOIN dbo.PermissionTarget pt2 ON pt2.Id=pt.ParentId
				INNER JOIN dbo.RolePermission rp ON rp.TargetId = pt2.Id
				INNER JOIN UserInRole uir ON uir.RoleId = rp.RoleId
			WHERE uir.UserId=@userId AND ap.[Status] IS NULL
		END
		
		INSERT INTO @categorys(CategoryId,Code,[Description])
		SELECT pt1.Id,pt2.Code,(CASE WHEN @language='CHT' THEN fd.NameCHT ELSE (CASE WHEN @language='CHS' THEN fd.NameCHS ELSE fd.NameENG END) END)
		FROM @AllPermission ap 
			INNER JOIN dbo.PermissionTarget pt1 ON pt1.Id=ap.ParentId
			INNER JOIN dbo.PermissionTarget pt2 ON pt2.Id=pt1.ParentId
			INNER JOIN dbo.FunctionDescription fd ON fd.FunctionId=pt1.ParentId
		WHERE ap.[Status]=1
		
		INSERT INTO @modules(ModuleId,parentCode,Code,[Description])
		SELECT pt1.Id,pt2.Code,pt1.Code,(CASE WHEN @language='CHT' THEN fd.NameCHT ELSE (CASE WHEN @language='CHS' THEN fd.NameCHS ELSE fd.NameENG END) END)
		FROM @AllPermission ap 
			INNER JOIN dbo.PermissionTarget pt1 ON pt1.Id=ap.ParentId
			INNER JOIN dbo.PermissionTarget pt2 ON pt2.Id=pt1.ParentId
			INNER JOIN dbo.FunctionDescription fd ON fd.FunctionId=pt1.ParentId
		WHERE ap.[Status]=1
	END
	ELSE
	BEGIN
		INSERT INTO @categorys(CategoryId,Code,[Description])
		SELECT pt.Id,pt.Code,(CASE WHEN @language='CHT' THEN fd.NameCHT ELSE (CASE WHEN @language='CHS' THEN fd.NameCHS ELSE fd.NameENG END) END)
		FROM dbo.PermissionTarget pt
			INNER JOIN dbo.FunctionDescription fd ON fd.FunctionId=pt.Id
		WHERE pt.TargetType = 1 AND pt.[Level]=1
		
		INSERT INTO @modules(ModuleId,parentCode,Code,[Description])
		SELECT pt.Id,pt2.Code,pt.Code,(CASE WHEN @language='CHT' THEN fd.NameCHT ELSE (CASE WHEN @language='CHS' THEN fd.NameCHS ELSE fd.NameENG END) END)
		FROM dbo.PermissionTarget pt
			INNER JOIN dbo.PermissionTarget pt2 ON pt2.Id=pt.ParentId
			INNER JOIN dbo.FunctionDescription fd ON fd.FunctionId=pt.Id
		WHERE pt.TargetType = 1 AND pt.[Level]=2
	END
		
	SELECT DISTINCT CategoryId,Code AS CategoryCode, [Description] FROM @categorys
	SELECT DISTINCT ModuleId,Code AS ModuleCode,[Description],parentCode FROM @modules
END
