CREATE FUNCTION [dbo].[FV_GetDataPermissionExchangeCode]
(
	@TargetId	INT
)
RETURNS NVARCHAR(50)
AS
BEGIN
	DECLARE @IExchangeCode	NVARCHAR(50)
	
	DECLARE @Level	TINYINT
	
	SELECT @Level = pt.[Level]
	FROM dbo.PermissionTarget pt
	WHERE pt.Id=@TargetId
	
	IF(@Level = 1)
	BEGIN
		SELECT @IExchangeCode=pt.Code
		FROM dbo.PermissionTarget pt
		WHERE pt.Id=@TargetId
	END
	ELSE IF(@Level = 2)
	BEGIN
		SELECT @IExchangeCode=pt2.Code
		FROM dbo.PermissionTarget pt
			INNER JOIN dbo.PermissionTarget pt2 ON pt2.Id=pt.ParentId
		WHERE pt.Id=@TargetId
	END
	ELSE IF(@Level = 3)
	BEGIN
		SELECT @IExchangeCode=pt3.Code
		FROM dbo.PermissionTarget pt
			INNER JOIN dbo.PermissionTarget pt2 ON pt2.Id=pt.ParentId
			INNER JOIN dbo.PermissionTarget pt3 ON pt3.Id = pt2.ParentId
		WHERE pt.Id=@TargetId
	END
	
	RETURN(@IExchangeCode)
END