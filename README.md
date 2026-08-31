# NeighbourHub – Smart Apartment Management System

> **A Complete C# .NET Windows Forms Desktop Application with SQL Server LocalDB & ADO.NET**

---

## 📌 Project Overview
**NeighbourHub** is a modern, enterprise-grade Apartment & Property Management Desktop Application developed for Windows. It provides 4 specialized role-based portals (**Admin**, **Property Owner**, **Building Manager**, and **Resident/Tenant**), each equipped with 12 complete features covering tenant allocations, lease contracts, rent invoicing, automated payment collection, complaint tickets, maintenance tracking, visitor logs, item sharing, emergency services, and financial reports.

---

## 🔑 Test User Credentials (Ready for Viva & Testing)

| Role | Username | Password | Full Name | Associated Property / Unit |
| :--- | :--- | :--- | :--- | :--- |
| **System Admin** | `admin` | `admin123` | System Administrator | System-wide Controller |
| **Property Owner** | `owner1` | `owner123` | Mr. Tariq Ahmed | Skyline Heights Complex |
| **Property Owner** | `owner2` | `owner123` | Mrs. Farhana Yasmin | Greenwood Residency |
| **Building Manager** | `manager1` | `manager123` | Kamal Hossain | Skyline Tower A |
| **Building Manager** | `manager2` | `manager123` | Sadia Islam | Greenwood Block B |
| **Resident / Tenant** | `tanisha` | `tenant123` | Tanisha Rahman | Flat 5A (৳15,000 / Paid) |
| **Resident / Tenant** | `rahim` | `tenant123` | Rahim Chowdhury | Flat 5B (৳18,000 / Due) |
| **Resident / Tenant** | `arif` | `tenant123` | Arif Hasan | Flat 1A (৳30,000 / Paid) |
| **Resident / Tenant** | `nusrat` | `tenant123` | Nusrat Jahan | Flat 2B (৳23,000 / Due) |

*(Note: The Login Form also includes 1-click Quick Demo buttons to pre-fill test credentials during live demonstrations!)*

---

## 🚀 How to Run in Visual Studio

1. Double click and open `NeighbourHub.sln` in **Visual Studio**.
2. Press **F5** or click **Start (Debug)**.
3. The application automatically verifies SQL Server LocalDB `(localdb)\MSSQLLocalDB`, creates `NeighbourHubDB`, executes the schema and seeds all sample data if not already present.
4. Sign in with any of the credentials listed above.

---

## 📂 Project Architecture

