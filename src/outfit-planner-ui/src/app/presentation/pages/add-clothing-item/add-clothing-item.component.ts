import { Component, inject, signal, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { WardrobeState } from '../../../core/state/wardrobe/wardrobe.reducer';
import { WardrobeActions } from '../../../core/state/wardrobe/wardrobe.actions';
import { selectWardrobeLoading } from '../../../core/state/wardrobe/wardrobe.selectors';
import Swal from 'sweetalert2';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { TitleCasePipe } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { Signal } from '@angular/core';
import { selectSelectedItem } from '../../../core/state/wardrobe/wardrobe.selectors';
import { ClothingItem } from '../../../domain/entities/clothing-item.entity';

@Component({
  selector: 'app-add-clothing-item',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: './add-clothing-item.component.html',
  styleUrl: './add-clothing-item.component.scss',
})
export class AddClothingItemComponent implements OnInit {
  private fb = inject(FormBuilder);
  private store = inject(Store);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  editModeId = signal<string | null>(null);
  itemToEdit: Signal<ClothingItem | null> = toSignal(this.store.select(selectSelectedItem), {
    initialValue: null,
  });

  imagePreview = signal<string | null>(null);
  selectedFile: File | null = null;

  constructor() {
    effect(() => {
      const item = this.itemToEdit();
      if (this.editModeId() && item && item.id === this.editModeId()) {
        this.clothingForm.patchValue({
          name: item.name || '',
          type: item.type || '',
          category: item.category || '',
          brand: item.brand || '',
          primaryColor: item.primaryColor || '#3b82f6',
          description: item.description || '',
          purchasePrice: item.purchasePrice || null,
          size: item.size || '',
          condition: item.condition || 'excellent',
          fabric: item.fabric || 'Cotton',
          currency: item.currency || 'USD',
        });
        if (item.imageUrl) {
          this.imagePreview.set(item.imageUrl);
        }
      }
    });
  }

  clothingForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    type: ['', Validators.required],
    category: ['', Validators.required],
    brand: [''],
    primaryColor: ['#3b82f6', Validators.required],
    description: [''],
    purchasePrice: [null],
    size: [''],
    condition: ['excellent', Validators.required],
    fabric: ['Cotton', Validators.required],
    currency: ['USD', Validators.required],
  });

  fabricTypes = [
    'Cotton',
    'Polyester',
    'Wool',
    'Silk',
    'Linen',
    'Leather',
    'Denim',
    'Nylon',
    'Spandex',
    'Rayon',
    'Other',
  ];
  conditions = ['new', 'excellent', 'good', 'fair', 'poor'];
  clothingTypes = [
    'Top',
    'Bottom',
    'Dress',
    'Outerwear',
    'Footwear',
    'Accessory',
    'Undergarment',
    'Swimwear',
    'Activewear',
  ];
  categories = ['Casual', 'Formal', 'Sport', 'Business', 'Work', 'Social', 'Date', 'Travel'];

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.editModeId.set(id);
      this.store.dispatch(WardrobeActions.loadClothingItemById({ id }));
    }
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview.set(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  }

  onColorChange(event: any) {
    this.clothingForm.patchValue({ primaryColor: event.target.value });
  }

  onSubmit() {
    if (this.clothingForm.valid) {
      if (this.editModeId()) {
        this.store.dispatch(
          WardrobeActions.updateClothingItem({
            id: this.editModeId()!,
            item: this.clothingForm.value,
            image: this.selectedFile || undefined,
          }),
        );
      } else {
        this.store.dispatch(
          WardrobeActions.createClothingItem({
            item: this.clothingForm.value,
            image: this.selectedFile || undefined,
          }),
        );
      }
    } else {
      this.clothingForm.markAllAsTouched();
      Swal.fire({
        title: 'Missing Required Fields',
        text: 'Please fill out all required fields before saving.',
        icon: 'warning',
        background: '#ffffff',
        color: '#2D3436',
        confirmButtonColor: '#F8B4C4',
      });
    }
  }
}
