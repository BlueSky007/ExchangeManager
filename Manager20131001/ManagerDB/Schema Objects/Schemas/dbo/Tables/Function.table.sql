﻿CREATE TABLE dbo.[Function]
(
	Id	INT IDENTITY(1,1) NOT NULL,
	ParentId	INT NOT NULL,
	NameCHT	NVARCHAR(50) NOT NULL,
	NameCHS	NVARCHAR(50) NOT NULL,
	NameENG	NVARCHAR(50) NOT NULL
)