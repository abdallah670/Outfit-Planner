-- SQL Script to fix FeedPost columns issue
-- This script will manually add the missing columns to the FeedPosts table
-- Handles existing default constraints before dropping columns

-- ========================================
-- PART 1: Handle CommentsCount column
-- ========================================

-- Drop the default constraint on CommentsCount dynamically
DECLARE @constraintName NVARCHAR(256)
SELECT @constraintName = name 
FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('FeedPosts') 
  AND col_name(parent_object_id, parent_column_id) = 'CommentsCount'

IF @constraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE FeedPosts DROP CONSTRAINT [' + @constraintName + ']')
    PRINT 'Dropped default constraint ' + @constraintName + ' on CommentsCount'
END

-- Drop the CommentsCount column if it exists
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FeedPosts') AND name = 'CommentsCount')
BEGIN
    ALTER TABLE FeedPosts DROP COLUMN CommentsCount
    PRINT 'Dropped CommentsCount column'
END

-- Re-add CommentsCount with default 0
ALTER TABLE FeedPosts ADD CommentsCount INT NOT NULL DEFAULT 0
PRINT 'Added CommentsCount column with default value 0'

-- Recreate the index for LikesCount (if it was dropped previously)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('FeedPosts') AND name = 'IX_FeedPosts_LikesCount')
BEGIN
    CREATE INDEX IX_FeedPosts_LikesCount ON FeedPosts (LikesCount)
    PRINT 'Recreated index IX_FeedPosts_LikesCount'
END

-- Verify the columns were added
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'FeedPosts' 
    AND COLUMN_NAME IN ('LikesCount', 'CommentsCount')
ORDER BY COLUMN_NAME

PRINT ''
PRINT 'FeedPost columns have been successfully fixed!'
PRINT 'You can now run the application and the DataSeeder should work correctly.'