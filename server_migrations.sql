IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [AppPreferences] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [TemperatureUnit] nvarchar(max) NOT NULL,
        [Language] nvarchar(10) NOT NULL,
        [Theme] nvarchar(max) NOT NULL,
        [MeasurementUnit] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AppPreferences] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [ProfilePictureUrl] nvarchar(max) NULL,
        [Bio] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [LastLogin] datetimeoffset NULL,
        [Role] nvarchar(max) NOT NULL,
        [RefreshToken] nvarchar(max) NULL,
        [RefreshTokenExpiration] datetime2 NULL,
        [EmailVerificationToken] nvarchar(max) NULL,
        [EmailVerificationTokenExpiry] datetime2 NULL,
        [PasswordResetToken] nvarchar(max) NULL,
        [PasswordResetTokenExpiry] datetime2 NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [UserName] nvarchar(max) NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [EntityType] nvarchar(max) NOT NULL,
        [EntityId] nvarchar(max) NOT NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [IpAddress] nvarchar(max) NOT NULL,
        [Timestamp] datetimeoffset NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [ContentReports] (
        [Id] uniqueidentifier NOT NULL,
        [ReporterId] nvarchar(max) NOT NULL,
        [ReporterUserName] nvarchar(max) NULL,
        [TargetUserId] nvarchar(max) NOT NULL,
        [ContentType] nvarchar(max) NOT NULL,
        [ContentId] nvarchar(max) NOT NULL,
        [Reason] int NOT NULL,
        [Description] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [ResolvedById] nvarchar(max) NULL,
        [ResolvedAt] datetimeoffset NULL,
        [Resolution] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_ContentReports] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [ActionUrl] nvarchar(500) NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [NotificationSettings] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [DailyOutfitSuggestion] bit NOT NULL DEFAULT CAST(1 AS bit),
        [WeeklyStyleReport] bit NOT NULL DEFAULT CAST(0 AS bit),
        [WeatherAlerts] bit NOT NULL DEFAULT CAST(1 AS bit),
        [NewFeatures] bit NOT NULL DEFAULT CAST(1 AS bit),
        [SocialNotifications] bit NOT NULL DEFAULT CAST(1 AS bit),
        [PushNotificationsEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_NotificationSettings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [SystemSettings] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        [DataType] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Category] nvarchar(max) NOT NULL,
        [IsEditable] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [UserActivities] (
        [Id] nvarchar(450) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [UserName] nvarchar(max) NOT NULL,
        [Type] int NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [IpAddress] nvarchar(max) NOT NULL,
        [UserAgent] nvarchar(max) NULL,
        [AdditionalData] nvarchar(max) NULL,
        CONSTRAINT [PK_UserActivities] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [ClothingItems] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [Category] nvarchar(max) NOT NULL,
        [PrimaryColor] nvarchar(max) NOT NULL,
        [SecondaryColors] nvarchar(max) NOT NULL,
        [Fabric] nvarchar(max) NOT NULL,
        [Brand] nvarchar(max) NOT NULL,
        [PurchasePrice] decimal(18,2) NOT NULL,
        [PurchaseCurrency] nvarchar(3) NOT NULL,
        [PurchaseDate] datetime2 NULL,
        [Size] nvarchar(max) NOT NULL,
        [Condition] nvarchar(max) NOT NULL,
        [ImageUrl] nvarchar(max) NOT NULL,
        [ThumbnailUrl] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [LastWorn] datetimeoffset NULL,
        [WearCount] int NOT NULL,
        [LastWashed] datetimeoffset NULL,
        [MaintenanceNotes] nvarchar(max) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_ClothingItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClothingItems_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [Follows] (
        [Id] uniqueidentifier NOT NULL,
        [FollowerId] nvarchar(450) NOT NULL,
        [FollowedId] nvarchar(450) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Follows] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Follows_AspNetUsers_FollowerId] FOREIGN KEY ([FollowerId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Follows_AspNetUsers_FollowedId] FOREIGN KEY ([FollowedId]) REFERENCES [AspNetUsers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [Outfits] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Occasion] nvarchar(max) NOT NULL,
        [WeatherCondition] nvarchar(max) NOT NULL,
        [Season] int NOT NULL,
        [ComfortRating] int NULL,
        [LastWorn] datetimeoffset NULL,
        [TimesWorn] int NOT NULL,
        [ImageUrl] nvarchar(max) NULL,
        [LikesCount] int NOT NULL,
        [CommentsCount] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Outfits] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Outfits_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [UserPreferences] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ShareOutfitsAnonymously] bit NOT NULL,
        [IncludeInTrendAnalysis] bit NOT NULL,
        [AllowFriendRequests] bit NOT NULL,
        [DefaultOutfitPrivacy] nvarchar(max) NOT NULL,
        [ShowBodyMetrics] bit NOT NULL,
        [AllowLocationTracking] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_UserPreferences] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserPreferences_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [UserStyleProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Style] nvarchar(max) NOT NULL,
        [PreferredColors] nvarchar(max) NOT NULL,
        [FitPreferences] nvarchar(500) NOT NULL,
        [ComfortPriority] int NOT NULL,
        [AcceptsTrends] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_UserStyleProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserStyleProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [ValidationPolls] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Question] nvarchar(500) NOT NULL,
        [Context] nvarchar(max) NOT NULL,
        [ExpiresAt] datetimeoffset NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [TotalVotes] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_ValidationPolls] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ValidationPolls_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [ClothingTags] (
        [Id] uniqueidentifier NOT NULL,
        [ClothingItemId] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Source] nvarchar(50) NOT NULL,
        [Confidence] decimal(5,4) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_ClothingTags] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClothingTags_ClothingItems_ClothingItemId] FOREIGN KEY ([ClothingItemId]) REFERENCES [ClothingItems] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [OutfitItems] (
        [Id] uniqueidentifier NOT NULL,
        [OutfitId] uniqueidentifier NOT NULL,
        [ClothingItemId] uniqueidentifier NOT NULL,
        [Role] nvarchar(max) NOT NULL,
        [LayeringOrder] int NOT NULL,
        [IsEssential] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_OutfitItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OutfitItems_ClothingItems_ClothingItemId] FOREIGN KEY ([ClothingItemId]) REFERENCES [ClothingItems] ([Id]),
        CONSTRAINT [FK_OutfitItems_Outfits_OutfitId] FOREIGN KEY ([OutfitId]) REFERENCES [Outfits] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [WearEvents] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ClothingItemId] uniqueidentifier NULL,
        [OutfitId] uniqueidentifier NULL,
        [EventId] uniqueidentifier NULL,
        [WornAt] datetimeoffset NOT NULL,
        [DurationMinutes] int NOT NULL,
        [WeatherCondition] nvarchar(100) NOT NULL,
        [Rating] int NOT NULL,
        [Notes] nvarchar(1000) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_WearEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WearEvents_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_WearEvents_ClothingItems_ClothingItemId] FOREIGN KEY ([ClothingItemId]) REFERENCES [ClothingItems] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_WearEvents_Outfits_OutfitId] FOREIGN KEY ([OutfitId]) REFERENCES [Outfits] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [StyleRules] (
        [Id] uniqueidentifier NOT NULL,
        [UserStyleProfileId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [IsActive] bit NOT NULL,
        [CriteriaJson] nvarchar(max) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_StyleRules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StyleRules_UserStyleProfiles_UserStyleProfileId] FOREIGN KEY ([UserStyleProfileId]) REFERENCES [UserStyleProfiles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [FeedPosts] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [PostType] int NOT NULL,
        [OutfitId] uniqueidentifier NULL,
        [PollId] uniqueidentifier NULL,
        [Caption] nvarchar(500) NULL,
        [Tags] nvarchar(max) NOT NULL,
        [Visibility] int NOT NULL,
        [LikesCount] int NOT NULL,
        [CommentsCount] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_FeedPosts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FeedPosts_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_FeedPosts_Outfits_OutfitId] FOREIGN KEY ([OutfitId]) REFERENCES [Outfits] ([Id]),
        CONSTRAINT [FK_FeedPosts_ValidationPolls_PollId] FOREIGN KEY ([PollId]) REFERENCES [ValidationPolls] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [PollOptions] (
        [Id] uniqueidentifier NOT NULL,
        [PollId] uniqueidentifier NOT NULL,
        [OutfitId] uniqueidentifier NULL,
        [Description] nvarchar(200) NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_PollOptions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PollOptions_Outfits_OutfitId] FOREIGN KEY ([OutfitId]) REFERENCES [Outfits] ([Id]),
        CONSTRAINT [FK_PollOptions_ValidationPolls_PollId] FOREIGN KEY ([PollId]) REFERENCES [ValidationPolls] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [TrendingOutfits] (
        [Id] uniqueidentifier NOT NULL,
        [OutfitId] uniqueidentifier NOT NULL,
        [PollId] uniqueidentifier NULL,
        [VoteCount] int NOT NULL,
        [LikesCount] int NOT NULL DEFAULT 0,
        [CommentsCount] int NOT NULL DEFAULT 0,
        [TrendingScore] decimal(10,2) NOT NULL,
        [RankPosition] int NOT NULL,
        [Date] datetime2 NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_TrendingOutfits] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TrendingOutfits_Outfits_OutfitId] FOREIGN KEY ([OutfitId]) REFERENCES [Outfits] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TrendingOutfits_ValidationPolls_PollId] FOREIGN KEY ([PollId]) REFERENCES [ValidationPolls] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [CalendarEvents] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Location] nvarchar(200) NULL,
        [EventDate] datetimeoffset NOT NULL,
        [StartTime] time NULL,
        [EndTime] time NULL,
        [EventType] nvarchar(50) NOT NULL,
        [WearEventId] uniqueidentifier NULL,
        [Notes] nvarchar(1000) NULL,
        [IsRecurring] bit NOT NULL DEFAULT CAST(0 AS bit),
        [RecurrencePattern] nvarchar(500) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_CalendarEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CalendarEvents_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CalendarEvents_WearEvents_WearEventId] FOREIGN KEY ([WearEventId]) REFERENCES [WearEvents] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [PostComments] (
        [Id] uniqueidentifier NOT NULL,
        [PostId] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ParentCommentId] uniqueidentifier NULL,
        [Content] nvarchar(1000) NOT NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_PostComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PostComments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_PostComments_FeedPosts_PostId] FOREIGN KEY ([PostId]) REFERENCES [FeedPosts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PostComments_PostComments_ParentCommentId] FOREIGN KEY ([ParentCommentId]) REFERENCES [PostComments] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [PostReactions] (
        [Id] uniqueidentifier NOT NULL,
        [PostId] uniqueidentifier NOT NULL,
        [ReactionType] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_PostReactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PostReactions_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_PostReactions_FeedPosts_PostId] FOREIGN KEY ([PostId]) REFERENCES [FeedPosts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE TABLE [Votes] (
        [Id] uniqueidentifier NOT NULL,
        [PollId] uniqueidentifier NOT NULL,
        [OptionId] uniqueidentifier NOT NULL,
        [VoterId] nvarchar(450) NOT NULL,
        [Rating] int NOT NULL,
        [IsAnonymous] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Votes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Votes_AspNetUsers_VoterId] FOREIGN KEY ([VoterId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Votes_PollOptions_OptionId] FOREIGN KEY ([OptionId]) REFERENCES [PollOptions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Votes_ValidationPolls_PollId] FOREIGN KEY ([PollId]) REFERENCES [ValidationPolls] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] ON;
    EXEC(N'INSERT INTO [AspNetRoles] ([Id], [ConcurrencyStamp], [Name], [NormalizedName])
    VALUES (N''5765715a-93be-4628-86f7-b12e35a1a1f1'', N''ece01a6a-4caf-4a95-a704-9f03712e7fbb'', N''Admin'', N''ADMIN''),
    (N''76208571-0083-4a8b-9149-8d769c0d9c02'', N''bd9a512a-b188-467d-9fd9-875f09673ac3'', N''Planner'', N''PLANNER'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AppPreferences_UserId] ON [AppPreferences] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_CalendarEvents_EventDate] ON [CalendarEvents] ([EventDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_CalendarEvents_UserId] ON [CalendarEvents] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_CalendarEvents_UserId_EventDate] ON [CalendarEvents] ([UserId], [EventDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_CalendarEvents_WearEventId] ON [CalendarEvents] ([WearEventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_ClothingItems_UserId] ON [ClothingItems] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_ClothingTags_ClothingItemId] ON [ClothingTags] ([ClothingItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_FeedPosts_CommentsCount] ON [FeedPosts] ([CommentsCount]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_FeedPosts_CreatedAt] ON [FeedPosts] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_FeedPosts_LikesCount] ON [FeedPosts] ([LikesCount]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_FeedPosts_OutfitId] ON [FeedPosts] ([OutfitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_FeedPosts_PollId] ON [FeedPosts] ([PollId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_FeedPosts_UserId] ON [FeedPosts] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Follows_FollowerId] ON [Follows] ([FollowerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Follows_FollowerId_FollowedId] ON [Follows] ([FollowerId], [FollowedId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Follows_FollowedId] ON [Follows] ([FollowedId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Notifications_CreatedAt] ON [Notifications] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Notifications_IsRead] ON [Notifications] ([IsRead]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NotificationSettings_UserId] ON [NotificationSettings] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_OutfitItems_ClothingItemId] ON [OutfitItems] ([ClothingItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_OutfitItems_OutfitId] ON [OutfitItems] ([OutfitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Outfits_UserId] ON [Outfits] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PollOptions_OutfitId] ON [PollOptions] ([OutfitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PollOptions_PollId] ON [PollOptions] ([PollId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PostComments_ParentCommentId] ON [PostComments] ([ParentCommentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PostComments_PostId] ON [PostComments] ([PostId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PostComments_UserId] ON [PostComments] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PostReactions_PostId_UserId] ON [PostReactions] ([PostId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PostReactions_UserId] ON [PostReactions] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_StyleRules_UserStyleProfileId] ON [StyleRules] ([UserStyleProfileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_TrendingOutfits_Date_RankPosition] ON [TrendingOutfits] ([Date], [RankPosition]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_TrendingOutfits_Date_TrendingScore] ON [TrendingOutfits] ([Date], [TrendingScore]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TrendingOutfits_OutfitId_Date] ON [TrendingOutfits] ([OutfitId], [Date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_TrendingOutfits_PollId] ON [TrendingOutfits] ([PollId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserPreferences_UserId] ON [UserPreferences] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserStyleProfiles_UserId] ON [UserStyleProfiles] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_ValidationPolls_UserId] ON [ValidationPolls] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Votes_OptionId_VoterId] ON [Votes] ([OptionId], [VoterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Votes_PollId] ON [Votes] ([PollId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Votes_VoterId] ON [Votes] ([VoterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_WearEvents_ClothingItemId] ON [WearEvents] ([ClothingItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_WearEvents_OutfitId] ON [WearEvents] ([OutfitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_WearEvents_UserId] ON [WearEvents] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509222553_InitialMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260509222553_InitialMigration', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512204234_delete description and rating from poll'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Votes]') AND [c].[name] = N'IsAnonymous');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Votes] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Votes] DROP COLUMN [IsAnonymous];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512204234_delete description and rating from poll'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Votes]') AND [c].[name] = N'Rating');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Votes] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [Votes] DROP COLUMN [Rating];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512204234_delete description and rating from poll'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PollOptions]') AND [c].[name] = N'Description');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [PollOptions] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [PollOptions] DROP COLUMN [Description];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512204234_delete description and rating from poll'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260512204234_delete description and rating from poll', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514123228_addingdescriptiontoptionpoll'
)
BEGIN
    ALTER TABLE [PollOptions] ADD [Description] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514123228_addingdescriptiontoptionpoll'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260514123228_addingdescriptiontoptionpoll', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516170851_addding total replies to comments'
)
BEGIN
    ALTER TABLE [PostComments] ADD [TotalReplies] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516170851_addding total replies to comments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260516170851_addding total replies to comments', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516172301_Delete unused filelds'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TrendingOutfits]') AND [c].[name] = N'VoteCount');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [TrendingOutfits] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [TrendingOutfits] DROP COLUMN [VoteCount];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516172301_Delete unused filelds'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260516172301_Delete unused filelds', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520030527_UpdateModelChanges'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'Role');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT ' + @var4 + ';');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [Role] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520030527_UpdateModelChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260520030527_UpdateModelChanges', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [WearEvents] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [WearEvents] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [WearEvents] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [Votes] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [Votes] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [Votes] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [ValidationPolls] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [ValidationPolls] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [ValidationPolls] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [UserStyleProfiles] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [UserStyleProfiles] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [UserStyleProfiles] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [UserPreferences] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [UserPreferences] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [UserPreferences] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [TrendingOutfits] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [TrendingOutfits] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [TrendingOutfits] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [SystemSettings] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [SystemSettings] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [SystemSettings] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [StyleRules] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [StyleRules] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [StyleRules] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [PostReactions] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [PostReactions] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [PostReactions] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [PostComments] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [PostComments] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [PollOptions] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [PollOptions] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [PollOptions] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [Outfits] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [Outfits] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [Outfits] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [OutfitItems] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [OutfitItems] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [OutfitItems] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [NotificationSettings] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [NotificationSettings] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [Notifications] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [Notifications] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [Notifications] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [Follows] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [Follows] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [Follows] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [FeedPosts] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [FeedPosts] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [ContentReports] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [ContentReports] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [ContentReports] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [ClothingTags] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [ClothingTags] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [ClothingTags] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [ClothingItems] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [ClothingItems] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [ClothingItems] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [CalendarEvents] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [CalendarEvents] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [CalendarEvents] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [UpdatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [AppPreferences] ADD [DeletedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    ALTER TABLE [AppPreferences] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    CREATE TABLE [ChatSessions] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Status] nvarchar(20) NOT NULL DEFAULT N'Active',
        [MessageCount] int NOT NULL DEFAULT 0,
        [LastActivityAt] datetimeoffset NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetimeoffset NULL,
        CONSTRAINT [PK_ChatSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatSessions_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    CREATE TABLE [ChatMessages] (
        [Id] uniqueidentifier NOT NULL,
        [SessionId] uniqueidentifier NOT NULL,
        [SenderId] nvarchar(450) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [Intent] nvarchar(50) NULL,
        [Metadata] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetimeoffset NULL,
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatMessages_ChatSessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [ChatSessions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    CREATE INDEX [IX_ChatMessages_CreatedAt] ON [ChatMessages] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    CREATE INDEX [IX_ChatMessages_SessionId] ON [ChatMessages] ([SessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    CREATE INDEX [IX_ChatSessions_LastActivityAt] ON [ChatSessions] ([LastActivityAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    CREATE INDEX [IX_ChatSessions_UserId] ON [ChatSessions] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260523085359_AddChatSessionAndMessageEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260523085359_AddChatSessionAndMessageEntities', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602155901_PendingModelChanges'
)
BEGIN
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[NotificationSettings]') AND [c].[name] = N'UpdatedAt');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [NotificationSettings] DROP CONSTRAINT ' + @var5 + ';');
    ALTER TABLE [NotificationSettings] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602155901_PendingModelChanges'
)
BEGIN
    DECLARE @var6 nvarchar(max);
    SELECT @var6 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[NotificationSettings]') AND [c].[name] = N'CreatedAt');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [NotificationSettings] DROP CONSTRAINT ' + @var6 + ';');
    ALTER TABLE [NotificationSettings] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602155901_PendingModelChanges'
)
BEGIN
    DROP INDEX [IX_Notifications_CreatedAt] ON [Notifications];
    DECLARE @var7 nvarchar(max);
    SELECT @var7 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Notifications]') AND [c].[name] = N'CreatedAt');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Notifications] DROP CONSTRAINT ' + @var7 + ';');
    ALTER TABLE [Notifications] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_Notifications_CreatedAt] ON [Notifications] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602155901_PendingModelChanges'
)
BEGIN
    DECLARE @var8 nvarchar(max);
    SELECT @var8 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FeedPosts]') AND [c].[name] = N'UpdatedAt');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [FeedPosts] DROP CONSTRAINT ' + @var8 + ';');
    EXEC(N'UPDATE [FeedPosts] SET [UpdatedAt] = ''0001-01-01T00:00:00.0000000+00:00'' WHERE [UpdatedAt] IS NULL');
    ALTER TABLE [FeedPosts] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
    ALTER TABLE [FeedPosts] ADD DEFAULT '0001-01-01T00:00:00.0000000+00:00' FOR [UpdatedAt];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602155901_PendingModelChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260602155901_PendingModelChanges', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622151714_RefactorTrendingOutfitsToFeedPost'
)
BEGIN
    DELETE FROM TrendingOutfits;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622151714_RefactorTrendingOutfitsToFeedPost'
)
BEGIN
    ALTER TABLE [TrendingOutfits] DROP CONSTRAINT [FK_TrendingOutfits_Outfits_OutfitId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622151714_RefactorTrendingOutfitsToFeedPost'
)
BEGIN
    ALTER TABLE [TrendingOutfits] DROP CONSTRAINT [FK_TrendingOutfits_ValidationPolls_PollId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622151714_RefactorTrendingOutfitsToFeedPost'
)
BEGIN
    DROP INDEX [IX_TrendingOutfits_PollId] ON [TrendingOutfits];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622151714_RefactorTrendingOutfitsToFeedPost'
)
BEGIN
    DECLARE @var9 nvarchar(max);
    SELECT @var9 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TrendingOutfits]') AND [c].[name] = N'PollId');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [TrendingOutfits] DROP CONSTRAINT ' + @var9 + ';');
    ALTER TABLE [TrendingOutfits] DROP COLUMN [PollId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622151714_RefactorTrendingOutfitsToFeedPost'
)
BEGIN
    EXEC sp_rename N'[TrendingOutfits].[OutfitId]', N'FeedPostId', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622151714_RefactorTrendingOutfitsToFeedPost'
)
BEGIN
    EXEC sp_rename N'[TrendingOutfits].[IX_TrendingOutfits_OutfitId_Date]', N'IX_TrendingOutfits_FeedPostId_Date', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622151714_RefactorTrendingOutfitsToFeedPost'
)
BEGIN
    ALTER TABLE [TrendingOutfits] ADD [PostType] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622151714_RefactorTrendingOutfitsToFeedPost'
)
BEGIN
    ALTER TABLE [TrendingOutfits] ADD CONSTRAINT [FK_TrendingOutfits_FeedPosts_FeedPostId] FOREIGN KEY ([FeedPostId]) REFERENCES [FeedPosts] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622151714_RefactorTrendingOutfitsToFeedPost'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260622151714_RefactorTrendingOutfitsToFeedPost', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701213444_add images to Chat message'
)
BEGIN
    ALTER TABLE [ChatMessages] ADD [Images] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701213444_add images to Chat message'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260701213444_add images to Chat message', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215455_AddSentReminderEntity'
)
BEGIN
    CREATE TABLE [SentReminders] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [CalendarEventId] uniqueidentifier NULL,
        [ReminderType] nvarchar(50) NOT NULL,
        [SentAt] datetimeoffset NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetimeoffset NULL,
        CONSTRAINT [PK_SentReminders] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215455_AddSentReminderEntity'
)
BEGIN
    CREATE INDEX [IX_SentReminders_UserId] ON [SentReminders] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215455_AddSentReminderEntity'
)
BEGIN
    CREATE INDEX [IX_SentReminders_UserId_CalendarEventId_ReminderType_SentAt] ON [SentReminders] ([UserId], [CalendarEventId], [ReminderType], [SentAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215455_AddSentReminderEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260704215455_AddSentReminderEntity', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704222214_MakeComfortRatingNonNullableWithDefault'
)
BEGIN
    DECLARE @var10 nvarchar(max);
    SELECT @var10 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Outfits]') AND [c].[name] = N'ComfortRating');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Outfits] DROP CONSTRAINT ' + @var10 + ';');
    EXEC(N'UPDATE [Outfits] SET [ComfortRating] = 5 WHERE [ComfortRating] IS NULL');
    ALTER TABLE [Outfits] ALTER COLUMN [ComfortRating] int NOT NULL;
    ALTER TABLE [Outfits] ADD DEFAULT 5 FOR [ComfortRating];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704222214_MakeComfortRatingNonNullableWithDefault'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260704222214_MakeComfortRatingNonNullableWithDefault', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714002354_remove thumbnail for clothing item'
)
BEGIN
    DECLARE @var11 nvarchar(max);
    SELECT @var11 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClothingItems]') AND [c].[name] = N'ThumbnailUrl');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [ClothingItems] DROP CONSTRAINT ' + @var11 + ';');
    ALTER TABLE [ClothingItems] DROP COLUMN [ThumbnailUrl];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714002354_remove thumbnail for clothing item'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260714002354_remove thumbnail for clothing item', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714235150_AddCommentMentionedUsers'
)
BEGIN
    ALTER TABLE [PostComments] ADD [MentionedUsers] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714235150_AddCommentMentionedUsers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260714235150_AddCommentMentionedUsers', N'10.0.5');
END;

COMMIT;
GO

