export class BlizzardRegionDefinition {
  private name: string;
  private code: number;

  constructor(name: string, code: number) {
    this.name = name;
    this.code = code;
  }

  public get Name() {
    return this.name;
  }

  public get Code() {
    return this.code;
  }
}

export class BlizzardRealms {
  public static US = new BlizzardRegionDefinition("US", 1);
  public static EU = new BlizzardRegionDefinition("EU", 2);

  public static get AllRealms(): Array<BlizzardRegionDefinition> {
    return [
      BlizzardRealms.US,
      BlizzardRealms.EU
    ];
  }
}
