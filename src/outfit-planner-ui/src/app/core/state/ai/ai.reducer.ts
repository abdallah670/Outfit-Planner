import { createFeature, createReducer, on } from '@ngrx/store';
import { AiActions } from './ai.actions';
import { AiState, initialAiState } from './ai.state';

export const aiFeature = createFeature({
  name: 'ai',
  reducer: createReducer(
    initialAiState,

    on(AiActions.sendMessage, (state) => ({
      ...state,
      isSending: true,
      error: null,
    })),
    on(AiActions.sendMessageSuccess, (state, { response }) => ({
      ...state,
      currentSessionId: response.sessionId,
      messages: [
        ...state.messages,
        { id: crypto.randomUUID(), sessionId: response.sessionId, senderId: 'user', content: '', role: 'user', createdAt: new Date().toISOString() },
        { id: crypto.randomUUID(), sessionId: response.sessionId, senderId: response.sessionId, content: response.message, role: 'assistant', createdAt: new Date().toISOString() },
      ],
      isSending: false,
    })),
    on(AiActions.sendMessageFailure, (state, { error }) => ({
      ...state,
      isSending: false,
      error,
    })),

    on(AiActions.loadSessions, (state) => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(AiActions.loadSessionsSuccess, (state, { sessions }) => ({
      ...state,
      sessions,
      isLoading: false,
    })),
    on(AiActions.loadSessionsFailure, (state, { error }) => ({
      ...state,
      isLoading: false,
      error,
    })),

    on(AiActions.appendMessage, (state, { role, content }) => ({
      ...state,
      messages: [
        ...state.messages,
        { id: crypto.randomUUID(), sessionId: state.currentSessionId || '', senderId: role === 'user' ? 'user' : 'ai', content, role, createdAt: new Date().toISOString() },
      ],
    })),
    on(AiActions.clearCurrentSession, (state) => ({
      ...state,
      currentSessionId: null,
      messages: [],
    })),
  ),
});

export const {
  name,
  reducer,
  selectAiState,
  selectSessions,
  selectCurrentSessionId,
  selectMessages,
  selectIsSending,
  selectIsLoading,
  selectError,
} = aiFeature;