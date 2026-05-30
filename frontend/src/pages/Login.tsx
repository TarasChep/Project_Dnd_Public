import { useState } from "react";
import { useAuthStore } from "../store/authStore";
import { Navigate } from "react-router-dom";
import { api } from "../api/axios";

export default function Login() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const login = useAuthStore((state) => state.login);

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [isRegisterMode, setIsRegisterMode] = useState(false);
  const [username, setUsername] = useState(""); // НОВЕ ПОЛЕ
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  const validateForm = () => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      setError("Invalid email format.");
      return false;
    }

    if (isRegisterMode) {
      if (username.length < 3) {
        setError("Username must be at least 3 characters long.");
        return false;
      }
      if (password.length < 6) {
        setError("Password must be at least 6 characters long.");
        return false;
      }
      if (password !== confirmPassword) {
        setError("Passwords do not match.");
        return false;
      }
    }
    return true;
  };

  const handleLocalAuth = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!validateForm()) return;

    try {
      setIsLoading(true);
      const endpoint = isRegisterMode ? "/auth/register" : "/auth/login";

      // Формуємо payload динамічно. При логіні username не потрібен.
      const payload = isRegisterMode
        ? { username, email, password }
        : { email, password };

      const response = await api.post(endpoint, payload);

      const token = response.data.token || response.data;

      if (token && typeof token === "string") {
        login(token);
      } else {
        setError("Invalid response: Token missing.");
      }
    } catch (err: any) {
      console.error("Local Auth Error:", err);

      // Оновлений парсер, який жере і об'єкти, і масиви
      if (err.response?.data?.errors) {
        const errorsData = err.response.data.errors;

        if (Array.isArray(errorsData)) {
          // Якщо бекенд прислав масив (як зараз із 'already taken')
          setError(errorsData.join(" | "));
        } else {
          // Якщо бекенд прислав словник (FluentValidation)
          const errorMessages = Object.entries(errorsData)
            .map(
              ([field, messages]) =>
                `${field}: ${(messages as string[]).join(", ")}`,
            )
            .join(" | ");
          setError(errorMessages);
        }
      } else if (err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError("Authentication failed. Check credentials or backend.");
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleOAuthLogin = async (provider: string) => {
    try {
      setIsLoading(true);
      setError(null);

      const redirectUri = `${window.location.origin}/auth/${provider}/callback`;
      const response = await api.get(
        `/auth/oauth/${provider}/authorize?redirectUri=${encodeURIComponent(redirectUri)}`,
      );

      if (response.data && response.data.authUrl) {
        window.location.href = response.data.authUrl;
      } else {
        setError("Invalid response from backend.");
      }
    } catch (err: any) {
      console.error("OAuth Error:", err);
      setError("Failed to initialize OAuth.");
    } finally {
      setIsLoading(false);
    }
  };

  const toggleMode = () => {
    setIsRegisterMode(!isRegisterMode);
    setError(null);
    setUsername("");
    setConfirmPassword("");
  };

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        marginTop: "50px",
        fontFamily: "sans-serif",
      }}
    >
      <h1>D&D Platform</h1>

      {error && (
        <div
          style={{
            color: "#d32f2f",
            marginBottom: "15px",
            backgroundColor: "#ffebee",
            padding: "10px 15px",
            borderRadius: "4px",
            maxWidth: "300px",
            textAlign: "center",
            border: "1px solid #ef5350",
          }}
        >
          {error}
        </div>
      )}

      <form
        onSubmit={handleLocalAuth}
        style={{
          display: "flex",
          flexDirection: "column",
          width: "300px",
          gap: "15px",
          marginBottom: "30px",
          padding: "20px",
          border: "1px solid #ddd",
          borderRadius: "8px",
        }}
      >
        <h3 style={{ margin: "0 0 10px 0", textAlign: "center" }}>
          {isRegisterMode ? "Create Account" : "Sign In"}
        </h3>

        {/* Поле Username тільки для реєстрації */}
        {isRegisterMode && (
          <input
            type="text"
            placeholder="Username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
            style={{
              padding: "10px",
              fontSize: "16px",
              borderRadius: "4px",
              border: "1px solid #ccc",
            }}
          />
        )}

        <input
          type="email"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          style={{
            padding: "10px",
            fontSize: "16px",
            borderRadius: "4px",
            border: "1px solid #ccc",
          }}
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          style={{
            padding: "10px",
            fontSize: "16px",
            borderRadius: "4px",
            border: "1px solid #ccc",
          }}
        />

        {isRegisterMode && (
          <input
            type="password"
            placeholder="Confirm Password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
            style={{
              padding: "10px",
              fontSize: "16px",
              borderRadius: "4px",
              border: "1px solid #ccc",
            }}
          />
        )}

        <button
          type="submit"
          disabled={isLoading}
          style={{
            padding: "12px",
            fontSize: "16px",
            cursor: isLoading ? "not-allowed" : "pointer",
            backgroundColor: "#4CAF50",
            color: "white",
            border: "none",
            borderRadius: "4px",
          }}
        >
          {isLoading ? "Processing..." : isRegisterMode ? "Register" : "Login"}
        </button>

        <p
          onClick={toggleMode}
          style={{
            color: "#2196F3",
            fontSize: "14px",
            textAlign: "center",
            cursor: "pointer",
            margin: 0,
            textDecoration: "underline",
          }}
        >
          {isRegisterMode
            ? "Already have an account? Login"
            : "Don't have an account? Register"}
        </p>
      </form>

      <div
        style={{
          display: "flex",
          alignItems: "center",
          width: "300px",
          marginBottom: "30px",
        }}
      >
        <div style={{ flex: 1, height: "1px", backgroundColor: "#ccc" }}></div>
        <span style={{ padding: "0 10px", color: "#666", fontSize: "14px" }}>
          OR
        </span>
        <div style={{ flex: 1, height: "1px", backgroundColor: "#ccc" }}></div>
      </div>

      <div style={{ display: "flex", gap: "20px" }}>
        <button
          type="button"
          onClick={() => handleOAuthLogin("google")}
          disabled={isLoading}
          style={{
            padding: "12px 24px",
            fontSize: "16px",
            cursor: isLoading ? "not-allowed" : "pointer",
            backgroundColor: "#db4437",
            color: "white",
            border: "none",
            borderRadius: "4px",
          }}
        >
          Google
        </button>

        <button
          type="button"
          onClick={() => handleOAuthLogin("discord")}
          disabled={isLoading}
          style={{
            padding: "12px 24px",
            fontSize: "16px",
            cursor: isLoading ? "not-allowed" : "pointer",
            backgroundColor: "#5865F2",
            color: "white",
            border: "none",
            borderRadius: "4px",
          }}
        >
          Discord
        </button>
      </div>
    </div>
  );
}
