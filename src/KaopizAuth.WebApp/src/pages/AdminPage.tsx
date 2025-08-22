import { useAuth } from '../hooks/useAuth';
import { Navigation } from '../components/Navigation';
import { UserType } from '../types/auth';
import './AdminPage.css';

/**
 * Admin page component - only accessible by Admin users
 */
export function AdminPage() {
  const { user } = useAuth();

  if (!user || user.userType !== UserType.Admin) {
    return null; // ProtectedRoute should handle this case
  }

  return (
    <div className="admin-page">
      <Navigation />
      
      <main className="admin-content">
        <div className="admin-container">
          <div className="admin-header">
            <h1 className="admin-title">Admin Dashboard</h1>
            <p className="admin-subtitle">
              Welcome to the administrative panel. You have full access to system management features.
            </p>
          </div>

          <div className="admin-stats">
            <div className="stat-card">
              <div className="stat-icon users">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                  <path
                    d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2M9 11a4 4 0 100-8 4 4 0 000 8zM23 21v-2a4 4 0 00-3-3.87M16 3.13a4 4 0 010 7.75"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                </svg>
              </div>
              <div className="stat-content">
                <div className="stat-number">1,234</div>
                <div className="stat-label">Total Users</div>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-icon sessions">
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
              <div className="stat-content">
                <div className="stat-number">567</div>
                <div className="stat-label">Active Sessions</div>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-icon activity">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                  <path
                    d="M22 12h-4l-3 9L9 3l-3 9H2"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                </svg>
              </div>
              <div className="stat-content">
                <div className="stat-number">89</div>
                <div className="stat-label">API Requests/min</div>
              </div>
            </div>
          </div>

          <div className="admin-sections">
            <div className="admin-section">
              <h2 className="section-title">User Management</h2>
              <div className="section-grid">
                <div className="feature-card">
                  <div className="feature-icon">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                      <path
                        d="M16 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2M12.5 11a4 4 0 100-8 4 4 0 000 8zM23 21v-2a4 4 0 00-3-3.87M16 3.13a4 4 0 010 7.75"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  </div>
                  <h3 className="feature-title">Manage Users</h3>
                  <p className="feature-description">
                    View, edit, and manage user accounts and permissions.
                  </p>
                  <button className="feature-button">Access Users</button>
                </div>

                <div className="feature-card">
                  <div className="feature-icon">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                      <path
                        d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  </div>
                  <h3 className="feature-title">Role Management</h3>
                  <p className="feature-description">
                    Configure roles and permissions for different user types.
                  </p>
                  <button className="feature-button">Manage Roles</button>
                </div>
              </div>
            </div>

            <div className="admin-section">
              <h2 className="section-title">System Management</h2>
              <div className="section-grid">
                <div className="feature-card">
                  <div className="feature-icon">
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
                  <h3 className="feature-title">System Settings</h3>
                  <p className="feature-description">
                    Configure global system settings and preferences.
                  </p>
                  <button className="feature-button">Open Settings</button>
                </div>

                <div className="feature-card">
                  <div className="feature-icon">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                      <path
                        d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8zM14 2v6h6M16 13H8M16 17H8M10 9H8"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  </div>
                  <h3 className="feature-title">Audit Logs</h3>
                  <p className="feature-description">
                    View and analyze system activity and security logs.
                  </p>
                  <button className="feature-button">View Logs</button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}