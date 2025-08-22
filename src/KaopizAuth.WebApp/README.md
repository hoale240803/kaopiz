# Kaopiz Authentication Frontend

A React-based frontend application with comprehensive authentication and authorization features.

## Features

### 🔐 Authentication
- **Login/Logout**: Secure user authentication with JWT tokens
- **User Types**: Support for EndUser, Admin, and Partner user types
- **Remember Me**: Optional session persistence across browser sessions
- **Automatic Token Refresh**: Background token refresh to maintain user sessions
- **Cross-tab Synchronization**: Authentication state synced across browser tabs

### 🛡️ Authorization & Route Protection
- **Protected Routes**: Route-level access control
- **Role-based Access**: Fine-grained permissions based on user roles
- **User Type Restrictions**: Routes restricted by user type (Admin, Partner, EndUser)
- **Automatic Redirects**: Seamless redirection for unauthorized access

### 🎨 User Interface
- **Responsive Design**: Mobile-first responsive layout
- **Navigation Bar**: User-friendly navigation with user menu
- **Login Page**: Clean, accessible login form with validation
- **Home Dashboard**: Comprehensive user information display
- **Admin Panel**: Administrative features for admin users
- **Test Page**: Development tool for testing authentication flows

## Project Structure

```
src/
├── components/          # Reusable UI components
│   ├── Navigation.tsx   # Main navigation bar
│   ├── ProtectedRoute.tsx # Route protection component
│   └── TokenRefreshProvider.tsx # Automatic token refresh
├── contexts/           # React Context providers
│   └── AuthContext.tsx # Authentication state management
├── hooks/              # Custom React hooks
│   ├── useAuth.ts      # Authentication hook
│   └── useForm.ts      # Form handling hook
├── pages/              # Page components
│   ├── LoginPage.tsx   # Login form page
│   ├── HomePage.tsx    # Main dashboard
│   ├── AdminPage.tsx   # Admin-only page
│   ├── TestPage.tsx    # Development test page
│   └── UnauthorizedPage.tsx # Access denied page
├── services/           # API services
│   ├── authService.ts  # Authentication API calls
│   └── httpService.ts  # HTTP utility service
├── types/              # TypeScript type definitions
│   └── auth.ts         # Authentication-related types
├── utils/              # Utility functions
│   ├── authReducer.ts  # Authentication state reducer
│   └── tokenStorage.ts # Token storage management
└── App.tsx             # Main application component
```

## Key Components

### AuthProvider
Provides authentication state and actions to the entire application using React Context.

### ProtectedRoute
Guards routes based on authentication status, user roles, and user types.

### TokenRefreshProvider
Automatically refreshes access tokens in the background to maintain user sessions.

### useAuth Hook
Convenient hook for accessing authentication state and actions from any component.

## Authentication Flow

1. **Login**: User submits credentials via login form
2. **Token Storage**: JWT tokens stored securely (localStorage/sessionStorage)
3. **Route Protection**: Access to protected routes validated on navigation
4. **Token Refresh**: Background refresh of tokens before expiry
5. **Logout**: Secure cleanup of authentication state and tokens

## User Types & Permissions

### EndUser
- Access to home dashboard
- Personal profile management
- Basic application features

### Partner
- EndUser permissions
- Additional partner-specific features

### Admin
- Full application access
- User management capabilities
- System administration features

## Development Features

### Test Page (`/test`)
A comprehensive testing interface that allows developers to:
- View current authentication state
- Test login/logout flows
- Verify token refresh mechanisms
- Test route protection
- Monitor authentication errors

### Error Handling
- Graceful error handling for network issues
- User-friendly error messages
- Automatic retry mechanisms for token refresh
- Fallback to login on authentication failures

## Security Features

- **Secure Token Storage**: Tokens stored with appropriate security measures
- **Automatic Logout**: Logout on token expiry or security issues
- **CSRF Protection**: Protection against cross-site request forgery
- **XSS Prevention**: Protection against cross-site scripting
- **Secure Headers**: Appropriate HTTP security headers

## Browser Support

- Modern browsers supporting ES2022
- Mobile browsers (iOS Safari, Chrome Mobile)
- Desktop browsers (Chrome, Firefox, Safari, Edge)

## Getting Started

1. **Install Dependencies**:
   ```bash
   npm install
   ```

2. **Development Mode**:
   ```bash
   npm run dev
   ```

3. **Build for Production**:
   ```bash
   npm run build
   ```

4. **Type Checking**:
   ```bash
   npm run type-check
   ```

## Environment Configuration

Configure the API base URL in your environment:

```env
VITE_API_BASE_URL=https://your-api-domain.com/api
```

## Testing

Visit `/test` when logged in to access the authentication testing interface. This page provides comprehensive tools for verifying all authentication flows work correctly.

## Architecture Decisions

- **React Context**: Used for global authentication state management
- **JWT Tokens**: Industry-standard token-based authentication
- **React Router**: Client-side routing with protection
- **TypeScript**: Type safety throughout the application
- **CSS Modules**: Component-scoped styling
- **Native Fetch**: No external HTTP library dependencies
