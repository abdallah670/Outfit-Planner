export interface AdminUser {
  id: string;
  userName: string;
  email: string;
  name: string;
  roles: string[];
  isLocked: boolean;
  isBanned: boolean;
  createdAt: string;
}

export interface AdminUserDetail extends AdminUser {
  outfitCount: number;
  postCount: number;
  commentsCount: number;
  recentActivity: AuditLogEntry[];
}

export interface ContentReport {
  id: string;
  reporterUserName: string | null;
  targetUserId: string;
  contentType: string;
  reason: string;
  status: string;
  createdAt: string;
}

export interface ContentReportDetail extends ContentReport {
  reporterEmail: string | null;
  contentPreview: string | null;
  targetUserName: string | null;
  resolution: string | null;
  resolvedAt: string | null;
}

export interface SystemSetting {
  key: string;
  value: string;
  dataType: string;
  description: string;
  isEditable: boolean;
}

export interface AnalyticsDashboard {
  totalUsers: number;
  newUsersToday: number;
  activeUsers: number;
  totalOutfits: number;
  totalPosts: number;
  totalPolls: number;
  pendingReports: number;
  resolvedReports: number;
  lockedAccounts: number;
  bannedUsers: number;
}

export interface AuditLogEntry {
  id: string;
  userName: string;
  action: string;
  entityType: string;
  timestamp: string;
}

export interface PaginatedResult<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface UserFilterRequest {
  search?: string;
  role?: string;
  status?: string;
  page?: number;
  pageSize?: number;
}

export interface ReportFilterRequest {
  status?: string;
  contentType?: string;
  page?: number;
  pageSize?: number;
}

export interface BanUserRequest {
  reason: string;
  expiry?: Date;
}

export interface ResolveReportRequest {
  resolution: string;
  takeAction: boolean;
}

export interface UpdateSettingRequest {
  value: string;
}

// Content Management DTOs
export interface AdminPostDto {
  id: string;
  userId: string;
  userName: string;
  caption?: string;
  tags: string[];
  likesCount: number;
  commentsCount: number;
  createdAt: string;
  postType: number;
  outfitId?: string;
  outfitName?: string;
  outfitImageUrl?: string;
  itemsImageUrls?: string[];
  pollId?: string;
  pollQuestion?: string;
  pollOptions?: string[];
  pollOptionVotes?: number[];
  totalPollVotes?: number;
  pollExpiresAt?: string;
  isApproved: boolean;
  status: string;
  approvedAt: string | null;
  approvedBy: string | null;
}

export interface AdminPollDto {
  id: string;
  userId: string;
  userName: string;
  question: string;
  options: string[];
  optionVotes: number[];
  totalVotes: number;
  createdAt: string;
  endsAt: string | null;
  status: string;
  isFeatured: boolean;
  featuredAt: string | null;
  featuredBy: string | null;
}

export interface AdminOutfitDto {
  id: string;
  userId: string;
  userName: string;
  name: string;
  description: string;
  tags: string[];
  imageUrls: string[];
  likesCount: number;
  commentsCount: number;
  createdAt: string;
  isFeatured: boolean;
  isApproved: boolean;
  featuredAt: string | null;
  featuredBy: string | null;
  approvedAt: string | null;
  approvedBy: string | null;
}

export interface ContentFilterRequest {
  search?: string;
  status?: string;
  contentType?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}

export interface BulkPostOperationRequest {
  operations: BulkPostOperationItem[];
}

export interface BulkPostOperationItem {
  postId: string;
  type: string; // "approve", "reject", "delete"
  reason?: string;
}

export interface BulkPollOperationRequest {
  operations: BulkPollOperationItem[];
}

export interface BulkPollOperationItem {
  pollId: string;
  type: string; // "close", "feature", "unfeature", "delete"
  reason?: string;
}

export interface BulkOutfitOperationRequest {
  operations: BulkOutfitOperationItem[];
}

export interface BulkOutfitOperationItem {
  outfitId: string;
  type: string; // "feature", "unfeature", "approve", "reject", "delete"
  reason?: string;
}

