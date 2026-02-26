# NgRx State Management — Full Guide for Outfit Planner

## Why Do We Need State Management?

Without state management, your Angular app scatters data across many components and services. This leads to:

| Problem                       | Example                                                                                                 |
| ----------------------------- | ------------------------------------------------------------------------------------------------------- |
| **Prop Drilling**             | Passing `user` data through 5 levels of components                                                      |
| **Inconsistent State**        | Login page shows "logged in" but navbar shows "logged out"                                              |
| **No Single Source of Truth** | `AuthService.currentUser`, `NavbarComponent.user`, and `ProfileComponent.user` all hold separate copies |
| **Hard to Debug**             | "Where did this value change?" — no history, no traceability                                            |

**NgRx solves all of this** by creating one central **Store** that every component reads from. Changes happen through a strict, predictable pipeline.

---

## The NgRx Architecture (How It All Fits Together)

```
┌──────────────────────────────────────────────────────────┐
│                    COMPONENT (UI)                        │
│                                                          │
│  ① User clicks "Login" button                           │
│  ② Component dispatches: store.dispatch(login({...}))   │
│  ⑥ Component reads state via selector                    │
│     selectIsAuthenticated → shows/hides navbar           │
└──────┬──────────────────────────────────────────▲────────┘
       │ dispatch(action)                         │ select(selector)
       ▼                                          │
┌──────────────┐    ┌──────────────┐    ┌────────┴─────────┐
│   ACTIONS    │───▶│   REDUCER    │───▶│     STORE        │
│              │    │              │    │                   │
│ "What        │    │ "How state   │    │ Single source    │
│  happened"   │    │  changes"    │    │ of truth         │
│              │    │              │    │                   │
│ login()      │    │ loading:true │    │ { auth: {        │
│ loginSuccess │    │ user: data   │    │   user: {...},   │
│ loginFailure │    │ error: msg   │    │   loading: false │
└──────┬───────┘    └──────────────┘    │   isAuth: true   │
       │                                │ }}               │
       ▼                                └──────────────────┘
┌──────────────┐
│   EFFECTS    │
│              │
│ "Side effects│
│  like API    │
│  calls"      │
│              │
│ ③ Listens for login action                              │
│ ④ Calls AuthService.login(email, password)              │
│ ⑤ Dispatches loginSuccess or loginFailure               │
└──────────────┘
```

### The Flow in Plain English

1. **User clicks Login** → Component dispatches `AuthActions.login({ request })`
2. **Effect catches it** → Calls `AuthService.login()` (HTTP request)
3. **API responds** → Effect dispatches `loginSuccess({ response })` or `loginFailure({ error })`
4. **Reducer updates state** → Sets `user`, `token`, `isAuthenticated`, `loading`
5. **Selectors notify components** → Navbar updates, router navigates, etc.

---

## The 5 Building Blocks — Explained with Our Code

### 1. Actions (`auth.actions.ts`) — "What Happened"

Actions are **events** that describe something that happened. They don't contain logic — just a name and optional data.

```typescript
// file: src/app/core/state/auth/auth.actions.ts

import { createActionGroup, emptyProps, props } from "@ngrx/store";

export const AuthActions = createActionGroup({
  source: "Auth", // Category name (shows in DevTools)
  events: {
    // Each event = one action
    Login: props<{ request: AuthRequest }>(), // carries login data
    "Login Success": props<{ response: AuthResponse }>(), // carries API response
    "Login Failure": props<{ error: string }>(), // carries error message
    Logout: emptyProps(), // no data needed
    "Refresh Token": emptyProps(),
    "Refresh Token Success": props<{ response: AuthResponse }>(),
    "Refresh Token Failure": props<{ error: string }>(),
  },
});
```

**Usage in a component:**

```typescript
// Dispatching an action
this.store.dispatch(AuthActions.login({ request: { email, password } }));
this.store.dispatch(AuthActions.logout());
```

> [!TIP]
> `createActionGroup` auto-generates action creators. `'Login Success'` becomes `AuthActions.loginSuccess()`.

---

### 2. Reducer (`auth.reducer.ts`) — "How State Changes"

The reducer is a **pure function** that takes the current state + an action and returns the **new state**. It never calls APIs or has side effects.

