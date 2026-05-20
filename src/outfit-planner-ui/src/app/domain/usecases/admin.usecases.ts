import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AdminRepository,
  ADMIN_REPOSITORY,
} from '../repositories/admin.repository';
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

@Injectable({
  providedIn: 'root',
})
export class AdminUseCases {
  constructor(
    @Inject(ADMIN_REPOSITORY) private readonly adminRepository: AdminRepository
  ) {}

  getUsers(filter: UserFilterRequest): Observable<PaginatedResult<AdminUser>> {
    return this.adminRepository.getUsers(filter);
  }

  getUserDetail(userId: string): Observable<AdminUserDetail> {
    return this.adminRepository.getUserDetail(userId);
  }

  banUser(userId: string, request: BanUserRequest): Observable<void> {
    return this.adminRepository.banUser(userId, request);
  }

  unbanUser(userId: string): Observable<void> {
    return this.adminRepository.unbanUser(userId);
  }

  getReports(filter: ReportFilterRequest): Observable<PaginatedResult<ContentReport>> {
    return this.adminRepository.getReports(filter);
  }

  getReportDetail(reportId: string): Observable<ContentReportDetail> {
    return this.adminRepository.getReportDetail(reportId);
  }

  resolveReport(reportId: string, request: ResolveReportRequest): Observable<void> {
    return this.adminRepository.resolveReport(reportId, request);
  }

  getSettings(): Observable<SystemSetting[]> {
    return this.adminRepository.getSettings();
  }

  updateSetting(key: string, request: UpdateSettingRequest): Observable<void> {
    return this.adminRepository.updateSetting(key, request);
  }

  getDashboardAnalytics(startDate?: Date, endDate?: Date): Observable<AnalyticsDashboard> {
    return this.adminRepository.getDashboardAnalytics(startDate, endDate);
  }

  getAuditLogs(filter: any): Observable<PaginatedResult<AuditLogEntry>> {
    return this.adminRepository.getAuditLogs(filter);
  }

  getLockedAccounts(): Observable<any[]> {
    return this.adminRepository.getLockedAccounts();
  }

  unlockAccount(userId: string): Observable<void> {
    return this.adminRepository.unlockAccount(userId);
  }

  // Content Management - Posts
  getPosts(filter: ContentFilterRequest): Observable<PaginatedResult<AdminPostDto>> {
    return this.adminRepository.getPosts(filter);
  }

  approvePost(postId: string): Observable<void> {
    return this.adminRepository.approvePost(postId);
  }

  rejectPost(postId: string, request: any): Observable<void> {
    return this.adminRepository.rejectPost(postId, request);
  }

  deletePost(postId: string): Observable<void> {
    return this.adminRepository.deletePost(postId);
  }

  bulkPostOperations(request: BulkPostOperationRequest): Observable<any> {
    return this.adminRepository.bulkPostOperations(request);
  }

  // Content Management - Polls
  getPolls(filter: ContentFilterRequest): Observable<PaginatedResult<AdminPollDto>> {
    return this.adminRepository.getPolls(filter);
  }

  closePoll(pollId: string): Observable<void> {
    return this.adminRepository.closePoll(pollId);
  }

  featurePoll(pollId: string): Observable<void> {
    return this.adminRepository.featurePoll(pollId);
  }

  unfeaturePoll(pollId: string): Observable<void> {
    return this.adminRepository.unfeaturePoll(pollId);
  }

  deletePoll(pollId: string): Observable<void> {
    return this.adminRepository.deletePoll(pollId);
  }

  bulkPollOperations(request: BulkPollOperationRequest): Observable<any> {
    return this.adminRepository.bulkPollOperations(request);
  }

  // Content Management - Outfits
  getOutfits(filter: ContentFilterRequest): Observable<PaginatedResult<AdminOutfitDto>> {
    return this.adminRepository.getOutfits(filter);
  }

  featureOutfit(outfitId: string): Observable<void> {
    return this.adminRepository.featureOutfit(outfitId);
  }

  unfeatureOutfit(outfitId: string): Observable<void> {
    return this.adminRepository.unfeatureOutfit(outfitId);
  }

  approveOutfit(outfitId: string): Observable<void> {
    return this.adminRepository.approveOutfit(outfitId);
  }

  rejectOutfit(outfitId: string, request: any): Observable<void> {
    return this.adminRepository.rejectOutfit(outfitId, request);
  }

  deleteOutfit(outfitId: string): Observable<void> {
    return this.adminRepository.deleteOutfit(outfitId);
  }

  bulkOutfitOperations(request: BulkOutfitOperationRequest): Observable<any> {
    return this.adminRepository.bulkOutfitOperations(request);
  }

  // Analytics Management
  getDetailedAnalytics(filter: AnalyticsFilterRequest): Observable<DetailedAnalyticsDto> {
    return this.adminRepository.getDetailedAnalytics(filter);
  }

  getRealtimeAnalytics(): Observable<RealtimeAnalyticsDto> {
    return this.adminRepository.getRealtimeAnalytics();
  }

  exportAnalytics(request: ExportAnalyticsRequest): Observable<Blob> {
    return this.adminRepository.exportAnalytics(request);
  }

  // System Management
  getSystemHealth(): Observable<SystemHealthDto> {
    return this.adminRepository.getSystemHealth();
  }

  getSystemLogs(filter: SystemLogFilterRequest): Observable<PaginatedResult<SystemLogDto>> {
    return this.adminRepository.getSystemLogs(filter);
  }

  getSystemPerformance(): Observable<SystemPerformanceDto> {
    return this.adminRepository.getSystemPerformance();
  }

  setMaintenanceMode(request: SetMaintenanceModeRequest): Observable<any> {
    return this.adminRepository.setMaintenanceMode(request);
  }

  createBackup(request: BackupRequest): Observable<BackupResult> {
    return this.adminRepository.createBackup(request);
  }

  restartService(serviceName: string): Observable<any> {
    return this.adminRepository.restartService(serviceName);
  }

  clearCache(request: ClearCacheRequest): Observable<any> {
    return this.adminRepository.clearCache(request);
  }
}