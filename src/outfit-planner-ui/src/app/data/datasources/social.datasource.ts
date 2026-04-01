import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  ValidationPoll,
  CreatePollRequest,
  CastVoteRequest,
  CommandResponse,
  PollOption,
  PollStatus,
} from '../../domain/entities/validation-poll.entity';
import {
  TrendingOutfit,
  OutfitEngagement,
  VoteComment,
  AddVoteCommentRequest,
} from '../../domain/entities/social-engagement.entity';

// DTOs matching the API response structure
interface ValidationPollDto {
  id: string;
  userId: string;
  question: string;
  context: string;
  expiresAt: string;
  status: string;
  options: PollOptionDto[];
  totalVotes: number;
  createdAt: string;
}

interface PollOptionDto {
  id: string;
  pollId?: string;
  outfitId?: string;
  description: string;
  displayOrder: number;
  voteCount: number;
  outfitThumbnail?: string;
}

/**
 * DTO for trending outfit from API
 */
interface TrendingOutfitDto {
  id: string;
  outfitId: string;
  outfitImageUrl: string;
  userName: string;
  userAvatarUrl: string;
  likeCount: number;
  commentCount: number;
  trendingScore: number;
  rankPosition: number;
  voteId: string;
  createdAt: string;
}

/**
 * DTO for outfit engagement
 */
interface OutfitEngagementDto {
  outfitId: string;
  likeCount: number;
  commentCount: number;
  userHasLiked: boolean;
}

/**
 * DTO for outfit comment
 */
interface OutfitCommentDto {
  id: string;
  outfitId: string;
  userId: string;
  userName: string;
  userAvatarUrl: string;
  content: string;
  createdAt: string;
  isDeleted: boolean;
}

export interface OutfitVoteResult {
  outfitId: string;
  voteCount: number;
}

export interface UpdatePollRequest {
  question?: string;
  context?: string;
  expiresAt?: string;
  options?: UpdatePollOptionRequest[];
}

export interface UpdatePollOptionRequest {
  id?: string;
  description?: string;
  displayOrder: number;
  outfitId?: string;
}

@Injectable({
  providedIn: 'root',
})
export class SocialDataSource {
  private readonly socialApiUrl = `${environment.baseUrl}/social`;
  private readonly voteEngagementApiUrl = `${environment.baseUrl}/vote-engagement`;
  private readonly trendingApiUrl = `${environment.baseUrl}/trending`;

  constructor(private http: HttpClient) {}

  // ============ Vote Engagement Methods ============

  /**
   * React to a vote (like, love, insightful)
   */
  reactToVote(voteId: string, reactionType: string): Observable<void> {
    return this.http.post<void>(
      `${this.voteEngagementApiUrl}/votes/${voteId}/react`,
      { reactionType }
    );
  }

  /**
   * Add a comment to a vote
   */
  addVoteComment(request: AddVoteCommentRequest): Observable<VoteComment> {
    return this.http.post<any>(
      `${this.voteEngagementApiUrl}/votes/${request.voteId}/comments`,
      { content: request.content, parentCommentId: request.parentCommentId }
    ).pipe(
      map(response => this.mapVoteCommentDtoToEntity(response))
    );
  }

  /**
   * Like a vote comment
   */
  likeVoteComment(commentId: string): Observable<void> {
    return this.http.post<void>(
      `${this.voteEngagementApiUrl}/comments/${commentId}/like`,
      {}
    );
  }

  /**
   * Get comments for a vote
   */
  getVoteComments(voteId: string, maxDepth = 3): Observable<VoteComment[]> {
    return this.http.get<any[]>(
      `${this.voteEngagementApiUrl}/votes/${voteId}/comments?maxDepth=${maxDepth}`
    ).pipe(
      map((dtos: any[]) => dtos.map(dto => this.mapVoteCommentDtoToEntity(dto)))
    );
  }

  // ============ Existing Poll Methods ============

  /**
   * Get all polls for the current user
   */
  getPolls(): Observable<ValidationPoll[]> {
    return this.http
      .get<ValidationPollDto[]>(`${this.socialApiUrl}/polls`)
      .pipe(
        map((polls: ValidationPollDto[]) =>
          polls.map((p: ValidationPollDto) => this.mapPollDtoToEntity(p)),
        ),
      );
  }