```typescript
// file: src/app/core/state/auth/auth.reducer.ts

// Step 1: Define the shape of this slice of state
export interface AuthState {
  user: Partial<User> | null;
  token: string | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: string | null;
}

// Step 2: Define the initial state (app start)
export const initialState: AuthState = {
  user: null,
  token: null,
  isAuthenticated: false,
  loading: false,
  error: null,
};

// Step 3: Define how each action changes the state
export const authReducer = createReducer(
  initialState,

  // When login is dispatched → set loading to true
  on(AuthActions.login, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  // When login succeeds → store user data
  on(AuthActions.loginSuccess, (state, { response }) => ({
    ...state,
    user: {
      id: response.id,
      email: response.email,
      username: response.userName,
    },
    token: response.token,
    isAuthenticated: true,
    loading: false,
    error: null,
  })),

  // When login fails → store error
  on(AuthActions.loginFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // When logout → reset everything
  on(AuthActions.logout, () => initialState),
);
```

**Key rules:**

- Reducers are **pure** — no API calls, no side effects
- Always return a **new object** (`{...state, ...changes}`) — never mutate `state` directly
- Every action that isn't handled is simply ignored (state passes through unchanged)

---

### 3. Effects (`auth.effects.ts`) — "Side Effects (API Calls)"

Effects listen for specific actions, perform async work (like HTTP calls), and dispatch new actions based on the result.

```typescript
// file: src/app/core/state/auth/auth.effects.ts

// NgRx v21 uses FUNCTIONAL effects (not class-based)
export const login$ = createEffect(
  // Dependencies injected as default parameters
  (actions$ = inject(Actions), authService = inject(AuthService)) => {
    return actions$.pipe(
      ofType(AuthActions.login), // ① Listen for login action
      exhaustMap(
        (
          { request }, // ② Take the request data
        ) =>
          authService.login(request).pipe(
            // ③ Call the API
            map(
              (response: AuthResponse) =>
                AuthActions.loginSuccess({ response }), // ④ Success → dispatch loginSuccess
            ),
            catchError((error) =>
              of(
                AuthActions.loginFailure({
                  // ⑤ Failure → dispatch loginFailure
                  error: error?.message || "Login failed",
                }),
              ),
            ),
          ),
      ),
    );
  },
  { functional: true }, // ← Required for functional effects
);
```

**Key concepts:**

- `ofType(...)` — Only react to specific actions
- `exhaustMap` — Wait for current API call to finish before accepting new ones (prevents duplicate requests)
- `catchError` — Handle errors without killing the effect stream
- `{ functional: true }` — Modern NgRx pattern (no class needed)
- `{ dispatch: false }` — For effects that DON'T dispatch a new action (like navigation)

---

### 4. Selectors (`auth.selectors.ts`) — "Read Data from the Store"

Selectors are **getter functions** that extract specific pieces of state. They are memoized (cached until state changes).

```typescript
// file: src/app/core/state/auth/auth.selectors.ts

// Select the entire auth slice
export const selectAuthState = createFeatureSelector<AuthState>("auth");

// Select individual properties from the auth slice
export const selectUser = createSelector(
  selectAuthState,
  (state) => state.user,
);

export const selectIsAuthenticated = createSelector(
  selectAuthState,
  (state) => state.isAuthenticated,
);

export const selectAuthLoading = createSelector(
  selectAuthState,
  (state) => state.loading,
);

export const selectAuthError = createSelector(
  selectAuthState,
  (state) => state.error,
);
```

**Usage in a component:**

```typescript
// Read state reactively
user$ = this.store.select(selectUser);
isAuthenticated$ = this.store.select(selectIsAuthenticated);

// In template with async pipe
// <div *ngIf="isAuthenticated$ | async">Welcome!</div>

// Or with signals (Angular 17+)
user = this.store.selectSignal(selectUser);
// In template: {{ user()?.username }}
```

> [!IMPORTANT]
> **Always use selectors** to read from the store. Never access `store.state` directly.

---

### 5. Store Registration (`app.config.ts`) — "Wiring It All Together"

