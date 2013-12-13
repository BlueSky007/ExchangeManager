CREATE PROCEDURE [dbo].[Roles_Update]
(
	@roleId		INT,
	@roleName	NVARCHAR(255) = NULL,
	@permissionTarget dbo.PermissionTargetType READONLY
)
AS
BEGIN
	DECLARE @tranCount INT,
			@error INT =0
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
	SET @tranCount=@@tranCount
	IF @tranCount=0
		BEGIN TRAN Tran1
	ELSE
		SAVE TRAN Tran1
	
	IF(@roleName IS NOT NULL)
	BEGIN
		UPDATE dbo.Roles
		SET RoleName = @roleName
		WHERE Id=@roleId
		SET @error=@@ERROR
		IF @error<>0 GOTO P_Falied
	END
	
	DECLARE @ExchangeCode	NVARCHAR(50),
			@Level			TINYINT,
			@Id				INT
	
	DECLARE PermissionCursor CURSOR 
	FOR SELECT p.ExchangeCode,p.[Level],p.Id FROM @permissionTarget p WHERE p.TargetType=2
	OPEN PermissionCursor;
	FETCH PermissionCursor INTO @ExchangeCode,@Level,@Id
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF NOT EXISTS(SELECT*FROM dbo.PermissionTarget pt WHERE pt.Code=@ExchangeCode AND pt.[Level]=1 AND pt.TargetType=2)
		BEGIN
			DECLARE @iExchangeId INT
			
			INSERT INTO dbo.PermissionTarget(ParentId, [Level], Code,TargetType)
			VALUES(2,1,@ExchangeCode,2)
			SET @iExchangeId = SCOPE_IDENTITY()
			INSERT INTO  dbo.PermissionTarget(ParentId,[Level],Code,TargetType)
			VALUES(@iExchangeId,2,'Account',2)
			INSERT INTO dbo.PermissionTarget(ParentId, [Level], Code, TargetType)
			VALUES(@iExchangeId,2,'Instrument',2)
		END
		FETCH PermissionCursor INTO @ExchangeCode,@Level,@Id;
	END
	CLOSE PermissionCursor
	DEALLOCATE PermissionCursor
	SET @error=@@ERROR
	IF	@error<>0 GOTO P_Falied
	
	;WITH dataPermission AS 
	(
		SELECT Code=p.Code,[Level]=p.[Level],DataObjectId=p.DataObjectId,[Status]=p.[Status],
		ParentId=dbo.FV_GetDataPermissionParentId(p.[Level],p.ExchangeCode,p.DataObjectType)
		FROM @permissionTarget p
		WHERE p.TargetType = 2
	)
	MERGE dbo.PermissionTarget pt
	USING dataPermission dp ON dp.Code=pt.Code AND dp.ParentId = pt.ParentId AND pt.TargetType = 2 AND dp.[Level]=pt.[Level]
	WHEN NOT MATCHED BY TARGET THEN
		INSERT(Code,ParentId,[Level],DataObjectId,TargetType)
		VALUES(dp.Code,dp.ParentId,dp.[Level],dp.DataObjectId,2);
	SET @error=@@ERROR
	IF	@error<>0 GOTO P_Falied
	
	DELETE FROM dbo.RolePermission WHERE RoleId=@roleId
	SET @error=@@ERROR
	IF	@error<>0 GOTO P_Falied
	
	DECLARE @dataPermission TABLE 
	(
		Id	INT,
		[Status] BIT
	)
	INSERT INTO @dataPermission(Id,[Status])
	SELECT pt.Id,p.[Status]
	FROM @permissionTarget p
		INNER JOIN dbo.PermissionTarget pt ON pt.Code = p.Code AND pt.ParentId=p.ParentId AND pt.[Level] = p.[Level]
	WHERE pt.TargetType=2
	
	INSERT INTO dbo.RolePermission(RoleId, TargetId, [Status])
	SELECT @roleId,d.Id,d.[Status]
	FROM @dataPermission d
	SET @error=@@ERROR
	IF	@error<>0 GOTO P_Falied
	
	INSERT INTO dbo.RolePermission(RoleId, TargetId, [Status])
	SELECT @roleId,p.Id,p.[Status]
	FROM @permissionTarget p
	WHERE p.TargetType = 1
	SET @error=@@ERROR
	IF	@error<>0 GOTO P_Falied
	
P_Succeed:
	IF @tranCount=0	COMMIT TRAN Tran1
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED
	SET @error=0
	GOTO P_End
	  
P_Falied:
	ROLLBACK TRAN Tran1
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED
	GOTO P_End	

P_End:			
	RETURN @error
END
