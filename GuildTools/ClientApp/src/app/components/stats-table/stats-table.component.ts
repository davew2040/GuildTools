import { Component, OnInit, Input } from '@angular/core';
import { StatsTableDefinition } from './subtypes/stats-table-definition';
import { StatsTablePlayer } from './subtypes/stats-table-player';
import { BlizzardStatsPlayer } from './subtypes/blizzard-stats-player';
import { BlizzardService } from 'app/blizzard-services/blizzard-services';
import { WowService } from 'app/services/wow-service';

@Component({
  selector: 'app-stats-table',
  templateUrl: './stats-table.component.html',
  styleUrls: ['./stats-table.component.scss']
})
export class StatsTableComponent implements OnInit {

  @Input() tableDefinition: StatsTableDefinition;
  @Input() players: StatsTablePlayer[];
  @Input() showIncrementSize = 10;
  @Input() showGuild = false;

  private incrementShown = 1;

  constructor(private wowService: WowService) { }

  ngOnInit() {
  }

  public get getDisplayedPlayers(): Array<StatsTablePlayer> {
    return this.players
      .filter(this.tableDefinition.Filter)
      .sort(this.tableDefinition.Sorter)
      .slice(0, this.playersShown);
  }

  public get title(): string {
    return this.tableDefinition.Title;
  }

  public showMore(): void {
    if (!this.canShowMore) {
      return;
    }

    this.incrementShown++;
  }

  public showLess(): void {
    if (!this.canShowLess) {
      return;
    }

    this.incrementShown--;
  }

  public get canShowLess(): boolean {
    return this.incrementShown > 1;
  }

  public get canShowMore(): boolean {
    return this.playersShown < this.players.length;
  }

  public getValue(player: StatsTablePlayer): number {
    return this.tableDefinition.ValueGetter(player);
  }

  public getPlayerArmoryLink(player: StatsTablePlayer): string {
    const url = `https://worldofwarcraft.com/${this.getPlayerRegionUrlSegment(player.regionName)}/character/`
      + `${player.regionName.toLowerCase()}/${BlizzardService.FormatRealm(player.realmName)}/`
      + `${player.name}`;
    return url;
  }

  public getGuildArmoryLink(player: StatsTablePlayer): string {
    const url = `http://${player.regionName}.battle.net/wow/en/guild/${BlizzardService.FormatRealm(player.realmName)}/`
      + `${BlizzardService.FormatGuild(player.guildName)}/`;
    return url;
  }

  public getPlayerRegionUrlSegment(realm: string): string {
    if (realm.toLowerCase() === 'us') {
      return `en-us`;
    }
    else {
      return `en-gb`;
    }
  }

  mapClassToColor(player: StatsTablePlayer): string {
    return 'background-' + this.wowService.getClassTag(player.class);
  }

  private get playersShown(): number {
    return this.showIncrementSize * this.incrementShown;
  }
}
