import { Component, OnInit, Input, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

export interface ConfirmationDialogData {
  title: string;
  confirmationText: string;
}

@Component({
  selector: 'app-confirmation-dialog',
  templateUrl: './confirmation-dialog.component.html',
  styleUrls: ['./confirmation-dialog.component.css']
})
export class ConfirmationDialogComponent implements OnInit {

  public title = 'Title';
  public confirmationText = 'Confirmation Text';

  constructor(
    public dialogRef: MatDialogRef<ConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmationDialogData)
  {
    this.title = data.title;
    this.confirmationText = data.confirmationText;
  }

  ngOnInit() {

  }

  handleCancelled() {
    this.dialogRef.close(false);
  }

  handleConfirmed() {
    this.dialogRef.close(true);
  }
}
