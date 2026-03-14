The goal of this task is to implement a simple ETL project in CLI that inserts data from a CSV into a single, flat table.

## Deliverables

- SQL scripts used for creating the database and tables.

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'crewred_test')
BEGIN
    CREATE DATABASE [crewred_test];
END
GO

USE [crewred_test]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SampleCabData](
	[Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[TpepPickupDatetime] [datetime2](7) NOT NULL,
	[TpepDropoffDatetime] [datetime2](7) NOT NULL,
	[PassengerCount] [int] NOT NULL,
	[TripDistance] [float] NOT NULL,
	[StoreAndFwdFlag] [nchar](3) NOT NULL,
	[PuLocationId] [int] NOT NULL,
	[DoLocationId] [int] NOT NULL,
	[FareAmount] [float] NOT NULL,
	[TipAmount] [float] NOT NULL,
	[TripDurationSeconds] AS (DATEDIFF(SECOND, [TpepPickupDatetime], [TpepDropoffDatetime]))
) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX IX_SampleCabData_PULocationID
ON SampleCabData(PULocationID)
INCLUDE (TipAmount);

CREATE NONCLUSTERED INDEX IX_SampleCabData_TripDistance
ON SampleCabData(TripDistance DESC);

CREATE NONCLUSTERED INDEX IX_SampleCabData_TravelTime 
ON SampleCabData(TripDurationSeconds DESC);
GO


- Number of rows in your table after running the program.

29840

- Any comments on any assumptions made.

I wasn't sure whats required from me in task 4, so I made SQL indexes for the table and here are the queries (tested after running programm):
SELECT TOP 1 
    PuLocationId, 
    AVG(TipAmount) AS AvgTip
FROM [dbo].[SampleCabData]
GROUP BY PuLocationId
ORDER BY AvgTip DESC;

SELECT TOP 100 
    Id, 
    TripDistance, 
    PuLocationId, 
    DoLocationId
FROM [dbo].[SampleCabData]
ORDER BY TripDistance DESC;

SELECT TOP 100 
    Id, 
    TpepPickupDatetime, 
    TpepDropoffDatetime,
    TripDurationSeconds
FROM [dbo].[SampleCabData]
ORDER BY TripDurationSeconds DESC;
