﻿using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysCall : NotificationSupport
    {
        readonly GenesysCallManager callManager;
        readonly CallUserData userData;

        public string Id { get; private set; }
        public string State { get; private set; }
        
        IList<string> capabilities;
        IReadOnlyCollection<string> readOnlyCapabilities;

        public CallUserData UserData { get { return userData; } }

        public GenesysCall(GenesysCallManager callManager, CallResource callResource)
        {
            this.callManager = callManager;
            Id = callResource.id;
            State = callResource.state;
            Participants = callResource.participants;
            SetCapabilities(callResource.capabilities);
            userData = new CallUserData(callResource);
            UpdateCapableProperties(null, callResource.capabilities);
        }

        public bool Finished
        {
            get { return State == "Released"; }
        }

        public IReadOnlyCollection<string> Capabilities
        {
            get { return readOnlyCapabilities; }
        }

        void SetCapabilities(IList<string> value)
        {
            capabilities = value;
            readOnlyCapabilities = new ReadOnlyCollection<string>(capabilities);
        }

        internal void HandleEvent(INotifications notifs, string notificationType, CallResource callResource)
        {
            bool userDataChanged = userData.HandleEvent(notificationType, callResource);
            if (userDataChanged)
                RaisePropertyChanged(notifs, "UserData");

            if (notificationType == "StatusChange")
            {
                ChangeAndNotifyProperty(notifs, "State", callResource.state);
                ChangeAndNotifyProperty(notifs, "Participants", callResource.participants);
                SetCapabilities(callResource.capabilities);
                UpdateCapableProperties(notifs, callResource.capabilities);
                RaisePropertyChanged(notifs, "Capabilities");
            }
        }

        void DoCallOperation(object parameters)
        {
            callManager.User.Connection.InternalClient.CreateRequest("POST", "/api/v2/me/calls/" + Id, parameters).SendAsync();
        }

        void DoCallOperation(string operationName)
        {
            DoCallOperation(new { operationName = operationName });
        }

        void UpdateCapableProperties(INotifications notifs, IList<string> capabilities)
        {
            foreach (var op in callOperations)
            {
                string propertyName = op + "Capable";
                bool capable = capabilities.Contains(op);
                SetPropertyValue(propertyName, capable);
                
                if (notifs != null)
                    RaisePropertyChanged(notifs, propertyName);
            }
        }

        readonly string[] callOperations =
        {
            "Answer",
            "Hangup",
            "InitiateTransfer",
            "CompleteTransfer",
            "AttachUserData",
            "UpdateUserData",
        };

        public void Answer() { DoCallOperation("Answer"); }
        public bool AnswerCapable { get; private set; }

        public void Hangup() { DoCallOperation("Hangup"); }
        public bool HangupCapable { get; private set; }

        public void InitiateTransfer(string phoneNumber)
        {
            DoCallOperation(new
                {
                    operationName = "InitiateTransfer",
                    destination = new { phoneNumber = phoneNumber }
                });
        }

        public bool InitiateTransferCapable { get; private set; }

        public void CompleteTransfer() { DoCallOperation("CompleteTransfer"); }
        public bool CompleteTransferCapable { get; private set; }

        public void AttachUserData(object obj)
        {
            DoCallOperation(new
            {
                operationName = "AttachUserData",
                userData = obj
            });
        }

        public bool AttachUserDataCapable { get; private set; }

        public void UpdateUserData(object obj)
        {
            DoCallOperation(new
            {
                operationName = "UpdateUserData",
                userData = obj
            });
        }

        public bool UpdateUserDataCapable { get; private set; }

        public IList<object> Participants { get; private set; }
    }
}
