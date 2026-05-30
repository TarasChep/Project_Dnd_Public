import { useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { api } from "../api/axios";

export default function JoinCampaign() {
  const { inviteCode } = useParams();
  const navigate = useNavigate();

  useEffect(() => {
    if (!inviteCode) {
      navigate("/campaigns");
      return;
    }
    
    api.post(`/campaigns/join/${inviteCode}`)
      .then(() => {
        navigate("/campaigns");
      })
      .catch(err => {
        alert(err.response?.data?.error || "Failed to join campaign. Invalid code or already a member.");
        navigate("/campaigns");
      });
  }, [inviteCode, navigate]);

  return (
    <div style={{ color: "#fff", padding: "50px", textAlign: "center", fontSize: "18px" }}>
      <span style={{ color: "#bb86fc" }}>Joining campaign...</span>
    </div>
  );
}