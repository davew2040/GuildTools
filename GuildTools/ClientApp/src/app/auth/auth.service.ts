import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../services/account-service';
import { BusyService } from '../shared-services/busy-service';
import { Observable, Subscription } from 'rxjs';
import { log } from 'util';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
(window as any).global = window;

@Injectable()
export class AuthService {

  constructor(
    public router: Router,
    private accountService: AccountService,
    private busyService: BusyService,
    private errorService: ErrorReportingService) { }

  public appInitialization(): void {
    if (!this.isAuthenticated) {
      this.clearLocalStorage();
    }
  }

  public logIn(email: string, password: string): Observable<Object> {
    this.busyService.setBusy();

    const loginObservable = this.accountService.login(email, password);

    loginObservable.subscribe(
      success => {
        this.busyService.unsetBusy();
        this.handleLoginSuccess(success);
      },
      error => {
        this.errorService.reportApiError(error);
        this.busyService.unsetBusy();
      });

    return loginObservable;
  }

  public processLogin(loginResponse: any) {
    this.handleLoginSuccess(loginResponse);
  }

  private handleLoginSuccess(authResult): void {
    let accessToken = authResult['access_token'];

    let decoded = this.decodeTokenExpiration(accessToken);

    // Set the time that the access token will expire at
    const expiresAt = JSON.stringify((decoded.exp * 1000) + new Date().getTime());

    localStorage.setItem(AuthKeys.AccessToken, accessToken);
    localStorage.setItem(AuthKeys.ExpiresAt, expiresAt);

    let userName = authResult['email'];
    localStorage.setItem(AuthKeys.UserName, userName);

    let permissions = authResult['permissions'];

    localStorage.setItem(AuthKeys.ProfilePermissions, JSON.stringify(permissions));
  }

  public getAccessToken(): string {
    return localStorage.getItem(AuthKeys.AccessToken);
  }

  public get username(): string {
    return localStorage.getItem(AuthKeys.UserName);
  }

  private decodeTokenExpiration(token: string): any {
    let base64Url = token.split('.')[1];
    let base64 = base64Url.replace('-', '+').replace('_', '/');

    return JSON.parse(window.atob(base64));
  }

  public logOut(): void {
    this.clearLocalStorage();
  }

  private clearLocalStorage(): void {
    localStorage.removeItem(AuthKeys.AccessToken);
    localStorage.removeItem(AuthKeys.ExpiresAt);
    localStorage.removeItem(AuthKeys.ProfilePermissions);
  }

  public get isAuthenticated(): boolean {
    // Check whether the current time is past the
    // access token's expiry time
    const expiresAt = JSON.parse(localStorage.getItem(AuthKeys.ExpiresAt) || '{}');
    return new Date().getTime() < expiresAt;
  }

  public getPermissionLevelForProfile(profileId: number): GuildProfilePermissionLevel | null {
    if (!this.isAuthenticated) {
      return null;
    }

    var profilePermissionsObject = JSON.parse(localStorage.get(AuthKeys.ProfilePermissions));

    return GuildProfilePermissionLevel.Visitor;
  }


  private getPermissionSetFromResponseContent(response: any): GuildProfilePermissionSet {
    return new GuildProfilePermissionSet();
  }
}

export enum GuildProfilePermissionLevel {
  Admin = 1,
  Officer = 2,
  Member = 3,
  Visitor = 4
}

export class GuildProfilePermissionSet {
  private permissionsByGuild: { [id: number]: GuildProfilePermissionLevel };

  constructor() {
    this.permissionsByGuild = {};
  }

  public addPermission(guildProfileId: number, permission: GuildProfilePermissionLevel) {
    this.permissionsByGuild[guildProfileId] = permission;
  }

  public getPermission(guildProfileId: number): GuildProfilePermissionLevel | null {
    if (this.permissionsByGuild[guildProfileId] === undefined) {
      return null;
    }

    return this.permissionsByGuild[guildProfileId];
  }
}

class AuthKeys {
  public static get AccessToken() { return 'access_token'; }
  public static get UserName() { return 'username'; }
  public static get ExpiresAt() { return 'expires_at'; }
  public static get ProfilePermissions() { return 'profile_permissions'; }
}
