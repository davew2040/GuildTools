import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router'
import { DataService } from '../services/data-services';
import { BlizzardService } from '../blizzard-services/blizzard-services';
import { GuildMemberStats } from '../services/ServiceTypes/service-types';
import { BusyService } from 'app/shared-services/busy-service';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';

enum GuildStatsStatus {
  Loading,
  GuildNotFound,
  DataNotReady,
  Ready
}

@Component({
  selector: 'app-guild-stats',
  templateUrl: './guild-stats.component.html',
  styleUrls: ['./guild-stats.component.css']
})
export class GuildStatsComponent implements OnInit {

  pageStatus: GuildStatsStatus = GuildStatsStatus.Loading;
  guildMembers: GuildMemberStats[] = [];
  guild: string;
  realm: string;
  region: string;
  prettyGuild: string;
  prettyRealm: string;
  guildExists: boolean = false;
  dataReady: boolean = false;
  statsTables: StatsTable[];

  constructor(
    public route: ActivatedRoute,
    public dataService: DataService,
    public blizzardService: BlizzardService,
    private busyService: BusyService,
    private errorService: ErrorReportingService) { }

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.realm = params['realm'];
      this.guild = params['guild'];
      this.region = params['region'];
    });

    this.busyService.setBusy()

    this.dataService.getGuildExists(this.region, this.guild, this.realm).subscribe(
      success => {
        this.busyService.unsetBusy();

        if (success !== null) {
          if (success.found === true) {
            this.prettyGuild = success.guildName;
            this.prettyRealm = success.realmName;

            this.loadGuildStats(this.region, this.realm, this.guild);
          }
          else {
            this.pageStatus = GuildStatsStatus.GuildNotFound;
          }
        }
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      });
  }

  populateStatsTableDefinitions(): void {
    this.statsTables = [];

    let equippedIlvl = new StatsTable(this.guildMembers);
    equippedIlvl.Title = "Equipped ILVL";
    equippedIlvl.NameDisplayer = (m) => {
      return m.equippedIlvl.toString();
    };
    equippedIlvl.Filter = (m => m.equippedIlvl < 500 && m.equippedIlvl > 0);
    equippedIlvl.Sorter = (g1, g2) => { return g2.equippedIlvl - g1.equippedIlvl; }
    this.statsTables.push(equippedIlvl);


    let maxIlvl = new StatsTable(this.guildMembers);
    maxIlvl.Title = "Max ILVL";
    maxIlvl.NameDisplayer = (m) => {
      return m.maximumIlvl.toString();
    };
    maxIlvl.Filter = (m => m.maximumIlvl < 500 && m.maximumIlvl > 0);
    maxIlvl.Sorter = (g1, g2) => { return g2.maximumIlvl - g1.maximumIlvl; }

    this.statsTables.push(maxIlvl);


    let achieves = new StatsTable(this.guildMembers);
    achieves.Title = "Achievement Points";
    achieves.NameDisplayer = (m) => {
      return m.achievementPoints.toString();
    };
    achieves.Sorter = (g1, g2) => { return g2.achievementPoints - g1.achievementPoints; }

    this.statsTables.push(achieves);


    let azerite = new StatsTable(this.guildMembers);
    azerite.Title = "Azerite Level";
    azerite.NameDisplayer = (m) => {
      return m.azeriteLevel.toString();
    };
    azerite.Sorter = (g1, g2) => { return g2.azeriteLevel - g1.azeriteLevel; }
    azerite.Filter = (g) => g.azeriteLevel > 0;

    this.statsTables.push(azerite);


    let mounts = new StatsTable(this.guildMembers);
    mounts.Title = "Mounts";
    mounts.NameDisplayer = (m) => {
      return m.mountCount.toString();
    };
    mounts.Sorter = (g1, g2) => { return g2.mountCount - g1.mountCount; }

    this.statsTables.push(mounts);


    let pets = new StatsTable(this.guildMembers);
    pets.Title = "Pets";
    pets.NameDisplayer = (m) => {
      return m.petCount.toString();
    };
    pets.Sorter = (g1, g2) => { return g2.petCount - g1.petCount; }

    this.statsTables.push(pets);


    let arena2v2 = new StatsTable(this.guildMembers);
    arena2v2.Title = "Arena 2v2";
    arena2v2.NameDisplayer = (m) => {
      return m.pvp2v2Rating.toString();
    };
    arena2v2.Sorter = (g1, g2) => { return g2.pvp2v2Rating - g1.pvp2v2Rating; }
    arena2v2.Filter = (g) => g.pvp2v2Rating > 0;

    this.statsTables.push(arena2v2);


    let arena3v3 = new StatsTable(this.guildMembers);
    arena3v3.Title = "Arena 3v3";
    arena3v3.NameDisplayer = (m) => {
      return m.pvp3v3Rating.toString();
    };
    arena3v3.Sorter = (g1, g2) => { return g2.pvp3v3Rating - g1.pvp3v3Rating; }
    arena3v3.Filter = (g) => g.pvp3v3Rating > 0;

    this.statsTables.push(arena3v3);


    let rbg = new StatsTable(this.guildMembers);
    rbg.Title = "Rated BG's";
    rbg.NameDisplayer = (m) => {
      return m.pvpRbgRating.toString();
    };
    rbg.Sorter = (g1, g2) => { return g2.pvpRbgRating - g1.pvpRbgRating; }
    rbg.Filter = (g) => g.pvpRbgRating > 0;

    this.statsTables.push(rbg);


    let hk = new StatsTable(this.guildMembers);
    hk.Title = "Honorable Kills";
    hk.NameDisplayer = (m) => {
      return m.totalHonorableKills.toString();
    };
    hk.Sorter = (g1, g2) => { return g2.totalHonorableKills - g1.totalHonorableKills; }
    hk.Filter = (g) => g.totalHonorableKills > 0;

    this.statsTables.push(hk);


    let guildRank = new StatsTable(this.guildMembers);
    guildRank.Title = "Guild Rank";
    guildRank.NameDisplayer = (m) => {
      return m.guildRank.toString();
    };
    guildRank.Sorter = (g1, g2) => { return g2.guildRank - g1.guildRank; }

    this.statsTables.push(guildRank);
  }

  loadGuildStats(region: string, realm: string, guild: string) {
    this.busyService.setBusy();

    this.dataService.getGuildMemberStats(region, guild, realm).subscribe(
      success => {
        this.busyService.unsetBusy();
        if (success && success.length > 0) {
          this.guildMembers = success;
          this.populateStatsTableDefinitions();
          this.pageStatus = GuildStatsStatus.Ready;
        }
        else {
          this.pageStatus = GuildStatsStatus.DataNotReady;
        }
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      });
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

  getPlayerArmoryLink(player: GuildMemberStats): string {
    let url = `https://worldofwarcraft.com/${this.getPlayerRegionUrlSegment(this.region)}/character/${BlizzardService.FormatRealm(player.realm)}/${player.name}`;
    return url;
  }

  getGuildArmoryLink(realm: string, guild: string): string {
    let url = `http://${this.region}.battle.net/wow/en/guild/${BlizzardService.FormatRealm(realm)}/${guild}/`;
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

  mapClassToColor(classIndex: number): string {
    switch (classIndex) {
      case 1:
        return "warrior";
      case 2:
        return "paladin";
      case 3:
        return "hunter";
      case 4:
        return "rogue";
      case 5:
        return "priest";
      case 6:
        return "dk";
      case 7:
        return "shaman";
      case 8:
        return "mage";
      case 9:
        return "warlock";
      case 10:
        return "monk";
      case 11:
        return "druid";
      case 12:
        return "dh";
      default:
        return "none";
    }
  }
}

class StatsTable {
  constructor(private members: GuildMemberStats[]) {
    this.RowsDisplayed = 10;
    this.Sorter = (g1, g2) => {
      if (g1 === g2) {
        return 0;
      }
      else if (g2 > g1) {
        return 1;
      }
      else {
        return 0;
      }
    };
    this.Filter = (g) => true;
  }

  RowsDisplayed: number;
  Title: string;
  NameDisplayer: (g: GuildMemberStats) => string;
  Sorter: (g1: GuildMemberStats, g2: GuildMemberStats) => number;
  Filter: (g: GuildMemberStats) => boolean;
  SortedMembers(): GuildMemberStats[] {
    return this.members
      .filter(g => this.Filter(g))
      .sort(this.Sorter)
      .slice(0, this.RowsDisplayed);
  };
  ShowMore(additional: number): void {
    this.RowsDisplayed += additional;
  }
}
