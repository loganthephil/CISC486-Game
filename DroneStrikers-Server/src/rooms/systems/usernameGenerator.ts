import { CommonUtils } from "src/utils";

// prettier-ignore
const BASE_NOUNS = [
  "Shadow", "Nova", "Blaze", "Pixel", "Orbit", "Vortex", "Phantom", "Echo",
  "Comet", "Rogue", "Falcon", "Rider", "Spectre", "Drifter", "Hunter",
  "Raven", "Wolf", "Tiger", "Dragon", "Phoenix", "Knight", "Sniper", "Ghost",
  "Reaper", "Ranger", "Samurai", "Ninja", "Wizard", "Mage", "Warlock",
  "Raptor", "Striker", "Rocket", "Bullet", "Trigger", "Glitch", "Circuit",
  "Drone", "Pilot", "Crusher", "Crusher", "Drift", "Volt", "Storm", "Blizzard",
  "Inferno", "Fury", "Hammer", "Blade", "Arrow", "Skull", "Rift", "Pulse",
  "Vector", "Cipher", "Cipher", "Crimson", "Nebula", "Meteor", "Astro",
  "Galaxy", "Orbit", "Comet", "Oblivion", "Mirage", "Eclipse", "Talon",
];

// prettier-ignore
const SOFT_WORDS = [
  "luna", "bloom", "mist", "aura", "dream", "cloud", "berry", "petal",
  "echo", "mocha", "mint", "moss", "dust", "glow", "willow", "river",
  "pearl", "sunrise", "sunset", "dawn", "dusk", "violet", "rose", "iris",
  "maple", "snow", "frost", "haze", "ember", "rain", "drizzle", "stone",
  "star", "starlit", "cosmos", "comet", "nebula", "midnight", "hollow",
  "velvet", "silk", "sugar", "honey", "mellow", "cotton", "marsh", "aurora",
];

// prettier-ignore
const ADJECTIVES = [
  "Silent", "Neon", "Cosmic", "Crimson", "Frozen", "Electric", "Golden",
  "Swift", "Radiant", "Obsidian", "Turbo", "Quantum",
  "Dark", "Rapid", "Stealth", "Savage", "Elite", "Lazy", "Furious",
  "Lucky", "Hidden", "Mystic", "Arctic", "Solar", "Lunar", "Nuclear",
  "Toxic", "Venom", "Cyber", "Chrome", "Pixelated", "Static", "Stormy",
  "Blazing", "Frosted", "Shadowed", "Shiny", "Cursed", "Blessed",
];

// prettier-ignore
const SUFFIXES = [
  "XD", "TV", "YT", "ZZ", "OP", "HQ", "JR", "PRO", "GG", "LOL", "LMAO",
];

// prettier-ignore
const TRYHARD_BASES = [
  "NoScope", "TryHard", "Sweaty", "AimGod", "LagMaster",
  "ClutchKing", "Tilted", "OneTap", "HardCarry", "Smurf",
  "TopFrag", "SoloQ", "RankGrinder", "FeedMachine",
];

// Pattern selection:
//  - 45%: gamer-style (adjective+noun etc.)
//  - 35%: aesthetic/soft
//  - 20%: tryhard/meme
export function generateAIUsername(): string {
  const r = Math.random();
  let baseName: string;

  if (r < 0.45) {
    baseName = makeGamerName();
  } else if (r < 0.8) {
    baseName = makeAestheticName();
  } else {
    baseName = makeTryhardName();
  }

  // 5% chance to decorate with xX/Xx style
  if (Math.random() < 0.05) {
    baseName = decorateWithXX(baseName);
  }

  return baseName;
}

// Helper shortcuts
function randInt(min: number, max: number): number {
  return Math.floor(CommonUtils.randomRange(min, max + 1));
}

function pick<T>(arr: T[]): T {
  return CommonUtils.randomChoice(arr);
}

// Maybe add a numeric suffix
function maybeNumber(probability: number = 0.7): string {
  if (Math.random() > probability) return "";
  const len = Math.random() < 0.5 ? 2 : 3;
  const min = len === 2 ? 10 : 100;
  const max = len === 2 ? 99 : 999;
  return String(randInt(min, max));
}

// xX / Xx decorations
function decorateWithXX(name: string): string {
  const style = Math.random();
  if (style < 0.33) {
    // xXNameXx
    return `xX${name}Xx`;
  } else if (style < 0.66) {
    // Xx_Name_xX
    return `Xx_${name}_xX`;
  } else {
    // xxNAMExx
    const upper = Math.random() < 0.5 ? name.toUpperCase() : name;
    return `xx${upper}xx`;
  }
}

// Pattern 1: Normal / gamer-style
function makeGamerName(): string {
  const style = Math.random();
  const adj = pick(ADJECTIVES);
  const noun = pick(BASE_NOUNS);

  if (style < 0.4) {
    // Adjective + Noun
    return adj + noun;
  } else if (style < 0.8) {
    // Adjective + Noun + numbers or suffix
    if (Math.random() < 0.5) {
      return adj + noun + maybeNumber();
    } else {
      return noun + pick(SUFFIXES);
    }
  } else {
    // xNoun_## or __Noun
    if (Math.random() < 0.5) {
      return "x" + noun + "_" + randInt(3, 99);
    } else {
      return "_" + noun + maybeNumber(0.9);
    }
  }
}

// Pattern 2: Aesthetic / soft
function makeAestheticName(): string {
  const w1 = pick(SOFT_WORDS);
  const w2 = Math.random() < 0.6 ? pick(SOFT_WORDS) : pick(BASE_NOUNS).toLowerCase();
  const style = Math.random();

  if (style < 0.35) {
    // w1.w2
    return `${w1}.${w2}`;
  } else if (style < 0.7) {
    // w1_w2
    return `${w1}_${w2}`;
  } else {
    // doubled letter inside w1: e.g. moonn
    const doubledIndex = randInt(1, Math.max(1, w1.length - 1));
    const chars = w1.split("");
    chars.splice(doubledIndex, 0, chars[doubledIndex]);
    const result = chars.join("");

    // occasionally add a small number
    return Math.random() < 0.3 ? result + randInt(1, 99) : result;
  }
}

// Pattern 3: Tryhard / meme-ish but "normal player"
function makeTryhardName(): string {
  const base = pick(TRYHARD_BASES);
  const noun = pick(BASE_NOUNS);

  const style = Math.random();
  if (style < 0.3) {
    // Base + Noun (NoScopeShadow)
    return base + noun;
  } else if (style < 0.7) {
    // Base + number or Noun + number
    if (Math.random() < 0.5) {
      return base + maybeNumber(1.0);
    } else {
      return noun + maybeNumber(1.0);
    }
  } else {
    // lowercase with underscore: noscope_shadow, sweaty_pilot
    const lowerBase = base.toLowerCase();
    const lowerNoun = noun.toLowerCase();
    return `${lowerBase}_${lowerNoun}`;
  }
}
