import { Component } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';
import { RoutePaths } from 'app/data/route-paths';
import { NavService, NavServiceLocation } from 'app/services/nav-service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
})
export class NavMenuComponent {
  isExpanded = false;

  constructor(
    public auth: AuthService,
    public navService: NavService,
    public router: Router) {

  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  logout() {
    this.navService.currentLocation = NavServiceLocation.None;
    this.auth.logOut();
    this.router.navigate(['/']);
  }

  navigateToLogin() {
    this.navService.currentLocation = NavServiceLocation.None;
    this.router.navigate(['/' + RoutePaths.Login]);
  }

  navigateToRegister() {
    this.navService.currentLocation = NavServiceLocation.None;
    this.router.navigate(['/' + RoutePaths.Register]);
  }

  navigateToUserSettings() {
    this.navService.currentLocation = NavServiceLocation.None;
    this.router.navigate(['/' + RoutePaths.UserSettings]);
  }

  navigateToAltTracker() {
    this.navService.currentLocation = NavServiceLocation.AltTracker;
    this.router.navigate(['/' + RoutePaths.AltTracker]);
  }

  navigateToStats() {
    this.navService.currentLocation = NavServiceLocation.Stats;
    this.router.navigate(['/' + RoutePaths.GuildStats]);
  }

  public getNavButtonClass(el: any) {
    const classes = [];

    const id = el.id;

    if (id === 'link-alt-tracker' && this.navService.currentLocation === NavServiceLocation.AltTracker) {
      classes.push('link-element-selected');
    }
    else if (id === 'link-stats' && this.navService.currentLocation === NavServiceLocation.Stats) {
      classes.push('link-element-selected');
    }

    return classes;
  }
}
