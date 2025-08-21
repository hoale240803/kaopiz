// Authentication related type definitions

export enum UserType {
  EndUser = 'EndUser',
  Admin = 'Admin',
  Partner = 'Partner'
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  roles: string[];
}

export interface LoginCredentials {
  email: string;
  password: string;
  rememberMe: boolean;
  userType?: UserType;
}

export interface RegisterData {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  userType: UserType;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface FormFieldError {
  [key: string]: string;
}

export interface FormState<T> {
  values: T;
  errors: FormFieldError;
  isSubmitting: boolean;
  isValid: boolean;
}