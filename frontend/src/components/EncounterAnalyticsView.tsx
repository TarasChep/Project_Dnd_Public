import React, { useState } from "react";
import { api } from "../api/axios";

export interface EncounterPredictionReport {
  threatRatio: number;
  survivalProbabilityPercentage: number;
  difficultyCategory: string;
  alliesEstimatedTTK: number;
  enemiesEstimatedTTK: number;
  warningMessages: string[];
}

export const EncounterAnalyticsView: React.FC<{ encounterId?: string; isNewEncounter?: boolean }> = ({ encounterId, isNewEncounter = false }) => {
  const [report, setReport] = useState<EncounterPredictionReport | null>(null);
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [error, setError] = useState("");

  const analyzeEncounter = async (id: string) => {
    const res = await api.get(`/encounters/${id}/analyze`);
    return res.data as EncounterPredictionReport;
  };

  const handleAnalyze = async () => {
    if (!encounterId || isNewEncounter) return;
    
    setIsAnalyzing(true);
    setError("");
    
    try {
      const data = await analyzeEncounter(encounterId);
      setReport(data);
    } catch (err) {
      setError("Failed to run combat simulation. Ensure you have saved your changes.");
    } finally {
      setIsAnalyzing(false);
    }
  };

  const getDifficultyColor = (category: string) => {
    switch (category) {
      case "Easy": return "#4caf50";
      case "Balanced": return "#03dac6";
      case "Hard": return "#ff9800";
      case "Deadly": return "#f44336";
      case "Party Wipe": return "#b00020";
      default: return "#bb86fc";
    }
  };

  // Progress bar coloring logic
  const getProgressColor = (percentage: number) => {
    if (percentage >= 70) return "#4caf50"; // mostly green
    if (percentage >= 40) return "#ff9800"; // yellow/orange
    return "#f44336"; // mostly red
  };

  return (
    <div style={{ backgroundColor: "#1e1e1e", padding: "20px", borderRadius: "12px", border: "1px solid #333", marginTop: "20px" }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: report ? "20px" : "0" }}>
        <h3 style={{ margin: 0, color: "#bb86fc", textTransform: "uppercase", letterSpacing: "1px" }}>
          Combat Prediction
        </h3>
        
        <button
          onClick={handleAnalyze}
          disabled={isNewEncounter || !encounterId || isAnalyzing}
          style={{
            backgroundColor: (isNewEncounter || !encounterId || isAnalyzing) ? "#333" : "#bb86fc",
            color: (isNewEncounter || !encounterId || isAnalyzing) ? "#757575" : "#000",
            padding: "8px 16px",
            border: "none",
            borderRadius: "6px",
            fontWeight: "bold",
            cursor: (isNewEncounter || !encounterId || isAnalyzing) ? "not-allowed" : "pointer",
            transition: "all 0.2s"
          }}
        >
          {isAnalyzing ? "Analyzing..." : "Analyze Chances"}
        </button>
      </div>

      {error && <div style={{ color: "#f44336", textAlign: "center", marginBottom: "15px", fontSize: "14px" }}>{error}</div>}

      {report && (
        <div>
          {report.warningMessages && report.warningMessages.length > 0 && (
            <div style={{ backgroundColor: "rgba(244, 67, 54, 0.1)", border: "1px solid #f44336", borderRadius: "8px", padding: "12px", marginBottom: "20px" }}>
              <strong style={{ color: "#f44336", fontSize: "14px", display: "block", marginBottom: "8px" }}>⚠️ Warnings:</strong>
              <ul style={{ margin: 0, paddingLeft: "20px", color: "#f44336", fontSize: "13px" }}>
                {report.warningMessages.map((msg, i) => (
                  <li key={i}>{msg}</li>
                ))}
              </ul>
            </div>
          )}

          <div style={{ textAlign: "center", marginBottom: "20px" }}>
            <div style={{ fontSize: "12px", color: "#757575", marginBottom: "4px" }}>DIFFICULTY</div>
            <div style={{ fontSize: "28px", fontWeight: "bold", color: getDifficultyColor(report.difficultyCategory) }}>
              {report.difficultyCategory}
            </div>
          </div>

          <div style={{ marginBottom: "25px" }}>
            <div style={{ display: "flex", justifyContent: "space-between", marginBottom: "8px", fontSize: "13px" }}>
              <span style={{ color: "#b0b0b0" }}>Survival Probability</span>
              <span style={{ fontWeight: "bold", color: "#fff" }}>{report.survivalProbabilityPercentage}%</span>
            </div>
            <div style={{ width: "100%", height: "10px", backgroundColor: "#333", borderRadius: "5px", overflow: "hidden" }}>
              <div style={{ height: "100%", width: `${report.survivalProbabilityPercentage}%`, backgroundColor: getProgressColor(report.survivalProbabilityPercentage), transition: "width 0.5s ease" }} />
            </div>
          </div>

          <div style={{ display: "flex", justifyContent: "space-between", fontSize: "13px", color: "#b0b0b0", backgroundColor: "#121212", padding: "15px", borderRadius: "8px", border: "1px solid #333" }}>
            <div style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
              <span>Est. Time to kill Enemies</span>
              <strong style={{ color: "#4caf50", fontSize: "16px" }}>{report.alliesEstimatedTTK.toFixed(1)} rounds</strong>
            </div>
            <div style={{ width: "1px", backgroundColor: "#333" }} />
            <div style={{ display: "flex", flexDirection: "column", gap: "4px", textAlign: "right" }}>
              <span>Est. Time to wipe Party</span>
              <strong style={{ color: "#f44336", fontSize: "16px" }}>{report.enemiesEstimatedTTK.toFixed(1)} rounds</strong>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};