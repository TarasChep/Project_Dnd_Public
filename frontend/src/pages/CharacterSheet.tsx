import { useEffect, useState, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { api } from "../api/axios";
import { useAuthStore } from "../store/authStore";
import { extractUserIdFromToken } from "../utils/tokenUtils";
import { CombatActionRow } from "../components/CombatActionRow";
import "./CharacterSheet.css";

export interface CharacterSheetProps {
  characterId?: string;
  isGmOverride?: boolean;
}

interface RollResult {
  id: string;
  label: string;
  diceType: number;
  baseRoll: number;
  bonus: number;
  total: number;
  timestamp: number;
  timeString: string;
  customBreakdown?: string;
}

const STAT_MAP: Record<string, number> = {
  strength: 1,
  dexterity: 2,
  constitution: 3,
  intelligence: 4,
  wisdom: 5,
  charisma: 6,
};

const RollButton = ({
  bonus,
  label,
  isPrimary = false,
  onRoll,
}: {
  bonus: number;
  label: string;
  isPrimary?: boolean;
  onRoll: (sides: number, bonus: number, label: string) => void;
}) => (
  <button
    onClick={() => onRoll(20, bonus, label)}
    className={`cs-roll-btn${isPrimary ? " cs-roll-btn--primary" : ""}`}
  >
    {bonus >= 0 ? `+${bonus}` : bonus}
  </button>
);

const ProficiencyCircle = ({
  isProf,
  onClick,
  size = 12,
  disabled = false,
}: {
  isProf: boolean;
  onClick: () => void;
  size?: number;
  disabled?: boolean;
}) => (
  <div
    onClick={(e) => {
      e.stopPropagation();
      if (!disabled) onClick();
    }}
    className={`cs-prof-circle${isProf ? " cs-prof-circle--on" : ""}`}
    style={{
      width: `${size}px`,
      height: `${size}px`,
      cursor: disabled ? "not-allowed" : "pointer",
      opacity: disabled ? 0.5 : 1,
    }}
  />
);

const SkillRow = ({
  name,
  baseMod,
  profBonus,
  dbFieldName,
  initialProf,
  onUpdate,
  onRoll,
  disabled = false,
}: {
  name: string;
  baseMod: number;
  profBonus: number;
  dbFieldName: string;
  initialProf: number;
  onUpdate: (field: string, val: number) => void;
  onRoll: (s: number, b: number, l: string) => void;
  disabled?: boolean;
}) => {
  const [state, setState] = useState(initialProf);

  useEffect(() => {
    setState(initialProf);
  }, [initialProf]);

  const handleToggle = () => {
    if (disabled) return;
    const next = (state + 1) % 3;
    setState(next);
    // Відправляємо PascalCase для C#
    const dotNetField =
      dbFieldName.charAt(0).toUpperCase() + dbFieldName.slice(1);
    onUpdate(dotNetField, next);
  };

  const bonus = Number(baseMod || 0) + state * Number(profBonus || 0);

  return (
    <div className="cs-skill-row">
      <div className="cs-skill-row__left">
        <div
          onClick={handleToggle}
          className={`cs-skill-dot${state === 1 ? " cs-skill-dot--prof" : state === 2 ? " cs-skill-dot--expert" : ""}`}
          style={{
            cursor: disabled ? "not-allowed" : "pointer",
            opacity: disabled ? 0.5 : 1,
          }}
        />
        <span className="cs-skill-row__name">{name}</span>
      </div>
      <RollButton bonus={bonus} label={name} onRoll={onRoll} />
    </div>
  );
};

type StatBlockData = {
  label: string;
  val: number;
  mod: number;
  saveBonus: number;
  prof: boolean;
  db: string;
  saveDb: string;
  skills: { name: string; f: string }[];
  abbr: string;
};

const StatBlockCard = ({
  stat,
  profBonus,
  getSkillProfLevel,
  onStatClick,
  onSaveProfToggle,
  onSkillUpdate,
  onRoll,
}: {
  stat: StatBlockData;
  profBonus: number;
  getSkillProfLevel: (f: string) => number;
  onStatClick: (stat: StatBlockData) => void;
  onSaveProfToggle: (saveDb: string, prof: boolean) => void;
  onSkillUpdate: (field: string, val: number) => void;
  onRoll: (s: number, b: number, l: string) => void;
}) => (
  <article className="cs-card cs-stat-block">
    <header className="cs-stat-block__header">
      <div className="cs-stat-block__title-wrap">
        <span className="cs-stat-block__abbr">{stat.abbr}</span>
        <button type="button" className="cs-stat-block__name" onClick={() => onStatClick(stat)}>
          {stat.label}
        </button>
      </div>
      <div className="cs-stat-block__score-badge" aria-label={`Score ${stat.val}`}>
        {stat.val || 0}
      </div>
    </header>

    <div className="cs-stat-actions">
      <div className="cs-stat-action">
        <div className="cs-stat-action__label">Check</div>
        <RollButton bonus={stat.mod || 0} label={`${stat.label} Check`} onRoll={onRoll} />
      </div>
      <div className="cs-stat-action cs-stat-action--save">
        <div className="cs-stat-action__label cs-stat-action__label--save">Save</div>
        <div className="cs-stat-action__row">
          <ProficiencyCircle
            isProf={stat.prof}
            onClick={() => onSaveProfToggle(stat.saveDb, stat.prof)}
            size={10}
          />
          <RollButton
            bonus={stat.saveBonus || 0}
            label={`${stat.label} Save`}
            isPrimary
            onRoll={onRoll}
          />
        </div>
      </div>
    </div>

    <div className={`cs-stat-block__skills${stat.skills.length === 0 ? " cs-stat-block__skills--empty" : ""}`}>
      {stat.skills.length === 0 ? (
        <span className="cs-stat-block__skills-placeholder">—</span>
      ) : (
        stat.skills.map((s) => (
          <SkillRow
            key={s.f}
            name={s.name}
            dbFieldName={s.f}
            baseMod={stat.mod}
            profBonus={profBonus}
            initialProf={getSkillProfLevel(s.f)}
            onUpdate={onSkillUpdate}
            onRoll={onRoll}
          />
        ))
      )}
    </div>
  </article>
);

interface AttackDamageDto {
  diceType: number;
  diceCount: number;
  modifierStat?: number | null;
  flatDamageBonus: number;
  damageType: string;
}

interface AttackActionDto {
  id: string;
  name: string;
  isAttackRoll: boolean;
  isProficient: boolean;
  attackStat?: number | null;
  flatAttackBonus: number;
  actionCost?: string | number;
  damages: AttackDamageDto[];
  spellId?: string | null;
  spell?: any;
  saveDC?: number;
}

interface AttacksPanelProps {
  attacks: AttackActionDto[];
  statModifiers: Record<number, number>;
  proficiencyBonus: number;
  spellcastingAbility?: number | null;
  canEdit: boolean;
  onAddAttack: () => void;
  onEditAttack: (attack: AttackActionDto) => void;
  onDeleteAttack: (id: string) => void;
  onRollHit: (hitBonus: number, name: string) => void;
  onRollDamage: (attack: AttackActionDto) => void;
}

const AttacksPanel: React.FC<AttacksPanelProps> = ({
  attacks,
  statModifiers,
  proficiencyBonus,
  spellcastingAbility,
  canEdit,
  onAddAttack,
  onEditAttack,
  onDeleteAttack,
  onRollHit,
  onRollDamage,
}) => {
  const getStatMod = (stat: any) => {
    if (stat === null || stat === undefined || stat === "") return 0;
    if (typeof stat === "string") {
       const mapped = STAT_MAP[stat.toLowerCase()];
       if (mapped !== undefined) return statModifiers[mapped] || 0;
       return statModifiers[Number(stat)] || 0;
    }
    return statModifiers[Number(stat)] || 0;
  };

  const getHitBonus = (attack: AttackActionDto) => {
    const effectiveAttackStat = attack.attackStat ?? (attack.spellId ? spellcastingAbility : null);
    const statMod = getStatMod(effectiveAttackStat);
    const prof = attack.isProficient ? Number(proficiencyBonus) : 0;
    return statMod + prof + (Number(attack.flatAttackBonus) || 0);
  };

  const formatDamage = (damages: AttackDamageDto[]) => {
    if (!damages || damages.length === 0) return "No damage";
    return damages
      .map((d) => {
        const modStat = getStatMod(d.modifierStat);
        const flat = Number(d.flatDamageBonus) || 0;
        const dt = typeof (d.diceType as any) === 'string' ? String(d.diceType).replace(/\D/g, '') : d.diceType || 8;
        
        let formula = `${d.diceCount || 1}d${dt}`;
        if (flat !== 0) formula += ` ${flat > 0 ? '+' : ''}${flat}`;
        if (d.modifierStat !== null && d.modifierStat !== undefined && (d.modifierStat as any) !== "") {
          formula += ` ${modStat >= 0 ? '+' : ''}${modStat}`;
        }
        
        return `${formula} ${d.damageType ? `(${d.damageType})` : ""}`.trim();
      })
      .join(" + ");
  };

  return (
    <div className="cs-attacks">
      {canEdit && (
        <button onClick={onAddAttack} className="cs-btn-add">
          + ADD ATTACK
        </button>
      )}
      {attacks.length === 0 ? (
        <div className="cs-empty">
          No attacks yet. Click "ADD ATTACK" to create one.
        </div>
      ) : (
        attacks.map((attack) => {
          const hitBonus = getHitBonus(attack);
          return (
            <div key={attack.id} className="cs-attack-card">
              <div className="cs-attack-card__header">
                <div className="cs-attack-card__name">{attack.name}</div>
                <div className="cs-attack-card__actions">
                  <button onClick={() => onEditAttack(attack)} className="cs-btn-edit">EDIT</button>
                  <button onClick={() => onDeleteAttack(attack.id)} className="cs-btn-del">DELETE</button>
                </div>
              </div>
              <div className="cs-attack-card__meta">
                {attack.isAttackRoll ? "Attack Roll" : "Save / No Roll"}
                {attack.isProficient && " • PROFICIENT"}
              </div>

              <div className="cs-attack-card__rolls">
                {attack.isAttackRoll ? (
                  <button
                    onClick={() => onRollHit(hitBonus, attack.name)}
                    className="cs-attack-roll-btn cs-attack-roll-btn--hit"
                  >
                    <span className="cs-attack-roll-btn__label">TO-HIT</span>
                    <span className="cs-attack-roll-btn__value">
                      {hitBonus >= 0 ? `+${hitBonus}` : hitBonus}
                    </span>
                  </button>
                ) : (
                  <div className="cs-attack-roll-btn cs-attack-roll-btn--static">
                    <span className="cs-attack-roll-btn__label">
                      {attack.spell?.saveStat ? `${attack.spell.saveStat} SAVE DC` : "SAVE DC"}
                    </span>
                    <span className="cs-attack-roll-btn__value">{attack.saveDC || 10}</span>
                  </div>
                )}
                <button
                  onClick={() => onRollDamage(attack)}
                  className="cs-attack-roll-btn cs-attack-roll-btn--dmg"
                >
                  <span className="cs-attack-roll-btn__label">DAMAGE</span>
                  <span className="cs-attack-roll-btn__value">🎲 {formatDamage(attack.damages)}</span>
                </button>
              </div>
            </div>
          );
        })
      )}
    </div>
  );
};

const RichTextEditor = ({ content, canEdit, onBlur }: { content: string, canEdit: boolean, onBlur: (val: string) => void }) => {
  const editorRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (editorRef.current && editorRef.current.innerHTML !== (content || "")) {
       editorRef.current.innerHTML = content || "";
    }
  }, [content]);

  const handleBlur = () => {
    if (editorRef.current) {
      const html = editorRef.current.innerHTML;
      if (html !== (content || "")) {
        onBlur(html);
      }
    }
  };

  const execCmd = (cmd: string) => {
    document.execCommand(cmd, false, undefined);
    if (editorRef.current) editorRef.current.focus();
  };

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "5px" }}>
      {canEdit && (
        <div style={{ display: "flex", gap: "5px", backgroundColor: "#222", padding: "5px", borderRadius: "4px" }}>
          <button onMouseDown={e => e.preventDefault()} onClick={() => execCmd('bold')} style={{ fontWeight: "bold", background: "none", border: "none", color: "#fff", cursor: "pointer", padding: "4px 8px" }}>B</button>
          <button onMouseDown={e => e.preventDefault()} onClick={() => execCmd('italic')} style={{ fontStyle: "italic", background: "none", border: "none", color: "#fff", cursor: "pointer", padding: "4px 8px" }}>I</button>
          <button onMouseDown={e => e.preventDefault()} onClick={() => execCmd('underline')} style={{ textDecoration: "underline", background: "none", border: "none", color: "#fff", cursor: "pointer", padding: "4px 8px" }}>U</button>
        </div>
      )}
      <div style={{ backgroundColor: "#121212", border: "1px solid #333", borderRadius: "4px", display: "flex", flexDirection: "column" }}>
        <div
          ref={editorRef}
          contentEditable={canEdit}
          suppressContentEditableWarning
          onBlur={handleBlur}
          style={{
            width: "100%",
            minHeight: "30px",
            padding: "10px",
            color: "#fff",
            outline: "none",
            boxSizing: "border-box",
            wordBreak: "break-word"
          }}
        />
      </div>
    </div>
  );
};

const PassivesPanel = ({ passives, canEdit, onUpdate }: { passives: string, canEdit: boolean, onUpdate: (val: string) => void }) => {
  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "8px", marginTop: "10px" }}>
      <h3 style={{ margin: "0", color: "#bb86fc", fontSize: "16px", borderBottom: "1px solid #333", paddingBottom: "8px" }}>PASSIVES</h3>
      <RichTextEditor content={passives} canEdit={canEdit} onBlur={onUpdate} />
    </div>
  );
};

