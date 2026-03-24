/*
    Fix wrongly mapped appointments from one user email to another.
    Default case:
      from sid@email.com -> to testuser@gmail.com

    Usage:
      1) Review the preview SELECT output.
      2) Run inside SSMS against AppointmentDB.
      3) Keep COMMIT or switch to ROLLBACK for dry run.
*/

USE AppointmentDB;
GO

DECLARE @FromEmail NVARCHAR(256) = 'sid@email.com';
DECLARE @ToEmail   NVARCHAR(256) = 'testuser@gmail.com';

DECLARE @FromUserId INT;
DECLARE @ToUserId INT;

SELECT @FromUserId = UserId FROM Users WHERE Email = @FromEmail;
SELECT @ToUserId = UserId FROM Users WHERE Email = @ToEmail;

IF @FromUserId IS NULL
BEGIN
    RAISERROR ('Source email not found in Users table.', 16, 1);
    RETURN;
END;

IF @ToUserId IS NULL
BEGIN
    RAISERROR ('Target email not found in Users table.', 16, 1);
    RETURN;
END;

IF @FromUserId = @ToUserId
BEGIN
    RAISERROR ('Source and target users are the same. Nothing to update.', 16, 1);
    RETURN;
END;

PRINT 'Source UserId: ' + CAST(@FromUserId AS NVARCHAR(20));
PRINT 'Target UserId: ' + CAST(@ToUserId AS NVARCHAR(20));

-- Preview appointments that will be changed
SELECT
    a.AppointmentId,
    a.UserId AS CurrentUserId,
    u.Email AS CurrentEmail,
    a.DoctorId,
    a.SlotId,
    a.AppointmentDate,
    a.Status
FROM Appointments a
INNER JOIN Users u ON a.UserId = u.UserId
WHERE a.UserId = @FromUserId
ORDER BY a.AppointmentId;

BEGIN TRANSACTION;

UPDATE Appointments
SET UserId = @ToUserId
WHERE UserId = @FromUserId;

PRINT 'Rows updated: ' + CAST(@@ROWCOUNT AS NVARCHAR(20));

-- Verify result
SELECT
    a.AppointmentId,
    a.UserId,
    u.Email,
    a.DoctorId,
    a.SlotId,
    a.AppointmentDate,
    a.Status
FROM Appointments a
INNER JOIN Users u ON a.UserId = u.UserId
WHERE u.Email IN (@FromEmail, @ToEmail)
ORDER BY a.AppointmentId;

COMMIT TRANSACTION;
-- For testing only, replace COMMIT with ROLLBACK.
