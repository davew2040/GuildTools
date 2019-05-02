import { Component, OnInit, ViewChild, ViewChildren } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FullGuildProfile, BlizzardPlayer, PlayerMain, PlayerAlt } from 'app/services/ServiceTypes/service-types';
import { DataService, WowClass } from 'app/services/data-services';
import { BusyService } from 'app/shared-services/busy-service';
import { WowService } from 'app/services/wow-service';
import { ContextMenuComponent } from 'ngx-contextmenu';
import { Observable, Subject, combineLatest } from 'rxjs';
import { startWith, map, filter } from 'rxjs/operators';
import { DropEvent } from 'ng-drag-drop';
import { MatExpansionModule, MatExpansionPanel } from '@angular/material';
import { RemoveAltEvent, RemoveMainEvent } from '../view-main/view-main.component';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';

class DropScopes {
  public DropMain = 'DropMain';
  public DropAlt = 'DropAlt';
}

@Component({
  selector: 'app-view-guild-profile',
  templateUrl: './view-guild-profile.component.html',
  styleUrls: ['./view-guild-profile.component.scss']
})
export class ViewGuildProfileComponent implements OnInit {
  public profile: FullGuildProfile = null;
  public playerColumns: Array<string> = ['playerName', 'playerLevel'];
  public unassignedPlayers = new Subject<Array<BlizzardPlayer>>();
  public mains = new Array<PlayerMain>();
  public mainsSubject = new Subject<Array<PlayerMain>>();
  public altsSubject = new Subject<Array<PlayerAlt>>();
  public altsObservable: Observable<Array<PlayerAlt>>;
  public orderedMains = new Observable<Array<PlayerMain>>();
  public filteredPlayersObs: Observable<Array<BlizzardPlayer>>;
  public filteredPlayers: Array<BlizzardPlayer>;
  public playerFilterText: Subject<string> = new Subject<string>();

  public dropScopes = new DropScopes();

  public items = [
    { name: 'John', otherProperty: 'Foo' },
    { name: 'Joe', otherProperty: 'Bar' }
  ];

  @ViewChild(ContextMenuComponent) public basicMenu: ContextMenuComponent;
  @ViewChildren('playerPanel') private expansionPanels: MatExpansionPanel;

  constructor(
      private route: ActivatedRoute,
      private dataService: DataService,
      private busyService: BusyService,
      private wowService: WowService,
      private errorService: ErrorReportingService) {

    this.orderedMains = this.mainsSubject.pipe(
      map(m => this.sortMains(m))
    );

    this.orderedMains.subscribe(mains => {
      this.altsSubject.next(this.getAltsFromMains(mains));
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

  public playersFilterChanged($e: any) {
    this.playerFilterText.next($e.target.value);
  }

  public getPlayerCssClass(player: BlizzardPlayer): string {
    const classString = this.wowService.getClassTag(player.class as WowClass);

    return `background-${classString}`;
  }

  public getWowClassLabel(wowClass: WowClass): string {
    return this.wowService.getClassLabel(wowClass);
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

  private getPlayerFilterer(): Observable<Array<BlizzardPlayer>> {
    return combineLatest(
      [
        this.unassignedPlayers.pipe(
          startWith([]),
          map(p => {
            return this.sortPlayers(p);
          })
        ),
        this.playerFilterText.pipe(
          startWith(''),
          map(
            s => s.trim())
        ),
        this.orderedMains.pipe(
          startWith([])
        ),
        this.altsSubject.pipe(
          startWith([])
        )
      ])
    .pipe(
      map(([players, searchText, orderedMains, alts]) => {
        return players
          .filter(p => p.playerName.toLowerCase().includes(searchText.toLowerCase()))
          .filter(p => !orderedMains.find(o => o.player.name.toLowerCase() === p.playerName.toLowerCase()))
          .filter(p => !alts.find(a => a.player.name.toLowerCase() === p.playerName.toLowerCase()));
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

        this.unassignedPlayers.next(this.profile.players);

        this.mains = this.transformMainsList(this.profile.mains);
        this.mainsSubject.next(this.mains);
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      }
    );
  }

  private sortPlayers(players: BlizzardPlayer[]): BlizzardPlayer[] {
    return players.sort((a, b) => {
      if (a.level === b.level) {
        if (a.playerName < b.playerName){
          return -1;
        }
        else if (a.playerName > b.playerName){
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
