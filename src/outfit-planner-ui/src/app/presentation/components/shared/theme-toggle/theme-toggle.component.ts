import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  template: `
    <button mat-icon-button 
            class="theme-toggle-btn" 
            (click)="toggleTheme()" 
            [attr.aria-label]="isDarkMode ? 'Switch to light mode' : 'Switch to dark mode'"
            style="border: none; cursor: pointer; color: inherit;">
      <mat-icon>{{ isDarkMode ? 'light_mode' : 'dark_mode' }}</mat-icon>
    </button>
  `,
  styles: [`
    .theme-toggle-btn {
      display: flex;
      align-items: center;
      justify-content: center;
    }
  `]
})
export class ThemeToggleComponent {
  isDarkMode = false;

  constructor() {
    this.isDarkMode = localStorage.getItem('theme') === 'dark' || 
      (!localStorage.getItem('theme') && window.matchMedia('(prefers-color-scheme: dark)').matches);
    this.applyTheme();
  }

  toggleTheme(): void {
    this.isDarkMode = !this.isDarkMode;
    localStorage.setItem('theme', this.isDarkMode ? 'dark' : 'light');
    this.applyTheme();
  }

  private applyTheme(): void {
    document.body.classList.toggle('dark-theme', this.isDarkMode);
  }
}