CREATE TABLE [dbo].[UserData]
(
	[UserId] NVARCHAR(450) NOT NULL PRIMARY KEY, 
    [Username] NVARCHAR(50) NULL, 
    [GuildName] NCHAR(50) NULL, 
    [GuildRealm] NCHAR(50) NULL, 
    CONSTRAINT [FK_Users_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id])
)

GO

CREATE INDEX [IX_Users_Column] ON [dbo].[UserData] ([UserId])
