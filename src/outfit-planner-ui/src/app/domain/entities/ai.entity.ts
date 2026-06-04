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
  role: 'user' | 'assistant';
  createdAt: string;
}

export interface ChatResponse {
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