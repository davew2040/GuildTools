import { Injectable } from '@angular/core';

@Injectable()
export class StoredValuesService {

  public static get lastUsedGuildKey() { return 'LastUsedGuild'; }
  public static get lastUsedRegionKey() { return 'LastUsedRegion'; }
  public static get lastUsedPlayerKey() { return 'LastUsedPlayer'; }
  public static get lastUsedRealmKey() { return 'LastUsedRealm'; }

  constructor() {

  }

  public get lastUsedGuild(): string {
    return localStorage.getItem(StoredValuesService.lastUsedGuildKey);
  }

  public set lastUsedGuild(value: string) {
    localStorage.setItem(StoredValuesService.lastUsedGuildKey, value);
  }

  public get lastUsedRegion(): string {
    return localStorage.getItem(StoredValuesService.lastUsedRegionKey);
  }

  public set lastUsedRegion(value: string) {
    localStorage.setItem(StoredValuesService.lastUsedRegionKey, value);
  }

  public get lastUsedPlayer(): string {
    return localStorage.getItem(StoredValuesService.lastUsedPlayerKey);
  }

  public set lastUsedPlayer(value: string) {
    localStorage.setItem(StoredValuesService.lastUsedPlayerKey, value);
  }
}
