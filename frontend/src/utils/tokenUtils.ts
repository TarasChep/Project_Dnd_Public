export function extractUserIdFromToken(token: string | null): string {
  if (!token) {
    // Fallback: try to get from localStorage directly
    const storedToken = localStorage.getItem('jwtToken') || localStorage.getItem('token');
    if (!storedToken) {
      console.warn('No token found in authStore or localStorage');
      return "";
    }
    token = storedToken;
  }
  
  try {
    const parts = token.split(".");
    if (parts.length !== 3) {
      console.error('Invalid JWT format (should have 3 parts):', parts.length);
      return "";
    }
    
    const base64Url = parts[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const payload = JSON.parse(window.atob(base64));
    
    // Try various common claims for user ID
    const userId = (
      payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ||
      payload["sub"] ||
      payload["user_id"] ||
      payload["uid"] ||
      payload["id"] ||
      ""
    );
    
    if (userId) {
      console.debug('Extracted user ID:', userId);
    } else {
      console.warn('Could not find user ID in token. Payload keys:', Object.keys(payload));
    }
    
    return userId;
  } catch (error) {
    console.error("Failed to extract user ID from token:", error, "Token preview:", token?.substring(0, 50));
    return "";
  }
}
