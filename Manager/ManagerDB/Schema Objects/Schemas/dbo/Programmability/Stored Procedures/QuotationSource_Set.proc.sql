CREATE PROCEDURE dbo.QuotationSource_Set
	@id INT OUTPUT,
	@name NVARCHAR(30),
	@authName NVARCHAR(30),
	@password NVARCHAR(60)
AS
BEGIN
	IF @id = 0
	BEGIN
		INSERT INTO dbo.QuotationSource(Name, AuthName, [Password]) VALUES(@name, @authName, @password)
		SELECT @id = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		UPDATE dbo.QuotationSource SET [Name] = @name, AuthName = @authName, [Password] = @password WHERE Id = @id
	END
END
