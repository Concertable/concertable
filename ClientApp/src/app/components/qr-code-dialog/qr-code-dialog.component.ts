import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-qr-code-dialog',
  standalone: false,
  templateUrl: './qr-code-dialog.component.html',
  styleUrl: './qr-code-dialog.component.scss'
})
export class QrCodeDialogComponent {
  qrCode?: string = '';

  constructor(private dialogRef: MatDialogRef<QrCodeDialogComponent>) {}

  onClose() {
    this.dialogRef.close();
  }
}
