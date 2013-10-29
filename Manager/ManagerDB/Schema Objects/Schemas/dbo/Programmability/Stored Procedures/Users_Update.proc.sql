CREATE PROCEDURE [dbo].[Users_Update]
(
	@userId	UNIQUEIDENTIFIER,
	@userName	NVARCHAR(255) = NULL,
	@password	NVARCHAR(255) = NULL,
	@roles		NVARCHAR(MAX) = NULL
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
	
	IF(@userName IS NOT NULL)
	BEGIN
		UPDATE dbo.Users
		SET [Name] = @userName
		WHERE Id=@userId
		SET @error=@@ERROR
		IF @error<>0 GOTO P_Falied
	END
	
	IF(@password IS NOT NULL)
	BEGIN
		UPDATE dbo.Users
		SET [Password] = @password
		WHERE id=@userId
		SET @error=@@ERROR
		IF @error<>0 GOTO P_Falied
	END
	
	IF(@roles IS NOT NULL)
	BEGIN
		DECLARE @RoleTable	TABLE 
		(
			RoleId	INT
		)
		DECLARE @CurrentIndex int;
		DECLARE @NextIndex int;
		DECLARE @ReturnText nvarchar(max);
		SELECT @CurrentIndex=1;
		WHILE(@CurrentIndex<=len(@roles))    
		BEGIN
			SELECT @NextIndex=charindex(',',@roles,@CurrentIndex);
			IF(@NextIndex=0 OR @NextIndex IS NULL)
			SELECT @NextIndex=len(@roles)+1;
			SELECT @ReturnText=substring(@roles,@CurrentIndex,@NextIndex-@CurrentIndex);
			INSERT INTO @RoleTable(RoleId) VALUES(@ReturnText);
			SELECT @CurrentIndex=@NextIndex+1;
			SET @error=@@ERROR
			IF @error<>0 GOTO P_Falied
		END
		
		DELETE FROM dbo.UserInRole
		WHERE UserId=@userId
		SET @error=@@ERROR
		IF @error<>0 GOTO P_Falied
		
		INSERT INTO dbo.UserInRole(UserId,RoleId)
		SELECT @userId,r.RoleId
		FROM @RoleTable r
		SET @error=@@ERROR
		IF @error<>0 GOTO P_Falied
	END
		
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