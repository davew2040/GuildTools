import { StatsTablePlayer } from './stats-table-player';

export class StatsTableDefinition {
  constructor() {
    this.Sorter = (g1, g2) => {
      return this.SortCriteria(g2) - this.SortCriteria(g1);
    };
    this.Filter = (g) => true;
    this.NameDisplayer = g => g.name;
    this.ValueGetter = (g) => {
      throw new Error('Not implemented.');
    };
    this.SortCriteria = (g) => {
      return this.ValueGetter(g);
    };
  }

  Title: string;
  NameDisplayer: (g: StatsTablePlayer) => string;
  Sorter: (g1: StatsTablePlayer, g2: StatsTablePlayer) => number;
  SortCriteria: (g: StatsTablePlayer) => number;
  Filter: (g: StatsTablePlayer) => boolean;
  ValueGetter: (g: StatsTablePlayer) => number;
}
