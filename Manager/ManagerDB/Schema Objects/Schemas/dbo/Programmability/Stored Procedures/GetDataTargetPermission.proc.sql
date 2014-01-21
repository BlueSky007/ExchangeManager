CREATE PROCEDURE [dbo].[GetDataTargetPermission]
(
	@userId	UNIQUEIDENTIFIER,
	@exchangeCode NVARCHAR(50),
	@type NVARCHAR(20),
	@groupId UNIQUEIDENTIFIER
)
AS
BEGIN
	DECLARE @hasPermission TINYINT,
			@status BIT	
	
	SELECT @status = rp.[Status]
	FROM dbo.RolePermission rp
		INNER JOIN dbo.PermissionTarget pt ON pt.Id = rp.TargetId
		INNER JOIN UserInRole uir ON uir.RoleId = rp.RoleId
	WHERE uir.UserId = @userId AND pt.TargetType=2 AND pt.DataObjectId = @groupId 
	
	IF(@status IS NULL)
	BEGIN
		SELECT @status = rp.[Status]
		FROM dbo.RolePermission rp
			INNER JOIN dbo.PermissionTarget pt ON pt.Id = rp.TargetId
			INNER JOIN dbo.PermissionTarget pt2 ON pt.ParentId = pt2.Id
			INNER JOIN UserInRole uir ON uir.RoleId = rp.RoleId
		WHERE uir.UserId = @userId AND pt.TargetType=2 AND pt.Code = @type AND pt2.Code = @exchangeCode AND pt.[Level] = 2
		IF(@status IS NULL)
		BEGIN
			SELECT @status = rp.[Status]
			FROM dbo.RolePermission rp
				INNER JOIN dbo.PermissionTarget pt ON pt.Id = rp.TargetId
				INNER JOIN dbo.PermissionTarget pt2 ON pt.ParentId = pt2.Id
				INNER JOIN UserInRole uir ON uir.RoleId = rp.RoleId
			WHERE uir.UserId = @userId AND pt.TargetType=2 AND pt.Code = @type AND pt2.Code = @exchangeCode 
		END 
	END
	
	SELECT @status
END