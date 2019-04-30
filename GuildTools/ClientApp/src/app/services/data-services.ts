import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/Rx';
import { AuthService } from '../auth/auth.service';
import { Realm, IRealm, IGuildProfile, GuildProfile, IGuildMemberStats, GuildMemberStats, GuildFound, IGuildFound, FullGuildProfile, BlizzardPlayer, PlayerMain, IPlayerMain, AddMainToProfile } from './ServiceTypes/service-types';
import { BlizzardRegionDefinition } from '../data/blizzard-realms';

export enum WowClass {
   Warrior = 1,
   Paladin = 2,
   Hunter = 3,
   Rogue = 4,
   Priest = 5,
   DK = 6,
   Shaman = 7,
   Mage = 8,
   Warlock = 9,
   Monk = 10,
   Druid = 11,
   DH = 12
}

export class WowClassTags {
  constructor() {

  }
}

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

  public getGuildMemberStats(region: string, guild: string, realm: string): Observable<Array<GuildMemberStats>> {
    return this.http.get(this.baseUrl + `api/data/getGuildMemberStats?region=${region}&guild=${guild}&realm=${realm}`)
      .map(response  => {
        const mappedResult = (response as Array<IGuildMemberStats>).map(i => new GuildMemberStats(i));

        return mappedResult;
      });
  }

  public createNewGuildProfile(name: string, guild: string, realm: string, region: BlizzardRegionDefinition): Observable<Object> {
    const headers = this.getAuthorizeHeader();

    const params = new HttpParams()
      .append('name', name)
      .append('guild', guild)
      .append('realm', realm)
      .append('region', region.Name);

    return this.http.post(this.baseUrl + `api/data/createGuildProfile`, null, { headers: headers, params: params });
  }

  public getFullGuildProfile(profileId: number): Observable<FullGuildProfile> {
    return this.http.get(this.baseUrl + `api/data/getGuildProfile?profileId=${profileId}`)
      .map(response  => {
        return new FullGuildProfile(response);
      });
  }

  public addPlayerMainToProfile(player: BlizzardPlayer, profile: FullGuildProfile): Observable<PlayerMain> {
    const headers = this.getAuthorizeHeader();

    const input = new AddMainToProfile();

    input.playerName = player.playerName;
    input.guildName = player.guildName;
    input.playerRealmName = player.realmName;
    input.guildRealmName = profile.realm.name;
    input.regionName = profile.region;
    input.profileId = profile.id;

    return this.http.post(this.baseUrl + `api/data/addMainToProfile`, input, { headers: headers })
      .map(response => {
        return new PlayerMain(response as IPlayerMain);
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

    const headers = new HttpHeaders().append('Authorization', authBearer);

    return headers;
  }
}
