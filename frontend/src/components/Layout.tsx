import { Outlet } from "react-router-dom";
import Header from "./Header";

export default function Layout() {
  return (
    <div
      style={{
        minHeight: "100vh",
        display: "flex",
        flexDirection: "column",
        backgroundColor: "#0c0c0e",
        color: "#e0e0e0",
        fontFamily: "sans-serif",
      }}
    >
        <Header />

      {/* Content */}
      <main
        style={{
          flex: 1,
          padding: "clamp(16px, 3vw, 40px) clamp(12px, 3vw, 20px)",
          maxWidth: "1920px",
          margin: "0 auto",
          width: "100%",
          boxSizing: "border-box",
        }}
      >
        <Outlet />
      </main>
    </div>
  );
}
