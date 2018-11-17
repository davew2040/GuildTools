CREATE TABLE [dbo].[GuildProfile]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [GuildName] NVARCHAR(200) NOT NULL, 
    [Realm] NCHAR(100) NOT NULL, 
    [Creator] NVARCHAR(450) NOT NULL, 
    CONSTRAINT [FK_GuildProfile_ToTable] FOREIGN KEY ([Creator]) REFERENCES [AspNetUsers]([Id])
)
