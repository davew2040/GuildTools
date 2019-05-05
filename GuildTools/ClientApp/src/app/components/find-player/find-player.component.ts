import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { BlizzardRegionDefinition, BlizzardRealms } from '../../data/blizzard-realms';
import { BusyService } from '../../shared-services/busy-service';
import { DataService } from '../../services/data-services';
import { Realm } from '../../services/ServiceTypes/service-types';
import { FormControl, FormGroup, AbstractControl } from '@angular/forms';
import { Observable, Subject, combineLatest } from 'rxjs';
import { startWith, map, filter, tap } from 'rxjs/operators';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { NotificationService } from 'app/shared-services/notification-service';
import { StoredValuesService } from 'app/shared-services/stored-values';
import { AuthService } from 'app/auth/auth.service';
import { SelectedPlayer } from 'app/models/selected-player';

@Component({
  selector: 'app-find-player',
  templateUrl: './find-player.component.html',
  styleUrls: ['./find-player.component.css']
})
export class FindPlayerComponent implements OnInit {

  public selectedRealm: Realm;
  public regions: Array<BlizzardRegionDefinition>;
  public realmsSubject: Subject<Realm[]>;
  public filteredRealmsObservable: Observable<Array<Realm>>;
  public realms = new Array<Realm>();

  public searchForm = new FormGroup({
    region: new FormControl(''),
    playerName: new FormControl(''),
    realm: new FormControl('')
  });

  public get regionControlName() { return 'region'; }
  public get playerNameControlName() { return 'playerName'; }
  public get realmNameControlName() { return 'realm'; }

  @Output() public playerSelected = new EventEmitter<SelectedPlayer>()
  @Output() public cancelled = new EventEmitter();

  constructor(
      public busyService: BusyService,
      private dataService: DataService,
      private notificationService: NotificationService,
      private errorService: ErrorReportingService,
      private authService: AuthService) {
    this.regions = BlizzardRealms.AllRealms;
  }

  ngOnInit() {
    this.realmsSubject = new Subject<Realm[]>();

    this.realmsSubject.subscribe(realms => {
      this.realms = realms;
    });

    const realmsControlValueChanges = this.realmsControl.valueChanges.pipe(
      startWith(''),
      map(value => {
        return typeof value === 'string' ? value : value.name;
      })
    );

    this.filteredRealmsObservable = combineLatest(
        [
          this.realmsSubject.pipe(startWith([])),
          realmsControlValueChanges
        ])
      .pipe(
        map(([realms, typedSearch]) => {
          if (typeof typedSearch !== 'string') {
            return typedSearch.slice();
          }
          return this.filterRealms(typedSearch, realms);
        }));

    this.regionControl.valueChanges.subscribe((newValue: BlizzardRegionDefinition) => {
      this.busyService.setBusy();

      this.dataService.getRealms(newValue.Name).subscribe(
        success => {
          this.busyService.unsetBusy();
          this.realmsSubject.next(success);
          this.realmsControl.setValue('');
          this.populateFieldInitialValues();
        },
        error => {
          this.busyService.unsetBusy();
          this.errorService.reportApiError(error);
        }
      );
    });

    this.setInitialRegion();
  }

  public displayRealm(realm?: Realm): string | undefined {
    return realm ? realm.name : undefined;
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public okay(): void {
    const player = new SelectedPlayer();

    const region: BlizzardRegionDefinition = this.regionControl.value;
    const playerName: string = this.playerNameControl.value;
    const realm: Realm = this.realmsControl.value;

    player.region = region;
    player.name = playerName;
    player.realm = realm.name;

    this.busyService.setBusy();

    this.dataService.getPlayerExists(
      region.Name,
      playerName,
      realm.name)
    .subscribe(
      success => {
        this.busyService.unsetBusy();

        if (success.found) {
          player.name = success.playerDetails.playerName;
          player.realm = success.playerDetails.realmName;
          player.guildName = success.playerDetails.guildName;
          player.guildRealm = success.playerDetails.guildRealm;

          localStorage.setItem(StoredValuesService.lastUsedPlayerKey, playerName);
          localStorage.setItem(StoredValuesService.lastUsedGuildKey, player.guildName);
          localStorage.setItem(StoredValuesService.lastUsedRealmKey, realm.name);
          localStorage.setItem(StoredValuesService.lastUsedRegionKey, region.Name);

          this.playerSelected.emit(player);
        }
        else {
          this.notificationService.showNotification('Player not found!');
        }
      },
      error => {
        this.errorService.reportApiError(error);
        this.busyService.unsetBusy();
      });
  }

  public get canContinue(): boolean {
    if (!this.searchForm) {
      return false;
    }

    return this.playerNameControl.value && this.realmsControl.value;
  }

  private get realmsControl(): AbstractControl {
    return this.searchForm.controls[this.realmNameControlName];
  }

  private get regionControl(): AbstractControl {
    return this.searchForm.controls[this.regionControlName];
  }

  private get playerNameControl(): AbstractControl {
    return this.searchForm.controls[this.playerNameControlName];
  }

  private filterRealms(typedSearch: string, realms: Array<Realm>): Array<Realm> {
    const lowered = typedSearch.trim().toLowerCase();

    if (typedSearch.trim() !== '') {
      realms = realms.filter(x => x.name.toLowerCase().includes(lowered));
    }

    realms = realms.sort(
      (a: Realm, b: Realm) => {
        if (a.name < b.name) {
          return -1;
        }
        if (a.name > b.name) {
          return 1;
        }
        return 0;
      });

    return realms;
  }

  private setInitialRegion(): void {
    const userDetails = this.authService.userDetails;

    let regionName = localStorage.getItem(StoredValuesService.lastUsedRegionKey);
    if (!regionName && userDetails) {
      regionName = this.authService.userDetails.playerRegion;
    }

    if (regionName) {
      const foundRegionFromList = this.regions.find(x => x.Name.toLowerCase() === regionName.toLowerCase());
      if (foundRegionFromList) {
        this.regionControl.setValue(foundRegionFromList);
        return;
      }
    }

    this.regionControl.setValue(this.regions[0]);
  }

  private populateFieldInitialValues(): void {
    const userDetails = this.authService.userDetails;

    let playerName = localStorage.getItem(StoredValuesService.lastUsedPlayerKey);
    if (!playerName && userDetails) {
      playerName = this.authService.userDetails.playerName;
    }

    if (playerName) {
      this.playerNameControl.setValue(playerName);
    }

    let realmName = localStorage.getItem(StoredValuesService.lastUsedRealmKey);
    if (!realmName && userDetails) {
      realmName = this.authService.userDetails.playerRealm;
    }

    if (realmName) {
      const foundRealmFromList = this.realms.find(x => x.name.toLowerCase() === realmName.toLowerCase());
      if (foundRealmFromList) {
        this.realmsControl.setValue(foundRealmFromList);
      }
    }
  }
}
