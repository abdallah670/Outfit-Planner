-- =============================================================
-- FixOutfitAndTrendingCounts.sql
-- 
-- Purpose: Correct LikesCount and CommentsCount on:
--   1. Outfits table - aggregate from all related FeedPosts
--   2. TrendingOutfits table - refresh from latest FeedPost data
--
-- The app code updates these when a reaction/comment is added or
-- deleted, but existing data may have stale values.
-- =============================================================

BEGIN TRANSACTION;

PRINT '=== Fixing Outfits.LikesCount and Outfits.CommentsCount ===';

-- Correct Outfits count columns from all related FeedPosts
-- An outfit can have multiple posts (outfit post + poll posts), so we SUM them
UPDATE o
SET
    o.LikesCount = sub.TotalLikes,
    o.CommentsCount = sub.TotalComments
FROM Outfits o
INNER JOIN (
    SELECT
        fp.OutfitId,
        SUM(fp.LikesCount) AS TotalLikes,
        SUM(fp.CommentsCount) AS TotalComments
    FROM FeedPosts fp
    WHERE fp.OutfitId IS NOT NULL
    GROUP BY fp.OutfitId
) sub ON o.Id = sub.OutfitId;

-- For outfits with no FeedPosts at all, reset counts to 0
UPDATE Outfits
SET LikesCount = 0, CommentsCount = 0
WHERE Id NOT IN (
    SELECT DISTINCT OutfitId FROM FeedPosts WHERE OutfitId IS NOT NULL
);

PRINT CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' outfits updated';

-- ========================================
-- PART 2: Correct TrendingOutfits counts
-- ========================================

PRINT '';
PRINT '=== Fixing TrendingOutfits.LikesCount and TrendingOutfits.CommentsCount ===';

-- Correct from current FeedPost data (the snapshot should reflect latest values)
UPDATE t
SET
    t.LikesCount = fp.LikesCount,
    t.CommentsCount = fp.CommentsCount,
    t.TrendingScore = (fp.LikesCount * 5) + (fp.CommentsCount * 2) + 1.0
FROM TrendingOutfits t
INNER JOIN FeedPosts fp ON 
    (t.PollId IS NOT NULL AND t.PollId = fp.PollId)
    OR (t.PollId IS NULL AND t.OutfitId = fp.OutfitId AND fp.PollId IS NULL);

PRINT CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' trending outfits updated';

-- ========================================
-- PART 3: Verification
-- ========================================

PRINT '';
PRINT '=== Verification: Corrected Outfits ===';
SELECT 
    o.Id AS OutfitId,
    LEFT(o.Name, 30) AS OutfitName,
    o.LikesCount,
    o.CommentsCount,
    (SELECT COUNT(*) FROM FeedPosts fp WHERE fp.OutfitId = o.Id) AS RelatedPosts
FROM Outfits o
ORDER BY o.LikesCount DESC;

PRINT '';
PRINT '=== Verification: Corrected TrendingOutfits ===';
SELECT 
    t.Id AS TrendingId,
    t.OutfitId,
    t.PollId,
    t.LikesCount,
    t.CommentsCount,
    t.TrendingScore,
    t.RankPosition,
    t.Date
FROM TrendingOutfits t
ORDER BY t.Date DESC, t.RankPosition ASC;

PRINT '';
PRINT '=== Done - Outfit and TrendingOutfit counts corrected ===';

COMMIT TRANSACTION;
GO