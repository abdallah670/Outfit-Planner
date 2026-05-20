-- SQL Script to add missing LikesCount and CommentsCount columns to Outfits and TrendingOutfits tables

PRINT 'Adding missing columns to Outfits and TrendingOutfits tables...'

-- ========================================
-- PART 1: Fix Outfits table
-- ========================================

-- Add LikesCount column to Outfits table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Outfits') AND name = 'LikesCount')
BEGIN
    PRINT 'LikesCount column already exists in Outfits table'
END
ELSE
BEGIN
    ALTER TABLE Outfits ADD LikesCount INT NOT NULL DEFAULT 0
    PRINT 'Added LikesCount column to Outfits table'
END

-- Add CommentsCount column to Outfits table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Outfits') AND name = 'CommentsCount')
BEGIN
    PRINT 'CommentsCount column already exists in Outfits table'
END
ELSE
BEGIN
    ALTER TABLE Outfits ADD CommentsCount INT NOT NULL DEFAULT 0
    PRINT 'Added CommentsCount column to Outfits table'
END

-- ========================================
-- PART 2: Fix TrendingOutfits table
-- ========================================

-- Add LikesCount column to TrendingOutfits table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TrendingOutfits') AND name = 'LikesCount')
BEGIN
    PRINT 'LikesCount column already exists in TrendingOutfits table'
END
ELSE
BEGIN
    ALTER TABLE TrendingOutfits ADD LikesCount INT NOT NULL DEFAULT 0
    PRINT 'Added LikesCount column to TrendingOutfits table'
END

-- Add CommentsCount column to TrendingOutfits table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TrendingOutfits') AND name = 'CommentsCount')
BEGIN
    PRINT 'CommentsCount column already exists in TrendingOutfits table'
END
ELSE
BEGIN
    ALTER TABLE TrendingOutfits ADD CommentsCount INT NOT NULL DEFAULT 0
    PRINT 'Added CommentsCount column to TrendingOutfits table'
END

-- ========================================
-- PART 3: Verification
-- ========================================

PRINT ''
PRINT '=== VERIFICATION ==='

-- Check Outfits table
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

-- Check TrendingOutfits table
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
PRINT 'Outfit and TrendingOutfit columns have been successfully added!'
PRINT 'You can now run the application and the DataSeeder should work correctly.'
