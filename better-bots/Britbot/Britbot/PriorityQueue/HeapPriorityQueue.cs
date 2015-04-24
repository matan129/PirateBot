﻿#region Usings

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Britbot.PriorityQueue
{
    /// <summary>
    ///     An implementation of a min-Priority Queue using a heap.  Has O(1) .Contains()!
    ///     See https://bitbucket.org/BlueRaja/high-speed-priority-queue-for-c/wiki/Getting%20Started for more information
    /// </summary>
    /// <typeparam name="T">The values in the queue.  Must implement the PriorityQueueNode interface</typeparam>
    public sealed class HeapPriorityQueue<T> : IPriorityQueue<T>
        where T : PriorityQueueNode
    {
        private readonly T[] _nodes;
        private long _numNodesEverEnqueued;

        /// <summary>
        ///     Instantiate a new Priority Queue
        /// </summary>
        /// <param name="maxNodes">The max nodes ever allowed to be enqueued (going over this will cause an exception)</param>
        public HeapPriorityQueue(int maxNodes)
        {
            this.Count = 0;
            this._nodes = new T[maxNodes + 1];
            this._numNodesEverEnqueued = 0;
        }

        /// <summary>
        ///     Returns the number of nodes in the queue.  O(1)
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        ///     Returns the maximum number of items that can be enqueued at once in this queue.  Once you hit this number (ie. once
        ///     Count == MaxSize),
        ///     attempting to enqueue another item will throw an exception.  O(1)
        /// </summary>
        public int MaxSize
        {
            get { return this._nodes.Length - 1; }
        }

        /// <summary>
        ///     Removes every node from the queue.  O(n) (So, don't do this often!)
        /// </summary>
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public void Clear()
        {
            Array.Clear(this._nodes, 1, this.Count);
            this.Count = 0;
        }

        /// <summary>
        ///     Returns (in O(1)!) whether the given node is in the queue.  O(1)
        /// </summary>
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public bool Contains(T node)
        {
            return (this._nodes[node.QueueIndex] == node);
        }

        /// <summary>
        ///     Enqueue a node - .Priority must be set beforehand!  O(log n)
        /// </summary>
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public void Enqueue(T node, double priority)
        {
            node.Priority = priority;
            this.Count++;
            this._nodes[this.Count] = node;
            node.QueueIndex = this.Count;
            node.InsertionIndex = this._numNodesEverEnqueued++;
            this.CascadeUp(this._nodes[this.Count]);
        }

#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif

        private void Swap(T node1, T node2)
        {
            //Swap the nodes
            this._nodes[node1.QueueIndex] = node2;
            this._nodes[node2.QueueIndex] = node1;

            //Swap their indicies
            int temp = node1.QueueIndex;
            node1.QueueIndex = node2.QueueIndex;
            node2.QueueIndex = temp;
        }

        //Performance appears to be slightly better when this is NOT inlined o_O
        private void CascadeUp(T node)
        {
            //aka Heapify-up
            int parent = node.QueueIndex / 2;
            while (parent >= 1)
            {
                T parentNode = this._nodes[parent];
                if (this.HasHigherPriority(parentNode, node))
                    break;

                //Node has lower priority value, so move it up the heap
                this.Swap(node, parentNode);
                //For some reason, this is faster with Swap() rather than (less..?) individual operations, like in CascadeDown()

                parent = node.QueueIndex / 2;
            }
        }

#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif

        private void CascadeDown(T node)
        {
            //aka Heapify-down
            T newParent;
            int finalQueueIndex = node.QueueIndex;
            while (true)
            {
                newParent = node;
                int childLeftIndex = 2 * finalQueueIndex;

                //Check if the left-child is higher-priority than the current node
                if (childLeftIndex > this.Count)
                {
                    //This could be placed outside the loop, but then we'd have to check newParent != node twice
                    node.QueueIndex = finalQueueIndex;
                    this._nodes[finalQueueIndex] = node;
                    break;
                }

                T childLeft = this._nodes[childLeftIndex];
                if (this.HasHigherPriority(childLeft, newParent))
                {
                    newParent = childLeft;
                }

                //Check if the right-child is higher-priority than either the current node or the left child
                int childRightIndex = childLeftIndex + 1;
                if (childRightIndex <= this.Count)
                {
                    T childRight = this._nodes[childRightIndex];
                    if (this.HasHigherPriority(childRight, newParent))
                    {
                        newParent = childRight;
                    }
                }

                //If either of the children has higher (smaller) priority, swap and continue cascading
                if (newParent != node)
                {
                    //Move new parent to its new index.  node will be moved once, at the end
                    //Doing it this way is one less assignment operation than calling Swap()
                    this._nodes[finalQueueIndex] = newParent;

                    int temp = newParent.QueueIndex;
                    newParent.QueueIndex = finalQueueIndex;
                    finalQueueIndex = temp;
                }
                else
                {
                    //See note above
                    node.QueueIndex = finalQueueIndex;
                    this._nodes[finalQueueIndex] = node;
                    break;
                }
            }
        }

        /// <summary>
        ///     Returns true if 'higher' has higher priority than 'lower', false otherwise.
        ///     Note that calling HasHigherPriority(node, node) (ie. both arguments the same node) will return false
        /// </summary>
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private bool HasHigherPriority(T higher, T lower)
        {
            return (higher.Priority < lower.Priority ||
                    (higher.Priority == lower.Priority && higher.InsertionIndex < lower.InsertionIndex));
        }

        /// <summary>
        ///     Removes the head of the queue (node with highest priority; ties are broken by order of insertion), and returns it.
        ///     O(log n)
        /// </summary>
        public T Dequeue()
        {
            T returnMe = this._nodes[1];
            this.Remove(returnMe);
            return returnMe;
        }

        /// <summary>
        ///     Returns the head of the queue, without removing it (use Dequeue() for that).  O(1)
        /// </summary>
        public T First
        {
            get { return this._nodes[1]; }
        }

        /// <summary>
        ///     This method must be called on a node every time its priority changes while it is in the queue.
        ///     <b>Forgetting to call this method will result in a corrupted queue!</b>
        ///     O(log n)
        /// </summary>
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public void UpdatePriority(T node, double priority)
        {
            node.Priority = priority;
            this.OnNodeUpdated(node);
        }

        private void OnNodeUpdated(T node)
        {
            //Bubble the updated node up or down as appropriate
            int parentIndex = node.QueueIndex / 2;
            T parentNode = this._nodes[parentIndex];

            if (parentIndex > 0 && this.HasHigherPriority(node, parentNode))
            {
                this.CascadeUp(node);
            }
            else
            {
                //Note that CascadeDown will be called if parentNode == node (that is, node is the root)
                this.CascadeDown(node);
            }
        }

        /// <summary>
        ///     Removes a node from the queue.  Note that the node does not need to be the head of the queue.  O(log n)
        /// </summary>
        public void Remove(T node)
        {
            if (!this.Contains(node))
            {
                return;
            }

            if (this.Count <= 1)
            {
                this._nodes[1] = null;
                this.Count = 0;
                return;
            }

            //Make sure the node is the last node in the queue
            bool wasSwapped = false;
            T formerLastNode = this._nodes[this.Count];
            if (node.QueueIndex != this.Count)
            {
                //Swap the node with the last node
                this.Swap(node, formerLastNode);
                wasSwapped = true;
            }

            this.Count--;
            this._nodes[node.QueueIndex] = null;

            if (wasSwapped)
            {
                //Now bubble formerLastNode (which is no longer the last node) up or down as appropriate
                this.OnNodeUpdated(formerLastNode);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 1; i <= this.Count; i++)
                yield return this._nodes[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        ///     <b>Should not be called in production code.</b>
        ///     Checks to make sure the queue is still in a valid state.  Used for testing/debugging the queue.
        /// </summary>
        public bool IsValidQueue()
        {
            for (int i = 1; i < this._nodes.Length; i++)
            {
                if (this._nodes[i] != null)
                {
                    int childLeftIndex = 2 * i;
                    if (childLeftIndex < this._nodes.Length && this._nodes[childLeftIndex] != null &&
                        this.HasHigherPriority(this._nodes[childLeftIndex], this._nodes[i]))
                        return false;

                    int childRightIndex = childLeftIndex + 1;
                    if (childRightIndex < this._nodes.Length && this._nodes[childRightIndex] != null &&
                        this.HasHigherPriority(this._nodes[childRightIndex], this._nodes[i]))
                        return false;
                }
            }
            return true;
        }
    }
}