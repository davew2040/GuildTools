import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../services/account-service';
import { BusyService } from '../shared-services/busy-service';
import { Observable } from 'rxjs';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { LoginResponse } from 'app/services/ServiceTypes/service-types';
import { StoredValuesService } from 'app/shared-services/stored-values';
import { IUserDetails } from './user-details';
(window as any).global = window;

export enum GuildProfilePermissionLevel {
  Admin = 1,
  Officer = 2,
  Member = 3,
  Visitor = 4
}

class AuthKeys {
  public static get UserName() { return 'username'; }
  public static get ExpiresAt() { return 'expires_at'; }
  public static get ProfilePermissions() { return 'profile_permissions'; }

  public static get UserDetails() { return 'user_details'; }
}

@Injectable()
export class AuthService {

  constructor(
    public router: Router,
    private accountService: AccountService,
    private busyService: BusyService,
    private errorService: ErrorReportingService,
    private valuesService: StoredValuesService) { }

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

  private handleLoginSuccess(authResult: LoginResponse): void {
    const accessToken = authResult.authenticationDetails['access_token'];

    const decoded = this.decodeTokenExpiration(accessToken);

    // Set the time that the access token will expire at
    const expiresAt = JSON.stringify((decoded.exp * 1000) + new Date().getTime());

    localStorage.setItem(StoredValuesService.AccessTokenKey, accessToken);
    localStorage.setItem(AuthKeys.ExpiresAt, expiresAt);

    const userName = authResult.authenticationDetails['email'];
    localStorage.setItem(AuthKeys.UserName, userName);

    const userDetails = {} as IUserDetails;

    userDetails.username = authResult.username;
    userDetails.email = authResult.email;
    userDetails.playerRegion = authResult.playerRegion;
    userDetails.playerName = authResult.playerName;
    userDetails.playerRealm = authResult.playerRealm;
    userDetails.guildName = authResult.guildName,
    userDetails.guildRealm = authResult.guildRealm;

    this.valuesService.userDetails = userDetails;
  }

  public getAccessToken(): string {
    return localStorage.getItem(StoredValuesService.AccessTokenKey);
  }

  public get username(): string {
    return localStorage.getItem(AuthKeys.UserName);
  }

  public logOut(): void {
    this.clearLocalStorage();
  }

  public get isAuthenticated(): boolean {
    // Check whether the current time is past the
    // access token's expiry time
    const expiresAt = JSON.parse(localStorage.getItem(AuthKeys.ExpiresAt) || '{}');
    return new Date().getTime() < expiresAt;
  }

  private clearLocalStorage(): void {
    localStorage.removeItem(StoredValuesService.AccessTokenKey);
    localStorage.removeItem(AuthKeys.ExpiresAt);
    localStorage.removeItem(AuthKeys.ProfilePermissions);
    this.valuesService.userDetails = null;
  }

  private decodeTokenExpiration(token: string): any {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace('-', '+').replace('_', '/');

    return JSON.parse(window.atob(base64));
  }
}
