import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  Poll,
  CreatePollRequest,
  CastVoteRequest,
  PollOption,
  PollStatus,
} from '../../domain/entities/poll.entity';
import { CommandResponse } from '../../domain/entities/response.entity';

interface PollDto {
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
export class PollsDataSource {
  private readonly apiUrl = `${environment.baseUrl}/api/polls`;

  constructor(private http: HttpClient) {}

  getPolls(): Observable<Poll[]> {
    return this.http
      .get<PollDto[]>(`${this.apiUrl}`)
      .pipe(
        map((polls: PollDto[]) =>
          polls.map((p: PollDto) => this.mapPollDtoToEntity(p)),
        ),
      );
  }

  getPollById(id: string): Observable<Poll> {
    return this.http
      .get<PollDto>(`${this.apiUrl}/${id}`)
      .pipe(map((poll: PollDto) => this.mapPollDtoToEntity(poll)));
  }

  createPoll(dto: CreatePollRequest): Observable<CommandResponse> {
    return this.http.post<CommandResponse>(`${this.apiUrl}`, dto);
  }

  vote(pollId: string, dto: CastVoteRequest): Observable<CommandResponse> {
    return this.http.post<CommandResponse>(`${this.apiUrl}/${pollId}/vote`, dto);
  }

  updatePoll(pollId: string, request: UpdatePollRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${pollId}`, request);
  }

  deletePoll(pollId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${pollId}`);
  }

  closePoll(pollId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${pollId}/close`, {});
  }

  uploadPollImage(file: File): Observable<string> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ url: string }>(`${environment.baseUrl}/poll-image-upload/upload`, formData)
      .pipe(map(res => res.url));
  }

  private mapPollDtoToEntity(dto: PollDto): Poll {
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

  private mapStatusToEnum(status: string): PollStatus {
    switch (status.toLowerCase()) {
      case 'active': return PollStatus.Active;
      case 'expired': return PollStatus.Expired;
      case 'closed': return PollStatus.Closed;
      default: return PollStatus.Active;
    }
  }

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
