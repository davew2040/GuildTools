import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, NavigationStart } from '@angular/router';
import { DataService } from '../../services/data-services';
import { BlizzardService } from '../../blizzard-services/blizzard-services';
import {
   GuildMemberStats,
   BlizzardGuildStatsResponse,
   AggregatedProfileBlizzardStatsItem,
   FullGuildProfile } from '../../services/ServiceTypes/service-types';
import { BusyService } from 'app/shared-services/busy-service';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { WowService } from 'app/services/wow-service';
import { BlizzardPlayerStatsTableDefinition } from 'app/components/stats-table/subtypes/blizzard-player-stats-table-definition';
import { BlizzardStatsPlayer } from 'app/components/stats-table/subtypes/blizzard-stats-player';

enum GuildStatsStatus {
  Loading,
  GuildNotFound,
  DataNotReady,
  Ready
}

@Component({
  selector: 'app-guild-profile-stats',
  templateUrl: './guild-profile-stats.component.html',
  styleUrls: ['./guild-profile-stats.component.css']
})
export class GuildProfileStatsComponent implements OnInit {

  public pageStatus: GuildStatsStatus = GuildStatsStatus.Loading;
  public guildMembers: GuildMemberStats[] = [];
  public isCompleted = false;
  public profileId: number;
  public dataReady = false;
  public individualStatsResponses = new Array<BlizzardGuildStatsResponse>();
  public tableDefinitions = new Array<BlizzardPlayerStatsTableDefinition>();
  public profile: FullGuildProfile;

  private timerHandler: any;

  private get recheckTime() { return 4000; }

  constructor(
    public route: ActivatedRoute,
    public router: Router,
    public dataService: DataService,
    public blizzardService: BlizzardService,
    private busyService: BusyService,
    private errorService: ErrorReportingService) { }

  ngOnInit() {
    this.tableDefinitions = this.getStatsTableDefinitions();

    this.route.params.subscribe(params => {
      this.profileId = params['profileId'];
    });

    this.router.events.subscribe(event => {
      if (event instanceof NavigationStart) {
        clearTimeout(this.timerHandler);
      }
    });

    this.loadProfileStats(this.profileId);
  }

  public get getStatsTablePlayers(): Array<BlizzardStatsPlayer> {
    return this.guildMembers.map(x => new BlizzardStatsPlayer(x));
  }

  getStatsTableDefinitions(): Array<BlizzardPlayerStatsTableDefinition> {
    const statsTables = new Array<BlizzardPlayerStatsTableDefinition>();

    statsTables.push(this.getEquippedIlvlDefinition());
    statsTables.push(this.getMaxIlvlDefinition());
    statsTables.push(this.getAchievementsDefinition());
    statsTables.push(this.getAzeriteLevelDefinition());
    statsTables.push(this.getMountsDefinition());
    statsTables.push(this.getPetsDefinition());
    statsTables.push(this.getArena2v2Definition());
    statsTables.push(this.getArena3v3Definition());
    statsTables.push(this.getRbgDefinition());
    statsTables.push(this.getHonorKillsDefinition());

    return statsTables;
  }

