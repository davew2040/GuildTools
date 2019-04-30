import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router'
import { DataService } from '../../services/data-services';
import { BlizzardService } from '../../blizzard-services/blizzard-services';
import { AuthService } from '../../auth/auth.service';
import { BusyService } from '../../shared-services/busy-service';
import {  MatDialog } from '@angular/material';
import { GuildProfile } from '../../services/ServiceTypes/service-types';
import { RoutePaths } from 'app/data/route-paths';

@Component({
  selector: 'app-my-guild-profiles',
  templateUrl: './my-guild-profiles.component.html',
  styleUrls: ['./my-guild-profiles.component.css']
})
export class MyGuildProfilesComponent implements OnInit {

  private isLoaded = false;
  public myGuildProfiles = Array<GuildProfile>();
  public displayedColumns: Array<string> = ['profileName', 'guildName', 'realmName'];

  constructor(
    public route: ActivatedRoute,
    public dataService: DataService,
    public blizzardService: BlizzardService,
    public authService: AuthService,
    private busyService: BusyService,
    private router: Router) {
      this.myGuildProfiles = [];
    }

  ngOnInit() {
    if (!this.authService.isAuthenticated) {
      this.isLoaded = true;
      return;
    }

    this.busyService.setBusy();

    this.dataService.getGuildProfilesForUser()
      .subscribe(
        success => {
          this.myGuildProfiles = success;
          this.busyService.unsetBusy();
        },
        error => {
          console.log(error);
          this.busyService.unsetBusy();
        });
  }

  public navigateToLogin(): void {
    this.router.navigate(['/' + RoutePaths.Login]);
  }

  public addProfile(): void {
    this.router.navigate(['/' + RoutePaths.NewProfile]);
  }

  public getGuildUrl(profileId: number): string {
    const foundProfile = this.myGuildProfiles.find(profile => profile.id === profileId);
    if (!foundProfile){
      return '';
    }

    return BlizzardService.GetGuildUrl(foundProfile.guildName, foundProfile.realm, foundProfile.region);
  }

  public navigateToProfile(profileId: number): boolean {
    const path = `/${RoutePaths.ViewProfile}/${profileId}`;
    this.router.navigate([path]);
    return false;
  }
}
