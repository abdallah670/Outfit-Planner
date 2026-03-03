import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatBadgeModule } from '@angular/material/badge';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { UserProfile } from '../../../../domain/entities/user-profile.entity';
import { selectUserProfile } from '../../../../core/state/user/user.selectors';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatBadgeModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss'],
})
export class NavbarComponent implements OnInit {
  profile$: Observable<UserProfile | null>;

  constructor(private store: Store) {
    this.profile$ = this.store.select(selectUserProfile);
  }

  ngOnInit() {}
}
