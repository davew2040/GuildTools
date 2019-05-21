import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router'
import { DataService } from '../../services/data-services';
import { BlizzardService } from '../../blizzard-services/blizzard-services';
import { AuthService } from '../../auth/auth.service';
import { BusyService } from '../../shared-services/busy-service';
import { GuildProfileSlim } from '../../services/ServiceTypes/service-types';
import { RoutePaths } from 'app/data/route-paths';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-my-guild-profiles',
  templateUrl: './my-guild-profiles.component.html',
  styleUrls: ['./my-guild-profiles.component.css']
})
export class MyGuildProfilesComponent implements OnInit {

  private isLoaded = false;
  public tableDataSource = new MatTableDataSource<GuildProfileSlim>();
  public myGuildProfiles = Array<GuildProfileSlim>();
  public displayedColumns: Array<string> = ['profileName', 'guildName', 'realmName', 'delete'];

  constructor(
    public route: ActivatedRoute,
    public dataService: DataService,
    public blizzardService: BlizzardService,
    public authService: AuthService,
    private busyService: BusyService,
    private errorService: ErrorReportingService,
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
          this.tableDataSource.data = this.myGuildProfiles;
          this.busyService.unsetBusy();
        },
        error => {
          this.busyService.unsetBusy();
          this.errorService.reportApiError(error);
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
    if (!foundProfile) {
      return '';
    }

    return BlizzardService.GetGuildUrl(foundProfile.primaryGuildName, foundProfile.realmName, foundProfile.regionName);
  }

  public navigateToProfile(profileId: number): boolean {
    const path = `/${RoutePaths.ViewProfile}/${profileId}`;
    this.router.navigate([path]);
    return false;
  }

  public onMouseEnter(event: any): void {
    const targetDiv = event.currentTarget.querySelector('.delete-profile-a');
    targetDiv.classList.remove('hidden');
  }

  public onMouseLeave(event: any): void {
    const targetDiv = event.currentTarget.querySelector('.delete-profile-a');
    targetDiv.classList.add('hidden');
  }

  public getDeleteCellClasses(): Array<string> {
    return ['hidden'];
  }

  public deleteProfile(profile: GuildProfileSlim) {
    this.busyService.setBusy();

    this.dataService.deleteProfile(profile).subscribe(
      success => {
        this.busyService.unsetBusy();
        const deleteIndex = this.myGuildProfiles.findIndex(p => p.id === profile.id);
        this.myGuildProfiles.splice(deleteIndex, 1);
        this.tableDataSource.data = this.myGuildProfiles;
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      }
    )
  }
}
