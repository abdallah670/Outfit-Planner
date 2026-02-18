export interface AuthRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  id: string;
  userName: string;
  email: string;
  token: string;
  refreshToken?: string;
}

export interface RegistrationRequest {
  firstName: string;
  lastName: string;
  email: string;
  userName: string;
  password: string;
}

export interface RegistrationResponse {
  userId: string;
  token: string;
  refreshToken: string;
  email: string;
  userName: string;
}
