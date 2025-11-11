export interface Player {
  name: string;
}

export function createPlayer(name: string): Player {
  return { name };
}
