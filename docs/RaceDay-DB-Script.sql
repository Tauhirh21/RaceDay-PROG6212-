-- =====================================================
-- RaceDay Database Script
-- PROG6212 POE - Part 1
-- Matches docs/ERD.png
-- =====================================================

USE master;
GO

-- Drop database if it already exists (for clean re-runs)
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'RaceDayDB')
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END
GO

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- =====================================================
-- 1. USER TABLE
-- Stores both Organisers and Participants
-- =====================================================
CREATE TABLE [User] (
    UserId          INT IDENTITY(1,1) NOT NULL,
    FullName        NVARCHAR(100) NOT NULL,
    Email           NVARCHAR(150) NOT NULL,
    PasswordHash    NVARCHAR(255) NOT NULL,
    Role            NVARCHAR(20) NOT NULL,
    PhoneNumber     NVARCHAR(20) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_User PRIMARY KEY (UserId),
    CONSTRAINT UQ_User_Email UNIQUE (Email),
    CONSTRAINT CK_User_Role CHECK (Role IN ('Organiser', 'Participant'))
);
GO

-- =====================================================
-- 2. EVENT TABLE
-- Created and managed by an Organiser
-- =====================================================
CREATE TABLE Event (
    EventId         INT IDENTITY(1,1) NOT NULL,
    Name            NVARCHAR(200) NOT NULL,
    Description     NVARCHAR(1000) NULL,
    EventDate       DATE NOT NULL,
    Location        NVARCHAR(200) NOT NULL,
    Province        NVARCHAR(50) NULL,
    OrganiserId     INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_Event PRIMARY KEY (EventId),
    CONSTRAINT FK_Event_User FOREIGN KEY (OrganiserId)
        REFERENCES [User](UserId)
);
GO

-- =====================================================
-- 3. CATEGORY TABLE
-- Race categories within an event (e.g., 10km, 21km)
-- =====================================================
CREATE TABLE Category (
    CategoryId      INT IDENTITY(1,1) NOT NULL,
    EventId         INT NOT NULL,
    Name            NVARCHAR(100) NOT NULL,
    DistanceKm      DECIMAL(5,2) NOT NULL,
    EntryFee        DECIMAL(10,2) NOT NULL DEFAULT 0,
    MaxParticipants INT NOT NULL DEFAULT 0,

    CONSTRAINT PK_Category PRIMARY KEY (CategoryId),
    CONSTRAINT FK_Category_Event FOREIGN KEY (EventId)
        REFERENCES Event(EventId)
);
GO

-- =====================================================
-- 4. ENROLMENT TABLE
-- Junction table resolving M:N between User and Category
-- =====================================================
CREATE TABLE Enrolment (
    EnrolmentId     INT IDENTITY(1,1) NOT NULL,
    UserId          INT NOT NULL,
    CategoryId      INT NOT NULL,
    EnrolmentDate   DATETIME2 NOT NULL DEFAULT GETDATE(),
    BibNumber       NVARCHAR(20) NULL,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'Confirmed',

    CONSTRAINT PK_Enrolment PRIMARY KEY (EnrolmentId),
    CONSTRAINT FK_Enrolment_User FOREIGN KEY (UserId)
        REFERENCES [User](UserId),
    CONSTRAINT FK_Enrolment_Category FOREIGN KEY (CategoryId)
        REFERENCES Category(CategoryId),
    CONSTRAINT UQ_Enrolment_User_Category UNIQUE (UserId, CategoryId)
);
GO

-- =====================================================
-- 5. RESULT TABLE
-- 1:1 with Enrolment
-- =====================================================
CREATE TABLE Result (
    ResultId        INT IDENTITY(1,1) NOT NULL,
    EnrolmentId     INT NOT NULL,
    FinishTime      TIME NULL,
    Position        INT NULL,
    DidNotFinish    BIT NOT NULL DEFAULT 0,
    CapturedAt      DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_Result PRIMARY KEY (ResultId),
    CONSTRAINT UQ_Result_Enrolment UNIQUE (EnrolmentId),
    CONSTRAINT FK_Result_Enrolment FOREIGN KEY (EnrolmentId)
        REFERENCES Enrolment(EnrolmentId)
);
GO

