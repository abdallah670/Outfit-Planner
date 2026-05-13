/**
 * Represents a validation poll for outfit feedback
 */
export interface Poll {
  id: string;
  userId: string;
  question: string;
  context: string;
  expiresAt: Date;
  status: PollStatus;
  options: PollOption[];
  totalVotes: number;
  userVotedOptionId?: string;
  createdAt: Date;
}

/**
 * Represents an option in a validation poll
 */
export interface PollOption {
  id: string;
  pollId?: string;
  outfitId?: string;
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
 * DTO for updating an existing poll
 */
export interface UpdatePollRequest {
  question?: string;
  context?: string;
  expiresAt?: string;
  options?: CreatePollOptionRequest[];
}

/**
 * DTO for creating a poll option
 */
export interface CreatePollOptionRequest {
  outfitId?: string;
  displayOrder?: number;
}

/**
 * DTO for casting a vote on a poll
 */
export interface CastVoteRequest {
  optionId: string;
}

/**
 * Status of a validation poll
 */
export enum PollStatus {
  Active = 'active',
  Expired = 'expired',
  Closed = 'closed',
}

export interface RecentPollWithCommentsDto {
  poll: Poll;
  comments: any[]; 
}

/**
 * Helper to map PollOption from ValidationPoll to local interface
 */
export function mapPollOptionToDisplayOption(option: PollOption): { id: string; imageUrl: string; votes: number } {
  return {
    id: option.id,
    imageUrl: option.outfitThumbnail || '',
    votes: option.voteCount,
  };
}

/**
 * Calculate time remaining string from expiresAt Date
 */
export function getTimeLeft(expiresAt: Date | string): string {
  if (!expiresAt) return '';
  const expiry = new Date(expiresAt);
  const now = new Date();
  const diff = expiry.getTime() - now.getTime();

  if (diff <= 0) return 'Expired';

  const hours = Math.floor(diff / (1000 * 60 * 60));
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));

  if (hours > 24) {
    const days = Math.floor(hours / 24);
    return `${days}d left`;
  }
  return hours > 0 ? `${hours}h ${minutes}m left` : `${minutes}m left`;
}

