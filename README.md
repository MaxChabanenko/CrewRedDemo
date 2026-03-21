The goal of this task is to implement a simple ETL project in CLI that inserts data from a CSV into a single, flat table. While the task is straightforward, make sure you take your time reading it more than once.

Input CSV data file: https://drive.google.com/file/d/1l2ARvh1-tJBqzomww45TrGtIh5j8Vud4/view?usp=sharing. You can download and read it locally rather than remotely.

## Objectives

1. Import the data from the CSV into an MS SQL table. We only want to store the following columns:
    - `tpep_pickup_datetime`
    - `tpep_dropoff_datetime`
    - `passenger_count`
    - `trip_distance`
    - `store_and_fwd_flag`
    - `PULocationID`
    - `DOLocationID`
    - `fare_amount`
    - `tip_amount`
2. Set up a SQL Server database (local or cloud-based, as per your convenience).
3. Design a table schema that will hold the processed data; make sure you are using the proper data types.
4. Users of the table will perform the following queries; ensure your schema is optimized for them:
    - Find out which `PULocationId` (Pick-up location ID) has the highest tip_amount on average.
    - Find the top 100 longest fares in terms of `trip_distance`.
    - Find the top 100 longest fares in terms of time spent traveling.
    - Search, where part of the conditions is `PULocationId`.
5. Implement efficient bulk insertion of the processed records into the database.
6. Identify and remove any duplicate records from the dataset based on a combination of `pickup_datetime`, `dropoff_datetime`, and `passenger_count`. Write all removed duplicates into a `duplicates.csv` file.
7. For the `store_and_fwd_flag` column, convert any 'N' values to 'No' and any 'Y' values to 'Yes'.
8. Ensure that all text-based fields are free from leading or trailing whitespace.
9. Assume your program will be used on much larger data files. Describe in a few sentences what you would change if you knew it would be used for a 10GB CSV input file.
10. (nice to have) The input data is in the EST timezone. Convert it to UTC when inserting into the DB.

## Requirements

- Use C# as the primary programming language.
- Efficiency of data insertion into SQL Server.
- Assume the data comes from a potentially unsafe source.

## Deliverables

- SQL scripts used for creating the database and tables.
```
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
	[StoreAndFwdFlag] [nchar](3) NULL,
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
```

- Number of rows in your table after running the program.

29840

- Any comments on any assumptions made.

I wasn't sure whats required from me in task 4, so I made SQL indexes for the table and here are the queries (tested after running program):
```
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
```

Also 'Assume the data comes from a potentially unsafe source' was puzzling but the only column with string (only 3 symbols) where SQL injection was possible I think was StoreAndFwdFlag, but in ReadAsync I transform it to only Yes/No/Null. Other columns are read using TryParse which should prevent converting to text
