import { Component, OnInit } from '@angular/core';
import { BlizzardRegionDefinition, BlizzardRealms } from '../../data/blizzard-realms';
import { BusyService } from '../../shared-services/busy-service';
import { DataService } from '../../services/data-services';
import { Realm } from '../../services/ServiceTypes/service-types';
import { FormControl, FormGroup, AbstractControl } from '@angular/forms';
import { Observable, Subject, combineLatest } from 'rxjs';
import { startWith, map, filter, tap } from 'rxjs/operators';

@Component({
  selector: 'app-find-guild',
  templateUrl: './find-guild.component.html',
  styleUrls: ['./find-guild.component.css']
})
export class FindGuildComponent implements OnInit {

  public selectedRegionName: string;
  public selectedRealm: Realm;
  public regions: Array<BlizzardRegionDefinition>;
  public realmsSubject: Subject<Realm[]>;
  public selectedGuild: string;
  public filteredRealms: Observable<Realm[]>;
  private searchState = SearchState.Waiting;
  public searchForm = new FormGroup({
    region: new FormControl(''),
    guild: new FormControl(''),
    realm: new FormControl('')
  });

  constructor(public busyService: BusyService, private dataService: DataService) { 
    this.selectedGuild = '';

    this.regions = BlizzardRealms.AllRealms;

    this.selectedRegionName = this.regions[0].Name;
  }

  ngOnInit() {
    this.realmsSubject = new Subject<Realm[]>();

    let realmsControlValueChanges = this.realmsControl.valueChanges.pipe(
      startWith(''),
      map(value => {
        return typeof value === 'string' ? value : value.name;}
        )
      );

      this.searchForm.valueChanges.subscribe(() => {
        this.searchState = SearchState.Waiting;
      });

    this.filteredRealms = combineLatest(
        this.realmsSubject.pipe(startWith([])),
        realmsControlValueChanges)
      .pipe(
        map(([realms, typedSearch]) => {
          if (typeof typedSearch !== 'string') {
            return typedSearch.slice();
          }
          return this.filterRealms(typedSearch, realms);
        }));

    this.busyService.setBusy();

    this.dataService.getRealms(this.selectedRegionName).subscribe(
      success => {
        this.busyService.unsetBusy();
        this.realmsSubject.next(success);
      },
      error => {
        this.busyService.unsetBusy();
        console.error("Failed to get realms.");
      }
    )
  }

  public get searchStateClasses(): Array<string> {
    if (this.searchState === SearchState.Waiting) {
      return []
    }
    else if (this.searchState === SearchState.Found) {
      return ['search-found'];  
    }
    else {
      return ['search-not-found'];
    }
  }

  public get searchStateText(): string {
    if (this.searchState === SearchState.Waiting) {
      return "Search";
    }
    else if (this.searchState === SearchState.Found) {
      return "Found!";  
    }
    else {
      return "Not Found";
    }
  }

  public get canContinue(): boolean {
    if (this.searchState === SearchState.Found) {
      return true;
    }
    else {
      return false;
    }
  }

  public search(): void {
    this.busyService.setBusy();

    this.dataService.getGuildExists(
        this.searchForm.controls['region'].value.name, 
        this.searchForm.controls['guild'].value, 
        this.searchForm.controls['realm'].value.name)
      .subscribe(
        success => {
          if (success.Found) {
            this.searchState = SearchState.Found
          }
          else {
            this.searchState = SearchState.NotFound;
          }
          this.busyService.unsetBusy();
        },
        error => {
          console.error("An error occurred while fetching this guild state.");
          this.busyService.unsetBusy;
        });
  }

  public displayRealm(realm?: Realm): string | undefined {
    return realm ? realm.name : undefined;
  }

  private get realmsControl(): AbstractControl {
    return this.searchForm.controls.realm;
  }

  private filterRealms(typedSearch: string, realms: Array<Realm>): Array<Realm> {
    const lowered = typedSearch.trim().toLowerCase();

    if (typedSearch.trim() !== '') {
      realms = realms.filter(x => x.name.toLowerCase().includes(lowered));
    }
    
    realms = realms.sort(
      (a: Realm,b : Realm) => {
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
}

enum SearchState {
  Waiting,
  Found,
  NotFound
}