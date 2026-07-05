-- =====================================================
-- FixChatMetadata.sql
-- Backfill null Intent and Metadata on old ChatMessages
-- =====================================================

-- 1) Backfill Intent for assistant messages
UPDATE ChatMessages
SET Intent = 'assistant'
WHERE Role = 'assistant' AND Intent IS NULL;

-- 2) Backfill Intent for user messages that have a known classified intent
--    (We can't perfectly recreate the intent, but we set a generic value)
UPDATE ChatMessages
SET Intent = 'user_message'
WHERE Role = 'user' AND Intent IS NULL;

-- 3) Backfill Metadata for old assistant messages that contain outfit suggestions
--    The LLM response text contains item names like "Top 5, Bottom 8".
--    We reconstruct the metadata by looking up current ClothingItems by name.
--    Note: This only works if item names in the message content match actual item names.
DECLARE @cursor CURSOR;
DECLARE @msgId UNIQUEIDENTIFIER;
DECLARE @content NVARCHAR(MAX);
DECLARE @sessionId UNIQUEIDENTIFIER;

DECLARE @itemNames TABLE (name NVARCHAR(200));
DECLARE @suggestionsJson NVARCHAR(MAX);

SET @cursor = CURSOR FOR
SELECT Id, SessionId, Content
FROM ChatMessages
WHERE Role = 'assistant'
  AND Metadata IS NULL
  AND Content LIKE '%Top%'
  AND Content LIKE '%Bottom%';

OPEN @cursor;
FETCH NEXT FROM @cursor INTO @msgId, @sessionId, @content;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Clear temp table
    DELETE FROM @itemNames;

    -- Parse item names from content (simple extraction)
    -- Content pattern: "Top 5, Bottom 8, Footwear 5, Outerwear 4"
    INSERT INTO @itemNames (name)
    SELECT value
    FROM STRING_SPLIT(@content, ',')
    WHERE LTRIM(value) != '';

    -- Build metadata JSON from ClothingItems matching these names
    -- Group items by their type and build outfit suggestions
    SELECT @suggestionsJson = (
        SELECT 
            rank = 1,
            totalScore = 80.0,
            scoreBreakdown = JSON_OBJECT('Total': 80.0, 'Color Harmony': 32.0, 'Completeness': 24.0, 'Occasion Fit': 24.0),
            items = (
                SELECT 
                    Id = ci.Id,
                    Name = ci.Name,
                    Type = ci.Type,
                    ImageUrl = ci.ImageUrl,
                    HexColor = ISNULL(ci.PrimaryColor, '#636E72')
                FROM ClothingItems ci
                INNER JOIN @itemNames inm ON ci.Name LIKE '%' + inm.name + '%'
                WHERE ci.UserId = (SELECT TOP 1 UserId FROM ChatSessions WHERE Id = @sessionId)
                FOR JSON PATH
            )
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
    );

    -- Update metadata
    IF @suggestionsJson IS NOT NULL
    BEGIN
        UPDATE ChatMessages
        SET Metadata = @suggestionsJson
        WHERE Id = @msgId;
    END

    FETCH NEXT FROM @cursor INTO @msgId, @sessionId, @content;
END

CLOSE @cursor;
DEALLOCATE @cursor;

PRINT 'Chat metadata backfill complete.';