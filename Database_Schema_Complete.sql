/*
================================================================================
APPOINTMENT BOOKING SYSTEM - DATABASE SCHEMA (COMPLETE)
Database Name: AppointmentDB
SQL Server: SQL Server 2019+
Created: March 2026
================================================================================
This document contains the complete database schema including:
- Database creation and setup
- Table definitions with constraints
- Foreign key relationships
- Indexes for performance
- Sample data
- Essential SQL queries (from repositories)
- Admin scripts for maintenance
================================================================================
*/

-- ================================================================================
-- 1. DATABASE CREATION & SETUP
-- ================================================================================

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AppointmentDB')
BEGIN
    CREATE DATABASE AppointmentDB;
    PRINT 'Database AppointmentDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database AppointmentDB already exists.';
END

USE AppointmentDB;
GO

-- ================================================================================
-- 2. TABLE DEFINITIONS (WITH CONSTRAINTS)
-- ================================================================================

-- TABLE 1: Users (Regular Users who book appointments)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        Password NVARCHAR(255) NOT NULL,
        Phone NVARCHAR(15),
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        Role NVARCHAR(50) NOT NULL DEFAULT 'User', -- 'User' or 'Admin'
        IsActive BIT DEFAULT 1
    );
    
    CREATE INDEX idx_Users_Email ON Users(Email);
    CREATE INDEX idx_Users_Role ON Users(Role);
    
    PRINT 'Table Users created successfully.';
END

-- TABLE 2: Doctors (Healthcare providers)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Doctors')
BEGIN
    CREATE TABLE Doctors (
        DoctorId INT PRIMARY KEY IDENTITY(1,1),
        DoctorName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        Password NVARCHAR(255) NOT NULL,
        Specialization NVARCHAR(100) NOT NULL, -- 'Cardiologist', 'Dentist', etc.
        Experience INT NOT NULL, -- Years of experience
        Phone NVARCHAR(15),
        IsActive BIT DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    CREATE INDEX idx_Doctors_Email ON Doctors(Email);
    CREATE INDEX idx_Doctors_Specialization ON Doctors(Specialization);
    
    PRINT 'Table Doctors created successfully.';
END

-- TABLE 3: TimeSlots (Doctor availability slots per date)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TimeSlots')
BEGIN
    CREATE TABLE TimeSlots (
        SlotId INT PRIMARY KEY IDENTITY(1,1),
        DoctorId INT NOT NULL,
        SlotDate DATE NOT NULL, -- Date of the slot
        StartTime TIME NOT NULL, -- e.g., 10:00:00
        EndTime TIME NOT NULL, -- e.g., 11:00:00
        IsBooked BIT NOT NULL DEFAULT 0, -- 0 = Available, 1 = Booked
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        
        -- Foreign Key Constraint
        CONSTRAINT fk_TimeSlots_Doctor FOREIGN KEY (DoctorId) 
            REFERENCES Doctors(DoctorId) ON DELETE CASCADE
    );
    
    CREATE INDEX idx_TimeSlots_DoctorId ON TimeSlots(DoctorId);
    CREATE INDEX idx_TimeSlots_SlotDate ON TimeSlots(SlotDate);
    CREATE INDEX idx_TimeSlots_IsBooked ON TimeSlots(IsBooked);
    CREATE INDEX idx_TimeSlots_DoctorDate ON TimeSlots(DoctorId, SlotDate);
    
    PRINT 'Table TimeSlots created successfully.';
END

-- TABLE 4: Appointments (Booked appointments)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Appointments')
BEGIN
    CREATE TABLE Appointments (
        AppointmentId INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        DoctorId INT NOT NULL,
        SlotId INT NOT NULL,
        AppointmentDate DATETIME NOT NULL DEFAULT GETDATE(), -- Date/Time of booking
        Status NVARCHAR(50), -- 'Booked', 'Doctor Unavailable', 'Reschedule Suggested', etc.
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME DEFAULT GETDATE(),
        
        -- Foreign Key Constraints
        CONSTRAINT fk_Appointments_User FOREIGN KEY (UserId) 
            REFERENCES Users(UserId) ON DELETE CASCADE,
        CONSTRAINT fk_Appointments_Doctor FOREIGN KEY (DoctorId) 
            REFERENCES Doctors(DoctorId) ON DELETE CASCADE,
        CONSTRAINT fk_Appointments_TimeSlot FOREIGN KEY (SlotId) 
            REFERENCES TimeSlots(SlotId) ON DELETE CASCADE
    );
    
    CREATE INDEX idx_Appointments_UserId ON Appointments(UserId);
    CREATE INDEX idx_Appointments_DoctorId ON Appointments(DoctorId);
    CREATE INDEX idx_Appointments_SlotId ON Appointments(SlotId);
    CREATE INDEX idx_Appointments_Status ON Appointments(Status);
    CREATE INDEX idx_Appointments_CreatedDate ON Appointments(CreatedDate);
    
    PRINT 'Table Appointments created successfully.';
END

GO

-- ================================================================================
-- 3. SAMPLE DATA (FOR TESTING)
-- ================================================================================

-- Sample Users
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'testuser@gmail.com')
BEGIN
    INSERT INTO Users (Name, Email, Password, Phone, Role)
    VALUES 
        ('Test User', 'testuser@gmail.com', 'password123', '9876543210', 'User'),
        ('Admin User', 'admin@gmail.com', 'admin123', '9876543211', 'Admin'),
        ('John Doe', 'john@gmail.com', 'john123', '9876543212', 'User'),
        ('Jane Smith', 'jane@gmail.com', 'jane123', '9876543213', 'User');
    
    PRINT 'Sample users inserted.';
