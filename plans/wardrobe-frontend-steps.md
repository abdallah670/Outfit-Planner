# Detailed Implementation Plan: Wardrobe Frontend Completion

This document outlines the specific technical steps to complete the Wardrobe frontend of the Outfit Planner, as defined in Section 1 of `vertical-slice-tasks.md`. It includes detailed code snippets for each file to ensure clear and easy implementation.

---

## 1. NgRx State Management (Task 79)

### 1.1 Fix and Complete `wardrobe.actions.ts`

**Path:** `src/outfit-planner-ui/src/app/core/state/wardrobe/wardrobe.actions.ts`

Replace the entire contents of the file with the following code. It fixes the syntax error (`=` instead of `:`) and adds the correct imports.

```typescript
import { createActionGroup, emptyProps, props } from "@ngrx/store";

// Assuming ClothingItem is defined, if not, create it in domain/entities
// Update path based on your exact domain structure
export interface ClothingItem {
  id: string;
  name: string;
  description?: string;
  clothingType: string;
  category: string;
  brand?: string;
  size?: string;
  primaryColor: string;
  imageUrl?: string;
  purchasePrice?: number;
  wearCount: number;
  lastWorn?: string;
}

export const WardrobeActions = createActionGroup({
  source: "wardrobe",
  events: {
    "Load Clothing Items": emptyProps(),
    "Load Clothing Items Success": props<{ items: ClothingItem[] }>(),
    "Load Clothing Items Failure": props<{ error: string }>(),

    "Load Clothing Item By Id": props<{ id: string }>(),
    "Load Clothing Item By Id Success": props<{ item: ClothingItem }>(),
    "Load Clothing Item By Id Failure": props<{ error: string }>(),

    "Load Clothing Items By Category": props<{ category: string }>(),
    "Load Clothing Items By Category Success": props<{
      items: ClothingItem[];
    }>(),
    "Load Clothing Items By Category Failure": props<{ error: string }>(),

    "Create Clothing Item": props<{
      item: Partial<ClothingItem>;
      image?: File;
    }>(),
    "Create Clothing Item Success": props<{ item: ClothingItem }>(),
    "Create Clothing Item Failure": props<{ error: string }>(),

    "Update Clothing Item": props<{
      id: string;
      item: Partial<ClothingItem>;
      image?: File;
    }>(),
    "Update Clothing Item Success": props<{ item: ClothingItem }>(),
    "Update Clothing Item Failure": props<{ error: string }>(),

    "Delete Clothing Item": props<{ id: string }>(),
    "Delete Clothing Item Success": props<{ id: string }>(),
    "Delete Clothing Item Failure": props<{ error: string }>(),

    "Record Wear": props<{ id: string }>(),
    "Record Wear Success": props<{ id: string }>(),
    "Record Wear Failure": props<{ error: string }>(),
  },
});
```

### 1.2 Implement `wardrobe.reducer.ts`

**Path:** `src/outfit-planner-ui/src/app/core/state/wardrobe/wardrobe.reducer.ts`

Add the state definition and handlers for all actions.

