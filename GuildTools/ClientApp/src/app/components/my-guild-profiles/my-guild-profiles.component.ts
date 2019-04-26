import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router'
import { DataService } from '../../services/data-services';
import { BlizzardService } from '../../blizzard-services/blizzard-services';
import { AuthService } from '../../auth/auth.service';
import { BusyService } from '../../shared-services/busy-service';
import { MatDialogRef, MatDialog } from '@angular/material';
import { FindGuildDialogComponent } from '../../dialogs/find-guild-dialog.component/find-guild-dialog.component';
import { GuildProfile } from '../../services/ServiceTypes/service-types';

@Component({
  selector: 'app-my-guild-profiles',
  templateUrl: './my-guild-profiles.component.html',
  styleUrls: ['./my-guild-profiles.component.css']
})
export class MyGuildProfilesComponent implements OnInit {

  private isLoaded = false;
  private myGuildProfiles = Array<GuildProfile>();

  constructor(
    public route: ActivatedRoute,
    public dataService: DataService,
    public blizzardService: BlizzardService,
    public authService: AuthService,
    private busyService: BusyService,
    private dialog: MatDialog,
    private router: Router) { }

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
    this.router.navigate(['/login']);
  }

  public addProfile(): void {
    const dialogRef = this.dialog.open(FindGuildDialogComponent, {
      disableClose: true,
      width: '600px'
    });

    dialogRef.afterClosed().subscribe(result => {

    });
  }
}
