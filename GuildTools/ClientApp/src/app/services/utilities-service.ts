import { _MatListOptionMixinBase } from '@angular/material';

class IsMobileService {
  public Android(): boolean {
    return navigator.userAgent.match(/Android/i).length > 0;
  }

  public BlackBerry(): boolean {
    return navigator.userAgent.match(/BlackBerry/i).length > 0;
  }

  public iOS(): boolean {
    return navigator.userAgent.match(/iPhone|iPad|iPod/i).length > 0;
  }

  public Opera(): boolean {
    return navigator.userAgent.match(/Opera Mini/i).length > 0;
  }

  public Windows(): boolean {
    return navigator.userAgent.match(/IEMobile/i).length > 0 || navigator.userAgent.match(/WPDesktop/i).length > 0;
  }

  public Any(): boolean {
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
