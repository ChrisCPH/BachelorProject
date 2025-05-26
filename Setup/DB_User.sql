USE [master]

GO

CREATE LOGIN [RunningDBUser] WITH PASSWORD = 'RunningDBPassword';

GO

USE [RunningDB]

GO

CREATE USER [RunningDBUser] FOR LOGIN [RunningDBUser];

GO

EXEC sp_addrolemember 'db_owner', 'RunningDBUser';

GO