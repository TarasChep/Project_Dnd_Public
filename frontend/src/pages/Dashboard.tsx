import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api/axios";

interface Character {
  id: string;
  name: string;
  race: string;
  class: string;
  level: number;
  imageUrl: string | null;
  currentHp: number;
  maxHp: number;
  temporaryHp: number;
}

export default function Dashboard() {
  const navigate = useNavigate();
  const [characters, setCharacters] = useState<Character[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchCharacters = async () => {
      try {
        // Використовуємо /characters (множина, як домовилися раніше)
        const response = await api.get("/characters");
        setCharacters(response.data);
      } catch (err: any) {
        console.error("Fetch error:", err);
        setError("Failed to load characters. Is backend running?");
      } finally {
        setIsLoading(false);
      }
    };

    fetchCharacters();
  }, []);

  const handleCreateCharacter = async () => {
    try {
      const payload = {
        Name: "New Hero", // Валідатор на бекенді не пропускає пусте ім'я
        Race: "",
        Class: "",
        Strength: 10,
        Dexterity: 10,
        Constitution: 10,
        Intelligence: 10,
        Wisdom: 10,
        Charisma: 10,
        MaxHp: 10,
        CurrentHp: 10,
        ArmorClass: 10,
        CurrentXp: 0,
      };
      const response = await api.post("/characters", payload);
      navigate(`/character/${response.data.id}`);
    } catch (err: any) {
      console.error("Failed to create character:", err);
      setError("Failed to create character.");
    }
  };

  return (
    <div>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          marginBottom: "30px",
        }}
      >
        <h1 style={{ margin: 0, color: "#ffffff", fontSize: "32px" }}>
          My Heroes
        </h1>
        <button
          onClick={handleCreateCharacter}
          style={{
            padding: "12px 24px",
            backgroundColor: "#03dac6",
            color: "#000",
            border: "none",
            borderRadius: "4px",
            cursor: "pointer",
            fontSize: "16px",
            fontWeight: "bold",
            boxShadow: "0 4px 14px rgba(3, 218, 198, 0.3)",
          }}
        >
          + Create New Character
        </button>
      </div>

      {error && (
        <div
          style={{
            padding: "15px",
            backgroundColor: "#cf6679",
            color: "#000",
            borderRadius: "4px",
            marginBottom: "20px",
            fontWeight: "bold",
          }}
        >
          ⚠️ {error}
        </div>
      )}

      {isLoading ? (
        <p style={{ fontSize: "18px", color: "#b0b0b0" }}>
          Searching the tavern...
        </p>
      ) : characters.length === 0 && !error ? (
        <div
          style={{
            padding: "80px",
            border: "2px dashed #333",
            textAlign: "center",
            borderRadius: "12px",
            backgroundColor: "#1e1e1e",
          }}
        >
          <h3
            style={{ color: "#b0b0b0", fontSize: "24px", marginBottom: "10px" }}
          >
            Your party is empty
          </h3>
          <p style={{ color: "#757575" }}>
            No characters found in the database.
          </p>
        </div>
      ) : (
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fill, minmax(320px, 1fr))",
            gap: "25px",
          }}
        >
          {characters.map((char) => {
            const hpPercentage = (char.currentHp / char.maxHp) * 100;
            const hpColor =
              hpPercentage > 50
                ? "#4caf50"
                : hpPercentage > 20
                  ? "#ff9800"
                  : "#f44336";

            return (
              <div
                key={char.id}
                onClick={() => navigate(`/character/${char.id}`)}
                style={{
                  backgroundColor: "#1e1e1e",
                  borderRadius: "12px",
                  overflow: "hidden",
                  border: "1px solid #333",
                  boxShadow: "0 8px 16px rgba(0,0,0,0.4)",
                  transition: "transform 0.2s, border-color 0.2s",
                  cursor: "pointer",
                }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.transform = "translateY(-5px)";
                  e.currentTarget.style.borderColor = "#bb86fc";
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.transform = "translateY(0)";
                  e.currentTarget.style.borderColor = "#333";
                }}
              >
                {/* Character Image Section */}
                <div
                  style={{
                    height: "180px",
                    backgroundColor: "#2c2c2c",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    position: "relative",
                    overflow: "hidden",
                  }}
                >
                  {char.imageUrl ? (
                    <img
                      src={char.imageUrl}
                      alt={char.name}
                      style={{
                        width: "100%",
                        height: "100%",
                        objectFit: "cover",
                      }}
                    />
                  ) : (
                    <span style={{ fontSize: "64px", opacity: 0.2 }}>🧙‍♂️</span>
                  )}
                  <div
                    style={{
                      position: "absolute",
                      bottom: "10px",
                      left: "10px",
                      backgroundColor: "rgba(0,0,0,0.7)",
                      padding: "4px 10px",
                      borderRadius: "4px",
                      fontSize: "12px",
                      fontWeight: "bold",
                      color: "#bb86fc",
                      border: "1px solid #bb86fc",
                    }}
                  >
                    LVL {char.level}
                  </div>
                </div>

                {/* Info Section */}
                <div style={{ padding: "20px" }}>
                  <h2
                    style={{
                      margin: "0 0 5px 0",
                      color: "#fff",
                      fontSize: "22px",
                    }}
                  >
                    {char.name}
                  </h2>
                  <p
                    style={{
                      margin: "0 0 15px 0",
                      color: "#b0b0b0",
                      textTransform: "capitalize",
                    }}
                  >
                    {char.race} {char.class}
                  </p>

                  {/* HP Bar */}
                  <div
                    style={{
                      marginBottom: "5px",
                      display: "flex",
                      justifyContent: "space-between",
                      fontSize: "12px",
                      fontWeight: "bold",
                    }}
                  >
                    <span style={{ color: hpColor }}>
                      HP {char.currentHp} / {char.maxHp}
                    </span>
                    {char.temporaryHp > 0 && (
                      <span style={{ color: "#03dac6" }}>
                        +{char.temporaryHp} TMP
                      </span>
                    )}
                  </div>
                  <div
                    style={{
                      width: "100%",
                      height: "8px",
                      backgroundColor: "#333",
                      borderRadius: "4px",
                      overflow: "hidden",
                    }}
                  >
                    <div
                      style={{
                        width: `${Math.min(hpPercentage, 100)}%`,
                        height: "100%",
                        backgroundColor: hpColor,
                        transition: "width 0.3s ease",
                      }}
                    />
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