  /**
   * Get a specific poll by ID
   */
  getPollById(id: string): Observable<ValidationPoll> {
    return this.http
      .get<ValidationPollDto>(`${this.socialApiUrl}/polls/${id}`)
      .pipe(map((poll: ValidationPollDto) => this.mapPollDtoToEntity(poll)));
  }

  /**
   * Create a new poll
   */
  createPoll(dto: CreatePollRequest): Observable<CommandResponse> {
    return this.http.post<CommandResponse>(`${this.socialApiUrl}/polls`, dto);
  }

  /**
   * Cast a vote on a poll
   */
  vote(pollId: string, dto: CastVoteRequest): Observable<CommandResponse> {
    return this.http.post<CommandResponse>(`${this.socialApiUrl}/polls/${pollId}/vote`, dto);
  }

  /**
   * Update a poll
   */
  updatePoll(pollId: string, request: UpdatePollRequest): Observable<void> {
    return this.http.put<void>(`${this.socialApiUrl}/polls/${pollId}`, request);
  }

  /**
   * Delete a poll
   */
  deletePoll(pollId: string): Observable<void> {
    return this.http.delete<void>(`${this.socialApiUrl}/polls/${pollId}`);
  }

  /**
   * Close a poll
   */
  closePoll(pollId: string): Observable<void> {
    return this.http.post<void>(`${this.socialApiUrl}/polls/${pollId}/close`, {});
  }

  /**
   * Get trending outfits from the community
   */
  getTrendingOutfits(): Observable<TrendingOutfit[]> {
    return this.http
      .get<TrendingOutfitDto[]>(`${this.trendingApiUrl}/outfits`)
      .pipe(
        map((outfits: TrendingOutfitDto[]) =>
          outfits.map((o: TrendingOutfitDto) => this.mapTrendingOutfitDtoToEntity(o)),
        ),
      );
  }

  /**
   * Map API DTO to domain entity
   */
  private mapPollDtoToEntity(dto: ValidationPollDto): ValidationPoll {
    return {
      id: dto.id,
      userId: dto.userId,
      question: dto.question,
      context: dto.context,
      expiresAt: new Date(dto.expiresAt),
      status: this.mapStatusToEnum(dto.status),
      options: dto.options.map((o: PollOptionDto) => this.mapOptionDtoToEntity(o)),
      totalVotes: dto.totalVotes,
      createdAt: new Date(dto.createdAt),
    };
  }

  /**
   * Map API status string to PollStatus enum
   */
  private mapStatusToEnum(status: string): PollStatus {
    switch (status.toLowerCase()) {
      case 'active':
        return PollStatus.Active;
      case 'expired':
        return PollStatus.Expired;
      case 'closed':
        return PollStatus.Closed;
      default:
        return PollStatus.Active;
    }
  }

  /**
   * Map option DTO to domain entity
   */
  private mapOptionDtoToEntity(dto: PollOptionDto): PollOption {
    return {
      id: dto.id,
      pollId: dto.pollId,
      outfitId: dto.outfitId,
      description: dto.description,
      displayOrder: dto.displayOrder,
      voteCount: dto.voteCount,
      outfitThumbnail: dto.outfitThumbnail,
    };
  }

  /**
   * Map trending outfit DTO to domain entity
   */
  private mapTrendingOutfitDtoToEntity(dto: TrendingOutfitDto): TrendingOutfit {
    return {
      id: dto.id,
      userId: '', // Not in DTO anymore for simplicity or fetch via userName
      userName: dto.userName,
      userAvatar: dto.userAvatarUrl,
      imageUrl: dto.outfitImageUrl,
      likes: dto.likeCount,
      comments: dto.commentCount,
      occasion: 'Casual', // Default or handle if added to DTO
      trendingScore: dto.trendingScore,
      voteId: dto.voteId,
      createdAt: new Date(dto.createdAt),
    };
  }

  /**
   * Map API DTO to vote comment entity
   */
  private mapVoteCommentDtoToEntity(dto: any): VoteComment {
    return {
      id: dto.id,
      voteId: dto.voteId,
      userId: dto.userId || '',
      userName: dto.userName || 'Anonymous',
      userAvatarUrl: dto.userAvatarUrl,
      content: dto.content,
      createdAt: new Date(dto.createdAt),
      isDeleted: dto.isDeleted || false,
      parentCommentId: dto.parentCommentId,
      likes: dto.likes || [],
      replies: dto.replies ? dto.replies.map((r: any) => this.mapVoteCommentDtoToEntity(r)) : []
    };
  }
}
