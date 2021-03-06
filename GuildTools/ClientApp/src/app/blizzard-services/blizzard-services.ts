import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { StoredPlayer } from 'app/services/ServiceTypes/service-types';

@Injectable()
export class BlizzardService {

  private readonly accessTokenKey: string = "accessToken";
  private currentAccessToken: string;

  constructor(private router: Router, private http: HttpClient) {
    this.currentAccessToken = localStorage.getItem(this.accessTokenKey);
  }

  public static FormatRealm(name: string): string {
    name = name.trim();

    name = name.replace(/ /g, "-");
    name = name.replace(/`/g, "");

    return name.toLowerCase();
  }

  public static FormatGuild(name: string): string {
    name = name.trim();

    name = name.replace(/ /g, "-");
    name = name.replace(/`/g, "");

    return name;
  }

  public static GetGuildUrl(guildName: string, realm: string, region: string): string {
    realm = BlizzardService.FormatRealm(realm);
    guildName = BlizzardService.FormatGuild(guildName);

    return `http://${region}.battle.net/wow/en/guild/${realm}/${guildName}/`;
  }
}