```
C:\c# project\
├── NeighbourHub.sln                 # Visual Studio Solution File
├── README.md                        # Documentation & Viva Preparation Guide
└── NeighbourHub/
    ├── NeighbourHub.csproj          # .NET WinForms Windows Desktop Project
    ├── App.config                   # Centralized LocalDB Connection String Configuration
    ├── Program.cs                   # Application entry point with DB initialization
    │
    ├── Data/
    │   ├── DatabaseConnection.cs    # ADO.NET SqlConnection factory
    │   ├── DatabaseInitializer.cs   # Auto-DDL/DML database creation & self-healing
    │   ├── DbHelper.cs              # Parameterized SQL query executor (DataTable/NonQuery/Scalar)
    │   └── SessionManager.cs        # Authenticated user session & role-based context
    │
    ├── Database/
    │   └── NeighbourHubDB_Schema_And_Seed.sql # Complete SQL Server Schema & Seed Data Script
    │
    ├── Models/                      # OOP Domain Models
    │   ├── User.cs
    │   ├── Property.cs
    │   ├── Building.cs
    │   ├── Flat.cs
    │   ├── Tenant.cs
    │   ├── Resident.cs
    │   ├── Rent.cs
    │   ├── Payment.cs
    │   ├── Complaint.cs
    │   ├── Notice.cs
    │   ├── Visitor.cs
    │   ├── MaintenanceRequest.cs
    │   ├── UtilityBill.cs
    │   ├── SharedItem.cs
    │   └── EmergencyContact.cs
    │
    ├── UI/
    │   ├── ThemeColors.cs           # Modern Navy, Royal Blue, Emerald & Slate palette
    │   └── UIHelper.cs              # DataGridView styling, Metric Card builder, MessageBox wrappers
    │
    ├── Forms/                       # WinForms User Interfaces
    │   ├── LoginForm.cs             # Branded login with database role verification
    │   ├── AdminDashboard.cs        # 12 Admin features & system-wide overview
    │   ├── PropertyOwnerDashboard.cs# 12 Owner features & financial stats breakdown
    │   ├── ManagerDashboard.cs      # 12 Manager features & daily building operations
    │   ├── ResidentDashboard.cs     # 12 Resident features & community tools
    │   ├── UserManagementForm.cs    # System user CRUD & role assignment
    │   ├── PropertyManagementForm.cs# Property CRUD & owner isolation
    │   ├── BuildingManagementForm.cs# Building & manager assignment CRUD
    │   ├── FlatManagementForm.cs    # Flat units, floor, rent amount, and status
    │   ├── TenantManagementForm.cs  # Lease allocation & flat occupancy sync
    │   ├── ResidentManagementForm.cs# Resident directory & family member tracking
    │   ├── RentManagementForm.cs    # Monthly rent billing & batch rent generation
    │   ├── PaymentManagementForm.cs # Payment recording & instant rent status update
    │   ├── ComplaintManagementForm.cs# Ticket lifecycle (Pending -> In Progress -> Solved)
    │   ├── NoticeManagementForm.cs  # Building bulletins & emergency announcements
    │   ├── VisitorManagementForm.cs # Visitor check-in/out gate log
    │   ├── MaintenanceManagementForm.cs # Scheduled repairs, contractors & expenses
    │   ├── UtilityManagementForm.cs # Electricity, water, gas & generator bill tracking
    │   ├── SharedItemManagementForm.cs# Community tool library & item lending
    │   ├── EmergencyContactForm.cs  # Emergency directory (Police, Fire, Hospital, Electrician)
    │   ├── ReportsForm.cs           # Multi-role analytics with CSV file export
    │   └── ProfileForm.cs           # User profile & password updating
    │
    └── Verification/
        └── SystemVerifier.cs        # Automated end-to-end test suite
```

---

## 🗄️ Database Relational Design (15 Tables)

