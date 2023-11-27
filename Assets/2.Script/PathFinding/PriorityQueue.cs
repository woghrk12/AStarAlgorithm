using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue
{
    private List<Node> itemList = null;
    
    public int Count => itemList.Count;

    public PriorityQueue(int maxCapacity = 256)
    {
        itemList = new(maxCapacity);
    }

    /// <summary>
    /// Add the node to the last index of the heap tree and find appopriate position
    /// </summary>
    /// <param name="item">The item to be added</param>
    public void Add(Node item)
    {
        itemList.Add(item);

        if (itemList.Count > 1)
        {
            BubbleUp(itemList.Count - 1);
        }
    }

    /// <summary>
    /// Pop the minimum F value node and fix the heap tree
    /// </summary>
    /// <returns>The node that F value is minimum</returns>
    public Node Pop()
    {
        if (itemList.Count >= 1)
        {
            Node minNode = itemList[0];

            // No need to fix the heap tree if the heap tree has only one item
            if (itemList.Count == 1)
            {
                itemList.RemoveAt(0);
            }
            // Need to fix the heap tree
            else
            {
                int lastIndex = itemList.Count - 1;
                itemList[0] = itemList[lastIndex];
                itemList.RemoveAt(lastIndex);
                FixHeap(0, itemList.Count);
            }

            return minNode;
        }

        Debug.LogWarning("There is no element in the priority queue.");
        return null;
    }

    /// <summary>
    /// Clear the heap tree
    /// </summary>
    public void Clear()
    {
        itemList.Clear();
    }

    /// <summary>
    /// Fix the heap tree with heap sort algorithm
    /// </summary>
    /// <param name="fixIndex">The node index to be fixed</param>
    /// <param name="heapCount">The count of nodes</param>
    private void FixHeap(int fixIndex, int heapCount)
    {
        int leftIndex = fixIndex * 2;
        int rightIndex = fixIndex * 2 + 1;

        // if the node has both of the child node
        if (leftIndex < heapCount && rightIndex < heapCount)
        {
            if (itemList[leftIndex] < itemList[rightIndex])
            {
                if (itemList[fixIndex] <= itemList[leftIndex]) return;

                Swap(fixIndex, leftIndex);
                FixHeap(leftIndex, heapCount);
            }
            else
            {
                if (itemList[fixIndex] <= itemList[rightIndex]) return;
                Swap(fixIndex, rightIndex);
                FixHeap(rightIndex, heapCount);
            }
        }
        // if the node has only the left child node
        else if (leftIndex < heapCount)
        {
            if (itemList[fixIndex] <= itemList[leftIndex]) return;

            Swap(fixIndex, leftIndex);
            FixHeap(leftIndex, heapCount);
        }
    }

    /// <summary>
    /// Find the appropriate position of the target node by comparing its parent node
    /// </summary>
    /// <param name="fixIndex">The node index to be fixed</param>
    private void BubbleUp(int fixIndex)
    {
        // if the node to be fixed reach the root node
        if (fixIndex == 0) return;

        int parent = fixIndex / 2;

        if (itemList[fixIndex] >= itemList[parent]) return;

        Swap(fixIndex, parent);
        BubbleUp(parent);
    }

    /// <summary>
    /// Swap the two nodes provided as parameters
    /// </summary>
    private void Swap(int a, int b)
    {
        if (a < 0 || a >= itemList.Count || b < 0 || b >= itemList.Count)
        {
            throw new System.Exception($"Out of range. a : {a}, b : {b}");
        }
        if (a == b)
        {
            Debug.LogWarning($"Input value is equal. a : {a}, b : {b}");
            return;
        }

        Node temp = itemList[a];
        itemList[a] = itemList[b];
        itemList[b] = temp;
    }
}