const TrackersPanel = ({ trackers, canEdit, onAddTracker, onEditTracker, onDeleteTracker, onAdjustTracker, onUpdateTrackerDescription }: any) => {
  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "12px", marginTop: "10px" }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", borderBottom: "1px solid #333", paddingBottom: "8px" }}>
        <h3 style={{ margin: "0", color: "#bb86fc", fontSize: "16px" }}>TRACKERS</h3>
        {canEdit && (
          <button onClick={onAddTracker} style={{ padding: "4px 8px", backgroundColor: "transparent", color: "#03dac6", border: "1px solid #03dac6", borderRadius: "4px", fontWeight: "bold", cursor: "pointer", fontSize: "10px" }}>
            + ADD TRACKER
          </button>
        )}
      </div>
      
      <div style={{ display: "flex", flexDirection: "column", gap: "10px" }}>
        {trackers.length === 0 ? (
          <div style={{ color: "#757575", fontSize: "12px", textAlign: "center", padding: "10px 0" }}>No trackers active.</div>
        ) : (
          trackers.map((t: any) => (
            <div key={t.id} style={{ display: "flex", flexDirection: "column", backgroundColor: "#1a1a1a", borderRadius: "6px", border: "1px solid #333", overflow: "hidden" }}>
              <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", padding: "8px 12px", backgroundColor: "#1a1a1a" }}>
                <div>
                  <div style={{ fontWeight: "bold", color: "#fff", fontSize: "13px" }}>{t.name}</div>
                  <div style={{ fontSize: "10px", color: "#757575", marginTop: "2px" }}>
                    Resets on: {t.resetCondition === "ShortRest" ? "Short Rest" : t.resetCondition === "LongRest" ? "Long Rest" : t.resetCondition === "Dawn" ? "Dawn" : "None"}
                  </div>
                </div>
                <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
                  <div style={{ display: "flex", alignItems: "center", gap: "8px", backgroundColor: "#121212", padding: "2px 6px", borderRadius: "4px", border: "1px solid #444" }}>
                    <button onClick={() => onAdjustTracker(t.id, -1)} disabled={t.currentValue <= 0 || !canEdit} style={{ background: "none", border: "none", color: "#f44336", fontSize: "16px", cursor: (t.currentValue <= 0 || !canEdit) ? "not-allowed" : "pointer", opacity: (t.currentValue <= 0 || !canEdit) ? 0.3 : 1 }}>-</button>
                    <span style={{ fontWeight: "bold", width: "35px", textAlign: "center", fontSize: "13px" }}>{t.currentValue} <span style={{ color: "#757575", fontSize: "11px" }}>/ {t.maxValue}</span></span>
                    <button onClick={() => onAdjustTracker(t.id, 1)} disabled={t.currentValue >= t.maxValue || !canEdit} style={{ background: "none", border: "none", color: "#4caf50", fontSize: "16px", cursor: (t.currentValue >= t.maxValue || !canEdit) ? "not-allowed" : "pointer", opacity: (t.currentValue >= t.maxValue || !canEdit) ? 0.3 : 1 }}>+</button>
                  </div>
                  {canEdit && (
                    <div style={{ display: "flex", gap: "5px" }}>
                      <button onClick={() => onEditTracker(t)} style={{ padding: "4px 8px", fontSize: "9px", backgroundColor: "#333", color: "#fff", border: "none", borderRadius: "4px", cursor: "pointer" }}>EDIT</button>
                      <button onClick={() => onDeleteTracker(t.id)} style={{ padding: "4px 8px", fontSize: "9px", backgroundColor: "transparent", border: "1px solid #f44336", color: "#f44336", borderRadius: "4px", cursor: "pointer" }}>DEL</button>
                    </div>
                  )}
                </div>
              </div>
              <div style={{ borderTop: "1px dashed #333", padding: "8px", backgroundColor: "#121212" }}>
                <RichTextEditor 
                  content={t.description || ""} 
                  canEdit={canEdit} 
                  onBlur={(val) => onUpdateTrackerDescription(t.id, val)} 
                />
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

const SpellsPanel = ({ spellSlots, spellActions, canEdit, characterId, spellcastingAbility, onUpdateSpellcastingAbility, onAddSpell, onSaveSlot, onAdjustSlot, onRollHit, onRollDamage }: any) => {
  const spellsByLevel: Record<number, any[]> = {};
  (spellActions || []).forEach((sa: any) => {
     if (!spellsByLevel[sa.spellLevel]) spellsByLevel[sa.spellLevel] = [];
     spellsByLevel[sa.spellLevel].push(sa);
  });

  const sortedLevels = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "15px" }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
          <h3 style={{ margin: "0", color: "#bb86fc" }}>SPELLS & SLOTS</h3>
          <div style={{ display: "flex", alignItems: "center", gap: "5px", backgroundColor: "#121212", padding: "4px 8px", borderRadius: "6px", border: "1px solid #333" }}>
            <span style={{ fontSize: "10px", color: "#757575", fontWeight: "bold" }}>MAGIC STAT:</span>
            <select
              value={spellcastingAbility ?? 0}
              onChange={(e) => {
                if (canEdit && onUpdateSpellcastingAbility) {
                  onUpdateSpellcastingAbility(Number(e.target.value));
                }
              }}
              disabled={!canEdit}
              style={{
                background: "none",
                border: "none",
                color: "#03dac6",
                fontSize: "12px",
                fontWeight: "bold",
                cursor: canEdit ? "pointer" : "not-allowed",
                outline: "none",
                padding: 0
              }}
            >
              <option value={0} style={{ color: "#000" }}>None</option>
              <option value="1" style={{ color: "#000" }}>STR</option>
              <option value="2" style={{ color: "#000" }}>DEX</option>
              <option value="3" style={{ color: "#000" }}>CON</option>
              <option value="4" style={{ color: "#000" }}>INT</option>
              <option value="5" style={{ color: "#000" }}>WIS</option>
              <option value="6" style={{ color: "#000" }}>CHA</option>
            </select>
          </div>
        </div>
        <div style={{ display: "flex", gap: "10px" }}>
          {canEdit && (
            <button onClick={onAddSpell} style={{ padding: "6px 10px", backgroundColor: "transparent", color: "#03dac6", border: "1px solid #03dac6", borderRadius: "6px", fontWeight: "bold", cursor: "pointer", fontSize: "11px" }}>
              🔍 ADD SPELL
            </button>
          )}
        </div>
      </div>

      {sortedLevels.map((level: number) => {
          const slot = (spellSlots || []).find((s:any) => s.level === level);
          const spells = spellsByLevel[level] || [];
          return (
          <div key={level} style={{ backgroundColor: "#1e1e1e", borderRadius: "8px", border: "1px solid #333", padding: "10px" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", borderBottom: "1px dashed #444", paddingBottom: "5px", marginBottom: "10px" }}>
              <div style={{ fontWeight: "bold", color: "#03dac6", fontSize: "13px" }}>{level === 0 ? "Cantrips" : `Level ${level} Slots`}</div>
              {level > 0 && (
                <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                  <button onClick={() => onAdjustSlot(slot?.id, -1)} disabled={!slot || slot.currentValue <= 0 || !canEdit} style={{ background: "none", border: "none", color: "#f44336", cursor: (!slot || slot.currentValue <= 0 || !canEdit) ? "not-allowed" : "pointer", opacity: (!slot || slot.currentValue <= 0 || !canEdit) ? 0.3 : 1 }}>-</button>
                  <span style={{ fontSize: "12px", fontWeight: "bold" }}>{slot ? slot.currentValue : 0} / {slot ? slot.maxValue : 0}</span>
                  <button onClick={() => onAdjustSlot(slot?.id, 1)} disabled={!slot || slot.currentValue >= slot.maxValue || !canEdit} style={{ background: "none", border: "none", color: "#4caf50", cursor: (!slot || slot.currentValue >= slot.maxValue || !canEdit) ? "not-allowed" : "pointer", opacity: (!slot || slot.currentValue >= slot.maxValue || !canEdit) ? 0.3 : 1 }}>+</button>
                  {canEdit && (
                    <button onClick={() => onSaveSlot(level, slot?.id, slot ? slot.maxValue : 0)} style={{ background: "transparent", border: "1px solid #bb86fc", color: "#bb86fc", borderRadius: "4px", fontSize: "10px", padding: "2px 6px", cursor: "pointer", marginLeft: "5px" }}>EDIT MAX</button>
                  )}
                </div>
              )}
            </div>
            {spells.length === 0 ? (
               <div style={{ color: "#757575", fontSize: "11px", fontStyle: "italic", textAlign: "center", padding: "5px 0" }}>
                 No spells for this level.
               </div>
            ) : (
               spells.map((action: any) => (
                  <CombatActionRow 
                     key={action.actionId}
                     characterId={characterId}
                     action={action}
                     onRollHit={(bonus, name) => onRollHit(bonus, name)}
                     onRollDamage={() => onRollDamage(action.originalAttack)}
                  />
               ))
            )}
          </div>
        )})}
    </div>
  );
};

export default function CharacterSheet({ characterId, isGmOverride }: CharacterSheetProps) {
  const { id: routeId } = useParams();
  const id = characterId || routeId;
  const navigate = useNavigate();
  const [char, setChar] = useState<any | null>(null);
  const [loading, setLoading] = useState(true);

  const token = useAuthStore((state) => state.token);
  const currentUserId = extractUserIdFromToken(token);

  const [rolls, setRolls] = useState<RollResult[]>([]);
  const [isDiceMenuOpen, setIsDiceMenuOpen] = useState(false);
  const [sendRollsToDiscord, setSendRollsToDiscord] = useState(true);
  const [activeTab, setActiveTab] = useState<
    "attacks" | "abilities" | "equipment" | "spells" | "notes"
  >("attacks");

  const [localSpeed, setLocalSpeed] = useState<number | "">("");
  const [localAc, setLocalAc] = useState<number | "">("");

  const [activeModal, setActiveModal] = useState<
    "none" | "settings" | "hp" | "stat" | "xp" | "wallet" | "initiative" | "attack" | "attackEdit" | "tracker" | "rest" | "spellSlot" | "spellCompendium"
  >("none");
  const [modalData, setModalData] = useState<any>(null);
  const [modalInputValue, setModalInputValue] = useState<string>("");
  const [walletCurrency, setWalletCurrency] = useState<
    "platinum" | "gold" | "silver" | "copper"
  >("gold");
  
  // Attack form state
  const [attackForm, setAttackForm] = useState<any>({
    name: "",
    isAttackRoll: true,
    isProficient: false,
    attackStat: null,
    flatAttackBonus: 0,
    actionCost: "Action",
    damages: [],
  });
  const [editingAttackId, setEditingAttackId] = useState<string | null>(null);

  const [trackerForm, setTrackerForm] = useState<any>({
    name: "",
    maxValue: 1,
    currentValue: 1,
    resetCondition: 1
  });
  const [editingTrackerId, setEditingTrackerId] = useState<string | null>(null);

  const [spellSlotForm, setSpellSlotForm] = useState<any>({ id: null, level: 1, maxValue: 1 });
  const [availableSpells, setAvailableSpells] = useState<any[]>([]);

  useEffect(() => {
    fetchData();
  }, [id]);

  const fetchData = () => {
    api
      .get(`/characters/${id}`)
      .then((res) => {
        const data = res.data;
        setChar(data);
        setLocalSpeed(data.speed ?? "");
        setLocalAc(data.armorClass ?? "");
        setLoading(false);
      })
      .catch(() => navigate("/"));
  };

  if (loading || !char)
    return (
      <div className="cs-page cs-loading">
        <div className="cs-loading-spinner" />
        <span>Завантаження аркуша персонажа...</span>
      </div>
    );

  // DELEGATED GM ACCESS: Determine if current user can edit this character
  const canEdit = (char.userId === currentUserId) || !!isGmOverride || !!char.canEdit;

  const updateCharacter = async (
    endpoint: string,
    method: "put" | "patch",
    payload: any,
  ) => {
    try {
      const res = await api[method](
        `/characters/${char.id}${endpoint}`,
        payload,
      );
      if (res.data) {
        setChar(res.data);
        if (endpoint === "/progression") {
          setLocalSpeed(res.data.speed ?? "");
          setLocalAc(res.data.armorClass ?? "");
        }
      } else fetchData();
    } catch (err) {
      console.error("Update failed", err);
    }
  };

  const handleHeaderStatUpdate = (field: string, val: number | "") => {
    if (val === "") return;
    // PascalCase for C#
    const dotNetField = field.charAt(0).toUpperCase() + field.slice(1);
    updateCharacter("/progression", "put", { [dotNetField]: Number(val) });
  };

  const closeModals = () => {
    setActiveModal("none");
    setModalData(null);
    setModalInputValue("");
  };

  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) {
      if (activeModal === "stat" && modalInputValue !== "") {
        handleStatSave();
      } else if (activeModal === "initiative" && modalInputValue !== "") {
        updateCharacter("/progression", "put", {
          AdditionalInitiativeBonus: Number(modalInputValue),
        });
        closeModals();
      } else {
        closeModals();
      }
    }
  };

  const handleHpAction = async (type: "damage" | "heal" | "temp") => {
    if (modalInputValue === "" || modalInputValue === "0") return;
    const amount = Number(modalInputValue);
    // Using PascalCase 'Amount'
    if (type === "damage")
      await updateCharacter("/health", "patch", { Amount: -Math.abs(amount) });
    else if (type === "heal")
      await updateCharacter("/health", "patch", { Amount: Math.abs(amount) });
    else if (type === "temp")
      await updateCharacter("/vitals", "patch", {
        TemporaryHp: Math.abs(amount),
      });
    setModalInputValue("");
  };

  const handleStatSave = async () => {
    if (modalInputValue === "") return;
    // ТУТ БУЛА ПОМИЛКА: було modalData.dbField, а треба modalData.db
    await updateCharacter("/progression", "put", {
      [modalData.db]: Number(modalInputValue),
    });
    closeModals();
  };

  const handleXpAction = async (isAdd: boolean) => {
    if (modalInputValue === "") return;
    const amount = Number(modalInputValue);
    await updateCharacter("/vitals", "patch", {
      AddXp: isAdd ? Math.abs(amount) : -Math.abs(amount),
    });
    closeModals();
  };

  const handleWalletAction = async (isAdd: boolean) => {
    if (modalInputValue === "") return;
    const amount = Number(modalInputValue);
    const currentAmount = char[walletCurrency] || 0;
    const newTotal = isAdd
      ? currentAmount + amount
      : Math.max(0, currentAmount - amount);
    // Using PascalCase for everything
    const payload = {
      Platinum: char.platinum || 0,
      Gold: char.gold || 0,
      Silver: char.silver || 0,
      Copper: char.copper || 0,
      [walletCurrency.charAt(0).toUpperCase() + walletCurrency.slice(1)]:
        newTotal,
    };
    setChar({ ...char, [walletCurrency]: newTotal });
    await updateCharacter("/wallet", "patch", payload);
    setModalInputValue("");
  };

  const handlePerformRest = async (restType: number) => {
    try {
      const res = await api.post(`/characters/${char.id}/rest`, { RestType: restType });
      if (res.data) setChar(res.data);
      closeModals();
    } catch (err) {
      console.error(err);
    }
  };

  const handleSpendHitDice = async () => {
    try {
      const res = await api.post(`/characters/${char.id}/hitdice/spend`, { Count: 1 });
      if (res.data) {
        const result = res.data;
        setChar((prev: any) => ({
          ...prev,
          currentHp: result.newCurrentHp,
          hitDiceCurrent: result.hitDiceRemaining
        }));

        const sides = Number(String(char.hitDiceType).replace(/\D/g, "")) || 8;
        const baseRoll = result.totalHealed - result.constitutionModifier;
        
        const rollId = Math.random().toString(36).substring(7);
        const label = "Hit Dice (Heal)";
        const newRoll = {
          id: rollId,
          label,
          diceType: sides,
          baseRoll,
          bonus: result.constitutionModifier,
          total: result.totalHealed,
          timestamp: Date.now(),
          timeString: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit", second: "2-digit" }),
        };
        setRolls((prev) => [newRoll, ...prev]);
        sendToDiscord(sides, baseRoll, result.constitutionModifier, result.totalHealed, label);
        setTimeout(() => setRolls((prev) => prev.filter((r) => r.id !== rollId)), 15000);
      }
    } catch (err: any) {
      alert(err.response?.data?.error || "Failed to spend hit dice");
    }
  };

  const statModifiers: Record<number, number> = {
    1: char.strengthModifier || 0,
    2: char.dexterityModifier || 0,
    3: char.constitutionModifier || 0,
    4: char.intelligenceModifier || 0,
    5: char.wisdomModifier || 0,
    6: char.charismaModifier || 0,
  };

  const getStatMod = (stat: any) => {
    if (stat === null || stat === undefined || stat === "") return 0;
    if (typeof stat === "string") {
       const mapped = STAT_MAP[stat.toLowerCase()];
       if (mapped !== undefined) return statModifiers[mapped] || 0;
       return statModifiers[Number(stat)] || 0;
    }
    return statModifiers[Number(stat)] || 0;
  };

  const parseStatToNumber = (val: any) => {
    if (val === null || val === undefined || val === "") return null;
    if (typeof val === "string") {
        const mapped = STAT_MAP[val.toLowerCase()];
        if (mapped !== undefined) return mapped;
        const num = Number(val);
        return isNaN(num) ? null : num;
    }
    const num = Number(val);
    return isNaN(num) ? null : num;
  };

  const parseDiceType = (dt: any) => {
    if (typeof dt === 'string') {
       const num = parseInt(dt.replace(/\D/g, ''), 10);
       return isNaN(num) ? 8 : num;
    }
    return Number(dt) || 8;
  };

  const rollDamageFormula = (attack: any) => {
    if (!attack.damages || attack.damages.length === 0) return { total: 0, breakdown: "" };
    
    let totalDamage = 0;
    const parts: string[] = [];

    attack.damages.forEach((d: any) => {
      let damageRoll = 0;
      const diceCount = Number(d.diceCount) || 1;
      const dtStr = typeof (d.diceType as any) === 'string' ? String(d.diceType).replace(/\D/g, '') : d.diceType;
      const diceType = Number(dtStr) || 8;
      
      for (let i = 0; i < diceCount; i++) {
        damageRoll += Math.floor(Math.random() * diceType) + 1;
      }
      
      const modStat = getStatMod(d.modifierStat);
      const flatBonus = Number(d.flatDamageBonus) || 0;
      const subtotal = damageRoll + modStat + flatBonus;
      totalDamage += subtotal;
      
      const flatStr = flatBonus !== 0 ? ` ${flatBonus > 0 ? '+' : ''}${flatBonus}` : '';
      const statStr = (d.modifierStat !== null && d.modifierStat !== undefined && d.modifierStat !== "") 
                        ? ` ${modStat >= 0 ? '+' : ''}${modStat}` : '';
      
      parts.push(
        `${damageRoll}/${diceCount}d${diceType}${flatStr}${statStr} (${d.damageType || "damage"})`
      );
    });

    return {
      total: totalDamage,
      breakdown: `(${parts.join(" + ")}) = ${totalDamage}`,
    };
  };

  // ===== ATTACK HANDLERS =====
  const handleAddAttack = async () => {
    if (!attackForm.name.trim()) return;
    try {
      const payload = {
        Name: attackForm.name,
        IsAttackRoll: attackForm.isAttackRoll,
        IsProficient: attackForm.isProficient,
        AttackStat: attackForm.attackStat,
        FlatAttackBonus: Number(attackForm.flatAttackBonus) || 0,
        ActionCost: attackForm.actionCost === "BonusAction" ? 1 : 0,
        SpellId: attackForm.spellId,
        Damages: attackForm.damages.map((d: any) => ({
          DiceType: Number(d.diceType) || 8,
          DiceCount: Number(d.diceCount) || 1,
          ModifierStat: d.modifierStat,
          FlatDamageBonus: Number(d.flatDamageBonus) || 0,
          DamageType: d.damageType,
        })),
      };
      const res = await api.post(`/characters/${char.id}/attacks`, payload);
      if (res.data) setChar(res.data);
      resetAttackForm();
      closeModals();
    } catch (err) {
      console.error("Add attack failed", err);
    }
  };

  const handleEditAttack = async () => {
    if (!attackForm.name.trim() || !editingAttackId) return;
    try {
      const payload = {
        Name: attackForm.name,
        IsAttackRoll: attackForm.isAttackRoll,
        IsProficient: attackForm.isProficient,
        AttackStat: attackForm.attackStat,
        FlatAttackBonus: Number(attackForm.flatAttackBonus) || 0,
        ActionCost: attackForm.actionCost === "BonusAction" ? 1 : 0,
        SpellId: attackForm.spellId,
        Damages: attackForm.damages.map((d: any) => ({
          DiceType: Number(d.diceType) || 8,
          DiceCount: Number(d.diceCount) || 1,
          ModifierStat: d.modifierStat,
          FlatDamageBonus: Number(d.flatDamageBonus) || 0,
          DamageType: d.damageType,
        })),
      };
      const res = await api.put(
        `/characters/${char.id}/attacks/${editingAttackId}`,
        payload
      );
      if (res.data) setChar(res.data);
      resetAttackForm();
      closeModals();
    } catch (err) {
      console.error("Edit attack failed", err);
    }
  };

  const handleDeleteAttack = async (attackId: string) => {
    if (!confirm("Delete this attack?")) return;
    try {
      const res = await api.delete(`/characters/${char.id}/attacks/${attackId}`);
      if (res.data) setChar(res.data);
    } catch (err) {
      console.error("Delete attack failed", err);
    }
  };

  const rollDamageAction = (attackName: string, total: number, breakdown: string) => {
    const rollId = Math.random().toString(36).substring(7);
    const label = `${attackName} (damage)`;
    const newRoll = {
      id: rollId,
      label,
      diceType: 0,
      baseRoll: 0,
      bonus: 0,
      total,
      timestamp: Date.now(),
      timeString: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit", second: "2-digit" }),
      customBreakdown: breakdown
    };
    setRolls((prev) => [newRoll, ...prev]);

    if (sendRollsToDiscord && char?.discordWebhookUrl) {
      let embedColor = 3447003;
      if (char.themeColor) {
        try { embedColor = parseInt(char.themeColor.replace("#", ""), 16); } catch (e) {}
      }
      const payload = {
        username: char.name,
        embeds: [
          {
            title: `[ROLL] ${label}`,
            color: embedColor,
            description: breakdown,
            thumbnail: char.imageUrl ? { url: char.imageUrl } : undefined,
            author: { name: char.name },
          },
        ],
      };
      fetch(char.discordWebhookUrl, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(payload) }).catch(() => {});
    }

    setTimeout(() => setRolls((prev) => prev.filter((r) => r.id !== rollId)), 15000);
  };

  const handleRollAttackDamage = async (attack: any) => {
    const { total, breakdown } = rollDamageFormula(attack);
    rollDamageAction(attack.name, total, breakdown);
  };

  const resetAttackForm = () => {
    setAttackForm({
      name: "",
      isAttackRoll: true,
      isProficient: false,
      attackStat: null,
      flatAttackBonus: 0,
      spellId: null,
      actionCost: "Action",
      damages: [],
    });
    setEditingAttackId(null);
  };

  const openAttackEditModal = (attack: any) => {
    setAttackForm({
      name: attack.name,
      isAttackRoll: attack.isAttackRoll,
      isProficient: attack.isProficient,
      attackStat: parseStatToNumber(attack.attackStat),
      flatAttackBonus: attack.flatAttackBonus,
      actionCost: attack.actionCost === 1 || attack.actionCost === "BonusAction" ? "BonusAction" : "Action",
      spellId: attack.spellId,
      damages: (attack.damages || []).map((d: any) => ({
         ...d,
         diceType: parseDiceType(d.diceType),
         modifierStat: parseStatToNumber(d.modifierStat),
      })),
    });
    setEditingAttackId(attack.id);
    setActiveModal("attackEdit");
  };

  // ===== TRACKER HANDLERS =====
  const handleAddTracker = async () => {
    if (!trackerForm.name.trim()) return;
    try {
      const payload = {
        Name: trackerForm.name,
        MaxValue: trackerForm.maxValue,
        ResetCondition: trackerForm.resetCondition
      };
      const res = await api.post(`/characters/${char.id}/trackers`, payload);
      if (res.data) setChar(res.data);
      closeModals();
    } catch (err) { console.error(err); }
  };

  const handleEditTracker = async () => {
    if (!trackerForm.name.trim() || !editingTrackerId) return;
    try {
      const payload = {
        Name: trackerForm.name,
        MaxValue: trackerForm.maxValue,
        CurrentValue: trackerForm.currentValue,
        ResetCondition: trackerForm.resetCondition
      };
      const res = await api.patch(`/characters/${char.id}/trackers/${editingTrackerId}`, payload);
      if (res.data) setChar(res.data);
      closeModals();
    } catch (err) { console.error(err); }
  };

  const handleDeleteTracker = async (trackerId: string) => {
    if (!confirm("Delete this tracker?")) return;
    try {
      const res = await api.delete(`/characters/${char.id}/trackers/${trackerId}`);
      if (res.data) setChar(res.data);
    } catch (err) { console.error(err); }
  };

  const handleAdjustTracker = async (trackerId: string, adjustValue: number) => {
    try {
      const res = await api.patch(`/characters/${char.id}/trackers/${trackerId}`, { AdjustValue: adjustValue });
      if (res.data) setChar(res.data);
    } catch (err) { console.error(err); }
  };

  const handleUpdateTrackerDescription = async (trackerId: string, description: string) => {
    try {
      const res = await api.patch(`/characters/${char.id}/trackers/${trackerId}`, { Description: description });
      if (res.data) setChar(res.data);
    } catch (err) { console.error(err); }
  };

  // ===== SPELL SLOT HANDLERS =====
  const handleSaveSlot = async () => {
    try {
      if (Number(spellSlotForm.maxValue) <= 0 && spellSlotForm.id) {
        const res = await api.delete(`/characters/${char.id}/spellslots/${spellSlotForm.id}`);
        if (res.data) setChar(res.data);
      } else if (spellSlotForm.id) {
        const res = await api.patch(`/characters/${char.id}/spellslots/${spellSlotForm.id}`, { MaxValue: Number(spellSlotForm.maxValue) });
        if (res.data) setChar(res.data);
      } else if (Number(spellSlotForm.maxValue) > 0) {
        const res = await api.post(`/characters/${char.id}/spellslots`, { Level: Number(spellSlotForm.level), MaxValue: Number(spellSlotForm.maxValue) });
        if (res.data) setChar(res.data);
      }
      closeModals();
    } catch (err) { console.error(err); }
  };

  const handleAdjustSlot = async (slotId: string | undefined, adjustValue: number) => {
    if (!slotId) return;
    try {
      const res = await api.patch(`/characters/${char.id}/spellslots/${slotId}`, { AdjustValue: adjustValue });
      if (res.data) setChar(res.data);
    } catch (err) { console.error(err); }
  };

  const openSpellCompendium = async () => {
    setActiveModal("spellCompendium");
    try {
      const res = await api.get("/spells");
      setAvailableSpells(res.data);
    } catch(err) {
      console.error(err);
    }
  };

  const handleAddSpellToCharacter = async (spell: any) => {
    try {
      const damages = [];
      if (spell.damageDice) {
         const parts = spell.damageDice.toLowerCase().split("d");
         if (parts.length === 2) {
             damages.push({
                diceCount: Number(parts[0]) || 1,
                diceType: Number(parts[1]) || 8,
                modifierStat: null,
                flatDamageBonus: 0,
                damageType: spell.damageType || "Magic"
             });
         }
      }
      const payload = {
        Name: spell.name,
        IsAttackRoll: !spell.requiresSave && !!spell.damageDice,
        IsProficient: true,
        AttackStat: null,
        FlatAttackBonus: 0,
        SpellId: spell.id,
        Damages: damages
      };
      const res = await api.post(`/characters/${char.id}/attacks`, payload);
      if (res.data) setChar(res.data);
      closeModals();
    } catch(err) {
      console.error(err);
    }
  };

  const sendToDiscord = async (
    sides: number,
    baseRoll: number,
    bonus: number,
    total: number,
    label: string,
  ) => {
    if (!sendRollsToDiscord || !char.discordWebhookUrl) return;
    let embedColor = 3447003;
    if (char.themeColor) {
      try {
        embedColor = parseInt(char.themeColor.replace("#", ""), 16);
      } catch (e) {}
    }

    let critPrefix = "";
    if (sides === 20) {
      if (baseRoll === 20) critPrefix = "🌟 **CRITICAL SUCCESS!**\n";
      else if (baseRoll === 1) critPrefix = "💀 **CRITICAL FAIL!**\n";
    }

    const payload = {
      username: char.name,
      embeds: [
        {
          title: `[ROLL] ${label}`,
          color: embedColor,
          description: `${critPrefix}**(${baseRoll} + ${bonus}) = ${total}**\n*(1d${sides} + ${bonus})*`,
          thumbnail: char.imageUrl ? { url: char.imageUrl } : undefined,
          author: { name: char.name },
        },
      ],
    };
    try {
      await fetch(char.discordWebhookUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });
    } catch (e) {}
  };

  const rollDice = (sides: number, bonus: number, label: string) => {
    const baseRoll = Math.floor(Math.random() * sides) + 1;
    const total = baseRoll + bonus;
    const rollId = Math.random().toString(36).substring(7);
    const newRoll = {
      id: rollId,
      label,
      diceType: sides,
      baseRoll,
      bonus,
      total,
      timestamp: Date.now(),
      timeString: new Date().toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit",
      }),
    };
    setRolls((prev) => [newRoll, ...prev]);
    sendToDiscord(sides, baseRoll, bonus, total, label);
    setTimeout(
      () => setRolls((prev) => prev.filter((r) => r.id !== rollId)),
      15000,
    );
  };

  const removeRoll = (id: string) =>
    setRolls((prev) => prev.filter((r) => r.id !== id));

  const getSkillProfLevel = (dbFieldName: string) => {
    const fieldName = `${dbFieldName.charAt(0).toUpperCase() + dbFieldName.slice(1)}Proficiency`;
    const rawValue = char[dbFieldName + "Proficiency"] ?? char[fieldName] ?? 0;
    return rawValue === "Proficient" || rawValue === 1
      ? 1
      : rawValue === "Expertise" || rawValue === 2
        ? 2
        : 0;
  };

  const getHpColor = () => {
    const ratio = (char.currentHp || 0) / (char.maxHp || 1);
    if (ratio > 0.5) return "#4caf50";
    if (ratio > 0.25) return "#ff9800";
    return "#f44336";
  };

  const hpColor = getHpColor();
  const perceptionState = getSkillProfLevel("perception");
  const passivePerception =
    10 +
    (char.wisdomModifier || 0) +
    perceptionState * (char.proficiencyBonus || 0);
  const currentInitiativeBonus =
    (char.dexterityModifier || 0) + (char.additionalInitiativeBonus || 0);
  const hitDiceNumber = String(char.hitDiceType || "8").replace(/\D/g, "");

  const physicalStats: StatBlockData[] = [
    {
      label: "STRENGTH",
      abbr: "STR",
      val: char.strength,
      mod: char.strengthModifier,
      saveBonus: char.strengthSaveBonus,
      prof: char.isStrengthSaveProficient,
      db: "Strength",
      saveDb: "IsStrengthSaveProficient",
      skills: [{ name: "Athletics", f: "athletics" }],
    },
    {
      label: "DEXTERITY",
      abbr: "DEX",
      val: char.dexterity,
      mod: char.dexterityModifier,
      saveBonus: char.dexteritySaveBonus,
      prof: char.isDexteritySaveProficient,
      db: "Dexterity",
      saveDb: "IsDexteritySaveProficient",
      skills: [
        { name: "Acrobatics", f: "acrobatics" },
        { name: "Sleight of Hand", f: "sleightOfHand" },
        { name: "Stealth", f: "stealth" },
      ],
    },
    {
      label: "CONSTITUTION",
      abbr: "CON",
      val: char.constitution,
      mod: char.constitutionModifier,
      saveBonus: char.constitutionSaveBonus,
      prof: char.isConstitutionSaveProficient,
      db: "Constitution",
      saveDb: "IsConstitutionSaveProficient",
      skills: [],
    },
  ];

  const mentalStats: StatBlockData[] = [
    {
      label: "INTELLIGENCE",
      abbr: "INT",
      val: char.intelligence,
      mod: char.intelligenceModifier,
      saveBonus: char.intelligenceSaveBonus,
      prof: char.isIntelligenceSaveProficient,
      db: "Intelligence",
      saveDb: "IsIntelligenceSaveProficient",
      skills: [
        { name: "Arcana", f: "arcana" },
        { name: "History", f: "history" },
        { name: "Investigation", f: "investigation" },
        { name: "Nature", f: "nature" },
        { name: "Religion", f: "religion" },
      ],
    },
    {
      label: "WISDOM",
      abbr: "WIS",
      val: char.wisdom,
      mod: char.wisdomModifier,
      saveBonus: char.wisdomSaveBonus,
      prof: char.isWisdomSaveProficient,
      db: "Wisdom",
      saveDb: "IsWisdomSaveProficient",
      skills: [
        { name: "Animal Handling", f: "animalHandling" },
        { name: "Insight", f: "insight" },
        { name: "Medicine", f: "medicine" },
        { name: "Perception", f: "perception" },
        { name: "Survival", f: "survival" },
      ],
    },
    {
      label: "CHARISMA",
      abbr: "CHA",
      val: char.charisma,
      mod: char.charismaModifier,
      saveBonus: char.charismaSaveBonus,
      prof: char.isCharismaSaveProficient,
      db: "Charisma",
      saveDb: "IsCharismaSaveProficient",
      skills: [
        { name: "Deception", f: "deception" },
        { name: "Intimidation", f: "intimidation" },
        { name: "Performance", f: "performance" },
        { name: "Persuasion", f: "persuasion" },
      ],
    },
  ];

  const statBlockProps = {
    profBonus: char.proficiencyBonus,
    getSkillProfLevel,
    onStatClick: (stat: StatBlockData) => {
      setActiveModal("stat");
      setModalData(stat);
      setModalInputValue(stat.val || 0);
    },
    onSaveProfToggle: (saveDb: string, prof: boolean) =>
      updateCharacter("/progression", "put", { [saveDb]: !prof }),
    onSkillUpdate: (f: string, v: number) =>
      updateCharacter("/progression", "put", { [f]: v }),
    onRoll: rollDice,
  };

  const spellAttacks = (char.attacks || []).filter((a: any) => a.spellId && a.spell);
  const mappedSpellActions = spellAttacks.map((a: any) => {
      const profBonus = char.proficiencyBonus || 0;
      
      let atkBonus = a.flatAttackBonus || 0;
      const effectiveAttackStat = a.attackStat ?? char.spellcastingAbility;
      if (effectiveAttackStat !== null && effectiveAttackStat !== undefined) {
         atkBonus += getStatMod(effectiveAttackStat);
      }
      if (a.isProficient) atkBonus += profBonus;

      const dmg = a.damages?.[0];
      const damageDice = a.spell.damageDice || (dmg ? `${dmg.diceCount}d${dmg.diceType}` : "");

      return {
          actionId: a.id,
          displayName: a.spell.name,
          isSpell: true,
          isSave: a.spell.requiresSave,
          saveDC: a.saveDC,
          saveStat: a.spell.saveStat || "",
          attackBonus: atkBonus,
          damageDice: damageDice,
          spellLevel: a.spell.level,
          actionCost: a.spell?.castingTime ? a.spell.castingTime : (a.actionCost === 1 || a.actionCost === "BonusAction" ? "BonusAction" : "Action"),
          originalAttack: a
      };
  });

  return (
    <div className="cs-page">
      <div className="cs-grid">
        {/* LEFT COLUMN */}
        <div className="cs-col">
          <div className="cs-card cs-card--accent cs-hero">
            <div className="cs-hero-top">
              <div className="cs-hero-identity">
                <div className="cs-avatar">
                  {char.imageUrl ? (
                    <img
                      src={char.imageUrl}
                      style={{
                        width: "100%",
                        height: "100%",
                        objectFit: "cover",
                      }}
                    />
                  ) : null}
                </div>
                <div>
                  <h1 className="cs-hero-name">{char.name}</h1>
                  <p className="cs-hero-meta">
                    {char.race} {char.class} • <span>Level {char.level}</span>
                  </p>
                </div>
              </div>

              <div className="cs-hero-actions">
              <div className="cs-quick-stats">
                <div className="cs-stat-pill">
                  <div
                    onClick={() => {
                      if (canEdit) {
                        setActiveModal("initiative");
                        setModalInputValue(char.additionalInitiativeBonus || 0);
                      }
                    }}
                    className="cs-stat-pill__label cs-stat-pill__label--purple"
                    style={{
                      cursor: canEdit ? "pointer" : "default",
                      textDecoration: canEdit ? "underline" : "none",
                      opacity: canEdit ? 1 : 0.5,
                    }}
                  >
                    INITIATIVE
                  </div>
                  <div
                    onClick={async () => {
                      try {
                        const res = await api.post(`/characters/${char.id}/roll`, {
                          type: "Initiative",
                          diceSides: 20,
                          diceCount: 1
                        });
                        const rollData = res.data;
                        const newRoll = {
                          id: Math.random().toString(36).substring(7),
                          label: "Initiative",
                          diceType: 20,
                          baseRoll: rollData.rolls[0].value,
                          bonus: rollData.modifier,
                          total: rollData.total,
                          timestamp: Date.now(),
                          timeString: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit", second: "2-digit" }),
                          customBreakdown: rollData.modifierBreakdown
                        };
                        setRolls((prev) => [newRoll, ...prev]);
                        setTimeout(() => setRolls((prev) => prev.filter((r) => r.id !== newRoll.id)), 15000);
                      } catch (err) {
                        console.error("Failed to roll initiative", err);
                      }
                    }}
                    className="cs-stat-pill__value cs-stat-pill__value--clickable"
                  >
                    {currentInitiativeBonus >= 0
                      ? `+${currentInitiativeBonus}`
                      : currentInitiativeBonus}
                  </div>
                </div>
                <div className="cs-stat-pill">
                  <div className="cs-stat-pill__label">SPEED</div>
                  <input
                    type="number"
                    value={localSpeed}
                    onChange={(e) => canEdit && setLocalSpeed(Number(e.target.value))}
                    onBlur={() => canEdit && handleHeaderStatUpdate("Speed", localSpeed)}
                    disabled={!canEdit}
                    className="cs-stat-pill__input"
                  />
                </div>
                <div className="cs-stat-pill">
                  <div className="cs-stat-pill__label">AC</div>
                  <input
                    type="number"
                    value={localAc}
                    onChange={(e) => canEdit && setLocalAc(Number(e.target.value))}
                    onBlur={() => canEdit && handleHeaderStatUpdate("ArmorClass", localAc)}
                    disabled={!canEdit}
                    className="cs-stat-pill__input"
                  />
                </div>
                <div className="cs-stat-pill cs-stat-pill--teal">
                  <div className="cs-stat-pill__label cs-stat-pill__label--teal">PROF.</div>
                  <div className="cs-stat-pill__value" style={{ color: "#03dac6" }}>
                    +{char.proficiencyBonus || 0}
                  </div>
                </div>
              </div>

              <div className="cs-hp-block">
                <div className="cs-hp-row">
                  <div style={{ display: "flex", flexDirection: "column", alignItems: "flex-end", gap: "8px" }}>
                  {canEdit && (
                    <button
                      onClick={(e) => { e.stopPropagation(); setActiveModal("rest"); }}
                      className="cs-btn-sm"
                    >
                      ⛺ REST
                    </button>
                  )}
                <div
                  className="cs-hp-display"
                  style={{ cursor: canEdit ? "pointer" : "default", opacity: canEdit ? 1 : 0.5 }}
                  onClick={() => canEdit && setActiveModal("hp")}
                >
                  <div className="cs-hp-display__label">HP (EDIT)</div>
                  <div className="cs-hp-display__values">
                    <span className="cs-hp-display__current" style={{ color: hpColor }}>
                      {char.currentHp || 0}
                    </span>
                    <span className="cs-hp-display__max">/ {char.maxHp || 0}</span>
                  </div>
                  {(char.temporaryHp || 0) > 0 && (
                    <div className="cs-hp-display__temp">+{char.temporaryHp} TEMP</div>
                  )}
                </div>
                </div>
                <button
                  onClick={() => setActiveModal("settings")}
                  className="cs-btn-icon"
                  aria-label="Settings"
                >
                  ⚙
                </button>
              </div>
            </div>
              </div>
            </div>

            <div className="cs-xp" onClick={() => setActiveModal("xp")}>
              <div className="cs-xp__labels">
                <span>LVL {char.level || 1}</span>
                <span className="cs-xp__value">
                  {char.currentXp || 0} / {char.nextLevelXp || 1} XP
                </span>
                <span>LVL {(char.level || 1) + 1}</span>
              </div>
              <div className="cs-xp__track">
                <div
                  className="cs-xp__fill"
                  style={{
                    width: `${Math.min(((char.currentXp || 0) / (char.nextLevelXp || 1)) * 100, 100)}%`,
                  }}
                />
              </div>
            </div>
          </div>

          <section className="cs-stats-panel" aria-label="Ability scores and skills">
            <div className="cs-stats-group">
              <h2 className="cs-stats-group__title">
                <span className="cs-stats-group__icon" aria-hidden>⚔</span>
                Physical
              </h2>
              <div className="cs-stats-grid">
                {physicalStats.map((stat) => (
                  <StatBlockCard key={stat.label} stat={stat} {...statBlockProps} />
                ))}
              </div>
            </div>

            <div className="cs-stats-group">
              <h2 className="cs-stats-group__title">
                <span className="cs-stats-group__icon" aria-hidden>✦</span>
                Mental
              </h2>
              <div className="cs-stats-grid">
                {mentalStats.map((stat) => (
                  <StatBlockCard key={stat.label} stat={stat} {...statBlockProps} />
                ))}
              </div>
            </div>

            <div className="cs-card cs-passive-banner">
              <span className="cs-passive-banner__label">Passive Perception</span>
              <strong className="cs-passive-banner__value">{passivePerception}</strong>
            </div>
          </section>
        </div>

        {/* RIGHT COLUMN */}
        <div className="cs-col cs-col--sidebar">
          <div className="cs-card" onClick={() => setActiveModal("wallet")} style={{ cursor: "pointer" }}>
            <div className="cs-wallet__title">WALLET (EDIT)</div>
            <div className="cs-wallet__grid">
              {["platinum", "gold", "silver", "copper"].map((c) => (
                <div key={c}>
                  <div className="cs-wallet__item-label">{c[0].toUpperCase()}P</div>
                  <div className="cs-wallet__item-value">{char[c] || 0}</div>
                </div>
              ))}
            </div>
          </div>

          <div className="cs-card cs-tabs-card">
            <div className="cs-tabs">
              {["attacks", "abilities", "equipment", "spells", "notes"].map(
                (t) => (
                  <button
                    key={t}
                    onClick={() => setActiveTab(t as any)}
                    className={`cs-tab${activeTab === t ? " cs-tab--active" : ""}`}
                  >
                    {t.toUpperCase()}
                  </button>
                ),
              )}
            </div>
            <div className="cs-tab-content">
              {activeTab === "attacks" ? (
                <div style={{ display: "flex", flexDirection: "column", gap: "20px" }}>
                  <AttacksPanel
                    attacks={char.attacks || []}
                    statModifiers={statModifiers}
                    proficiencyBonus={char.proficiencyBonus || 0}
                    spellcastingAbility={parseStatToNumber(char.spellcastingAbility)}
                    canEdit={canEdit}
                    onAddAttack={() => {
                      resetAttackForm();
                      setActiveModal("attack");
                    }}
                    onEditAttack={openAttackEditModal}
                    onDeleteAttack={handleDeleteAttack}
                    onRollHit={(bonus, name) => rollDice(20, bonus, `${name} (To-Hit)`)}
                    onRollDamage={handleRollAttackDamage}
                  />
                  <PassivesPanel
                    passives={char.passives || ""}
                    canEdit={canEdit}
                    onUpdate={(val) => updateCharacter("/progression", "put", { Passives: val })}
                  />
                  <TrackersPanel
                    trackers={char.trackers || []}
                    canEdit={canEdit}
                    onUpdateTrackerDescription={handleUpdateTrackerDescription}
                    onAddTracker={() => {
                      setTrackerForm({ name: "", maxValue: 1, currentValue: 1, resetCondition: 1 });
                      setEditingTrackerId(null);
                      setActiveModal("tracker");
                    }}
                    onEditTracker={(t: any) => {
                      setTrackerForm({
                        name: t.name,
                        maxValue: t.maxValue,
                        currentValue: t.currentValue,
                        resetCondition: t.resetCondition === "ShortRest" ? 1 : t.resetCondition === "LongRest" ? 2 : t.resetCondition === "Dawn" ? 3 : 0
                      });
                      setEditingTrackerId(t.id);
                      setActiveModal("tracker");
                    }}
                    onDeleteTracker={handleDeleteTracker}
                    onAdjustTracker={handleAdjustTracker}
                  />
                </div>
              ) : activeTab === "spells" ? (
                <SpellsPanel 
                  spellSlots={char.spellSlots || []} 
                  spellActions={mappedSpellActions}
                  canEdit={canEdit}
                  characterId={char.id}
                  spellcastingAbility={parseStatToNumber(char.spellcastingAbility)}
                  onUpdateSpellcastingAbility={(val: number | null) => updateCharacter("/progression", "put", { SpellcastingAbility: val })}
                  onAddSpell={openSpellCompendium}
                  onSaveSlot={(level: number, slotId: string, currentMax: number) => {
                     setSpellSlotForm({ id: slotId, level, maxValue: currentMax });
                     setActiveModal("spellSlot");
                  }}
                  onAdjustSlot={handleAdjustSlot}
                  onRollHit={(bonus: number, name: string) => rollDice(20, bonus, `${name} (To-Hit)`)}
                  onRollDamage={handleRollAttackDamage}
                />
              ) : (
                `${activeTab} panel pending...`
              )}
            </div>
          </div>
        </div>
      </div>

      {/* --- MODALS --- */}
      {activeModal !== "none" && (
        <div onClick={handleBackdropClick} className="cs-modal-backdrop">
          <div onClick={(e) => e.stopPropagation()} className="cs-modal">
            {activeModal === "initiative" && (
              <div style={{ textAlign: "center" }}>
                <h3 style={{ margin: "0 0 20px 0" }}>Initiative Bonus</h3>
                <input
                  type="number"
                  value={modalInputValue}
                  onChange={(e) => setModalInputValue(e.target.value)}
                  style={{
                    width: "100%",
                    padding: "12px",
                    fontSize: "24px",
                    textAlign: "center",
                    backgroundColor: "#121212",
                    border: "1px solid #bb86fc",
                    color: "#fff",
                    borderRadius: "6px",
                    marginBottom: "20px",
                    boxSizing: "border-box",
                  }}
                />
                <button
                  onClick={() => {
                    updateCharacter("/progression", "put", {
                      AdditionalInitiativeBonus: Number(modalInputValue),
                    });
                    closeModals();
                  }}
                  style={{
                    width: "100%",
                    padding: "12px",
                    backgroundColor: "#bb86fc",
                    color: "#000",
                    fontWeight: "bold",
                    borderRadius: "6px",
                    cursor: "pointer",
                    border: "none",
                  }}
                >
                  SAVE BONUS
                </button>
              </div>
            )}

            {activeModal === "stat" && (
              <div style={{ textAlign: "center" }}>
                <h3 style={{ margin: "0 0 20px 0" }}>
                  Edit {modalData?.label}
                </h3>
                <input
                  type="number"
                  value={modalInputValue}
                  onChange={(e) => setModalInputValue(e.target.value)}
                  style={{
                    width: "100%",
                    padding: "12px",
                    fontSize: "24px",
                    textAlign: "center",
                    backgroundColor: "#121212",
                    border: "1px solid #333",
                    color: "#fff",
                    borderRadius: "6px",
                    marginBottom: "20px",
                    boxSizing: "border-box",
                  }}
                />
                <div
                  style={{
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    gap: "10px",
                    marginBottom: "25px",
                  }}
                >
                  <ProficiencyCircle
                    isProf={char[modalData.saveDb.charAt(0).toLowerCase() + modalData.saveDb.slice(1)]}
                    onClick={() => {
                      const camelDb = modalData.saveDb.charAt(0).toLowerCase() + modalData.saveDb.slice(1);
                      updateCharacter("/progression", "put", {
                        [modalData.saveDb]: !char[camelDb],
                      })
                    }}
                    size={16}
                  />
                  <span style={{ fontSize: "14px" }}>
                    Saving Throw Proficiency
                  </span>
                </div>
                <button
                  onClick={handleStatSave}
                  style={{
                    width: "100%",
                    padding: "12px",
                    backgroundColor: "#bb86fc",
                    color: "#000",
                    fontWeight: "bold",
                    borderRadius: "6px",
                    cursor: "pointer",
                    border: "none",
                  }}
                >
                  SAVE CHANGES
                </button>
              </div>
            )}

            {activeModal === "hp" && (
              <div>
                <h3 style={{ margin: "0 0 20px 0", textAlign: "center" }}>
                  HP Management
                </h3>
                <div style={{ textAlign: "center", marginBottom: "20px" }}>
                  <span
                    style={{
                      fontSize: "36px",
                      fontWeight: "bold",
                      color: hpColor,
                      transition: "color 0.3s",
                    }}
                  >
                    {char.currentHp}
                  </span>
                  <span style={{ fontSize: "24px", color: "#757575" }}>
                    {" "}
                    / {char.maxHp}
                  </span>
                  {(char.temporaryHp || 0) > 0 && (
                    <span
                      style={{
                        fontSize: "20px",
                        color: "#03dac6",
                        marginLeft: "10px",
                        fontWeight: "bold",
                      }}
                    >
                      +{char.temporaryHp} TEMP
                    </span>
                  )}
                </div>
                <input
                  type="number"
                  placeholder="Enter Amount"
                  value={modalInputValue}
                  onChange={(e) => setModalInputValue(e.target.value)}
                  style={{
                    width: "100%",
                    padding: "12px",
                    textAlign: "center",
                    backgroundColor: "#121212",
                    border: "1px solid #333",
                    color: "#fff",
                    borderRadius: "6px",
                    marginBottom: "20px",
                    boxSizing: "border-box",
                  }}
                />
                <div
                  style={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr 1fr",
                    gap: "10px",
                    marginBottom: "20px",
                  }}
                >
                  <button
                    onClick={() => handleHpAction("damage")}
                    style={{
                      padding: "10px",
                      border: "2px solid #f44336",
                      color: "#f44336",
                      background: "none",
                      borderRadius: "6px",
                      fontWeight: "bold",
                      cursor: "pointer",
                    }}
                  >
                    DAMAGE
                  </button>
                  <button
                    onClick={() => handleHpAction("heal")}
                    style={{
                      padding: "10px",
                      border: "2px solid #4caf50",
                      color: "#4caf50",
                      background: "none",
                      borderRadius: "6px",
                      fontWeight: "bold",
                      cursor: "pointer",
                    }}
                  >
                    HEAL
                  </button>
                  <button
                    onClick={() => handleHpAction("temp")}
                    style={{
                      padding: "10px",
                      border: "2px solid #03dac6",
                      color: "#03dac6",
                      background: "none",
                      borderRadius: "6px",
                      fontWeight: "bold",
                      cursor: "pointer",
                    }}
                  >
                    TEMP
                  </button>
                </div>
                <div
                  style={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr",
                    gap: "10px",
                    borderTop: "1px solid #333",
                    paddingTop: "15px",
                  }}
                >
                  <div>
                    <label style={{ fontSize: "9px", color: "#757575" }}>
                      MAX HP
                    </label>
                    <input
                      type="number"
                      defaultValue={char.maxHp}
                      onBlur={(e) =>
                        updateCharacter("/vitals", "patch", {
                          MaxHp: parseInt(e.target.value),
                        })
                      }
                      style={{
                        width: "100%",
                        padding: "8px",
                        background: "#121212",
                        border: "1px solid #333",
                        color: "#fff",
                        textAlign: "center",
                        boxSizing: "border-box",
                        borderRadius: "4px",
                      }}
                    />
                  </div>
                  <div>
                    <label style={{ fontSize: "9px", color: "#757575" }}>
                      HIT DICE
                    </label>
                    <select
                      value={hitDiceNumber}
                      onChange={(e) =>
                        updateCharacter("/progression", "put", {
                          HitDiceType: parseInt(e.target.value),
                        })
                      }
                      style={{
                        width: "100%",
                        padding: "8px",
                        background: "#121212",
                        border: "1px solid #333",
                        color: "#fff",
                        textAlign: "center",
                        boxSizing: "border-box",
                        borderRadius: "4px",
                      }}
                    >
                      <option value="4">d4</option>
                      <option value="6">d6</option>
                      <option value="8">d8</option>
                      <option value="10">d10</option>
                      <option value="12">d12</option>
                    </select>
                  </div>
                </div>
              </div>
            )}

            {activeModal === "xp" && (
              <div style={{ textAlign: "center" }}>
                <h3 style={{ margin: "0 0 20px 0" }}>Manage Experience</h3>
                <input
                  type="number"
                  placeholder="Enter XP amount"
                  value={modalInputValue}
                  onChange={(e) => setModalInputValue(e.target.value)}
                  style={{
                    width: "100%",
                    padding: "12px",
                    fontSize: "18px",
                    textAlign: "center",
                    backgroundColor: "#121212",
                    border: "1px solid #bb86fc",
                    color: "#fff",
                    borderRadius: "6px",
                    marginBottom: "20px",
                    boxSizing: "border-box",
                  }}
                />
                <div
                  style={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr",
                    gap: "10px",
                  }}
                >
                  <button
                    onClick={() => handleXpAction(false)}
                    style={{
                      padding: "12px",
                      backgroundColor: "#121212",
                      border: "1px solid #f44336",
                      color: "#f44336",
                      fontWeight: "bold",
                      borderRadius: "6px",
                      cursor: "pointer",
                    }}
                  >
                    SUBTRACT
                  </button>
                  <button
                    onClick={() => handleXpAction(true)}
                    style={{
                      padding: "12px",
                      backgroundColor: "#121212",
                      border: "1px solid #4caf50",
                      color: "#4caf50",
                      fontWeight: "bold",
                      borderRadius: "6px",
                      cursor: "pointer",
                    }}
                  >
                    ADD XP
                  </button>
                </div>
              </div>
            )}

            {activeModal === "wallet" && (
              <div>
                <h3 style={{ margin: "0 0 20px 0", textAlign: "center" }}>
                  Manage Wealth
                </h3>
                <div
                  style={{ display: "flex", gap: "5px", marginBottom: "20px" }}
                >
                  {["platinum", "gold", "silver", "copper"].map((c) => (
                    <button
                      key={c}
                      onClick={() => setWalletCurrency(c as any)}
                      style={{
                        flex: 1,
                        padding: "8px",
                        fontSize: "9px",
                        backgroundColor:
                          walletCurrency === c ? "#bb86fc" : "#121212",
                        color: walletCurrency === c ? "#000" : "#fff",
                        border: "1px solid #333",
                        cursor: "pointer",
                        borderRadius: "4px",
                        fontWeight: "bold",
                        textTransform: "uppercase",
                      }}
                    >
                      {c}
                    </button>
                  ))}
                </div>
                <input
                  type="number"
                  value={modalInputValue}
                  onChange={(e) => setModalInputValue(e.target.value)}
                  style={{
                    width: "100%",
                    padding: "12px",
                    textAlign: "center",
                    backgroundColor: "#121212",
                    border: "1px solid #333",
                    color: "#fff",
                    marginBottom: "20px",
                    boxSizing: "border-box",
                    borderRadius: "6px",
                    fontSize: "18px",
                  }}
                />
                <div
                  style={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr",
                    gap: "10px",
                  }}
                >
                  <button
                    onClick={() => handleWalletAction(false)}
                    style={{
                      padding: "12px",
                      border: "1px solid #f44336",
                      color: "#f44336",
                      background: "none",
                      cursor: "pointer",
                      fontWeight: "bold",
                      borderRadius: "4px",
                    }}
                  >
                    SPEND
                  </button>
                  <button
                    onClick={() => handleWalletAction(true)}
                    style={{
                      padding: "12px",
                      border: "1px solid #4caf50",
                      color: "#4caf50",
                      background: "none",
                      cursor: "pointer",
                      fontWeight: "bold",
                      borderRadius: "4px",
                    }}
                  >
                    ADD
                  </button>
                </div>
              </div>
            )}

            {activeModal === "rest" && (
              <div>
                <h3 style={{ margin: "0 0 20px 0", textAlign: "center" }}>Rest</h3>
                
                <div style={{ marginBottom: "20px", padding: "15px", backgroundColor: "#121212", border: "1px solid #333", borderRadius: "6px" }}>
                  <h4 style={{ margin: "0 0 10px 0", color: "#bb86fc" }}>Short Rest</h4>
                  <p style={{ fontSize: "11px", color: "#757575", marginBottom: "15px" }}>
                    Spend Hit Dice to heal. Restores short rest abilities.
                  </p>
                  <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "15px" }}>
                    <span style={{ fontSize: "14px" }}>Hit Dice Remaining:</span>
                    <span style={{ fontSize: "16px", fontWeight: "bold", color: char.hitDiceCurrent > 0 ? "#4caf50" : "#f44336" }}>
                      {char.hitDiceCurrent} / {char.hitDiceMax} (d{String(char.hitDiceType).replace(/\D/g, "")})
                    </span>
                  </div>
                  <div style={{ display: "flex", gap: "10px" }}>
                    <button
                      onClick={handleSpendHitDice}
                      disabled={char.hitDiceCurrent <= 0}
                      style={{
                        flex: 1,
                        padding: "8px",
                        backgroundColor: "transparent",
                        color: "#03dac6",
                        border: "1px solid #03dac6",
                        borderRadius: "4px",
                        cursor: char.hitDiceCurrent > 0 ? "pointer" : "not-allowed",
                        opacity: char.hitDiceCurrent > 0 ? 1 : 0.5,
                        fontWeight: "bold",
                        fontSize: "12px"
                      }}
                    >
                      SPEND 1 HIT DICE
                    </button>
                    <button
                      onClick={() => handlePerformRest(1)}
                      style={{
                        flex: 1,
                        padding: "8px",
                        backgroundColor: "#03dac6",
                        color: "#000",
                        border: "none",
                        borderRadius: "4px",
                        cursor: "pointer",
                        fontWeight: "bold",
                        fontSize: "12px"
                      }}
                    >
                      FINISH SHORT REST
                    </button>
                  </div>
                </div>

                <div style={{ padding: "15px", backgroundColor: "#121212", border: "1px solid #333", borderRadius: "6px" }}>
                  <h4 style={{ margin: "0 0 10px 0", color: "#bb86fc" }}>Long Rest</h4>
                  <p style={{ fontSize: "11px", color: "#757575", marginBottom: "15px" }}>
                    Restores all HP, short/long rest abilities, and half of max Hit Dice.
                  </p>
                  <button
                    onClick={() => handlePerformRest(2)}
                    style={{
                      width: "100%",
                      padding: "10px",
                      backgroundColor: "#bb86fc",
                      color: "#000",
                      border: "none",
                      borderRadius: "4px",
                      cursor: "pointer",
                      fontWeight: "bold",
                      fontSize: "12px"
                    }}
                  >
                    TAKE LONG REST
                  </button>
                </div>

                <button
                  onClick={closeModals}
                  style={{
                    width: "100%",
                    padding: "10px",
                    marginTop: "20px",
                    backgroundColor: "#333",
                    color: "#fff",
                    border: "none",
                    borderRadius: "6px",
                    cursor: "pointer",
                    fontWeight: "bold"
                  }}
                >
                  CANCEL
                </button>
              </div>
            )}

            {activeModal === "spellSlot" && (
              <div>
                <h3 style={{ margin: "0 0 20px 0", textAlign: "center" }}>Edit Level {spellSlotForm.level} Slots</h3>
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "11px", color: "#757575" }}>Total Slots (Uses)</label>
                  <input type="number" min="0" value={spellSlotForm.maxValue} onChange={(e) => setSpellSlotForm({ ...spellSlotForm, maxValue: Number(e.target.value) })} style={{ width: "100%", padding: "8px", background: "#121212", border: "1px solid #333", color: "#fff", marginTop: "5px", boxSizing: "border-box", borderRadius: "4px" }} />
                </div>
                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "10px" }}>
                  <button onClick={closeModals} style={{ padding: "10px", backgroundColor: "#333", color: "#fff", border: "none", borderRadius: "6px", cursor: "pointer", fontWeight: "bold" }}>CANCEL</button>
                  <button onClick={handleSaveSlot} style={{ padding: "10px", backgroundColor: "#bb86fc", color: "#000", border: "none", borderRadius: "6px", cursor: "pointer", fontWeight: "bold" }}>SAVE</button>
                </div>
              </div>
            )}

            {activeModal === "spellCompendium" && (
              <div>
                <h3 style={{ margin: "0 0 20px 0", textAlign: "center", color: "#bb86fc" }}>
                  Spell Compendium
                </h3>
                <div style={{ maxHeight: "400px", overflowY: "auto", display: "flex", flexDirection: "column", gap: "10px" }}>
                  {availableSpells.length === 0 ? (
                     <div style={{ textAlign: "center", color: "#757575", padding: "20px" }}>Loading spells...</div>
                  ) : (
                     availableSpells.map(spell => (
                        <div key={spell.id} style={{ display: "flex", justifyContent: "space-between", alignItems: "center", backgroundColor: "#121212", padding: "10px", borderRadius: "6px", border: "1px solid #333" }}>
                           <div>
                              <div style={{ fontWeight: "bold", fontSize: "14px", color: "#fff" }}>{spell.name}</div>
                              <div style={{ fontSize: "10px", color: "#757575" }}>
                                 {spell.level === 0 ? "Cantrip" : `Level ${spell.level}`} • {spell.damageDice ? `${spell.damageDice} ${spell.damageType}` : "Utility"}
                              </div>
                           </div>
                           {canEdit && (
                           <button 
                              onClick={() => handleAddSpellToCharacter(spell)}
                              style={{ padding: "6px 12px", backgroundColor: "#03dac6", color: "#000", border: "none", borderRadius: "4px", fontWeight: "bold", cursor: "pointer", fontSize: "10px" }}
                           >
                              + ADD
                           </button>
                           )}
                        </div>
                     ))
                  )}
                </div>
                <button
                  onClick={closeModals}
                  style={{
                    width: "100%",
                    padding: "10px",
                    marginTop: "20px",
                    backgroundColor: "#333",
                    color: "#fff",
                    border: "none",
                    borderRadius: "6px",
                    cursor: "pointer",
                    fontWeight: "bold"
                  }}
                >
                  CLOSE
                </button>
              </div>
            )}

            {activeModal === "attack" && (
              <div>
                <h3 style={{ margin: "0 0 20px 0", textAlign: "center" }}>
                  Create Attack
                </h3>
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "11px", color: "#757575" }}>
                    Attack Name
                  </label>
                  <input
                    type="text"
                    value={attackForm.name}
                    onChange={(e) =>
                      setAttackForm({ ...attackForm, name: e.target.value })
                    }
                    style={{
                      width: "100%",
                      padding: "8px",
                      background: "#121212",
                      border: "1px solid #333",
                      color: "#fff",
                      marginTop: "5px",
                      boxSizing: "border-box",
                      borderRadius: "4px",
                    }}
                  />
                </div>
                <div
                  style={{
                    marginBottom: "15px",
                    display: "flex",
                    gap: "10px",
                    alignItems: "center",
                  }}
                >
                  <input
                    type="checkbox"
                    checked={attackForm.isAttackRoll}
                    onChange={(e) =>
                      setAttackForm({
                        ...attackForm,
                        isAttackRoll: e.target.checked,
                      })
                    }
                  />
                  <span style={{ fontSize: "11px" }}>Requires Attack Roll (To-Hit)</span>
                </div>
                <div
                  style={{
                    marginBottom: "15px",
                    display: "flex",
                    gap: "10px",
                    alignItems: "center",
                  }}
                >
                  <input
                    type="checkbox"
                    checked={attackForm.isProficient}
                    onChange={(e) =>
                      setAttackForm({
                        ...attackForm,
                        isProficient: e.target.checked,
                      })
                    }
                  />
                  <span style={{ fontSize: "11px" }}>Add Proficiency to Bonus/DC</span>
                </div>
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "11px", color: "#757575" }}>
                    Stat Dependency (Attack/DC)
                  </label>
                  <select
                    value={attackForm.attackStat ?? ""}
                    onChange={(e) =>
                      setAttackForm({
                        ...attackForm,
                        attackStat: e.target.value === "" ? null : Number(e.target.value),
                      })
                    }
                    style={{
                      width: "100%",
                      padding: "8px",
                      background: "#121212",
                      border: "1px solid #333",
                      color: "#fff",
                      marginTop: "5px",
                      boxSizing: "border-box",
                      borderRadius: "4px",
                    }}
                  >
                    <option value="">None</option>
                    <option value="1">Strength</option>
                    <option value="2">Dexterity</option>
                    <option value="3">Constitution</option>
                    <option value="4">Intelligence</option>
                    <option value="5">Wisdom</option>
                    <option value="6">Charisma</option>
                  </select>
                </div>
                <div style={{ marginBottom: "15px", display: "flex", flexDirection: "column", gap: "5px" }}>
                  <label style={{ fontSize: "11px", color: "#757575", fontWeight: "bold" }}>
                    ACTION ECONOMY
                  </label>
                  <div style={{ display: "flex", backgroundColor: "#121212", border: "1px solid #333", borderRadius: "6px", width: "max-content", overflow: "hidden" }}>
                    <button
                      type="button"
                      onClick={() => setAttackForm({ ...attackForm, actionCost: 'Action' })}
                      style={{
                        padding: "6px 12px",
                        fontSize: "11px",
                        fontWeight: "bold",
                        cursor: "pointer",
                        border: "none",
                        backgroundColor: attackForm.actionCost === 'Action' ? '#bb86fc' : 'transparent',
                        color: attackForm.actionCost === 'Action' ? '#000' : '#757575',
                        transition: "all 0.2s"
                      }}
                    >
                      ACTION
                    </button>
                    <button
                      type="button"
                      onClick={() => setAttackForm({ ...attackForm, actionCost: 'BonusAction' })}
                      style={{
                        padding: "6px 12px",
                        fontSize: "11px",
                        fontWeight: "bold",
                        cursor: "pointer",
                        border: "none",
                        backgroundColor: attackForm.actionCost === 'BonusAction' ? '#bb86fc' : 'transparent',
                        color: attackForm.actionCost === 'BonusAction' ? '#000' : '#757575',
                        transition: "all 0.2s"
                      }}
                    >
                      BONUS ACTION
                    </button>
                  </div>
                </div>
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "11px", color: "#757575" }}>
                    Extra Flat Bonus
                  </label>
                  <input
                    type="number"
                    value={attackForm.flatAttackBonus}
                    onChange={(e) =>
                      setAttackForm({
                        ...attackForm,
                        flatAttackBonus: Number(e.target.value),
                      })
                    }
                    style={{
                      width: "100%",
                      padding: "8px",
                      background: "#121212",
                      border: "1px solid #333",
                      color: "#fff",
                      marginTop: "5px",
                      boxSizing: "border-box",
                      borderRadius: "4px",
                    }}
                  />
                </div>
                <div style={{ marginBottom: "15px", borderTop: "1px solid #333", paddingTop: "15px" }}>
                  <div
                    style={{
                      display: "flex",
                      justifyContent: "space-between",
                      alignItems: "center",
                      marginBottom: "10px",
                    }}
                  >
                    <label style={{ fontSize: "11px", color: "#757575" }}>
                      DAMAGE FORMULA
                    </label>
                    <button
                      onClick={() =>
                        setAttackForm({
                          ...attackForm,
                          damages: [
                            ...(attackForm.damages || []),
                            {
                              diceType: 8,
                              diceCount: 1,
                              modifierStat: null,
                              flatDamageBonus: 0,
                              damageType: "slashing",
                            },
                          ],
                        })
                      }
                      style={{
                        padding: "4px 8px",
                        fontSize: "9px",
                        backgroundColor: "#bb86fc",
                        color: "#000",
                        border: "none",
                        borderRadius: "4px",
                        cursor: "pointer",
                        fontWeight: "bold",
                      }}
                    >
                      + ADD DAMAGE
                    </button>
                  </div>
                  {(attackForm.damages || []).map((damage: any, idx: number) => (
                    <div
                      key={idx}
                      style={{
                        display: "grid",
                        gridTemplateColumns: "1fr 1fr 1fr",
                        gap: "8px",
                        marginBottom: "10px",
                        padding: "10px",
                        backgroundColor: "#0a0a0a",
                        borderRadius: "4px",
                        border: "1px solid #222",
                      }}
                    >
                      <div>
                        <label style={{ fontSize: "9px", color: "#757575" }}>Count</label>
                        <input
                          type="number"
                          value={damage.diceCount}
                          onChange={(e) => {
                            const newDamages = [...(attackForm.damages || [])];
                            newDamages[idx].diceCount = Number(e.target.value);
                            setAttackForm({ ...attackForm, damages: newDamages });
                          }}
                          style={{
                            width: "100%",
                            padding: "4px",
                            background: "#121212",
                            border: "1px solid #333",
                            color: "#fff",
                            borderRadius: "3px",
                            fontSize: "11px",
                          }}
                        />
                      </div>
                      <div>
                        <label style={{ fontSize: "9px", color: "#757575" }}>Type</label>
                        <select
                          value={damage.diceType}
                          onChange={(e) => {
                            const newDamages = [...(attackForm.damages || [])];
                            newDamages[idx].diceType = Number(e.target.value);
                            setAttackForm({ ...attackForm, damages: newDamages });
                          }}
                          style={{
                            width: "100%",
                            padding: "4px",
                            background: "#121212",
                            border: "1px solid #333",
                            color: "#fff",
                            borderRadius: "3px",
                            fontSize: "11px",
                          }}
                        >
                          <option value="4">d4</option>
                          <option value="6">d6</option>
                          <option value="8">d8</option>
                          <option value="10">d10</option>
                          <option value="12">d12</option>
                          <option value="20">d20</option>
                        </select>
                      </div>
                      <div>
                        <label style={{ fontSize: "9px", color: "#757575" }}>Stat Mod</label>
                        <select
                          value={damage.modifierStat ?? ""}
                          onChange={(e) => {
                            const newDamages = [...(attackForm.damages || [])];
                            newDamages[idx].modifierStat = e.target.value === "" ? null : Number(e.target.value);
                            setAttackForm({ ...attackForm, damages: newDamages });
                          }}
                          style={{
                            width: "100%",
                            padding: "4px",
                            background: "#121212",
                            border: "1px solid #333",
                            color: "#fff",
                            borderRadius: "3px",
                            fontSize: "11px",
                          }}
                        >
                          <option value="">None</option>
                          <option value="1">STR</option>
                          <option value="2">DEX</option>
                          <option value="3">CON</option>
                          <option value="4">INT</option>
                          <option value="5">WIS</option>
                          <option value="6">CHA</option>
                        </select>
                      </div>
                      <div>
                        <label style={{ fontSize: "9px", color: "#757575" }}>Flat Bonus</label>
                        <input
                          type="number"
                          value={damage.flatDamageBonus}
                          onChange={(e) => {
                            const newDamages = [...(attackForm.damages || [])];
                            newDamages[idx].flatDamageBonus = Number(e.target.value);
                            setAttackForm({ ...attackForm, damages: newDamages });
                          }}
                          style={{
                            width: "100%",
                            padding: "4px",
                            background: "#121212",
                            border: "1px solid #333",
                            color: "#fff",
                            borderRadius: "3px",
                            fontSize: "11px",
                          }}
                        />
                      </div>
                      <div>
                        <label style={{ fontSize: "9px", color: "#757575" }}>Damage Type</label>
                        <input
                          type="text"
                          value={damage.damageType}
                          onChange={(e) => {
                            const newDamages = [...(attackForm.damages || [])];
                            newDamages[idx].damageType = e.target.value;
                            setAttackForm({ ...attackForm, damages: newDamages });
                          }}
                          style={{
                            width: "100%",
                            padding: "4px",
                            background: "#121212",
                            border: "1px solid #333",
                            color: "#fff",
                            borderRadius: "3px",
                            fontSize: "11px",
                          }}
                        />
                      </div>
                      <div style={{ display: "flex", alignItems: "flex-end" }}>
                        <button
                          onClick={() => {
                            const newDamages = attackForm.damages.filter(
                              (_: any, i: number) => i !== idx
                            );
                            setAttackForm({ ...attackForm, damages: newDamages });
                          }}
                          style={{
                            width: "100%",
                            padding: "5px",
                            fontSize: "10px",
                            backgroundColor: "#f44336",
                            color: "#fff",
                            border: "none",
                            borderRadius: "3px",
                            cursor: "pointer",
                            fontWeight: "bold",
                          }}
                        >
                          REMOVE
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
                <div
                  style={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr",
                    gap: "10px",
                  }}
                >
                  <button
                    onClick={() => {
                      resetAttackForm();
                      closeModals();
                    }}
                    style={{
                      padding: "10px",
                      backgroundColor: "#333",
                      color: "#fff",
                      border: "none",
                      borderRadius: "6px",
                      cursor: "pointer",
                      fontWeight: "bold",
                    }}
                  >
                    CANCEL
                  </button>
                  <button
                    onClick={handleAddAttack}
                    style={{
                      padding: "10px",
                      backgroundColor: "#bb86fc",
                      color: "#000",
                      border: "none",
                      borderRadius: "6px",
                      cursor: "pointer",
                      fontWeight: "bold",
                    }}
                  >
                    CREATE ATTACK
                  </button>
                </div>
              </div>
            )}

            {activeModal === "attackEdit" && (
              <div>
                <h3 style={{ margin: "0 0 20px 0", textAlign: "center" }}>
                  Edit Attack
                </h3>
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "11px", color: "#757575" }}>
                    Attack Name
                  </label>
                  <input
                    type="text"
                    value={attackForm.name}
                    onChange={(e) =>
                      setAttackForm({ ...attackForm, name: e.target.value })
                    }
                    style={{
                      width: "100%",
                      padding: "8px",
                      background: "#121212",
                      border: "1px solid #333",
                      color: "#fff",
                      marginTop: "5px",
                      boxSizing: "border-box",
                      borderRadius: "4px",
                    }}
                  />
                </div>
                <div
                  style={{
                    marginBottom: "15px",
                    display: "flex",
                    gap: "10px",
                    alignItems: "center",
                  }}
                >
                  <input
                    type="checkbox"
                    checked={attackForm.isAttackRoll}
                    onChange={(e) =>
                      setAttackForm({
                        ...attackForm,
                        isAttackRoll: e.target.checked,
                      })
                    }
                  />
                  <span style={{ fontSize: "11px" }}>Requires Attack Roll (To-Hit)</span>
                </div>
                <div
                  style={{
                    marginBottom: "15px",
                    display: "flex",
                    gap: "10px",
                    alignItems: "center",
                  }}
                >
                  <input
                    type="checkbox"
                    checked={attackForm.isProficient}
                    onChange={(e) =>
                      setAttackForm({
                        ...attackForm,
                        isProficient: e.target.checked,
                      })
                    }
                  />
                  <span style={{ fontSize: "11px" }}>Add Proficiency to Bonus/DC</span>
                </div>
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "11px", color: "#757575" }}>
                    Stat Dependency (Attack/DC)
                  </label>
                  <select
                    value={attackForm.attackStat ?? ""}
                    onChange={(e) =>
                      setAttackForm({
                        ...attackForm,
                        attackStat: e.target.value === "" ? null : Number(e.target.value),
                      })
                    }
                    style={{
                      width: "100%",
                      padding: "8px",
                      background: "#121212",
                      border: "1px solid #333",
                      color: "#fff",
                      marginTop: "5px",
                      boxSizing: "border-box",
                      borderRadius: "4px",
                    }}
                  >
                    <option value="">None</option>
                    <option value="1">Strength</option>
                    <option value="2">Dexterity</option>
                    <option value="3">Constitution</option>
                    <option value="4">Intelligence</option>
                    <option value="5">Wisdom</option>
                    <option value="6">Charisma</option>
                  </select>
                </div>
                <div style={{ marginBottom: "15px", display: "flex", flexDirection: "column", gap: "5px" }}>
                  <label style={{ fontSize: "11px", color: "#757575", fontWeight: "bold" }}>
                    ACTION ECONOMY
                  </label>
                  <div style={{ display: "flex", backgroundColor: "#121212", border: "1px solid #333", borderRadius: "6px", width: "max-content", overflow: "hidden" }}>
                    <button
                      type="button"
                      onClick={() => setAttackForm({ ...attackForm, actionCost: 'Action' })}
                      style={{
                        padding: "6px 12px",
                        fontSize: "11px",
                        fontWeight: "bold",
                        cursor: "pointer",
                        border: "none",
                        backgroundColor: attackForm.actionCost === 'Action' ? '#bb86fc' : 'transparent',
                        color: attackForm.actionCost === 'Action' ? '#000' : '#757575',
                        transition: "all 0.2s"
                      }}
                    >
                      ACTION
                    </button>
                    <button
                      type="button"
                      onClick={() => setAttackForm({ ...attackForm, actionCost: 'BonusAction' })}
                      style={{
                        padding: "6px 12px",
                        fontSize: "11px",
                        fontWeight: "bold",
                        cursor: "pointer",
                        border: "none",
                        backgroundColor: attackForm.actionCost === 'BonusAction' ? '#bb86fc' : 'transparent',
                        color: attackForm.actionCost === 'BonusAction' ? '#000' : '#757575',
                        transition: "all 0.2s"
                      }}
                    >
                      BONUS ACTION
                    </button>
                  </div>
                </div>
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "11px", color: "#757575" }}>
                    Extra Flat Bonus
                  </label>
                  <input
                    type="number"
                    value={attackForm.flatAttackBonus}
                    onChange={(e) =>
                      setAttackForm({
                        ...attackForm,
                        flatAttackBonus: Number(e.target.value),
                      })
                    }
                    style={{
                      width: "100%",
                      padding: "8px",
                      background: "#121212",
                      border: "1px solid #333",
                      color: "#fff",
                      marginTop: "5px",
                      boxSizing: "border-box",
                      borderRadius: "4px",
                    }}
                  />
                </div>
                <div style={{ marginBottom: "15px", borderTop: "1px solid #333", paddingTop: "15px" }}>
                  <div
                    style={{
                      display: "flex",
                      justifyContent: "space-between",
                      alignItems: "center",
                      marginBottom: "10px",
                    }}
                  >
                    <label style={{ fontSize: "11px", color: "#757575" }}>
                      DAMAGE FORMULA
                    </label>
                    <button
                      onClick={() =>
                        setAttackForm({
                          ...attackForm,
                          damages: [
                            ...(attackForm.damages || []),
                            {
                              diceType: 8,
                              diceCount: 1,
                              modifierStat: null,
                              flatDamageBonus: 0,
                              damageType: "Slashing",
                            },
                          ],
                        })
                      }
                      style={{
                        padding: "4px 8px",
                        fontSize: "9px",
                        backgroundColor: "#bb86fc",
                        color: "#000",
                        border: "none",
                        borderRadius: "4px",
                        cursor: "pointer",
                        fontWeight: "bold",
                      }}
                    >
                      + ADD DAMAGE
                    </button>
                  </div>
                  {(attackForm.damages || []).map((damage: any, idx: number) => (
                    <div
                      key={idx}
                      style={{
                        display: "grid",
                        gridTemplateColumns: "1fr 1fr 1fr",
                        gap: "8px",
                        marginBottom: "10px",
                        padding: "10px",
                        backgroundColor: "#0a0a0a",
                        borderRadius: "4px",
                        border: "1px solid #222",
                      }}
                    >
                      <div>
                        <label style={{ fontSize: "9px", color: "#757575" }}>Count</label>
                        <input
                          type="number"
                          value={damage.diceCount}
                          onChange={(e) => {
                            const newDamages = [...(attackForm.damages || [])];
                            newDamages[idx].diceCount = Number(e.target.value);
                            setAttackForm({ ...attackForm, damages: newDamages });
                          }}
                          style={{
                            width: "100%",
                            padding: "4px",
                            background: "#121212",
                            border: "1px solid #333",
                            color: "#fff",
                            borderRadius: "3px",
                            fontSize: "11px",
                          }}
                        />
                      </div>
                      <div>
                        <label style={{ fontSize: "9px", color: "#757575" }}>Type</label>
                        <select
                          value={damage.diceType}
                          onChange={(e) => {
                            const newDamages = [...(attackForm.damages || [])];
                            newDamages[idx].diceType = Number(e.target.value);
                            setAttackForm({ ...attackForm, damages: newDamages });
                          }}
                          style={{
                            width: "100%",
                            padding: "4px",
                            background: "#121212",
                            border: "1px solid #333",
                            color: "#fff",
                            borderRadius: "3px",
                            fontSize: "11px",
                          }}
                        >
                          <option value="4">d4</option>
                          <option value="6">d6</option>
                          <option value="8">d8</option>
                          <option value="10">d10</option>
                          <option value="12">d12</option>
                          <option value="20">d20</option>
                        </select>
                      </div>
                      <div>
                        <label style={{ fontSize: "9px", color: "#757575" }}>Stat Mod</label>
                        <select
                          value={damage.modifierStat ?? ""}
                          onChange={(e) => {
                            const newDamages = [...(attackForm.damages || [])];
                            newDamages[idx].modifierStat = e.target.value === "" ? null : Number(e.target.value);
                            setAttackForm({ ...attackForm, damages: newDamages });
                          }}
                          style={{
                            width: "100%",
                            padding: "4px",
                            background: "#121212",
                            border: "1px solid #333",
                            color: "#fff",
                            borderRadius: "3px",
                            fontSize: "11px",
                          }}
                        >
                          <option value="">None</option>
                          <option value="1">STR</option>
                          <option value="2">DEX</option>
                          <option value="3">CON</option>
                          <option value="4">INT</option>
                          <option value="5">WIS</option>
                          <option value="6">CHA</option>
                        </select>
                      </div>
                      <div>
                        <label style={{ fontSize: "9px", color: "#757575" }}>Flat Bonus</label>
                        <input
                          type="number"
                          value={damage.flatDamageBonus}
                          onChange={(e) => {
                            const newDamages = [...(attackForm.damages || [])];
                            newDamages[idx].flatDamageBonus = Number(e.target.value);
                            setAttackForm({ ...attackForm, damages: newDamages });
                          }}
                          style={{
                            width: "100%",
                            padding: "4px",
                            background: "#121212",
                            border: "1px solid #333",
                            color: "#fff",
                            borderRadius: "3px",
                            fontSize: "11px",
                          }}
                        />
                      </div>
                      <div>
                        <label style={{ fontSize: "9px", color: "#757575" }}>Damage Type</label>
                        <input
                          type="text"
                          value={damage.damageType}
                          onChange={(e) => {
                            const newDamages = [...(attackForm.damages || [])];
                            newDamages[idx].damageType = e.target.value;
                            setAttackForm({ ...attackForm, damages: newDamages });
                          }}
                          style={{
                            width: "100%",
                            padding: "4px",
                            background: "#121212",
                            border: "1px solid #333",
                            color: "#fff",
                            borderRadius: "3px",
                            fontSize: "11px",
                          }}
                        />
                      </div>
                      <div style={{ display: "flex", alignItems: "flex-end" }}>
                        <button
                          onClick={() => {
                            const newDamages = attackForm.damages.filter(
                              (_: any, i: number) => i !== idx
                            );
                            setAttackForm({ ...attackForm, damages: newDamages });
                          }}
                          style={{
                            width: "100%",
                            padding: "5px",
                            fontSize: "10px",
                            backgroundColor: "#f44336",
                            color: "#fff",
                            border: "none",
                            borderRadius: "3px",
                            cursor: "pointer",
                            fontWeight: "bold",
                          }}
                        >
                          REMOVE
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
                <div
                  style={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr",
                    gap: "10px",
                  }}
                >
                  <button
                    onClick={() => {
                      resetAttackForm();
                      closeModals();
                    }}
                    style={{
                      padding: "10px",
                      backgroundColor: "#333",
                      color: "#fff",
                      border: "none",
                      borderRadius: "6px",
                      cursor: "pointer",
                      fontWeight: "bold",
                    }}
                  >
                    CANCEL
                  </button>
                  <button
                    onClick={handleEditAttack}
                    style={{
                      padding: "10px",
                      backgroundColor: "#bb86fc",
                      color: "#000",
                      border: "none",
                      borderRadius: "6px",
                      cursor: "pointer",
                      fontWeight: "bold",
                    }}
                  >
                    SAVE CHANGES
                  </button>
                </div>
              </div>
            )}

            {activeModal === "tracker" && (
              <div>
                <h3 style={{ margin: "0 0 20px 0", textAlign: "center" }}>
                  {editingTrackerId ? "Edit Tracker" : "Create Tracker"}
                </h3>
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "11px", color: "#757575" }}>Name</label>
                  <input type="text" value={trackerForm.name} onChange={(e) => setTrackerForm({ ...trackerForm, name: e.target.value })} style={{ width: "100%", padding: "8px", background: "#121212", border: "1px solid #333", color: "#fff", marginTop: "5px", boxSizing: "border-box", borderRadius: "4px" }} />
                </div>
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "11px", color: "#757575" }}>Max Value</label>
                  <input type="number" value={trackerForm.maxValue} onChange={(e) => setTrackerForm({ ...trackerForm, maxValue: Number(e.target.value) })} style={{ width: "100%", padding: "8px", background: "#121212", border: "1px solid #333", color: "#fff", marginTop: "5px", boxSizing: "border-box", borderRadius: "4px" }} />
                </div>
                {editingTrackerId && (
                  <div style={{ marginBottom: "15px" }}>
                    <label style={{ fontSize: "11px", color: "#757575" }}>Current Value</label>
                    <input type="number" value={trackerForm.currentValue} onChange={(e) => setTrackerForm({ ...trackerForm, currentValue: Number(e.target.value) })} style={{ width: "100%", padding: "8px", background: "#121212", border: "1px solid #333", color: "#fff", marginTop: "5px", boxSizing: "border-box", borderRadius: "4px" }} />
                  </div>
                )}
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "11px", color: "#757575" }}>Reset Condition</label>
                  <select value={trackerForm.resetCondition} onChange={(e) => setTrackerForm({ ...trackerForm, resetCondition: Number(e.target.value) })} style={{ width: "100%", padding: "8px", background: "#121212", border: "1px solid #333", color: "#fff", marginTop: "5px", boxSizing: "border-box", borderRadius: "4px" }}>
                    <option value="0">None</option>
                    <option value="1">Short Rest</option>
                    <option value="2">Long Rest</option>
                    <option value="3">Dawn</option>
                  </select>
                </div>
                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "10px" }}>
                  <button onClick={() => { setTrackerForm({ name: "", maxValue: 1, currentValue: 1, resetCondition: 1 }); closeModals(); }} style={{ padding: "10px", backgroundColor: "#333", color: "#fff", border: "none", borderRadius: "6px", cursor: "pointer", fontWeight: "bold" }}>CANCEL</button>
                  <button onClick={editingTrackerId ? handleEditTracker : handleAddTracker}
                    style={{
                      padding: "10px",
                      backgroundColor: "#bb86fc",
                      color: "#000",
                      border: "none",
                      borderRadius: "6px",
                      cursor: "pointer",
                      fontWeight: "bold",
                    }}
                  >
                    SAVE CHANGES
                  </button>
                </div>
              </div>
            )}

            {activeModal === "settings" && (
              <div>
                <h3 style={{ margin: "0 0 20px 0", textAlign: "center" }}>
                  Character Settings
                </h3>
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "12px", color: "#757575" }}>
                    Name
                  </label>
                  <input
                    defaultValue={char.name}
                    onBlur={(e) =>
                      updateCharacter("/progression", "put", {
                        Name: e.target.value,
                      })
                    }
                    style={{
                      width: "100%",
                      padding: "10px",
                      background: "#121212",
                      border: "1px solid #333",
                      color: "#fff",
                      marginTop: "5px",
                      boxSizing: "border-box",
                      borderRadius: "4px",
                    }}
                  />
                </div>
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "12px", color: "#757575" }}>
                    Race
                  </label>
                  <input
                    defaultValue={char.race}
                    onBlur={(e) =>
                      updateCharacter("/progression", "put", {
                        Race: e.target.value,
                      })
                    }
                    style={{
                      width: "100%",
                      padding: "10px",
                      background: "#121212",
                      border: "1px solid #333",
                      color: "#fff",
                      marginTop: "5px",
                      boxSizing: "border-box",
                      borderRadius: "4px",
                    }}
                  />
                </div>
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "12px", color: "#757575" }}>
                    Class
                  </label>
                  <input
                    defaultValue={char.class}
                    onBlur={(e) =>
                      updateCharacter("/progression", "put", {
                        Class: e.target.value,
                      })
                    }
                    style={{
                      width: "100%",
                      padding: "10px",
                      background: "#121212",
                      border: "1px solid #333",
                      color: "#fff",
                      marginTop: "5px",
                      boxSizing: "border-box",
                      borderRadius: "4px",
                    }}
                  />
                </div>
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "12px", color: "#757575" }}>
                    Image URL
                  </label>
                  <input
                    defaultValue={char.imageUrl}
                    onBlur={(e) =>
                      updateCharacter("/image", "patch", {
                        ImageUrl: e.target.value,
                      })
                    }
                    style={{
                      width: "100%",
                      padding: "10px",
                      background: "#121212",
                      border: "1px solid #333",
                      color: "#fff",
                      marginTop: "5px",
                      boxSizing: "border-box",
                      borderRadius: "4px",
                    }}
                  />
                </div>
                <div style={{ marginBottom: "15px" }}>
                  <label style={{ fontSize: "12px", color: "#757575" }}>
                    Discord Webhook
                  </label>
                  <input
                    defaultValue={char.discordWebhookUrl}
                    onBlur={(e) =>
                      updateCharacter("/integrations", "patch", {
                        DiscordWebhookUrl: e.target.value,
                      })
                    }
                    style={{
                      width: "100%",
                      padding: "10px",
                      background: "#121212",
                      border: "1px solid #333",
                      color: "#fff",
                      marginTop: "5px",
                      boxSizing: "border-box",
                      borderRadius: "4px",
                    }}
                  />
                </div>
                <div style={{ marginBottom: "20px" }}>
                  <label style={{ fontSize: "12px", color: "#757575" }}>
                    Theme Color (Discord Embed)
                  </label>
                  <div
                    style={{ display: "flex", gap: "10px", marginTop: "5px" }}
                  >
                    <input
                      type="color"
                      defaultValue={char.themeColor || "#bb86fc"}
                      onBlur={(e) =>
                        updateCharacter("/integrations", "patch", {
                          ThemeColor: e.target.value,
                        })
                      }
                      style={{
                        width: "40px",
                        height: "40px",
                        padding: 0,
                        border: "none",
                        background: "none",
                        cursor: "pointer",
                      }}
                    />
                    <input
                      type="text"
                      defaultValue={char.themeColor || "#bb86fc"}
                      onBlur={(e) =>
                        updateCharacter("/integrations", "patch", {
                          ThemeColor: e.target.value,
                        })
                      }
                      style={{
                        flex: 1,
                        padding: "10px",
                        background: "#121212",
                        border: "1px solid #333",
                        color: "#fff",
                        borderRadius: "4px",
                        boxSizing: "border-box",
                      }}
                    />
                  </div>
                </div>
                <div
                  style={{
                    marginBottom: "10px",
                    display: "flex",
                    alignItems: "center",
                    gap: "10px",
                  }}
                >
                  <input
                    type="checkbox"
                    checked={sendRollsToDiscord}
                    onChange={(e) => setSendRollsToDiscord(e.target.checked)}
                  />
                  <span style={{ fontSize: "14px" }}>
                    Send rolls to Discord
                  </span>
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      {/* --- ROLL STACK --- */}
      <div className="cs-roll-stack">
        {rolls.map((roll, i) => {
          const isNat20 = roll.diceType === 20 && roll.baseRoll === 20;
          const isNat1 = roll.diceType === 20 && roll.baseRoll === 1;
          let rollBorder = i === 0 ? "#bb86fc" : "#444";
          let rollAnimation = "fadeIn 0.2s";

          if (isNat20) {
            rollBorder = "#ffd700";
            rollAnimation = "fadeIn 0.2s, critSuccessPulse 1.5s infinite 0.2s";
          } else if (isNat1) {
            rollBorder = "#f44336";
            rollAnimation = "fadeIn 0.2s, critFailShake 0.5s 3 0.2s";
          }

          return (
            <div
              key={roll.id}
              className="cs-roll-card"
              style={{
                border: `1px solid ${rollBorder}`,
                animation: rollAnimation,
                opacity: i > 2 ? 0.4 : 1,
              }}
            >
              <button
                onClick={() => removeRoll(roll.id)}
                style={{
                  position: "absolute",
                  top: "5px",
                  right: "5px",
                  background: "none",
                  border: "none",
                  color: "#757575",
                  cursor: "pointer",
                  fontWeight: "bold",
                }}
              >
                x
              </button>
              <div
                style={{
                  fontSize: "10px",
                  color: "#b0b0b0",
                  textTransform: "uppercase",
                  paddingRight: "15px",
                }}
              >
                {roll.label}{" "}
                <span style={{ float: "right" }}>{roll.timeString}</span>
              </div>
              <div
                style={{
                  fontSize: "28px",
                  fontWeight: "bold",
                  color: isNat20 ? "#ffd700" : isNat1 ? "#f44336" : "#fff",
                  textAlign: "center",
                  margin: "5px 0",
                  textShadow: isNat20 ? "0 0 10px rgba(255, 215, 0, 0.5)" : isNat1 ? "0 0 10px rgba(244, 67, 54, 0.5)" : "none",
                }}
              >
                {roll.total}
              </div>
              <div
                style={{
                  fontSize: "10px",
                  color: "#757575",
                  textAlign: "center",
                }}
              >
                {roll.customBreakdown ? roll.customBreakdown : `(${roll.baseRoll} + ${roll.bonus})`}
              </div>
            </div>
          );
        })}
      </div>

      {/* DICE MENU */}
      <div className="cs-dice-fab-wrap">
        {isDiceMenuOpen && (
          <div className="cs-dice-menu">
            {[100, 20, 12, 10, 8, 6, 4].map((d) => (
              <button
                key={d}
                onClick={() => {
                  rollDice(d, 0, `d${d} Roll`);
                  setIsDiceMenuOpen(false);
                }}
              >
                d{d}
              </button>
            ))}
          </div>
        )}
        <button
          onClick={() => setIsDiceMenuOpen(!isDiceMenuOpen)}
          className="cs-dice-fab"
        >
          DICE
        </button>
      </div>
    </div>
  );
}
