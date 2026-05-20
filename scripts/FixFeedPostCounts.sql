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

PRINT 'Updated CommentsCount from actual PostComments data'

-- ========================================
-- PART 2: Correct LikesCount (Heart reactions)
-- ========================================
UPDATE fp
SET fp.LikesCount = (
    SELECT COUNT(*)
    FROM PostReactions pr
    WHERE pr.PostId = fp.Id AND pr.ReactionType = 0
)
FROM FeedPosts fp

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
ORDER BY PostType, CreatedAt DESC

PRINT ''
PRINT 'FeedPost counts have been successfully corrected!'