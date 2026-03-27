import { Component, Inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { Store } from '@ngrx/store';
import { CalendarActions } from '../../../../core/state/calendar/calendar.actions';
import { Outfit, ClothingItem } from '../../../../domain/entities/outfit.entity';

// Occasion types must match backend OccasionType enum exactly
const OCCASION_TYPES = [
  'Casual',
  'BusinessCasual', 
  'Formal',
  'Athletic',
  'Social',
  'Work',
  'Date',
  'Travel'
] as const;

type OccasionTypeValue = typeof OCCASION_TYPES[number];
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { firstValueFrom } from 'rxjs';

export interface ScheduleOutfitModalData {
  date: Date;
  outfits: Outfit[];
  clothingItems?: ClothingItem[];
}

type CreateMode = 'photo' | 'items';

@Component({
  selector: 'app-schedule-outfit-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatTabsModule,
    MatIconModule,
  ],
  templateUrl: './schedule-outfit-modal.component.html',
  styleUrl: './schedule-outfit-modal.component.scss',
})
export class ScheduleOutfitModalComponent implements OnInit {
  scheduleForm!: FormGroup;
  createOutfitForm!: FormGroup;
  occasions = [...OCCASION_TYPES];
  
  // Tab management
  activeTab = signal<'existing' | 'create'>('existing');
  createMode = signal<CreateMode>('photo');
  
  // Existing outfit selection
  selectedOutfitId = signal<string | null>(null);
  
  // Photo upload
  selectedFile = signal<File | null>(null);
  photoPreviewUrl = signal<string | null>(null);
  isUploading = signal(false);
  
  // Clothing items selection for "build from items" mode
  selectedClothingItems = signal<ClothingItem[]>([]);
  
