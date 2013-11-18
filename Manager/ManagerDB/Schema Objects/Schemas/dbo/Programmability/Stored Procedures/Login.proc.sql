CREATE PROCEDURE [dbo].[Login]
	@UserName NVARCHAR(32), 
	@Password NVARCHAR(255)
AS
BEGIN 
	SELECT u.Id,u.[Password]
	FROM dbo.Users u 
		INNER JOIN dbo.UserInRole uir ON uir.UserId = u.Id
		INNER JOIN dbo.Roles r ON r.Id = uir.RoleId
	WHERE u.[Name]=@UserName
	
	SELECT r.Id,r.RoleName
	FROM dbo.Users u 
		INNER JOIN dbo.UserInRole uir ON uir.UserId = u.Id
		INNER JOIN dbo.Roles r ON r.Id = uir.RoleId
	WHERE u.[Name]=@UserName
	
	DECLARE @IsAdmin BIT = 0
	IF(EXISTS(SELECT r.RoleName FROM dbo.UserInRole ui INNER JOIN dbo.Roles r ON r.Id = ui.RoleId INNER JOIN Users u ON u.Id=ui.UserId WHERE u.[Name]=@UserName AND r.RoleName='admin'))
	SET @IsAdmin=1
	
	IF(@IsAdmin=0)
	BEGIN
		DECLARE @AllDataObject TABLE 
		(
			Id				INT,
			IExchangeCode	NVARCHAR(50),
			DataType		NVARCHAR(50),
			DataObjectId	UNIQUEIDENTIFIER,
			[Status]		BIT
		)
		INSERT INTO @AllDataObject(Id,IExchangeCode,DataType,DataObjectId,[Status])
		SELECT rp.TargetId,dbo.FV_GetDataPermissionExchangeCode(rp.TargetId),(CASE pt.ParentId WHEN 8 THEN 'Account' WHEN 9 THEN 'Instrument' ELSE NULL END),pt.DataObjectId,rp.[Status]
		FROM dbo.RolePermission rp
			INNER JOIN UserInRole uir ON uir.RoleId = rp.RoleId
			INNER JOIN dbo.Users u ON u.Id=uir.UserId
			INNER JOIN dbo.PermissionTarget pt ON pt.Id=rp.TargetId
		WHERE u.[Name]=@UserName AND pt.TargetType = 2
		
		SELECT DISTINCT Id,IExchangeCode,DataType, DataObjectId, [Status]
		FROM @AllDataObject
	END
END



