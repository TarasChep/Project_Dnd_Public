import type { ElementType, CSSProperties } from "react";
import "./CompactCharacterCard.css";

interface CharacterBriefDto {
  id: string;
  name: string;
  level: number;
  class: string;
  race: string;
  imageUrl: string | null;
  currentHp: number;
  maxHp: number;
  temporaryHp: number;
  ownerUserName?: string;
}

interface Props {
  char: CharacterBriefDto | null | undefined;
  onClick?: () => void;
  href?: string;
}

export default function CompactCharacterCard({ char, onClick, href }: Props) {
  if (!char) return null;

  const maxHp = char.maxHp || 1;
  const hpPercentage = Math.min(100, Math.max(0, ((char.currentHp || 0) / maxHp) * 100));
  const hpColor = hpPercentage > 50 ? "#66bb6a" : hpPercentage > 20 ? "#ffb74d" : "#ff6b6b";
  const hpGlow =
    hpPercentage > 50 ? "rgba(102, 187, 106, 0.5)" : hpPercentage > 20 ? "rgba(255, 183, 77, 0.5)" : "rgba(255, 107, 107, 0.5)";

  const CardWrapper: ElementType = href ? "a" : "div";

  return (
    <CardWrapper
      href={href}
      target={href ? "_blank" : undefined}
      rel={href ? "noopener noreferrer" : undefined}
      onClick={onClick}
      className="compact-char-card"
      style={{ "--hp-glow": hpGlow } as CSSProperties}
    >
      <div className="compact-char-card__media">
        {char.imageUrl ? (
          <img src={char.imageUrl} alt={char.name} />
        ) : (
          <div className="compact-char-card__placeholder">🧙‍♂️</div>
        )}
        <span className="compact-char-card__level">LVL {char.level || 1}</span>
      </div>

      <div className="compact-char-card__body">
        <div>
          <div className="compact-char-card__name">{char.name}</div>
          <div className="compact-char-card__class">
            {char.race} {char.class}
          </div>
          {char.ownerUserName && <div className="compact-char-card__player">Player: {char.ownerUserName}</div>}
        </div>

        <div className="compact-char-card__hp-wrap">
          <div className="compact-char-card__hp-labels">
            <span style={{ color: hpColor }}>
              HP {char.currentHp}/{char.maxHp}
            </span>
            {char.temporaryHp > 0 && <span className="compact-char-card__temp">+{char.temporaryHp}</span>}
          </div>
          <div className="compact-char-card__hp-bar">
            <div
              className="compact-char-card__hp-fill"
              style={{ width: `${hpPercentage}%`, backgroundColor: hpColor }}
            />
          </div>
        </div>
      </div>
    </CardWrapper>
  );
}
