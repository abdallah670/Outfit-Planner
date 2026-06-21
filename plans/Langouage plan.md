# Application Translation (i18n) Implementation Plan

This plan outlines the steps required to fully implement translations (English, Arabic, French, and German) in the Angular application using the official `@angular/localize` package.

## User Review Required

> [!WARNING]
> **Development Server Limitation:** Angular's default development server (`ng serve`) can only serve **one language at a time**. Switching languages via the UI during local development will result in a 404 error unless we build the application or run a specific language configuration (e.g., `ng serve --configuration=ar`). 

> [!IMPORTANT]
> **Translation Effort:** Translating an entire app is a huge effort. For this implementation, I will translate the **core navigation and settings pages** as a proof-of-concept so you can see it working immediately. The rest of the pages will have the translation keys generated, but will fall back to English until a human translator fills in the `messages.xlf` files.

## Open Questions

> [!IMPORTANT]
> 1. Are you okay with me translating just the core Navbar, Sidebar, and Settings pages as a proof-of-concept for now?
> 2. For local development, I will add package.json scripts like `npm run start:ar` to serve the Arabic version. Is this acceptable for your workflow?

## Proposed Changes

---

### 1. Project Configuration (`angular.json`)

We need to configure Angular to recognize the new locales and their translation files.

#### [MODIFY] `src/outfit-planner-ui/angular.json`
- Add an `i18n` block to the project root defining `sourceLocale: "en"` and `locales` for `ar`, `fr`, and `de`.
- Add build configurations for each language (`"ar": { "localize": ["ar"] }`).
- Add serve configurations so we can test individual languages using `ng serve --configuration=ar`.

#### [MODIFY] `src/outfit-planner-ui/package.json`
- Add convenience scripts: `"start:ar": "ng serve --configuration=ar"`, `"start:fr": "ng serve --configuration=fr"`, etc.

---

### 2. Template Marking

We must tell Angular which text needs to be translated.

#### [MODIFY] Core Components
- `navbar.component.html`
- `settings.component.html`
- `app.component.html` (if any text exists)
- Add the `i18n` attribute to all text-containing tags (e.g., `<a i18n>Settings</a>`).

---

### 3. Translation Extraction and Files

We will generate the translation dictionaries.

#### [NEW] `src/outfit-planner-ui/src/locale/messages.xlf`
- Run `ng extract-i18n` to generate the master English extraction file.

#### [NEW] `src/outfit-planner-ui/src/locale/messages.ar.xlf`
- Copy the master file and provide Arabic translations for the core navigation items.

#### [NEW] `src/outfit-planner-ui/src/locale/messages.fr.xlf`
- Copy the master file and provide French translations for the core navigation items.

#### [NEW] `src/outfit-planner-ui/src/locale/messages.de.xlf`
- Copy the master file and provide German translations for the core navigation items.

---

## Verification Plan

### Automated Build Verification
- Run `npm run build` to ensure the Angular compiler successfully generates `dist/outfit-planner-ui/browser/en`, `/ar`, `/fr`, and `/de` folders without syntax errors.

### Manual Verification
- Run `npm run start:ar` and verify that the application launches in RTL (Right-to-Left) mode and the navigation links appear in Arabic.
- Run `npm run start:fr` and verify the text appears in French.

---

*Note: The remaining Backend Testing tasks from the previous plan are temporarily paused and will resume once this translation framework is established.*
