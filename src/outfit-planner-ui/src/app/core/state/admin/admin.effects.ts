import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, mergeMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { AdminUseCases } from '../../../domain/usecases/admin.usecases';
import * as AdminActions from './admin.actions';

@Injectable()
export class AdminEffects {
  private actions$ = inject(Actions);
  private adminUseCases = inject(AdminUseCases);

  loadUsers$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadUsers),
      mergeMap(({ filter }) =>
        this.adminUseCases.getUsers(filter).pipe(
          map((response) => AdminActions.loadUsersSuccess({ users: response.data, total: response.total })),
          catchError(error => of(AdminActions.loadUsersFailure({ error: error.message })))
      ))
    )
  );

  loadUserDetail$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadUserDetail),
      mergeMap(({ userId }) =>
        this.adminUseCases.getUserDetail(userId).pipe(
          map(user => AdminActions.loadUserDetailSuccess({ user })),
          catchError(error => of(AdminActions.loadUserDetailFailure({ error: error.message })))
      ))
    )
  );

  banUser$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.banUser),
      mergeMap(({ userId, reason, expiry }) =>
        this.adminUseCases.banUser(userId, { reason, expiry }).pipe(
          map(() => AdminActions.banUserSuccess({ userId })),
          catchError(error => of(AdminActions.banUserFailure({ error: error.message })))
      ))
    )
  );

  unbanUser$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.unbanUser),
      mergeMap(({ userId }) =>
        this.adminUseCases.unbanUser(userId).pipe(
          map(() => AdminActions.unbanUserSuccess({ userId })),
          catchError(error => of(AdminActions.unbanUserFailure({ error: error.message })))
      ))
    )
  );

  loadReports$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadReports),
      mergeMap(({ filter }) =>
        this.adminUseCases.getReports(filter).pipe(
          map((response) => AdminActions.loadReportsSuccess({ reports: response.data, total: response.total })),
          catchError(error => of(AdminActions.loadReportsFailure({ error: error.message })))
      ))
    )
  );

  loadReportDetail$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadReportDetail),
      mergeMap(({ reportId }) =>
        this.adminUseCases.getReportDetail(reportId).pipe(
          map(report => AdminActions.loadReportDetailSuccess({ report })),
          catchError(error => of(AdminActions.loadReportDetailFailure({ error: error.message })))
      ))
    )
  );

  resolveReport$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.resolveReport),
      mergeMap(({ reportId, resolution, takeAction }) =>
        this.adminUseCases.resolveReport(reportId, { resolution, takeAction }).pipe(
          map(() => AdminActions.resolveReportSuccess({ reportId })),
          catchError(error => of(AdminActions.resolveReportFailure({ error: error.message })))
      ))
    )
  );

  loadSettings$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadSettings),
      mergeMap(() =>
        this.adminUseCases.getSettings().pipe(
          map(settings => AdminActions.loadSettingsSuccess({ settings })),
          catchError(error => of(AdminActions.loadSettingsFailure({ error: error.message })))
      ))
    )
  );

  updateSetting$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.updateSetting),
      mergeMap(({ key, value }) =>
        this.adminUseCases.updateSetting(key, { value }).pipe(
          map(() => AdminActions.updateSettingSuccess({ key, value })),
          catchError(error => of(AdminActions.updateSettingFailure({ error: error.message })))
      ))
    )
  );

  loadDashboardAnalytics$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadDashboardAnalytics),
      mergeMap(({ startDate, endDate }) =>
        this.adminUseCases.getDashboardAnalytics(startDate, endDate).pipe(
          map(analytics => AdminActions.loadDashboardAnalyticsSuccess({ analytics })),
          catchError(error => of(AdminActions.loadDashboardAnalyticsFailure({ error: error.message })))
      ))
    )
  );

  loadAuditLogs$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadAuditLogs),
      mergeMap(({ filter }) =>
        this.adminUseCases.getAuditLogs(filter).pipe(
          map((response) => AdminActions.loadAuditLogsSuccess({ logs: response.data, total: response.total })),
          catchError(error => of(AdminActions.loadAuditLogsFailure({ error: error.message })))
      ))
    )
  );

  loadLockedAccounts$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadLockedAccounts),
      mergeMap(() =>
        this.adminUseCases.getLockedAccounts().pipe(
          map((accounts: any[]) => AdminActions.loadLockedAccountsSuccess({ accounts })),
          catchError(error => of(AdminActions.loadLockedAccountsFailure({ error: error.message })))
      ))
    )
  );

  unlockAccount$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.unlockAccount),
      mergeMap(({ userId }) =>
        this.adminUseCases.unlockAccount(userId).pipe(
          map(() => AdminActions.unlockAccountSuccess({ userId })),
          catchError(error => of(AdminActions.unlockAccountFailure({ error: error.message })))
      ))
    )
  );
}