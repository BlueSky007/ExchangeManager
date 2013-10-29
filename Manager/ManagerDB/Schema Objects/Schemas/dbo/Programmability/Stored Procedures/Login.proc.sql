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
	
	SELECT DISTINCT rdp.DataObjectId,rdp.IExchangeCode,rdp.DataObjectType 
	FROM dbo.Users u
		INNER JOIN dbo.UserInRole uir ON uir.UserId = u.Id
		INNER JOIN dbo.RoleDataPermission rdp ON rdp.RoleId = uir.RoleId
	WHERE u.[Name]=@UserName
END