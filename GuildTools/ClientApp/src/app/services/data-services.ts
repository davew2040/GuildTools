import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { GuildProfile } from './Models/GuildProfile';
import { GuildMember } from './Models/GuildMember';
import { GuildExists } from './Models/GuildExists';
import { Observable } from 'rxjs/Observable';
import 'rxjs/Rx';
import { AuthService } from '../auth/auth.service';
import { Realm, IRealm } from './ServiceTypes/service-types';

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
        let profile = new GuildProfile();

        profile.guildName = response["guildName"];
        profile.realm = response["realm"];

        return profile;
      });
  }

  public getGuildExists(region: string, guild: string, realm: string): Observable<GuildExists> {
    return this.http.get(this.baseUrl + `api/data/guildExists?region=${region}&guild=${guild}&realm=${realm}`)
      .map(response => {
        const guildExists = new GuildExists();

        guildExists.Found = response["found"];
        guildExists.Realm = response["realm"];
        guildExists.Name = response["name"];

        return guildExists;
      });
  }

  public getMyGuildProfiles(): Observable<Array<GuildProfile>> {
    var authBearer = 'Bearer ' + this.auth.getAccessToken();

    const headers = new HttpHeaders().set('Authorization', authBearer);

    return this.http.get(this.baseUrl + `api/data/getGuildProfiles`, { headers: headers })
      .map(response => {
        return [];
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
        const mappedResult = (response as Array<GuildMember>).map(r => {
          let member = new GuildMember();

          member.AchievementPoints = r.AchievementPoints;
          member.Class = r.Class;
          member.EquippedIlvl = r.EquippedIlvl;
          member.MaximumIlvl = r.MaximumIlvl
          member.Level = r.Level;
          member.Name = r.Name;
          member.Realm = r.Realm;
          member.PetCount = r.PetCount;
          member.MountCount = r.MountCount;
          member.GuildRank = r.GuildRank;
          member.Pvp2v2Rating = r.Pvp2v2Rating
          member.Pvp3v3Rating = r.Pvp2v2Rating
          member.PvpRbgRating = r.PvpRbgRating
          member.TotalHonorableKills = r.TotalHonorableKills
          member.AzeriteLevel = r.AzeriteLevel
          member.RaiderIoMplusScore = r.RaiderIoMplusScore;

          return member;
        });

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
}
