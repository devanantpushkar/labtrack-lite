-- LabTrack Lite Database Schema
-- SQLite Compatible

-- Users table
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL CHECK(Role IN ('Admin', 'Engineer', 'Technician')),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Assets table
CREATE TABLE IF NOT EXISTS Assets (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    QRCode TEXT UNIQUE,
    Status TEXT NOT NULL CHECK(Status IN ('Available', 'InUse', 'Maintenance', 'Retired')),
    Location TEXT,
    Category TEXT,
    CreatedBy INTEGER NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

-- Tickets table
CREATE TABLE IF NOT EXISTS Tickets (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Description TEXT,
    Status TEXT NOT NULL CHECK(Status IN ('Open', 'InProgress', 'Resolved', 'Closed')),
    Priority TEXT NOT NULL CHECK(Priority IN ('Low', 'Medium', 'High', 'Critical')),
    AssetId INTEGER,
    CreatedBy INTEGER NOT NULL,
    AssignedTo INTEGER,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (AssetId) REFERENCES Assets(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (AssignedTo) REFERENCES Users(Id)
);

-- Comments table
CREATE TABLE IF NOT EXISTS Comments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TicketId INTEGER NOT NULL,
    UserId INTEGER NOT NULL,
    Content TEXT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (TicketId) REFERENCES Tickets(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Audit Logs table
CREATE TABLE IF NOT EXISTS AuditLogs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER,
    Action TEXT NOT NULL,
    EntityType TEXT NOT NULL,
    EntityId INTEGER,
    Details TEXT,
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_users_role ON Users(Role);
CREATE INDEX IF NOT EXISTS idx_assets_status ON Assets(Status);
CREATE INDEX IF NOT EXISTS idx_assets_category ON Assets(Category);
CREATE INDEX IF NOT EXISTS idx_tickets_status ON Tickets(Status);
CREATE INDEX IF NOT EXISTS idx_tickets_priority ON Tickets(Priority);
CREATE INDEX IF NOT EXISTS idx_tickets_assigned ON Tickets(AssignedTo);
CREATE INDEX IF NOT EXISTS idx_comments_ticket ON Comments(TicketId);
CREATE INDEX IF NOT EXISTS idx_audit_entity ON AuditLogs(EntityType, EntityId);
