import { createActionGroup, props } from '@ngrx/store';
import { ChatSession, ChatMessage, ChatResponse } from '../../../domain/entities/ai.entity';

export const AiActions = createActionGroup({
  source: 'ai',
  events: {
    'Send Message': props<{ message: string; sessionId?: string; images?: File[] }>(),
    'Send Message Success': props<{ response: ChatResponse }>(),
    'Send Message Failure': props<{ error: string }>(),

    'Load Sessions': props<{ userId?: string }>(),
    'Load Sessions Success': props<{ sessions: ChatSession[] }>(),
    'Load Sessions Failure': props<{ error: string }>(),

    'Append Message': props<{ role: 'user' | 'assistant'; content: string }>(),
    'Clear Current Session': props<{ userId: string }>(),
  },
});