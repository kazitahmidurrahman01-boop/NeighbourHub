-- ============================================================================
-- NeighbourHub – Smart Apartment Management System
-- Database: NeighbourHubDB (SQL Server LocalDB)
-- Complete Relational Schema & Seed Test Data
-- ============================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'NeighbourHubDB')
BEGIN
    CREATE DATABASE [NeighbourHubDB];
END
GO

USE [NeighbourHubDB];
GO

-- 1. Users Table
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Password NVARCHAR(100) NOT NULL,
        FullName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NULL,
        Phone NVARCHAR(20) NULL,
        Role NVARCHAR(30) NOT NULL, -- 'Admin', 'Property Owner', 'Building Manager', 'Resident'
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active', -- 'Active', 'Inactive'
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 2. Properties Table (Owned by Property Owners)
IF OBJECT_ID('dbo.Properties', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Properties (
        PropertyId INT IDENTITY(1,1) PRIMARY KEY,
        OwnerUserId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId),
        PropertyName NVARCHAR(100) NOT NULL,
        PropertyType NVARCHAR(50) NOT NULL DEFAULT 'Residential Complex',
        Address NVARCHAR(200) NOT NULL,
        City NVARCHAR(50) NOT NULL DEFAULT 'Dhaka',
        TotalFloors INT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 3. Buildings Table
IF OBJECT_ID('dbo.Buildings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Buildings (
        BuildingId INT IDENTITY(1,1) PRIMARY KEY,
        PropertyId INT NOT NULL FOREIGN KEY REFERENCES dbo.Properties(PropertyId),
        ManagerUserId INT NULL FOREIGN KEY REFERENCES dbo.Users(UserId),
        BuildingName NVARCHAR(100) NOT NULL,
        BuildingCode NVARCHAR(50) NULL,
        TotalUnits INT NOT NULL DEFAULT 0,
        Address NVARCHAR(200) NULL,
        SecurityContact NVARCHAR(50) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 4. Flats Table
IF OBJECT_ID('dbo.Flats', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Flats (
        FlatId INT IDENTITY(1,1) PRIMARY KEY,
        BuildingId INT NOT NULL FOREIGN KEY REFERENCES dbo.Buildings(BuildingId),
        FlatNumber NVARCHAR(20) NOT NULL,
        FloorNumber INT NOT NULL,
        Bedrooms INT NOT NULL DEFAULT 2,
        Bathrooms INT NOT NULL DEFAULT 2,
        AreaSqFt DECIMAL(10,2) NOT NULL DEFAULT 1000,
        MonthlyRent DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Vacant', -- 'Vacant', 'Occupied', 'Under Maintenance'
        Description NVARCHAR(250) NULL
    );
END
GO

-- 5. Tenants Table (Tenant Allocation)
IF OBJECT_ID('dbo.Tenants', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tenants (
        TenantId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId),
        FlatId INT NOT NULL FOREIGN KEY REFERENCES dbo.Flats(FlatId),
        LeaseStartDate DATE NOT NULL,
        LeaseEndDate DATE NULL,
        AgreedRent DECIMAL(18,2) NOT NULL,
        SecurityDeposit DECIMAL(18,2) NOT NULL DEFAULT 0,
        EmergencyContact NVARCHAR(100) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active' -- 'Active', 'Terminated'
    );
END
GO

-- 6. Residents Table
IF OBJECT_ID('dbo.Residents', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Residents (
        ResidentId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId),
        FlatId INT NOT NULL FOREIGN KEY REFERENCES dbo.Flats(FlatId),
        TenantId INT NULL FOREIGN KEY REFERENCES dbo.Tenants(TenantId),
        Relationship NVARCHAR(50) NOT NULL DEFAULT 'Self', -- 'Self', 'Spouse', 'Child', 'Parent', 'Other'
        NID NVARCHAR(50) NULL,
        Profession NVARCHAR(100) NULL,
        MoveInDate DATE NOT NULL DEFAULT GETDATE(),
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active'
    );
END
GO

-- 7. Rent Table
IF OBJECT_ID('dbo.Rent', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Rent (
        RentId INT IDENTITY(1,1) PRIMARY KEY,
        FlatId INT NOT NULL FOREIGN KEY REFERENCES dbo.Flats(FlatId),
        TenantId INT NOT NULL FOREIGN KEY REFERENCES dbo.Tenants(TenantId),
        Month NVARCHAR(20) NOT NULL,
        Year INT NOT NULL,
        RentAmount DECIMAL(18,2) NOT NULL,
        UtilityCharges DECIMAL(18,2) NOT NULL DEFAULT 0,
        DueDate DATE NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Due', -- 'Paid', 'Due', 'Partially Paid'
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 8. Payments Table
IF OBJECT_ID('dbo.Payments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments (
        PaymentId INT IDENTITY(1,1) PRIMARY KEY,
        RentId INT NULL FOREIGN KEY REFERENCES dbo.Rent(RentId),
        TenantId INT NOT NULL FOREIGN KEY REFERENCES dbo.Tenants(TenantId),
        FlatId INT NOT NULL FOREIGN KEY REFERENCES dbo.Flats(FlatId),
        AmountPaid DECIMAL(18,2) NOT NULL,
        PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
        PaymentMethod NVARCHAR(50) NOT NULL DEFAULT 'Cash', -- 'Cash', 'Bank Transfer', 'bKash/Nagad', 'Card'
        TransactionRef NVARCHAR(100) NULL,
        PaidBy NVARCHAR(100) NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Completed',
        Remarks NVARCHAR(250) NULL
    );
END
GO

-- 9. Complaints Table
IF OBJECT_ID('dbo.Complaints', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Complaints (
        ComplaintId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId),
        FlatId INT NULL FOREIGN KEY REFERENCES dbo.Flats(FlatId),
        BuildingId INT NOT NULL FOREIGN KEY REFERENCES dbo.Buildings(BuildingId),
        Category NVARCHAR(50) NOT NULL, -- 'Plumbing', 'Electrical', 'Elevator', 'Security', 'Cleanliness', 'Noise', 'Other'
        Title NVARCHAR(150) NOT NULL,
        Description NVARCHAR(MAX) NOT NULL,
        Priority NVARCHAR(20) NOT NULL DEFAULT 'Medium', -- 'Low', 'Medium', 'High', 'Urgent'
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'In Progress', 'Solved', 'Rejected'
        AssignedToManagerId INT NULL FOREIGN KEY REFERENCES dbo.Users(UserId),
        SubmittedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ResolvedDate DATETIME NULL,
        ResolutionNotes NVARCHAR(500) NULL
    );
END
GO

-- 10. Notices Table
IF OBJECT_ID('dbo.Notices', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notices (
        NoticeId INT IDENTITY(1,1) PRIMARY KEY,
        BuildingId INT NOT NULL FOREIGN KEY REFERENCES dbo.Buildings(BuildingId),
        PublishedByUserId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId),
        Title NVARCHAR(150) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        Category NVARCHAR(50) NOT NULL DEFAULT 'General', -- 'General', 'Maintenance', 'Emergency', 'Meeting', 'Billing'
        Priority NVARCHAR(20) NOT NULL DEFAULT 'Normal', -- 'Normal', 'Important', 'Urgent'
        PublishedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ExpiryDate DATE NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END
GO

-- 11. Visitors Table
IF OBJECT_ID('dbo.Visitors', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Visitors (
        VisitorId INT IDENTITY(1,1) PRIMARY KEY,
        FlatId INT NOT NULL FOREIGN KEY REFERENCES dbo.Flats(FlatId),
        ResidentUserId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId),
        VisitorName NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NULL,
        Purpose NVARCHAR(150) NULL,
        CheckInTime DATETIME NOT NULL DEFAULT GETDATE(),
        CheckOutTime DATETIME NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Inside' -- 'Inside', 'Checked Out', 'Expected'
    );
END
GO

-- 12. Maintenance Table
IF OBJECT_ID('dbo.Maintenance', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Maintenance (
        MaintenanceId INT IDENTITY(1,1) PRIMARY KEY,
        BuildingId INT NOT NULL FOREIGN KEY REFERENCES dbo.Buildings(BuildingId),
        FlatId INT NULL FOREIGN KEY REFERENCES dbo.Flats(FlatId),
        Title NVARCHAR(150) NOT NULL,
        Description NVARCHAR(500) NULL,
        Cost DECIMAL(18,2) NOT NULL DEFAULT 0,
        VendorName NVARCHAR(100) NULL,
        ScheduledDate DATE NOT NULL,
        CompletionDate DATE NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Scheduled' -- 'Scheduled', 'In Progress', 'Completed', 'Cancelled'
    );
END
GO

-- 13. Utilities Table
IF OBJECT_ID('dbo.Utilities', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Utilities (
        UtilityId INT IDENTITY(1,1) PRIMARY KEY,
        BuildingId INT NOT NULL FOREIGN KEY REFERENCES dbo.Buildings(BuildingId),
        FlatId INT NULL FOREIGN KEY REFERENCES dbo.Flats(FlatId),
        UtilityType NVARCHAR(50) NOT NULL, -- 'Electricity', 'Water', 'Gas', 'Internet', 'Waste', 'Generator'
        BillingMonth NVARCHAR(20) NOT NULL,
        BillingYear INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        DueDate DATE NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Unpaid' -- 'Unpaid', 'Paid'
    );
END
GO

-- 14. SharedItems Table
IF OBJECT_ID('dbo.SharedItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SharedItems (
        ItemId INT IDENTITY(1,1) PRIMARY KEY,
        OwnerUserId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId),
        ItemName NVARCHAR(100) NOT NULL,
        Category NVARCHAR(50) NOT NULL DEFAULT 'Tools', -- 'Tools', 'Appliances', 'Ladders', 'Sports/Games', 'Books', 'Other'
        Description NVARCHAR(250) NULL,
        AvailabilityStatus NVARCHAR(20) NOT NULL DEFAULT 'Available', -- 'Available', 'Borrowed', 'Unavailable'
        ContactNumber NVARCHAR(20) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 15. EmergencyContacts Table
IF OBJECT_ID('dbo.EmergencyContacts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmergencyContacts (
        ContactId INT IDENTITY(1,1) PRIMARY KEY,
        BuildingId INT NULL FOREIGN KEY REFERENCES dbo.Buildings(BuildingId),
        ServiceName NVARCHAR(100) NOT NULL, -- 'Police', 'Fire Service', 'Ambulance', 'Building Security', 'Electrician', 'Plumber', 'Doctor', 'Manager'
        ContactPerson NVARCHAR(100) NULL,
        Phone NVARCHAR(30) NOT NULL,
        AltPhone NVARCHAR(30) NULL,
        AvailableHours NVARCHAR(50) NOT NULL DEFAULT '24/7',
        Address NVARCHAR(200) NULL
    );
END
GO

-- ============================================================================
-- SEED DATA INSERTION (Only if Users table is empty)
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Users)
BEGIN
    -- 1. Insert Users
    INSERT INTO dbo.Users (Username, Password, FullName, Email, Phone, Role, Status) VALUES
    ('admin', 'admin123', 'System Administrator', 'admin@neighbourhub.com', '01700000001', 'Admin', 'Active'),
    ('owner1', 'owner123', 'Mr. Tariq Ahmed', 'tariq.ahmed@gmail.com', '01811111111', 'Property Owner', 'Active'),
    ('owner2', 'owner123', 'Mrs. Farhana Yasmin', 'farhana.y@gmail.com', '01822222222', 'Property Owner', 'Active'),
    ('manager1', 'manager123', 'Kamal Hossain', 'kamal.manager@neighbourhub.com', '01711998877', 'Building Manager', 'Active'),
    ('manager2', 'manager123', 'Sadia Islam', 'sadia.manager@neighbourhub.com', '01711998888', 'Building Manager', 'Active'),
    ('tanisha', 'tenant123', 'Tanisha Rahman', 'tanisha.rahman@gmail.com', '01911223344', 'Resident', 'Active'),
    ('rahim', 'tenant123', 'Rahim Chowdhury', 'rahim.chowdhury@gmail.com', '01922334455', 'Resident', 'Active'),
    ('arif', 'tenant123', 'Arif Hasan', 'arif.hasan@gmail.com', '01933445566', 'Resident', 'Active'),
    ('nusrat', 'tenant123', 'Nusrat Jahan', 'nusrat.jahan@gmail.com', '01944556677', 'Resident', 'Active');

    -- 2. Insert Properties
    INSERT INTO dbo.Properties (OwnerUserId, PropertyName, PropertyType, Address, City, TotalFloors) VALUES
    (2, 'Skyline Heights', 'Residential Complex', 'Plot 42, Road 11, Gulshan-2', 'Dhaka', 8),
    (3, 'Greenwood Residency', 'Apartment Tower', 'House 15, Road 27, Dhanmondi', 'Dhaka', 6);

    -- 3. Insert Buildings
    INSERT INTO dbo.Buildings (PropertyId, ManagerUserId, BuildingName, BuildingCode, TotalUnits, Address, SecurityContact) VALUES
    (1, 4, 'Skyline Tower A', 'SKY-A', 16, 'Plot 42, Road 11, Gulshan-2, Dhaka', '01700112233'),
    (2, 5, 'Greenwood Block B', 'GW-B', 12, 'House 15, Road 27, Dhanmondi, Dhaka', '01700112244');

    -- 4. Insert Flats
    INSERT INTO dbo.Flats (BuildingId, FlatNumber, FloorNumber, Bedrooms, Bathrooms, AreaSqFt, MonthlyRent, Status, Description) VALUES
    (1, '1A', 1, 3, 3, 1600.00, 30000.00, 'Occupied', 'Spacious 3BHK south-facing luxury flat'),
    (1, '1B', 1, 2, 2, 1200.00, 22000.00, 'Vacant', 'Modern 2BHK flat with balconies'),
    (1, '2A', 2, 3, 3, 1600.00, 30000.00, 'Vacant', '3BHK corner unit with road view'),
    (1, '2B', 2, 2, 2, 1250.00, 23000.00, 'Occupied', '2BHK fully furnished flat'),
    (1, '5A', 5, 2, 2, 1100.00, 15000.00, 'Occupied', '2BHK with open kitchen layout'),
    (1, '5B', 5, 3, 3, 1450.00, 18000.00, 'Occupied', '3BHK apartment with master suite'),
    (1, '6A', 6, 2, 2, 1150.00, 16000.00, 'Vacant', 'Top floor peaceful unit'),
    (1, '6B', 6, 3, 3, 1500.00, 20000.00, 'Vacant', 'Penthouse style 3BHK unit'),
    (2, 'G-101', 1, 3, 2, 1350.00, 25000.00, 'Vacant', 'Ground floor apartment'),
    (2, 'G-201', 2, 3, 3, 1550.00, 28000.00, 'Vacant', 'Second floor luxury apartment');

    -- 5. Insert Tenants (Active Leases)
    INSERT INTO dbo.Tenants (UserId, FlatId, LeaseStartDate, LeaseEndDate, AgreedRent, SecurityDeposit, EmergencyContact, Status) VALUES
    (6, 5, '2026-01-01', '2026-12-31', 15000.00, 30000.00, 'Farhan Rahman (Brother) - 01755123456', 'Active'),
    (7, 6, '2026-01-01', '2026-12-31', 18000.00, 36000.00, 'Anis Chowdhury (Father) - 01766234567', 'Active'),
    (8, 1, '2026-02-01', '2027-01-31', 30000.00, 60000.00, 'Nasir Hasan (Uncle) - 01777345678', 'Active'),
    (9, 4, '2026-03-01', '2027-02-28', 23000.00, 46000.00, 'Shamim Jahan (Mother) - 01788456789', 'Active');

    -- 6. Insert Residents
    INSERT INTO dbo.Residents (UserId, FlatId, TenantId, Relationship, NID, Profession, MoveInDate, Status) VALUES
    (6, 5, 1, 'Self', '1995269485123456', 'Software Engineer', '2026-01-01', 'Active'),
    (7, 6, 2, 'Self', '1992269485654321', 'Bank Officer', '2026-01-01', 'Active'),
    (8, 1, 3, 'Self', '1988269485987654', 'Business Executive', '2026-02-01', 'Active'),
    (9, 4, 4, 'Self', '1994269485321654', 'Architect', '2026-03-01', 'Active');

    -- 7. Insert Rent Records
    INSERT INTO dbo.Rent (FlatId, TenantId, Month, Year, RentAmount, UtilityCharges, DueDate, Status) VALUES
    (5, 1, 'August', 2026, 15000.00, 2000.00, '2026-08-10', 'Paid'),
    (6, 2, 'August', 2026, 18000.00, 2500.00, '2026-08-10', 'Due'),
    (1, 3, 'August', 2026, 30000.00, 3500.00, '2026-08-10', 'Paid'),
    (4, 4, 'August', 2026, 23000.00, 2500.00, '2026-08-10', 'Due');

    -- 8. Insert Payments
    INSERT INTO dbo.Payments (RentId, TenantId, FlatId, AmountPaid, PaymentDate, PaymentMethod, TransactionRef, PaidBy, Status, Remarks) VALUES
    (1, 1, 5, 17000.00, '2026-08-05 10:30:00', 'bKash/Nagad', 'BK-99482711', 'Tanisha Rahman', 'Completed', 'Rent + Utilities for August 2026'),
    (3, 3, 1, 33500.00, '2026-08-06 14:15:00', 'Bank Transfer', 'EBL-TXN-558291', 'Arif Hasan', 'Completed', 'August Rent paid via Online Banking');

    -- 9. Insert Complaints
    INSERT INTO dbo.Complaints (UserId, FlatId, BuildingId, Category, Title, Description, Priority, Status, AssignedToManagerId, SubmittedDate, ResolvedDate, ResolutionNotes) VALUES
    (6, 5, 1, 'Electrical', '5th floor corridor light is damaged.', 'The bulb outside flat 5A flickers and went completely dark yesterday evening.', 'High', 'In Progress', 4, '2026-08-25 18:30:00', NULL, 'Assigned to building electrician Rafiq. Replacement bulb ordered.'),
    (7, 6, 1, 'Plumbing', 'Water leakage in master bathroom sink.', 'Slow drip under the sink pipe causing water accumulation on bathroom floor.', 'Medium', 'Pending', 4, '2026-08-28 09:15:00', NULL, NULL),
    (8, 1, 1, 'Elevator', 'Elevator #2 making strange friction sound.', 'During descent between 4th and 2nd floor, a loud squeak is heard.', 'Urgent', 'Solved', 4, '2026-08-15 11:00:00', '2026-08-17 16:00:00', 'Technician from Otis inspected and lubricated guide rails and roller bearings.');

    -- 10. Insert Notices
    INSERT INTO dbo.Notices (BuildingId, PublishedByUserId, Title, Content, Category, Priority, PublishedDate, ExpiryDate, IsActive) VALUES
    (1, 4, 'Scheduled Water Tank Cleaning', 'Please be advised that the underground and overhead water tanks will be cleaned on Friday from 8:00 AM to 2:00 PM. Water supply will be temporarily paused.', 'Maintenance', 'Urgent', '2026-08-28 10:00:00', '2026-09-05', 1),
    (1, 4, 'Monthly Apartment Society Meeting', 'The general body meeting for all residents and owners will be held on the Community Hall (Rooftop) this Sunday at 7:30 PM.', 'Meeting', 'Important', '2026-08-26 12:00:00', '2026-09-10', 1),
    (1, 4, 'Security Protocol: Mandatory Visitor Entry Log', 'All visitors, couriers, and maintenance staff must enter their details at the front gate guard post before proceeding upstairs.', 'General', 'Normal', '2026-08-01 09:00:00', '2026-12-31', 1);

    -- 11. Insert Visitors
    INSERT INTO dbo.Visitors (FlatId, ResidentUserId, VisitorName, Phone, Purpose, CheckInTime, CheckOutTime, Status) VALUES
    (5, 6, 'Arman Khan', '01711223344', 'Family Visit', '2026-08-31 16:00:00', NULL, 'Inside'),
    (1, 8, 'Delivery Agent (Foodpanda)', '01899887766', 'Food Delivery', '2026-08-31 13:10:00', '2026-08-31 13:25:00', 'Checked Out'),
    (6, 7, 'Dr. M. Alam', '01722334455', 'Medical Consultation', '2026-08-30 19:00:00', '2026-08-30 20:30:00', 'Checked Out');

    -- 12. Insert Maintenance
    INSERT INTO dbo.Maintenance (BuildingId, FlatId, Title, Description, Cost, VendorName, ScheduledDate, CompletionDate, Status) VALUES
    (1, NULL, 'Elevator Routine Preventive Servicing', 'Bi-monthly inspection of both passenger lifts and safety brakes.', 8500.00, 'Otis Bangladesh Ltd', '2026-08-15', '2026-08-15', 'Completed'),
    (1, NULL, 'CCTV Security Camera Upgrade', 'Installing 4 new high-definition IP cameras in parking and rooftop areas.', 25000.00, 'SecureTech Solutions', '2026-09-05', NULL, 'Scheduled'),
    (1, NULL, 'Diesel Generator Overhaul & Fuel Refill', 'Replacing air filters, oil, and 200L diesel refill.', 14500.00, 'Energy Solutions Ltd', '2026-08-20', '2026-08-20', 'Completed');

    -- 13. Insert Utilities
    INSERT INTO dbo.Utilities (BuildingId, FlatId, UtilityType, BillingMonth, BillingYear, Amount, DueDate, Status) VALUES
    (1, NULL, 'Electricity (Common Area & Lift)', 'August', 2026, 18500.00, '2026-09-15', 'Unpaid'),
    (1, NULL, 'Generator Fuel', 'August', 2026, 14500.00, '2026-08-25', 'Paid'),
    (1, 5, 'Electricity', 'August', 2026, 2150.00, '2026-08-15', 'Paid'),
    (1, 6, 'Electricity', 'August', 2026, 2800.00, '2026-08-15', 'Unpaid');

    -- 14. Insert Shared Items
    INSERT INTO dbo.SharedItems (OwnerUserId, ItemName, Category, Description, AvailabilityStatus, ContactNumber) VALUES
    (7, 'Bosch Impact Drill Machine (650W)', 'Tools', 'Includes masonry drill bit set and tool box. Great for mounting frames/shelves.', 'Available', '01922334455'),
    (8, '8-Foot Heavy Duty Aluminum Ladder', 'Ladders', 'Foldable sturdy step ladder for ceiling work and painting.', 'Available', '01933445566'),
    (6, 'High-Pressure Steam Cleaner', 'Appliances', 'Karcher steam cleaner for sofa/car cleaning.', 'Borrowed', '01911223344'),
    (6, 'Board Game Collection (Catan, Monopoly)', 'Sports/Games', 'Complete box with all accessories for weekend community fun.', 'Available', '01911223344');

    -- 15. Insert Emergency Contacts
    INSERT INTO dbo.EmergencyContacts (BuildingId, ServiceName, ContactPerson, Phone, AltPhone, AvailableHours, Address) VALUES
    (1, 'Police Emergency', 'National Emergency Service', '999', '02-8822222', '24/7', 'Gulshan Thana, Dhaka'),
    (1, 'Fire Service & Civil Defence', 'Central Control Room', '16163', '02-9555555', '24/7', 'Tejgaon Fire Station, Dhaka'),
    (1, 'Ambulance Service', 'United Hospital Emergency', '10666', '01914001234', '24/7', 'Plot 15, Road 71, Gulshan-2, Dhaka'),
    (1, 'Building Security In-charge', 'Salam Hawlader', '01700112233', NULL, '24/7', 'Gate #1 Guard Room, Skyline Tower A'),
    (1, '24/7 Electrician', 'Rafiqul Islam', '01822334455', NULL, '24/7', 'On-Call Resident Technician'),
    (1, '24/7 Plumber', 'Belal Hossain', '01933445566', NULL, '24/7', 'On-Call Resident Technician'),
    (1, 'Building Manager', 'Kamal Hossain', '01711998877', NULL, '8 AM - 10 PM', 'Management Office, Ground Floor');
END
GO