END

-- Sample Doctors
IF NOT EXISTS (SELECT 1 FROM Doctors WHERE Email = 'dr.sharma@gmail.com')
BEGIN
    INSERT INTO Doctors (DoctorName, Email, Password, Specialization, Experience, Phone)
    VALUES 
        ('Dr. Sharma', 'dr.sharma@gmail.com', 'doc123', 'Cardiologist', 10, '9988776655'),
        ('Dr. Patel', 'dr.patel@gmail.com', 'doc123', 'Dentist', 8, '9988776656'),
        ('Dr. Singh', 'dr.singh@gmail.com', 'doc123', 'Dermatologist', 12, '9988776657'),
        ('Dr. Gupta', 'dr.gupta@gmail.com', 'doc123', 'Neurologist', 15, '9988776658');
    
    PRINT 'Sample doctors inserted.';
END

GO

-- ================================================================================
-- 4. STANDARD TIME SLOTS SETUP
-- ================================================================================

-- Standard appointment slots (10 AM - 5 PM, 1-hour slots)
-- This procedure can be called for each doctor and date
-- Standard slots: 10-11 AM, 11 AM-12 PM, 1-2 PM, 2-3 PM, 3-4 PM, 4-5 PM

IF OBJECT_ID('sp_CreateStandardSlots', 'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE sp_CreateStandardSlots AS SELECT 1');
END
GO

ALTER PROCEDURE sp_CreateStandardSlots
    @DoctorId INT,
    @SlotDate DATE
AS
BEGIN
    -- Check if slots already exist for this date
    IF NOT EXISTS (SELECT 1 FROM TimeSlots WHERE DoctorId = @DoctorId AND SlotDate = @SlotDate)
    BEGIN
        INSERT INTO TimeSlots (DoctorId, SlotDate, StartTime, EndTime, IsBooked)
        VALUES
            (@DoctorId, @SlotDate, '10:00:00', '11:00:00', 0),
            (@DoctorId, @SlotDate, '11:00:00', '12:00:00', 0),
            (@DoctorId, @SlotDate, '13:00:00', '14:00:00', 0),
            (@DoctorId, @SlotDate, '14:00:00', '15:00:00', 0),
            (@DoctorId, @SlotDate, '15:00:00', '16:00:00', 0),
            (@DoctorId, @SlotDate, '16:00:00', '17:00:00', 0);
        
        PRINT 'Standard slots created for Doctor ' + CAST(@DoctorId AS VARCHAR) + ' on ' + CAST(@SlotDate AS VARCHAR);
    END
END
GO

-- Execute to create slots for today for all doctors
EXEC sp_CreateStandardSlots @DoctorId = 1, @SlotDate = CAST(GETDATE() AS DATE);
EXEC sp_CreateStandardSlots @DoctorId = 2, @SlotDate = CAST(GETDATE() AS DATE);
EXEC sp_CreateStandardSlots @DoctorId = 3, @SlotDate = CAST(GETDATE() AS DATE);
EXEC sp_CreateStandardSlots @DoctorId = 4, @SlotDate = CAST(GETDATE() AS DATE);

