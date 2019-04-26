import { Injectable } from '@angular/core';
import { Subject, BehaviorSubject } from 'rxjs';

@Injectable()
export class BusyService {

  private busyCounter = 0;
  public isBusySubscription: BehaviorSubject<boolean>;

  constructor() { 

    this.isBusySubscription = new BehaviorSubject<boolean>(false);
  }

  public get isBusy(): boolean {
    return this.busyCounter > 0;
  }

  public setBusy(): void {
    this.busyCounter++;
    if (this.busyCounter == 1){
      this.isBusySubscription.next(this.isBusy);
    }
  }

  public unsetBusy(): void {
    this.busyCounter--;

    if (this.busyCounter < 0) {
      throw new Error('Busy counter should not be decremented below zero!');
    }

    if (this.busyCounter == 0) {
      this.isBusySubscription.next(this.isBusy);
    }
  }
}
