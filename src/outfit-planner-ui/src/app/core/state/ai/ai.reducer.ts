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
    on(AiActions.sendMessageSuccess, (state, { response }) => {
      const isNewSession = !state.sessions?.some(s => s.id === response.sessionId);
      const newSessionObj = {
        id: response.sessionId,
        userId: 'temp', // This should be updated on reload, but keeps the sidebar happy
        title: 'New Conversation',
        status: 'Active',
        images: [],
        messageCount: 2,
        createdAt: new Date().toISOString(),
        lastActivityAt: new Date().toISOString()
      };
      
      const newSessions = isNewSession && response.sessionId 
        ? [newSessionObj, ...(state.sessions || [])]
        : state.sessions;
        
      return {
        ...state,
        currentSessionId: response.sessionId,
        sessions: newSessions,
        messages: [
          ...state.messages, // Keep the user message that was appended
          {
            id: crypto.randomUUID(),
            sessionId: response.sessionId,
            senderId: 'ai',
            content: response.message,
            images: [],
            role: 'assistant' as const,
            createdAt: new Date().toISOString()
          },
        ],
        isSending: false,
      };
    }),
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

    on(AiActions.selectSession, (state, { sessionId }) => ({
      ...state,
      currentSessionId: sessionId,
      messages: [],
      currentPage: 1,
      hasMoreMessages: false,
      isLoading: true,
      error: null,
    })),
    on(AiActions.loadMessages, (state) => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(AiActions.loadMessagesSuccess, (state, { messages, page, pageSize }) => {
      // The backend returns messages ordered by CreatedAt ASC, so we can just prepend or replace.
      // But wait! If we append page 1 to empty, we just set it.
      // If we prepend page 2, we spread new messages then old messages.
      const hasMore = messages.length === pageSize;
      const newMessages = page === 1 ? messages : [...messages, ...state.messages];
      
      return {
        ...state,
        messages: newMessages,
        currentPage: page,
        hasMoreMessages: hasMore,
        isLoading: false,
      };
    }),
    on(AiActions.loadMessagesFailure, (state, { error }) => ({
      ...state,
      isLoading: false,
      error,
    })),

    on(AiActions.appendMessage, (state, { role, content }) => ({
      ...state,
      messages: [
        ...state.messages,
        {
          id: crypto.randomUUID(),
          sessionId: state.currentSessionId || '',
          senderId: role === 'user' ? 'user' : 'ai',
          content,
          images: [],
          role,
          createdAt: new Date().toISOString()
        },
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
  selectCurrentPage,
  selectHasMoreMessages,
} = aiFeature;