GO

-- ================================================================================
-- 5. IMPORTANT SQL QUERIES (FROM APPLICATION REPOSITORIES)
-- ================================================================================

-- =======================
-- 5.1 USER QUERIES
-- =======================

-- Query 1: Register New User
PRINT '-- Query 1: Register New User';
/*
INSERT INTO Users (Name, Email, Password, Phone, CreatedDate, Role)
VALUES (@Name, @Email, @Password, @Phone, GETDATE(), @Role)
*/

-- Query 2: User Login
PRINT '-- Query 2: User Login (Read)';
/*
SELECT TOP 1 * FROM Users 
WHERE Email = @Email AND Password = @Password
*/

-- Query 3: Get User by Email
PRINT '-- Query 3: Get User by Email';
/*
SELECT TOP 1 * FROM Users WHERE Email = @Email
*/

-- =======================
-- 5.2 DOCTOR QUERIES
-- =======================

-- Query 4: Get All Doctors
PRINT '-- Query 4: Get All Doctors';
/*
SELECT * FROM Doctors
*/

-- Query 5: Get Available Doctors for Specific Date and Time
PRINT '-- Query 5: Get Available Doctors (Specific Date/Time)';
/*
SELECT DISTINCT d.*
FROM Doctors d
JOIN TimeSlots t ON d.DoctorId = t.DoctorId
WHERE t.SlotDate = @date
AND @time BETWEEN t.StartTime AND t.EndTime
AND t.IsBooked = 0
*/

-- Query 6: Get Total Doctors (Admin Dashboard)
PRINT '-- Query 6: Get Total Doctors Count';
/*
SELECT COUNT(*) FROM Doctors
*/

-- =======================
-- 5.3 TIME SLOT QUERIES
-- =======================

-- Query 7: Get Slots by Doctor
PRINT '-- Query 7: Get Slots by Doctor';
/*
SELECT SlotId, DoctorId, SlotDate, StartTime, EndTime, IsBooked
FROM TimeSlots
WHERE DoctorId = @DoctorId
*/

-- Query 8: Get Available Slots for Doctor
PRINT '-- Query 8: Get Available Slots (Only UnBooked)';
/*
SELECT * FROM TimeSlots
WHERE DoctorId = @DoctorId AND IsBooked = 0
*/

-- Query 9: Create New Slot
PRINT '-- Query 9: Create New Time Slot';
/*
INSERT INTO TimeSlots (DoctorId, SlotDate, StartTime, EndTime, IsBooked)
VALUES (@DoctorId, @SlotDate, @StartTime, @EndTime, 0)
*/

-- Query 10: Get Total Booked Appointments (Admin Dashboard)
PRINT '-- Query 10: Get Total Booked Slots';
/*
SELECT COUNT(*) FROM TimeSlots WHERE IsBooked = 1
*/

-- =======================
-- 5.4 APPOINTMENT QUERIES
-- =======================

-- Query 11: Book Appointment (TRANSACTION - WITH VALIDATION)
PRINT '-- Query 11: Book Appointment (Transaction)';
/*
STEP 1: Check if slot exists and is available
SELECT IsBooked FROM TimeSlots WHERE SlotId = @SlotId AND DoctorId = @DoctorId

STEP 2: If available, insert appointment
INSERT INTO Appointments (UserId, DoctorId, SlotId, AppointmentDate, Status)
VALUES (@UserId, @DoctorId, @SlotId, @AppointmentDate, @Status)

STEP 3: Update TimeSlot to mark as booked
UPDATE TimeSlots SET IsBooked = 1 WHERE SlotId = @SlotId

-- All steps within transaction
*/

-- Query 12: Get User's Phone Number
PRINT '-- Query 12: Get User Phone Number';
/*
SELECT TOP 1 Phone FROM Users WHERE UserId = @UserId
*/

-- Query 13: Get Booked Users for a Specific Slot (For Notifications)
PRINT '-- Query 13: Get Affected Users When Marking Slot Unavailable';
/*
SELECT a.AppointmentId, a.UserId, u.Name, u.Phone
FROM Appointments a
INNER JOIN Users u ON a.UserId = u.UserId
WHERE a.DoctorId = @DoctorId AND a.SlotId = @SlotId
AND (a.Status IS NULL OR a.Status = 'Booked')
*/

