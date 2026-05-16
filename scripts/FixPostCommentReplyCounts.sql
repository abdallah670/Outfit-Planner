-- =============================================================
-- FixPostCommentReplyCounts.sql
-- 
-- Purpose: Correct TotalReplies column on PostComments table
--          by recursively counting ALL descendants (replies at any depth).
--
-- The migration added TotalReplies with default 0, but the app
-- code never increments it when a reply is added.
--
-- This script:
--   1. Uses a recursive CTE to flatten the comment hierarchy
--   2. Counts ALL descendants (direct replies + replies-of-replies, etc.)
--   3. Updates TotalReplies for every comment that has descendants
--   4. Sets TotalReplies = 0 for comments with no descendants
--   5. Shows a verification report
-- =============================================================

BEGIN TRANSACTION;

-- Recursive CTE: for each comment, find ALL descendants at any depth
WITH CommentHierarchy AS (
    -- Anchor: direct replies (comments that have a ParentCommentId)
    SELECT 
        Id AS DescendantId,
        ParentCommentId AS RootParentId,
        1 AS Level
    FROM PostComments
    WHERE ParentCommentId IS NOT NULL

    UNION ALL

    -- Recursive: find replies to those replies, carrying up the original RootParentId
    SELECT 
        pc.Id AS DescendantId,
        ch.RootParentId,
        ch.Level + 1
    FROM PostComments pc
    INNER JOIN CommentHierarchy ch ON pc.ParentCommentId = ch.DescendantId
)
-- Update TotalReplies with the total descendant count per comment
UPDATE pc
SET pc.TotalReplies = sub.TotalDescendants
FROM PostComments pc
INNER JOIN (
    SELECT 
        RootParentId AS CommentId, 
        COUNT(*) AS TotalDescendants
    FROM CommentHierarchy
    GROUP BY RootParentId
) sub ON pc.Id = sub.CommentId;

-- For comments with no descendants (not covered by the UPDATE above),
-- ensure TotalReplies stays at 0 (or reset any incorrectly set values)
UPDATE PostComments
SET TotalReplies = 0
WHERE Id NOT IN (
    SELECT DISTINCT RootParentId FROM CommentHierarchy
);

COMMIT TRANSACTION;

-- =============================================================
-- Verification Report
-- =============================================================
PRINT '=== Verification Report: PostComment TotalReplies ===';
PRINT '';

-- Show top-level comments with their total reply counts
SELECT 
    pc.Id AS CommentId,
    LEFT(pc.Content, 50) AS ContentPreview,
    pc.ParentCommentId,
    pc.TotalReplies,
    CASE 
        WHEN pc.TotalReplies = 0 THEN 'No replies'
        WHEN pc.TotalReplies = 1 THEN '1 total reply'
        ELSE CAST(pc.TotalReplies AS NVARCHAR(10)) + ' total replies'
    END AS ReplySummary
FROM PostComments pc
ORDER BY pc.TotalReplies DESC;

-- Show summary stats
SELECT 
    COUNT(*) AS TotalComments,
    SUM(TotalReplies) AS TotalReplyRelationships,
    MAX(TotalReplies) AS MaxRepliesOnSingleComment
FROM PostComments;

PRINT '';
PRINT '=== Done ===';
GO