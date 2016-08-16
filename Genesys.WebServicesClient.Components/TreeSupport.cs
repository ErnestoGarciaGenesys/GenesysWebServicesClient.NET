﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Genesys.WebServicesClient.Components
{
    public abstract class TreeSupport : DisposableSupport
    {
        TreeSupport parent;

        List<TreeSupport> children = new List<TreeSupport>();

        protected TreeSupport Parent
        {
            get
            {
                return parent;
            }

            set
            {
                if (value != parent)
                {
                    if (value == null)
                    {
                        // null value must be allowed, as it is typical to be set by code generated by Visual Designer
                        parent.children.Remove(this);
                        parent = null;
                    }
                    else
                    {
                        parent = value;
                        parent.children.Add(this);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (parent != null)
                    parent.children.Remove(this);
            }

            base.Dispose(disposing);
        }

        class Notifications : INotifications
        {
            readonly List<Action> notifs = new List<Action>();

            public void Notify(Action a)
            {
                notifs.Add(a);
            }

            public IEnumerable<Action> Content
            {
                get { return notifs; }
            }
        }

        protected class UpdateResult
        {
            public UpdateResult(INotifications notifications, INotifications notificationsOnAwait)
            {
                this.Notifications = notifications;
                this.NotificationsOnAwait = notificationsOnAwait;
            }

            public INotifications Notifications { get; private set; }
            public INotifications NotificationsOnAwait { get; private set; }
            public bool Changed { get; set; }
            public object MessageToChildren { get; set; }
        }

        protected void UpdateTree(Action<UpdateResult> updateAction)
        {
            var notifs = new Notifications();
            var result = new UpdateResult(notifs, null);
            updateAction(result);

            PropagateToChildren(result);
            
            foreach (Action a in notifs.Content.Reverse())
                a();
        }

        protected async Task UpdateTreeAsync(Func<UpdateResult, Task> updateAction)
        {
            var notifs = new Notifications();
            var notifsOnAwait = new Notifications();
            var result = new UpdateResult(notifs, notifsOnAwait);
            var updateTask = updateAction(result);

            foreach (Action a in notifsOnAwait.Content)
                a();

            try
            {
                await updateTask;
            }
            finally
            {
                PropagateToChildren(result);

                foreach (Action a in notifs.Content.Reverse())
                    a();
            }
        }

        void PropagateToChildren(UpdateResult result)
        {
            bool anyChildChanged = false;

            foreach (var child in children)
            {
                var childResult = new UpdateResult(result.Notifications, null);
                child.OnParentUpdated(result.MessageToChildren, childResult);
                child.PropagateToChildren(childResult);
                anyChildChanged |= childResult.Changed;
            }

            if (anyChildChanged)
                RaiseUpdated(result.Notifications);
        }

        protected virtual void OnParentUpdated(object message, UpdateResult result)
        {
        }
    }
}
