import { Component, OnInit, EventEmitter, Output, Input } from '@angular/core';
import { BlizzardRegionDefinition, BlizzardRealms } from '../../data/blizzard-realms';
import { BusyService } from '../../shared-services/busy-service';
import { DataService } from '../../services/data-services';
import { Realm } from '../../services/ServiceTypes/service-types';
import { FormControl, FormGroup, AbstractControl } from '@angular/forms';
import { Observable, Subject, combineLatest } from 'rxjs';
import { startWith, map, filter, tap } from 'rxjs/operators';
import { SelectedGuild } from '../../models/selected-guild';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { NotificationService } from 'app/shared-services/notification-service';
import { StoredValuesService } from 'app/shared-services/stored-values';
import { AuthService } from 'app/auth/auth.service';

@Component({
  selector: 'app-find-guild',
  templateUrl: './find-guild.component.html',
  styleUrls: ['./find-guild.component.css']
})
export class FindGuildComponent implements OnInit {

  public selectedRealm: Realm;
  public regions: Array<BlizzardRegionDefinition>;
  public realmsSubject: Subject<Realm[]>;
  public filteredRealmsObservable: Observable<Array<Realm>>;
  public realms = new Array<Realm>();

  public searchForm = new FormGroup({
    region: new FormControl(''),
    guildName: new FormControl(''),
    realm: new FormControl('')
  });

  @Input() showCancel: boolean = true;
  @Input() okayButtonText: string = 'Okay';

  @Output() guildSelected = new EventEmitter<SelectedGuild>()
  @Output() cancelled = new EventEmitter();

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
    const guild = new SelectedGuild();

    const region: BlizzardRegionDefinition = this.regionControl.value;
    const guildName: string = this.guildNameControl.value;
    const realm: Realm = this.realmsControl.value;

    guild.region = region;
    guild.name = guildName;
    guild.realm = realm.name;

    this.busyService.setBusy();

    this.dataService.getGuildExists(
      region.Name,
      guildName,
      realm.name)
    .subscribe(
      success => {
        this.busyService.unsetBusy();

        if (success.found) {
          localStorage.setItem(StoredValuesService.lastUsedGuildKey, guildName);
          localStorage.setItem(StoredValuesService.lastUsedRealmKey, realm.name);
          localStorage.setItem(StoredValuesService.lastUsedRegionKey, region.Name);

          this.guildSelected.emit(guild);
        }
        else {
          this.notificationService.showNotification('Guild not found!');
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

    return this.guildNameControl.value && this.realmsControl.value;
  }

  private get realmsControl(): AbstractControl {
    return this.searchForm.controls['realm'];
  }

  private get regionControl(): AbstractControl {
    return this.searchForm.controls['region'];
  }

  private get guildNameControl(): AbstractControl {
    return this.searchForm.controls['guildName'];
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

    let guildName = localStorage.getItem(StoredValuesService.lastUsedGuildKey);
    if (!guildName && userDetails) {
      guildName = this.authService.userDetails.guildName;
    }

    if (guildName) {
      this.guildNameControl.setValue(guildName);
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
