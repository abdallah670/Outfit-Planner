import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import {
  ValidationPoll,
  CreatePollRequest,
  CastVoteRequest,
  CommandResponse,
} from '../entities/validation-poll.entity';
import {
  TrendingOutfit,
  OutfitEngagement,
  VoteComment,
  AddVoteCommentRequest,
} from '../entities/social-engagement.entity';
import { UpdatePollRequest } from '../../data/datasources/social.datasource';

export const SOCIAL_REPOSITORY = new InjectionToken<SocialRepository>('SocialRepository');

/**
 * Repository interface for social-related operations
 */
export interface SocialRepository {
  /**
   * Get all polls for the current user
   */
  getPolls(): Observable<ValidationPoll[]>;

  /**
   * Get a specific poll by ID
   */
  getPollById(id: string): Observable<ValidationPoll>;

  /**
   * Create a new poll
   */
  createPoll(dto: CreatePollRequest): Observable<CommandResponse>;

  /**
   * Cast a vote on a poll
   */
  vote(pollId: string, dto: CastVoteRequest): Observable<CommandResponse>;

  /**
   * Get trending outfits from the community with pagination
   */
  getTrendingOutfits(page?: number, pageSize?: number): Observable<{ items: TrendingOutfit[]; totalCount: number }>;

  // ============ Vote Engagement Methods ============

  /**
   * React to a vote
   */
  reactToVote(voteId: string, reactionType: string): Observable<void>;

  /**
   * Add a comment to a vote
   */
  addVoteComment(request: AddVoteCommentRequest): Observable<VoteComment>;

  /**
   * Like a vote comment
   */
  likeVoteComment(commentId: string): Observable<void>;

  /**
   * Get comments for a vote
   */
  getVoteComments(voteId: string, maxDepth?: number): Observable<VoteComment[]>;

  // ============ Poll Management Methods ============

  /**
   * Update a poll
   */
  updatePoll(pollId: string, request: UpdatePollRequest): Observable<void>;

  /**
   * Delete a poll
   */
  deletePoll(pollId: string): Observable<void>;

  /**
   * Close a poll
   */
  closePoll(pollId: string): Observable<void>;
}
