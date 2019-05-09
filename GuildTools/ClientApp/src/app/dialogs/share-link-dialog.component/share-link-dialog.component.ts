import { Component, OnInit, Input, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';


export interface ShareLinkDialogData {
  url: string;
}

@Component({
  selector: 'app-share-link-dialog',
  templateUrl: './share-link-dialog.component.html',
  styleUrls: ['./share-link-dialog.component.css']
})
export class ShareLinkDialogComponent implements OnInit {
  @Input() url: string;

  constructor(
    public dialogRef: MatDialogRef<ShareLinkDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ShareLinkDialogData) {
      this.url = data.url;
    }

  ngOnInit() {
    const copyTextarea = document.querySelector('#copier') as HTMLInputElement;
    copyTextarea.focus();
    copyTextarea.select();
  }

  close() {
    this.dialogRef.close(null);
  }

  public copyToClipboard() {
    const copyTextarea = document.querySelector('#copier') as HTMLInputElement;
    copyTextarea.focus();
    copyTextarea.select();
    document.execCommand('copy');
    this.dialogRef.close();
  }
}