```typescript
import { createReducer, on } from "@ngrx/store";
import { WardrobeActions, ClothingItem } from "./wardrobe.actions";

export interface WardrobeState {
  items: ClothingItem[];
  selectedItem: ClothingItem | null;
  loading: boolean;
  error: string | null;
  filter: { category: string | null };
}

export const initialState: WardrobeState = {
  items: [],
  selectedItem: null,
  loading: false,
  error: null,
  filter: { category: null },
};

export const wardrobeReducer = createReducer(
  initialState,

  // Load Items
  on(WardrobeActions.loadClothingItems, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(WardrobeActions.loadClothingItemsSuccess, (state, { items }) => ({
    ...state,
    items,
    loading: false,
  })),
  on(WardrobeActions.loadClothingItemsFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false,
  })),

  // Load Item By Id
  on(WardrobeActions.loadClothingItemById, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(WardrobeActions.loadClothingItemByIdSuccess, (state, { item }) => ({
    ...state,
    selectedItem: item,
    loading: false,
  })),
  on(WardrobeActions.loadClothingItemByIdFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false,
  })),

  // Create Item
  on(WardrobeActions.createClothingItem, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(WardrobeActions.createClothingItemSuccess, (state, { item }) => ({
    ...state,
    items: [...state.items, item],
    loading: false,
  })),
  on(WardrobeActions.createClothingItemFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false,
  })),

  // Update Item
  on(WardrobeActions.updateClothingItem, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(WardrobeActions.updateClothingItemSuccess, (state, { item }) => ({
    ...state,
    items: state.items.map((i) => (i.id === item.id ? item : i)),
    selectedItem:
      state.selectedItem?.id === item.id ? item : state.selectedItem,
    loading: false,
  })),
  on(WardrobeActions.updateClothingItemFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false,
  })),

  // Delete Item
  on(WardrobeActions.deleteClothingItem, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(WardrobeActions.deleteClothingItemSuccess, (state, { id }) => ({
    ...state,
    items: state.items.filter((i) => i.id !== id),
    loading: false,
  })),
  on(WardrobeActions.deleteClothingItemFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false,
  })),

  // Record Wear
  on(WardrobeActions.recordWear, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(WardrobeActions.recordWearSuccess, (state, { id }) => ({
    ...state,
    items: state.items.map((i) =>
      i.id === id ? { ...i, wearCount: i.wearCount + 1 } : i,
    ),
    selectedItem:
      state.selectedItem?.id === id
        ? { ...state.selectedItem, wearCount: state.selectedItem.wearCount + 1 }
        : state.selectedItem,
    loading: false,
  })),
  on(WardrobeActions.recordWearFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false,
  })),
);
```

### 1.3 Implement `wardrobe.effects.ts`

**Path:** `src/outfit-planner-ui/src/app/core/state/wardrobe/wardrobe.effects.ts`

Set up the effects to listen to actions and make HTTP calls. _Note: You'll need an injected service like `WardrobeService`._

```typescript
import { Injectable, inject } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { catchError, map, mergeMap, of } from "rxjs";
import { WardrobeActions } from "./wardrobe.actions";
import { HttpClient } from "@angular/common/http";
// Import your environment to get API URL
// import { environment } from '../../../../environments/environment';

@Injectable()
export class WardrobeEffects {
  private actions$ = inject(Actions);
  private http = inject(HttpClient);
  private apiUrl = "https://localhost:5001/api/wardrobe"; // Update with environment.apiUrl

  loadClothingItems$ = createEffect(() =>
    this.actions$.pipe(
      ofType(WardrobeActions.loadClothingItems),
      mergeMap(() =>
        this.http.get<any[]>(this.apiUrl).pipe(
          map((items) => WardrobeActions.loadClothingItemsSuccess({ items })),
          catchError((error) =>
            of(
              WardrobeActions.loadClothingItemsFailure({
                error: error.message,
              }),
            ),
          ),
        ),
      ),
    ),
  );

  loadClothingItemById$ = createEffect(() =>
    this.actions$.pipe(
      ofType(WardrobeActions.loadClothingItemById),
      mergeMap(({ id }) =>
        this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
          map((item) => WardrobeActions.loadClothingItemByIdSuccess({ item })),
          catchError((error) =>
            of(
              WardrobeActions.loadClothingItemByIdFailure({
                error: error.message,
              }),
            ),
          ),
        ),
      ),
    ),
  );

  // Create a FormData object when uploading images
  createClothingItem$ = createEffect(() =>
    this.actions$.pipe(
      ofType(WardrobeActions.createClothingItem),
      mergeMap(({ item, image }) => {
        const formData = new FormData();
        formData.append("data", JSON.stringify(item));
        if (image) formData.append("image", image);

        return this.http.post<any>(this.apiUrl, formData).pipe(
          map((newItem) =>
            WardrobeActions.createClothingItemSuccess({ item: newItem }),
          ),
          catchError((error) =>
            of(
              WardrobeActions.createClothingItemFailure({
                error: error.message,
              }),
            ),
          ),
        );
      }),
    ),
  );

  // Add similar effects for update, delete, and record wear here following the pattern above.
  deleteClothingItem$ = createEffect(() =>
    this.actions$.pipe(
      ofType(WardrobeActions.deleteClothingItem),
      mergeMap(({ id }) =>
        this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
          map(() => WardrobeActions.deleteClothingItemSuccess({ id })),
          catchError((error) =>
            of(
              WardrobeActions.deleteClothingItemFailure({
                error: error.message,
              }),
            ),
          ),
        ),
      ),
    ),
  );

  recordWear$ = createEffect(() =>
    this.actions$.pipe(
      ofType(WardrobeActions.recordWear),
      mergeMap(({ id }) =>
        this.http.post<void>(`${this.apiUrl}/${id}/wear/quick`, {}).pipe(
          map(() => WardrobeActions.recordWearSuccess({ id })),
          catchError((error) =>
            of(WardrobeActions.recordWearFailure({ error: error.message })),
          ),
        ),
      ),
    ),
  );
}
```

