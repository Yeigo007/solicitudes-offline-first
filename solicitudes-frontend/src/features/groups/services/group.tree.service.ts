import { database } from '../../../infrastructure/database/app.database';
import type { GroupTreeNode } from '../types/group.types';

export class GroupTreeService {

  async getTree(): Promise<GroupTreeNode[]> {
    const groups = await database.groups.toArray();
    const requests = await database.requests.toArray();

    const nodes = new Map<number, GroupTreeNode>();

    // Crear un nodo por cada grupo
    for (const group of groups) {
      if (group.localId === undefined) continue;

      const directRequestCount = requests.filter(
        request => request.groupId === group.localId
      ).length;

      nodes.set(group.localId, {
        ...group,
        children: [],
        directRequestCount,
        totalRequestCount: directRequestCount
      });
    }

    const roots: GroupTreeNode[] = [];

    // Construir las relaciones padre-hijo
    for (const group of groups) {
      if (group.localId === undefined) continue;

      const node = nodes.get(group.localId);

      if (!node) continue;

      if (group.parentGroupId === null) {
        roots.push(node);
        continue;
      }

      const parent = nodes.get(group.parentGroupId);

      if (parent) {
        parent.children.push(node);
      } else {
        // Un grupo cuyo padre no existe se muestra como raíz.
        roots.push(node);
      }
    }

    // Calcular el total incluyendo todos los descendientes
    const calculateTotals = (node: GroupTreeNode): number => {
      let total = node.directRequestCount;

      for (const child of node.children) {
        total += calculateTotals(child);
      }

      node.totalRequestCount = total;

      return total;
    };

    for (const root of roots) {
      calculateTotals(root);
    }

    return roots;
  }
}

export const groupTreeService = new GroupTreeService();
