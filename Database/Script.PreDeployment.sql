/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script.	
 Use SQLCMD syntax to include a file in the pre-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 11/14/2018 2:20:35 PM ******/

declare @SQL nvarchar(max)

SELECT @SQL = STUFF((SELECT ', ' + quotename(TABLE_SCHEMA) + '.' + quotename(TABLE_NAME) 

FROM INFORMATION_SCHEMA.TABLES WHERE Table_Name LIKE '%lku'
FOR XML PATH('')),1,2,'')

SET @SQL = 'DROP TABLE ' + @SQL

PRINT @SQL

EXECUTE (@SQL)