import { StatsTablePlayer } from './stats-table-player';
import { StatsTableDefinition } from './stats-table-definition';
import { BlizzardStatsPlayer } from './blizzard-stats-player';

export class BlizzardPlayerStatsTableDefinition extends StatsTableDefinition {
  constructor() {
    super();
    this.NameDisplayer = (g) => {
      if (g instanceof BlizzardStatsPlayer) {
        return g.name;
      }
    };
  }

  Title: string;
  NameDisplayer: (g: StatsTablePlayer) => string;
  Sorter: (g1: StatsTablePlayer, g2: StatsTablePlayer) => number;
  Filter: (g: StatsTablePlayer) => boolean;
  ValueGetter: (g: StatsTablePlayer) => number;
}