  getEquippedIlvlDefinition(): BlizzardPlayerStatsTableDefinition {
    const equippedIlvl = new BlizzardPlayerStatsTableDefinition();
    equippedIlvl.Title = 'Equipped ILVL';
    equippedIlvl.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.equippedIlvl;
    };
    equippedIlvl.Filter = (p) => {
      const casted = p as BlizzardStatsPlayer;
      return casted.player.equippedIlvl < 500 && casted.player.equippedIlvl > 0;
    }
    return equippedIlvl;
  }

  getMaxIlvlDefinition(): BlizzardPlayerStatsTableDefinition {
    const maxIlvl = new BlizzardPlayerStatsTableDefinition();
    maxIlvl.Title = 'Max ILVL';
    maxIlvl.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.maximumIlvl;
    };
    maxIlvl.Filter = (p) => {
      const casted = p as BlizzardStatsPlayer;
      return casted.player.maximumIlvl < 500 && casted.player.maximumIlvl > 0;
    };

    return maxIlvl;
  }

  getAchievementsDefinition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Achievement Points';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.achievementPoints;
    };

    return definition;
  }

  getAzeriteLevelDefinition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Azerite Level';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.azeriteLevel;
    };
    definition.Filter = p => (p as BlizzardStatsPlayer).player.azeriteLevel > 0;

    return definition;
  }

  getMountsDefinition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Mounts';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.mountCount;
    };
    definition.Filter = p => !this.playerIsAlt(p as BlizzardStatsPlayer);

    return definition;
  }

  getPetsDefinition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Pets';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.petCount;
    };

    return definition;
  }

  getArena2v2Definition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Arena 2v2';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.pvp2v2Rating;
    };
    definition.Filter = (p) => (p as BlizzardStatsPlayer).player.pvp2v2Rating > 0;

    return definition;
  }

  getArena3v3Definition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Arena 3v3';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.pvp3v3Rating;
    };
    definition.Filter = (p) => (p as BlizzardStatsPlayer).player.pvp3v3Rating > 0;

    return definition;
  }

  getRbgDefinition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Rated BG\'s';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.pvpRbgRating;
    };
    definition.Filter = (p) => (p as BlizzardStatsPlayer).player.pvpRbgRating > 0;

    return definition;
  }

  getHonorKillsDefinition(): BlizzardPlayerStatsTableDefinition {
    const definition = new BlizzardPlayerStatsTableDefinition();

    definition.Title = 'Honorable Kills';
    definition.ValueGetter = (p) => {
      return (p as BlizzardStatsPlayer).player.totalHonorableKills;
    };
    definition.Filter = (p) => (p as BlizzardStatsPlayer).player.totalHonorableKills > 0;

    return definition;
  }

  loadProfileStats(profileId: number) {
    this.busyService.setBusy();

    this.dataService.getBlizzardProfileStats(profileId).subscribe(
      success => {
        this.busyService.unsetBusy();

        if (success.isCompleted) {
          this.isCompleted = true;
          this.guildMembers = success.values;
          this.pageStatus = GuildStatsStatus.Ready;
          this.profile = success.profile;
        }
        else {
          this.isCompleted = false;
          this.individualStatsResponses = success.individualGuildResponses;
          this.pageStatus = GuildStatsStatus.DataNotReady;
          this.timerHandler = setTimeout(
            () => {
              this.loadProfileStats(this.profileId);
            },
            this.recheckTime);
        }
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      });
  }

  public playerIsAlt(player: BlizzardStatsPlayer): boolean {
    if (!this.profile) {
      return false;
    }

    for (const main of this.profile.mains) {
      for (const alt of main.alts) {
        if (alt.player.realm.name.toLowerCase() === player.realmName.toLowerCase()
          && alt.player.name.toLowerCase() === player.name.toLowerCase()) {
            return true;
        }
      }
    }

    return false;
  }

  public getCompletionPercentage(percentage: number): string {
    return (percentage * 100).toFixed(0);
  }

  public getGuildName(statsResponse: AggregatedProfileBlizzardStatsItem): string {
    return statsResponse.guildName;
  }

  public getPositionInLine(statsResponse: AggregatedProfileBlizzardStatsItem): string {
    if (statsResponse.individualStats.isCompleted){
      return 'Complete';
    }

    const position = statsResponse.individualStats.positionInQueue;

    if (position !== null && position !== undefined){
      return statsResponse.individualStats.positionInQueue.toString();
    }

    return 'Active';
  }

  public getPercentComplete(statsResponse: AggregatedProfileBlizzardStatsItem): string {
    if (statsResponse.individualStats.isCompleted){
      return '100';
    }

    return this.getCompletionPercentage(statsResponse.individualStats.completionProgress);
  }

  pageLoading(): boolean {
    return this.pageStatus === GuildStatsStatus.Loading;
  }

  guildNotFound(): boolean {
    return this.pageStatus === GuildStatsStatus.GuildNotFound;
  }

  dataNotReady(): boolean {
    return this.pageStatus === GuildStatsStatus.DataNotReady;
  }

  pageReady(): boolean {
    return this.pageStatus === GuildStatsStatus.Ready;
  }

  getGuildArmoryLink(player: GuildMemberStats): string {
    const url = `http://${player.regionName}.battle.net/wow/en/guild/${BlizzardService.FormatRealm(player.realmName)}`
      + `/${BlizzardService.FormatGuild(player.guildName)}/`;
    return url;
  }

  getPlayerRegionUrlSegment(realm: string): string {
    if (realm.toLowerCase() === "us") {
      return "en-us";
    }
    else {
      return "en-gb";
    }
  }
}