-- Query 14: Get All Appointments (Admin View)
PRINT '-- Query 14: Get All Appointments (Admin)';
/*
SELECT * FROM Appointments
*/

-- Query 15: Update Appointment Status
PRINT '-- Query 15: Update Appointment Status';
/*
UPDATE Appointments SET Status = @Status WHERE AppointmentId = @AppointmentId
*/

-- Query 16: Get Appointments by User (With Doctor Details)
PRINT '-- Query 16: Get User Appointments (Dashboard)';
/*
SELECT 
    a.AppointmentId,
    a.UserId,
    a.DoctorId,
    a.SlotId,
    a.AppointmentDate,
    a.Status,
    d.DoctorName,
    t.SlotDate,
    t.StartTime,
    t.EndTime
FROM Appointments a
INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
INNER JOIN TimeSlots t ON a.SlotId = t.SlotId
WHERE a.UserId = @UserId
ORDER BY t.SlotDate DESC
*/

-- Query 17: Get Appointments by Doctor (With Patient Details)
PRINT '-- Query 17: Get Doctor Appointments (Doctor Dashboard)';
/*
SELECT 
    a.AppointmentId,
    a.UserId,
    u.Name AS PatientName,
    u.Email AS PatientEmail,
    u.Phone AS PatientPhone,
    a.DoctorId,
    t.SlotDate,
    t.StartTime,
    t.EndTime,
    a.Status
FROM Appointments a
INNER JOIN Users u ON a.UserId = u.UserId
INNER JOIN TimeSlots t ON a.SlotId = t.SlotId
WHERE a.DoctorId = @DoctorId
ORDER BY t.SlotDate DESC
*/

GO

-- ================================================================================
-- 6. USEFUL ADMIN SCRIPTS & QUERIES
-- ================================================================================

-- =======================
-- 6.1 DATA VALIDATION QUERIES
-- =======================

PRINT '--- ADMIN SCRIPTS & UTILITIES ---';

-- Check data integrity
PRINT '-- Check: Verify all appointments have valid foreign keys';
SELECT 
    a.AppointmentId,
    a.UserId,
    a.DoctorId,
    a.SlotId,
    CASE WHEN u.UserId IS NULL THEN 'INVALID USER' ELSE 'OK' END AS UserStatus,
    CASE WHEN d.DoctorId IS NULL THEN 'INVALID DOCTOR' ELSE 'OK' END AS DoctorStatus,
    CASE WHEN t.SlotId IS NULL THEN 'INVALID SLOT' ELSE 'OK' END AS SlotStatus
FROM Appointments a
LEFT JOIN Users u ON a.UserId = u.UserId
LEFT JOIN Doctors d ON a.DoctorId = d.DoctorId
LEFT JOIN TimeSlots t ON a.SlotId = t.SlotId
WHERE u.UserId IS NULL OR d.DoctorId IS NULL OR t.SlotId IS NULL;

-- =======================
-- 6.2 REPORTING QUERIES
-- =======================

PRINT '-- Report 1: Total Appointments by Status';
SELECT 
    Status,
    COUNT(*) AS Total
FROM Appointments
GROUP BY Status
ORDER BY Total DESC;

PRINT '-- Report 2: Doctor Workload (Booked Slots)';
SELECT 
    d.DoctorId,
    d.DoctorName,
    d.Specialization,
    COUNT(a.AppointmentId) AS TotalAppointments,
    COUNT(CASE WHEN a.Status = 'Booked' THEN 1 END) AS BookedCount
FROM Doctors d
LEFT JOIN Appointments a ON d.DoctorId = a.DoctorId
GROUP BY d.DoctorId, d.DoctorName, d.Specialization
ORDER BY TotalAppointments DESC;

PRINT '-- Report 3: User Booking History';
SELECT 
    u.UserId,
    u.Name,
    u.Email,
    COUNT(a.AppointmentId) AS TotalBookings,
    MAX(a.AppointmentDate) AS LastBookingDate
FROM Users u
LEFT JOIN Appointments a ON u.UserId = a.UserId
WHERE u.Role = 'User'
GROUP BY u.UserId, u.Name, u.Email
ORDER BY TotalBookings DESC;

