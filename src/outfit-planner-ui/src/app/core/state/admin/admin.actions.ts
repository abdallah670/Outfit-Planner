import { createAction, props } from '@ngrx/store';

// User Management Actions
export const loadUsers = createAction('[Admin] Load Users', props<{ filter: any }>());
export const loadUsersSuccess = createAction('[Admin] Load Users Success', props<{ users: any[], total: number }>());
export const loadUsersFailure = createAction('[Admin] Load Users Failure', props<{ error: string }>());

export const loadUserDetail = createAction('[Admin] Load User Detail', props<{ userId: string }>());
export const loadUserDetailSuccess = createAction('[Admin] Load User Detail Success', props<{ user: any }>());
export const loadUserDetailFailure = createAction('[Admin] Load User Detail Failure', props<{ error: string }>());

export const banUser = createAction('[Admin] Ban User', props<{ userId: string, reason: string, expiry?: Date }>());
export const banUserSuccess = createAction('[Admin] Ban User Success', props<{ userId: string }>());
export const banUserFailure = createAction('[Admin] Ban User Failure', props<{ error: string }>());

export const unbanUser = createAction('[Admin] Unban User', props<{ userId: string }>());
export const unbanUserSuccess = createAction('[Admin] Unban User Success', props<{ userId: string }>());
export const unbanUserFailure = createAction('[Admin] Unban User Failure', props<{ error: string }>());

// Reports Actions
export const loadReports = createAction('[Admin] Load Reports', props<{ filter: any }>());
export const loadReportsSuccess = createAction('[Admin] Load Reports Success', props<{ reports: any[], total: number }>());
export const loadReportsFailure = createAction('[Admin] Load Reports Failure', props<{ error: string }>());

export const loadReportDetail = createAction('[Admin] Load Report Detail', props<{ reportId: string }>());
export const loadReportDetailSuccess = createAction('[Admin] Load Report Detail Success', props<{ report: any }>());
export const loadReportDetailFailure = createAction('[Admin] Load Report Detail Failure', props<{ error: string }>());

export const resolveReport = createAction('[Admin] Resolve Report', props<{ reportId: string, resolution: string, takeAction: boolean }>());
export const resolveReportSuccess = createAction('[Admin] Resolve Report Success', props<{ reportId: string }>());
export const resolveReportFailure = createAction('[Admin] Resolve Report Failure', props<{ error: string }>());

// Analytics Actions
export const loadDashboardAnalytics = createAction('[Admin] Load Dashboard Analytics', props<{ startDate?: Date, endDate?: Date }>());
export const loadDashboardAnalyticsSuccess = createAction('[Admin] Load Dashboard Analytics Success', props<{ analytics: any }>());
export const loadDashboardAnalyticsFailure = createAction('[Admin] Load Dashboard Analytics Failure', props<{ error: string }>());

// Settings Actions
export const loadSettings = createAction('[Admin] Load Settings');
export const loadSettingsSuccess = createAction('[Admin] Load Settings Success', props<{ settings: any[] }>());
export const loadSettingsFailure = createAction('[Admin] Load Settings Failure', props<{ error: string }>());

export const updateSetting = createAction('[Admin] Update Setting', props<{ key: string, value: string }>());
export const updateSettingSuccess = createAction('[Admin] Update Setting Success', props<{ key: string, value: string }>());
export const updateSettingFailure = createAction('[Admin] Update Setting Failure', props<{ error: string }>());

// Audit Log Actions
export const loadAuditLogs = createAction('[Admin] Load Audit Logs', props<{ filter: any }>());
export const loadAuditLogsSuccess = createAction('[Admin] Load Audit Logs Success', props<{ logs: any[], total: number }>());
export const loadAuditLogsFailure = createAction('[Admin] Load Audit Logs Failure', props<{ error: string }>());

// Clear selected items
export const clearSelectedUser = createAction('[Admin] Clear Selected User');
export const clearSelectedReport = createAction('[Admin] Clear Selected Report');

// Locked Accounts
export const loadLockedAccounts = createAction('[Admin] Load Locked Accounts');
export const loadLockedAccountsSuccess = createAction('[Admin] Load Locked Accounts Success', props<{ accounts: any[] }>());
export const loadLockedAccountsFailure = createAction('[Admin] Load Locked Accounts Failure', props<{ error: string }>());
export const unlockAccount = createAction('[Admin] Unlock Account', props<{ userId: string }>());
export const unlockAccountSuccess = createAction('[Admin] Unlock Account Success', props<{ userId: string }>());
export const unlockAccountFailure = createAction('[Admin] Unlock Account Failure', props<{ error: string }>());
