CREATE DATABASE LIBRARY_DB;
USE LIBRARY_DB;

CREATE TABLE Students (
    StudentId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100),
    Branch NVARCHAR(50)
);

CREATE TABLE Books (
    BookId INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(150),
    Publisher NVARCHAR(100),
    Genre NVARCHAR(50),
    IsAvailable BIT DEFAULT 1
);

CREATE TABLE BorrowRecords (
    BorrowId INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT,
    BookId INT,
    BorrowDate DATETIME DEFAULT GETDATE(),
    ReturnDate DATETIME NULL,
    FOREIGN KEY (StudentId) REFERENCES Students(StudentId),
    FOREIGN KEY (BookId) REFERENCES Books(BookId)
);

-- ─────────────────────────────────────────────────────────────────
-- NEW: Users table for registration / login
-- ─────────────────────────────────────────────────────────────────
CREATE TABLE Users (
    UserId       INT PRIMARY KEY IDENTITY(1,1),
    Username     NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Salt         NVARCHAR(64)  NOT NULL,
    Email        NVARCHAR(200) NULL,
    CreatedAt    DATETIME      DEFAULT GETUTCDATE()
);

-- Sample data
INSERT INTO Students (Name, Branch) VALUES
('Rahul', 'CSE'),
('Aman', 'IT'),
('Sneha', 'ECE'),
('Priya', 'CSE'),
('Karan', 'ME');

INSERT INTO Books (Title, Publisher, Genre) VALUES
('DBMS Basics', 'Pearson', 'Education'),
('C# in Depth', 'Manning', 'Programming'),
('Data Structures', 'OReilly', 'Education'),
('Clean Code', 'Prentice Hall', 'Programming'),
('AI Intro', 'Springer', 'AI');

INSERT INTO BorrowRecords (StudentId, BookId)
VALUES (1, 2);

UPDATE Books SET IsAvailable = 0 WHERE BookId = 2;

UPDATE BorrowRecords
SET ReturnDate = GETDATE()
WHERE BookId = 2 AND ReturnDate IS NULL;

UPDATE Books SET IsAvailable = 1 WHERE BookId = 2;

-- NOTE: Users are created through the /Account/Register page.
-- To manually add an admin user, register via the app first,
-- then you can update their record here if needed.
-- Example (do NOT store plain text — use the app to register):
-- The app stores: SHA256(Salt + Password) as PasswordHash
