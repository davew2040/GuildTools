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
  private savedRegionKey: string = "stats-launcher-region";
  savedRealm: string;
  savedGuild: string;
  savedRegion: string;
  selectedRegion: string;
  regions: string[];

  constructor(public dataService: DataService, public router: Router) {
  }

  ngOnInit() {
    this.regions = ['US', 'EU'];

    this.savedRealm = localStorage.getItem(this.savedRealmKey);
    this.savedGuild = localStorage.getItem(this.savedGuildKey);
    this.savedRegion = localStorage.getItem(this.savedRegionKey);

    this.selectedRegion = this.savedRegion || this.regions[0];
  }

  search(region: string, guild: string, realm: string): void {
    this.dataService.GetGuildExists(region, guild, realm, (result) => {
      if (result.Found) {
        this.guildNotFound = false;

        localStorage.setItem(this.savedRealmKey, realm);
        localStorage.setItem(this.savedGuildKey, guild);
        localStorage.setItem(this.savedRegionKey, region);

        this.router.navigate(['/guildstats', region, guild, realm]);
      }
      else {
        this.guildNotFound = true;
      }
    });
  }
}
