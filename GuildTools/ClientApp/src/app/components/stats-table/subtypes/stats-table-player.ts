import { WowClass } from 'app/services/wow-service';

export abstract class StatsTablePlayer {
  public abstract get name(): string;
  public abstract get class(): WowClass;
  public abstract get realmName(): string;
  public abstract get regionName(): string;
  public abstract get guildName(): string;
}
