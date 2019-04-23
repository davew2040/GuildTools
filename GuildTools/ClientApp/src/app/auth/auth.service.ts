import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { r } from '@angular/core/src/render3';
(window as any).global = window;

@Injectable()
export class AuthService {

  constructor(public router: Router) {}

  public loggedIn(authResult: any): void {

    let decoded = this.decodeToken(authResult.access_token);

    // Set the time that the access token will expire at
    const expiresAt = JSON.stringify((decoded.exp * 1000) + new Date().getTime());

    localStorage.setItem('access_token', authResult.access_token);
    localStorage.setItem('id_token', authResult.id_token);
    localStorage.setItem('expires_at', expiresAt);
  }

  public handleAuthentication(): void {

  }

  public getAccessToken(): string {
    return localStorage.getItem('access_token');
  }

  private decodeToken(token: string): any {
      let base64Url = token.split('.')[1];
      let base64 = base64Url.replace('-', '+').replace('_', '/');

      return JSON.parse(window.atob(base64));
  }

  public logout(): void {
    // Remove tokens and expiry time from localStorage
    localStorage.removeItem('access_token');
    localStorage.removeItem('id_token');
    localStorage.removeItem('expires_at');
    // Go back to the home route
    this.router.navigate(['/']);
  }

  public isAuthenticated(): boolean {
    // Check whether the current time is past the
    // access token's expiry time
    const expiresAt = JSON.parse(localStorage.getItem('expires_at') || '{}');
    return new Date().getTime() < expiresAt;
  }

  public getPermissionLevelForProfile(profileId: number): GuildProfilePermissions {
    return GuildProfilePermissions.Admin;
  }
}

export enum GuildProfilePermissions {
  Admin = 1,
  Officer = 2,
  Member = 3,
  Visitor = 4
}
