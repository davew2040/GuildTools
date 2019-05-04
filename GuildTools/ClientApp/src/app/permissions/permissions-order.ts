import { GuildProfilePermissionLevel } from 'app/auth/auth.service';

interface SortablePermissionLevel {
  level: GuildProfilePermissionLevel;
  order: number;
}

export class PermissionsOrder {

  private static _instance: PermissionsOrder = null;

  private static get staticInstance() {
    if (PermissionsOrder._instance === null){
      PermissionsOrder._instance = new PermissionsOrder();
    }

    return PermissionsOrder._instance;
  }

  private orderMap = new Map<GuildProfilePermissionLevel, number>();
  private labelMap = new Map<GuildProfilePermissionLevel, string>();

  constructor() {
    this.orderMap.set(GuildProfilePermissionLevel.Visitor, 1);
    this.orderMap.set(GuildProfilePermissionLevel.Member, 2);
    this.orderMap.set(GuildProfilePermissionLevel.Officer, 3);
    this.orderMap.set(GuildProfilePermissionLevel.Admin, 4);

    this.labelMap.set(GuildProfilePermissionLevel.Visitor, 'Visitor');
    this.labelMap.set(GuildProfilePermissionLevel.Member, 'Member');
    this.labelMap.set(GuildProfilePermissionLevel.Officer, 'Officer');
    this.labelMap.set(GuildProfilePermissionLevel.Admin, 'Admin');
  }

  public static GreaterThanOrEqual(target: GuildProfilePermissionLevel, compareAgainst: GuildProfilePermissionLevel): boolean {
    return PermissionsOrder.staticInstance.orderMap.get(target) >= PermissionsOrder.staticInstance.orderMap.get(compareAgainst);
  }

  public static LessThanOrEqual(target: GuildProfilePermissionLevel, compareAgainst: GuildProfilePermissionLevel): boolean {
    return PermissionsOrder.staticInstance.orderMap.get(target) <= PermissionsOrder.staticInstance.orderMap.get(compareAgainst);
  }

  public static GetOrderedPermissions(): Array<GuildProfilePermissionLevel> {
    const sortableArray = new Array<SortablePermissionLevel>();

    for (const [permission, order] of PermissionsOrder.staticInstance.orderMap.entries()) {
      sortableArray.push({
        level: permission,
        order: order
      });
    }

    sortableArray.sort((a, b) => {
      return a.order - b.order;
    });

    return sortableArray.map(x => x.level);
  }

  public static GetPermissionLabel(level: GuildProfilePermissionLevel): string {
    return PermissionsOrder.staticInstance.labelMap.get(level);
  }
}
