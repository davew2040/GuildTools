import { Injectable } from '@angular/core';

@Injectable()
export class BusyService {

  private busyCounter = 0;

  constructor() { }

  public get isBusy(): boolean {
    return this.busyCounter > 0;
  }

  public setBusy(): void {
    this.busyCounter++;
  }

  public unsetBusy(): void {
    this.busyCounter--;

    if (this.busyCounter < 0) {
      throw new Error('Busy counter should not be decremented below zero!');
    }
  }
}
