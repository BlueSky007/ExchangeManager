CREATE PROCEDURE [dbo].[GetAllRoleData]
(
	@language NVARCHAR(32)
)
AS
BEGIN
	SELECT r.Id,r.RoleName
	FROM dbo.Roles r
	
	SELECT rfp.RoleId,f3.Id AS CategoryId,f2.Id AS ModuleId,f.Id AS OperationId,(CASE WHEN @language='CHT' THEN f.NameCHT ELSE (CASE WHEN @language='CHS' THEN f.NameCHS ELSE f.NameENG END) END) AS OperationName
	FROM dbo.RoleFunctionPermission rfp 
		INNER JOIN dbo.[Function] f ON f.Id=rfp.FunctionId
		INNER JOIN dbo.[Function] f2 ON f2.Id=f.ParentId
		INNER JOIN dbo.[Function] f3 ON f3.Id=f2.ParentId
	WHERE f3.ParentId=0
	
	SELECT rdp.RoleId,rdp.IExchangeCode,rdp.DataObjectType,rdp.DataObjectId,rdp.DataObjectDescription
	FROM dbo.RoleDataPermission rdp
END
