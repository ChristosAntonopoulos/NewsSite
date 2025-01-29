export interface User {
  id: string;
  email: string;
  name: string;
  language: string;
  theme: 'light' | 'dark';
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface RegisterData extends LoginCredentials {
  name: string;
  language: string;
}

export interface AuthResponse {
  user: User;
  token: string;
}

export class AuthError extends Error {
  constructor(message: string, public code: string) {
    super(message);
    this.name = 'AuthError';
  }
}

export class AuthApi {
  private baseUrl = import.meta.env.VITE_API_URL;

  private async handleResponse<T>(response: Response): Promise<T> {
    const data = await response.json();
    if (!response.ok) {
      throw new AuthError(data.message || 'An error occurred', data.code || 'UNKNOWN_ERROR');
    }
    return data.data;
  }

  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const response = await fetch(`${this.baseUrl}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(credentials),
    });

    return this.handleResponse<AuthResponse>(response);
  }

  async register(data: RegisterData): Promise<AuthResponse> {
    const response = await fetch(`${this.baseUrl}/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });

    return this.handleResponse<AuthResponse>(response);
  }

  async googleLogin(token: string): Promise<AuthResponse> {
    const response = await fetch(`${this.baseUrl}/auth/google`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(token),
    });

    return this.handleResponse<AuthResponse>(response);
  }

  async redditLogin(code: string): Promise<AuthResponse> {
    const response = await fetch(`${this.baseUrl}/auth/reddit`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(code),
    });

    return this.handleResponse<AuthResponse>(response);
  }
}

export const authApi = new AuthApi(); 