CREATE PROCEDURE dbo.QuotationSource_Set
	@id INT OUTPUT,
	@name NVARCHAR(30) = NULL,
	@authName NVARCHAR(30) = NULL,
	@password NVARCHAR(60) = NULL
AS
BEGIN
	IF @id = 0
	BEGIN
		INSERT INTO dbo.QuotationSource(Name, AuthName, [Password]) VALUES(@name, @authName, @password)
		SELECT @id = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		UPDATE dbo.QuotationSource
		SET [Name] = ISNULL(@name, [Name]),
		    AuthName = ISNULL(@authName, AuthName),
			[Password] = ISNULL(@password, [Password])
	    WHERE Id = @id
	END
END
