import React from "react";
import { api } from "../api/axios";
import "../pages/CharacterSheet.css";

interface CombatActionDto {
  actionId: string;
  displayName: string;
  isSpell: boolean;
  isSave: boolean;
  saveDC: number;
  saveStat: string;
  attackBonus: number;
  damageDice: string;
  spellLevel: number;
  actionCost: string;
}

interface Props {
  characterId: string;
  action: CombatActionDto;
  onRollHit: (bonus: number, name: string) => void;
  onRollDamage: (dice: string, name: string) => void;
}

export const CombatActionRow: React.FC<Props> = ({ characterId, action, onRollHit, onRollDamage }) => {
  const handleCastOrDamage = async () => {
    if (action.isSpell && action.spellLevel > 0) {
      try {
        await api.post(`/characters/${characterId}/combat/cast/${action.actionId}`);
      } catch (err: any) {
        alert(err.response?.data?.error || "Not enough spell slots!");
        return;
      }
    }
    onRollDamage(action.damageDice, action.displayName);
  };

  return (
    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", backgroundColor: "#1e1e1e", padding: "10px", borderRadius: "8px", border: "1px solid #333", marginBottom: "8px" }}>
      <div style={{ flex: 1 }}>
        <div style={{ fontWeight: "bold", color: action.isSpell ? "#03dac6" : "#bb86fc", fontSize: "14px" }}>
          {action.displayName} {action.isSpell && <span style={{ fontSize: "10px", color: "#757575" }}>(Lv.{action.spellLevel})</span>}
        </div>
      </div>

      <div className="cs-combat-row__btns">
        {action.isSave ? (
          <button disabled className="cs-combat-btn cs-combat-btn--disabled">
            DC {action.saveDC} {action.saveStat}
          </button>
        ) : (
          <button
            onClick={() => onRollHit(action.attackBonus, action.displayName)}
            className="cs-combat-btn cs-combat-btn--hit"
          >
            {action.attackBonus >= 0 ? `+${action.attackBonus}` : action.attackBonus} TO HIT
          </button>
        )}

        {action.damageDice && (
          <button onClick={handleCastOrDamage} className="cs-combat-btn cs-combat-btn--dmg">
            {action.damageDice} DAMAGE
          </button>
        )}
      </div>
    </div>
  );
};
