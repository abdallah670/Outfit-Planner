import { Component, OnInit, inject, ChangeDetectorRef, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, take } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../../environments/environment';
import { AiActions } from '../../../../core/state/ai/ai.actions';
import { selectMessages, selectIsSending, selectSessions, selectCurrentSessionId, selectHasMoreMessages, selectCurrentPage } from '../../../../core/state/ai/ai.reducer';
import { selectUser } from '../../../../core/state/auth/auth.selectors';
import { selectUserProfile } from '../../../../core/state/user/user.selectors';
import { selectAllItems } from '../../../../core/state/wardrobe/wardrobe.selectors';
import { selectCurrentWeather, selectWeatherLoading } from '../../../../core/state/weather/weather.selectors';
import { WeatherActions } from '../../../../core/state/weather/weather.actions';
import { ChatMessage, OutfitSuggestion } from '../../../../domain/entities/ai.entity';
import { ClothingItem } from '../../../../domain/entities/clothing-item.entity';
import { WardrobeHealthComponent } from '../../../components/shared/wardrobe-health/wardrobe-health.component';
import { WeatherDisplayComponent } from '../../../components/shared/weather-display/weather-display.component';
import { Weather } from '../../../../domain/entities/weather.entity';

interface WeatherTip {
  icon: string;
  text: string;
}

