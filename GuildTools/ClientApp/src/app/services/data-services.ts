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
    PromoteAltToMain,
    CreateNewGuildProfile,
    StoredPlayer,
    GuildStatsResponse,
    IGuildStatsResponse,
    RequestStatsCompleteNotification,
    RaiderIoStatsResponse,
    IRaiderIoStatsResponse} from './ServiceTypes/service-types';
import { BlizzardRegionDefinition } from '../data/blizzard-realms';
import { NotificationRequestType } from 'app/data/notification-request-type';

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

  public getGuildMemberStats(region: string, guild: string, realm: string): Observable<GuildStatsResponse> {
    return this.http.get(this.baseUrl + `api/data/getGuildMemberStats?region=${region}&guild=${guild}&realm=${realm}`)
      .pipe(
        map(response  => {
          return new GuildStatsResponse(response as IGuildStatsResponse);
        }));
  }

  public getRaiderIoStats(region: string, guild: string, realm: string): Observable<RaiderIoStatsResponse> {
    return this.http.get(this.baseUrl + `api/data/getRaiderIoStats?region=${region}&guild=${guild}&realm=${realm}`)
      .pipe(
        map(response  => {
          return new RaiderIoStatsResponse(response as IRaiderIoStatsResponse);
        }));
  }

  public requestStatsCompleteNotification(
      email: string,
      region: string,
      guild: string,
      realm: string,
      requestType: NotificationRequestType): Observable<Object> {
    const body = new RequestStatsCompleteNotification();

    body.email = email;
    body.region = region;
    body.guild = guild;
    body.realm = realm;
    body.requestType = requestType;

    return this.http.post(this.baseUrl + `api/data/requestStatsCompleteNotification`, body);
  }

  public createNewGuildProfile(
      name: string,
      guild: string,
      realm: string,
      region: BlizzardRegionDefinition,
      isPublic: boolean): Observable<number> {
    const headers = this.getAuthorizeHeader();

    const body = new CreateNewGuildProfile();

    body.profileName = name;
    body.guildName = guild;
    body.guildRealmName = realm;
    body.isPublic = isPublic;
    body.regionName = region.Name;

    return this.http.post(this.baseUrl + `api/data/createGuildProfile`, body, { headers: headers })
        .pipe(
          map(response => response as number)
        );
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


  public addPlayerMainToProfile(player: StoredPlayer, profile: FullGuildProfile): Observable<PlayerMain> {
    const headers = this.getAuthorizeHeader();

    const input = new AddMainToProfile();

    input.profileId = profile.id;
    input.playerId = player.id;

    return this.http.post(this.baseUrl + `api/data/addMainToProfile`, input, { headers: headers })
      .pipe(
        map(response => {
          return new PlayerMain(response as IPlayerMain);
        }));
  }

  public addAltToMain(player: StoredPlayer, main: PlayerMain, profile: FullGuildProfile): Observable<PlayerAlt> {
    const headers = this.getAuthorizeHeader();

    const input = new AddAltToMain();

    input.profileId = profile.id;
    input.mainId = main.id;
    input.playerId = player.id;

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
