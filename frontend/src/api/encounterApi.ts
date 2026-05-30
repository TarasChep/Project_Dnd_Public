import { api } from "./axios";

export interface EncounterPredictionReport {
  threatRatio: number;
  survivalProbabilityPercentage: number;
  difficultyCategory: string;
  alliesEstimatedTTK: number;
  enemiesEstimatedTTK: number;
  warningMessages: string[];
}

export const analyzeEncounter = async (id: string): Promise<EncounterPredictionReport> => {
  const response = await api.get(`/encounters/${id}/analyze`);
  return response.data;
};