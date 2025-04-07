import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ToastService } from '../../../services/toast/toast.service';
import { BlobStorageService } from '../../../services/blob-storage/blob-storage.service';

@Component({
  selector: 'app-image',
  standalone: false,
  
  templateUrl: './image.component.html',
  styleUrl: './image.component.scss'
})
export class ImageComponent {
  preview?: string;

  @Input() src?: string;
  @Input() alt: string = 'Preview';
  @Input() width: string = '200';
  @Input() height: string = '200';
  @Input() editMode?: boolean;

  @Output() srcChange = new EventEmitter<string>();
  @Output() imageChange = new EventEmitter<File>();


  constructor(private blobStorageService: BlobStorageService) {}

  get imageSrc(): string | undefined {
    return this.preview || this.blobStorageService.getUrl(this.src ?? '');
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = () => {
      this.preview = reader.result as string

      this.imageChange.emit(file);
    };

    reader.readAsDataURL(file);
  }
}
