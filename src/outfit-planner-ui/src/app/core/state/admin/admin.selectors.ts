import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AdminState } from './admin.reducer';

export const selectAdminState = createFeatureSelector<AdminState>('admin');

export const selectUsers = createSelector(selectAdminState, (state: AdminState) => state.users);
export const selectTotalUsers = createSelector(selectAdminState, (state: AdminState) => state.totalUsers);
export const selectSelectedUser = createSelector(selectAdminState, (state: AdminState) => state.selectedUser);
export const selectUserLoading = createSelector(selectAdminState, (state: AdminState) => state.loading);

export const selectReports = createSelector(selectAdminState, (state: AdminState) => state.reports);
export const selectTotalReports = createSelector(selectAdminState, (state: AdminState) => state.totalReports);
export const selectSelectedReport = createSelector(selectAdminState, (state: AdminState) => state.selectedReport);
export const selectPendingReportsCount = createSelector(selectReports, reports => 
  reports.filter(r => r.status === 'Pending').length
);

export const selectSettings = createSelector(selectAdminState, (state: AdminState) => state.settings);
export const selectAnalytics = createSelector(selectAdminState, (state: AdminState) => state.analytics);
export const selectAuditLogs = createSelector(selectAdminState, (state: AdminState) => state.auditLogs);
export const selectTotalAuditLogs = createSelector(selectAdminState, (state: AdminState) => state.totalAuditLogs);
export const selectAdminError = createSelector(selectAdminState, (state: AdminState) => state.error);
export const selectAdminLoading = createSelector(selectAdminState, (state: AdminState) => state.loading);
