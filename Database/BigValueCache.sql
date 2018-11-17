CREATE TABLE [dbo].[BigValueCache]
(
	[Id] NVARCHAR(50) NOT NULL, 
	[Type] NVARCHAR(50) NOT NULL, 
    [Value] NVARCHAR(MAX) NOT NULL, 
    [ExpiresOn] DATETIME NOT NULL,
	primary key ([Id], [Type])
)
