import { Component, OnInit } from '@angular/core';
import { PendingAccessRequest, ProfilePermissionByUser, UpdatePermissionSet, UpdatePermission, FriendGuild, StoredGuild } from 'app/services/ServiceTypes/service-types';
import { ActivatedRoute, Router } from '@angular/router';
import { DataService } from 'app/services/data-services';
import { BusyService } from 'app/shared-services/busy-service';
import { MatTableDataSource } from '@angular/material/table';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { NotificationService } from 'app/shared-services/notification-service';
import { RoutePaths } from 'app/data/route-paths';
import { GuildProfilePermissionLevel } from 'app/auth/auth.service';
import { PermissionsOrder } from 'app/permissions/permissions-order';
import { MatDialog } from '@angular/material';
import { FindGuildDialogComponent } from 'app/dialogs/find-guild-dialog.component/find-guild-dialog.component';
import { SelectedGuild } from 'app/models/selected-guild';

class PermissionLabelPair {
  public label: string;
  public level: GuildProfilePermissionLevel;
}

class PermissionByUserViewModel {
  markForDelete: boolean;
  permission: ProfilePermissionByUser;
}

class SnapshotRowEntry {
  userId: string;
  level: GuildProfilePermissionLevel;
  markedForDelete: boolean;
}

class PermissionsValuesSnapshot {
  values: Array<SnapshotRowEntry>;
}

@Component({
  selector: 'app-manage-permissions',
  templateUrl: './manage-permissions.component.html',
  styleUrls: ['./manage-permissions.component.scss']
})
export class ManagePermissionsComponent implements OnInit {

  public pendingAccessRequests = new Array<PendingAccessRequest>();
  public accessRequestsTableDataSource = new MatTableDataSource<PendingAccessRequest>();
  public accessRequestsPermissionsColumns: Array<string> = ['name', 'email',  'createdOn', 'approve'];

  public profilePermissionsByUser = new Array<PermissionByUserViewModel>();
  public profilePermissionsTableDataSource = new MatTableDataSource<PermissionByUserViewModel>();
  public profilePermissionsColumns: Array<string> = ['name', 'permission', 'remove'];

  public orderedPermissions: Array<PermissionLabelPair>;
  public availablePermissions: Array<PermissionLabelPair>;

  public friendGuilds = new Array<FriendGuild>();

  private profileId: number;
  private activeProfilePermission: GuildProfilePermissionLevel;
  private snapshot: PermissionsValuesSnapshot;

