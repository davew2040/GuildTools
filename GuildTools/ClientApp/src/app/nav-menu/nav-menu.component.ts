import { Component } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';
import { RoutePaths } from 'app/data/route-paths';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;

  constructor(
    public auth: AuthService,
    public router: Router) {

  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  logout() {
    this.auth.logOut();
  }

  navigateToLogin() {
    this.router.navigate(['/' + RoutePaths.Login]);
  }

  navigateToRegister() {
    this.router.navigate(['/' + RoutePaths.Register]);
  }

  navigateToUserSettings() {
    this.router.navigate(['/' + RoutePaths.UserSettings]);
  }
}
