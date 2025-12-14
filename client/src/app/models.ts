export interface Item {
  id: number;
  name: string;
  area: string;
}

export interface GroceryEntry {
  id: number;
  itemId: number | null;
  itemName: string | null;
  itemArea: string | null;
  amount?: string | null;
  note?: string | null;
  isDone: boolean;
  createdAt: string;
}

export interface CreateItemPayload {
  name: string;
  area: string;
}

export interface CreateEntryPayload {
  itemId: number | null;
  amount?: string | null;
  note?: string | null;
}

export interface AuthUser {
  id: number;
  userName: string;
  isAdmin: boolean;
}

export interface User {
  id: number;
  userName: string;
  isAdmin: boolean;
  createdAt: string;
}
