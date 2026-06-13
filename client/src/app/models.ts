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

export interface MealPlanDay {
  date: string;
  dinner: string | null;
  recipeSlug: string | null;
  recipeName: string | null;
  recipeId: string | null;
}

export interface RecipeSummary {
  id: string;
  slug: string;
  name: string;
  description: string | null;
}

export interface RecipeIngredient {
  name: string;
  amount: string | null;
  display: string;
  matchedItemId: number | null;
  matchedItemArea: string | null;
  alreadyOnList: boolean;
}

export interface RecipeIngredients {
  slug: string;
  name: string;
  ingredients: RecipeIngredient[];
}

export interface AddFromRecipeIngredient {
  name: string;
  amount: string | null;
  itemId: number | null;
  area: string | null;
}

export interface AddFromRecipePayload {
  source: string | null;
  ingredients: AddFromRecipeIngredient[];
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
