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

@Injectable({
  providedIn: 'root',
})
export class SocialDataSource {
  private readonly apiUrl = `${environment.baseUrl}/social`;

  constructor(private http: HttpClient) {}

  /**
   * Get all polls for the current user
   */
  getPolls(): Observable<ValidationPoll[]> {
    return this.http
      .get<ValidationPollDto[]>(`${this.apiUrl}/polls`)
      .pipe(map((polls: ValidationPollDto[]) => polls.map((p: ValidationPollDto) => this.mapPollDtoToEntity(p))));
  }

  /**
   * Get a specific poll by ID
   */
  getPollById(id: string): Observable<ValidationPoll> {
    return this.http
      .get<ValidationPollDto>(`${this.apiUrl}/polls/${id}`)
      .pipe(map((poll: ValidationPollDto) => this.mapPollDtoToEntity(poll)));
  }

  /**
   * Create a new poll
   */
  createPoll(dto: CreatePollRequest): Observable<CommandResponse> {
    return this.http.post<CommandResponse>(`${this.apiUrl}/polls`, dto);
  }

  /**
   * Cast a vote on a poll
   */
  vote(pollId: string, dto: CastVoteRequest): Observable<CommandResponse> {
    return this.http.post<CommandResponse>(`${this.apiUrl}/polls/${pollId}/vote`, dto);
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
}
