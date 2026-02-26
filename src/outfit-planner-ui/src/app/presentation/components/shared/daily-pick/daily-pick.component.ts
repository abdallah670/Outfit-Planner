import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-daily-pick',
  standalone: true,
  imports: [CommonModule, MatButtonModule],
  template: `
    <div class="daily-pick-widget glass-card">
      <div class="header">
        <h3 class="title">Daily Pick</h3>
        <span class="match-badge">94% Match</span>
      </div>
      <p class="description">Perfect for a mild afternoon walk.</p>

      <div class="items-list">
        <div class="pick-item">
          <div class="item-img-placeholder">
            <img src="assets/pick-shirt.png" alt="Shirt" />
          </div>
          <div class="item-info">
            <span class="item-name">Beige Linen Shirt</span>
            <span class="item-desc">Lightweight • Casual</span>
          </div>
        </div>

        <div class="pick-item">
          <div class="item-img-placeholder">
            <img src="assets/pick-chinos.png" alt="Chinos" />
          </div>
          <div class="item-info">
            <span class="item-name">Navy Chinos</span>
            <span class="item-desc">Slim Fit • Cotton</span>
          </div>
        </div>

        <div class="pick-item">
          <div class="item-img-placeholder">
            <img src="assets/pick-loafers.png" alt="Loafers" />
          </div>
          <div class="item-info">
            <span class="item-name">White Loafers</span>
            <span class="item-desc">Leather • Comfortable</span>
          </div>
        </div>
      </div>

      <button mat-flat-button class="wear-btn">Wear This Today</button>
    </div>
  `,
  styles: [
    `
      .daily-pick-widget {
        padding: 1.5rem;
        background: var(--glass-bg);
        border: 1px solid var(--glass-border);
        border-radius: var(--radius-lg);
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }

      .header {
        display: flex;
        justify-content: space-between;
        align-items: center;

        .title {
          margin: 0;
          font-size: 1.1rem;
          font-weight: 600;
        }

        .match-badge {
          font-size: 0.75rem;
          font-weight: 700;
          padding: 0.2rem 0.6rem;
          background: linear-gradient(to right, #f59e0b, #fbbf24);
          color: #78350f;
          border-radius: 12px;
        }
      }

      .description {
        font-size: 0.9rem;
        color: var(--text-secondary);
        margin: 0;
      }

      .items-list {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
        margin: 0.5rem 0;
      }

      .pick-item {
        display: flex;
        align-items: center;
        gap: 1rem;
        padding: 0.6rem;
        background: rgba(255, 255, 255, 0.03);
        border-radius: 12px;

        .item-img-placeholder {
          width: 48px;
          height: 48px;
          background: rgba(255, 255, 255, 0.05);
          border-radius: 8px;
          overflow: hidden;

          img {
            width: 100%;
            height: 100%;
            object-fit: cover;
          }
        }

        .item-info {
          display: flex;
          flex-direction: column;

          .item-name {
            font-size: 0.9rem;
            font-weight: 500;
          }

          .item-desc {
            font-size: 0.75rem;
            color: var(--text-dim);
          }
        }
      }

      .wear-btn {
        width: 100%;
        background: #a3b18a;
        color: #344e41;
        font-weight: 600;
        border-radius: 12px;
        padding: 0.6rem;
        margin-top: 0.5rem;

        &:hover {
          background: #dad7cd;
        }
      }
    `,
  ],
})
export class DailyPickComponent {}
