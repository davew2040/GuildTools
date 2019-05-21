import { StatsTablePlayer } from "./stats-table-player";
import { GuildMemberStats, RaiderIoStatsResponse, RaiderIoStats } from 'app/services/ServiceTypes/service-types';
import { WowClass } from 'app/services/wow-service';

export class RaiderIoStatsPlayer extends StatsTablePlayer {

  constructor(public player: RaiderIoStats) {
    super();
  }

  public get name(): string {
    return this.player.name;
  }

  public get realmName(): string {
    return this.player.realmName;
  }

  public get regionName(): string {
    return this.player.regionName;
  }

  public get guildName(): string {
    return this.player.guildName;
  }

  public get class(): WowClass {
    return this.player.class;
  }
}
