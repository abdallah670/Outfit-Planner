import { Visibility, TaggedUser } from "./feed.entity";

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
  tags?: string[];
  taggedUsers?: TaggedUser[];
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
  description?: string;
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
  visibility: Visibility;
  options: CreatePollOptionRequest[];
  tags?: string[];
}

/**
 * DTO for updating an existing poll
 */
export interface UpdatePollRequest {
  question?: string;
  context?: string;
  expiresAt?: string;
  options?: CreatePollOptionRequest[];
  tags?: string[];
}

/**
 * DTO for creating a poll option
 */
export interface CreatePollOptionRequest {
  outfitId?: string;
  displayOrder?: number;
  description?: string;
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
export interface VoterInfo {
  voterId: string;
  voterName: string;
  voterAvatarUrl: string;
  votedAt: Date;
  optionId: string;
  optionDescription?: string;
  optionDisplayOrder: number;
}

/**
 * Helper to map PollOption from ValidationPoll to local interface
 */
export function mapPollOptionToDisplayOption(option: PollOption): { id: string; imageUrl: string; votes: number; description: string } {
  return {
    id: option.id,
    imageUrl: option.outfitThumbnail || '',
    votes: option.voteCount,
    description: option.description || ''
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

