import React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { useAuthStore } from "./store/authStore";

// Сторінки
import Login from "./pages/Login";
import Dashboard from "./pages/Dashboard";
import OAuthCallback from "./pages/OAuthCallback";
import CharacterSheet from "./pages/CharacterSheet";
import Campaigns from "./pages/Campaigns";
import CampaignDashboard from "./pages/CampaignDashboard";
import JoinCampaign from "./pages/JoinCampaign";

// Компоненти
import Layout from "./components/Layout";

const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
};

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Публічні маршрути */}
        <Route path="/login" element={<Login />} />
        <Route path="/auth/:provider/callback" element={<OAuthCallback />} />

        {/* Захищені маршрути, обгорнуті в Layout */}
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <Layout />
            </ProtectedRoute>
          }
        >
          {/* Тут лежать всі сторінки, які будуть всередині Layout (<Outlet />) */}
          <Route index element={<Dashboard />} />
          {/* В майбутньому тут будуть: */}
          {/* <Route path="character/create" element={<CreateCharacter />} /> */}
          {<Route path="character/:id" element={<CharacterSheet />} />}
          <Route path="campaigns" element={<Campaigns />} />
          <Route path="campaigns/:id" element={<CampaignDashboard />} />
          <Route path="join/:inviteCode" element={<JoinCampaign />} />
        </Route>

        {/* Фолбек для невідомих адрес */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
