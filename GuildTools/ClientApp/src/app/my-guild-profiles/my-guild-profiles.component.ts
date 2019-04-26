import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router'
import { DataService } from '../services/data-services';
import { BlizzardService } from '../blizzard-services/blizzard-services';
import { GuildProfile } from '../services/Models/GuildProfile';
import { GuildMember } from '../services/Models/GuildMember';
import { AuthService } from '../auth/auth.service';
import { BusyService } from '../shared-services/busy-service';
import { MatDialogRef, MatDialog } from '@angular/material';
import { NewProfileDialogComponent } from '../dialogs/new-profile-dialog-component/new-profile-dialog';

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

    this.dataService.getMyGuildProfiles()
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
    let dialogRef = this.dialog.open(NewProfileDialogComponent, {
      disableClose: true,
      width: '600px',
      height: '500px'
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      let test = result;
    });
  }
}
