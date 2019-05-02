import { Injectable, Output, EventEmitter } from '@angular/core';
import { Subject, BehaviorSubject } from 'rxjs';
import { MatSnackBar } from '@angular/material';

const ErrorTypes = {
  Reportable: 'Reportable',
  Server: 'Server'
}

@Injectable()
export class ErrorReportingService {

  @Output() public errorReported = new EventEmitter<string>();

  constructor(private snackBar: MatSnackBar) {
  }

  public reportError(errorText: string) {
    this.errorReported.emit(errorText);

    const snackBarRef = this.snackBar.open(errorText, 'OKAY', { duration: 3000});
    snackBarRef.onAction().subscribe(() => {
      snackBarRef.dismiss();
    });
  }

  public reportApiError(userApiError: any) {
    const error = userApiError.error;
    let errorText = '';

    if (error.ErrorType === ErrorTypes.Reportable) {
      errorText = error.Message;
    }
    else {
      errorText = `${error.StatusCode} - An error occurred on the server.`;
    }

    this.errorReported.emit(errorText);

    const snackBarRef = this.snackBar.open(errorText, 'OKAY', { duration: 3000});
    snackBarRef.onAction().subscribe(() => {
      snackBarRef.dismiss();
    });
  }
}
