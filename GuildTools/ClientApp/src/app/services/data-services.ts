import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/Rx';
import { AuthService } from '../auth/auth.service';
import { Realm, IRealm, IGuildProfile, GuildProfile, IGuildMember, GuildMember, GuildFound, IGuildFound } from './ServiceTypes/service-types';

@Injectable()
export class DataService {

  constructor(
    private router: Router,
    private http: HttpClient,
    private auth: AuthService,
    @Inject('BASE_URL') private baseUrl: string) {

  }

  public getGuildProfile(profileId: number): Observable<GuildProfile> {
    return this.http.get(this.baseUrl + `api/data/guildProfile?id=${profileId}`)
      .map(response => {
        return new GuildProfile(response as IGuildProfile);
      });
  }

  public getGuildExists(region: string, guild: string, realm: string): Observable<GuildFound> {
    return this.http.get(this.baseUrl + `api/data/guildExists?region=${region}&guild=${guild}&realm=${realm}`)
      .map(response => {
        const guildExists = new GuildFound(response as IGuildFound);

        return guildExists;
      });
  }

  public getGuildProfilesForUser(): Observable<Array<GuildProfile>> {
    const headers = this.getAuthorizeHeader();

    return this.http.get(this.baseUrl + `api/data/getGuildProfiles`, { headers: headers })
      .map(response => {
        return (response as Array<IGuildProfile>).map(i => new GuildProfile(i));
      });
  }

  public getRealms(regionName: string): Observable<Array<Realm>> {
    return this.http.get(this.baseUrl + `api/data/getRealms?region=${regionName}`)
      .map(response => {
        return (response as Array<IRealm>).map(i => new Realm(i));
      });
  }

  public getGuildMemberStats(region: string, guild: string, realm: string): Observable<Array<GuildMember>> {
    return this.http.get(this.baseUrl + `api/data/getGuildMemberStats?region=${region}&guild=${guild}&realm=${realm}`)
      .map(response  => {
        const mappedResult = (response as Array<IGuildMember>).map(i => new GuildMember(i));

        return mappedResult;
      });
  }

  public postPasswordReset(userId: string, token: string, newPassword: string): Observable<Object> {
    return this.http.post(this.getResetUrl(this.baseUrl),
      {
        userId: userId,
        token: token,
        newPassword: btoa(newPassword)
      }
    );
  }

  private getResetUrl(baseUrl: string) {
    return `${baseUrl}/api/account/resetpasswordtoken`;
  }

  private getAuthorizeHeader(): HttpHeaders {
    const authBearer = 'Bearer ' + this.auth.getAccessToken();

    const headers = new HttpHeaders().set('Authorization', authBearer);

    return headers;
  }
}
