import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AdminRepository, ADMIN_REPOSITORY } from '../../domain/repositories/admin.repository';
import { AdminDataSource } from '../datasources/admin.datasource';
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
} from '../../domain/entities/admin.entity';

@Injectable({
  providedIn: 'root',
})
export class AdminRepositoryImpl implements AdminRepository {
  private readonly dataSource = inject(AdminDataSource);

  getUsers(filter: UserFilterRequest): Observable<PaginatedResult<AdminUser>> {
    return this.dataSource.getUsers(filter);
  }

  getUserDetail(userId: string): Observable<AdminUserDetail> {
    return this.dataSource.getUserDetail(userId);
  }

  banUser(userId: string, request: BanUserRequest): Observable<void> {
    return this.dataSource.banUser(userId, request);
  }

  unbanUser(userId: string): Observable<void> {
    return this.dataSource.unbanUser(userId);
  }

  getReports(filter: ReportFilterRequest): Observable<PaginatedResult<ContentReport>> {
    return this.dataSource.getReports(filter);
  }

  getReportDetail(reportId: string): Observable<ContentReportDetail> {
    return this.dataSource.getReportDetail(reportId);
  }

  resolveReport(reportId: string, request: ResolveReportRequest): Observable<void> {
    return this.dataSource.resolveReport(reportId, request);
  }

  getSettings(): Observable<SystemSetting[]> {
    return this.dataSource.getSettings();
  }

  updateSetting(key: string, request: UpdateSettingRequest): Observable<void> {
    return this.dataSource.updateSetting(key, request);
  }

  getDashboardAnalytics(startDate?: Date, endDate?: Date): Observable<AnalyticsDashboard> {
    return this.dataSource.getDashboardAnalytics(startDate, endDate);
  }

  getAuditLogs(filter: any): Observable<PaginatedResult<AuditLogEntry>> {
    return this.dataSource.getAuditLogs(filter);
  }

  getLockedAccounts(): Observable<any[]> {
    return this.dataSource.getLockedAccounts();
  }

  unlockAccount(userId: string): Observable<void> {
    return this.dataSource.unlockAccount(userId);
  }

  // Content Management - Posts
  getPosts(filter: ContentFilterRequest): Observable<PaginatedResult<AdminPostDto>> {
    return this.dataSource.getPosts(filter);
  }

  approvePost(postId: string): Observable<void> {
    return this.dataSource.approvePost(postId);
  }

  rejectPost(postId: string, request: any): Observable<void> {
    return this.dataSource.rejectPost(postId, request);
  }

  deletePost(postId: string): Observable<void> {
    return this.dataSource.deletePost(postId);
  }

  bulkPostOperations(request: BulkPostOperationRequest): Observable<any> {
    return this.dataSource.bulkPostOperations(request);
  }

  // Content Management - Polls
  getPolls(filter: ContentFilterRequest): Observable<PaginatedResult<AdminPollDto>> {
    return this.dataSource.getPolls(filter);
  }

  closePoll(pollId: string): Observable<void> {
    return this.dataSource.closePoll(pollId);
  }

  featurePoll(pollId: string): Observable<void> {
    return this.dataSource.featurePoll(pollId);
  }

  unfeaturePoll(pollId: string): Observable<void> {
    return this.dataSource.unfeaturePoll(pollId);
  }

  deletePoll(pollId: string): Observable<void> {
    return this.dataSource.deletePoll(pollId);
  }

  bulkPollOperations(request: BulkPollOperationRequest): Observable<any> {
    return this.dataSource.bulkPollOperations(request);
  }

  // Content Management - Outfits
  getOutfits(filter: ContentFilterRequest): Observable<PaginatedResult<AdminOutfitDto>> {
    return this.dataSource.getOutfits(filter);
  }

  featureOutfit(outfitId: string): Observable<void> {
    return this.dataSource.featureOutfit(outfitId);
  }

  unfeatureOutfit(outfitId: string): Observable<void> {
    return this.dataSource.unfeatureOutfit(outfitId);
  }

  approveOutfit(outfitId: string): Observable<void> {
    return this.dataSource.approveOutfit(outfitId);
  }

  rejectOutfit(outfitId: string, request: any): Observable<void> {
    return this.dataSource.rejectOutfit(outfitId, request);
  }

  deleteOutfit(outfitId: string): Observable<void> {
    return this.dataSource.deleteOutfit(outfitId);
  }

  bulkOutfitOperations(request: BulkOutfitOperationRequest): Observable<any> {
    return this.dataSource.bulkOutfitOperations(request);
  }

  // Analytics Management
  getDetailedAnalytics(filter: AnalyticsFilterRequest): Observable<DetailedAnalyticsDto> {
    return this.dataSource.getDetailedAnalytics(filter);
  }

  getRealtimeAnalytics(): Observable<RealtimeAnalyticsDto> {
    return this.dataSource.getRealtimeAnalytics();
  }

  exportAnalytics(request: ExportAnalyticsRequest): Observable<Blob> {
    return this.dataSource.exportAnalytics(request);
  }

  // System Management
  getSystemHealth(): Observable<SystemHealthDto> {
    return this.dataSource.getSystemHealth();
  }

  getSystemLogs(filter: SystemLogFilterRequest): Observable<PaginatedResult<SystemLogDto>> {
    return this.dataSource.getSystemLogs(filter);
  }

  getSystemPerformance(): Observable<SystemPerformanceDto> {
    return this.dataSource.getSystemPerformance();
  }

  setMaintenanceMode(request: SetMaintenanceModeRequest): Observable<any> {
    return this.dataSource.setMaintenanceMode(request);
  }

  createBackup(request: BackupRequest): Observable<BackupResult> {
    return this.dataSource.createBackup(request);
  }

  restartService(serviceName: string): Observable<any> {
    return this.dataSource.restartService(serviceName);
  }

  clearCache(request: ClearCacheRequest): Observable<any> {
    return this.dataSource.clearCache(request);
  }
}

export const adminRepositoryProvider = {
  provide: ADMIN_REPOSITORY,
  useClass: AdminRepositoryImpl,
};