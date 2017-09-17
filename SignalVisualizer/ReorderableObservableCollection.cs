using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SignalVisualizer
{
    public interface IReorderable
    {
        bool IsFirst { get; set; }
        bool IsLast { get; set; }
        bool CanMoveUp { get; set; }
        bool CanMoveDown { get; set; }
        IReorderer Reorderer { get; set; }
    }

    public interface IReorderer
    {
        bool MoveUp(IReorderable reorderable);
        bool MoveDown(IReorderable reorderable);
    }

    public class ReorderingEventArgs : EventArgs
    {
        public IReorderer Reorderer { get; }
        public IReorderable Reorderable { get; }
        public int OldIndex { get; }
        public int NewIndex { get; }

        public ReorderingEventArgs(IReorderer reorderer, IReorderable reorderable, int oldIndex, int newIndex)
        {
            Reorderer = reorderer;
            Reorderable = reorderable;
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }
    }

    public class ReorderableObservableCollection<T> : ObservableCollection<T>, IReorderer where T : IReorderable
    {
        public event EventHandler<ReorderingEventArgs> ChildReordered;

        public ReorderableObservableCollection()
        {
        }

        public ReorderableObservableCollection(IEnumerable<T> collection)
            : base(collection)
        {
            UpdateReorderables(Items);
        }

        public ReorderableObservableCollection(List<T> list)
            : base(list)
        {
            UpdateReorderables(Items);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            UpdateReorderables(e.Action != NotifyCollectionChangedAction.Remove ? Items : null);
        }

        public bool IsReordering { get; private set; }

        public bool MoveUp(IReorderable reorderable)
        {
            if (reorderable.CanMoveUp == false)
                return false;

            T item = (T)reorderable;

            int oldIndex = Items.IndexOf(item);
            if (oldIndex < 1)
                return false;

            IsReordering = true;

            try
            {
                Remove(item);
                int newIndex = oldIndex - 1;
                Insert(newIndex, item);

                ChildReordered?.Invoke(this, new ReorderingEventArgs(this, reorderable, oldIndex, newIndex));
            }
            finally
            {
                IsReordering = false;
            }

            return true;
        }

        public bool MoveDown(IReorderable reorderable)
        {
            if (reorderable.CanMoveDown == false)
                return false;

            T item = (T)reorderable;

            int oldIndex = Items.IndexOf(item);
            if (oldIndex >= Items.Count - 1)
                return false;

            IsReordering = true;

            try
            {
                Remove(item);
                int newIndex = oldIndex + 1;
                Insert(newIndex, item);

                ChildReordered?.Invoke(this, new ReorderingEventArgs(this, reorderable, oldIndex, newIndex));
            }
            finally
            {
                IsReordering = false;
            }

            return true;
        }

        private void UpdateReorderables(IEnumerable newItems)
        {
            if (newItems != null)
            {
                foreach (IReorderable item in newItems)
                {
                    item.IsFirst = false;
                    item.IsLast = false;
                    item.CanMoveDown = true;
                    item.CanMoveUp = true;
                    item.Reorderer = this;
                }
            }

            if (Items.Count == 0)
                return;

            if (Items.Count == 1)
            {
                var reord = (IReorderable)Items[0];
                if (reord != null)
                {
                    reord.IsFirst = true;
                    reord.IsLast = true;
                    reord.CanMoveDown = false;
                    reord.CanMoveUp = false;
                }
            }
            else
            {
                var first = (IReorderable)Items[0];
                var last = (IReorderable)Items[Items.Count - 1];

                if (first != null)
                {
                    first.IsFirst = true;
                    first.IsLast = false;
                    first.CanMoveUp = false;
                    first.CanMoveDown = true;
                }

                if (last != null)
                {
                    last.IsFirst = false;
                    last.IsLast = true;
                    last.CanMoveUp = true;
                    last.CanMoveDown = false;
                }
            }
        }
    }
}
