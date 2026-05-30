import { useEffect, useState, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { api } from "../api/axios";
import CompactCharacterCard from "../components/CompactCharacterCard";
import EncounterTracker from "../components/EncounterTracker";
import { useAuthStore } from "../store/authStore";
import { extractUserIdFromToken } from "../utils/tokenUtils";
import "./CampaignDashboard.css";
import { EncounterAnalyticsView } from "../components/EncounterAnalyticsView";

type MobilePanel = "party" | "encounters" | "battle";


const EncounterSidebar = ({ isGm, campaignId, encounters, setEncounters, onStart, onEdit, onDeleted }: { isGm: boolean; campaignId: string; encounters: any[]; setEncounters: React.Dispatch<React.SetStateAction<any[]>>; onStart: (id: string) => void; onEdit: (id: string) => void; onDeleted: (id: string) => void }) => {
  const [isCreating, setIsCreating] = useState(false);
  const [encounterName, setEncounterName] = useState("");

  if (!isGm) return null; // STRICT RULE: Only visible if isGm === true

  const handleCreate = async () => {
    if (!encounterName.trim()) return;
    setIsCreating(true);
    try {
      const res = await api.post('/encounters/campaign', { 
        campaignId: campaignId, 
        name: encounterName.trim() 
      });
      setEncounters((prev) => [...prev, res.data]);
      setEncounterName("");
      onEdit(res.data.id); // Автоматично відкриваємо вікно налаштувань
    } catch (err: any) {
      alert(err.response?.data?.error || "Failed to create encounter");
    } finally {
      setIsCreating(false);
    }
  };

  const handleDelete = async (encId: string) => {
    if (!confirm("Are you sure you want to delete this encounter?")) return;
    try {
      await api.delete(`/encounters/${encId}`);
      setEncounters((prev) => prev.filter((e) => e.id !== encId));
      onDeleted(encId);
    } catch (err: any) {
      alert("Failed to delete encounter");
    }
  };

  return (
    <aside className="camp-encounters">
      <h2 className="camp-encounters__title">Encounter Manager</h2>

      <div className="camp-encounters__form">
        <input
          type="text"
          className="camp-input"
          placeholder="Encounter name…"
          value={encounterName}
          onChange={(e) => setEncounterName(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && handleCreate()}
        />
        <button
          type="button"
          className="camp-btn camp-btn--primary"
          onClick={handleCreate}
          disabled={isCreating || !encounterName.trim()}
        >
          {isCreating ? "Creating…" : "+ Create encounter"}
        </button>
      </div>

      {encounters.length === 0 ? (
        <p className="camp-encounters__empty">No encounters yet. Create one to start a session.</p>
      ) : (
        encounters.map((enc) => (
          <div key={enc.id} className="camp-encounter-card">
            <h3 className="camp-encounter-card__name">{enc.name}</h3>
            <div className="camp-encounter-card__actions">
              <button type="button" className="camp-btn camp-btn--ghost camp-btn--sm" onClick={() => onEdit(enc.id)}>
                Edit
              </button>
              <button type="button" className="camp-btn camp-btn--teal camp-btn--sm" onClick={() => onStart(enc.id)}>
                Open
              </button>
              <button type="button" className="camp-btn camp-btn--danger-icon camp-btn--sm" onClick={() => handleDelete(enc.id)} aria-label="Delete encounter">
                ✕
              </button>
            </div>
          </div>
        ))
      )}
    </aside>
  );
};

// --- Модальне вікно для налаштування Encounter ---
const EncounterSettingsModal = ({ encounterId, onClose, campaignCharacters }: { encounterId: string, onClose: () => void, campaignCharacters: any[] }) => {
  const [encounter, setEncounter] = useState<any>(null);
  const [myChars, setMyChars] = useState<any[]>([]);
  const [searchQuery, setSearchQuery] = useState("");

  const loadData = async () => {
    try {
      const [encRes, charsRes] = await Promise.all([
        api.get(`/encounters/${encounterId}`),
        api.get(`/characters`)
      ]);
      setEncounter(encRes.data);
      setMyChars(charsRes.data);
    } catch (err) { console.error(err); }
  };

  useEffect(() => { loadData(); }, [encounterId]);

  const handleAdd = async (charId: string, name: string, faction: number) => {
    try {
      await api.post(`/encounters/${encounterId}/participants`, { characterId: charId, faction, customName: name });
      loadData();
    } catch (err) { console.error(err); }
  };

  const handleRemove = async (partId: string) => {
    try {
      await api.delete(`/encounters/${encounterId}/participants/${partId}`);
      loadData();
    } catch (err) { console.error(err); }
  };

  // Комбінуємо всіх можливих персонажів для пошуку (і прибираємо дублікати)
  const searchPool = [
    ...(campaignCharacters || []).map((c: any) => ({ id: c.characterId, name: c.character?.name || c.characterName, source: 'Campaign' })),
    ...myChars.map(c => ({ id: c.id, name: c.name, source: 'My Heroes' }))
  ];
  const uniquePool = Array.from(new Map(searchPool.map(item => [item.id, item])).values());
  const filtered = uniquePool.filter(c => c.name.toLowerCase().includes(searchQuery.toLowerCase()));

  if (!encounter) return null;
  
  const allies = encounter.participants.filter((p: any) => p.faction === 1 || p.faction === 'Player');
  const enemies = encounter.participants.filter((p: any) => p.faction === 2 || p.faction === 'Enemy');

  return (
    <div className="camp-modal-overlay" onClick={onClose}>
      <div className="camp-modal camp-modal--wide" onClick={(e) => e.stopPropagation()}>
        <div className="camp-modal__head">
          <h2 className="camp-modal__title">
            Configure: <span>{encounter.name}</span>
          </h2>
          <button type="button" className="camp-modal__close" onClick={onClose} aria-label="Close">
            ✕
          </button>
        </div>

        <div className="camp-modal__grid">
          <div className="camp-modal__column">
            <div className="camp-modal__panel">
              <h3 className="camp-modal__panel-title camp-modal__panel-title--ally">Allies ({allies.length})</h3>
              {allies.length === 0 && <p className="camp-encounters__empty">No allies yet.</p>}
              {allies.map((p: any) => (
                <div key={p.id} className="camp-participant-row camp-participant-row--ally">
                  <span>{p.customName || p.characterName}</span>
                  <button type="button" className="camp-btn camp-btn--reject camp-btn--sm" onClick={() => handleRemove(p.id)}>
                    Remove
                  </button>
                </div>
              ))}
            </div>
            <div className="camp-modal__panel">
              <h3 className="camp-modal__panel-title camp-modal__panel-title--enemy">Enemies ({enemies.length})</h3>
              {enemies.length === 0 && <p className="camp-encounters__empty">No enemies yet.</p>}
              {enemies.map((p: any) => (
                <div key={p.id} className="camp-participant-row camp-participant-row--enemy">
                  <span>{p.customName || p.characterName}</span>
                  <button type="button" className="camp-btn camp-btn--reject camp-btn--sm" onClick={() => handleRemove(p.id)}>
                    Remove
                  </button>
                </div>
              ))}
            </div>
          </div>

          <div className="camp-modal__column">
            <input
              type="text"
              className="camp-input"
              placeholder="Search characters…"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
            <div className="camp-modal__panel" style={{ maxHeight: "none", flex: 1 }}>
              {filtered.map((char) => (
                <div key={char.id} className="camp-participant-row">
                  <div>
                    <div>{char.name}</div>
                    <div style={{ fontSize: "0.7rem", color: "var(--cd-muted)" }}>{char.source}</div>
                  </div>
                  <div style={{ display: "flex", gap: "6px", flexWrap: "wrap" }}>
                    <button type="button" className="camp-btn camp-btn--teal camp-btn--sm" onClick={() => handleAdd(char.id, char.name, 1)}>
                      + Ally
                    </button>
                    <button type="button" className="camp-btn camp-btn--reject camp-btn--sm" onClick={() => handleAdd(char.id, char.name, 2)}>
                      + Enemy
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
        
        <div style={{ marginTop: "20px" }}>
          <EncounterAnalyticsView encounterId={encounterId} isNewEncounter={!encounterId} />
        </div>
      </div>
    </div>
  );
};

const FolderView = ({ folder, characters, isGm, onImport, onCardClick }: any) => {
  const [isExpanded, setIsExpanded] = useState(true);
  const canImport = isGm || folder.id === null; // Player can only import to the "Active Party" (null folder)
  
  return (
    <section className={`camp-folder${isExpanded ? "" : " camp-folder--collapsed"}`}>
      <div className="camp-folder__head" onClick={() => setIsExpanded(!isExpanded)} role="button" tabIndex={0} onKeyDown={(e) => e.key === "Enter" && setIsExpanded(!isExpanded)}>
        <h3 className="camp-folder__title">
          <span aria-hidden>📁</span> {folder.name}
          <span className="camp-folder__count">({characters.length})</span>
        </h3>
        <div className="camp-folder__head-actions">
          {canImport && (
            <button
              type="button"
              className="camp-btn camp-btn--teal camp-btn--sm"
              onClick={(e) => {
                e.stopPropagation();
                onImport(folder.id);
              }}
            >
              {isGm ? "Import" : "+ Add mine"}
            </button>
          )}
          <span className="camp-folder__chevron" aria-hidden>
            ▼
          </span>
        </div>
      </div>
      {isExpanded && (
        <div className="camp-folder__body">
          {characters.length === 0 && <p className="camp-folder__empty">Folder is empty.</p>}
          {characters.map((c: any) => (
            <CompactCharacterCard
              key={c.characterId}
              char={{ ...(c.character || c), ownerUserName: c.ownerUserName }}
              href={`/character/${c.characterId}`}
            />
          ))}
        </div>
      )}
    </section>
  );
};

export default function CampaignDashboard() {
  const { id } = useParams();
  const navigate = useNavigate();
  const token = useAuthStore((state) => state.token);
  const currentUserId = extractUserIdFromToken(token);
  
  const [campaign, setCampaign] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [myCharacters, setMyCharacters] = useState<any[]>([]);
  const [activeModal, setActiveModal] = useState<"none" | "import">("none");
  const [selectedCharId, setSelectedCharId] = useState("");
  const [selectedFolderId, setSelectedFolderId] = useState<string | null>(null);
  
  // Стан для відображення правого блоку EncounterTracker
  const [activeEncounterId, setActiveEncounterId] = useState<string | null>(null);
  const [editingEncounterId, setEditingEncounterId] = useState<string | null>(null);
  const [encounters, setEncounters] = useState<any[]>([]);

  const [isCreatingFolder, setIsCreatingFolder] = useState(false);
  const [newFolderName, setNewFolderName] = useState("");
  const [isSavingFolder, setIsSavingFolder] = useState(false);
  const [mobilePanel, setMobilePanel] = useState<MobilePanel>("party");
  const folderInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (activeEncounterId) {
      setMobilePanel("battle");
    }
  }, [activeEncounterId]);

  useEffect(() => {
    fetchCampaign();
    if (id) {
      api.get(`/encounters/campaign/${id}`)
        .then(res => setEncounters(res.data))
        .catch(err => console.error("Failed to load encounters", err));
    }
  }, [id]);

  const fetchCampaign = async () => {
    try {
      const res = await api.get(`/campaigns/${id}`);
      setCampaign(res.data);
    } catch (err) {
      console.error(err);
      navigate("/campaigns");
    } finally {
      setLoading(false);
    }
  };

  const openImportModal = async (folderId: string | null = null) => {
    try {
      const res = await api.get("/characters");
      setMyCharacters(res.data);
      if (res.data.length > 0) setSelectedCharId(res.data[0].id);
      setSelectedFolderId(folderId);
      setActiveModal("import");
    } catch (err) { console.error(err); }
  };

  const handleImportSubmit = async () => {
    if (!selectedCharId) return;
    try {
      await api.post(`/campaigns/${id}/characters`, {
        characterId: selectedCharId,
        folderId: selectedFolderId
      });
      setActiveModal("none");
      fetchCampaign();
    } catch (err: any) {
      alert(err.response?.data?.message || "Failed to import character");
    }
  };

  const handleMemberAction = async (userId: string, action: 'approve' | 'reject') => {
    try {
      await api.post(`/campaigns/${id}/members/${userId}/${action}`);
      fetchCampaign();
    } catch (err) {
      console.error(err);
    }
  };

  const handleDeleteOrLeave = async (isGmRole: boolean) => {
    if (isGmRole) {
      if (!confirm("Delete Campaign? This cannot be undone.")) return;
      await api.delete(`/campaigns/${id}`);
    } else {
      if (!confirm("Leave Campaign?")) return;
      await api.delete(`/campaigns/${id}/members/${currentUserId}`);
    }
    navigate("/campaigns");
  };

  useEffect(() => {
    if (isCreatingFolder && folderInputRef.current) {
      folderInputRef.current.focus();
    }
  }, [isCreatingFolder]);

  const handleCancelFolder = () => {
    setIsCreatingFolder(false);
    setNewFolderName("");
  };

  const handleSaveFolder = async () => {
    const trimmedName = newFolderName.trim();
    if (!trimmedName) return;

    setIsSavingFolder(true);
    try {
      const response = await api.post(`/campaigns/${id}/folders`, {
        name: trimmedName,
      });
      const newFolder = response.data;
      setCampaign((prev: any) => ({
        ...prev,
        folders: [...(prev?.folders || []), newFolder]
      }));
      handleCancelFolder();
    } catch (error: any) {
      alert(error.response?.data?.message || "Failed to create folder");
    } finally {
      setIsSavingFolder(false);
    }
  };

  const handleFolderKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleSaveFolder();
    } else if (e.key === 'Escape') {
      handleCancelFolder();
    }
  };

  useEffect(() => {
    if (campaign) {
      const isGmRole = String(currentUserId).toLowerCase() === String(campaign.gmUserId).toLowerCase();
      if (!isGmRole) {
        const activeEnc = encounters.find((e: any) => e.isActive);
        if (activeEnc && activeEnc.id !== activeEncounterId) {
          setActiveEncounterId(activeEnc.id);
        } else if (!activeEnc && activeEncounterId !== null) {
          setActiveEncounterId(null);
        }
      }
    }
  }, [campaign, encounters, currentUserId, activeEncounterId]);

  if (loading || !campaign) return <div className="camp-loading">Loading campaign…</div>;

  // STRICT SINGLE-SOURCE-OF-TRUTH ROLE LOGIC
  const isGm = String(currentUserId).toLowerCase() === String(campaign.gmUserId).toLowerCase();
  
  const currentMember = (campaign.members || []).find((m: any) => String(m.userId).toLowerCase() === String(currentUserId).toLowerCase());
  const isPending = !isGm && currentMember?.status === 'Pending';

  if (isPending) {
    return (
      <div className="camp-dash">
        <button type="button" className="camp-back-link" onClick={() => navigate("/campaigns")}>
          ← Back to campaigns
        </button>
        <div className="camp-pending-page">
          <h2 style={{ color: "var(--cd-warning)", margin: "0 0 15px" }}>⏳ Pending approval</h2>
          <p style={{ color: "var(--cd-muted)", marginBottom: "25px", lineHeight: 1.6 }}>
            You requested to join <strong style={{ color: "var(--cd-text)" }}>{campaign.name}</strong>.
            <br />
            Wait for the Game Master to approve your request.
          </p>
          <button type="button" className="camp-btn camp-btn--danger-outline" onClick={() => handleDeleteOrLeave(false)}>
            Cancel join request
          </button>
        </div>
      </div>
    );
  }

  // Grouping the Active Party by ensuring they belong to APPROVED members only
  const approvedMemberIds = new Set((campaign.members || [])
    .filter((m: any) => m.status === 'Approved' || m.role === 'GM')
    .map((m: any) => String(m.userId).toLowerCase()));

  const activeParty = (campaign.characters || []).filter((c: any) => 
    c.isPlayerCharacter && approvedMemberIds.has(String(c.ownerUserId).toLowerCase())
  );
  const pendingMembers = (campaign.members || []).filter((m: any) => m.status === 'Pending');
  const folders = campaign.folders || [];

  const layoutClass = [
    "camp-dash__layout",
    isGm ? "camp-dash__layout--gm" : "camp-dash__layout--player",
    activeEncounterId ? "camp-dash__layout--tracker" : "",
  ]
    .filter(Boolean)
    .join(" ");

  const panelHidden = (panel: MobilePanel) =>
    isGm && mobilePanel !== panel ? " camp-dash__panel--hidden-mobile" : "";

  return (
    <div className="camp-dash">
      {isGm && (
        <div className="camp-dash__tabs" role="tablist">
          <button
            type="button"
            role="tab"
            className={`camp-dash__tab${mobilePanel === "party" ? " camp-dash__tab--active" : ""}`}
            onClick={() => setMobilePanel("party")}
          >
            Party
          </button>
          <button
            type="button"
            role="tab"
            className={`camp-dash__tab${mobilePanel === "encounters" ? " camp-dash__tab--active" : ""}`}
            onClick={() => setMobilePanel("encounters")}
          >
            Encounters
          </button>
          {activeEncounterId && (
            <button
              type="button"
              role="tab"
              className={`camp-dash__tab camp-dash__tab--battle${mobilePanel === "battle" ? " camp-dash__tab--active" : ""}`}
              onClick={() => setMobilePanel("battle")}
            >
              Battle
            </button>
          )}
        </div>
      )}

      <div className={layoutClass}>
        {isGm && (
          <div className={`camp-dash__panel--visible-mobile${panelHidden("encounters")}`}>
            <EncounterSidebar
              isGm={isGm}
              campaignId={id!}
              encounters={encounters}
              setEncounters={setEncounters}
              onStart={(encId) => {
                setActiveEncounterId(encId);
                setMobilePanel("battle");
              }}
              onEdit={(encId) => setEditingEncounterId(encId)}
              onDeleted={(encId) => {
                if (activeEncounterId === encId) setActiveEncounterId(null);
                if (editingEncounterId === encId) setEditingEncounterId(null);
              }}
            />
          </div>
        )}

        <main className={`camp-dash__main${panelHidden("party")}`}>
        <header className="camp-hero">
          <div className="camp-hero__content">
            <h1 className="camp-hero__title">{campaign.name}</h1>
            <div className="camp-hero__badges">
              <span className={`camp-badge ${isGm ? "camp-badge--gm" : "camp-badge--player"}`}>
                {isGm ? "GM mode" : "Player"}
              </span>
            </div>
            {isGm && (
              <p className="camp-hero__invite">
                Invite code: <code>{campaign.inviteCode}</code>
              </p>
            )}
          </div>
          <div className="camp-hero__actions">
            <button type="button" className="camp-btn camp-btn--danger-outline" onClick={() => handleDeleteOrLeave(isGm)}>
              {isGm ? "Delete campaign" : "Leave"}
            </button>
          </div>
        </header>

        {isGm && pendingMembers.length > 0 && (
          <section className="camp-pending-block">
            <h3 className="camp-pending-block__title">Pending join requests</h3>
            {pendingMembers.map((member: any) => (
              <div key={member.userId} className="camp-pending-row">
                <span style={{ fontWeight: 700 }}>{member.userName}</span>
                <div className="camp-pending-row__actions">
                  <button type="button" className="camp-btn camp-btn--reject camp-btn--sm" onClick={() => handleMemberAction(member.userId, "reject")}>
                    Reject
                  </button>
                  <button type="button" className="camp-btn camp-btn--approve camp-btn--sm" onClick={() => handleMemberAction(member.userId, "approve")}>
                    Approve
                  </button>
                </div>
              </div>
            ))}
          </section>
        )}

        {/* Active Party Section */}
        <FolderView 
          folder={{ name: "Active Party", id: null }} 
          characters={activeParty} 
          isGm={isGm} 
          onImport={() => openImportModal(null)}
        />

        {isGm && (
          <section className="camp-folders-section">
            <div className="camp-folders-section__head">
              <h2 className="camp-folders-section__title">Folder management</h2>
              {isCreatingFolder ? (
                <div className="camp-folder-create">
                  <input
                    ref={folderInputRef}
                    type="text"
                    className="camp-input"
                    value={newFolderName}
                    onChange={(e) => setNewFolderName(e.target.value)}
                    onKeyDown={handleFolderKeyDown}
                    disabled={isSavingFolder}
                    placeholder="Folder name"
                  />
                  <button type="button" className="camp-btn camp-btn--teal camp-btn--sm" onClick={handleSaveFolder} disabled={isSavingFolder || !newFolderName.trim()}>
                    {isSavingFolder ? "…" : "Save"}
                  </button>
                  <button type="button" className="camp-btn camp-btn--ghost camp-btn--sm" onClick={handleCancelFolder} disabled={isSavingFolder}>
                    Cancel
                  </button>
                </div>
              ) : (
                <button type="button" className="camp-btn camp-btn--ghost camp-btn--sm" onClick={() => setIsCreatingFolder(true)}>
                  + Add folder
                </button>
              )}
            </div>
            {folders.map((folder: any) => {
              const folderChars = (campaign.characters || []).filter((c: any) => c.folderId === folder.id && !c.isPlayerCharacter);
              return (
                <FolderView key={folder.id} folder={folder} characters={folderChars} isGm={isGm} onImport={openImportModal} />
              );
            })}
          </section>
        )}
        </main>

        {activeEncounterId && (
          <div className={`camp-dash__tracker-slot${isGm ? panelHidden("battle") : ""}`}>
            <EncounterTracker
              isGm={isGm}
              currentUserId={currentUserId}
              encounterId={activeEncounterId}
              onClose={() => {
                setActiveEncounterId(null);
                setMobilePanel("party");
              }}
            />
          </div>
        )}
      </div>

      {activeModal === "import" && (
        <div className="camp-modal-overlay" onClick={() => setActiveModal("none")}>
          <div className="camp-modal" onClick={(e) => e.stopPropagation()}>
            <h2 className="camp-modal__title" style={{ marginBottom: 20 }}>
              Import character
            </h2>
            <select className="camp-select" value={selectedCharId} onChange={(e) => setSelectedCharId(e.target.value)}>
              <option value="" disabled>
                Select a character
              </option>
              {myCharacters.map((char) => (
                <option key={char.id} value={char.id}>
                  {char.name} (Lvl {char.level})
                </option>
              ))}
            </select>
            <div className="camp-modal__actions">
              <button type="button" className="camp-btn camp-btn--ghost" onClick={() => setActiveModal("none")}>
                Cancel
              </button>
              <button type="button" className="camp-btn camp-btn--teal" onClick={handleImportSubmit} disabled={!selectedCharId}>
                Import
              </button>
            </div>
          </div>
        </div>
      )}

      {editingEncounterId && (
        <EncounterSettingsModal
          encounterId={editingEncounterId}
          onClose={() => setEditingEncounterId(null)}
          campaignCharacters={campaign.characters}
        />
      )}
    </div>
  );
}