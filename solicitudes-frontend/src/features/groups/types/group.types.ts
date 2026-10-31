export interface LocalGroup {
  localId?: number;
  name: string;
  parentGroupId: number | null;
  createdAt: string;
}

export interface GroupTreeNode extends LocalGroup {
  children: GroupTreeNode[];
  directRequestCount: number;
  totalRequestCount: number;
}
