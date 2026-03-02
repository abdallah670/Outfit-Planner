import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-daily-pick',
  standalone: true,
  imports: [CommonModule, MatButtonModule],
  templateUrl: './daily-pick.component.html',
  styleUrls: ['./daily-pick.component.scss'],
})
export class DailyPickComponent {}
