﻿DROP TABLE [Match];

CREATE TABLE Match
(
	[Id] INT NOT NULL UNIQUE IDENTITY(1, 1),
	[FirstPlayer] NVARCHAR(30) NOT NULL,
	[SecondPlayer] NVARCHAR(30) NOT NULL,
	[Result] BIT NOT NULL,
	[RatingChange] SMALLINT
)

