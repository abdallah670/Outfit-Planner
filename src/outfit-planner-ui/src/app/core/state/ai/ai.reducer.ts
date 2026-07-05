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
      // Dedup: If the last message is already an assistant message with the same
      // content and outfitSuggestions, do NOT append a duplicate.
      const lastMsg = state.messages[state.messages.length - 1];
      if (lastMsg?.role === 'assistant' && lastMsg.content === response.message) {
        return { ...state, isSending: false };
      }

      // response.id is the sessionId (BaseCommandResponse format)
      // response.data?.outfitSuggestions carries the outfit data
      const sessionId = response.id || (response as any).sessionId || (response as any).SessionId;
      const data = response.data || (response as any).Data;
      const rawSuggestions = data?.outfitSuggestions || data?.OutfitSuggestions || (response as any).outfitSuggestions || (response as any).OutfitSuggestions;
      
      // Normalize PascalCase from API to camelCase for the UI
      const outfitSuggestions = rawSuggestions?.map((s: any) => ({
        rank: s.rank ?? s.Rank,
        totalScore: s.totalScore ?? s.TotalScore ?? 0,
        scoreBreakdown: s.scoreBreakdown ?? s.ScoreBreakdown,
        items: (s.items || s.Items)?.map((item: any) => ({
          id: item.id ?? item.Id,
          name: item.name ?? item.Name ?? '',
          type: item.type ?? item.Type ?? '',
          imageUrl: item.imageUrl ?? item.ImageUrl ?? '',
          hexColor: item.hexColor ?? item.HexColor ?? '#ccc'
        })) ?? []
      })) ?? [];

      const uploadedImageUrls = response.uploadedImageUrls || (response as any).UploadedImageUrls;
      
      // Update the last user message with persisted image URLs if available
      const updatedMessages = state.messages.map((msg, idx) => {
        if (idx === state.messages.length - 1 && msg.role === 'user' && uploadedImageUrls && uploadedImageUrls.length > 0) {
          return { ...msg, images: uploadedImageUrls };
        }
        return msg;
      });
      
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
          ...updatedMessages,
          {
            id: crypto.randomUUID(),
            sessionId: sessionId,
            senderId: 'ai',
            content: response.message,
            images: [],
            role: 'assistant' as const,
            createdAt: new Date().toISOString(),
            outfitSuggestions,
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
      // The backend returns messages ordered by CreatedAt ASC.
      // Page 1 replaces the list; older pages are prepended.
      const hasMore = messages.length === pageSize;
      const parsedMessages = messages.map(m => {
        let outfitSuggestions;
        let data;
        // Images for user messages are stored as a JSON string in the Images DB column.
        // Assistant messages must NEVER show images (images belong to the user's bubble).
        let images: string[] = [];
        let metadata;
        try {
          const parsed = m.metadata ? JSON.parse(m.metadata) : null;
          // Only pull outfit suggestions and data from assistant message metadata.
          outfitSuggestions = parsed?.outfitSuggestions;
          data = parsed?.data;
          metadata = m.metadata;

          if (m.role === 'user') {
            // m.images may be a raw JSON string from the DB column (e.g. '["url1","url2"]')
            // or already a parsed array (depending on the serialiser). Cast through unknown
            // before the string check because the interface declares string[] but the runtime
            // value can arrive as a JSON string.
            const rawImages = m.images as unknown;
            if (Array.isArray(rawImages)) {
              images = rawImages as string[];
            } else if (typeof rawImages === 'string' && (rawImages as string).trim().startsWith('[')) {
              try { images = JSON.parse(rawImages as string); } catch { images = []; }
            }
          }
          // For assistant messages, images stays empty — they don't own any images.
        } catch {
          outfitSuggestions = undefined;
          data = undefined;
          images = m.role === 'user' && Array.isArray(m.images) ? m.images : [];
          metadata = m.metadata;
        }
        return { ...m, outfitSuggestions, data, images, metadata };
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

    on(AiActions.appendMessage, (state, { role, content, imagePreviews }) => ({
      ...state,
      messages: [
        ...state.messages,
        {
          id: crypto.randomUUID(),
          sessionId: state.currentSessionId || '',
          senderId: role === 'user' ? 'user' : 'ai',
          content,
          images: imagePreviews ?? [],
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

    on(AiActions.deleteSession, (state) => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(AiActions.deleteSessionSuccess, (state, { sessionId }) => {
      const newSessions = state.sessions.filter(s => s.id !== sessionId);
      const newCurrentSessionId = state.currentSessionId === sessionId ? null : state.currentSessionId;
      return {
        ...state,
        sessions: newSessions,
        currentSessionId: newCurrentSessionId,
        messages: state.currentSessionId === sessionId ? [] : state.messages,
        isLoading: false,
      };
    }),
    on(AiActions.deleteSessionFailure, (state, { error }) => ({
      ...state,
      isLoading: false,
      error,
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