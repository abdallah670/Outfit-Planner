BEGIN TRANSACTION;
ALTER TABLE [TrendingOutfits] DROP CONSTRAINT [FK_TrendingOutfits_Outfits_OutfitId];

ALTER TABLE [TrendingOutfits] DROP CONSTRAINT [FK_TrendingOutfits_ValidationPolls_PollId];

DROP INDEX [IX_TrendingOutfits_PollId] ON [TrendingOutfits];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TrendingOutfits]') AND [c].[name] = N'PollId');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [TrendingOutfits] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [TrendingOutfits] DROP COLUMN [PollId];

EXEC sp_rename N'[TrendingOutfits].[OutfitId]', N'FeedPostId', 'COLUMN';

EXEC sp_rename N'[TrendingOutfits].[IX_TrendingOutfits_OutfitId_Date]', N'IX_TrendingOutfits_FeedPostId_Date', 'INDEX';

ALTER TABLE [TrendingOutfits] ADD [PostType] int NOT NULL DEFAULT 0;

ALTER TABLE [TrendingOutfits] ADD CONSTRAINT [FK_TrendingOutfits_FeedPosts_FeedPostId] FOREIGN KEY ([FeedPostId]) REFERENCES [FeedPosts] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260622151714_RefactorTrendingOutfitsToFeedPost', N'10.0.5');

COMMIT;
GO

