-- Check current database schema for all tables
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME IN ('FeedPosts', 'Outfits', 'TrendingOutfits') 
    AND COLUMN_NAME IN ('LikesCount', 'CommentsCount')
ORDER BY TABLE_NAME, COLUMN_NAME
