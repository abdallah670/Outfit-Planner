import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatBadgeModule } from '@angular/material/badge';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatBadgeModule],
  template: `
    <nav class="navbar glass-card">
      <div class="nav-left">
        <div class="logo" routerLink="/">
          <i class="fa-solid fa-shirt logo-icon"></i>
          <span class="logo-text">Outfit Planner</span>
        </div>
      </div>

      <div class="nav-center">
        <a routerLink="/wardrobe" routerLinkActive="active" class="nav-link">My Wardrobe</a>
        <a routerLink="/outfits" routerLinkActive="active" class="nav-link">Plan Outfits</a>
        <a routerLink="/calendar" routerLinkActive="active" class="nav-link">Calendar</a>
        <a routerLink="/social" routerLinkActive="active" class="nav-link">Social</a>
        <a routerLink="/settings" routerLinkActive="active" class="nav-link">Settings</a>
      </div>

      <div class="nav-right">
        <button mat-icon-button class="nav-icon-btn">
          <i class="fa-solid fa-magnifying-glass"></i>
        </button>
        <button mat-icon-button class="nav-icon-btn" matBadge="2" matBadgeColor="warn">
          <i class="fa-regular fa-bell"></i>
        </button>
        <div class="user-profile">
          <img src="assets/avatar-placeholder.png" alt="User Avatar" class="avatar" />
        </div>
      </div>
    </nav>
  `,
  styles: [
    `
      .navbar {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 0.75rem 2rem;
        margin: 1rem 2rem;
        border-radius: var(--radius-lg);
        backdrop-filter: blur(12px);
        background: var(--glass-bg);
        border: 1px solid var(--glass-border);
        position: sticky;
        top: 1rem;
        z-index: 1000;
      }

      .nav-left .logo {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        cursor: pointer;

        .logo-icon {
          color: var(--accent-primary);
          font-size: 1.5rem;
          display: flex;
          align-items: center;
          justify-content: center;
        }

        .logo-text {
          font-size: 1.25rem;
          font-weight: 700;
          letter-spacing: -0.02em;
          background: linear-gradient(to right, var(--text-primary), var(--text-secondary));
          -webkit-background-clip: text;
          -webkit-text-fill-color: transparent;
        }
      }

      .nav-center {
        display: flex;
        gap: 2rem;

        .nav-link {
          text-decoration: none;
          color: var(--text-secondary);
          font-weight: 500;
          font-size: 0.95rem;
          transition: all 0.3s ease;
          position: relative;

          &:hover,
          &.active {
            color: var(--text-primary);
          }

          &.active::after {
            content: '';
            position: absolute;
            bottom: -4px;
            left: 0;
            width: 100%;
            height: 2px;
            background: var(--accent-primary);
            border-radius: 2px;
          }
        }
      }

      .nav-right {
        display: flex;
        align-items: center;
        gap: 0.5rem;

        .nav-icon-btn {
          color: var(--text-secondary);
          font-size: 1.1rem;
          &:hover {
            color: var(--text-primary);
            background: rgba(255, 255, 255, 0.05);
          }
        }

        .user-profile {
          margin-left: 0.5rem;
          cursor: pointer;

          .avatar {
            width: 36px;
            height: 36px;
            border-radius: 50%;
            border: 2px solid var(--glass-border);
            object-fit: cover;
          }
        }
      }
    `,
  ],
})
export class NavbarComponent {}
