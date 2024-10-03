// UI Interfaces
export interface Page {
  id: number;
  title: string;
  slug: string;
  content: string;
  components: Component[];
}

export interface PageListItemProps {
    page: Page;
}

// components.ts
export interface Component {
  component_id: number;
  id: number;
  name: string;
  settings: Record<string, any>;
  ordinal: number;
}

export interface ComponentListItemProps {
    component: Component;
    componentPages: string[];
}


// WebSocker Interfaces
export interface Mutation {
  type: "ordinalUpdate" | "pageMove";
  newOrdinal?: number;
  destinationPageID?: number;
  pageComponentID: number;
  componentName?: string;
}

export interface Message {
  data?: {
    pages: Page[];
  };
  mutations?: Mutation[];
}
