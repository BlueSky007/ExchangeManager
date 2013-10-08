CREATE PROCEDURE [dbo].[FunctionTree_GetData]
(
	@userId	UNIQUEIDENTIFIER,
	@language	NVARCHAR(50)
)
AS 
BEGIN
	DECLARE @categorys TABLE 
	(
		Id	INT,
		[Description]	NVARCHAR(50)
	)
	
	DECLARE @modules TABLE 
	(
		Id	INT,
		parentId	INT,
		[Description]	NVARCHAR(50)
	)
	
	DECLARE @IsAdmin BIT = 0
	IF((SELECT r.RoleName FROM dbo.UserInRole ui INNER JOIN dbo.Roles r ON r.Id = ui.RoleId WHERE ui.UserId=@userId)='admin')
	SET @IsAdmin=1
	
	IF(@IsAdmin=0)
	BEGIN
		INSERT INTO @categorys(Id,[Description])
		SELECT f3.Id,(CASE WHEN @language='CHT' THEN f3.NameCHT ELSE (CASE WHEN @language='CHS' THEN f3.NameCHS ELSE f3.NameENG END) END)
		FROM dbo.RoleFunctionPermission rfp
			INNER JOIN dbo.[Function] f ON f.Id=rfp.FunctionId
			INNER JOIN dbo.[Function] f2 ON f2.Id=f.ParentId
			INNER JOIN dbo.[Function] f3 ON f3.Id=f2.ParentId
		WHERE f3.ParentId=0	
		
		INSERT INTO @modules(Id,parentId,[Description])
		SELECT f2.Id,f2.ParentId,(CASE WHEN @language='CHT' THEN f2.NameCHT ELSE (CASE WHEN @language='CHS' THEN f2.NameCHS ELSE f2.NameENG END) END)
		FROM dbo.RoleFunctionPermission rfp
			INNER JOIN dbo.[Function] f ON f.Id=rfp.FunctionId
			INNER JOIN dbo.[Function] f2 ON f2.Id=f.ParentId
	END
	ELSE
	BEGIN
		INSERT INTO @categorys(Id,[Description])
		SELECT f.Id,(CASE WHEN @language='CHT' THEN f.NameCHT ELSE (CASE WHEN @language='CHS' THEN f.NameCHS ELSE f.NameENG END) END) 
		FROM dbo.[Function] f
		WHERE f.ParentId=0
		
		INSERT INTO @modules(Id,[Description],parentId)
		SELECT f.Id,(CASE WHEN @language='CHT' THEN f.NameCHT ELSE (CASE WHEN @language='CHS' THEN f.NameCHS ELSE f.NameENG END) END),f.ParentId
		FROM dbo.[Function] f
			INNER JOIN @categorys c ON c.Id=f.ParentId
	END
		
	SELECT * FROM @categorys
	SELECT Id,[Decription],parentId FROM @modules
END