@Component({
  selector: 'app-ai-assistant',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, WardrobeHealthComponent, WeatherDisplayComponent],
  templateUrl: './ai-assistant.component.html',
  styleUrls: ['./ai-assistant.component.scss'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class AiAssistantComponent implements OnInit {
  private store = inject(Store);
  private cdRef = inject(ChangeDetectorRef);

  messages$: Observable<ChatMessage[]> = this.store.select(selectMessages);
  isSending$: Observable<boolean> = this.store.select(selectIsSending);
  sessions$ = this.store.select(selectSessions);
  currentSessionId$: Observable<string | null> = this.store.select(selectCurrentSessionId);
  hasMoreMessages$: Observable<boolean> = this.store.select(selectHasMoreMessages);
  currentPage$: Observable<number> = this.store.select(selectCurrentPage);
  user$ = this.store.select(selectUser);
  profile$ = this.store.select(selectUserProfile);
  lastMessage$: Observable<ChatMessage | undefined> = this.messages$.pipe(
    map(messages => messages.length > 0 ? messages[messages.length - 1] : undefined)
  );
  weather$: Observable<Weather | null> = this.store.select(selectCurrentWeather);
  weatherLoading$: Observable<boolean> = this.store.select(selectWeatherLoading);

  wardrobeItems$: Observable<ClothingItem[]> = this.store.select(selectAllItems);

  tips$: Observable<WeatherTip[]> = this.weather$.pipe(
    map(weather => this.getWeatherTips(weather?.condition || ''))
  );

  userMessage = '';
  attachedFiles: { file: File, preview: string }[] = [];
  quickSuggestions = ['Date night?', 'Casual Friday', 'Beach trip', "What's missing?"];

  ngOnInit() {
    this.store.dispatch(AiActions.loadSessions({}));
    this.loadWeather();
  }

  resolveImageUrl(url: string): string;
  resolveImageUrl(url: File): string;
  resolveImageUrl(url: string | File): string {
    if (!url) return '';
    const urlStr = typeof url === 'string' ? url : url.name;
    return urlStr.startsWith('http') || urlStr.startsWith('data:') ? urlStr : `${environment.resourceBaseUrl}/${urlStr}`;
  }

  private loadWeather(): void {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          this.store.dispatch(
            WeatherActions.loadCurrentWeather({
              lat: position.coords.latitude,
              lon: position.coords.longitude,
            })
          );
        },
        () => {
          this.store.dispatch(WeatherActions.loadCurrentWeather({ city: 'Cairo' }));
        }
      );
    } else {
      this.store.dispatch(WeatherActions.loadCurrentWeather({ city: 'Cairo' }));
    }
  }

  onFileSelected(event: Event) {
    const files = (event.target as HTMLInputElement).files;
    if (files) {
      const remainingSlots = 6 - this.attachedFiles.length;
      const filesToAdd = Array.from(files).slice(0, remainingSlots);
      
      filesToAdd.forEach(file => {
        const reader = new FileReader();
        reader.onload = () => {
          this.attachedFiles.push({
            file: file,
            preview: reader.result as string
          });
          this.cdRef.detectChanges();
        };
        reader.readAsDataURL(file);
      });
    }
    // Clear input so the same file can be selected again if needed
    (event.target as HTMLInputElement).value = '';
  }

  removeImage(index: number) {
    this.attachedFiles.splice(index, 1);
  }

  sendMessage() {
    const msg = this.userMessage.trim();
    if (!msg && this.attachedFiles.length === 0) return;

    const filesToSend = this.attachedFiles.map(f => f.file);
    const previewsToSend = this.attachedFiles.map(f => f.preview);

    // Get the current session ID and dispatch message with it
    this.store.select(selectCurrentSessionId).pipe(take(1)).subscribe(sid => {
      this.store.dispatch(AiActions.appendMessage({ role: 'user', content: msg || `Attached ${filesToSend.length} image(s)`, imagePreviews: previewsToSend }));
      this.store.dispatch(AiActions.sendMessage({ message: msg, sessionId: sid ?? undefined, images: filesToSend }));
      this.attachedFiles = [];
    });
    this.userMessage = '';
  }

  sendQuickSuggestion(suggestion: string) {
    this.userMessage = suggestion;
    this.sendMessage();
  }

  executeAction(action: string, outfitSuggestion?: OutfitSuggestion) {
  this.store.select(selectCurrentSessionId).pipe(take(1)).subscribe(sessionId => {
    this.store.dispatch(AiActions.appendMessage({ role: 'user', content: action }));
    this.store.dispatch(AiActions.sendMessage({
      message: action,
      sessionId: sessionId ?? undefined,
      outfitSuggestion: outfitSuggestion  // ADD this
      ,
      clothingItemIds: outfitSuggestion?.items.map(item => item.id),
    }));
  });
 }

  newSession() {
    this.store.dispatch(AiActions.clearCurrentSession({ userId: '' }));
  }

  selectSession(id: string) {
    this.store.dispatch(AiActions.selectSession({ sessionId: id }));
  }

  showDeleteModal = false;
  deleteSessionId: string | null = null;

  openDeleteModal(sessionId: string) {
    this.deleteSessionId = sessionId;
    this.showDeleteModal = true;
  }

  cancelDelete() {
    this.showDeleteModal = false;
    this.deleteSessionId = null;
  }

  confirmDelete() {
    if (this.deleteSessionId) {
      this.store.dispatch(AiActions.deleteSession({ sessionId: this.deleteSessionId }));
    }
    this.cancelDelete();
  }

  loadMoreMessages() {
    this.store.select(selectCurrentSessionId).pipe(take(1)).subscribe(sessionId => {
      if (sessionId) {
        this.store.select(selectCurrentPage).pipe(take(1)).subscribe(page => {
          this.store.dispatch(AiActions.loadMessages({ sessionId, page: page + 1, pageSize: 20 }));
        });
      }
    });
  }

  trackById(_index: number, item: ChatMessage) {
    return item.id;
  }

  getActionIcon(action: string): string {
    const a = action.toLowerCase();
    if (a.includes('save')) return 'lucide:bookmark';
    if (a.includes('wear') || a.includes('event')) return 'lucide:calendar';
    if (a.includes('outfit')) return 'lucide:shirt';
    if (a.includes('share') || a.includes('social')) return 'lucide:share-2';
    if (a.includes('like') || a.includes('favorite')) return 'lucide:heart';
    return 'lucide:check';
  }

  shouldShowAction(action: string, suggestionCount: number): boolean {
    const a = action.toLowerCase();
    if (a.includes('save') && suggestionCount === 1) return false;
    return true;
  }


  isString(value: any): boolean {
    return typeof value === 'string';
  }
  

  private getWeatherTips(condition: string): WeatherTip[] {
    const c = condition.toLowerCase();
    if (c.includes('rain') || c.includes('drizzle')) {
      return [
        { icon: 'lucide:droplets', text: 'Water-proof your leather shoes before heading out.' },
        { icon: 'lucide:wind', text: 'Bring a compact umbrella — scattered showers expected.' },
        { icon: 'lucide:thermometer', text: 'Layer up with a waterproof trench coat.' },
      ];
    }
    if (c.includes('cloud')) {
      return [
        { icon: 'lucide:droplets', text: 'Clouds often precede rain — pack a light jacket.' },
        { icon: 'lucide:wind', text: 'Light winds forecasted — fabrics with airflow work well.' },
        { icon: 'lucide:thermometer', text: 'Temperatures are mild — layer a light cardigan.' },
      ];
    }
    if (c.includes('sun') || c.includes('clear')) {
      return [
        { icon: 'lucide:sun', text: 'UV is high — consider a wide-brim hat or sunglasses.' },
        { icon: 'lucide:droplets', text: 'Choose breathable fabrics like linen or cotton.' },
        { icon: 'lucide:thermometer', text: 'Stay cool — shorts and open-knit tops work great.' },
      ];
    }
    return [
      { icon: 'lucide:droplets', text: 'Check your wardrobe for weather-appropriate layers.' },
      { icon: 'lucide:wind', text: 'Consider the wind chill factor when choosing outerwear.' },
      { icon: 'lucide:thermometer', text: 'Layer your outfit to adapt to changing temperatures.' },
    ];
  }
}