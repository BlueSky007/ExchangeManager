CREATE PROCEDURE [dbo].[Role_Delete]
(
	@roleId	INT
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
	
	DECLARE @user TABLE
	(
		UserId	UNIQUEIDENTIFIER
	)
	INSERT INTO @user(UserId)
	SELECT uir.UserId
	FROM dbo.UserInRole uir
	WHERE uir.RoleId=@roleId	
	SET @error=@@ERROR
	IF	@error<>0 GOTO P_Falied
	
	DELETE FROM dbo.RolePermission WHERE RoleId=@roleId
	SET @error=@@ERROR
	IF	@error<>0 GOTO P_Falied
	
	DELETE FROM dbo.UserInRole WHERE RoleId=@roleId
	SET @error=@@ERROR
	IF	@error<>0 GOTO P_Falied
	
	DELETE FROM dbo.Roles WHERE Id=@roleId
	SET @error=@@ERROR
	IF	@error<>0 GOTO P_Falied
	
	DELETE FROM @user WHERE EXISTS(SELECT* FROM dbo.UserInRole uir WHERE uir.UserId=UserId)
	
	INSERT INTO dbo.UserInRole(UserId, RoleId)
	SELECT u.UserId,2
	FROM @user u
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