- `Users`: `UserId (PK)`, `Username (Unique)`, `Password`, `FullName`, `Email`, `Phone`, `Role`, `Status`, `CreatedAt`.
- `Properties`: `PropertyId (PK)`, `OwnerUserId (FK -> Users)`, `PropertyName`, `PropertyType`, `Address`, `City`, `TotalFloors`.
- `Buildings`: `BuildingId (PK)`, `PropertyId (FK -> Properties)`, `ManagerUserId (FK -> Users)`, `BuildingName`, `BuildingCode`, `TotalUnits`, `Address`, `SecurityContact`.
- `Flats`: `FlatId (PK)`, `BuildingId (FK -> Buildings)`, `FlatNumber`, `FloorNumber`, `Bedrooms`, `Bathrooms`, `AreaSqFt`, `MonthlyRent`, `Status`, `Description`.
- `Tenants`: `TenantId (PK)`, `UserId (FK -> Users)`, `FlatId (FK -> Flats)`, `LeaseStartDate`, `LeaseEndDate`, `AgreedRent`, `SecurityDeposit`, `EmergencyContact`, `Status`.
- `Residents`: `ResidentId (PK)`, `UserId (FK -> Users)`, `FlatId (FK -> Flats)`, `TenantId (FK -> Tenants)`, `Relationship`, `NID`, `Profession`, `MoveInDate`, `Status`.
- `Rent`: `RentId (PK)`, `FlatId (FK -> Flats)`, `TenantId (FK -> Tenants)`, `Month`, `Year`, `RentAmount`, `UtilityCharges`, `DueDate`, `Status`.
- `Payments`: `PaymentId (PK)`, `RentId (FK -> Rent)`, `TenantId (FK -> Tenants)`, `FlatId (FK -> Flats)`, `AmountPaid`, `PaymentDate`, `PaymentMethod`, `TransactionRef`, `PaidBy`, `Status`, `Remarks`.
- `Complaints`: `ComplaintId (PK)`, `UserId (FK -> Users)`, `FlatId (FK -> Flats)`, `BuildingId (FK -> Buildings)`, `Category`, `Title`, `Description`, `Priority`, `Status`, `AssignedToManagerId (FK -> Users)`, `SubmittedDate`, `ResolvedDate`, `ResolutionNotes`.
- `Notices`: `NoticeId (PK)`, `BuildingId (FK -> Buildings)`, `PublishedByUserId (FK -> Users)`, `Title`, `Content`, `Category`, `Priority`, `PublishedDate`, `ExpiryDate`, `IsActive`.
- `Visitors`: `VisitorId (PK)`, `FlatId (FK -> Flats)`, `ResidentUserId (FK -> Users)`, `VisitorName`, `Phone`, `Purpose`, `CheckInTime`, `CheckOutTime`, `Status`.
- `Maintenance`: `MaintenanceId (PK)`, `BuildingId (FK -> Buildings)`, `FlatId (FK -> Flats)`, `Title`, `Description`, `Cost`, `VendorName`, `ScheduledDate`, `CompletionDate`, `Status`.
- `Utilities`: `UtilityId (PK)`, `BuildingId (FK -> Buildings)`, `FlatId (FK -> Flats)`, `UtilityType`, `BillingMonth`, `BillingYear`, `Amount`, `DueDate`, `Status`.
- `SharedItems`: `ItemId (PK)`, `OwnerUserId (FK -> Users)`, `ItemName`, `Category`, `Description`, `AvailabilityStatus`, `ContactNumber`.
- `EmergencyContacts`: `ContactId (PK)`, `BuildingId (FK -> Buildings)`, `ServiceName`, `ContactPerson`, `Phone`, `AltPhone`, `AvailableHours`, `Address`.

---

## 🎯 Viva Q&A Guide

### Q1: What architecture does this project use?
> **Answer**: The application uses a clean 3-Tier Layered Architecture:
> 1. **Presentation Layer (Forms & UI)**: Windows Forms controls, responsive layout containers, stylized DataGridViews, and role-based navigation.
> 2. **Business / Model Layer (Models & Session)**: Encapsulated domain entities and user session state.
> 3. **Data Access Layer (ADO.NET & DbHelper)**: Parameterized SQL commands, `SqlConnection`, `SqlCommand`, `SqlDataAdapter`, and `DataTable` binding.

### Q2: How does the application prevent SQL Injection?
> **Answer**: All SQL commands in `DbHelper.cs` and Form CRUD operations strictly use **SqlParameter** instances (`@Username`, `@Password`, etc.) rather than string concatenation. This ensures user inputs are treated solely as literal values and cannot manipulate query logic.

### Q3: How is Role-Based Access Control (RBAC) enforced?
> **Answer**: 
> - Authentication verifies both the password and the user's role from `dbo.Users`.
> - Upon login, the user's role and IDs are stored in `SessionManager`.
> - The application redirects the user directly to their respective Dashboard (`AdminDashboard`, `PropertyOwnerDashboard`, `ManagerDashboard`, `ResidentDashboard`).
> - Data queries are filtered dynamically based on the active user session (e.g. Property Owners only see their own properties; Residents only see their own flats and rent bills).

### Q4: How is database synchronization handled across tenant allocations?
> **Answer**: When a tenant allocation is created or activated, the system automatically updates the flat's status to `'Occupied'` and registers a corresponding record in `dbo.Residents`. When a lease is terminated or deleted, the flat status automatically resets to `'Vacant'`.