### 1.4 Implement `wardrobe.selectors.ts`

**Path:** `src/outfit-planner-ui/src/app/core/state/wardrobe/wardrobe.selectors.ts`

```typescript
import { createFeatureSelector, createSelector } from "@ngrx/store";
import { WardrobeState } from "./wardrobe.reducer";

export const selectWardrobeState =
  createFeatureSelector<WardrobeState>("wardrobe");

export const selectAllItems = createSelector(
  selectWardrobeState,
  (state) => state.items,
);

export const selectSelectedItem = createSelector(
  selectWardrobeState,
  (state) => state.selectedItem,
);

export const selectWardrobeLoading = createSelector(
  selectWardrobeState,
  (state) => state.loading,
);

export const selectWardrobeError = createSelector(
  selectWardrobeState,
  (state) => state.error,
);

// Advanced selector example for Dashboard
export const selectWardrobeStats = createSelector(selectAllItems, (items) => {
  return {
    totalItems: items.length,
    totalCost: items.reduce((acc, curr) => acc + (curr.purchasePrice || 0), 0),
  };
});
```

---

## 2. Shared/Common UI Components

### 2.1 Clothing Card Component

Create this component using Angular CLI:
`ng generate component presentation/components/clothing-card`

**`clothing-card.component.ts`**

```typescript
import { Component, Input, Output, EventEmitter } from "@angular/core";
import { ClothingItem } from "../../../core/state/wardrobe/wardrobe.actions";
import { CommonModule } from "@angular/common";
import { MatCardModule } from "@angular/material/card";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { RouterModule } from "@angular/router";

@Component({
  selector: "app-clothing-card",
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    RouterModule,
  ],
  template: `
    <mat-card class="clothing-card">
      <img
        mat-card-image
        [src]="item.imageUrl || 'assets/placeholder-clothes.png'"
        [alt]="item.name"
      />
      <mat-card-header>
        <mat-card-title>{{ item.name }}</mat-card-title>
        <mat-card-subtitle
          >{{ item.brand }} - {{ item.clothingType }}</mat-card-subtitle
        >
      </mat-card-header>
      <mat-card-content>
        <div
          class="color-swatch"
          [style.background-color]="item.primaryColor"
        ></div>
        <p>Worn: {{ item.wearCount }} times</p>
      </mat-card-content>
      <mat-card-actions align="end">
        <button
          mat-icon-button
          color="primary"
          [routerLink]="['/wardrobe', item.id]"
        >
          <mat-icon>visibility</mat-icon>
        </button>
        <button mat-icon-button color="warn" (click)="delete.emit(item.id)">
          <mat-icon>delete</mat-icon>
        </button>
      </mat-card-actions>
    </mat-card>
  `,
  styles: [
    `
      .clothing-card {
        max-width: 300px;
        margin-bottom: 20px;
        border-radius: 12px;
      }
      img {
        height: 200px;
        object-fit: cover;
      }
      .color-swatch {
        width: 24px;
        height: 24px;
        border-radius: 50%;
        border: 1px solid #ccc;
        margin-bottom: 8px;
      }
    `,
  ],
})
export class ClothingCardComponent {
  @Input({ required: true }) item!: ClothingItem;
  @Output() delete = new EventEmitter<string>();
}
```

