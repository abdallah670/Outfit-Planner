import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
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
export class AdminDataSource {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.baseUrl}/Admin`;

  private buildHttpParams(params: any): HttpParams {
    let httpParams = new HttpParams();
    Object.keys(params).forEach(key => {
      if (params[key] !== undefined && params[key] !== null) {
        httpParams = httpParams.set(key, params[key].toString());
      }
    });
    return httpParams;
  }

  // User Management
  getUsers(filter: UserFilterRequest): Observable<PaginatedResult<AdminUser>> {
    const params = this.buildHttpParams(filter);
    return this.http.get<PaginatedResult<AdminUser>>(`${this.apiUrl}/users`, { params });
  }

  getUserDetail(userId: string): Observable<AdminUserDetail> {
    return this.http.get<AdminUserDetail>(`${this.apiUrl}/users/${userId}`);
  }

  banUser(userId: string, request: BanUserRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/users/${userId}/ban`, request);
  }

  unbanUser(userId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/users/${userId}/unban`, {});
  }

  // Reports
  getReports(filter: ReportFilterRequest): Observable<PaginatedResult<ContentReport>> {
    const params = this.buildHttpParams(filter);
    return this.http.get<PaginatedResult<ContentReport>>(`${this.apiUrl}/reports`, { params });
  }

  getReportDetail(reportId: string): Observable<ContentReportDetail> {
    return this.http.get<ContentReportDetail>(`${this.apiUrl}/reports/${reportId}`);
  }

  resolveReport(reportId: string, request: ResolveReportRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/reports/${reportId}/resolve`, request);
  }

  // Settings
  getSettings(): Observable<SystemSetting[]> {
    return this.http.get<SystemSetting[]>(`${this.apiUrl}/settings`);
  }

  updateSetting(key: string, request: UpdateSettingRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/settings/${key}`, request);
  }

  // Analytics
  getDashboardAnalytics(startDate?: Date, endDate?: Date): Observable<AnalyticsDashboard> {
    const params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();
    return this.http.get<AnalyticsDashboard>(`${this.apiUrl}/analytics/dashboard`, { params });
  }

  // Audit Logs
  getAuditLogs(filter: any): Observable<PaginatedResult<AuditLogEntry>> {
    const params = this.buildHttpParams(filter);
    return this.http.get<PaginatedResult<AuditLogEntry>>(`${this.apiUrl}/audit-logs`, { params });
  }

  // Account Management
  getLockedAccounts(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/locked-accounts`);
  }

  unlockAccount(userId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/unlock-account/${userId}`, {});
  }

  // Content Management - Posts
  getPosts(filter: ContentFilterRequest): Observable<PaginatedResult<AdminPostDto>> {
    const params = this.buildHttpParams(filter);
    return this.http.get<PaginatedResult<AdminPostDto>>(`${this.apiUrl}/content/posts`, { params });
  }

  approvePost(postId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/content/posts/${postId}/approve`, {});
  }

  rejectPost(postId: string, request: any): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/content/posts/${postId}/reject`, request);
  }

  deletePost(postId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/content/posts/${postId}`);
  }

  bulkPostOperations(request: BulkPostOperationRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/content/posts/bulk`, request);
  }

  // Content Management - Polls
  getPolls(filter: ContentFilterRequest): Observable<PaginatedResult<AdminPollDto>> {
    const params = this.buildHttpParams(filter);
    return this.http.get<PaginatedResult<AdminPollDto>>(`${this.apiUrl}/content/polls`, { params });
  }

  closePoll(pollId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/content/polls/${pollId}/close`, {});
  }

  featurePoll(pollId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/content/polls/${pollId}/feature`, {});
  }

  unfeaturePoll(pollId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/content/polls/${pollId}/unfeature`, {});
  }

  deletePoll(pollId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/content/polls/${pollId}`);
  }

  bulkPollOperations(request: BulkPollOperationRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/content/polls/bulk`, request);
  }

  // Content Management - Outfits
  getOutfits(filter: ContentFilterRequest): Observable<PaginatedResult<AdminOutfitDto>> {
    const params = this.buildHttpParams(filter);
    return this.http.get<PaginatedResult<AdminOutfitDto>>(`${this.apiUrl}/content/outfits`, { params });
  }

  featureOutfit(outfitId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/content/outfits/${outfitId}/feature`, {});
  }

  unfeatureOutfit(outfitId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/content/outfits/${outfitId}/unfeature`, {});
  }

  approveOutfit(outfitId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/content/outfits/${outfitId}/approve`, {});
  }

  rejectOutfit(outfitId: string, request: any): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/content/outfits/${outfitId}/reject`, request);
  }

  deleteOutfit(outfitId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/content/outfits/${outfitId}`);
  }

  bulkOutfitOperations(request: BulkOutfitOperationRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/content/outfits/bulk`, request);
  }

  // Analytics Management
  getDetailedAnalytics(filter: AnalyticsFilterRequest): Observable<DetailedAnalyticsDto> {
    const params = this.buildHttpParams(filter);
    return this.http.get<DetailedAnalyticsDto>(`${this.apiUrl}/analytics/detailed`, { params });
  }

  getRealtimeAnalytics(): Observable<RealtimeAnalyticsDto> {
    return this.http.get<RealtimeAnalyticsDto>(`${this.apiUrl}/analytics/realtime`);
  }

  exportAnalytics(request: ExportAnalyticsRequest): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/analytics/export`, request, {
      responseType: 'blob'
    });
  }

  // System Management
  getSystemHealth(): Observable<SystemHealthDto> {
    return this.http.get<SystemHealthDto>(`${this.apiUrl}/system/health`);
  }

  getSystemLogs(filter: SystemLogFilterRequest): Observable<PaginatedResult<SystemLogDto>> {
    const params = this.buildHttpParams(filter);
    return this.http.get<PaginatedResult<SystemLogDto>>(`${this.apiUrl}/system/logs`, { params });
  }

  getSystemPerformance(): Observable<SystemPerformanceDto> {
    return this.http.get<SystemPerformanceDto>(`${this.apiUrl}/system/performance`);
  }

  setMaintenanceMode(request: SetMaintenanceModeRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/system/maintenance`, request);
  }

  createBackup(request: BackupRequest): Observable<BackupResult> {
    return this.http.post<BackupResult>(`${this.apiUrl}/system/backup`, request);
  }

  restartService(serviceName: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/system/restart/${serviceName}`, {});
  }

  clearCache(request: ClearCacheRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/system/clear-cache`, request);
  }
}