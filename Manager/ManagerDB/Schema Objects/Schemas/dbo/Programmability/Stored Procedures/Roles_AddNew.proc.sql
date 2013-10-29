CREATE PROCEDURE [dbo].[Roles_AddNew]
(
	@roleName	NVARCHAR(255),
	@functionPermssions	NVARCHAR(MAX),
	@dataPermissions dbo.DataPermissionsTableType READONLY
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
	
	DECLARE @FunctionPermissionTable TABLE 
	(
		FunctionId	INT
	)
	DECLARE @CurrentIndex int;
    DECLARE @NextIndex int;
    DECLARE @ReturnText nvarchar(max);
	SELECT @CurrentIndex=1;
    WHILE(@CurrentIndex<=len(@functionPermssions))    
	BEGIN
		SELECT @NextIndex=charindex(',',@functionPermssions,@CurrentIndex);
		IF(@NextIndex=0 OR @NextIndex IS NULL)
		SELECT @NextIndex=len(@functionPermssions)+1;
		SELECT @ReturnText=substring(@functionPermssions,@CurrentIndex,@NextIndex-@CurrentIndex);
		INSERT INTO @FunctionPermissionTable(FunctionId) VALUES(@ReturnText);
		SELECT @CurrentIndex=@NextIndex+1;
		SET @error=@@ERROR
		IF @error<>0 GOTO P_Falied
	END	
	DECLARE @roleId INT
	INSERT INTO dbo.Roles(RoleName) VALUES (@roleName)
	SET @error=@@ERROR
	IF	@error<>0 GOTO P_Falied
	
	SELECT @roleId=r.Id FROM dbo.Roles r WHERE r.RoleName=@roleName
	
	INSERT INTO dbo.RoleFunctionPermission(RoleId,FunctionId)
	SELECT @roleId,ft.FunctionId
	FROM @FunctionPermissionTable ft
	SET @error=@@ERROR
	IF	@error<>0 GOTO P_Falied
	
	INSERT INTO dbo.RoleDataPermission(RoleId,IExchangeCode,DataObjectType,DataObjectId,DataObjectDescription)
	SELECT @roleId,d.ExchangeCode,d.DataObjectType,d.DataObjectId,d.DataObjectDescription
	FROM @dataPermissions d	
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