  constructor(
      private route: ActivatedRoute,
      private router: Router,
      private dataService: DataService,
      private busyService: BusyService,
      private errorService: ErrorReportingService,
      private notificationService: NotificationService,
      private dialog: MatDialog) {
    this.orderedPermissions = this.getOrderedPermissions();
  }

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.updateRouteParams(params);
    });
  }

  private getOrderedPermissions(): Array<PermissionLabelPair> {
    const orderedPermissions = PermissionsOrder.GetOrderedPermissions();

    return orderedPermissions.map(
      p => {
        const pair = {} as PermissionLabelPair;

        pair.level = p;
        pair.label = PermissionsOrder.GetPermissionLabel(pair.level);

        return pair;
      })
      .reverse();
  }

  private getAvailablePermissions(): Array<PermissionLabelPair> {
    // TODO - Figure out how to restrict selection of invalid permissions
    return this.orderedPermissions;
  }

  private updateRouteParams(params: any): void {
    this.profileId = params['id'];

    this.getAccessRequests();
    this.getPermissions();
    this.getFriendGuilds();
  }

  public onMouseEnter(event: any): void {
    const targetDiv = event.currentTarget.querySelector('.show-hide-column');
    targetDiv.classList.remove('hidden');
  }

  public onMouseLeave(event: any): void {
    const targetDiv = event.currentTarget.querySelector('.show-hide-column');
    targetDiv.classList.add('hidden');
  }

  public approveRequest(request: PendingAccessRequest) {
    this.busyService.setBusy();

    this.dataService.approveProfileAccessRequest(request.id).subscribe(
      success => {
        this.busyService.unsetBusy();
        const deleteIndex = this.pendingAccessRequests.findIndex(p => p.id === request.id);
        this.pendingAccessRequests.splice(deleteIndex, 1);

        this.accessRequestsTableDataSource.data = this.pendingAccessRequests;

        this.notificationService.showNotification('Successfully approved request.');

        this.getPermissions();
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      }
    );
  }

  public savePermissions(): void {
    const updateSet = this.buildUpdatePermissionsSet();

    if (updateSet.updates.length === 0) {
      this.notificationService.showNotification('No updates present!');
      return;
    }

    this.busyService.setBusy();

    this.dataService.updatePermissions(updateSet).subscribe(
      success => {
        this.busyService.unsetBusy();
        this.notificationService.showNotification('Saved permissions successfully!');
        this.getPermissions();
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      });
  }

  private buildUpdatePermissionsSet(): UpdatePermissionSet {
    const updateSet = new UpdatePermissionSet();

    const comparisonSnapshot = this.getPermissionsSnapshot(this.profilePermissionsByUser);

    if (comparisonSnapshot.values.length !== this.snapshot.values.length){
      console.error('Snapshot mismatch!');
      return;
    }

    const updates = new Array<UpdatePermission>();

    for (let i = 0; i < comparisonSnapshot.values.length; i++) {
      const baseValue = this.snapshot.values[i];
      const newValue = comparisonSnapshot.values[i];

      if (newValue.markedForDelete) {
        const newUpdate = new UpdatePermission();

        newUpdate.delete = true;
        newUpdate.userId = newValue.userId;

        updates.push(newUpdate);
      }
      else {
        if (newValue.level !== baseValue.level) {
          const newUpdate = new UpdatePermission();

          newUpdate.newPermissionLevel = newValue.level;
          newUpdate.userId = newValue.userId;

          updates.push(newUpdate);
        }
      }
    }

    updateSet.updates = updates;
    updateSet.profileId = this.profileId;

    return updateSet;
  }

  public navigateToProfile(): void {
    this.router.navigate([`/${RoutePaths.ViewProfile}/${this.profileId}`]);
  }

  public getFormattedDate(input: string): string {
    const date = new Date(input);

    return `${date.getMonth()}/${date.getDate()}/${date.getFullYear()} ${date.getHours()}:${date.getMinutes()}`;
  }

  public getFormattedRealm(guild: StoredGuild): string {
    return `${guild.realm.name} (${guild.realm.region.regionName})`;
  }

  public getUserDescription(permission: ProfilePermissionByUser): string {
    return `${permission.user.username} (${permission.user.email})`;
  }

  public changeMarkForDelete(event: any, vm: PermissionByUserViewModel) {
    vm.markForDelete = event.checked;
  }

  public addFriendGuild(): void {
    const dialogRef = this.dialog.open(FindGuildDialogComponent, {
      disableClose: true,
      width: '600px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (!result) {
        return;
      }

      this.busyService.setBusy();
      const selectedGuild = result as SelectedGuild;

      this.dataService.addFriendGuild(this.profileId, selectedGuild.region.Name, selectedGuild.realm, selectedGuild.name).subscribe(
        success => {
          this.busyService.unsetBusy();
          this.friendGuilds.push(success);
        },
        error => {
          this.busyService.unsetBusy();
          this.errorService.reportApiError(error);
        }
      );
    });
  }

  public deleteFriendGuild(friendGuild: FriendGuild): void {
    this.busyService.setBusy();

    this.dataService.deleteFriendGuild(this.profileId, friendGuild.id).subscribe(
      success => {
        this.busyService.unsetBusy();

        const deleteIndex = this.friendGuilds.findIndex(p => p.id === friendGuild.id);
        this.friendGuilds.splice(deleteIndex, 1);

        this.notificationService.showNotification(`Removed friend guild '${friendGuild.guild.name}'.`);

      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      });
  }

  private getAccessRequests(): void {
    this.busyService.setBusy();

    this.dataService.getAccessRequests(this.profileId).subscribe(
      success => {
        this.busyService.unsetBusy();

        this.pendingAccessRequests = success;
        this.accessRequestsTableDataSource.data = this.pendingAccessRequests;
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      }
    );
  }

  private getPermissions(): void {
    this.busyService.setBusy();

    this.dataService.getAllProfilePermissions(this.profileId).subscribe(
      success => {
        this.busyService.unsetBusy();

        this.activeProfilePermission = success.currentPermissions;
        this.availablePermissions = this.getAvailablePermissions();

        this.profilePermissionsByUser = this.sortPermissions(success.permissions)
          .map(p => {
            const vm = new PermissionByUserViewModel();

            vm.markForDelete = false;
            vm.permission = p;

            return vm;
          });

        this.profilePermissionsTableDataSource.data = this.profilePermissionsByUser;
        this.snapshot = this.getPermissionsSnapshot(this.profilePermissionsByUser);
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      }
    );
  }

  private getFriendGuilds(): void {
    this.busyService.setBusy();

    this.dataService.getFriendGuilds(this.profileId).subscribe(
      success => {
        this.busyService.unsetBusy();

        this.friendGuilds = success;
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      }
    );
  }

  private sortPermissions(permissions: Array<ProfilePermissionByUser>): Array<ProfilePermissionByUser> {
    return permissions.sort((a, b) => {
      const aName = a.user.username;
      const bName = b.user.username;
      if (aName < bName) {
        return 1;
      }
      else if (aName > bName){
        return -1;
      }
      return 0;
    });
  }

  private getPermissionsSnapshot(permissions: Array<PermissionByUserViewModel>): PermissionsValuesSnapshot {
    const snapshot = new PermissionsValuesSnapshot();

    snapshot.values = permissions.map(p => {
      const entry = new SnapshotRowEntry();

      entry.level = p.permission.permissionLevel;
      entry.markedForDelete = p.markForDelete;
      entry.userId = p.permission.user.id;

      return entry;
    });

    return snapshot;
  }
}
