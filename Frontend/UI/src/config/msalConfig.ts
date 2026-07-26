// import { PublicClientApplication, Configuration, RedirectRequest } from '@azure/msal-browser';

import { PublicClientApplication } from "@azure/msal-browser";
import type { Configuration, RedirectRequest } from "@azure/msal-browser";

const clientId =
  (import.meta.env.VITE_AZURE_CLIENT_ID as string | undefined) ||
  '8b76b2f9-142f-451c-9d2e-2a1755775e5b';
const authorityOverride = (import.meta.env.VITE_AZURE_AUTHORITY as string | undefined)?.trim();
const redirectUri =
  (import.meta.env.VITE_REDIRECT_URI as string | undefined) ||
  window.location.origin;
const postLogoutRedirectUri =
  (import.meta.env.VITE_POST_LOGOUT_REDIRECT_URI as string | undefined) || redirectUri;
const tenantId =
  (import.meta.env.VITE_AZURE_TENANT_ID as string | undefined)?.trim() ||
  '9da4bbe6-c2df-43c6-83a4-6540ddb08c7e';
const authority = authorityOverride || `https://login.microsoftonline.com/${tenantId}`;

const msalConfiguration: Configuration = {
  auth: {
    clientId,
    authority,
    redirectUri,
    postLogoutRedirectUri,
    navigateToLoginRequestUrl: false,
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false,
  },
};

export const msalInstance = new PublicClientApplication(msalConfiguration);
export const msalInitialization = msalInstance.initialize();


export const loginRequest: RedirectRequest = {
  scopes: ["openid", "profile", "email"],
};
