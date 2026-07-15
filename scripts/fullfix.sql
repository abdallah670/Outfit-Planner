-- SQL Script to correct LikesCount and CommentsCount in FeedPosts
-- Recalculates from actual data in PostReactions and PostComments tables

-- ========================================
-- PART 1: Correct CommentsCount
-- ========================================
UPDATE fp
SET fp.CommentsCount = (
    SELECT COUNT(*)
    FROM PostComments pc
    WHERE pc.PostId = fp.Id AND pc.IsDeleted = 0
)
FROM FeedPosts fp
WHERE fp.IsDeleted = 0

PRINT 'Updated CommentsCount from actual PostComments data'

-- ========================================
-- PART 2: Correct LikesCount (Heart reactions)
-- ========================================
UPDATE fp
SET fp.LikesCount = (
    SELECT COUNT(*)
    FROM PostReactions pr
    WHERE pr.PostId = fp.Id AND pr.ReactionType = 0 AND pr.IsDeleted = 0
)
FROM FeedPosts fp
WHERE fp.IsDeleted = 0

PRINT 'Updated LikesCount from actual PostReactions data (Heart reactions)'

-- ========================================
-- PART 3: Verify the updates
-- ========================================
SELECT 
    Id,
    PostType,
    LikesCount AS CurrentLikes,
    CommentsCount AS CurrentComments
FROM FeedPosts
WHERE IsDeleted = 0
ORDER BY PostType, CreatedAt DESC

PRINT ''
PRINT 'FeedPost counts have been successfully corrected!'
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
    o.LikesCount = ISNULL(sub.TotalLikes, 0),
    o.CommentsCount = ISNULL(sub.TotalComments, 0)
FROM Outfits o
LEFT JOIN (
    SELECT
        fp.OutfitId,
        SUM(fp.LikesCount) AS TotalLikes,
        SUM(fp.CommentsCount) AS TotalComments
    FROM FeedPosts fp
    WHERE fp.OutfitId IS NOT NULL AND fp.IsDeleted = 0
    GROUP BY fp.OutfitId
) sub ON o.Id = sub.OutfitId
WHERE o.IsDeleted = 0;

-- For outfits with no FeedPosts at all, reset counts to 0
UPDATE Outfits
SET LikesCount = 0, CommentsCount = 0
WHERE Id NOT IN (
    SELECT DISTINCT OutfitId FROM FeedPosts WHERE OutfitId IS NOT NULL AND IsDeleted = 0
) AND IsDeleted = 0;

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
INNER JOIN FeedPosts fp ON t.FeedPostId = fp.Id
WHERE fp.IsDeleted = 0 AND t.IsDeleted = 0;

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
    (SELECT COUNT(*) FROM FeedPosts fp WHERE fp.OutfitId = o.Id AND fp.IsDeleted = 0) AS RelatedPosts
FROM Outfits o
WHERE o.IsDeleted = 0
ORDER BY o.LikesCount DESC;

PRINT '';
PRINT '=== Verification: Corrected TrendingOutfits ===';
SELECT 
    t.Id AS TrendingId,
    t.FeedPostId,
    t.PostType,
    t.LikesCount,
    t.CommentsCount,
    t.TrendingScore,
    t.RankPosition,
    t.Date
FROM TrendingOutfits t
WHERE t.IsDeleted = 0
ORDER BY t.Date DESC, t.RankPosition ASC;

PRINT '';
PRINT '=== Done - Outfit and TrendingOutfit counts corrected ===';

COMMIT TRANSACTION;
GO
-- Step 1: Delete duplicate votes - keep only the most recent vote per user per poll
DELETE v FROM Votes v
INNER JOIN (
    SELECT 
        VoterId, 
        PollId,
        Id,
        ROW_NUMBER() OVER (PARTITION BY VoterId, PollId ORDER BY CreatedAt DESC) AS rn
    FROM Votes
    WHERE IsDeleted = 0
) ranked ON v.Id = ranked.Id
WHERE ranked.rn > 1

-- Step 2: Fix TotalVotes in ValidationPolls by counting actual distinct voter votes per poll
UPDATE vp
SET vp.TotalVotes = vote_counts.ActualVoteCount
FROM ValidationPolls vp
INNER JOIN (
    SELECT v.PollId, COUNT(DISTINCT v.VoterId) AS ActualVoteCount
    FROM Votes v
    WHERE v.IsDeleted = 0
    GROUP BY v.PollId
) vote_counts ON vp.Id = vote_counts.PollId
WHERE vp.IsDeleted = 0

-- Step 3: For polls that have no votes at all, set TotalVotes to 0
UPDATE ValidationPolls
SET TotalVotes = 0
WHERE Id NOT IN (SELECT DISTINCT PollId FROM Votes WHERE IsDeleted = 0)
AND IsDeleted = 0