---

## 3. Pages & Features

### 3.1 Wardrobe Dashboard (Task 87)

`ng generate component presentation/pages/wardrobe-dashboard`

**`wardrobe-dashboard.component.ts`**

```typescript
import { Component, OnInit, inject } from "@angular/core";
import { Store } from "@ngrx/store";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { WardrobeActions } from "../../../core/state/wardrobe/wardrobe.actions";
import {
  selectAllItems,
  selectWardrobeLoading,
} from "../../../core/state/wardrobe/wardrobe.selectors";
import { ClothingCardComponent } from "../../components/clothing-card/clothing-card.component";

@Component({
  selector: "app-wardrobe-dashboard",
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    ClothingCardComponent,
  ],
  template: `
    <div class="dashboard-container">
      <header class="dashboard-header">
        <h1>My Wardrobe</h1>
        <button mat-raised-button color="primary" routerLink="/wardrobe/add">
          <mat-icon>add</mat-icon> Add Item
        </button>
      </header>

      <div *ngIf="loading$ | async" class="loading-state">
        <mat-spinner diameter="40"></mat-spinner>
      </div>

      <div class="items-grid" *ngIf="!(loading$ | async)">
        <app-clothing-card
          *ngFor="let item of items$ | async"
          [item]="item"
          (delete)="onDelete(item.id!)"
        >
        </app-clothing-card>
      </div>

      <div
        *ngIf="!(loading$ | async) && (items$ | async)?.length === 0"
        class="empty-state"
      >
        <p>Your wardrobe is empty. Start adding some clothes!</p>
      </div>
    </div>
  `,
  styles: [
    `
      .dashboard-container {
        padding: 24px;
        max-width: 1200px;
        margin: 0 auto;
      }
      .dashboard-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 32px;
      }
      .items-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
        gap: 24px;
      }
      .loading-state,
      .empty-state {
        display: flex;
        justify-content: center;
        padding: 64px 0;
      }
    `,
  ],
})
export class WardrobeDashboardComponent implements OnInit {
  private store = inject(Store);

  items$ = this.store.select(selectAllItems);
  loading$ = this.store.select(selectWardrobeLoading);

  ngOnInit() {
    this.store.dispatch(WardrobeActions.loadClothingItems());
  }

  onDelete(id: string) {
    if (confirm("Are you sure you want to delete this item?")) {
      this.store.dispatch(WardrobeActions.deleteClothingItem({ id }));
    }
  }
}
```

### 3.2 Add Clothing Item Formulation (Task 88)

`ng generate component presentation/pages/add-clothing-item`

**`add-clothing-item.component.ts`**

