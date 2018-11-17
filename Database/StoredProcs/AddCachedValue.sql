CREATE PROCEDURE [dbo].[AddCachedValue]
	@id NVARCHAR(50),
	@type NVARCHAR(50),
	@value NVARCHAR(MAX),
	@expiresOn DATETIME
AS
  SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
  BEGIN TRAN
 
    IF EXISTS ( SELECT * FROM [dbo].[BigValueCache] WITH (UPDLOCK) WHERE Id = @id AND Type = @type)
 
      UPDATE [dbo].[BigValueCache]
         SET 
			Value = @value,
			ExpiresOn = @expiresOn
       WHERE Id = id AND Type = @type
 
    ELSE 
 
      INSERT [dbo].[BigValueCache] ( Id, Type, Value, ExpiresOn )
      VALUES ( @id, @type, @value, @expiresOn );
 
  COMMIT
RETURN 0