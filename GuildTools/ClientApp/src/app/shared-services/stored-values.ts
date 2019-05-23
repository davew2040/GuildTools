import { Injectable } from '@angular/core';
import { IUserDetails } from 'app/auth/user-details';

@Injectable()
export class StoredValuesService {

  public static get lastUsedGuildKey() { return 'LastUsedGuild'; }
  public static get lastUsedRegionKey() { return 'LastUsedRegion'; }
  public static get lastUsedPlayerKey() { return 'LastUsedPlayer'; }
  public static get lastUsedRealmKey() { return 'LastUsedRealm'; }
  public static get userDetailsKey() { return 'UserDetails'; }
  public static get AccessTokenKey() { return 'access_token'; }

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

  public set userDetails(userDetails: IUserDetails) {
    if (!userDetails) {
      localStorage.removeItem(StoredValuesService.userDetailsKey);
      return;
    }

    localStorage.setItem(StoredValuesService.userDetailsKey, JSON.stringify(userDetails));
  }

  public get userDetails(): IUserDetails {
    const storedJson = localStorage.getItem(StoredValuesService.userDetailsKey);

    if (!storedJson) {
      return null;
    }

    const details = JSON.parse(storedJson) as IUserDetails;

    return details;
  }
}
