import { Component, OnInit, ViewChild, ViewChildren, AfterViewChecked, QueryList, ElementRef, ViewContainerRef, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FullGuildProfile, BlizzardPlayer, PlayerMain, PlayerAlt, StoredPlayer } from 'app/services/ServiceTypes/service-types';
import { DataService } from 'app/services/data-services';
import { BusyService } from 'app/shared-services/busy-service';
import { WowService, WowClass } from 'app/services/wow-service';
import { ContextMenuComponent } from 'ngx-contextmenu';
import { Observable, Subject, combineLatest } from 'rxjs';
import { startWith, map, filter } from 'rxjs/operators';
import { DropEvent } from 'ng-drag-drop';
import { MatExpansionPanel, MatDialog } from '@angular/material';
import {
  RemoveAltEvent,
  RemoveMainEvent,
  EditPlayerNotesEvent,
  EditOfficerNotesEvent,
  PromoteAltToMainEvent } from '../view-main/view-main.component';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { PermissionsOrder } from 'app/permissions/permissions-order';
import { GuildProfilePermissionLevel, AuthService } from 'app/auth/auth.service';
import { NotificationService } from 'app/shared-services/notification-service';
import { RoutePaths } from 'app/data/route-paths';
import { ConfirmationDialogComponent } from 'app/dialogs/confirmation-dialog.component/confirmation-dialog.component';
import { ShareLinkDialogData, ShareLinkDialogComponent } from 'app/dialogs/share-link-dialog.component/share-link-dialog.component';

class DropScopes {
  public DropMain = 'DropMain';
  public DropAlt = 'DropAlt';
}

class PlayerTableColumnLabels {
  public static get PlayerName() { return 'playerName'; }
  public static get PlayerGuild() { return 'playerGuild'; }
  public static get PlayerLevel() { return 'playerLevel'; }
}

interface ComponentWithElement {
  component: MatExpansionPanel;
  element: ElementRef;
}

@Component({
  selector: 'app-view-guild-profile',
  templateUrl: './view-guild-profile.component.html',
  styleUrls: ['./view-guild-profile.component.scss']
})
export class ViewGuildProfileComponent implements OnInit, AfterViewChecked {
  public profile: FullGuildProfile = null;
  public unassignedPlayers = new Subject<Array<StoredPlayer>>();
  public mains = new Array<PlayerMain>();
  public mainsSubject = new Subject<Array<PlayerMain>>();
  public altsSubject = new Subject<Array<PlayerAlt>>();
  public altsObservable: Observable<Array<PlayerAlt>>;
  public orderedMains = new Array<PlayerMain>();
  public orderedMainsObs = new Observable<Array<PlayerMain>>();
  public filteredPlayersObs: Observable<Array<StoredPlayer>>;
  public filteredPlayers: Array<StoredPlayer>;
  public playerFilterText: Subject<string> = new Subject<string>();
  public dropScopes = new DropScopes();
  public isAdmin = false;
  public playerFlaggedForExpansion: number = null;

  @ViewChild(ContextMenuComponent) public contextMenu: ContextMenuComponent;
  @ViewChildren('playerPanel',  { read: MatExpansionPanel  }) public playerPanelComponents: QueryList<MatExpansionPanel>;
  @ViewChildren('playerPanel',  { read: ElementRef  }) public playerPanelElements: QueryList<ElementRef>;

