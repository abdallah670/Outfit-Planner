import { createActionGroup, props } from '@ngrx/store';
import { ChatSession, ChatMessage, ChatResponse, OutfitSuggestion } from '../../../domain/entities/ai.entity';

export const AiActions = createActionGroup({
  source: 'ai',
  events: {
    'Send Message': props<{ message: string; sessionId?: string; images?: File[], 
       outfitSuggestion?: OutfitSuggestion; clothingItemIds?: string[]; }>(),
    'Send Message Success': props<{ response: ChatResponse }>(),
    'Send Message Failure': props<{ error: string }>(),

    'Load Sessions': props<{ userId?: string }>(),
    'Load Sessions Success': props<{ sessions: ChatSession[] }>(),
    'Load Sessions Failure': props<{ error: string }>(),

    'Select Session': props<{ sessionId: string }>(),
    'Load Messages': props<{ sessionId: string; page?: number; pageSize?: number }>(),
    'Load Messages Success': props<{ messages: ChatMessage[]; page: number; pageSize: number }>(),
    'Load Messages Failure': props<{ error: string }>(),

    'Append Message': props<{ role: 'user' | 'assistant'; content: string; imagePreviews?: string[] }>(),
    'Clear Current Session': props<{ userId: string }>(),

    'Delete Session': props<{ sessionId: string }>(),
    'Delete Session Success': props<{ sessionId: string }>(),
    'Delete Session Failure': props<{ error: string }>(),
  },
});
