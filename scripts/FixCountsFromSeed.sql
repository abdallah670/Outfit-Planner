-- =============================================================================
-- FixCountsFromSeed.sql
-- =============================================================================
-- Fixes denormalized count columns that were seeded with arbitrary random values
-- instead of reflecting the real rows in child tables.
--
-- Tables fixed
--   1. FeedPosts       - LikesCount    (actual PostReactions rows)
--                      - CommentsCount (actual PostComments rows)
--   2. Outfits         - LikesCount    (reactions on the outfit's linked FeedPost)
--                      - CommentsCount (comments on the outfit's linked FeedPost)
--   3. ValidationPolls - TotalVotes    (sum of Votes rows across all PollOptions)
--   4. PostComments    - TotalReplies  (child PostComments with ParentCommentId = Id)
--   5. TrendingOutfits - LikesCount / CommentsCount (mirror from linked FeedPost)
-- =============================================================================

SET NOCOUNT ON;
PRINT '============================================================';
PRINT 'FixCountsFromSeed.sql - starting reconciliation';
PRINT '============================================================';

-- ─────────────────────────────────────────────────────────────────────────────
-- 1. FeedPosts.LikesCount  ->  COUNT(PostReactions.Id) per PostId
--    The seeder assigned random.Next(5,50) / random.Next(3,30) but the actual
--    reactions inserted are far fewer (1-3 per post from 3 seeded users).
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '1. Updating FeedPosts.LikesCount from PostReactions...';

UPDATE fp
SET    fp.LikesCount = ISNULL(r.ActualCount, 0)
FROM   FeedPosts fp
LEFT JOIN (
    SELECT PostId, COUNT(*) AS ActualCount
    FROM   PostReactions
    GROUP BY PostId
) r ON r.PostId = fp.Id;

PRINT '   Done. Rows affected: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

-- ─────────────────────────────────────────────────────────────────────────────
-- 2. FeedPosts.CommentsCount  ->  COUNT(PostComments.Id) per PostId
--    The seeder assigned random.Next(0,10) / random.Next(1,8) but actual
--    comments seeded are 0-3 per post.
--    Counts ALL comments (including replies) so the badge matches the total.
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '2. Updating FeedPosts.CommentsCount from PostComments...';

UPDATE fp
SET    fp.CommentsCount = ISNULL(c.ActualCount, 0)
FROM   FeedPosts fp
LEFT JOIN (
    SELECT PostId, COUNT(*) AS ActualCount
    FROM   PostComments
    GROUP BY PostId
) c ON c.PostId = fp.Id;

PRINT '   Done. Rows affected: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

-- ─────────────────────────────────────────────────────────────────────────────
-- 3. Outfits.LikesCount  ->  sum of LikesCount on FeedPost(s) that link to outfit
--    The seeder left Outfits.LikesCount at default 0 even though the outfit
--    is published as a FeedPost with reactions attached.
--    An outfit could theoretically appear in multiple posts; SUM covers that.
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '3. Updating Outfits.LikesCount from their linked FeedPost(s)...';

UPDATE o
SET    o.LikesCount = ISNULL(agg.TotalLikes, 0)
FROM   Outfits o
LEFT JOIN (
    SELECT fp.OutfitId, SUM(fp.LikesCount) AS TotalLikes
    FROM   FeedPosts fp
    WHERE  fp.OutfitId IS NOT NULL
    GROUP BY fp.OutfitId
) agg ON agg.OutfitId = o.Id;

PRINT '   Done. Rows affected: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

-- ─────────────────────────────────────────────────────────────────────────────
-- 4. Outfits.CommentsCount  ->  sum of CommentsCount on linked FeedPost(s)
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '4. Updating Outfits.CommentsCount from their linked FeedPost(s)...';

UPDATE o
SET    o.CommentsCount = ISNULL(agg.TotalComments, 0)
FROM   Outfits o
LEFT JOIN (
    SELECT fp.OutfitId, SUM(fp.CommentsCount) AS TotalComments
    FROM   FeedPosts fp
    WHERE  fp.OutfitId IS NOT NULL
    GROUP BY fp.OutfitId
) agg ON agg.OutfitId = o.Id;

PRINT '   Done. Rows affected: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

-- ─────────────────────────────────────────────────────────────────────────────
-- 5. ValidationPolls.TotalVotes  ->  COUNT(Votes.Id) per PollId
--    The seeder inserts Vote rows per PollOption but never increments TotalVotes
--    (it stays at the entity default of 0).
--    Votes carry a PollId foreign key directly, so we group on that.
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '5. Updating ValidationPolls.TotalVotes from Votes table...';

UPDATE vp
SET    vp.TotalVotes = ISNULL(v.ActualCount, 0)
FROM   ValidationPolls vp
LEFT JOIN (
    SELECT PollId, COUNT(*) AS ActualCount
    FROM   Votes
    GROUP BY PollId
) v ON v.PollId = vp.Id;

PRINT '   Done. Rows affected: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

-- ─────────────────────────────────────────────────────────────────────────────
-- 6. PostComments.TotalReplies  ->  COUNT of child comments per ParentCommentId
--    The seeder never sets TotalReplies; seed data has no nested replies,
--    but this future-proofs the column for any later data.
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '6. Updating PostComments.TotalReplies from child PostComments...';

UPDATE parent
SET    parent.TotalReplies = ISNULL(r.ActualCount, 0)
FROM   PostComments parent
LEFT JOIN (
    SELECT ParentCommentId, COUNT(*) AS ActualCount
    FROM   PostComments
    WHERE  ParentCommentId IS NOT NULL
    GROUP BY ParentCommentId
) r ON r.ParentCommentId = parent.Id;

PRINT '   Done. Rows affected: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

-- ─────────────────────────────────────────────────────────────────────────────
-- 7. TrendingOutfits.LikesCount / CommentsCount  ->  mirror the linked FeedPost
--    NOTE: Requires migration 20260622181314_RefactorTrendingOutfitsToFeedPost
--    to have been applied first (replaces OutfitId/PollId with FeedPostId).
--    Now that FeedPosts counts are correct (steps 1-2), copy them across.
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '7. Updating TrendingOutfits counts from their linked FeedPost...';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TrendingOutfits') AND name = 'FeedPostId')
BEGIN
    UPDATE tr
    SET    tr.LikesCount    = fp.LikesCount,
           tr.CommentsCount = fp.CommentsCount
    FROM   TrendingOutfits tr
    INNER JOIN FeedPosts fp ON fp.Id = tr.FeedPostId;

    PRINT '   Done. Rows affected: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));
END
ELSE
BEGIN
    PRINT '   SKIPPED - FeedPostId column not found. Apply migration 20260622181314_RefactorTrendingOutfitsToFeedPost first.';
END

-- =============================================================================
-- VERIFICATION REPORT
-- Shows the reconciled values so you can confirm everything looks right.
-- =============================================================================
PRINT '';
PRINT '============================================================';
PRINT 'VERIFICATION REPORT';
PRINT '============================================================';

PRINT '';
PRINT '--- FeedPosts (LikesCount vs actual PostReactions, CommentsCount vs actual PostComments) ---';
SELECT
    fp.Id,
    CAST(fp.PostType AS NVARCHAR(20))  AS PostType,
    fp.LikesCount                      AS LikesCount_Stored,
    COUNT(DISTINCT r.Id)               AS ActualReactions,
    fp.CommentsCount                   AS CommentsCount_Stored,
    COUNT(DISTINCT c.Id)               AS ActualComments
FROM      FeedPosts      fp
LEFT JOIN PostReactions   r ON r.PostId = fp.Id
LEFT JOIN PostComments    c ON c.PostId = fp.Id
GROUP BY  fp.Id, fp.PostType, fp.LikesCount, fp.CommentsCount
ORDER BY  fp.PostType, fp.Id;

PRINT '';
PRINT '--- Outfits (LikesCount and CommentsCount) ---';
SELECT
    o.Id,
    o.Name,
    o.LikesCount,
    o.CommentsCount
FROM  Outfits o
ORDER BY o.Id;

PRINT '';
PRINT '--- ValidationPolls (TotalVotes vs actual Votes) ---';
SELECT
    vp.Id,
    vp.Question,
    vp.TotalVotes                    AS TotalVotes_Stored,
    COUNT(v.Id)                      AS ActualVotesInTable
FROM      ValidationPolls vp
LEFT JOIN Votes           v  ON v.PollId = vp.Id
GROUP BY  vp.Id, vp.Question, vp.TotalVotes
ORDER BY  vp.Id;

PRINT '';
PRINT '--- PostComments with TotalReplies (only those with replies shown) ---';
SELECT
    pc.Id,
    LEFT(pc.Content, 50)             AS ContentPreview,
    pc.TotalReplies                  AS TotalReplies_Stored,
    COUNT(ch.Id)                     AS ActualRepliesInTable
FROM      PostComments  pc
LEFT JOIN PostComments  ch ON ch.ParentCommentId = pc.Id
GROUP BY  pc.Id, pc.Content, pc.TotalReplies
HAVING    pc.TotalReplies > 0 OR COUNT(ch.Id) > 0
ORDER BY  pc.Id;

PRINT '';
PRINT '--- TrendingOutfits (should mirror their FeedPost) ---';
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TrendingOutfits') AND name = 'FeedPostId')
BEGIN
    SELECT
        tr.Id,
        tr.RankPosition,
        tr.TrendingScore,
        tr.LikesCount,
        tr.CommentsCount,
        fp.LikesCount    AS FeedPost_LikesCount,
        fp.CommentsCount AS FeedPost_CommentsCount
    FROM  TrendingOutfits tr
    JOIN  FeedPosts       fp ON fp.Id = tr.FeedPostId
    ORDER BY tr.RankPosition;
END
ELSE
BEGIN
    PRINT '   (TrendingOutfits still uses OutfitId/PollId - migration not yet applied)';
    SELECT Id, RankPosition, TrendingScore, LikesCount, CommentsCount FROM TrendingOutfits ORDER BY RankPosition;
END

PRINT '';
PRINT '============================================================';
PRINT 'FixCountsFromSeed.sql - reconciliation COMPLETE';
PRINT '============================================================';
-- Step 1: Delete duplicate votes - keep only the most recent vote per user per poll
DELETE v FROM Votes v
INNER JOIN (
    SELECT 
        VoterId, 
        PollId,
        Id,
        ROW_NUMBER() OVER (PARTITION BY VoterId, PollId ORDER BY CreatedAt DESC) AS rn
    FROM Votes
) ranked ON v.Id = ranked.Id
WHERE ranked.rn > 1