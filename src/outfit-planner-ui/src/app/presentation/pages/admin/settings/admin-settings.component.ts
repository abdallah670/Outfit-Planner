import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSliderModule } from '@angular/material/slider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { selectSettings, selectAdminLoading } from '../../../../core/state/admin/admin.selectors';
import { loadSettings, updateSetting } from '../../../../core/state/admin/admin.actions';
import { SystemSetting } from '../../../../domain/entities/admin.entity';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatSliderModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="admin-settings-container">
      <div class="page-header">
        <h2>System Settings</h2>
        <button mat-raised-button color="primary" (click)="saveAllSettings()" [disabled]="loading$ | async">
          <mat-icon>save</mat-icon>
          Save All Settings
        </button>
      </div>

      <div class="settings-grid">
        @if (loading$ | async) {
          <div class="loading-container">
            <mat-spinner diameter="50"></mat-spinner>
            <p>Loading settings...</p>
          </div>
        } @else {
          @for (setting of settings$ | async; track setting.key) {
            <mat-card class="setting-card">
              <mat-card-header>
                <h3>{{setting.description}}</h3>
                <span class="setting-key">{{setting.key}}</span>
              </mat-card-header>
              <mat-card-content>
                @switch (setting.dataType) {
                  @case ('bool') {
                    <mat-checkbox 
                      [(ngModel)]="settingValues[setting.key]"
                      [disabled]="!setting.isEditable"
                      (change)="onSettingChange(setting.key, $event.checked)">
                      {{setting.isEditable ? 'Enable' : 'Disabled'}}
                    </mat-checkbox>
                  }
                  @case ('string') {
                    @if (setting.key.includes('password')) {
                      <mat-form-field appearance="outline">
                        <mat-label>{{setting.description}}</mat-label>
                        <input matInput 
                          type="password" 
                          [(ngModel)]="settingValues[setting.key]"
                          [disabled]="!setting.isEditable"
                          placeholder="Enter new value">
                      </mat-form-field>
                    } @else {
                      <mat-form-field appearance="outline">
                        <mat-label>{{setting.description}}</mat-label>
                        <input matInput 
                          [(ngModel)]="settingValues[setting.key]"
                          [disabled]="!setting.isEditable"
                          placeholder="Enter new value">
                      </mat-form-field>
                    }
                  }
                  @case ('int') {
                    <mat-form-field appearance="outline">
                      <mat-label>{{setting.description}}</mat-label>
                      <input matInput 
                        type="number" 
                        [(ngModel)]="settingValues[setting.key]"
                        [disabled]="!setting.isEditable"
                        placeholder="Enter number">
                    </mat-form-field>
                  }
                  @case ('json') {
                    <mat-form-field appearance="outline">
                      <mat-label>{{setting.description}}</mat-label>
                      <textarea matInput 
                        [(ngModel)]="settingValues[setting.key]"
                        [disabled]="!setting.isEditable"
                        rows="4"
                        placeholder="Enter JSON value">
                      </textarea>
                    </mat-form-field>
                  }
                  @default {
                    <mat-form-field appearance="outline">
                      <mat-label>{{setting.description}}</mat-label>
                      <input matInput 
                        [(ngModel)]="settingValues[setting.key]"
                        [disabled]="!setting.isEditable"
                        placeholder="Enter value">
                      </mat-form-field>
                  }
                }
              </mat-card-content>
              <mat-card-actions *ngIf="setting.isEditable">
                <button mat-button (click)="saveSetting(setting.key)">
                  <mat-icon>save</mat-icon>
                  Save
                </button>
                <button mat-button (click)="resetSetting(setting.key)">
                  <mat-icon>refresh</mat-icon>
                  Reset
                </button>
              </mat-card-actions>
            </mat-card>
          }
        }
      </div>
    </div>
  `,
  styles: [`
    .admin-settings-container {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }
    
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
    }
    
    .page-header h2 {
      margin: 0;
      color: #333;
    }
    
    .settings-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
      gap: 24px;
    }
    
    .setting-card {
      height: 100%;
    }
    
    .setting-card mat-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    
    .setting-card h3 {
      margin: 0;
      font-size: 16px;
      font-weight: 500;
    }
    
    .setting-key {
      font-family: monospace;
      font-size: 12px;
      color: #666;
      background: #f5f5f5;
      padding: 2px 6px;
      border-radius: 4px;
    }
    
    .setting-card mat-form-field {
      width: 100%;
      margin-bottom: 16px;
    }
    
    .loading-container {
      display: flex;
      justify-content: center;
      align-items: center;
      height: 200px;
      flex-direction: column;
      gap: 16px;
    }
    
    mat-card-actions {
      display: flex;
      gap: 8px;
      padding: 16px;
    }
    
    mat-card-actions button {
      display: flex;
      align-items: center;
      gap: 8px;
    }
  `]
})
export class AdminSettingsComponent implements OnInit {
  private readonly store = inject(Store);
  private readonly snackBar = inject(MatSnackBar);

  settings$: Observable<SystemSetting[]> = this.store.select(selectSettings);
  loading$: Observable<boolean> = this.store.select(selectAdminLoading);

  settingValues: { [key: string]: any } = {};
  originalValues: { [key: string]: any } = {};

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.store.dispatch(loadSettings());
    
    this.settings$.subscribe(settings => {
      settings.forEach(setting => {
        this.settingValues[setting.key] = this.parseValue(setting.value, setting.dataType);
        this.originalValues[setting.key] = this.parseValue(setting.value, setting.dataType);
      });
    });
  }

  onSettingChange(key: string, value: any): void {
    this.settingValues[key] = value;
  }

  saveSetting(key: string): void {
    const value = this.formatValue(this.settingValues[key]);
    this.store.dispatch(updateSetting({ key, value }));
    this.snackBar.open('Setting saved successfully', 'Close', { duration: 3000 });
  }

  saveAllSettings(): void {
    Object.keys(this.settingValues).forEach(key => {
      const value = this.formatValue(this.settingValues[key]);
      this.store.dispatch(updateSetting({ key, value }));
    });
    this.snackBar.open('All settings saved successfully', 'Close', { duration: 3000 });
  }

  resetSetting(key: string): void {
    this.settingValues[key] = this.originalValues[key];
    this.snackBar.open('Setting reset to original value', 'Close', { duration: 3000 });
  }

  private parseValue(value: string, dataType: string): any {
    switch (dataType) {
      case 'bool':
        return value === 'true';
      case 'int':
        return parseInt(value, 10);
      case 'json':
        try {
          return JSON.parse(value);
        } catch {
          return {};
        }
      default:
        return value;
    }
  }

  private formatValue(value: any): string {
    if (typeof value === 'object') {
      return JSON.stringify(value);
    }
    return String(value);
  }
}
