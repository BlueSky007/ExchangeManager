CREATE FUNCTION [dbo].[FV_GetDataPermissionParentId]
(
	@Level		TINYINT,
	@IExchangeCode	NVARCHAR(50),
	@DataType	NVARCHAR(20)
)
RETURNS INT
AS
BEGIN
	DECLARE @ParentId INT
	
	IF(@Level = 1)
	BEGIN
		SELECT @ParentId = 2
	END
	ELSE IF (@Level = 2)
	BEGIN
		SELECT @ParentId=pt.Id
		FROM dbo.PermissionTarget pt
		WHERE pt.[Level] = 1 AND pt.Code=@IExchangeCode
	END
	ELSE IF(@Level=3)
	BEGIN
		SELECT @ParentId=pt.Id
		FROM dbo.PermissionTarget pt
			INNER JOIN dbo.PermissionTarget pt2 ON pt2.Id = pt.ParentId
		WHERE pt2.Code=@IExchangeCode AND pt.Code=@DataType
	END
	
	RETURN (@ParentId)
END
