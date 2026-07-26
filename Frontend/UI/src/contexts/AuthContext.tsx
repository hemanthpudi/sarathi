import React, {
  createContext,
  useState,
  useEffect,
  useCallback,
  useRef,
} from 'react';
import { msalInitialization, msalInstance, loginRequest } from '../config/msalConfig';
import { authService } from '../services/authService';
import type { UserDto, LoginResponse } from '../services/authService';

const SESSION_TOKEN_KEY = 'sarathi_token';
const INACTIVITY_TIMEOUT_MS = 30 * 60 * 1000; // 30 minutes
const UNAUTHENTICATED_MESSAGE = 'Unauthorized. Please sign in with an authorized Sarathi account.';

const getAuthErrorMessage = (error: unknown): string => {
  const message = error instanceof Error ? error.message : String(error ?? '');

  if (
    message.includes('AADSTS50020') ||
    message.includes('does not exist in tenant') ||
    message.includes('cannot access the application') ||
    message.includes('identity provider')
  ) {
    return 'This Microsoft account is not part of the configured Entra tenant. Sign in with a tenant account, or ask an admin to add your account as a guest.';
  }

  if (
    message.includes('unauthorized_client') ||
    message.includes('client does not exist') ||
    message.includes('not enabled for consumers') ||
    message.includes('App Registrations')
  ) {
    return 'The Microsoft Entra app registration is not configured correctly for public sign-in. Verify the client ID, redirect URI, and app registration settings in Azure Portal.';
  }

  if (message.includes('interaction_in_progress')) {
    return 'A sign-in attempt is already in progress. Please try again after the previous step completes.';
  }

  return UNAUTHENTICATED_MESSAGE;
};

const clearStaleMsalInteraction = () => {
  const hasRedirectResponse =
    window.location.search.includes('code=') ||
    window.location.search.includes('error=') ||
    window.location.hash.includes('code=') ||
    window.location.hash.includes('error=');

  if (hasRedirectResponse) {
    return;
  }

  Object.keys(sessionStorage)
    .filter((key) => key.toLowerCase().includes('interaction.status'))
    .forEach((key) => sessionStorage.removeItem(key));
};

export interface AuthContextValue {
  user: UserDto | null;
  token: string | null;
  role: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  login: () => void;
  logout: () => Promise<void>;
  clearError: () => void;
}

// eslint-disable-next-line react-refresh/only-export-components
export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<UserDto | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [role, setRole] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const refreshTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const inactivityTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const scheduleRefreshRef = useRef<(expiresAt: string) => void>(() => undefined);

  const clearSession = useCallback(() => {
    setUser(null);
    setToken(null);
    setRole(null);
    setError(null);
    sessionStorage.removeItem(SESSION_TOKEN_KEY);
    if (refreshTimerRef.current) {
      clearTimeout(refreshTimerRef.current);
    }
    if (inactivityTimerRef.current) {
      clearTimeout(inactivityTimerRef.current);
    }
  }, []);

  const scheduleRefresh = useCallback((expiresAt: string) => {
    if (refreshTimerRef.current) {
      clearTimeout(refreshTimerRef.current);
    }
    const expiresMs = new Date(expiresAt).getTime();
    const nowMs = Date.now();
    const refreshInMs = expiresMs - nowMs - 60_000; // 1 min before expiry

    if (refreshInMs > 0) {
      refreshTimerRef.current = setTimeout(async () => {
        try {
          const refreshed = await authService.refreshToken();
          sessionStorage.setItem(SESSION_TOKEN_KEY, refreshed.token);
          setToken(refreshed.token);
          scheduleRefreshRef.current(refreshed.expiresAt);
        } catch {
          clearSession();
        }
      }, refreshInMs);
    }
  }, [clearSession]);

  useEffect(() => {
    scheduleRefreshRef.current = scheduleRefresh;
  }, [scheduleRefresh]);

  const hydrateSession = useCallback(async (loginResponse: LoginResponse) => {
    sessionStorage.setItem(SESSION_TOKEN_KEY, loginResponse.token);
    setToken(loginResponse.token);
    setRole(loginResponse.role);
    const me = await authService.getMe();
    setUser(me);
    scheduleRefresh(loginResponse.expiresAt);
  }, [scheduleRefresh]);

  const logout = useCallback(async () => {
    const activeAccount = msalInstance.getActiveAccount();

    try {
      if (token) {
        await authService.logout();
      }
    } catch {
      // continue logout even if backend call fails
    } finally {
      clearSession();
      localStorage.clear();
      sessionStorage.clear();
      await msalInstance.logoutRedirect({ account: activeAccount ?? undefined });
    }
  }, [token, clearSession]);

  // Reset inactivity timer on user activity
  const resetInactivityTimer = useCallback(() => {
    if (inactivityTimerRef.current) {
      clearTimeout(inactivityTimerRef.current);
    }
    inactivityTimerRef.current = setTimeout(() => {
      logout();
    }, INACTIVITY_TIMEOUT_MS);
  }, [logout]);

  // Inactivity tracking
  useEffect(() => {
    const events = ['mousemove', 'keydown', 'click', 'scroll', 'touchstart'];
    const handler = () => resetInactivityTimer();
    events.forEach((e) => window.addEventListener(e, handler));
    return () => {
      events.forEach((e) => window.removeEventListener(e, handler));
    };
  }, [resetInactivityTimer]);

  // Handle auth:unauthorized event from axiosInstance interceptor
  useEffect(() => {
    const handler = () => {
      clearSession();
    };
    window.addEventListener('auth:unauthorized', handler);
    return () => window.removeEventListener('auth:unauthorized', handler);
  }, [clearSession]);

  // On mount: handle redirect promise and restore session
  useEffect(() => {
    const initAuth = async () => {
      setIsLoading(true);
      try {
        clearStaleMsalInteraction();
        await msalInitialization;
        const redirectResult = await msalInstance.handleRedirectPromise();

        if (redirectResult?.idToken || redirectResult?.accessToken) {
          const loginResponse = await authService.loginWithMicrosoftToken(
            redirectResult.idToken ?? redirectResult.accessToken
          );
          await hydrateSession(loginResponse);
          setError(null);
          resetInactivityTimer();
          return;
        }

        // Try to restore session from existing MSAL account
        const accounts = msalInstance.getAllAccounts();
        if (accounts.length > 0) {
          msalInstance.setActiveAccount(accounts[0]);
          const tokenResponse = await msalInstance.acquireTokenSilent({
            ...loginRequest,
            account: accounts[0],
          });

          const loginResponse = await authService.loginWithMicrosoftToken(
            tokenResponse.idToken ?? tokenResponse.accessToken
          );
          await hydrateSession(loginResponse);
          setError(null);
          resetInactivityTimer();
        }
      } catch (authError) {
        clearSession();
        setError(getAuthErrorMessage(authError));
      } finally {
        setIsLoading(false);
      }
    };

    initAuth();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const login = useCallback(() => {
    console.log("Login clicked");
    setError(null);

    msalInitialization
        .then(() => {
            console.log("Calling loginRedirect...");
            return msalInstance.loginRedirect(loginRequest);
        })
        .catch((err) => {
            console.error("loginRedirect failed:", err);
            setError(getAuthErrorMessage(err));
        });
}, []);

  const clearError = useCallback(() => setError(null), []);

  const value: AuthContextValue = {
    user,
    token,
    role,
    isAuthenticated: !!user && !!token,
    isLoading,
    error,
    login,
    logout,
    clearError,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
