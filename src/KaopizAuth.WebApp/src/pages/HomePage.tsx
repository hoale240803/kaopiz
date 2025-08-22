import { useAuth } from '../hooks/useAuth';
import { Navigation } from '../components/Navigation';
import './HomePage.css';

/**
 * Home page component displaying user information and dashboard
 */
export function HomePage() {
  const { user } = useAuth();

  if (!user) {
    return null; // ProtectedRoute should handle this case
  }

  const formatDate = (dateString: string) => {
    try {
      return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      });
    } catch (error) {
      return 'N/A';
    }
  };

  const getUserTypeColor = (userType: string) => {
    switch (userType) {
      case 'Admin':
        return 'user-type-admin';
      case 'Partner':
        return 'user-type-partner';
      case 'EndUser':
      default:
        return 'user-type-user';
    }
  };

  const getUserTypeIcon = (userType: string) => {
    switch (userType) {
      case 'Admin':
        return (
          <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
            <path
              d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
        );
      case 'Partner':
        return (
          <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
            <path
              d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
        );
      default:
        return (
          <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
            <path
              d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
        );
    }
  };

  return (
    <div className="home-page">
      <Navigation />
      
      <main className="home-content">
        <div className="home-container">
          {/* Welcome Section */}
          <section className="welcome-section">
            <div className="welcome-header">
              <div className="user-avatar-large">
                {user.firstName.charAt(0)}{user.lastName.charAt(0)}
              </div>
              <div className="welcome-text">
                <h1 className="welcome-title">Welcome back, {user.firstName}!</h1>
                <p className="welcome-subtitle">
                  You are successfully authenticated and logged in to your dashboard.
                </p>
              </div>
            </div>
          </section>

          {/* User Information Cards */}
          <section className="info-section">
            <h2 className="section-title">Account Information</h2>
            
            <div className="info-grid">
              {/* Personal Information Card */}
              <div className="info-card">
                <div className="card-header">
                  <div className="card-icon personal-info">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                      <path
                        d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2M12 11a4 4 0 100-8 4 4 0 000 8z"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  </div>
                  <h3 className="card-title">Personal Information</h3>
                </div>
                <div className="card-content">
                  <div className="info-item">
                    <span className="info-label">Full Name:</span>
                    <span className="info-value">{user.fullName}</span>
                  </div>
                  <div className="info-item">
                    <span className="info-label">Email:</span>
                    <span className="info-value">{user.email}</span>
                  </div>
                  <div className="info-item">
                    <span className="info-label">User ID:</span>
                    <span className="info-value font-mono">{user.id}</span>
                  </div>
                  <div className="info-item">
                    <span className="info-label">Email Verified:</span>
                    <span className={`verification-badge ${user.isEmailConfirmed ? 'verified' : 'unverified'}`}>
                      {user.isEmailConfirmed ? (
                        <>
                          <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                            <path
                              d="M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z"
                              fill="currentColor"
                            />
                          </svg>
                          Verified
                        </>
                      ) : (
                        <>
                          <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                            <path
                              d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z"
                              fill="currentColor"
                            />
                            <path
                              d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708z"
                              fill="currentColor"
                            />
                          </svg>
                          Not Verified
                        </>
                      )}
                    </span>
                  </div>
                </div>
              </div>

              {/* Account Details Card */}
              <div className="info-card">
                <div className="card-header">
                  <div className="card-icon account-details">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                      <path
                        d="M12 2a10 10 0 1010 10A10.011 10.011 0 0012 2zm0 18a8 8 0 118-8 8.009 8.009 0 01-8 8z"
                        fill="currentColor"
                      />
                      <path
                        d="M11 11h1v5h-1zM12 8.5a1 1 0 11-1-1 1 1 0 011 1z"
                        fill="currentColor"
                      />
                    </svg>
                  </div>
                  <h3 className="card-title">Account Details</h3>
                </div>
                <div className="card-content">
                  <div className="info-item">
                    <span className="info-label">User Type:</span>
                    <span className={`user-type-badge ${getUserTypeColor(user.userType)}`}>
                      {getUserTypeIcon(user.userType)}
                      {user.userType}
                    </span>
                  </div>
                  <div className="info-item">
                    <span className="info-label">Roles:</span>
                    <div className="roles-container">
                      {user.roles.length > 0 ? (
                        user.roles.map((role, index) => (
                          <span key={index} className="role-badge">
                            {role}
                          </span>
                        ))
                      ) : (
                        <span className="no-roles">No additional roles</span>
                      )}
                    </div>
                  </div>
                  <div className="info-item">
                    <span className="info-label">Account Created:</span>
                    <span className="info-value">{formatDate(user.createdAt)}</span>
                  </div>
                  {user.lastLoginAt && (
                    <div className="info-item">
                      <span className="info-label">Last Login:</span>
                      <span className="info-value">{formatDate(user.lastLoginAt)}</span>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </section>

          {/* Quick Actions Section */}
          <section className="actions-section">
            <h2 className="section-title">Quick Actions</h2>
            <div className="actions-grid">
              <button className="action-button">
                <div className="action-icon">
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                    <path
                      d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2M12 11a4 4 0 100-8 4 4 0 000 8z"
                      stroke="currentColor"
                      strokeWidth="2"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                    />
                  </svg>
                </div>
                <div className="action-content">
                  <h3 className="action-title">Edit Profile</h3>
                  <p className="action-description">Update your personal information</p>
                </div>
              </button>

              <button className="action-button">
                <div className="action-icon">
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                    <path
                      d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"
                      stroke="currentColor"
                      strokeWidth="2"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                    />
                  </svg>
                </div>
                <div className="action-content">
                  <h3 className="action-title">Security Settings</h3>
                  <p className="action-description">Manage password and security</p>
                </div>
              </button>

              {user.userType === 'Admin' && (
                <button className="action-button">
                  <div className="action-icon">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                      <path
                        d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  </div>
                  <div className="action-content">
                    <h3 className="action-title">Admin Panel</h3>
                    <p className="action-description">Access administrative features</p>
                  </div>
                </button>
              )}
            </div>
          </section>
        </div>
      </main>
    </div>
  );
}