PRINT '-- Report 4: Availability Overview';
SELECT 
    t.SlotDate,
    COUNT(*) AS TotalSlots,
    COUNT(CASE WHEN t.IsBooked = 0 THEN 1 END) AS AvailableSlots,
    COUNT(CASE WHEN t.IsBooked = 1 THEN 1 END) AS BookedSlots
FROM TimeSlots t
WHERE t.SlotDate >= CAST(GETDATE() AS DATE)
GROUP BY t.SlotDate
ORDER BY t.SlotDate;

-- =======================
-- 6.3 CLEANING & MAINTENANCE QUERIES
-- =======================

PRINT '-- Delete Past Slots (Keep database clean)';
/*
DELETE FROM TimeSlots 
WHERE SlotDate < CAST(GETDATE() AS DATE) 
AND IsBooked = 0;
*/

PRINT '-- Deactivate Inactive Users (Optional)';
/*
UPDATE Users 
SET IsActive = 0 
WHERE CreatedDate < DATEADD(YEAR, -1, GETDATE());
*/

-- =======================
-- 6.4 USER MANAGEMENT QUERIES
-- =======================

PRINT '-- Reset User Password';
/*
UPDATE Users 
SET Password = @NewPassword 
WHERE UserId = @UserId;
*/

PRINT '-- Check User Login History (Recent)';
SELECT TOP 20
    u.UserId,
    u.Name,
    u.Email,
    a.AppointmentDate AS LastActivity
FROM Users u
LEFT JOIN Appointments a ON u.UserId = a.UserId
WHERE a.AppointmentDate >= DATEADD(DAY, -7, GETDATE())
ORDER BY a.AppointmentDate DESC;

-- =======================
-- 6.5 DOCTOR MANAGEMENT QUERIES
-- =======================

PRINT '-- Deactivate Doctor';
/*
UPDATE Doctors SET IsActive = 0 WHERE DoctorId = @DoctorId;
UPDATE TimeSlots SET IsBooked = 1 WHERE DoctorId = @DoctorId AND SlotDate >= CAST(GETDATE() AS DATE);
UPDATE Appointments SET Status = 'Doctor Unavailable' WHERE DoctorId = @DoctorId AND Status = 'Booked';
*/

PRINT '-- Doctor Availability for Specific Date';
SELECT 
    d.DoctorId,
    d.DoctorName,
    d.Specialization,
    t.SlotDate,
    t.StartTime,
    t.EndTime,
    CASE WHEN t.IsBooked = 0 THEN 'Available' ELSE 'Booked' END AS Status
FROM Doctors d
LEFT JOIN TimeSlots t ON d.DoctorId = t.DoctorId
WHERE d.DoctorId = 1 -- Replace with specific doctor ID
AND t.SlotDate = CAST(GETDATE() AS DATE)
ORDER BY t.StartTime;

-- =======================
-- 6.6 DATABASE SIZE & PERFORMANCE
-- =======================

PRINT '-- Check Database Size';
SELECT 
    (SUM(CAST(size AS INT)) * 8.0 / 1024) AS TotalSize_MB
FROM sys.database_files;

PRINT '-- Check Row Counts Per Table';
SELECT 
    t.NAME AS TableName,
    i.rowcnt AS RowCount
FROM sys.tables t
INNER JOIN sys.sysindexes i ON t.object_id = i.id AND i.indid < 2
ORDER BY i.rowcnt DESC;

GO

-- ================================================================================
-- 7. CRITICAL PROCEDURES & FUNCTIONS
-- ================================================================================

-- Procedure: Mark Slot Unavailable and Notify Users
IF OBJECT_ID('sp_MarkSlotUnavailable', 'P') IS NOT NULL
    DROP PROCEDURE sp_MarkSlotUnavailable;
GO

