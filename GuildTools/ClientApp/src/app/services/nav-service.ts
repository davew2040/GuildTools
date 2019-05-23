
export enum NavServiceLocation {
  None,
  Stats,
  AltTracker
}

export class NavService {
  public currentLocation = NavServiceLocation.None;
}
