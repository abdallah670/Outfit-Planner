/**
 * Represents a validation poll for outfit feedback
 */
export interface ValidationPoll {
  id: string;
  userId: string;
  question: string;
  context: string;
  expiresAt: Date;
  status: PollStatus;
  options: PollOption[];
  totalVotes: number;
  createdAt: Date;
}

/**
 * Represents an option in a validation poll
 */
export interface PollOption {
  id: string;
  pollId?: string;
  outfitId?: string;
  description: string;
  displayOrder: number;
  voteCount: number;
  outfitThumbnail?: string;
}

/**
 * Represents a vote cast on a poll option
 */
export interface Vote {
  id: string;
  pollId: string;
  optionId: string;
  voterId: string;
  rating: number;
  comment?: string;
  isAnonymous: boolean;
  createdAt: Date;
}

/**
 * DTO for creating a new validation poll
 */
export interface CreatePollRequest {
  question: string;
  context?: string;
  expiresAt: Date;
  options: CreatePollOptionRequest[];
}

/**
 * DTO for creating a poll option
 */
export interface CreatePollOptionRequest {
  outfitId?: string;
  description: string;
  displayOrder: number;
}

/**
 * DTO for casting a vote on a poll
 */
export interface CastVoteRequest {
  optionId: string;
  rating: number;
  comment?: string;
  isAnonymous: boolean;
}

/**
 * Status of a validation poll
 */
export enum PollStatus {
  Active = 'active',
  Expired = 'expired',
  Closed = 'closed',
}

/**
 * Response from command operations (create poll, vote)
 */
export interface CommandResponse {
  id: string;
  success: boolean;
  message: string;
  errors: string[];
}
