import { Component, OnInit } from '@angular/core';
import { DataService } from '../services/data-services';
import { Router } from '@angular/router';
import { BlizzardRealms, BlizzardRegionDefinition } from '../data/blizzard-realms';
import { BusyService } from 'app/shared-services/busy-service';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { SelectedGuild } from 'app/models/selected-guild';
import { StoredValuesService } from 'app/shared-services/stored-values';
import { RoutePaths } from 'app/data/route-paths';
import { BlizzardService } from 'app/blizzard-services/blizzard-services';
import { FormGroup, FormControl } from '@angular/forms';

@Component({
  selector: 'app-guild-stats-launcher',
  templateUrl: './guild-stats-launcher.component.html',
  styleUrls: ['./guild-stats-launcher.component.css']
})
export class GuildStatsLauncherComponent implements OnInit {

  public get selectionBlizzard() { return 'blizzard'; }
  public get selectionRaiderIo() { return 'raiderio'; }

  public selectionForm = new FormGroup({
    statsType: new FormControl(this.selectionBlizzard)
  });

  constructor(
    public dataService: DataService,
    public router: Router) {
  }

  ngOnInit() {
  }

  handleGuildSelected(guild: SelectedGuild) {
    const formattedGuild = BlizzardService.FormatGuild(guild.name);
    const formattedRealm = BlizzardService.FormatRealm(guild.realm);

    const selectedType = this.selectionForm.get('statsType').value;

    if (selectedType === this.selectionBlizzard) {
      this.router.navigate(['/' + RoutePaths.GuildStats, guild.region.Name, formattedGuild, formattedRealm]);
    }
    else if (selectedType === this.selectionRaiderIo) {
      this.router.navigate(['/' + RoutePaths.RaiderIoStats, guild.region.Name, formattedGuild, formattedRealm]);
    }
  }
}
