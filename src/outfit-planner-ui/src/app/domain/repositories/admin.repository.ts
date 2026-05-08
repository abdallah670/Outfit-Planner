import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AdminUser,
  AdminUserDetail,
  ContentReport,
  ContentReportDetail,
  SystemSetting,
  AnalyticsDashboard,
  AuditLogEntry,
  PaginatedResult,
  UserFilterRequest,
  ReportFilterRequest,
  BanUserRequest,
  ResolveReportRequest,
  UpdateSettingRequest,
  // Content Management DTOs
  AdminPostDto,
  AdminPollDto,
  AdminOutfitDto,
  ContentFilterRequest,
  BulkPostOperationRequest,
  BulkPollOperationRequest,
  BulkOutfitOperationRequest,
  // Analytics DTOs
  DetailedAnalyticsDto,
  RealtimeAnalyticsDto,
  AnalyticsFilterRequest,
  ExportAnalyticsRequest,
  // System Management DTOs
  SystemHealthDto,
  SystemLogDto,
  SystemPerformanceDto,
  SystemLogFilterRequest,
  SetMaintenanceModeRequest,
  BackupRequest,
  BackupResult,
  ClearCacheRequest,
} from '../entities/admin.entity';

export const ADMIN_REPOSITORY = new InjectionToken<AdminRepository>('AdminRepository');

export interface AdminRepository {
  // User Management
  getUsers(filter: UserFilterRequest): Observable<PaginatedResult<AdminUser>>;
  getUserDetail(userId: string): Observable<AdminUserDetail>;
  banUser(userId: string, request: BanUserRequest): Observable<void>;
  unbanUser(userId: string): Observable<void>;

  // Reports
  getReports(filter: ReportFilterRequest): Observable<PaginatedResult<ContentReport>>;
  getReportDetail(reportId: string): Observable<ContentReportDetail>;
  resolveReport(reportId: string, request: ResolveReportRequest): Observable<void>;

  // Settings
  getSettings(): Observable<SystemSetting[]>;
  updateSetting(key: string, request: UpdateSettingRequest): Observable<void>;

  // Analytics
  getDashboardAnalytics(startDate?: Date, endDate?: Date): Observable<AnalyticsDashboard>;

  // Audit Logs
  getAuditLogs(filter: any): Observable<PaginatedResult<AuditLogEntry>>;

  // Account Management
  getLockedAccounts(): Observable<any[]>;
  unlockAccount(userId: string): Observable<void>;

  // Content Management - Posts
  getPosts(filter: ContentFilterRequest): Observable<PaginatedResult<AdminPostDto>>;
  approvePost(postId: string): Observable<void>;
  rejectPost(postId: string, request: any): Observable<void>;
  deletePost(postId: string): Observable<void>;
  bulkPostOperations(request: BulkPostOperationRequest): Observable<any>;

  // Content Management - Polls
  getPolls(filter: ContentFilterRequest): Observable<PaginatedResult<AdminPollDto>>;
  closePoll(pollId: string): Observable<void>;
  featurePoll(pollId: string): Observable<void>;
  unfeaturePoll(pollId: string): Observable<void>;
  deletePoll(pollId: string): Observable<void>;
  bulkPollOperations(request: BulkPollOperationRequest): Observable<any>;

  // Content Management - Outfits
  getOutfits(filter: ContentFilterRequest): Observable<PaginatedResult<AdminOutfitDto>>;
  featureOutfit(outfitId: string): Observable<void>;
  unfeatureOutfit(outfitId: string): Observable<void>;
  approveOutfit(outfitId: string): Observable<void>;
  rejectOutfit(outfitId: string, request: any): Observable<void>;
  deleteOutfit(outfitId: string): Observable<void>;
  bulkOutfitOperations(request: BulkOutfitOperationRequest): Observable<any>;

  // Analytics Management
  getDetailedAnalytics(filter: AnalyticsFilterRequest): Observable<DetailedAnalyticsDto>;
  getRealtimeAnalytics(): Observable<RealtimeAnalyticsDto>;
  exportAnalytics(request: ExportAnalyticsRequest): Observable<Blob>;

  // System Management
  getSystemHealth(): Observable<SystemHealthDto>;
  getSystemLogs(filter: SystemLogFilterRequest): Observable<PaginatedResult<SystemLogDto>>;
  getSystemPerformance(): Observable<SystemPerformanceDto>;
  setMaintenanceMode(request: SetMaintenanceModeRequest): Observable<any>;
  createBackup(request: BackupRequest): Observable<BackupResult>;
  restartService(serviceName: string): Observable<any>;
  clearCache(request: ClearCacheRequest): Observable<any>;
}