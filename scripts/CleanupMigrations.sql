-- SQL Script to clean up empty migrations from EF Core history
-- This removes the empty migrations that were causing issues

-- Remove the empty migrations from __EFMigrationsHistory
DELETE FROM __EFMigrationsHistory 
WHERE MigrationId IN (
    '20260509213944_AddFeedPostCounts',
    '20260509214900_AddMissingFeedPostColumns', 
    '20260509215016_MarkFeedPostColumnsAsExisting'
)

PRINT 'Removed empty migrations from EF Core history table'
PRINT 'Now you can run: dotnet ef database update AddFeedPostCounts'
PRINT 'This will apply the corrected migration with the actual column definitions'