```typescript
import { Component, inject } from "@angular/core";
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from "@angular/forms";
import { Store } from "@ngrx/store";
import { CommonModule } from "@angular/common";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatButtonModule } from "@angular/material/button";
import { Router, RouterModule } from "@angular/router";
import { WardrobeActions } from "../../../core/state/wardrobe/wardrobe.actions";

@Component({
  selector: "app-add-clothing-item",
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    RouterModule,
  ],
  template: `
    <div class="form-container">
      <h2>Add New Clothing Item</h2>
      <form [formGroup]="clothingForm" (ngSubmit)="onSubmit()">
        <mat-form-field appearance="fill" class="full-width">
          <mat-label>Item Name</mat-label>
          <input
            matInput
            formControlName="name"
            placeholder="e.g., Blue Denim Jacket"
          />
        </mat-form-field>

        <div class="row">
          <mat-form-field appearance="fill">
            <mat-label>Type</mat-label>
            <mat-select formControlName="clothingType">
              <mat-option value="Top">Top</mat-option>
              <mat-option value="Bottom">Bottom</mat-option>
              <mat-option value="Outerwear">Outerwear</mat-option>
              <mat-option value="Footwear">Footwear</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="fill">
            <mat-label>Brand</mat-label>
            <input matInput formControlName="brand" />
          </mat-form-field>
        </div>

        <mat-form-field appearance="fill" class="full-width">
          <mat-label>Primary Color (Hex)</mat-label>
          <input
            matInput
            formControlName="primaryColor"
            placeholder="#000000"
          />
        </mat-form-field>

        <div class="actions">
          <button mat-button type="button" routerLink="/wardrobe">
            Cancel
          </button>
          <button
            mat-raised-button
            color="primary"
            type="submit"
            [disabled]="clothingForm.invalid"
          >
            Save Item
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [
    `
      .form-container {
        max-width: 600px;
        margin: 40px auto;
        padding: 32px;
        background: white;
        border-radius: 12px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      }
      .full-width {
        width: 100%;
        margin-bottom: 16px;
      }
      .row {
        display: flex;
        gap: 16px;
        margin-bottom: 16px;
      }
      .row mat-form-field {
        flex: 1;
      }
      .actions {
        display: flex;
        justify-content: flex-end;
        gap: 16px;
        margin-top: 24px;
      }
    `,
  ],
})
export class AddClothingItemComponent {
  private fb = inject(FormBuilder);
  private store = inject(Store);
  private router = inject(Router);

  clothingForm: FormGroup = this.fb.group({
    name: ["", Validators.required],
    clothingType: ["", Validators.required],
    brand: [""],
    primaryColor: ["#000000", Validators.required],
  });

  onSubmit() {
    if (this.clothingForm.valid) {
      this.store.dispatch(
        WardrobeActions.createClothingItem({
          item: this.clothingForm.value,
        }),
      );
      // In a real app, you'd navigate only on Success Action via Effects or subscriptions
      this.router.navigate(["/wardrobe"]);
    }
  }
}
```

---

## 4. Routing Module Updates (Task 92)

**Path:** `src/outfit-planner-ui/src/app/app.routes.ts`

Make sure the routes are properly configured pointing to the standalone components.

```typescript
import { Routes } from "@angular/router";
// Import your auth guard
// import { AuthGuard } from './core/guards/auth.guard';

// Import components
import { WardrobeDashboardComponent } from "./presentation/pages/wardrobe-dashboard/wardrobe-dashboard.component";
import { AddClothingItemComponent } from "./presentation/pages/add-clothing-item/add-clothing-item.component";
// import { ClothingItemDetailComponent } from './presentation/pages/clothing-item-detail/clothing-item-detail.component';

export const routes: Routes = [
  // ... other routes ...
  {
    path: "wardrobe",
    component: WardrobeDashboardComponent,
    // canActivate: [AuthGuard] // Uncomment when Auth is ready
  },
  {
    path: "wardrobe/add",
    component: AddClothingItemComponent,
    // canActivate: [AuthGuard]
  },
  // {
  //   path: 'wardrobe/:id',
  //   component: ClothingItemDetailComponent,
  //   canActivate: [AuthGuard]
  // },
];
```

## Next Steps for the Developer

1. Copy-paste these files into their respective directories.
2. Run `ng serve` to ensure there are no compilation errors.
3. Import the `wardrobeReducer` and `WardrobeEffects` in your main `app.config.ts` or standalone main file to register them with the NgRx Store.
   _(e.g., `provideState('wardrobe', wardrobeReducer), provideEffects(WardrobeEffects)`)_.
