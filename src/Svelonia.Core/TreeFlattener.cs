using System;
using System.Collections.Generic;
using System.Linq;

namespace Svelonia.Core;

/// <summary>
/// Utility to reactively flatten a recursive tree structure into a flat list.
/// </summary>
public static class TreeFlattener
{
    /// <summary>
    /// Creates a Computed IEnumerable that automatically stays in sync with a tree structure.
    /// </summary>
    /// <typeparam name="TNode">The type of nodes in the tree.</typeparam>
    /// <param name="root">The root node of the tree.</param>
    /// <param name="getChildren">Function to get the children of a node (usually returns a StateList).</param>
    /// <param name="isExpanded">Optional function to check if a node is expanded.</param>
    /// <param name="projectEdge">Optional function to project an additional item (e.g. an Edge) between parent and child.</param>
    /// <returns>A reactive Computed list.</returns>
    public static Computed<IEnumerable<object>> Flatten<TNode>(
        TNode root,
        Func<TNode, IEnumerable<TNode>> getChildren,
        Func<TNode, bool>? isExpanded = null,
        Func<TNode, TNode, object>? projectEdge = null)
    {
        return new Computed<IEnumerable<object>>(() =>
        {
            var result = new List<object>();
            Traverse(root, result, getChildren, isExpanded, projectEdge);
            return result;
        });
    }

    private static void Traverse<TNode>(
        TNode node, 
        List<object> result, 
        Func<TNode, IEnumerable<TNode>> getChildren,
        Func<TNode, bool>? isExpanded,
        Func<TNode, TNode, object>? projectEdge)
    {
        // Note: Ordering matches AvaXMind logic (Edges -> Children -> Parent)
        // Or common tree ordering. We'll use Depth-First Search.
        
        bool expanded = isExpanded == null || isExpanded(node);
        
        if (expanded)
        {
            foreach (var child in getChildren(node))
            {
                // Project edge before traversing child
                if (projectEdge != null)
                {
                    result.Add(projectEdge(node, child));
                }
                
                Traverse(child, result, getChildren, isExpanded, projectEdge);
            }
        }

        // Add node itself (Post-order relative to its own children, similar to AvaXMind)
        result.Add(node!);
    }
}
