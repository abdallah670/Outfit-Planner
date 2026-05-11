-- SQL Script to fix all missing LikesCount and CommentsCount columns
-- This script will add missing columns to FeedPosts, Outfits, and TrendingOutfits tables

PRINT 'Starting to fix all count columns...'

-- ========================================
-- PART 1: Fix FeedPosts table
-- ========================================

-- Drop any dependent indexes first
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('FeedPosts') AND name = 'IX_FeedPosts_LikesCount')
BEGIN
    DROP INDEX IX_FeedPosts_LikesCount ON FeedPosts
    PRINT 'Dropped index IX_FeedPosts_LikesCount from FeedPosts'
END

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('FeedPosts') AND name = 'IX_FeedPosts_CommentsCount')
BEGIN
    DROP INDEX IX_FeedPosts_CommentsCount ON FeedPosts
    PRINT 'Dropped index IX_FeedPosts_CommentsCount from FeedPosts'
END

-- Drop default constraints and columns if they exist
DECLARE @constraintName NVARCHAR(256)

-- Handle LikesCount
SELECT @constraintName = name 
FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('FeedPosts') 
  AND col_name(parent_object_id, parent_column_id) = 'LikesCount'

IF @constraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE FeedPosts DROP CONSTRAINT [' + @constraintName + ']')
    PRINT 'Dropped default constraint on FeedPosts.LikesCount'
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FeedPosts') AND name = 'LikesCount')
BEGIN
    ALTER TABLE FeedPosts DROP COLUMN LikesCount
    PRINT 'Dropped LikesCount column from FeedPosts'
END

-- Handle CommentsCount
SELECT @constraintName = name 
FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('FeedPosts') 
  AND col_name(parent_object_id, parent_column_id) = 'CommentsCount'

IF @constraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE FeedPosts DROP CONSTRAINT [' + @constraintName + ']')
    PRINT 'Dropped default constraint on FeedPosts.CommentsCount'
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FeedPosts') AND name = 'CommentsCount')
BEGIN
    ALTER TABLE FeedPosts DROP COLUMN CommentsCount
    PRINT 'Dropped CommentsCount column from FeedPosts'
END

-- Add columns back
ALTER TABLE FeedPosts ADD LikesCount INT NOT NULL DEFAULT 0
PRINT 'Added LikesCount column to FeedPosts'

ALTER TABLE FeedPosts ADD CommentsCount INT NOT NULL DEFAULT 0
PRINT 'Added CommentsCount column to FeedPosts'

-- Recreate indexes
CREATE INDEX IX_FeedPosts_LikesCount ON FeedPosts (LikesCount)
PRINT 'Recreated index IX_FeedPosts_LikesCount'

CREATE INDEX IX_FeedPosts_CommentsCount ON FeedPosts (CommentsCount)
PRINT 'Recreated index IX_FeedPosts_CommentsCount'

-- ========================================
-- PART 2: Fix Outfits table
-- ========================================

-- Drop default constraints and columns if they exist
-- Handle LikesCount
SELECT @constraintName = name 
FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('Outfits') 
  AND col_name(parent_object_id, parent_column_id) = 'LikesCount'

IF @constraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE Outfits DROP CONSTRAINT [' + @constraintName + ']')
    PRINT 'Dropped default constraint on Outfits.LikesCount'
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Outfits') AND name = 'LikesCount')
BEGIN
    ALTER TABLE Outfits DROP COLUMN LikesCount
    PRINT 'Dropped LikesCount column from Outfits'
END

-- Handle CommentsCount
SELECT @constraintName = name 
FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('Outfits') 
  AND col_name(parent_object_id, parent_column_id) = 'CommentsCount'

IF @constraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE Outfits DROP CONSTRAINT [' + @constraintName + ']')
    PRINT 'Dropped default constraint on Outfits.CommentsCount'
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Outfits') AND name = 'CommentsCount')
BEGIN
    ALTER TABLE Outfits DROP COLUMN CommentsCount
    PRINT 'Dropped CommentsCount column from Outfits'
END

-- Add columns back
ALTER TABLE Outfits ADD LikesCount INT NOT NULL DEFAULT 0
PRINT 'Added LikesCount column to Outfits'

ALTER TABLE Outfits ADD CommentsCount INT NOT NULL DEFAULT 0
PRINT 'Added CommentsCount column to Outfits'

-- ========================================
-- PART 3: Fix TrendingOutfits table
-- ========================================

-- Drop default constraints and columns if they exist
-- Handle LikesCount
SELECT @constraintName = name 
FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('TrendingOutfits') 
  AND col_name(parent_object_id, parent_column_id) = 'LikesCount'

IF @constraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE TrendingOutfits DROP CONSTRAINT [' + @constraintName + ']')
    PRINT 'Dropped default constraint on TrendingOutfits.LikesCount'
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TrendingOutfits') AND name = 'LikesCount')
BEGIN
    ALTER TABLE TrendingOutfits DROP COLUMN LikesCount
    PRINT 'Dropped LikesCount column from TrendingOutfits'
END

-- Handle CommentsCount
SELECT @constraintName = name 
FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('TrendingOutfits') 
  AND col_name(parent_object_id, parent_column_id) = 'CommentsCount'

IF @constraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE TrendingOutfits DROP CONSTRAINT [' + @constraintName + ']')
    PRINT 'Dropped default constraint on TrendingOutfits.CommentsCount'
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TrendingOutfits') AND name = 'CommentsCount')
BEGIN
    ALTER TABLE TrendingOutfits DROP COLUMN CommentsCount
    PRINT 'Dropped CommentsCount column from TrendingOutfits'
END

-- Add columns back
ALTER TABLE TrendingOutfits ADD LikesCount INT NOT NULL DEFAULT 0
PRINT 'Added LikesCount column to TrendingOutfits'

ALTER TABLE TrendingOutfits ADD CommentsCount INT NOT NULL DEFAULT 0
PRINT 'Added CommentsCount column to TrendingOutfits'

-- ========================================
-- PART 4: Verification
-- ========================================

PRINT ''
PRINT '=== VERIFICATION ==='

-- Check FeedPosts
SELECT 
    'FeedPosts' as TableName,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'FeedPosts' 
    AND COLUMN_NAME IN ('LikesCount', 'CommentsCount')
ORDER BY COLUMN_NAME

-- Check Outfits
SELECT 
    'Outfits' as TableName,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Outfits' 
    AND COLUMN_NAME IN ('LikesCount', 'CommentsCount')
ORDER BY COLUMN_NAME

-- Check TrendingOutfits
SELECT 
    'TrendingOutfits' as TableName,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'TrendingOutfits' 
    AND COLUMN_NAME IN ('LikesCount', 'CommentsCount')
ORDER BY COLUMN_NAME

PRINT ''
PRINT 'All count columns have been successfully fixed!'
PRINT 'You can now run the application and the DataSeeder should work correctly.'