-- =====================================================
-- 6. WEATHERLOG TABLE
-- Weather info for an event on race day
-- =====================================================
CREATE TABLE WeatherLog (
    WeatherLogId    INT IDENTITY(1,1) NOT NULL,
    EventId         INT NOT NULL,
    LogDate         DATE NOT NULL,
    TemperatureC    DECIMAL(5,2) NULL,
    Condition       NVARCHAR(50) NULL,
    Humidity        INT NULL,
    WindSpeedKmh    DECIMAL(5,2) NULL,

    CONSTRAINT PK_WeatherLog PRIMARY KEY (WeatherLogId),
    CONSTRAINT FK_WeatherLog_Event FOREIGN KEY (EventId)
        REFERENCES Event(EventId)
);
GO

-- =====================================================
-- SEED DATA
-- =====================================================

-- Users: 2 Organisers, 2 Participants
INSERT INTO [User] (FullName, Email, PasswordHash, Role, PhoneNumber) VALUES
('Thabo Mokoena',        'thabo@raceday.co.za',   'hashed_pw_1', 'Organiser',   '0821234567'),
('Sarah van der Merwe',  'sarah@raceday.co.za',   'hashed_pw_2', 'Organiser',   '0839876543'),
('Lungi Dlamini',        'lungi@gmail.com',       'hashed_pw_3', 'Participant', '0715551234'),
('Johan Botha',          'johan@gmail.com',       'hashed_pw_4', 'Participant', '0724445678');
GO

-- Events: 3 events
INSERT INTO Event (Name, Description, EventDate, Location, Province, OrganiserId) VALUES
('Comrades Marathon 2027',          'The ultimate human race.',              '2027-06-14', 'Pietermaritzburg to Durban', 'KZN',     1),
('Cape Town Cycle Tour 2027',       'World''s largest timed cycle race.',    '2027-03-08', 'Cape Town',                  'WC',      1),
('Soweto Marathon 2027',            'The people''s race.',                   '2027-11-01', 'Soweto, Johannesburg',       'Gauteng', 2);
GO

-- Categories: each event has at least one category
INSERT INTO Category (EventId, Name, DistanceKm, EntryFee, MaxParticipants) VALUES
(1, 'Comrades 90km Men Open',       90.00, 550.00, 15000),
(1, 'Comrades 90km Women Open',     90.00, 550.00, 10000),
(2, 'Cycle Tour 109km',             109.00, 680.00, 35000),
(3, 'Soweto Marathon 42km',         42.20, 250.00, 12000),
(3, 'Soweto Half Marathon 21km',    21.10, 180.00, 8000);
GO

-- Enrolments: sample participants entering categories
INSERT INTO Enrolment (UserId, CategoryId, BibNumber, Status) VALUES
(3, 1, 'M00123', 'Confirmed'),
(3, 4, 'S00045', 'Confirmed'),
(4, 2, 'W00210', 'Confirmed');
GO

-- Results: sample results for completed enrolments
INSERT INTO Result (EnrolmentId, FinishTime, Position, DidNotFinish) VALUES
(1, '08:45:22', 245, 0),
(3, '04:12:08', 89, 0);
GO

-- WeatherLog: sample weather data
INSERT INTO WeatherLog (EventId, LogDate, TemperatureC, Condition, Humidity, WindSpeedKmh) VALUES
(1, '2027-06-14', 18.50, 'Sunny',       65, 12.30),
(2, '2027-03-08', 22.00, 'Partly Cloudy', 70, 25.00),
(3, '2027-11-01', 26.50, 'Hot and Clear', 45, 8.50);
GO

-- =====================================================
-- VERIFY
-- =====================================================
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM [User]
UNION ALL
SELECT 'Events', COUNT(*) FROM Event
UNION ALL
SELECT 'Categories', COUNT(*) FROM Category
UNION ALL
SELECT 'Enrolments', COUNT(*) FROM Enrolment
UNION ALL
SELECT 'Results', COUNT(*) FROM Result
UNION ALL
SELECT 'WeatherLogs', COUNT(*) FROM WeatherLog;
GO