import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ToastService } from '../../../services/toast/toast.service';

@Component({
  selector: 'app-image',
  standalone: false,
  
  templateUrl: './image.component.html',
  styleUrl: './image.component.scss'
})
export class ImageComponent {
  @Input() editMode?: boolean;
  @Input() src?: string;
  @Input() alt?: string;
  @Output() imageChange = new EventEmitter<string>

  imageUrl?: string;

  private maxFileSize = 2 * 1024 * 1024;

  constructor(private toastService: ToastService) { }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];

      if (file.size > this.maxFileSize) {
        this.toastService.showError("File is too large! Please select an image smaller than 2MB", "File too Large")
        return;
      }

      const fileName = file.name
      this.imageUrl = `images/${fileName}`

      const reader = new FileReader();

      reader.onload = (e: any) => {
        this.src = reader.result as string; // Update the image preview

        const img = new Image();
        img.src = e.target.result;

        img.onload = () => {
          // Resize the image to 200x200 pixels
          const canvas = document.createElement('canvas');
          const ctx = canvas.getContext('2d');
          canvas.width = 200;
          canvas.height = 200;

          if (ctx) {
            ctx.drawImage(img, 0, 0, 200, 200);
            this.src = canvas.toDataURL('image/jpeg', 0.8); // Compress to JPEG (adjust quality if needed)
          }
        };

      };
      reader.readAsDataURL(file);
      this.imageChange.emit(this.imageUrl);
    }
  }
}