// Analytics DTOs
export interface DetailedAnalyticsDto {
  userMetrics: UserEngagementMetrics;
  contentStats: ContentMetrics;
  systemStats: SystemPerformanceMetrics;
  trends: TimeSeriesData[];
  summaries: AnalyticsSummary[];
}

export interface UserEngagementMetrics {
  totalUsers: number;
  activeUsers: number;
  newUsers: number;
  userGrowthRate: number;
  dailyActivity: UserActivityData[];
  demographics: UserDemographics[];
}

export interface ContentMetrics {
  totalOutfits: number;
  totalPosts: number;
  totalPolls: number;
  totalComments: number;
  totalLikes: number;
  engagementRate: number;
  topContent: ContentPerformanceData[];
  contentTypeBreakdown: ContentTypeStats[];
}

export interface SystemPerformanceMetrics {
  cpuUsage: number;
  memoryUsage: number;
  diskUsage: number;
  activeConnections: number;
  responseTime: number;
  historicalPerformance: PerformanceData[];
}

export interface TimeSeriesData {
  date: string;
  metric: string;
  value: number;
}

export interface AnalyticsSummary {
  category: string;
  metric: string;
  value: number;
  changePercentage: number;
}

export interface UserActivityData {
  date: string;
  activeUsers: number;
  newUsers: number;
  returningUsers: number;
}

export interface UserDemographics {
  category: string;
  value: string;
  count: number;
  percentage: number;
}

export interface ContentPerformanceData {
  id: string;
  name: string;
  type: string;
  views: number;
  likes: number;
  comments: number;
  engagementScore: number;
}

export interface ContentTypeStats {
  type: string;
  count: number;
  totalEngagement: number;
  averageEngagement: number;
}

export interface PerformanceData {
  timestamp: string;
  cpuUsage: number;
  memoryUsage: number;
  responseTime: number;
}

export interface RealtimeAnalyticsDto {
  userMetrics: UserEngagementMetrics;
  contentStats: ContentMetrics;
  systemStats: SystemPerformanceMetrics;
  lastUpdated: string;
}

export interface AnalyticsFilterRequest {
  startDate?: string;
  endDate?: string;
  contentType?: string;
  metricType?: string;
}

export interface ExportAnalyticsRequest {
  format: string; // "csv", "json", "pdf"
  startDate?: string;
  endDate?: string;
}

// System Management DTOs
export interface SystemHealthDto {
  databaseHealthy: boolean;
  cacheHealthy: boolean;
  emailServiceHealthy: boolean;
  cpuUsage: number;
  memoryUsage: number;
  diskUsage: number;
  lastCheck: string;
  services: string[];
  healthChecks: HealthCheckResult[];
}

export interface SystemLogDto {
  id: string;
  timestamp: string;
  level: string;
  category: string;
  message: string;
  exceptionDetails?: string;
  userId?: string;
  userName?: string;
  ipAddress?: string;
  userAgent?: string;
}

export interface SystemPerformanceDto {
  cpuUsage: number;
  memoryUsage: number;
  diskUsage: number;
  activeConnections: number;
  averageResponseTime: number;
  requestsPerSecond: number;
  metrics: PerformanceMetric[];
  lastUpdated: string;
}

export interface HealthCheckResult {
  service: string;
  healthy: boolean;
  message: string;
  responseTime: string;
  lastCheck: string;
}

export interface PerformanceMetric {
  name: string;
  value: number;
  unit: string;
  timestamp: string;
}

export interface SystemLogFilterRequest {
  level?: string;
  startDate?: string;
  endDate?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface SetMaintenanceModeRequest {
  enabled: boolean;
  message?: string;
}

export interface BackupRequest {
  type: string; // "full", "incremental", "differential"
  description?: string;
}

export interface BackupResult {
  success: boolean;
  backupId: string;
  fileName: string;
  size: number;
  createdAt: string;
  message?: string;
}

export interface ClearCacheRequest {
  cacheKey?: string;
}