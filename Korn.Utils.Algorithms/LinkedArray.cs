using Korn.Shared;
using System;

namespace Korn.Utils.Algorithms
{
    public unsafe class LinkedArray : IDisposable
    {
        public LinkedArray()
            => MovelessNodePointer = (LinkedNode**)Memory.Alloc<IntPtr>();

        public LinkedNode** MovelessNodePointer { get; private set; }
        public LinkedNode* RootNode { get; private set; }
        public LinkedNode* LastNode { get; private set; }

        public bool HasNodes => RootNode != null;

        public LinkedNode* AddNode()
        {
            var node = LinkedNode.Dispatcher.AlocateNode();
            if (RootNode == null)
                *MovelessNodePointer = RootNode = LastNode = node;
            else LastNode = LastNode->Next = node;
            return node;
        }

        public void RemoveNode(LinkedNode* node)
        {
            if (RootNode == null)
            {
                KornShared.Logger.WriteWarning(
                    "Korn.Utils.Algorithms.LinkedArray.RemoveNode: ",
                    "Attempt to delete a node not included in this array."
                );
            }

            if (RootNode == node)
            {
                if (RootNode->Next->IsValid)
                {
                    var removedNode = RootNode;
                    *MovelessNodePointer = RootNode = RootNode->Next;
                    LinkedNode.Dispatcher.DestroyNode(removedNode);
                }
                else
                {
                    LinkedNode.Dispatcher.DestroyNode(RootNode);
                    *MovelessNodePointer = RootNode = LastNode = null;
                }
            }
            else
            {
                var previousNode = RootNode;
                var currentNode = RootNode;
                do
                {
                    currentNode = currentNode->Next;
                    if (currentNode == node)
                    {
                        previousNode->Next = currentNode->Next;
                        LinkedNode.Dispatcher.DestroyNode(currentNode);
                        break;
                    }
                    previousNode = currentNode;
                }
                while (currentNode->IsValid);
            }
        }

        public void Dispose()
        {
            Memory.Free(MovelessNodePointer);
            LinkedNode.Dispatcher.DestroySequence(RootNode);
        }
    }

    public unsafe struct LinkedNode
    {
        public IntPtr Value;
        public LinkedNode* Next;

        public bool IsValid => !(Value == IntPtr.Zero && Next == null);
        public bool HasNext => Next != null;

        public LinkedNode* SetValue(IntPtr value)
        {
            Value = value;
            return self;
        }

        public LinkedNode* SetNext(LinkedNode* nextNode)
        {
            Next = nextNode;
            return self;
        }

        LinkedNode* self
        {
            get
            {
                fixed (LinkedNode* self = &this)
                    return self;
            }
        }

        public static class Dispatcher
        {
            public static void DestroySequence(LinkedNode* node)
            {
                var currentNode = node;

                do
                {
                    var nextNext = currentNode->Next;
                    DeallocateNode(currentNode);

                    currentNode = nextNext;
                }
                while (currentNode != null);
            }

            public static void DestroyNode(LinkedNode* node) => DeallocateNode(node);

            public static LinkedNode* AlocateNode() => Memory.Alloc<LinkedNode>();
            public static void DeallocateNode(LinkedNode* node) => Memory.Free(node);
        }
    }
}