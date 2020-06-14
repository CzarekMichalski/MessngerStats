using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DataLoader;
using DataLoader.Model;

namespace DataPlotting
{
    public partial class SearchWindow : Window
    {
        private bool FromGroup { get; set; }
        public List<string> TypeList { get; set; }
        public List<string> ConversationList { get; set; }
        public List<string> GroupsList { get; set; }
        public List<string> PrivateConversationsList { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public SearchWindow(List<string> privateConversations, List<string> groups)
        {
            DataContext = this;
            GroupsList = groups;
            PrivateConversationsList = privateConversations;
            InitTypeList();
            ConversationList = PrivateConversationsList;
            InitializeComponent();
            GetDateRangeForConversation();
        }

        private void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dbManager = new DbManager();
            var conversations = new List<Message>();

            var isRegexSearch = IsRegexCheckbox?.IsChecked != null && (bool) IsRegexCheckbox.IsChecked;
            var isInAllConversations = ConversationTypeCombobox.SelectedItem as string == "All";
            var selectedConversation = (string) ConversationCombobox.SelectedItem;
            var searchText = SearchTextbox.Text;
            var processing = true;
            
            var loadingWindow = new LoadingWindow();
            loadingWindow.Show();

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                if (isRegexSearch)
                {
                    if (isInAllConversations)
                    {
                        conversations = dbManager.GetAllMessages(From, To);
                    }
                    else
                    {
                        conversations = dbManager.MessageEntityToMessage(dbManager.GetMessages(FromGroup, selectedConversation, From,
                            To));
                    }

                    var regex = new System.Text.RegularExpressions.Regex(searchText);
                    conversations = conversations.Where(x => x.Content != null).Where(x => regex.IsMatch(x.Content)).ToList();
                }
                else
                {
                    if (isInAllConversations)
                    {
                        conversations = dbManager.GetAllMessages(From, To, searchText);
                    }
                    else
                    {
                        conversations = dbManager.MessageEntityToMessage(dbManager.GetMessages(FromGroup, selectedConversation, From,
                            To, searchText));
                    }
                }

                processing = false;
            }).Start();

            while (processing)
            {
                Thread.Sleep(100);
            }
            
            loadingWindow.Close();

            var messageListWindow = new MessageListWindow(conversations);
            messageListWindow.ShowDialog();
        }

        private void TypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var conversationTypeCombobox = sender as ComboBox;

            if (conversationTypeCombobox == null)
            {
                return;
            }

            if (ConversationCombobox == null)
            {
                return;
            }

            FromGroup = (conversationTypeCombobox.SelectedItem as string) switch
            {
                "Group" => true,
                "Private" => false,
                "All" => false
            };

            ConversationCombobox.ItemsSource = (conversationTypeCombobox.SelectedItem as string) switch
            {
                "Group" => GroupsList,
                "Private" => PrivateConversationsList,
                "All" => null
            };
        }

        private void InitTypeList()
        {
            TypeList = new List<string>();
            TypeList.Add("All");
            TypeList.Add("Private");
            TypeList.Add("Group");
        }

        private void GetDateRangeForConversation()
        {
            var databaseManager = new DbManager();
            (From, To) = databaseManager.GetDateRange();

            SetFromAndToDates();
        }

        private void SetFromAndToDates()
        {
            FromDatePicker.SelectedDate = From;
            ToDatePicker.SelectedDate = To;
        }
    }
}
