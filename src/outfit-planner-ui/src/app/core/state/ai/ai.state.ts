import { ChatSession, ChatMessage } from '../../../domain/entities/ai.entity';

export interface AiState {
  sessions: ChatSession[];
  currentSessionId: string | null;
  messages: ChatMessage[];
  isSending: boolean;
  isLoading: boolean;
  error: string | null;
}

export const initialAiState: AiState = {
  sessions: [],
  currentSessionId: null,
  messages: [],
  isSending: false,
  isLoading: false,
  error: null,
};