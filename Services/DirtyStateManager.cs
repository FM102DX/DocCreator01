using DocCreator01.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DocCreator01.Services
{
    // этот класс управляет состоянием "грязности" (dirty state) объектов
    public class DirtyStateManager : IDirtyStateManager
    {
        private bool _isDirty;
        private readonly List<IDirtyTrackable> _subscriptions = new List<IDirtyTrackable>();

        /// <summary>
        /// Gets a value indicating whether this object or any of its subscriptions is dirty
        /// </summary>
        public bool IsDirty
        {
            get => _isDirty || _subscriptions.Any(s => s.DirtyStateMgr.IsDirty);
        }

        /// <summary>
        /// Occurs when the dirty state changes
        /// </summary>
        public event EventHandler<bool> IBecameDirty;

        /// <summary>
        /// Accepts all changes and resets the dirty state to false
        /// </summary>
        public void ResetDirtyState()
        {
            _isDirty = false;
            
            // Propagate to all subscriptions
            foreach (var subscription in _subscriptions)
            {
                subscription.DirtyStateMgr.ResetDirtyState();
            }
        }

        /// <summary>
        /// Marks this object as dirty and raises the DirtyChanged event
        /// </summary>
        public void MarkAsDirty()
        {
            bool oldIsDirty = IsDirty;
            _isDirty = true;
            
            if (oldIsDirty != true)
            {
                IBecameDirty?.Invoke(this, true);
            }
        }

        public void AddSubscription(IDirtyTrackable trackable)
        {
            if (trackable == null)
                throw new ArgumentNullException(nameof(trackable));

            if (!_subscriptions.Contains(trackable))
            {
                _subscriptions.Add(trackable);
                trackable.DirtyStateMgr.IBecameDirty += OnSubscriptionBecameDirty;
            }
        }

        private void OnSubscriptionBecameDirty(object sender, bool isDirty)
        {
            // если подписка стала грязной, то и мы грязные
            MarkAsDirty();
        }
    }
}
