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
      // response.id is the sessionId (BaseCommandResponse format)
      // response.data?.outfitSuggestions carries the outfit data
      const sessionId = response.id || (response as any).sessionId;
      const outfitSuggestions = response.data?.outfitSuggestions || (response as any).outfitSuggestions;
      const suggestedActions = response.data?.suggestedActions || (response as any).suggestedActions;
      const data = response.data;
      
      const isNewSession = !state.sessions?.some(s => s.id === sessionId);
      const newSessionObj = {
        id: sessionId,
        userId: 'temp',
        title: 'New Conversation',
        status: 'Active',
        images: [],
        messageCount: 2,
        createdAt: new Date().toISOString(),
        lastActivityAt: new Date().toISOString()
      };
      
      const newSessions = isNewSession && sessionId 
        ? [newSessionObj, ...(state.sessions || [])]
        : state.sessions;
        
      return {
        ...state,
        currentSessionId: sessionId,
        sessions: newSessions,
        messages: [
          ...state.messages,
          {
            id: crypto.randomUUID(),
            sessionId: sessionId,
            senderId: 'ai',
            content: response.message,
            images: [],
            role: 'assistant' as const,
            createdAt: new Date().toISOString(),
            outfitSuggestions,
            suggestedActions,
            data,
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
      const parsedMessages = messages.map(m => {
        let outfitSuggestions;
        let suggestedActions;
        let data;
        let metadata;
        try {
          const parsed = m.metadata ? JSON.parse(m.metadata) : null;
          outfitSuggestions = parsed?.outfitSuggestions;
          suggestedActions = parsed?.suggestedActions;
          data = parsed?.data;
          metadata = m.metadata;
        } catch {
          outfitSuggestions = undefined;
          suggestedActions = undefined;
          data = undefined;
          metadata = m.metadata;
        }
        return { ...m, outfitSuggestions, suggestedActions, data, metadata };
      });
      const loadedIds = new Set(parsedMessages.map(m => m.id));
      const dedupedExisting = state.messages.filter(m => !loadedIds.has(m.id));
      const newMessages = page === 1 ? parsedMessages : [...parsedMessages, ...dedupedExisting];

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