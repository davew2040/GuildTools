import { Component, OnInit } from '@angular/core';
import { DataService } from '../data-services/data-services';
import { Router } from '@angular/router';

@Component({
  selector: 'app-guild-stats-launcher',
  templateUrl: './guild-stats-launcher.component.html',
  styleUrls: ['./guild-stats-launcher.component.css']
})
export class GuildStatsLauncherComponent implements OnInit {

  guildNotFound = false;
  private savedRealmKey: string = "stats-launcher-realm";
  private savedGuildKey: string = "stats-launcher-guild";
  savedRealm: string;
  savedGuild: string;

  constructor(public dataService: DataService, public router: Router) {
  }

  ngOnInit() {
    this.savedRealm = localStorage.getItem(this.savedRealmKey);
    this.savedGuild = localStorage.getItem(this.savedGuildKey);
  }

  search(guild: string, realm: string): void {
    this.dataService.GetGuildExists(guild, realm, (result) => {
      if (result.Found) {
        this.guildNotFound = false;

        localStorage.setItem(this.savedRealmKey, realm);
        localStorage.setItem(this.savedGuildKey, guild);

        this.router.navigate(['/guildstats', guild, realm]);
      }
      else {
        this.guildNotFound = true;
      }
    });
  }
}
