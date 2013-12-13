CREATE PROCEDURE [dbo].[User_AddNew]
(
	@userId	UNIQUEIDENTIFIER,
	@userName	NVARCHAR(255),
	@password	NVARCHAR(255),
	@roles		NVARCHAR(MAX)
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
	
	INSERT INTO dbo.Users(Id,[Name],[Password]) VALUES(@userId,@userName,@password)
	SET @error=@@ERROR
	IF @error<>0 GOTO P_Falied
		
	INSERT INTO dbo.UserInRole(UserId,RoleId)
	SELECT @userId,r.RoleId
	FROM @RoleTable r
	SET @error=@@ERROR
	IF @error<>0 GOTO P_Falied
	
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