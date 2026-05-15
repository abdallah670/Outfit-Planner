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

-- Step 2: Fix TotalVotes in ValidationPolls by counting actual distinct voter votes per poll
UPDATE vp
SET vp.TotalVotes = vote_counts.ActualVoteCount
FROM ValidationPolls vp
INNER JOIN (
    SELECT v.PollId, COUNT(DISTINCT v.VoterId) AS ActualVoteCount
    FROM Votes v
    GROUP BY v.PollId
) vote_counts ON vp.Id = vote_counts.PollId

-- Step 3: For polls that have no votes at all, set TotalVotes to 0
UPDATE ValidationPolls
SET TotalVotes = 0
WHERE Id NOT IN (SELECT DISTINCT PollId FROM Votes)