  // Filtered outfits for display
  filteredOutfits!: ReturnType<typeof signal<Outfit[]>>;
  searchQuery = signal('');

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ScheduleOutfitModalComponent>,
    private store: Store,
    private http: HttpClient,
    @Inject(MAT_DIALOG_DATA) public data: ScheduleOutfitModalData,
  ) {}

  ngOnInit(): void {
    this.filteredOutfits = signal<Outfit[]>(this.data.outfits);
    this.initForms();
  }

  private initForms(): void {
    // Form for scheduling existing outfit
    this.scheduleForm = this.fb.group({
      scheduledDate: [this.data.date, Validators.required],
      occasion: ['Casual', Validators.required],
      notes: [''],
    });

    // Form for creating new outfit
    this.createOutfitForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      occasion: ['Casual', Validators.required],
      season: ['AllSeason'],
      scheduledDate: [this.data.date, Validators.required],
      notes: [''],
    });
  }

  // Tab switching
  setTab(tab: 'existing' | 'create'): void {
    this.activeTab.set(tab);
    this.selectedOutfitId.set(null);
  }

  setCreateMode(mode: CreateMode): void {
    this.createMode.set(mode);
    // Reset selections when switching modes
    if (mode === 'photo') {
      this.selectedClothingItems.set([]);
    } else {
      this.selectedFile.set(null);
      this.photoPreviewUrl.set(null);
    }
  }

  // Outfit search/filter
  onSearchChange(query: string): void {
    this.searchQuery.set(query);
    if (!query.trim()) {
      this.filteredOutfits.set(this.data.outfits);
    } else {
      const lower = query.toLowerCase();
      this.filteredOutfits.set(
        this.data.outfits.filter(o => 
          o.name.toLowerCase().includes(lower) ||
          o.occasion?.toLowerCase().includes(lower)
        )
      );
    }
  }

  // Outfit selection
  selectOutfit(outfitId: string): void {
    this.selectedOutfitId.set(outfitId);
  }

  isOutfitSelected(outfitId: string): boolean {
    return this.selectedOutfitId() === outfitId;
  }

  // Photo upload handling
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validate file type
      const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
      if (!allowedTypes.includes(file.type)) {
        alert('Please select a valid image file (JPEG, PNG, or WebP)');
        return;
      }

      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        alert('File size must be less than 5MB');
        return;
      }

      this.selectedFile.set(file);
      
      // Create preview
      const reader = new FileReader();
      reader.onload = (e) => {
        this.photoPreviewUrl.set(e.target?.result as string);
      };
      reader.readAsDataURL(file);
    }
  }

  removePhoto(): void {
    this.selectedFile.set(null);
    this.photoPreviewUrl.set(null);
  }

  // Clothing item selection for "build from items" mode
  toggleClothingItem(item: ClothingItem): void {
    const current = this.selectedClothingItems();
    const exists = current.find(i => i.id === item.id);
    
    if (exists) {
      this.selectedClothingItems.set(current.filter(i => i.id !== item.id));
    } else {
      this.selectedClothingItems.set([...current, item]);
    }
  }

  isItemSelected(itemId: string): boolean {
    return this.selectedClothingItems().some(i => i.id === itemId);
  }

  // Submit handlers
  async onScheduleExisting(): Promise<void> {
    if (this.scheduleForm.invalid || !this.selectedOutfitId()) {
      this.markFormGroupTouched(this.scheduleForm);
      return;
    }

    const formValue = this.scheduleForm.value;
    
    this.store.dispatch(
      CalendarActions.scheduleOutfit({
        request: {
          outfitId: this.selectedOutfitId()!,
          scheduledDate: formValue.scheduledDate,
          occasion: formValue.occasion,
          notes: formValue.notes,
        },
      }),
    );

    this.dialogRef.close({ success: true, mode: 'existing' });
  }

  async onCreateAndSchedule(): Promise<void> {
    if (this.createOutfitForm.invalid) {
      this.markFormGroupTouched(this.createOutfitForm);
      return;
    }

    const formValue = this.createOutfitForm.value;

    if (this.createMode() === 'photo') {
      // Validate photo is selected
      if (!this.selectedFile()) {
        alert('Please select a photo for your outfit');
        return;
      }

      await this.createOutfitWithPhoto(formValue);
    } else {
      // Validate items are selected
      if (this.selectedClothingItems().length === 0) {
        alert('Please select at least one clothing item');
        return;
      }

      await this.createOutfitFromItems(formValue);
    }
  }

  private async createOutfitWithPhoto(formValue: any): Promise<void> {
    this.isUploading.set(true);
    
    try {
      const formData = new FormData();
      formData.append('name', formValue.name);
      formData.append('photo', this.selectedFile()!);
      
      if (formValue.occasion) {
        formData.append('occasion', formValue.occasion);
      }
      if (formValue.season) {
        formData.append('season', formValue.season);
      }

      const response = await firstValueFrom(
        this.http.post<{ id: string; name: string; imageUrl: string }>(
          `${environment.baseUrl}/outfits/with-photo`,
          formData
        )
      );

      // Now schedule the newly created outfit
      this.store.dispatch(
        CalendarActions.scheduleOutfit({
          request: {
            outfitId: response.id,
            scheduledDate: formValue.scheduledDate,
            occasion: formValue.occasion || undefined,
            notes: formValue.notes || undefined,
          },
        }),
      );

      this.dialogRef.close({ success: true, mode: 'create-photo', outfitId: response.id });
    } catch (error) {
      console.error('Failed to create outfit with photo:', error);
      alert('Failed to create outfit. Please try again.');
    } finally {
      this.isUploading.set(false);
    }
  }

  private async createOutfitFromItems(formValue: any): Promise<void> {
    this.isUploading.set(true);
    
    try {
      // Build items array for outfit creation
      const items = this.selectedClothingItems().map((item, index) => ({
        clothingItemId: item.id,
        role: 'Primary' as const,
        layeringOrder: index,
        isEssential: true,
      }));

      const createOutfitRequest = {
        name: formValue.name,
        occasion: formValue.occasion || 'Casual',
        season: formValue.season || 'Spring',
        weatherCondition: '',
        items,
      };

      const response = await firstValueFrom(
        this.http.post<{ id: string; name: string; imageUrl?: string }>(
          `${environment.baseUrl}/outfits`,
          createOutfitRequest
        )
      );

      // Now schedule the newly created outfit
      this.store.dispatch(
        CalendarActions.scheduleOutfit({
          request: {
            outfitId: response.id,
            scheduledDate: formValue.scheduledDate,
            occasion: formValue.occasion || undefined,
            notes: formValue.notes || undefined,
          },
        }),
      );

      this.dialogRef.close({ success: true, mode: 'create-items', outfitId: response.id });
    } catch (error) {
      console.error('Failed to create outfit from items:', error);
      alert('Failed to create outfit. Please try again.');
    } finally {
      this.isUploading.set(false);
    }
  }

  onCancel(): void {
    this.dialogRef.close({ success: false });
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach((control) => {
      control.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  // Helper getters for template
  get canScheduleExisting(): boolean {
    return this.scheduleForm.valid && !!this.selectedOutfitId();
  }

  get canCreateAndSchedule(): boolean {
    const formValid = this.createOutfitForm.valid;
    if (this.createMode() === 'photo') {
      return formValid && !!this.selectedFile() && !this.isUploading();
    } else {
      return formValid && this.selectedClothingItems().length > 0 && !this.isUploading();
    }
  }
}
