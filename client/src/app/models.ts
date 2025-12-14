export interface Item {
  id: number;
  name: string;
  area: string;
}

export interface GroceryEntry {
  id: number;
  itemId: number;
  itemName: string;
  itemArea: string;
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
  itemId: number;
  amount?: string | null;
  note?: string | null;
}
