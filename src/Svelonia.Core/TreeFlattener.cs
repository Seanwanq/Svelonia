using System;
using System.Collections.Generic;
using System.Linq;

namespace Svelonia.Core;

/// <summary>
/// Utility to reactively flatten a recursive tree structure into a flat list.
/// </summary>
public static class TreeFlattener
{
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
        // PRE-ORDER: Parent first, then children.
        // This is much more natural for keyboard navigation (Up/Down).
        result.Add(node!);

        bool expanded = isExpanded == null || isExpanded(node);
        if (expanded)
        {
            foreach (var child in getChildren(node))
            {
                if (projectEdge != null)
                {
                    result.Add(projectEdge(node, child));
                }
                Traverse(child, result, getChildren, isExpanded, projectEdge);
            }
        }
    }
}