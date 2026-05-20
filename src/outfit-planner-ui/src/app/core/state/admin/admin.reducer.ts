import { createReducer, on } from '@ngrx/store';
import { 
  loadUsers, loadUsersSuccess, loadUsersFailure,
  loadUserDetail, loadUserDetailSuccess, loadUserDetailFailure,
  banUser, banUserSuccess, banUserFailure,
  unbanUser, unbanUserSuccess, unbanUserFailure,
  loadReports, loadReportsSuccess, loadReportsFailure,
  loadReportDetail, loadReportDetailSuccess, loadReportDetailFailure,
  resolveReport, resolveReportSuccess, resolveReportFailure,
  loadDashboardAnalytics, loadDashboardAnalyticsSuccess, loadDashboardAnalyticsFailure,
  loadSettings, loadSettingsSuccess, loadSettingsFailure,
  updateSetting, updateSettingSuccess, updateSettingFailure,
  loadAuditLogs, loadAuditLogsSuccess, loadAuditLogsFailure,
  clearSelectedUser, clearSelectedReport
} from './admin.actions';

export interface AdminState {
  users: any[];
  totalUsers: number;
  selectedUser: any | null;
  reports: any[];
  totalReports: number;
  selectedReport: any | null;
  settings: any[];
  analytics: any | null;
  auditLogs: any[];
  totalAuditLogs: number;
  // Content management state
  posts: any[];
  totalPosts: number;
  polls: any[];
  totalPolls: number;
  outfits: any[];
  totalOutfits: number;
  // System management state
  systemHealth: any | null;
  systemPerformance: any | null;
  systemLogs: any[];
  totalSystemLogs: number;
  loading: boolean;
  error: string | null;
}

export const initialState: AdminState = {
  users: [],
  totalUsers: 0,
  selectedUser: null,
  reports: [],
  totalReports: 0,
  selectedReport: null,
  settings: [],
  analytics: null,
  auditLogs: [],
  totalAuditLogs: 0,
  // Content management state
  posts: [],
  totalPosts: 0,
  polls: [],
  totalPolls: 0,
  outfits: [],
  totalOutfits: 0,
  // System management state
  systemHealth: null,
  systemPerformance: null,
  systemLogs: [],
  totalSystemLogs: 0,
  loading: false,
  error: null
};

export const adminReducer = createReducer(
  initialState,
  on(loadUsers, state => ({ ...state, loading: true, error: null })),
  on(loadUsersSuccess, (state, { users, total }) => ({ 
    ...state, 
    users, 
    totalUsers: total, 
    loading: false 
  })),
  on(loadUsersFailure, (state, { error }) => ({ 
    ...state, 
    loading: false, 
    error 
  })),
  
  on(loadUserDetail, state => ({ ...state, loading: true, error: null })),
  on(loadUserDetailSuccess, (state, { user }) => ({ 
    ...state, 
    selectedUser: user, 
    loading: false 
  })),
  on(loadUserDetailFailure, (state, { error }) => ({ 
    ...state, 
    loading: false, 
    error 
  })),
  
  on(banUser, state => ({ ...state, loading: true, error: null })),
  on(banUserSuccess, (state, { userId }) => ({ 
    ...state, 
    loading: false,
    users: state.users.map(u => u.id === userId ? { ...u, isBanned: true } : u)
  })),
  on(banUserFailure, (state, { error }) => ({ 
    ...state, 
    loading: false, 
    error 
  })),
  
  on(unbanUser, state => ({ ...state, loading: true, error: null })),
  on(unbanUserSuccess, (state, { userId }) => ({ 
    ...state, 
    loading: false,
    users: state.users.map(u => u.id === userId ? { ...u, isBanned: false } : u)
  })),
  on(unbanUserFailure, (state, { error }) => ({ 
    ...state, 
    loading: false, 
    error 
  })),
  
  on(loadReports, state => ({ ...state, loading: true, error: null })),
  on(loadReportsSuccess, (state, { reports, total }) => ({ 
    ...state, 
    reports, 
    totalReports: total, 
    loading: false 
  })),
  on(loadReportsFailure, (state, { error }) => ({ 
    ...state, 
    loading: false, 
    error 
  })),
  
  on(loadReportDetail, state => ({ ...state, loading: true, error: null })),
  on(loadReportDetailSuccess, (state, { report }) => ({ 
    ...state, 
    selectedReport: report, 
    loading: false 
  })),
  on(loadReportDetailFailure, (state, { error }) => ({ 
    ...state, 
    loading: false, 
    error 
  })),
  
  on(resolveReport, state => ({ ...state, loading: true, error: null })),
  on(resolveReportSuccess, (state, { reportId }) => ({ 
    ...state, 
    loading: false,
    reports: state.reports.map(r => r.id === reportId ? { ...r, status: 'Resolved' } : r)
  })),
  on(resolveReportFailure, (state, { error }) => ({ 
    ...state, 
    loading: false, 
    error 
  })),
  
  on(loadDashboardAnalytics, state => ({ ...state, loading: true, error: null })),
  on(loadDashboardAnalyticsSuccess, (state, { analytics }) => ({ 
    ...state, 
    analytics, 
    loading: false 
  })),
  on(loadDashboardAnalyticsFailure, (state, { error }) => ({ 
    ...state, 
    loading: false, 
    error 
  })),
  
  on(loadSettings, state => ({ ...state, loading: true, error: null })),
  on(loadSettingsSuccess, (state, { settings }) => ({ 
    ...state, 
    settings, 
    loading: false 
  })),
  on(loadSettingsFailure, (state, { error }) => ({ 
    ...state, 
    loading: false, 
    error 
  })),
  
  on(updateSetting, state => ({ ...state, loading: true, error: null })),
  on(updateSettingSuccess, (state, { key, value }) => ({ 
    ...state, 
    loading: false,
    settings: state.settings.map(s => s.key === key ? { ...s, value } : s)
  })),
  on(updateSettingFailure, (state, { error }) => ({ 
    ...state, 
    loading: false, 
    error 
  })),
  
  on(loadAuditLogs, state => ({ ...state, loading: true, error: null })),
  on(loadAuditLogsSuccess, (state, { logs, total }) => ({ 
    ...state, 
    auditLogs: logs, 
    totalAuditLogs: total, 
    loading: false 
  })),
  on(loadAuditLogsFailure, (state, { error }) => ({ 
    ...state, 
    loading: false, 
    error 
  })),
  
  on(clearSelectedUser, state => ({ ...state, selectedUser: null })),
  on(clearSelectedReport, state => ({ ...state, selectedReport: null }))
);
