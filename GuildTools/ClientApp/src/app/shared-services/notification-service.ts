import { Injectable, Output, EventEmitter } from '@angular/core';
import { Subject, BehaviorSubject } from 'rxjs';
import { MatSnackBar } from '@angular/material';


@Injectable()
export class NotificationService {

  constructor(private snackBar: MatSnackBar) {
  }

  public showNotification(text: string) {
    const snackBarRef = this.snackBar.open(text, 'OKAY', { duration: 3000});
    snackBarRef.onAction().subscribe(() => {
      snackBarRef.dismiss();
    });
  }
}
