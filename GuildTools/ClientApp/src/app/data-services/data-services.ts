import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { GuildProfile } from './Models/GuildProfile';
import { GuildMember } from './Models/GuildMember';
import { GuildExists } from './Models/GuildExists';

@Injectable()
export class DataService {
 
  constructor(
    private router: Router,
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string) {
    
  }

  public GetGuildProfile(profileId: number, handleResult: (result: GuildProfile) => void): void {
    this.http.get(this.baseUrl + `api/data/guildProfile?id=${profileId}`).subscribe(
      success => {
        let profile = new GuildProfile();

        profile.GuildName = success["guildName"];
        profile.Realm = success["realm"];

        handleResult(profile);
      },
      error => {
        console.error(error);
      });
  }

  public GetGuildExists(guild: string, realm: string, handleResult: (result: GuildExists) => void): void {
    this.http.get(this.baseUrl + `api/data/guildExists?guild=${guild}&realm=${realm}`).subscribe(
      success => {
        let guildExists = new GuildExists();

        guildExists.Found = success["found"];
        guildExists.Realm = success["realm"];
        guildExists.Name = success["name"];

        handleResult(guildExists);
      },
      error => {
        console.error(error);
      });
  }

  public GetGuildMemberStats(guild: string, realm: string, handleResult: (result: GuildMember[]) => void): void {
    this.http.get(this.baseUrl + `api/data/getGuildMemberStats?guild=${guild}&realm=${realm}`).subscribe(
      success => {
        if (success) {
          let mappedResult = (success as Array<any>).map(r => {
            let member = new GuildMember();

            member.AchievementPoints = r["achievementPoints"];
            member.Class = r["class"];
            member.EquippedIlvl = r["equippedIlvl"];
            member.MaximumIlvl = r["maximumIlvl"];
            member.Level = r["level"];
            member.Name = r["name"];
            member.Realm = r["realm"];
            member.PetCount = r["petCount"];
            member.MountCount = r["mountCount"];
            member.GuildRank = r["guildRank"];
            member.Pvp2v2Rating = r["pvp2v2Rating"];
            member.Pvp3v3Rating = r["pvp3v3Rating"];
            member.PvpRbgRating = r["pvpRbgRating"];
            member.TotalHonorableKills = r["totalHonorableKills"];
            member.AzeriteLevel = r["azeriteLevel"];

            return member;
          });
          handleResult(mappedResult);
        }
        else {
          handleResult([]);
        }
      },
      error => {
        console.error(error);
      });
  }
}