  constructor(
      private route: ActivatedRoute,
      private router: Router,
      private dataService: DataService,
      private busyService: BusyService,
      private wowService: WowService,
      private authService: AuthService,
      private notificationService: NotificationService,
      private dialog: MatDialog,
      private errorService: ErrorReportingService,
      @Inject('BASE_URL') private baseUrl: string) {

    this.orderedMainsObs = this.mainsSubject.pipe(
      map(m => {
        return this.sortMains(m);
      }));
      this.orderedMainsObs.subscribe(orderedMains => {

      this.altsSubject.next(this.getAltsFromMains(orderedMains));
      this.orderedMains = orderedMains;
    });

    this.filteredPlayersObs = this.getPlayerFilterer();

    this.filteredPlayersObs.subscribe(result => {
      this.filteredPlayers = result;
    });
  }

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.updateRouteParams(params);
    });
  }

  ngAfterViewChecked(): void {

    if (this.playerFlaggedForExpansion) {
      // Couldn't find a way to get both the components and elements in the same list, so had
      //   to generate it manually
      const pairArray = new Array<ComponentWithElement>();

      const componentsArray = this.playerPanelComponents.toArray();
      const elementsArray = this.playerPanelElements.toArray();

      for (let i = 0; i < componentsArray.length; i++) {
        pairArray.push({
          component: componentsArray[i],
          element: elementsArray[i]
        });
      }

      pairArray.some(pair => {
        const elementIdString = pair.element.nativeElement.getAttribute('data-main-id');
        const elementId = parseInt(elementIdString, 10);

        if (elementId === this.playerFlaggedForExpansion) {
          pair.component.expanded = true;
          this.playerFlaggedForExpansion = null;

          return true;
        }
        return false;
      });
    }
  }

  public get showAdminToolbar(): boolean {
    if (!this.profile) {
      return false;
    }

    if (this.profile.isPublic) {
      return false;
    }

    if (!this.profile.currentPermissionLevel) {
      return false;
    }

    return PermissionsOrder.GreaterThanOrEqual(this.profile.currentPermissionLevel, GuildProfilePermissionLevel.Officer);
  }

  public get showVisitorToolbar(): boolean {
    if (!this.profile) {
      return false;
    }

    if (this.profile.isPublic){
      return false;
    }

    return (!this.authService.isAuthenticated || !this.profile.currentPermissionLevel);
  }

  public get getAccessRequestBadge(): number {
    if (!this.profile) {
      return null;
    }

    if (this.profile.accessRequestCount === 0) {
      return null;
    }

    return this.profile.accessRequestCount;
  }

  public get getManagePermissionsTooltip(): string {
    if (this.profile && this.profile.accessRequestCount > 0) {
      return `You have ${this.profile.accessRequestCount} pending access request${this.profile.accessRequestCount > 1 ? 's' : ''}.`;
    }
  }

  public playersFilterChanged($e: any) {
    this.playerFilterText.next($e.target.value);
  }

  public getPlayerCssClass(player: StoredPlayer): string {
    const classString = this.wowService.getClassTag(player.class as WowClass);

    return `background-${classString}`;
  }

  public getWowClassLabel(wowClass: WowClass): string {
    return this.wowService.getClassLabel(wowClass);
  }

  public requestAccess() {
    if (!this.authService.isAuthenticated) {
      this.errorService.reportError('You must log in to request access!');
      return;
    }

    this.busyService.setBusy();
    this.dataService.requestProfileAccess(this.profile.id).subscribe(
      success => {
        this.busyService.unsetBusy();
        this.notificationService.showNotification('You have requested access for this profile.');
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      });
  }

  public navigateToAccessRequests(): void {
    this.router.navigate(['/' + RoutePaths.ManagePermissions, this.profile.id]);
  }

  public onUnassignedPlayerDropped($event: DropEvent) {
    const draggedPlayer = $event.dragData as BlizzardPlayer;

    this.busyService.setBusy();

    this.dataService.addPlayerMainToProfile(draggedPlayer, this.profile)
      .subscribe(
        success => {
          this.busyService.unsetBusy();
          this.mains.push(success);
          this.mainsSubject.next(this.mains);
        },
        error => {
          this.busyService.unsetBusy();
          this.errorService.reportApiError(error);
        }
      );
  }

  public onAltDropped($event: DropEvent, main: PlayerMain, component: MatExpansionPanel) {
    const draggedPlayer = $event.dragData as BlizzardPlayer;

    this.busyService.setBusy();

    this.dataService.addAltToMain(draggedPlayer, main, this.profile)
      .subscribe(
        success => {
          this.busyService.unsetBusy();

          main.alts.push(success);
          main.alts = this.sortAlts(main.alts);
          this.altsSubject.next(this.getAltsFromMains(this.mains));

          component.expanded = true;
        },
        error => {
          this.busyService.unsetBusy();
          this.errorService.reportApiError(error);
        }
      );
  }

  public getPlayerDescription(main: PlayerMain): string {
    return `Level ${main.player.level} ${this.wowService.getClassLabel(main.player.class)}`;
  }

  public removeAlt(event: RemoveAltEvent): void {
    const main = event.main;
    const alt = event.alt;

    this.busyService.setBusy();
      this.dataService.removeAltFromMain(main, alt, this.profile)
        .subscribe(
          success => {
            this.busyService.unsetBusy();

            const targetIndex = main.alts.findIndex(a => a.id === alt.id);
            main.alts.splice(targetIndex, 1);
            this.altsSubject.next(this.getAltsFromMains(this.mains));
          },
          error => {
            this.busyService.unsetBusy();
            this.errorService.reportApiError(error);
          });
  }

  public removeMainWithConfirmation(main: PlayerMain): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      disableClose: true,
      data: {
        title: 'Are you sure?',
        confirmationText: 'Are you sure you want to delete this player?'
      },
      width: '400px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (!result) {
        return;
      }

      this.removeMain({
        main: main
      } as RemoveMainEvent);
    });
  }

  public removeMain(event: RemoveMainEvent): void {
    const main = event.main;

    this.busyService.setBusy();
      this.dataService.removeMain(main, this.profile)
        .subscribe(
          success => {
            this.busyService.unsetBusy();

            const targetIndex = this.mains.findIndex(m => m.id === main.id);
            this.mains.splice(targetIndex, 1);
            this.mainsSubject.next(this.mains);
          },
          error => {
            this.busyService.unsetBusy();
            this.errorService.reportApiError(error);
          });
  }

  public viewBlizzardProfile(main: PlayerMain) {
    WowService.viewBlizzardProfile(main.player);
  }

  public viewRaiderIo(main: PlayerMain) {
    WowService.viewRaiderIo(main.player);
  }

  public viewWowProgress(main: PlayerMain) {
    WowService.viewWowProgress(main.player);
  }

  public onAltPromoted(event: PromoteAltToMainEvent) {
    this.busyService.setBusy();

    this.dataService.promoteAltToMain(event.alt.id, this.profile).subscribe(
      success => {
        this.busyService.unsetBusy();

        success.alts = this.sortAlts(success.alts);

        const oldMainIndex = this.mains.findIndex(x => x.id === event.main.id);
        this.mains.splice(oldMainIndex, 1);
        this.mains.push(success);
        this.mainsSubject.next(this.mains);

        this.altsSubject.next(this.getAltsFromMains(this.mains));

        this.playerFlaggedForExpansion = success.id;
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      }
    );
  }

  public onPlayerNotesChanged(event: EditPlayerNotesEvent): void {
    this.busyService.setBusy();
      this.dataService.editPlayerNotes(event.main, this.profile, event.newNotes)
        .subscribe(
          success => {
            this.busyService.unsetBusy();
          },
          error => {
            this.busyService.unsetBusy();
            this.errorService.reportApiError(error);
          });
  }

  public onOfficerNotesChanged(event: EditOfficerNotesEvent): void {
    this.busyService.setBusy();
      this.dataService.editOfficerNotes(event.main, this.profile, event.newNotes)
        .subscribe(
          success => {
            this.busyService.unsetBusy();
          },
          error => {
            this.busyService.unsetBusy();
            this.errorService.reportApiError(error);
          });
  }

  public shareProfile() {
    const profileUrl = `${this.baseUrl}/${RoutePaths.ViewProfile}/${this.profile.id}`;

    const dialogRef = this.dialog.open(ShareLinkDialogComponent, {
      disableClose: false,
      data: {
        url: profileUrl
      } as ShareLinkDialogData,
      width: '400px'
    });
  }


  public get playerColumns(): Array<string> {
    if (!this.profile) {
      return [];
    }

    if (this.profile.friendGuilds.length === 0) {
      return [PlayerTableColumnLabels.PlayerName, PlayerTableColumnLabels.PlayerLevel];
    }
    else {
      return [
        PlayerTableColumnLabels.PlayerName,
        PlayerTableColumnLabels.PlayerGuild,
        PlayerTableColumnLabels.PlayerLevel
      ];
    }
  }

  public getPlayerAbbreviation(player: StoredPlayer): string {
    if (player.guild.abbreviation) {
      return player.guild.abbreviation;
    }

    return player.guild.name.substr(0, Math.min(player.guild.name.length, 3)).toUpperCase();
  }

  private getPlayerFilterer(): Observable<Array<BlizzardPlayer>> {
    return combineLatest(
      [
        this.unassignedPlayers.pipe(
          startWith(new Array<StoredPlayer>()),
          map(p => {
            return this.sortPlayers(p);
          })
        ),
        this.playerFilterText.pipe(
          startWith(''),
          map(
            s => s.trim())
        ),
        this.orderedMainsObs.pipe(
          startWith(new Array<PlayerMain>())
        ),
        this.altsSubject.pipe(
          startWith(new Array<PlayerAlt>())
        )
      ])
    .pipe(
      map(([players, searchText, orderedMains, alts]) => {
        return players
          .filter(p => p.name.toLowerCase().includes(searchText.toLowerCase()))
          .filter(p => !orderedMains.find(o => o.player.name.toLowerCase() === p.name.toLowerCase()))
          .filter(p => !alts.find(a => a.player.name.toLowerCase() === p.name.toLowerCase()));
      }));
  }

  private updateRouteParams(params: any): void {
    const profileId = params['id'];

    this.unassignedPlayers.next([]);

    this.busyService.setBusy();

    this.dataService.getFullGuildProfile(profileId).subscribe(
      success => {
        this.busyService.unsetBusy();
        this.profile = success;
        this.isAdmin = this.profile.currentPermissionLevel
          && PermissionsOrder.GreaterThanOrEqual(this.profile.currentPermissionLevel, GuildProfilePermissionLevel.Officer);

        this.unassignedPlayers.next(this.profile.players);

        if (this.profile.isPublic) {
          this.isAdmin = true;
        }

        this.mains = this.transformMainsList(this.profile.mains);
        this.mainsSubject.next(this.mains);
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      }
    );
  }

  private sortPlayers(players: StoredPlayer[]): StoredPlayer[] {
    return players.sort((a, b) => {
      if (a.level === b.level) {
        if (a.name < b.name){
          return -1;
        }
        else if (a.name > b.name){
          return 1;
        }

        return 0;
      }

      return b.level - a.level;
    });
  }

  private sortMains(players: PlayerMain[]): PlayerMain[] {
    return players.sort((a, b) => {
        if (a.player.name < b.player.name) {
          return -1;
        }
        else if (a.player.name > b.player.name) {
          return 1;
        }

        return 0;
      });
  }

  private sortAlts(players: PlayerAlt[]): PlayerAlt[] {
    return players.sort((a, b) => {
        if (a.player.name < b.player.name) {
          return -1;
        }
        else if (a.player.name > b.player.name) {
          return 1;
        }

        return 0;
      });
  }

  private getAltsFromMains(mains: Array<PlayerMain>): Array<PlayerAlt> {
    return mains.reduce(
      (a, b) => {
        if (!b.alts){
          b.alts =  [];
        }
        return a.concat(b.alts);
      },
      new Array<PlayerAlt>());
  }

  private transformMainsList(mains: Array<PlayerMain>): Array<PlayerMain> {
    mains.forEach(main => {
      if (!main.alts) {
        main.alts = [];
      }

      main.alts = this.sortAlts(main.alts);
    });

    return mains;
  }

}
