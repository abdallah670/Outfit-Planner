export interface ChatSession {
  id: string;
  userId: string;
  title: string;
  status: string;
  messageCount: number;
  lastActivityAt: string;
  createdAt: string;
}

export interface ChatMessage {
  id: string;
  sessionId: string;
  senderId: string;
  content: string;
  images: File[];
  role: 'user' | 'assistant';
  createdAt: string;
  metadata?: string;
  outfitSuggestions?: OutfitSuggestion[];
  suggestedActions?: string[];
  data?: any;
}

export interface ChatResponse {
  id: string;
  message: string;
  success: boolean;
  errors: string[];
  data?: {
    outfitSuggestions?: OutfitSuggestion[];
    suggestedActions?: string[];
  };
}

/** For backward compatibility, also accept the newer shape */
export interface ChatResponsePayload {
  message: string;
  sessionId: string;
  outfitSuggestions: OutfitSuggestion[];
  suggestedActions: string[];
}

export interface OutfitSuggestion {
  rank: number;
  totalScore: number;
  scoreBreakdown: { [key: string]: number };
  items: SuggestedItem[];
}

export interface SuggestedItem {
  id: string;
  name: string;
  type: string;
  imageUrl: string;
  hexColor: string;
}