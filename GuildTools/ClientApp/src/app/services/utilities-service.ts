import { _MatListOptionMixinBase } from '@angular/material';

class IsMobileService {
  public Android(): boolean {
    return navigator.userAgent.match(/Android/i) !== null;
  }

  public BlackBerry(): boolean {
    return navigator.userAgent.match(/BlackBerry/i) !== null;
  }

  public iOS(): boolean {
    return navigator.userAgent.match(/iPhone|iPad|iPod/i) !== null;
  }

  public Opera(): boolean {
    return navigator.userAgent.match(/Opera Mini/i) !== null;
  }

  public Windows(): boolean {
    return navigator.userAgent.match(/IEMobile/i) !== null || navigator.userAgent.match(/WPDesktop/i) !== null;
  }

  public AnyMobile(): boolean {
    return (this.Android() || this.BlackBerry() || this.iOS() || this.Opera() || this.Windows());
  }
}

export class UtilitiesService {
  private _isMobileService: IsMobileService;

  constructor() {
    this._isMobileService = new IsMobileService();
  }

  public get mobileService(): IsMobileService {
    return this._isMobileService;
  }
}
