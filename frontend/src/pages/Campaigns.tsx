import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api/axios";

export default function Campaigns() {
  const navigate = useNavigate();
  const [campaigns, setCampaigns] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  const [activeModal, setActiveModal] = useState<"none" | "create" | "join">("none");
  const [inputValue, setInputValue] = useState("");

  useEffect(() => {
    fetchCampaigns();
  }, []);

  const fetchCampaigns = async () => {
    try {
      const res = await api.get("/campaigns"); // Expected to return CampaignListDto[]
      setCampaigns(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateCampaign = async () => {
    if (!inputValue.trim()) return;
    try {
      await api.post("/campaigns", { name: inputValue });
      setInputValue("");
      setActiveModal("none");
      fetchCampaigns();
    } catch (err) {
      console.error(err);
      alert("Failed to create campaign");
    }
  };

  const handleJoinCampaign = async () => {
    if (!inputValue.trim()) return;
    try {
      await api.post(`/campaigns/join/${inputValue}`);
      setInputValue("");
      setActiveModal("none");
      fetchCampaigns();
    } catch (err: any) {
      console.error(err);
      alert(err.response?.data?.error || "Failed to join campaign");
    }
  };

  if (loading) return <div style={{ color: "#fff", padding: "20px", textAlign: "center" }}>Loading campaigns...</div>;

  return (
    <div style={{ padding: "20px", color: "#fff", maxWidth: "1200px", margin: "0 auto" }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "30px" }}>
        <h1>My Campaigns</h1>
        <div style={{ display: "flex", gap: "10px" }}>
          <button onClick={() => setActiveModal("join")} style={{ padding: "10px 15px", backgroundColor: "#03dac6", color: "#000", border: "none", borderRadius: "6px", fontWeight: "bold", cursor: "pointer" }}>
            JOIN CAMPAIGN
          </button>
          <button onClick={() => setActiveModal("create")} style={{ padding: "10px 15px", backgroundColor: "#bb86fc", color: "#000", border: "none", borderRadius: "6px", fontWeight: "bold", cursor: "pointer" }}>
            + START A CAMPAIGN
          </button>
        </div>
      </div>

      {campaigns.length === 0 ? (
        <div style={{ textAlign: "center", padding: "50px", backgroundColor: "#1e1e1e", borderRadius: "12px", border: "1px solid #333" }}>
          <h3 style={{ color: "#757575" }}>You don't have any campaigns yet.</h3>
          <p style={{ color: "#757575" }}>Create one to act as a GM or join a friend's campaign using an invite code.</p>
        </div>
      ) : (
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))", gap: "20px" }}>
          {campaigns.map(camp => {
            const isGm = camp.role === "GM";
            return (
              <div key={camp.id} onClick={() => navigate(`/campaigns/${camp.id}`)} style={{ backgroundColor: "#1e1e1e", border: "1px solid #333", borderRadius: "8px", padding: "20px", cursor: "pointer", transition: "all 0.2s" }}>
                <h2 style={{ margin: "0 0 10px 0", color: "#fff", fontSize: "18px" }}>{camp.name}</h2>
                <div style={{ color: "#757575", fontSize: "12px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                  <span>Role: <strong style={{ color: isGm ? "#bb86fc" : "#03dac6" }}>{isGm ? "Game Master" : "Player"}</strong></span>
                  <span>Joined: {new Date(camp.createdAt).toLocaleDateString()}</span>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* MODALS */}
      {activeModal !== "none" && (
        <div style={{ position: "fixed", inset: 0, backgroundColor: "rgba(0,0,0,0.8)", display: "flex", justifyContent: "center", alignItems: "center", zIndex: 1000 }} onClick={() => setActiveModal("none")}>
          <div style={{ backgroundColor: "#1e1e1e", padding: "30px", borderRadius: "12px", width: "400px", border: "1px solid #444" }} onClick={e => e.stopPropagation()}>
            <h2 style={{ marginTop: 0, color: "#fff" }}>{activeModal === "create" ? "Start a New Campaign" : "Join a Campaign"}</h2>
            <p style={{ color: "#757575", fontSize: "14px", marginBottom: "20px" }}>
              {activeModal === "create" ? "Enter a name for your new adventure." : "Enter the 8-character invite code provided by your GM."}
            </p>
            <input 
              type="text" 
              placeholder={activeModal === "create" ? "Campaign Name" : "Invite Code (e.g. A3B8E1D9)"}
              value={inputValue}
              onChange={e => setInputValue(activeModal === "join" ? e.target.value.toUpperCase() : e.target.value)}
              style={{ width: "100%", padding: "12px", backgroundColor: "#121212", border: "1px solid #333", color: "#fff", borderRadius: "6px", marginBottom: "20px", boxSizing: "border-box" }}
            />
            <div style={{ display: "flex", gap: "10px" }}>
              <button onClick={() => setActiveModal("none")} style={{ flex: 1, padding: "12px", backgroundColor: "#333", color: "#fff", border: "none", borderRadius: "6px", cursor: "pointer", fontWeight: "bold" }}>CANCEL</button>
              <button onClick={activeModal === "create" ? handleCreateCampaign : handleJoinCampaign} style={{ flex: 1, padding: "12px", backgroundColor: activeModal === "create" ? "#bb86fc" : "#03dac6", color: "#000", border: "none", borderRadius: "6px", cursor: "pointer", fontWeight: "bold" }}>
                {activeModal === "create" ? "CREATE" : "JOIN"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}