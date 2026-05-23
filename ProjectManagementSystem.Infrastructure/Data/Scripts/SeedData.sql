DECLARE @AdminEmail NVARCHAR(256) = 'admin@projectsystem.com';
DECLARE @AdminId NVARCHAR(450);
SELECT @AdminId = Id FROM AspNetUsers WHERE Email = @AdminEmail;

IF @AdminId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Projects WHERE Number = 1)
    BEGIN
        INSERT INTO Projects (Name, Description, Number, CreatedAt, CreatorId)
        VALUES ('E-Commerce Platform', 'Web application development.', 1, GETUTCDATE(), @AdminId);
    END

    IF NOT EXISTS (SELECT 1 FROM Projects WHERE Number = 2)
    BEGIN
        INSERT INTO Projects (Name, Description, Number, CreatedAt, CreatorId)
        VALUES ('Internal System', 'Legacy migration.', 2, GETUTCDATE(), @AdminId);
    END

    DECLARE @WebId INT = (SELECT Id FROM Projects WHERE Number = 1);
    DECLARE @SysId INT = (SELECT Id FROM Projects WHERE Number = 2);

    IF NOT EXISTS (SELECT 1 FROM Tasks WHERE ProjectId = @WebId AND Number = 1)
    BEGIN
        INSERT INTO Tasks (Title, Description, Status, Priority, Type, Deadline, ProjectId, ReporterId, AssigneeId, Number, CreatedAt)
        VALUES 
        ('Database Design', 'Initial schema.', 2, 3, 2, DATEADD(day, 5, GETUTCDATE()), @WebId, @AdminId, @AdminId, 1, GETUTCDATE()),
        ('Login Bug', 'Fixing auth redirect.', 1, 3, 1, DATEADD(day, 2, GETUTCDATE()), @WebId, @AdminId, @AdminId, 2, GETUTCDATE());
    END

    IF NOT EXISTS (SELECT 1 FROM Tasks WHERE ProjectId = @SysId AND Number = 1)
    BEGIN
        INSERT INTO Tasks (Title, Description, Status, Priority, Type, Deadline, ProjectId, ReporterId, AssigneeId, Number, CreatedAt)
        VALUES 
        ('Azure Setup', 'Configuring cloud.', 0, 3, 2, DATEADD(day, 3, GETUTCDATE()), @SysId, @AdminId, @AdminId, 1, GETUTCDATE());
    END

    DECLARE @T1 INT = (SELECT Id FROM Tasks WHERE ProjectId = @WebId AND Number = 1);
    DECLARE @T2 INT = (SELECT Id FROM Tasks WHERE ProjectId = @WebId AND Number = 2);

    IF NOT EXISTS (SELECT 1 FROM TimeLogs WHERE TaskId = @T1)
    BEGIN
        INSERT INTO TimeLogs (Hours, Date, Description, TaskId, UserId)
        VALUES (2.5, GETUTCDATE(), 'Modeling tables', @T1, @AdminId);
        
        INSERT INTO TimeLogs (Hours, Date, Description, TaskId, UserId)
        VALUES (1.0, GETUTCDATE(), 'Fixing redirect logic', @T2, @AdminId);
    END

    IF NOT EXISTS (SELECT 1 FROM Comments WHERE TaskId = @T1)
    BEGIN
        INSERT INTO Comments (Content, CreatedAt, TaskId, UserId)
        VALUES 
        ('Schema looks solid.', GETUTCDATE(), @T1, @AdminId),
        ('Found a bug in the redirect.', GETUTCDATE(), @T2, @AdminId);
    END
END