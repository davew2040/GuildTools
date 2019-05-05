import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { map,} from 'rxjs/operators';
import { AuthService } from '../auth/auth.service';
import {
    Realm,
    IRealm,
    IGuildProfile,
    GuildProfile,
    IGuildMemberStats,
    GuildMemberStats,
    GuildFound,
    IGuildFound,
    FullGuildProfile,
    BlizzardPlayer,
    PlayerMain,
    IPlayerMain,
    AddMainToProfile,
    PlayerAlt,
    AddAltToMain,
    IPlayerAlt,
    RemoveAltFromMain,
    RemoveMain,
    PendingAccessRequest,
    IPendingAccessRequest,
    ProfilePermissionByUser,
    IProfilePermissionByUser,
    FullProfilePermissions,
    IFullProfilePermissions,
    UpdatePermissionSet,
    PlayerFound,
    IPlayerFound,
    EditNotes,
    PromoteAltToMain} from './ServiceTypes/service-types';
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
      .pipe(
        map(response => {
          return new GuildProfile(response as IGuildProfile);
        }));
  }

  public getGuildExists(region: string, guild: string, realm: string): Observable<GuildFound> {
    return this.http.get(this.baseUrl + `api/data/guildExists?region=${region}&guild=${guild}&realm=${realm}`)
      .pipe(
        map(response => {
          const guildExists = new GuildFound(response as IGuildFound);

          return guildExists;
        }));
  }

  public getPlayerExists(region: string, playerName: string, realm: string): Observable<PlayerFound> {
    return this.http.get(this.baseUrl + `api/data/playerExists?region=${region}&playerName=${playerName}&realm=${realm}`)
      .pipe(
        map(response => {
          const guildExists = new PlayerFound(response as IPlayerFound);

          return guildExists;
        }));
  }

  public getGuildProfilesForUser(): Observable<Array<GuildProfile>> {
    const headers = this.getAuthorizeHeader();

    return this.http.get(this.baseUrl + `api/data/getGuildProfiles`, { headers: headers })
      .pipe(
        map(response => {
          return (response as Array<IGuildProfile>).map(i => new GuildProfile(i));
        }));
  }

  public getRealms(regionName: string): Observable<Array<Realm>> {
    return this.http.get(this.baseUrl + `api/data/getRealms?region=${regionName}`)
      .pipe(
        map(response => {
          return (response as Array<IRealm>).map(i => new Realm(i));
      }));
  }

  public getGuildMemberStats(region: string, guild: string, realm: string): Observable<Array<GuildMemberStats>> {
    return this.http.get(this.baseUrl + `api/data/getGuildMemberStats?region=${region}&guild=${guild}&realm=${realm}`)
      .pipe(
        map(response  => {
          const mappedResult = (response as Array<IGuildMemberStats>).map(i => new GuildMemberStats(i));

          return mappedResult;
        }));
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
    const headers = this.getAuthorizeHeader();

    return this.http.get(this.baseUrl + `api/data/getGuildProfile?profileId=${profileId}`, { headers: headers })
      .pipe(
        map(response  => {
          return new FullGuildProfile(response);
        }));
  }

  public getAccessRequests(profileId: number): Observable<Array<PendingAccessRequest>> {
    const headers = this.getAuthorizeHeader();

    return this.http.get(this.baseUrl + `api/data/getAccessRequests?profileId=${profileId}`, { headers: headers })
      .pipe(
        map(
          response => (response as Array<IPendingAccessRequest>)
            .map(i => new PendingAccessRequest(i))));
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
      .pipe(
        map(response => {
          return new PlayerMain(response as IPlayerMain);
        }));
  }

  public addAltToMain(player: BlizzardPlayer, main: PlayerMain, profile: FullGuildProfile): Observable<PlayerAlt> {
    const headers = this.getAuthorizeHeader();

    const input = new AddAltToMain();

    input.playerName = player.playerName;
    input.guildName = player.guildName;
    input.playerRealmName = player.realmName;
    input.guildRealmName = profile.realm.name;
    input.regionName = profile.region;
    input.profileId = profile.id;
    input.mainId = main.id;

    return this.http.post(this.baseUrl + `api/data/addAltToMain`, input, { headers: headers })
      .pipe(
        map(response => {
          return new PlayerAlt(response as IPlayerAlt);
        }));
  }

  public removeAltFromMain(main: PlayerMain, alt: PlayerAlt, profile: FullGuildProfile): Observable<Object> {
    const headers = this.getAuthorizeHeader();

    const input = new RemoveAltFromMain();

    input.mainId = main.id;
    input.altId = alt.id;
    input.profileId = profile.id;

    return this.http.post(this.baseUrl + `api/data/removeAltFromMain`, input, { headers: headers });
  }

  public removeMain(main: PlayerMain, profile: FullGuildProfile): Observable<Object> {
    const headers = this.getAuthorizeHeader();

    const input = new RemoveMain();

    input.mainId = main.id;
    input.profileId = profile.id;

    return this.http.post(this.baseUrl + `api/data/removeMain`, input, { headers: headers });
  }

  public promoteAltToMain(altId: number, profile: FullGuildProfile): Observable<PlayerMain> {
    const headers = this.getAuthorizeHeader();

    const input = new PromoteAltToMain();

    input.altId = altId;
    input.profileId = profile.id;

    return this.http.post(this.baseUrl + `api/data/promoteAltToMain`, input, { headers: headers })
      .pipe(
        map(response => new PlayerMain(response as IPlayerMain))
      );
  }

  public editPlayerNotes(main: PlayerMain, profile: FullGuildProfile, newNotes: string): Observable<Object> {
    const headers = this.getAuthorizeHeader();

    const input = new EditNotes();

    input.playerMainId = main.id;
    input.profileId = profile.id;
    input.newNotes = newNotes;

    return this.http.post(this.baseUrl + `api/data/editPlayerNotes`, input, { headers: headers });
  }

  public editOfficerNotes(main: PlayerMain, profile: FullGuildProfile, newNotes: string): Observable<Object> {
    const headers = this.getAuthorizeHeader();

    const input = new EditNotes();

    input.playerMainId = main.id;
    input.profileId = profile.id;
    input.newNotes = newNotes;

    return this.http.post(this.baseUrl + `api/data/editOfficerNotes`, input, { headers: headers });
  }

  public deleteProfile(profile: GuildProfile): Observable<Object> {
    const headers = this.getAuthorizeHeader();

    return this.http.delete(this.baseUrl + `api/data/deleteProfile?profileId=${profile.id}`, { headers: headers });
  }

  public requestProfileAccess(profileId: number): Observable<Object> {
    const headers = this.getAuthorizeHeader();

    return this.http.post(this.baseUrl + `api/data/AddAccessRequest?profileId=${profileId}`, null, { headers: headers });
  }

  public approveProfileAccessRequest(requestId: number): Observable<Object> {
    const headers = this.getAuthorizeHeader();

    return this.http.post(this.baseUrl + `api/data/approveAccessRequest?requestId=${requestId}`, null, { headers: headers });
  }

  public getAllProfilePermissions(profileId: number): Observable<FullProfilePermissions> {
    const headers = this.getAuthorizeHeader();

    return this.http.get(this.baseUrl + `api/data/getAllProfilePermissions?profileId=${profileId}`, { headers: headers })
      .pipe(
        map(response => new FullProfilePermissions(response as IFullProfilePermissions))
      );
  }

  public updatePermissions(permissionsSet: UpdatePermissionSet): Observable<Object> {
    const headers = this.getAuthorizeHeader();

    return this.http.post(this.baseUrl + `api/data/updatePermissions`, permissionsSet, { headers: headers });
  }

  private getAuthorizeHeader(): HttpHeaders {
    const authBearer = 'Bearer ' + this.auth.getAccessToken();

    const headers = new HttpHeaders().append('Authorization', authBearer);

    return headers;
  }
}