CREATE PROCEDURE sp_MarkSlotUnavailable
    @DoctorId INT,
    @SlotId INT,
    @Reason NVARCHAR(255) = 'Doctor Unavailable'
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Step 1: Get affected users
        DECLARE @AffectedUsers TABLE (
            AppointmentId INT,
            UserId INT,
            UserName NVARCHAR(100),
            UserPhone NVARCHAR(15)
        );
        
        INSERT INTO @AffectedUsers
        SELECT a.AppointmentId, a.UserId, u.Name, u.Phone
        FROM Appointments a
        INNER JOIN Users u ON a.UserId = u.UserId
        WHERE a.DoctorId = @DoctorId 
        AND a.SlotId = @SlotId
        AND (a.Status IS NULL OR a.Status = 'Booked');
        
        -- Step 2: Update appointment status
        UPDATE Appointments
        SET Status = @Reason
        WHERE DoctorId = @DoctorId AND SlotId = @SlotId AND (Status IS NULL OR Status = 'Booked');
        
        -- Step 3: Mark slot as booked to prevent new bookings
        UPDATE TimeSlots
        SET IsBooked = 1
        WHERE SlotId = @SlotId;
        
        -- Step 4: Return affected users (for notification)
        SELECT * FROM @AffectedUsers;
        
        COMMIT TRANSACTION;
        
        PRINT 'Slot marked unavailable and affected users retrieved.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ================================================================================
-- 8. DATA CORRECTION SCRIPT (For Historical Data Fixes)
-- ================================================================================

PRINT '-- Fix Wrong Appointment Owner';
/*
DECLARE @FromEmail NVARCHAR(100) = 'sid@email.com'; -- Wrong user email
DECLARE @ToEmail NVARCHAR(100) = 'testuser@gmail.com'; -- Correct user email

-- Check users exist
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = @FromEmail)
BEGIN
    RAISERROR ('Source email does not exist', 16, 1);
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = @ToEmail)
BEGIN
    RAISERROR ('Target email does not exist', 16, 1);
END

DECLARE @FromUserId INT = (SELECT UserId FROM Users WHERE Email = @FromEmail);
DECLARE @ToUserId INT = (SELECT UserId FROM Users WHERE Email = @ToEmail);

-- Preview affected appointments
PRINT 'PREVIEW: Appointments to be moved:';
SELECT AppointmentId, UserId, DoctorId, SlotId, AppointmentDate
FROM Appointments
WHERE UserId = @FromUserId;

-- Move appointments
BEGIN TRANSACTION;

UPDATE Appointments
SET UserId = @ToUserId
WHERE UserId = @FromUserId;

COMMIT TRANSACTION;

PRINT 'VERIFICATION: Moved appointments:';
SELECT AppointmentId, UserId, DoctorId, SlotId, AppointmentDate
FROM Appointments
WHERE UserId = @ToUserId
ORDER BY AppointmentDate DESC;
*/

GO

-- ================================================================================
-- 9. SUMMARY & QUICK REFERENCE
-- ================================================================================

PRINT '================================================================================';
PRINT 'DATABASE SCHEMA SETUP COMPLETE';
PRINT '================================================================================';
PRINT 'Database Name: AppointmentDB';
PRINT 'SQL Server: SQL Server 2019+';
PRINT '';
PRINT 'TABLES CREATED:';
PRINT '  1. Users - Regular users who book appointments';
PRINT '  2. Doctors - Healthcare providers';
PRINT '  3. TimeSlots - Doctor availability (date + time)';
PRINT '  4. Appointments - Booked appointments with doctor and user';
PRINT '';
PRINT 'KEY INFORMATION:';
PRINT '  - Standard slot hours: 10-11 AM, 11 AM-12 PM, 1-5 PM (hourly)';
PRINT '  - Appointment statuses: "Booked", "Doctor Unavailable", "Reschedule Suggested"';
PRINT '  - Roles: "User" (regular users), "Admin" (administrators)';
PRINT '  - All foreign key constraints include CASCADE DELETE';
PRINT '  - Indexes created on frequently queried columns for performance';
PRINT '';
PRINT 'SAMPLE DATA INSERTED:';
PRINT '  - 4 Sample Users (including 1 Admin)';
PRINT '  - 4 Sample Doctors';
PRINT '  - Standard slots created for today for all doctors';
PRINT '';
PRINT 'TO CREATE SLOTS FOR FUTURE DATES:';
PRINT '  EXEC sp_CreateStandardSlots @DoctorId = 1, @SlotDate = ''YYYY-MM-DD'';';
PRINT '';
PRINT 'TO MARK SLOT UNAVAILABLE AND NOTIFY:';
PRINT '  EXEC sp_MarkSlotUnavailable @DoctorId = 1, @SlotId = 1, @Reason = ''Doctor Unavailable'';';
PRINT '';
PRINT '================================================================================';
