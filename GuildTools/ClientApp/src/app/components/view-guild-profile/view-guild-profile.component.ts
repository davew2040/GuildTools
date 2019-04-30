import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FullGuildProfile, BlizzardPlayer, PlayerMain } from 'app/services/ServiceTypes/service-types';
import { DataService, WowClass } from 'app/services/data-services';
import { BusyService } from 'app/shared-services/busy-service';
import { WowService } from 'app/services/wow-service';
import { ContextMenuComponent } from 'ngx-contextmenu';
import { Observable, Subject, combineLatest } from 'rxjs';
import { startWith, map } from 'rxjs/operators';
import { DropEvent } from 'ng-drag-drop';

@Component({
  selector: 'app-view-guild-profile',
  templateUrl: './view-guild-profile.component.html',
  styleUrls: ['./view-guild-profile.component.scss']
})
export class ViewGuildProfileComponent implements OnInit {
  public profile: FullGuildProfile = null;
  public playerColumns: Array<string> = ['playerName', 'playerLevel'];
  public unassignedPlayers = new Subject<Array<BlizzardPlayer>>();
  public mains = new Subject<Array<PlayerMain>>();
  public filteredPlayersObs: Observable<Array<BlizzardPlayer>>;
  public filteredPlayers: Array<BlizzardPlayer>;
  public playerFilterText: Subject<string> = new Subject<string>();

  public items = [
    { name: 'John', otherProperty: 'Foo' },
    { name: 'Joe', otherProperty: 'Bar' }
  ];

  @ViewChild(ContextMenuComponent) public basicMenu: ContextMenuComponent;

  constructor(
      private route: ActivatedRoute,
      private dataService: DataService,
      private busyService: BusyService,
      private wowService: WowService) {

    this.filteredPlayersObs = this.getPlayerFilterer();

    this.filteredPlayersObs.subscribe(result => {
      this.filteredPlayers = result;
    });
  }

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.updateParams(params);
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
          var result = success;
        },
        error => {
          this.busyService.unsetBusy();
          alert("Encountered error while attempting to assign this player.")
        }
      )
  }

  private getPlayerFilterer(): Observable<Array<BlizzardPlayer>> {
    return combineLatest(
      [
        this.unassignedPlayers.pipe(
          startWith([]),
          map(p => {
            return this.sortPlayers(p);
          })),
          this.playerFilterText.pipe(
            startWith(''),
            map(s => s.trim())
          )
      ])
    .pipe(
      map(([players, searchText]) => {
        return players.filter(p => p.playerName.toLowerCase().includes(searchText.toLowerCase()));
      }));
  }

  private updateParams(params: any): void {
    const profileId = params['id'];
    this.unassignedPlayers.next([]);

    this.busyService.setBusy();

    this.dataService.getFullGuildProfile(profileId).subscribe(
      success => {
        this.busyService.unsetBusy();
        this.profile = success;

        this.unassignedPlayers.next(this.profile.players);
        this.mains.next(this.profile.mains);
      },
      error => {
        this.busyService.unsetBusy();
        alert('Error retrieving guild profile.');
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
}