```typescript
// file: src/app/app.config.ts

import { provideStore } from "@ngrx/store";
import { provideEffects } from "@ngrx/effects";
import { provideStoreDevtools } from "@ngrx/store-devtools";
import { authReducer } from "./core/state/auth/auth.reducer";
import * as authEffects from "./core/state/auth/auth.effects";

export const appConfig: ApplicationConfig = {
  providers: [
    provideStore({
      auth: authReducer, // Register the auth slice
      // wardrobe: wardrobeReducer, // Future slices go here
    }),
    provideEffects(authEffects), // Register functional effects
    provideStoreDevtools({
      // Dev tools (Redux DevTools browser extension)
      maxAge: 25,
      logOnly: !isDevMode(),
    }),
  ],
};
```

---

## How to Use in a Component (Full Example)

```typescript
import { Component, inject } from "@angular/core";
import { Store } from "@ngrx/store";
import { AuthActions } from "../core/state/auth/auth.actions";
import {
  selectIsAuthenticated,
  selectAuthLoading,
  selectAuthError,
} from "../core/state/auth/auth.selectors";

@Component({
  selector: "app-login",
  template: `
    <form (ngSubmit)="onLogin()">
      <input [(ngModel)]="email" placeholder="Email" />
      <input [(ngModel)]="password" type="password" placeholder="Password" />

      <!-- Show loading spinner while API call is in progress -->
      @if (loading()) {
        <div class="spinner">Logging in...</div>
      }

      <!-- Show error message if login failed -->
      @if (error()) {
        <div class="error">{{ error() }}</div>
      }

      <button type="submit" [disabled]="loading()">Login</button>
    </form>
  `,
})
export class LoginComponent {
  private readonly store = inject(Store);

  // Read state using signals (modern Angular)
  loading = this.store.selectSignal(selectAuthLoading);
  error = this.store.selectSignal(selectAuthError);

  email = "";
  password = "";

  onLogin() {
    // Dispatch action — the effect will handle the API call
    this.store.dispatch(
      AuthActions.login({
        request: { email: this.email, password: this.password },
      }),
    );
    // That's it! No subscribe(), no manual state management.
    // The effect calls the API, the reducer updates state,
    // and the selectors automatically update the template.
  }
}
```

---

## Why NgRx Over Plain Services?

| Feature                | Plain Service (Signals)          | NgRx Store                                    |
| ---------------------- | -------------------------------- | --------------------------------------------- |
| **Small apps**         | ✅ Simple, fast                  | ❌ Overkill                                   |
| **Debugging**          | ❌ No history                    | ✅ Time-travel debugging with Redux DevTools  |
| **Predictability**     | ⚠️ State can change anywhere     | ✅ State only changes through reducers        |
| **Testing**            | ⚠️ Mock entire services          | ✅ Test actions, reducers, effects separately |
| **Complex state**      | ❌ Gets messy with many services | ✅ Organized by feature slices                |
| **Team collaboration** | ⚠️ Everyone's own pattern        | ✅ Enforced, consistent architecture          |

> [!NOTE]
> For the Outfit Planner, we use NgRx because we'll have multiple interconnected state slices (Auth, Wardrobe, Outfits, Social, Weather) that need to be predictable and debuggable.

---

## File Structure for State Management

```
src/app/core/state/
├── auth/
│   ├── auth.actions.ts     ← What happened (events)
│   ├── auth.reducer.ts     ← How state changes (pure logic)
│   ├── auth.effects.ts     ← Side effects (API calls)
│   └── auth.selectors.ts   ← Read state (getters)
├── wardrobe/               ← Future: same pattern
│   ├── wardrobe.actions.ts
│   ├── wardrobe.reducer.ts
│   ├── wardrobe.effects.ts
│   └── wardrobe.selectors.ts
└── outfit/                 ← Future: same pattern
    └── ...
```

Each feature gets its own folder with the same 4 files. This pattern scales cleanly.

---

## Quick Reference: Adding a New State Slice

When you need to add state management for a new feature (e.g., Wardrobe):

1. **Create actions** — Define what events can happen
2. **Create reducer** — Define state shape and how each action changes it
3. **Create effects** — Handle API calls and side effects
4. **Create selectors** — Define how components read state
5. **Register in `app.config.ts`** — Add reducer to `provideStore()` and effects to `provideEffects()`
