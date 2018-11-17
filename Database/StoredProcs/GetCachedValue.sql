CREATE PROCEDURE [dbo].[GetCachedValue]
	@id NVARCHAR(50),
	@type NVARCHAR(50),
	@Out_Id NVARCHAR(50) OUTPUT,
	@Out_Value NVARCHAR(MAX) OUTPUT,
	@Out_Type NVARCHAR(50) OUTPUT,
	@Out_ExpiresOn DATETIME OUTPUT
AS
	DELETE FROM [dbo].[BigValueCache]
	WHERE Id = @id AND Type = @type
		AND DATEDIFF(second, ExpiresOn, CAST(CURRENT_TIMESTAMP AS DATETIME)) > 0

	SELECT @Out_Id = Id, @Out_Value = Value, @Out_Type = Type, @Out_ExpiresOn = ExpiresOn
		FROM [dbo].[BigValueCache] 
		WHERE Id = @id AND Type = @type

  RETURN 