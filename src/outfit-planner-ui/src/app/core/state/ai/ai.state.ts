import { ChatSession, ChatMessage } from '../../../domain/entities/ai.entity';

export interface AiState {
  sessions: ChatSession[];
  currentSessionId: string | null;
  messages: ChatMessage[];
  images: File[];
  currentPage: number;
  hasMoreMessages: boolean;
  isSending: boolean;
  isLoading: boolean;
  error: string | null;
}

export const initialAiState: AiState = {
  sessions: [],
  currentSessionId: null,
  messages: [],
  images: [],
  currentPage: 1,
  hasMoreMessages: false,
  isSending: false,
  isLoading: false,
  error: null,
};