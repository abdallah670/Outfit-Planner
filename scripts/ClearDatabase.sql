-- SQL Script to clear all data from OutfitPlanner database
-- This script deletes all data while preserving the table structure
-- Run this script before re-seeding the database

-- Disable foreign key constraints temporarily
EXEC sp_msforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT ALL"

-- Clear data from child tables first (tables with foreign keys)
-- Order matters: delete from dependent tables before parent tables

-- Delete from audit and activity logs
DELETE FROM UserActivities
DELETE FROM AuditLogs

-- Delete from feed-related tables
DELETE FROM PostReactions
DELETE FROM PostComments
DELETE FROM FeedPosts

-- Delete from outfit-related tables
DELETE FROM OutfitItems
DELETE FROM PollOptions
DELETE FROM Votes
DELETE FROM ValidationPolls
DELETE FROM Outfits
DELETE FROM WearEvents

-- Delete from clothing items
DELETE FROM ClothingTags
DELETE FROM ClothingItems

-- Delete from style rules
DELETE FROM StyleRules

-- Delete from user-related tables
DELETE FROM Follows
DELETE FROM UserStyleProfiles
DELETE FROM UserPreferences
DELETE FROM AppPreferences
DELETE FROM NotificationSettings
DELETE FROM Notifications

-- Delete from calendar and events
DELETE FROM CalendarEvents

-- Delete from system tables
DELETE FROM ContentReports
DELETE FROM SystemSettings

-- Delete from trending data
DELETE FROM TrendingOutfits

-- Finally delete users (except admin roles if you want to keep them)
-- Uncomment the line below if you want to delete all users including admins
-- DELETE FROM AspNetUsers

-- If you want to keep admin users, only delete non-admin users:
DELETE FROM AspNetUsers WHERE Role != 'Admin'

-- Re-enable foreign key constraints
EXEC sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL"

-- Reset identity columns to start from 1 (optional)
-- Uncomment these if you want to reset auto-increment IDs
/*
DBCC CHECKIDENT ('AspNetUsers', RESEED, 1)
DBCC CHECKIDENT ('UserActivities', RESEED, 1)
DBCC CHECKIDENT ('AuditLogs', RESEED, 1)
DBCC CHECKIDENT ('FeedPosts', RESEED, 1)
DBCC CHECKIDENT ('PostReactions', RESEED, 1)
DBCC CHECKIDENT ('PostComments', RESEED, 1)
DBCC CHECKIDENT ('Outfits', RESEED, 1)
DBCC CHECKIDENT ('OutfitItems', RESEED, 1)
DBCC CHECKIDENT ('PollOptions', RESEED, 1)
DBCC CHECKIDENT ('ValidationPolls', RESEED, 1)
DBCC CHECKIDENT ('Votes', RESEED, 1)
DBCC CHECKIDENT ('ClothingItems', RESEED, 1)
DBCC CHECKIDENT ('ClothingTags', RESEED, 1)
DBCC CHECKIDENT ('Follows', RESEED, 1)
DBCC CHECKIDENT ('UserStyleProfiles', RESEED, 1)
DBCC CHECKIDENT ('UserPreferences', RESEED, 1)
DBCC CHECKIDENT ('AppPreferences', RESEED, 1)
DBCC CHECKIDENT ('Notifications', RESEED, 1)
DBCC CHECKIDENT ('NotificationSettings', RESEED, 1)
DBCC CHECKIDENT ('CalendarEvents', RESEED, 1)
DBCC CHECKIDENT ('WearEvents', RESEED, 1)
DBCC CHECKIDENT ('StyleRules', RESEED, 1)
DBCC CHECKIDENT ('ContentReports', RESEED, 1)
DBCC CHECKIDENT ('SystemSettings', RESEED, 1)
DBCC CHECKIDENT ('TrendingOutfits', RESEED, 1)
*/

PRINT 'Database cleared successfully. Ready for re-seeding.'
