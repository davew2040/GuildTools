import { Component, OnInit } from '@angular/core';
import { DataService } from '../../services/data-services';
import { Router } from '@angular/router';
import { BlizzardRealms, BlizzardRegionDefinition } from '../../data/blizzard-realms';
import { BusyService } from 'app/shared-services/busy-service';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { SelectedGuild } from 'app/models/selected-guild';
import { StoredValuesService } from 'app/shared-services/stored-values';
import { RoutePaths } from 'app/data/route-paths';
import { BlizzardService } from 'app/blizzard-services/blizzard-services';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { integerValidator } from 'app/forms/custom-validators/integer-validator';

@Component({
  selector: 'app-guild-stats-launcher',
  templateUrl: './guild-stats-launcher.component.html',
  styleUrls: ['./guild-stats-launcher.component.css']
})
export class GuildStatsLauncherComponent implements OnInit {

  public get selectionBlizzard() { return 'blizzard'; }
  public get selectionRaiderIo() { return 'raiderio'; }

  public byGuildForm = new FormGroup({
    statsType: new FormControl(this.selectionBlizzard)
  });

  public byProfileForm = new FormGroup({
    statsType: new FormControl(this.selectionBlizzard),
    profileId: new FormControl('', [Validators.required, integerValidator])
  });

  constructor(
    public dataService: DataService,
    public router: Router) {
  }

  ngOnInit() {
  }

  public handleGuildSelected(guild: SelectedGuild) {
    const formattedGuild = BlizzardService.FormatGuild(guild.name);
    const formattedRealm = BlizzardService.FormatRealm(guild.realm);

    const selectedType = this.byGuildForm.get('statsType').value;

    if (selectedType === this.selectionBlizzard) {
      this.router.navigate(['/' + RoutePaths.GuildStats, guild.region.Name, formattedGuild, formattedRealm]);
    }
    else if (selectedType === this.selectionRaiderIo) {
      this.router.navigate(['/' + RoutePaths.RaiderIoStats, guild.region.Name, formattedGuild, formattedRealm]);
    }
  }

  public viewProfileStats() {
    const profileId = this.byProfileForm.get('profileId').value;
    const selectedType = this.byProfileForm.get('statsType').value;

    if (selectedType === this.selectionBlizzard) {
      this.router.navigate(['/' + RoutePaths.ProfileStats, profileId]);
    }
    else if (selectedType === this.selectionRaiderIo) {
      this.router.navigate(['/' + RoutePaths.RaiderIoProfileStats, profileId]);
    }
  }

  public get profileSelectionIsValid(): boolean {
    return this.byProfileForm.valid;
  }
}
