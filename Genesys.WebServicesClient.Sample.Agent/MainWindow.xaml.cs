﻿using Genesys.ObservableResource;
using Genesys.WebServicesClient.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Genesys.WebServicesClient.Sample.Agent.WPF
{
    public partial class MainWindow : Window
    {
        readonly GenesysConnection genesysConnection;
        readonly GenesysUser genesysAgent;
        readonly GenesysCallManager genesysCallManager;
        
        readonly Resource userResource = new Resource(new Resource.ResourceDescription()
            {
                SimpleKeys = new string[] { "id", "userName" },
                ArrayResources = new Resource.ArrayResourceDescription[]
                    {
                        new Resource.ArrayResourceDescription()
                        {
                            ArrayResourceKey = "devices",
                            Index = 0,
                            ResourceIdKey = "id",
                            ResourceDescription = new Resource.ResourceDescription()
                            {
                                SimpleKeys = new string[] { "id" },
                                ArrayResources = new Resource.ArrayResourceDescription[] {},
                            },
                        },
                    },
            });

        readonly GenesysResourceManagerOld genesysResourceManager = new GenesysResourceManagerOld();

        readonly TestComponent testComponent = new TestComponent();

        readonly dynamic dynamicObject = new ExpandoObject();

        public MainWindow()
        {
            InitializeComponent();

            genesysConnection = new GenesysConnection()
                {
                    ServerUri = "http://localhost:5088",
                    Username = "paveld@redwings.com",
                    Password = "password",
                };

            genesysAgent = new GenesysUser()
                {
                    Connection = genesysConnection,
                };

            genesysCallManager = new GenesysCallManager()
                {
                    User = genesysAgent,
                };

            genesysAgent.UserResourceUpdated += data =>
                {
                    userResource.Update((IDictionary<string, object>)data["user"]);
                    genesysResourceManager.UpdateResource("Agent", (IDictionary<string, object>)data["user"]);
                };

            genesysAgent.EventHandlingFinished += e =>
                {
                    UpdateUserDataGrid();
                };

            this.DataContext = genesysAgent;
            callGrid.DataContext = genesysCallManager;
            callDataGrid.ItemsSource = genesysCallManager.Calls;
            userGrid.DataContext = userResource;

            genesysResourceManager.CreateResource("Agent");
            userGrid2.DataContext = genesysResourceManager;

            testGrid.DataContext = testComponent;

            dynamicObject.Property = "testing expando";
            testGrid2.DataContext = dynamicObject;
        }

        void UpdateUserDataGrid()
        {
            if (genesysCallManager.ActiveCall != null)
            {
                userDataPropertyGrid.SelectedObject = null;
                userDataPropertyGrid.SelectedObject = genesysCallManager.ActiveCall.UserData;
                userDataPropertyGrid.Refresh();
            }
        }

        void readyButton_Click(object sender, RoutedEventArgs e)
        {
            genesysAgent.MakeReady();
        }

        void notReadyButton_Click(object sender, RoutedEventArgs e)
        {
            genesysAgent.MakeNotReady();
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            genesysAgent.Initialize();
            genesysCallManager.Initialize();
            genesysConnection.Initialize();
            UpdateUserDataGrid();
        }

        void answerButton_Click(object sender, RoutedEventArgs e)
        {
            genesysCallManager.ActiveCall.AnswerOperation.Do();
        }

        void hangupButton_Click(object sender, RoutedEventArgs e)
        {
            genesysCallManager.ActiveCall.HangupOperation.Do();
        }

        void addPropertyButton_Click(object sender, RoutedEventArgs e)
        {
            testComponent.AddProperty();
            testGrid.DataContext = null;
            testGrid.DataContext = testComponent;
        }

        void addPropertyButton2_Click(object sender, RoutedEventArgs e)
        {
            dynamicObject.Property = "changed";
            dynamicObject.ExtendedProperty = "another";
        }
    }
}