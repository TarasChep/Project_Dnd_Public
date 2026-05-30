import React, { useState, useEffect, useMemo } from "react";
import { api } from "../api/axios";
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
} from "@dnd-kit/core";
import type { DragEndEvent } from "@dnd-kit/core";
import {
  SortableContext,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
  useSortable,
  arrayMove,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import "./EncounterTracker.css";

// --- Interfaces ---

export interface ParticipantDetailDto {
  id: string;
  characterId: string;
  characterName: string;
  faction: number | string;
  currentHp: number;
  maxHp: number;
  customName?: string;
  initiativeRoll: number;
  computedName?: string;
}

export interface EncounterDetailDto {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  currentTurnIndex: number;
  participants: ParticipantDetailDto[];
}

interface EncounterTrackerProps {
  isGm: boolean;
  currentUserId: string;
  encounterId: string;
  onClose?: () => void;
}

// --- Sortable Card Component ---

interface SortableCardProps {
  participant: ParticipantDetailDto;
  isActiveTurn: boolean;
  isGm: boolean;
  onRemove: (id: string) => void;
  onInitiativeChange: (id: string, val: number) => void;
}

const SortableParticipantCard = ({ participant, isActiveTurn, isGm, onRemove, onInitiativeChange }: SortableCardProps) => {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: participant.id, disabled: !isGm });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    zIndex: isDragging ? 100 : 1,
    opacity: isDragging ? 0.8 : 1,
  };

  const displayName = participant.computedName || participant.customName || participant.characterName;

  const getFactionStyle = (faction: number | string) => {
    if (faction === 1 || faction === "Player") return { bg: "#1e3a8a", text: "#bfdbfe", border: "#2563eb", label: "PLY" };
    if (faction === 2 || faction === "Enemy") return { bg: "#7f1d1d", text: "#fecaca", border: "#dc2626", label: "ENM" };
    return { bg: "#374151", text: "#e5e7eb", border: "#6b7280", label: "NPC" };
  };
  
  const factionStyle = getFactionStyle(participant.faction);

  return (
    <div ref={setNodeRef} style={style} className={`et-participant${isActiveTurn ? " et-participant--active" : ""}`}>
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", width: "100%", gap: "10px" }}>
        
        <div style={{ display: "flex", alignItems: "center", gap: "12px", flex: 1, minWidth: 0 }}>
          {isGm && (
            <div className="et-participant__drag" {...attributes} {...listeners} style={{ cursor: "grab", color: "#757575", padding: "0 4px" }}>
              ⠿
            </div>
          )}
          <div style={{
            backgroundColor: factionStyle.bg,
            color: factionStyle.text,
            padding: "2px 6px",
            borderRadius: "4px",
            fontSize: "10px",
            fontWeight: "bold",
            border: `1px solid ${factionStyle.border}`
          }}>
            {factionStyle.label}
          </div>
          <div className="et-participant__info" style={{ display: "flex", alignItems: "center", minWidth: 0 }}>
            <div className="et-participant__name" style={{ whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis", fontWeight: "bold" }}>
              {displayName}
            </div>
          </div>
        </div>

        <div style={{ display: "flex", alignItems: "center", gap: "10px", flexShrink: 0 }}>
        {isGm ? (
          <input
            type="number"
            className="et-participant__init"
            value={participant.initiativeRoll}
            onChange={(e) => onInitiativeChange(participant.id, Number(e.target.value))}
              style={{ width: "50px", textAlign: "center", padding: "4px", borderRadius: "4px", border: "1px solid #333", backgroundColor: "#121212", color: "#fff" }}
          />
        ) : (
            <div className="et-participant__init-display" style={{ width: "50px", textAlign: "center", fontWeight: "bold", color: "#fff" }}>
              {participant.initiativeRoll}
            </div>
        )}

          {isGm && (
            <button
              type="button"
              className="et-participant__remove"
              onClick={(e) => {
                e.stopPropagation();
                onRemove(participant.id);
              }}
              style={{ padding: "4px 8px", backgroundColor: "transparent", color: "#f44336", border: "1px solid #f44336", borderRadius: "4px", cursor: "pointer", fontSize: "12px" }}
            >
              Remove
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

// --- Main Tracker Component ---

export default function EncounterTracker({
  isGm,
  currentUserId,
  encounterId,
  onClose,
}: EncounterTrackerProps) {
  const [encounter, setEncounter] = useState<EncounterDetailDto | null>(null);

  const fetchEncounter = async () => {
    try {
      const res = await api.get(`/encounters/${encounterId}`);
      setEncounter(res.data);
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    if (!encounterId) return;

    fetchEncounter();

    // Автоматичне підтягування кидків гравців кожні 3 секунди
    const interval = setInterval(() => {
      fetchEncounter();
    }, 3000); 

    return () => clearInterval(interval);
  }, [encounterId]);

  const sortedParticipants = useMemo(() => {
    if (!encounter?.participants) return [];
    
    const baseList = [...encounter.participants].sort((a, b) => b.initiativeRoll - a.initiativeRoll);
    
    const nameCounts: Record<string, number> = {};
    baseList.forEach(p => {
       const name = p.customName || p.characterName;
       nameCounts[name] = (nameCounts[name] || 0) + 1;
    });
    
    const nameIndexes: Record<string, number> = {};
    
    return baseList.map(p => {
      const baseName = p.customName || p.characterName;
      let computedName = baseName;
      
      if (nameCounts[baseName] > 1) {
         nameIndexes[baseName] = (nameIndexes[baseName] || 0) + 1;
         computedName = `${baseName} #${nameIndexes[baseName]}`;
      }
      
      return { ...p, computedName };
    });
  }, [encounter]);

  // Feature 1: Tie-Breaker Detection
  const tieBreakers = useMemo(() => {
    const rollMap: Record<number, string[]> = {};
    sortedParticipants.forEach((p) => {
      if (p.initiativeRoll > 0) {
        if (!rollMap[p.initiativeRoll]) rollMap[p.initiativeRoll] = [];
        rollMap[p.initiativeRoll].push(p.computedName || p.customName || p.characterName);
      }
    });

    return Object.entries(rollMap)
      .filter(([_, names]) => names.length > 1)
      .map(([roll, names]) => ({ roll: Number(roll), names }));
  }, [sortedParticipants]);

  // Setup Sensors for DnD
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  // Feature 2: Drag and Drop Logic (GM Only)
  const handleDragEnd = async (event: DragEndEvent) => {
    const { active, over } = event;
    if (!over || active.id === over.id || !encounter) return;

    const oldIndex = sortedParticipants.findIndex((p) => p.id === active.id);
    const newIndex = sortedParticipants.findIndex((p) => p.id === over.id);

    if (oldIndex !== -1 && newIndex !== -1) {
      let newParticipants = arrayMove(sortedParticipants, oldIndex, newIndex);
      const sortedInitiatives = [...sortedParticipants].map(p => p.initiativeRoll).sort((a, b) => b - a);
      
      const updatedList = newParticipants.map((p, idx) => ({
        ...p,
        initiativeRoll: sortedInitiatives[idx]
      }));

      setEncounter({ ...encounter, participants: updatedList });

      try {
        await Promise.all(
          updatedList.map(p => api.patch(`/encounters/participants/${p.id}/initiative`, { initiative: p.initiativeRoll }))
        );
      } catch (err) {
        console.error("Failed to update initiative in backend", err);
        fetchEncounter(); // Force revert on error
      }
    }
  };

  const handleStartEncounter = async () => {
    try {
      await api.patch(`/encounters/${encounterId}/start`);
      fetchEncounter();
    } catch (err) { console.error(err); }
  };

  const handleEndEncounter = async () => {
    if (!confirm("End this encounter? Initiative will be reset.")) return;
    try {
      await api.patch(`/encounters/${encounterId}/end`);
      fetchEncounter();
    } catch (err) { console.error(err); }
  };

  const handleNextTurn = async () => {
    try {
      await api.post(`/encounters/${encounterId}/next-turn`);
      fetchEncounter();
    } catch (err) {
      console.error(err);
    }
  };

  const handleManualInitiativeChange = async (id: string, val: number) => {
    if (!encounter) return;
    const updated = encounter.participants.map(p => p.id === id ? { ...p, initiativeRoll: val } : p);
    setEncounter({ ...encounter, participants: updated });
    
    try {
      await api.patch(`/encounters/participants/${id}/initiative`, { initiative: val });
    } catch (err) {
      console.error(err);
      fetchEncounter();
    }
  };

  const handleRemove = async (id: string) => {
    try {
      await api.delete(`/encounters/${encounterId}/participants/${id}`);
      fetchEncounter();
    } catch (err) {
      console.error(err);
    }
  };

  if (!encounter) return <div className="encounter-tracker" style={{ padding: "20px" }}>Loading…</div>;

  const canGoNext = isGm;

  return (
    <div className="encounter-tracker">
      <header className="encounter-tracker__head">
        <h2 className="encounter-tracker__title">{encounter.name || "Encounter"}</h2>
        <div className="encounter-tracker__head-actions">
          <button type="button" className="encounter-tracker__refresh" onClick={fetchEncounter}>
            ↻ Refresh
          </button>
          {onClose && (
            <button type="button" className="encounter-tracker__close" onClick={onClose} aria-label="Close">
              ✕
            </button>
          )}
        </div>
      </header>

      {tieBreakers.length > 0 && (
        <div className="encounter-tracker__tie">
          <div className="encounter-tracker__tie-title">Tie-breaker required</div>
          {tieBreakers.map((tie, idx) => (
            <div key={idx}>
              {tie.names.join(" and ")} rolled {tie.roll}. Please re-roll.
            </div>
          ))}
        </div>
      )}

      <div className="encounter-tracker__list">
        {sortedParticipants.length === 0 ? (
          <p className="encounter-tracker__empty">No participants in this encounter.</p>
        ) : (
          isGm ? (
            <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
              <SortableContext items={sortedParticipants.map((p) => p.id)} strategy={verticalListSortingStrategy}>
                {sortedParticipants.map((p, idx) => (
                  <SortableParticipantCard
                    key={p.id}
                    participant={p}
                    isActiveTurn={encounter.isActive && idx === encounter.currentTurnIndex}
                    isGm={isGm}
                    onRemove={handleRemove}
                    onInitiativeChange={handleManualInitiativeChange}
                  />
                ))}
              </SortableContext>
            </DndContext>
          ) : (
            <div>
              {sortedParticipants.map((p, idx) => (
                <SortableParticipantCard
                  key={p.id}
                  participant={p}
                  isActiveTurn={encounter.isActive && idx === encounter.currentTurnIndex}
                  isGm={isGm}
                  onRemove={handleRemove}
                  onInitiativeChange={handleManualInitiativeChange}
                />
              ))}
            </div>
          )
        )}
      </div>

      <footer className="encounter-tracker__footer">
        {!encounter.isActive && isGm && (
          <button type="button" className="encounter-tracker__btn-start" onClick={handleStartEncounter}>
            Start encounter
          </button>
        )}

        {encounter.isActive && canGoNext && (
          <button type="button" className="encounter-tracker__btn-next" onClick={handleNextTurn}>
            Next turn
          </button>
        )}

        {encounter.isActive && isGm && (
          <button type="button" className="encounter-tracker__btn-end" onClick={handleEndEncounter}>
            End initiative
          </button>
        )}
      </footer>
    </div>
  );
}
