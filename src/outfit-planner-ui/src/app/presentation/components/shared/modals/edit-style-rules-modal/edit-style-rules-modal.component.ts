import { Component, Inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { Subscription } from 'rxjs';
import { filter, take } from 'rxjs/operators';
import { StyleRule } from '../../../../../domain/entities/user-profile.entity';
import { UserActions } from '../../../../../core/state/user/user.actions';
import { selectUserError, selectStyleRulesLoading } from '../../../../../core/state/user/user.selectors';

export interface EditStyleRulesModalData {
  rules?: StyleRule[];  // Array of rules for batch editing
  rule?: StyleRule | null;  // Single rule for add/edit
  isNew?: boolean;  // Whether this is a new rule
}

@Component({
  selector: 'app-edit-style-rules-modal',
  templateUrl: './edit-style-rules-modal.component.html',
  styleUrls: ['./edit-style-rules-modal.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatIconModule,
    MatSnackBarModule,
  ],
})
export class EditStyleRulesModalComponent implements OnDestroy {
  rules: StyleRule[] = [];
  isLoading = false;
  private errorSubscription?: Subscription;
  private loadingSubscription?: Subscription;
  private deletedRuleIds: string[] = []; // Track deleted rule IDs

  constructor(
    private dialogRef: MatDialogRef<EditStyleRulesModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EditStyleRulesModalData,
    private store: Store,
    private snackBar: MatSnackBar,
  ) {
    // Support both array (data.rules) and single rule (data.rule) formats
    if (data.rules && Array.isArray(data.rules)) {
      // Batch editing mode - deep copy the rules
      this.rules = data.rules.map(rule => ({ ...rule }));
    } else if (data.rule) {
      // Single rule editing mode
      this.rules = [{ ...data.rule }];
    } else {
      // New rule mode - start with empty rule
      this.rules = [];
    }
    
    // If no rules, add an empty one
    if (this.rules.length === 0) {
      this.addRule();
    }
  }

  addRule(): void {
    const newRule: StyleRule = {
      id: '', // Empty ID means new rule
      name: '',
      description: '',
      isActive: true,
      criteriaJson: '{}',
    };
    this.rules = [...this.rules, newRule];
  }

  removeRule(index: number): void {
    const rule = this.rules[index];
    
    // If rule has an ID, it's an existing rule that needs to be deleted from backend
    if (rule.id) {
      this.deletedRuleIds.push(rule.id);
      this.store.dispatch(UserActions.deleteStyleRule({ id: rule.id }));
    }
    
    this.rules = this.rules.filter((_, i) => i !== index);
  }

  onSave(): void {
    this.isLoading = true;

    // Process each rule - create or update
    this.rules.forEach((rule, index) => {
      if (rule.id) {
        // Update existing rule
        this.store.dispatch(
          UserActions.updateStyleRule({
            id: rule.id,
            rule: {
              id: rule.id,
              name: rule.name || `Rule ${index + 1}`,
              description: rule.description || '',
              isActive: rule.isActive ?? true,
              criteriaJson: rule.criteriaJson || '{}',
            },
          })
        );
      } else {
        // Create new rule
        this.store.dispatch(
          UserActions.createStyleRule({
            rule: {
              name: rule.name || `Rule ${index + 1}`,
              description: rule.description || '',
              criteriaJson: rule.criteriaJson || '{}',
            },
          })
        );
      }
    });

    // Subscribe to loading state
    this.loadingSubscription = this.store.select(selectStyleRulesLoading).pipe(
      filter(loading => loading === false),
      take(1)
    ).subscribe(() => {
      // Check for errors
      this.store.select(selectUserError).pipe(take(1)).subscribe((error: string | null) => {
        this.isLoading = false;
        if (error) {
          this.snackBar.open(`Failed to save rules: ${error}`, 'Close', { duration: 5000 });
        } else {
          this.snackBar.open('Style rules saved successfully!', 'Close', { duration: 3000 });
          this.dialogRef.close(true);
        }
      });
    });

    // Fallback timeout
    setTimeout(() => {
      if (this.isLoading) {
        this.isLoading = false;
        this.snackBar.open('Save timed out. Please try again.', 'Close', { duration: 5000 });
      }
    }, 10000);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  ngOnDestroy(): void {
    this.errorSubscription?.unsubscribe();
    this.loadingSubscription?.unsubscribe();
  }
}
